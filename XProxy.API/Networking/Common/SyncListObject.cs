namespace XProxy.API.Networking.Common;

public class SyncListObject<T> : SyncObject
{
    public override void OnSerializeAll(NetworkWriter writer)
    {
        writer.WriteUInt(0);
        writer.WriteUInt(0);
    }

    public override void OnSerializeDelta(NetworkWriter writer)
    {
        writer.WriteUInt(0);
    }

    public override void OnDeserializeAll(NetworkReader reader)
    {
        int count = (int)reader.ReadUInt();

        for (int i = 0; i < count; i++)
        {
            reader.Read<T>();
        }

        reader.ReadUInt();
    }

    public override void OnDeserializeDelta(NetworkReader reader)
    {
        int count = (int)reader.ReadUInt();
    }
}
