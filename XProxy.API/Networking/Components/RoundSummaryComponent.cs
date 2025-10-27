using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class RoundSummaryComponent : BehaviourComponent
{

    private int _extraTargets;

    private int _targetCount;

    public int ExtraTargets
    {
        get => _extraTargets;
        set
        {
            SetSyncVarDirtyBit(1);
            _extraTargets = value;
        }
    }

    public int TargetCount
    {
        get => _targetCount;
        set
        {
            SetSyncVarDirtyBit(2);
            _targetCount = value;
        }
    }

    public RoundSummaryComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteInt(_extraTargets);
            writer.WriteInt(_targetCount);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteInt(_extraTargets);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteInt(_targetCount);
        }
    }
}
