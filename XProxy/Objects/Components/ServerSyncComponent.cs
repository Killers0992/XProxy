namespace XProxy.Objects.Components;

public class ServerSyncComponent : BehaviourInfo
{
    string _serverName;

    public string ServerName
    {
        get => _serverName;
        set
        {
            SetSyncVarDirtyBit(2);
            _serverName = value;
        }
    }

    public ServerSyncComponent(SpawnableObject owner) : base(owner, 
        new SyncObjectInfo() { Type = (sbyte)0 }, 
        new SyncObjectInfo() { Type = new ServerConfigSynchronizer.AmmoLimit() }, 
        new SyncObjectInfo() { Type = new ServerConfigSynchronizer.PredefinedBanTemplate() })
    {
        this.OnSerializeSyncVars += SerializeSyncVars;
        this.OnDeserializeSyncVars += DeserializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool intial)
    {
        if ((SyncVarDirtyBits & 2) != 0)
        {
            writer.WriteString(_serverName);
        }
    }

    void DeserializeSyncVars(NetworkReader reader, long mask, bool intial)
    {
        if (intial)
        {
            reader.ReadByte();
            _serverName = reader.ReadString();
            reader.ReadBool();
            reader.ReadString();
            reader.ReadString();
            return;
        }

        if ((mask & 2) != 0)
        {
            _serverName = reader.ReadString();
        }
    }
}
