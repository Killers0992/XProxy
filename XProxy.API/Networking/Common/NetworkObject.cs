namespace XProxy.API.Networking.Common;

public class NetworkObject : IDisposable
{
    public virtual uint NetworkId { get; set; }
    public virtual uint AssetId { get; }
    public virtual ulong SceneId { get; }

    public bool WithPayload { get; set; }

    public Vector3 Position { get; set; } = Vector3.zero;
    public Quaternion Rotation { get; set; } = Quaternion.identity;
    public Vector3 Scale { get; set; } = Vector3.one;

    public Client Owner { get; private set; }

    public World World { get; }

    public NetworkObject(World world, Client owner, uint networkId = 0)
    {
        Owner = owner;
        World = world;

        if (networkId != 0)
            NetworkId = networkId;

        if (NetworkId == 0)
        {
            NetworkId = world.GetFreeId();
            world.Objects.Add(NetworkId, this);
        }
        else
        {
            NetworkId = networkId;

            if (world != null && !world.Objects.ContainsKey(networkId))
                world.Objects.Add(networkId, this);
        }
    }

    public BehaviourComponent[] Behaviours { get; set; }

    public void SendUpdate(Client client)
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

    public void Deserialize(NetworkReader reader, bool intialState)
    {
        ulong mask = Compression.DecompressVarUInt(reader);

        for (int i = 0; i < Behaviours.Length; ++i)
        {
            if (IsDirty(mask, i))
            {
                BehaviourComponent comp = Behaviours[i];

                comp.Deserialize(reader, intialState);
            }
        }
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
                BehaviourComponent bInfo = Behaviours[x];

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

        for (int x = 0; x < Behaviours.Length; x++)
        {
            BehaviourComponent bInfo = Behaviours[x];

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

    public void AssignOwner(Client owner)
    {
        Owner = owner;
    }

    public void Destroy()
    {
        if (Owner != null)
            Owner.DestroyObject(NetworkId);

        Dispose();
    }

    public void SpawnWithPayload(Client client)
    {
        NetworkWriter wr2 = Serialize(true);
        Spawn(client, wr2.ToArraySegment());
    }

    public void Spawn(Client client, ArraySegment<byte> payload = default)
    {
        NetworkWriter wr = new NetworkWriter();

        wr.WriteUShort(NetworkMessageId<SpawnMessage>.Id);

        wr.WriteUInt(NetworkId);

        // IsLocalPlayer
        wr.WriteBool(Owner == client);
        // IsOwner
        wr.WriteBool(Owner == client);

        wr.WriteULong(SceneId);
        wr.WriteUInt(AssetId);

        wr.WriteVector3(Position);
        wr.WriteQuaternion(Rotation);
        wr.WriteVector3(Scale);

        wr.WriteArraySegmentAndSize(payload);

        client.SendMirrorData(wr);
    }

    public virtual void OnReceiveCommand(byte componentIndex, ushort functionHash, ArraySegment<byte> payload = default)
    {
        if (Behaviours.Length > componentIndex && Behaviours[componentIndex] != null)
            Behaviours[componentIndex].OnReceiveCommand(functionHash, payload);
    }

    public void Dispose()
    {
        World.Objects.Remove(NetworkId);
        Owner = null;
    }
}

