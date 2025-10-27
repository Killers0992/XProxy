using InventorySystem.Items.Pickups;
using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class Scp2176ProjectileComponent : BehaviourComponent
{

    private PickupSyncInfo _info;

    private double _syncTargetTime;

    private bool _playedDropSound;

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

    public bool PlayedDropSound
    {
        get => _playedDropSound;
        set
        {
            SetSyncVarDirtyBit(4);
            _playedDropSound = value;
        }
    }

    public Scp2176ProjectileComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<byte>())
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
            writer.WriteBool(_playedDropSound);
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

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteBool(_playedDropSound);
        }
    }
}
