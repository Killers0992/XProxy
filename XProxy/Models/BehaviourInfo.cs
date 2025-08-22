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

    public Action<NetworkReader, long, bool> OnDeserializeSyncVars;

    public BehaviourInfo(SpawnableObject owner, params SyncObjectInfo[] objects)
    {
        Owner = owner;
        SyncObjects = objects;
    }

    public virtual void OnReceiveCommand(ushort functionHash, ArraySegment<byte> payload = default) { }

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
        int headerPosition = writer.Position;
        writer.WriteByte(0);

        int contentPosition = writer.Position;
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

    public bool Deserialize(NetworkReader reader, bool initialState)
    {
        bool result = true;

        byte safety = reader.ReadByte();
        int chunkStart = reader.Position;

        try
        {
            OnDeserialize(reader, initialState);
        }
        catch (Exception e)
        {
            Logger.Error(e);
            result = false;
        }

        int size = reader.Position - chunkStart;
        byte sizeHash = (byte)(size & 0xFF);
        if (sizeHash != safety)
        {
            int correctedSize = ErrorCorrection(size, safety);
            reader.Position = chunkStart + correctedSize;
            result = false;
        }

        return result;
    }

    void OnSerialize(NetworkWriter writer, bool initialState)
    {
        OnBeforeSerialize?.Invoke(writer, initialState);

        SerializeSyncObjects(writer, initialState);
        SerializeSyncVars(writer, initialState);

        OnAfterSerialize?.Invoke(writer, initialState);
    }

    void OnDeserialize(NetworkReader reader, bool initialState)
    {
        DeserializeSyncObjects(reader, initialState);
        DeserializeSyncVars(reader, initialState);
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

            if ((SyncObjectsDirtyBits & 1UL << i) != 0UL)
                syncObject.OnSerializeDelta(writer);
        }
    }

    void DeserializeSyncObjects(NetworkReader reader, bool initialState)
    {
        if (initialState)
            DeserializeObjectsAll(reader);
        else
            DeserializeObjectsDelta(reader);
    }

    void DeserializeSyncVars(NetworkReader reader, bool initialState)
    {
        long mask = 0;

        if (!initialState)
            mask = (long)reader.ReadULong();

        OnDeserializeSyncVars?.Invoke(reader, mask, initialState);
    }

    void DeserializeObjectsAll(NetworkReader reader)
    {
        for (int i = 0; i < SyncObjects.Length; i++)
        {
            SyncObjectInfo syncObject = SyncObjects[i];
            syncObject.OnDeserializeAll(reader);
        }
    }

    void DeserializeObjectsDelta(NetworkReader reader)
    {
        ulong dirty = reader.ReadULong();
        for (int i = 0; i < SyncObjects.Length; i++)
        {
            SyncObjectInfo syncObject = SyncObjects[i];
            if ((dirty & (1UL << i)) != 0)
                syncObject.OnDeserializeDelta(reader);
        }
    }

    internal static int ErrorCorrection(int size, byte safety)
    {
        uint cleared = (uint)size & 0xFFFFFF00;

        return (int)(cleared | safety);
    }
}
