using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class WorkstationControllerComponent : BehaviourComponent
{

    private byte _status;

    public byte Status
    {
        get => _status;
        set
        {
            SetSyncVarDirtyBit(1);
            _status = value;
        }
    }

    public WorkstationControllerComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteByte(_status);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteByte(_status);
        }
    }
}
