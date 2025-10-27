using InventorySystem.Items.Pickups;
using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class AmmoPickupComponent : BehaviourComponent
{

    private PickupSyncInfo _info;

    private ushort _savedAmmo;

    public PickupSyncInfo Info
    {
        get => _info;
        set
        {
            SetSyncVarDirtyBit(1);
            _info = value;
        }
    }

    public ushort SavedAmmo
    {
        get => _savedAmmo;
        set
        {
            SetSyncVarDirtyBit(2);
            _savedAmmo = value;
        }
    }

    public AmmoPickupComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<byte>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WritePickupSyncInfo(_info);
            writer.WriteUShort(_savedAmmo);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WritePickupSyncInfo(_info);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteUShort(_savedAmmo);
        }
    }
}
