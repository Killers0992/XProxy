using InventorySystem.Items.Pickups;
using Mirror;
using InventorySystem.Items.Usables.Scp330;
using System;

namespace XProxy.API.Networking.Components;

public class Scp330PickupComponent : BehaviourComponent
{

    private PickupSyncInfo _info;

    private CandyKindID _exposedCandy;

    public PickupSyncInfo Info
    {
        get => _info;
        set
        {
            SetSyncVarDirtyBit(1);
            _info = value;
        }
    }

    public CandyKindID ExposedCandy
    {
        get => _exposedCandy;
        set
        {
            SetSyncVarDirtyBit(2);
            _exposedCandy = value;
        }
    }

    public Scp330PickupComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<byte>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WritePickupSyncInfo(_info);
            writer.Write(_exposedCandy);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WritePickupSyncInfo(_info);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.Write(_exposedCandy);
        }
    }
}
