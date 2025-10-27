using InventorySystem.Items.Pickups;
using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class Scp244DeployablePickupComponent : BehaviourComponent
{

    private PickupSyncInfo _info;

    private byte _syncSizePercent;

    private byte _syncState;

    public PickupSyncInfo Info
    {
        get => _info;
        set
        {
            SetSyncVarDirtyBit(1);
            _info = value;
        }
    }

    public byte SyncSizePercent
    {
        get => _syncSizePercent;
        set
        {
            SetSyncVarDirtyBit(2);
            _syncSizePercent = value;
        }
    }

    public byte SyncState
    {
        get => _syncState;
        set
        {
            SetSyncVarDirtyBit(4);
            _syncState = value;
        }
    }

    public Scp244DeployablePickupComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<byte>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WritePickupSyncInfo(_info);
            writer.WriteByte(_syncSizePercent);
            writer.WriteByte(_syncState);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WritePickupSyncInfo(_info);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteByte(_syncSizePercent);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteByte(_syncState);
        }
    }
}
