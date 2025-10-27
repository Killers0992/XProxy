using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class PlayerAuthenticationManagerComponent : BehaviourComponent
{

    private string _syncedUserId;

    public string SyncedUserId
    {
        get => _syncedUserId;
        set
        {
            SetSyncVarDirtyBit(1);
            _syncedUserId = value;
        }
    }

    public PlayerAuthenticationManagerComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteString(_syncedUserId);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteString(_syncedUserId);
        }
    }
}
