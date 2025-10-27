using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class HubertMoonComponent : BehaviourComponent
{

    private float _elapsedMovementTime;

    public float ElapsedMovementTime
    {
        get => _elapsedMovementTime;
        set
        {
            SetSyncVarDirtyBit(1);
            _elapsedMovementTime = value;
        }
    }

    public HubertMoonComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteFloat(_elapsedMovementTime);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteFloat(_elapsedMovementTime);
        }
    }
}
