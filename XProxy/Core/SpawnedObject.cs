using UnityEngine;

namespace XProxy.Core;

public class SpawnableObject : IDisposable
{
    public uint NetworkId { get; }
    public uint AssetId { get; }
    public ulong SceneId { get; }

    public bool IsOwner { get; }
    public bool IsLocalPlayer { get; }

    public BaseClient Owner { get; private set; }

    public SpawnableObject(bool isLocalPlayer, bool isOwner, uint networkId, uint assetId, ulong sceneId)
    {
        NetworkId = networkId;
        AssetId = assetId;
        SceneId = sceneId;

        IsOwner = isOwner;
        IsLocalPlayer = isLocalPlayer;
    }

    public BehaviourInfo[] Behaviours { get; set; }

    public void SendUpdate(BaseClient client)
    {
        NetworkWriter wr2 = Serialize(false);

        if (wr2 == null)
            return;

        NetworkWriter wr = new NetworkWriter();
        wr.WriteUShort(NetworkMessageId<EntityStateMessage>.Id);
        wr.WriteUInt(NetworkId);
        wr.WriteArraySegmentAndSize(wr2.ToArraySegment());

        client.SendMirrorData(wr);
    }

    public NetworkWriter Serialize(bool intialState)
    {
        NetworkWriter writer = new NetworkWriter();

        ulong observerMask = DirtyMasks(intialState);

        if (observerMask != 0)
            Compression.CompressVarUInt(writer, observerMask);

        if (observerMask != 0)
        {
            for (int x = 0; x < Behaviours.Length; x++)
            {
                BehaviourInfo bInfo = Behaviours[x];

                if (bInfo == null)
                    continue;

                bool observersDirty = IsDirty(observerMask, x);

                if (observersDirty)
                {
                    NetworkWriter temp = new NetworkWriter();

                    bInfo.Serialize(temp, intialState);

                    ArraySegment<byte> segment = temp.ToArraySegment();

                    writer.WriteBytes(segment.Array, segment.Offset, segment.Count);

                    if (!intialState)
                        bInfo.ClearAllDirtyBits();
                }
            }
        }
        else
            return null;

        return writer;
    }

    internal static bool IsDirty(ulong mask, int index)
    {
        ulong nthBit = (ulong)(1 << index);
        return (mask & nthBit) != 0;
    }

    public ulong DirtyMasks(bool intialState)
    {
        ulong bit = 0;

        for(int x = 0; x < Behaviours.Length; x++)
        {
            BehaviourInfo bInfo = Behaviours[x];

            if (bInfo == null)
                continue;
            
            bool isDirty = bInfo.IsDirty();

            ulong behaviorBit = 1UL << (x & 31);

            if (intialState || isDirty)
            {
                bit |= behaviorBit;
            }
        }

        return bit;
    }

    public void AssignOwner(BaseClient owner)
    {
        Owner = owner;
    }

    public void Destroy()
    {
        if (Owner != null)
        {
            Owner.DestroyObject(NetworkId);
        }
        
        Dispose();
    }

    public void Spawn(BaseClient client, ArraySegment<byte> payload)
    {
        NetworkWriter wr = new NetworkWriter();

        wr.WriteUShort(NetworkMessageId<SpawnMessage>.Id);

        wr.WriteUInt(NetworkId);
        wr.WriteBool(IsLocalPlayer);
        wr.WriteBool(IsOwner);

        wr.WriteULong(SceneId);
        wr.WriteUInt(AssetId);

        wr.WriteVector3(Vector3.zero);
        wr.WriteQuaternion(Quaternion.identity);
        wr.WriteVector3(Vector3.one);

        wr.WriteArraySegmentAndSize(payload);

        client.SendMirrorData(wr);
    }

    public void Dispose()
    {
        Owner = null;
    }
}
