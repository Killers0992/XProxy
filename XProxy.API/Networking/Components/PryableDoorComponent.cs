using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class PryableDoorComponent : BehaviourComponent
{

    private bool _targetState;

    private ushort _activeLocks;

    private byte _doorId;

    private bool _restrict106WhileLocked;

    public bool TargetState
    {
        get => _targetState;
        set
        {
            SetSyncVarDirtyBit(1);
            _targetState = value;
        }
    }

    public ushort ActiveLocks
    {
        get => _activeLocks;
        set
        {
            SetSyncVarDirtyBit(2);
            _activeLocks = value;
        }
    }

    public byte DoorId
    {
        get => _doorId;
        set
        {
            SetSyncVarDirtyBit(4);
            _doorId = value;
        }
    }

    public bool Restrict106WhileLocked
    {
        get => _restrict106WhileLocked;
        set
        {
            SetSyncVarDirtyBit(8);
            _restrict106WhileLocked = value;
        }
    }

    public PryableDoorComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteBool(_targetState);
            writer.WriteUShort(_activeLocks);
            writer.WriteByte(_doorId);
            writer.WriteBool(_restrict106WhileLocked);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteBool(_targetState);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteUShort(_activeLocks);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteByte(_doorId);
        }

        if ((SyncVarDirtyBits & 8U) != 0)
        {
            writer.WriteBool(_restrict106WhileLocked);
        }
    }
}
