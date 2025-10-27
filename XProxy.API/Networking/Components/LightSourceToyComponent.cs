using Mirror;
using UnityEngine;
using System;

namespace XProxy.API.Networking.Components;

public class LightSourceToyComponent : BehaviourComponent
{

    private Vector3 _position;

    private Quaternion _rotation;

    private Vector3 _scale;

    private byte _movementSmoothing;

    private bool _isStatic;

    private float _lightIntensity;

    private float _lightRange;

    private Color _lightColor;

    private LightShadows _shadowType;

    private float _shadowStrength;

    private LightType _lightType;

    private LightShape _lightShape;

    private float _spotAngle;

    private float _innerSpotAngle;

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

    public float LightIntensity
    {
        get => _lightIntensity;
        set
        {
            SetSyncVarDirtyBit(32);
            _lightIntensity = value;
        }
    }

    public float LightRange
    {
        get => _lightRange;
        set
        {
            SetSyncVarDirtyBit(64);
            _lightRange = value;
        }
    }

    public Color LightColor
    {
        get => _lightColor;
        set
        {
            SetSyncVarDirtyBit(128);
            _lightColor = value;
        }
    }

    public LightShadows ShadowType
    {
        get => _shadowType;
        set
        {
            SetSyncVarDirtyBit(256);
            _shadowType = value;
        }
    }

    public float ShadowStrength
    {
        get => _shadowStrength;
        set
        {
            SetSyncVarDirtyBit(512);
            _shadowStrength = value;
        }
    }

    public LightType LightType
    {
        get => _lightType;
        set
        {
            SetSyncVarDirtyBit(1024);
            _lightType = value;
        }
    }

    public LightShape LightShape
    {
        get => _lightShape;
        set
        {
            SetSyncVarDirtyBit(2048);
            _lightShape = value;
        }
    }

    public float SpotAngle
    {
        get => _spotAngle;
        set
        {
            SetSyncVarDirtyBit(4096);
            _spotAngle = value;
        }
    }

    public float InnerSpotAngle
    {
        get => _innerSpotAngle;
        set
        {
            SetSyncVarDirtyBit(8192);
            _innerSpotAngle = value;
        }
    }

    public LightSourceToyComponent(NetworkObject networkObject) : base(networkObject)
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
            writer.WriteFloat(_lightIntensity);
            writer.WriteFloat(_lightRange);
            writer.WriteColor(_lightColor);
            writer.Write(_shadowType);
            writer.WriteFloat(_shadowStrength);
            writer.Write(_lightType);
            writer.Write(_lightShape);
            writer.WriteFloat(_spotAngle);
            writer.WriteFloat(_innerSpotAngle);
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
            writer.WriteFloat(_lightIntensity);
        }

        if ((SyncVarDirtyBits & 64U) != 0)
        {
            writer.WriteFloat(_lightRange);
        }

        if ((SyncVarDirtyBits & 128U) != 0)
        {
            writer.WriteColor(_lightColor);
        }

        if ((SyncVarDirtyBits & 256U) != 0)
        {
            writer.Write(_shadowType);
        }

        if ((SyncVarDirtyBits & 512U) != 0)
        {
            writer.WriteFloat(_shadowStrength);
        }

        if ((SyncVarDirtyBits & 1024U) != 0)
        {
            writer.Write(_lightType);
        }

        if ((SyncVarDirtyBits & 2048U) != 0)
        {
            writer.Write(_lightShape);
        }

        if ((SyncVarDirtyBits & 4096U) != 0)
        {
            writer.WriteFloat(_spotAngle);
        }

        if ((SyncVarDirtyBits & 8192U) != 0)
        {
            writer.WriteFloat(_innerSpotAngle);
        }
    }
}
