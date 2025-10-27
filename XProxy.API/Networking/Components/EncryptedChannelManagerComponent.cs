using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class EncryptedChannelManagerComponent : BehaviourComponent
{

    private string _serverRandom;

    public string ServerRandom
    {
        get => _serverRandom;
        set
        {
            SetSyncVarDirtyBit(1);
            _serverRandom = value;
        }
    }

    public EncryptedChannelManagerComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteString(_serverRandom);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteString(_serverRandom);
        }
    }
}
