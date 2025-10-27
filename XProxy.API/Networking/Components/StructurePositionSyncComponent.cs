using Mirror;
using System;
using UnityEngine;

namespace XProxy.API.Networking.Components;

public class StructurePositionSyncComponent : BehaviourComponent
{

    private sbyte _rotationY;

    private Vector3 _position;

    public sbyte RotationY
    {
        get => _rotationY;
        set
        {
            SetSyncVarDirtyBit(1);
            _rotationY = value;
        }
    }

    public Vector3 Position
    {
        get => _position;
        set
        {
            SetSyncVarDirtyBit(2);
            _position = value;
        }
    }

    public StructurePositionSyncComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteSByte(_rotationY);
            writer.WriteVector3(_position);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteSByte(_rotationY);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteVector3(_position);
        }
    }
}
