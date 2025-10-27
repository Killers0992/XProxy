using Mirror;
using UnityEngine;
using System;
using AdminToys;

namespace XProxy.API.Networking.Components;

public class PrimitiveObjectToyComponent : BehaviourComponent
{

    private Vector3 _position;

    private Quaternion _rotation;

    private Vector3 _scale;

    private byte _movementSmoothing;

    private bool _isStatic;

    private PrimitiveType _primitiveType;

    private Color _materialColor;

    private PrimitiveFlags _primitiveFlags;

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

    public PrimitiveType PrimitiveType
    {
        get => _primitiveType;
        set
        {
            SetSyncVarDirtyBit(32);
            _primitiveType = value;
        }
    }

    public Color MaterialColor
    {
        get => _materialColor;
        set
        {
            SetSyncVarDirtyBit(64);
            _materialColor = value;
        }
    }

    public PrimitiveFlags PrimitiveFlags
    {
        get => _primitiveFlags;
        set
        {
            SetSyncVarDirtyBit(128);
            _primitiveFlags = value;
        }
    }

    public PrimitiveObjectToyComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
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
            writer.Write(_primitiveType);
            writer.WriteColor(_materialColor);
            writer.Write(_primitiveFlags);
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
            writer.Write(_primitiveType);
        }

        if ((SyncVarDirtyBits & 64U) != 0)
        {
            writer.WriteColor(_materialColor);
        }

        if ((SyncVarDirtyBits & 128U) != 0)
        {
            writer.Write(_primitiveFlags);
        }
    }
}
