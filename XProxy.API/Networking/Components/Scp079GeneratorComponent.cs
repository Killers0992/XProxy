using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class Scp079GeneratorComponent : BehaviourComponent
{

    private float _totalActivationTime;

    private float _totalDeactivationTime;

    private byte _flags;

    private short _syncTime;

    public float TotalActivationTime
    {
        get => _totalActivationTime;
        set
        {
            SetSyncVarDirtyBit(1);
            _totalActivationTime = value;
        }
    }

    public float TotalDeactivationTime
    {
        get => _totalDeactivationTime;
        set
        {
            SetSyncVarDirtyBit(2);
            _totalDeactivationTime = value;
        }
    }

    public byte Flags
    {
        get => _flags;
        set
        {
            SetSyncVarDirtyBit(4);
            _flags = value;
        }
    }

    public short SyncTime
    {
        get => _syncTime;
        set
        {
            SetSyncVarDirtyBit(8);
            _syncTime = value;
        }
    }

    public Scp079GeneratorComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteFloat(_totalActivationTime);
            writer.WriteFloat(_totalDeactivationTime);
            writer.WriteByte(_flags);
            writer.WriteShort(_syncTime);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteFloat(_totalActivationTime);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteFloat(_totalDeactivationTime);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteByte(_flags);
        }

        if ((SyncVarDirtyBits & 8U) != 0)
        {
            writer.WriteShort(_syncTime);
        }
    }
}
