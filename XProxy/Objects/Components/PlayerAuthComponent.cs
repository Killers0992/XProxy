namespace XProxy.Objects.Components;

public class PlayerAuthComponent : BehaviourInfo
{
    string _userId;

    public string UserId
    {
        get => _userId;
        set
        {
            SetSyncVarDirtyBit(1);
            _userId = value;
        }
    }

    public PlayerAuthComponent(SpawnableObject owner) : base(owner)
    {
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool intial)
    {
        if ((SyncVarDirtyBits & 1) != 0)
        {
            writer.WriteString(_userId);
        }
    }
}
