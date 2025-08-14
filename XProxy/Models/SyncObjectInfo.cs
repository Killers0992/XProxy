namespace XProxy.Models;

public class SyncObjectInfo
{
    public void OnSerializeAll(NetworkWriter writer)
    {
        writer.WriteUInt(0);
        writer.WriteUInt(0);
    }

    public void OnSerializeDelta(NetworkWriter writer)
    {
        writer.WriteUInt(0);
    }
}
