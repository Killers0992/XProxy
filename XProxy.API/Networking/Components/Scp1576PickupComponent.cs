using InventorySystem.Items.Pickups;
using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class Scp1576PickupComponent : BehaviourComponent
{

    private PickupSyncInfo _info;

    private byte _syncHorn;

    public PickupSyncInfo Info
    {
        get => _info;
        set
        {
            SetSyncVarDirtyBit(1);
            _info = value;
        }
    }

    public byte SyncHorn
    {
        get => _syncHorn;
        set
        {
            SetSyncVarDirtyBit(2);
            _syncHorn = value;
        }
    }

    public Scp1576PickupComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<byte>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WritePickupSyncInfo(_info);
            writer.WriteByte(_syncHorn);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WritePickupSyncInfo(_info);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteByte(_syncHorn);
        }
    }
}
