using Mirror;
using UnityEngine;
using System;
using AdminToys;
using MapGeneration;

namespace XProxy.API.Networking.Components;

public class Scp079CameraToyComponent : BehaviourComponent
{

    private Vector3 _position;

    private Quaternion _rotation;

    private Vector3 _scale;

    private byte _movementSmoothing;

    private bool _isStatic;

    private string _label;

    private RoomIdentifier _room;

    private Vector2 _verticalConstraint;

    private Vector2 _horizontalConstraint;

    private Vector2 _zoomConstraint;

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

    public string Label
    {
        get => _label;
        set
        {
            SetSyncVarDirtyBit(32);
            _label = value;
        }
    }

    public RoomIdentifier Room
    {
        get => _room;
        set
        {
            SetSyncVarDirtyBit(64);
            _room = value;
        }
    }

    public Vector2 VerticalConstraint
    {
        get => _verticalConstraint;
        set
        {
            SetSyncVarDirtyBit(128);
            _verticalConstraint = value;
        }
    }

    public Vector2 HorizontalConstraint
    {
        get => _horizontalConstraint;
        set
        {
            SetSyncVarDirtyBit(256);
            _horizontalConstraint = value;
        }
    }

    public Vector2 ZoomConstraint
    {
        get => _zoomConstraint;
        set
        {
            SetSyncVarDirtyBit(512);
            _zoomConstraint = value;
        }
    }

    public Scp079CameraToyComponent(NetworkObject networkObject) : base(networkObject)
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
            writer.WriteString(_label);
            writer.WriteRoomIdentifier(_room);
            writer.WriteVector2(_verticalConstraint);
            writer.WriteVector2(_horizontalConstraint);
            writer.WriteVector2(_zoomConstraint);
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
            writer.WriteString(_label);
        }

        if ((SyncVarDirtyBits & 64U) != 0)
        {
            writer.WriteRoomIdentifier(_room);
        }

        if ((SyncVarDirtyBits & 128U) != 0)
        {
            writer.WriteVector2(_verticalConstraint);
        }

        if ((SyncVarDirtyBits & 256U) != 0)
        {
            writer.WriteVector2(_horizontalConstraint);
        }

        if ((SyncVarDirtyBits & 512U) != 0)
        {
            writer.WriteVector2(_zoomConstraint);
        }
    }
}
