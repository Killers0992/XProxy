namespace XProxy.Objects.Components;

public class WaypointComponent : BehaviourInfo
{
    bool _showBounds;

    public bool ShowBounds
    {
        get => _showBounds;
        set
        {
            SetSyncVarDirtyBit(32);
            _showBounds = value;
        }
    }

    public byte WaypointId { get; set; }

    public WaypointComponent(SpawnableObject owner) : base(owner)
    {
        this.OnBeforeSerialize += BeforeSerialize;
        this.OnSerializeSyncVars += SerializeSyncVars;
        this.OnAfterSerialize += AfterSerialize;
    }

    void BeforeSerialize(NetworkWriter writer, bool intial)
    {
        if (!intial)
            writer.WriteULong(SyncVarDirtyBits);
    }

    void SerializeSyncVars(NetworkWriter writer, bool intial)
    {
        // Spawn payload
        if (intial)
        {
            // AdminToyBase
            writer.WriteVector3(Owner.Position);
            writer.WriteQuaternion(Owner.Rotation);
            writer.WriteVector3(Owner.Scale);
            writer.WriteByte(0);
            writer.WriteBool(true);

            writer.WriteBool(_showBounds);
            writer.WriteFloat(1f);
            return;
        }

        if ((SyncVarDirtyBits & 32) != 0)
        {
            writer.WriteBool(_showBounds);
        }
    }

    void AfterSerialize(NetworkWriter writer, bool intial)
    {
        if (intial)
        {
            writer.WriteUInt(0);
            writer.WriteByte(WaypointId);
        }
    }
}
