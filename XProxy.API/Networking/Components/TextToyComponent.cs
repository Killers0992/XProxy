using Mirror;
using UnityEngine;
using System;

namespace XProxy.API.Networking.Components;

public class TextToyComponent : BehaviourComponent
{

    private Vector3 _position;

    private Quaternion _rotation;

    private Vector3 _scale;

    private byte _movementSmoothing;

    private bool _isStatic;

    private Vector2 _displaySize;

    private string _textFormat;

    public Vector3 Position
    {
        get => _position;
        set
        {
            SetSyncVarDirtyBit(1);
            _position = value;
        }
    }

    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            SetSyncVarDirtyBit(2);
            _rotation = value;
        }
    }

    public Vector3 Scale
    {
        get => _scale;
        set
        {
            SetSyncVarDirtyBit(4);
            _scale = value;
        }
    }

    public byte MovementSmoothing
    {
        get => _movementSmoothing;
        set
        {
            SetSyncVarDirtyBit(8);
            _movementSmoothing = value;
        }
    }

    public bool IsStatic
    {
        get => _isStatic;
        set
        {
            SetSyncVarDirtyBit(16);
            _isStatic = value;
        }
    }

    public Vector2 DisplaySize
    {
        get => _displaySize;
        set
        {
            SetSyncVarDirtyBit(32);
            _displaySize = value;
        }
    }

    public string TextFormat
    {
        get => _textFormat;
        set
        {
            SetSyncVarDirtyBit(64);
            _textFormat = value;
        }
    }


    public TextToyComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<string>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
        this.OnBeforeSerialize += BeforeSerialize;
        this.OnAfterSerialize += AfterSerialize;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteVector3(_position);
            writer.WriteQuaternion(_rotation);
            writer.WriteVector3(_scale);
            writer.WriteByte(_movementSmoothing);
            writer.WriteBool(_isStatic);
            writer.WriteVector2(_displaySize);
            writer.WriteString(_textFormat);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteVector3(_position);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteQuaternion(_rotation);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteVector3(_scale);
        }

        if ((SyncVarDirtyBits & 8U) != 0)
        {
            writer.WriteByte(_movementSmoothing);
        }

        if ((SyncVarDirtyBits & 16U) != 0)
        {
            writer.WriteBool(_isStatic);
        }

        if ((SyncVarDirtyBits & 32U) != 0)
        {
            writer.WriteVector2(_displaySize);
        }

        if ((SyncVarDirtyBits & 64U) != 0)
        {
            writer.WriteString(_textFormat);
        }
    }
    void BeforeSerialize(NetworkWriter writer, bool initial)
    {
        if (!initial)
        {
            writer.WriteULong(SyncVarDirtyBits);
        }
    }

    void AfterSerialize(NetworkWriter writer, bool initial)
    {
        if (initial)
        {
            writer.WriteUInt(0);
        }
    }

}
