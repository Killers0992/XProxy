namespace XProxy.Models;

public class SyncObjectInfo
{
    public object Type { get; set; }

    public void OnSerializeAll(NetworkWriter writer)
    {
        writer.WriteUInt(0);
        writer.WriteUInt(0);
    }

    public void OnSerializeDelta(NetworkWriter writer)
    {
        writer.WriteUInt(0);
    }

    public void OnDeserializeAll(NetworkReader reader)
    {
        int count = (int)reader.ReadUInt();

        for (int i = 0; i < count; i++)
        {
            switch (Type)
            {
                case ServerConfigSynchronizer.PredefinedBanTemplate _:
                    reader.ReadInt();
                    reader.ReadString();
                    reader.ReadString();
                    break;
                case ServerConfigSynchronizer.AmmoLimit _:
                    ItemType type = (ItemType)reader.ReadByte();
                    ushort amount = reader.ReadUShort();

                    //Logger.Info($"{type} {amount}");
                    break;
                case sbyte _:
                    sbyte s = reader.ReadSByte();
                    //Logger.Info("Category limits " + s);
                    break;
                case string _:
                    reader.ReadString();
                    break;
            }
        }

        reader.ReadUInt();
    }

    public void OnDeserializeDelta(NetworkReader reader)
    {
        int count = (int)reader.ReadUInt();
    }

}
