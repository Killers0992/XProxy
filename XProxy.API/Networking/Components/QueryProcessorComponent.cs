using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class QueryProcessorComponent : BehaviourComponent
{

    private bool _overridePasswordEnabled;

    public bool OverridePasswordEnabled
    {
        get => _overridePasswordEnabled;
        set
        {
            SetSyncVarDirtyBit(1);
            _overridePasswordEnabled = value;
        }
    }

    public QueryProcessorComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteBool(_overridePasswordEnabled);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteBool(_overridePasswordEnabled);
        }
    }
}
