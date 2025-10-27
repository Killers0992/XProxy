using Mirror;
using UnityEngine;

namespace XProxy.API.Networking.Components;

public class SpawnableCullingParentComponent : BehaviourComponent
{

    private Vector3 _boundsPosition;

    private Vector3 _boundsSize;

    public Vector3 BoundsPosition
    {
        get => _boundsPosition;
        set
        {
            SetSyncVarDirtyBit(1);
            _boundsPosition = value;
        }
    }

    public Vector3 BoundsSize
    {
        get => _boundsSize;
        set
        {
            SetSyncVarDirtyBit(2);
            _boundsSize = value;
        }
    }

    public SpawnableCullingParentComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteVector3(_boundsPosition);
            writer.WriteVector3(_boundsSize);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteVector3(_boundsPosition);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteVector3(_boundsSize);
        }
    }
}
