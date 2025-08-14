using UnityEngine;

namespace XProxy.Objects.Components;

public class TextToyComponent : BehaviourInfo
{
    string _text;
    Vector2 _displaySize = new Vector2(200f, 50f);

    public string Text
    {
        get => _text;
        set
        {
            SetSyncVarDirtyBit(64UL);
            _text = value;
        }
    }

    public Vector2 DisplaySize
    {
        get => _displaySize;
        set
        {
            SetSyncVarDirtyBit(32UL);
            _displaySize = value;
        }
    }

    public TextToyComponent(SpawnableObject owner) : base(owner, new SyncObjectInfo())
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
            writer.WriteBool(false);

            // TextToy
            writer.WriteVector2(_displaySize);
            writer.WriteString(_text);
            return;
        }

        if ((SyncVarDirtyBits & 32UL) != 0)
        {
            writer.WriteVector2(_displaySize);
        }

        if ((SyncVarDirtyBits & 64UL) != 0)
        {
            writer.WriteString(_text);
        }
    }

    void AfterSerialize(NetworkWriter writer, bool intial)
    {
        if (intial)
            writer.WriteUInt(0);
    }
}
