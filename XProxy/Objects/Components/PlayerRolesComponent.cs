namespace XProxy.Objects.Components;

public class PlayerRolesComponent : BehaviourInfo
{
    string _roleName, _roleColor;

    public string RoleName
    {
        get => _roleName;
        set
        {
            SetSyncVarDirtyBit(1);
            _roleName = value;
        }
    }

    public string RoleColor
    {
        get => _roleColor;
        set
        {
            SetSyncVarDirtyBit(2);
            _roleColor = value;
        }
    }

    public PlayerRolesComponent(SpawnableObject owner) : base(owner)
    {
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool intial)
    {
        if ((SyncVarDirtyBits & 1) != 0)
        {
            writer.WriteString(_roleName);
        }
        if ((SyncVarDirtyBits & 2) != 0)
        {
            writer.WriteString(_roleColor);
        }
    }
}
