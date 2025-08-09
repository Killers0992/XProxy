using System.Xml.Linq;

namespace XProxy.Models;

public class BehaviourInfo
{
    public ulong SyncVarDirtyBits = 0;
    public ulong SyncObjectsDirtyBits = 0;

    public SpawnableObject Owner;

    public SyncObjectInfo[] SyncObjects;

    public Action<NetworkWriter, bool> OnBeforeSerialize;
    public Action<NetworkWriter, bool> OnAfterSerialize;

    public Action<NetworkWriter, bool> OnSerializeSyncVars;

    public BehaviourInfo(SpawnableObject owner, params SyncObjectInfo[] objects)
    {
        Owner = owner;
        SyncObjects = objects;
    }

    public void SetSyncVarDirtyBit(ulong dirtyBit)
    {
        SyncVarDirtyBits |= dirtyBit;
    }

    public void ClearAllDirtyBits()
    {
        SyncVarDirtyBits = 0;
        SyncObjectsDirtyBits = 0;
    }

    public bool IsDirty() => (this.SyncVarDirtyBits | this.SyncObjectsDirtyBits) != 0;

    public void Serialize(NetworkWriter writer, bool initialState)
    {
        // reserve length header to ensure the correct amount will be read.
        // originally we used a 4 byte header (too bandwidth heavy).
        // instead, let's "& 0xFF" the size.
        //
        // this is cleaner than barriers at the end of payload, because:
        // - ensures the correct safety is read _before_ payload.
        // - it's quite hard to break the check.
        //   a component would need to read/write the intented amount
        //   multiplied by 255 in order to miss the check.
        //   with barriers, reading 1 byte too much may still succeed if the
        //   next component's first byte matches the expected barrier.
        // - we can still attempt to correct the invalid position via the
        //   safety length byte (we know that one is correct).
        //
        // it's just overall cleaner, and still low on bandwidth.

        // write placeholder length byte
        // (jumping back later is WAY faster than allocating a temporary
        //  writer for the payload, then writing payload.size, payload)

        int headerPosition = writer.Position;
        writer.WriteByte(0);
        int contentPosition = writer.Position;

        // write payload
        try
        {
            OnSerialize(writer, initialState);
        }
        catch (Exception e)
        {
            Logger.Error($"OnSerialize failed\n\n{e}");
        }

        int endPosition = writer.Position;

        writer.Position = headerPosition;
        int size = endPosition - contentPosition;
        byte safety = (byte)(size & 0xFF);
        writer.WriteByte(safety);
        writer.Position = endPosition;
    }

    void OnSerialize(NetworkWriter writer, bool initialState)
    {
        OnBeforeSerialize?.Invoke(writer, initialState);

        SerializeSyncObjects(writer, initialState);
        SerializeSyncVars(writer, initialState);

        OnAfterSerialize?.Invoke(writer, initialState);
    }

    void SerializeSyncObjects(NetworkWriter writer, bool initialState)
    {
        if (initialState)
            SerializeObjectsAll(writer);
        else
            SerializeObjectsDelta(writer);
    }

    void SerializeSyncVars(NetworkWriter writer, bool initialState)
    {
        if (!initialState)
            writer.WriteULong(SyncVarDirtyBits);

        OnSerializeSyncVars?.Invoke(writer, initialState);
    }

    void SerializeObjectsAll(NetworkWriter writer)
    {
        for (int i = 0; i < SyncObjects.Length; i++)
        {
            SyncObjects[i].OnSerializeAll(writer);
        }
    }

    void SerializeObjectsDelta(NetworkWriter writer)
    {
        writer.WriteULong(SyncObjectsDirtyBits);
        for (int i = 0; i < SyncObjects.Length; i++)
        {
            SyncObjectInfo syncObject = this.SyncObjects[i];
            if ((this.SyncObjectsDirtyBits & 1UL << i) != 0UL)
            {
                syncObject.OnSerializeDelta(writer);
            }
        }
    }
}
