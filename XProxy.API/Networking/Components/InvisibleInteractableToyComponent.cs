using Mirror;
using UnityEngine;
using System;
using AdminToys;
using static AdminToys.InvisibleInteractableToy;

namespace XProxy.API.Networking.Components;

public class InvisibleInteractableToyComponent : BehaviourComponent
{

    private Vector3 _position;

    private Quaternion _rotation;

    private Vector3 _scale;

    private byte _movementSmoothing;

    private bool _isStatic;

    private ColliderShape _shape;

    private float _interactionDuration;

    private bool _isLocked;

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

    public ColliderShape Shape
    {
        get => _shape;
        set
        {
            SetSyncVarDirtyBit(32);
            _shape = value;
        }
    }

    public float InteractionDuration
    {
        get => _interactionDuration;
        set
        {
            SetSyncVarDirtyBit(64);
            _interactionDuration = value;
        }
    }

    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            SetSyncVarDirtyBit(128);
            _isLocked = value;
        }
    }

    public InvisibleInteractableToyComponent(NetworkObject networkObject) : base(networkObject)
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
            writer.Write(_shape);
            writer.WriteFloat(_interactionDuration);
            writer.WriteBool(_isLocked);
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
            writer.Write(_shape);
        }

        if ((SyncVarDirtyBits & 64U) != 0)
        {
            writer.WriteFloat(_interactionDuration);
        }

        if ((SyncVarDirtyBits & 128U) != 0)
        {
            writer.WriteBool(_isLocked);
        }
    }
}
