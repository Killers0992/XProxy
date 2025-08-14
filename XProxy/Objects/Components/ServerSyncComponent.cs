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

    public ServerSyncComponent(SpawnableObject owner) : base(owner, new SyncObjectInfo())
    {
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool intial)
    {
        if ((SyncVarDirtyBits & 2) != 0)
        {
            writer.WriteString(_serverName);
        }
    }
}
