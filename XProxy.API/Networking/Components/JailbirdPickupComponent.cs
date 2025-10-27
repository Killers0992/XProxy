using InventorySystem.Items.Pickups;
using Mirror;
using InventorySystem.Items.Jailbird;
using System;

namespace XProxy.API.Networking.Components;

public class JailbirdPickupComponent : BehaviourComponent
{

    private PickupSyncInfo _info;

    private JailbirdWearState _wear;

    public PickupSyncInfo Info
    {
        get => _info;
        set
        {
            SetSyncVarDirtyBit(1);
            _info = value;
        }
    }

    public JailbirdWearState Wear
    {
        get => _wear;
        set
        {
            SetSyncVarDirtyBit(2);
            _wear = value;
        }
    }

    public JailbirdPickupComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<byte>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WritePickupSyncInfo(_info);
            writer.Write(_wear);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WritePickupSyncInfo(_info);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.Write(_wear);
        }
    }
}
