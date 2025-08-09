using UnityEngine;

namespace XProxy.Objects.Components;

public class TextToyComponent : BehaviourInfo
{
    string _text;

    public string Text
    {
        get => _text;
        set
        {
            SetSyncVarDirtyBit(1);
            _text = value;
        }
    }

    public TextToyComponent(SpawnableObject owner) : base(owner, new SyncObjectInfo())
    {
        this.OnSerializeSyncVars += SerializeSyncVars;
        this.OnAfterSerialize += AfterSerialize;
    }

    void SerializeSyncVars(NetworkWriter writer, bool intial)
    {
        // Spawn payload
        if (intial)
        {
            // AdminToyBase
            writer.WriteVector3(new Vector3(0f, 0f, 0f));
            writer.WriteQuaternion(Quaternion.identity);
            writer.WriteVector3(Vector3.one);
            writer.WriteByte(0);
            writer.WriteBool(true);

            // TextToy
            writer.WriteVector2(new Vector2(1920, 1080));
            writer.WriteString(_text);
            return;
        }

        if ((SyncVarDirtyBits & 64) != 0)
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
