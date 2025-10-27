using Mirror;
using System;
using LightContainmentZoneDecontamination;
using static LightContainmentZoneDecontamination.DecontaminationController;

namespace XProxy.API.Networking.Components;

public class DecontaminationControllerComponent : BehaviourComponent
{

    private double _roundStartTime;

    private string _elevatorsLockedText;

    private DecontaminationStatus _decontaminationOverride;

    private float _timeOffset;

    public double RoundStartTime
    {
        get => _roundStartTime;
        set
        {
            SetSyncVarDirtyBit(1);
            _roundStartTime = value;
        }
    }

    public string ElevatorsLockedText
    {
        get => _elevatorsLockedText;
        set
        {
            SetSyncVarDirtyBit(2);
            _elevatorsLockedText = value;
        }
    }

    public DecontaminationStatus DecontaminationOverride
    {
        get => _decontaminationOverride;
        set
        {
            SetSyncVarDirtyBit(4);
            _decontaminationOverride = value;
        }
    }

    public float TimeOffset
    {
        get => _timeOffset;
        set
        {
            SetSyncVarDirtyBit(8);
            _timeOffset = value;
        }
    }

    public DecontaminationControllerComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteDouble(_roundStartTime);
            writer.WriteString(_elevatorsLockedText);
            writer.Write(_decontaminationOverride);
            writer.WriteFloat(_timeOffset);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteDouble(_roundStartTime);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteString(_elevatorsLockedText);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.Write(_decontaminationOverride);
        }

        if ((SyncVarDirtyBits & 8U) != 0)
        {
            writer.WriteFloat(_timeOffset);
        }
    }
}
