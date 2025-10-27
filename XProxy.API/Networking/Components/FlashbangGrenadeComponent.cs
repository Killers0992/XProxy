using InventorySystem.Items.Pickups;
using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class FlashbangGrenadeComponent : BehaviourComponent
{

    private PickupSyncInfo _info;

    private double _syncTargetTime;

    public PickupSyncInfo Info
    {
        get => _info;
        set
        {
            SetSyncVarDirtyBit(1);
            _info = value;
        }
    }

    public double SyncTargetTime
    {
        get => _syncTargetTime;
        set
        {
            SetSyncVarDirtyBit(2);
            _syncTargetTime = value;
        }
    }

    public FlashbangGrenadeComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<byte>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WritePickupSyncInfo(_info);
            writer.WriteDouble(_syncTargetTime);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WritePickupSyncInfo(_info);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteDouble(_syncTargetTime);
        }
    }
}
