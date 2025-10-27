using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class RoundStartComponent : BehaviourComponent
{

    private short _timer;

    public short Timer
    {
        get => _timer;
        set
        {
            SetSyncVarDirtyBit(1);
            _timer = value;
        }
    }

    public RoundStartComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteShort(_timer);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteShort(_timer);
        }
    }
}
