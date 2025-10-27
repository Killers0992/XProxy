using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class SearchCoordinatorComponent : BehaviourComponent
{

    private float _rayDistance;

    public float RayDistance
    {
        get => _rayDistance;
        set
        {
            SetSyncVarDirtyBit(1);
            _rayDistance = value;
        }
    }

    public SearchCoordinatorComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteFloat(_rayDistance);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteFloat(_rayDistance);
        }
    }
}
