using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class ServerTimeComponent : BehaviourComponent
{

    private int _timeFromStartup;

    public int TimeFromStartup
    {
        get => _timeFromStartup;
        set
        {
            SetSyncVarDirtyBit(1);
            _timeFromStartup = value;
        }
    }

    public ServerTimeComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteInt(_timeFromStartup);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteInt(_timeFromStartup);
        }
    }
}
