using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class AlphaWarheadControllerComponent : BehaviourComponent
{

    private AlphaWarheadSyncInfo _info;

    private double _cooldownEndTime;

    public AlphaWarheadSyncInfo Info
    {
        get => _info;
        set
        {
            SetSyncVarDirtyBit(1);
            _info = value;
        }
    }

    public double CooldownEndTime
    {
        get => _cooldownEndTime;
        set
        {
            SetSyncVarDirtyBit(2);
            _cooldownEndTime = value;
        }
    }

    public AlphaWarheadControllerComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteAlphaWarheadSyncInfo(_info);
            writer.WriteDouble(_cooldownEndTime);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteAlphaWarheadSyncInfo(_info);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteDouble(_cooldownEndTime);
        }
    }
}
