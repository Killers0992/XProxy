using Mirror;
using UnityEngine;
using System;

namespace XProxy.API.Networking.Components;

public class WaypointToyComponent : BehaviourComponent
{

    private Vector3 _position;

    private Quaternion _rotation;

    private Vector3 _scale;

    private byte _movementSmoothing;

    private bool _isStatic;

    private bool _visualizeBounds;

    private float _priority;

    private Vector3 _boundsSize;

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

    public bool VisualizeBounds
    {
        get => _visualizeBounds;
        set
        {
            SetSyncVarDirtyBit(32);
            _visualizeBounds = value;
        }
    }

    public float Priority
    {
        get => _priority;
        set
        {
            SetSyncVarDirtyBit(64);
            _priority = value;
        }
    }

    public Vector3 BoundsSize
    {
        get => _boundsSize;
        set
        {
            SetSyncVarDirtyBit(128);
            _boundsSize = value;
        }
    }

    public byte WaypointId { get; set; }


    public WaypointToyComponent(NetworkObject networkObject) : base(networkObject)
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
            writer.WriteBool(_visualizeBounds);
            writer.WriteFloat(_priority);
            writer.WriteVector3(_boundsSize);
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
            writer.WriteBool(_visualizeBounds);
        }

        if ((SyncVarDirtyBits & 64U) != 0)
        {
            writer.WriteFloat(_priority);
        }

        if ((SyncVarDirtyBits & 128U) != 0)
        {
            writer.WriteVector3(_boundsSize);
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
            writer.WriteByte(WaypointId);
        }
    }

}
