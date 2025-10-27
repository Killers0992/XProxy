using Mirror;
using UnityEngine;
using System;

namespace XProxy.API.Networking.Components;

public class SpeakerToyComponent : BehaviourComponent
{

    private Vector3 _position;

    private Quaternion _rotation;

    private Vector3 _scale;

    private byte _movementSmoothing;

    private bool _isStatic;

    private byte _controllerId;

    private bool _isSpatial;

    private float _volume;

    private float _minDistance;

    private float _maxDistance;

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

    public byte ControllerId
    {
        get => _controllerId;
        set
        {
            SetSyncVarDirtyBit(32);
            _controllerId = value;
        }
    }

    public bool IsSpatial
    {
        get => _isSpatial;
        set
        {
            SetSyncVarDirtyBit(64);
            _isSpatial = value;
        }
    }

    public float Volume
    {
        get => _volume;
        set
        {
            SetSyncVarDirtyBit(128);
            _volume = value;
        }
    }

    public float MinDistance
    {
        get => _minDistance;
        set
        {
            SetSyncVarDirtyBit(256);
            _minDistance = value;
        }
    }

    public float MaxDistance
    {
        get => _maxDistance;
        set
        {
            SetSyncVarDirtyBit(512);
            _maxDistance = value;
        }
    }

    public SpeakerToyComponent(NetworkObject networkObject) : base(networkObject)
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
            writer.WriteByte(_controllerId);
            writer.WriteBool(_isSpatial);
            writer.WriteFloat(_volume);
            writer.WriteFloat(_minDistance);
            writer.WriteFloat(_maxDistance);
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
            writer.WriteByte(_controllerId);
        }

        if ((SyncVarDirtyBits & 64U) != 0)
        {
            writer.WriteBool(_isSpatial);
        }

        if ((SyncVarDirtyBits & 128U) != 0)
        {
            writer.WriteFloat(_volume);
        }

        if ((SyncVarDirtyBits & 256U) != 0)
        {
            writer.WriteFloat(_minDistance);
        }

        if ((SyncVarDirtyBits & 512U) != 0)
        {
            writer.WriteFloat(_maxDistance);
        }
    }
}
