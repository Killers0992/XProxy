using InventorySystem.Items.Pickups;
using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class RadioPickupComponent : BehaviourComponent
{

    private PickupSyncInfo _info;

    private bool _savedEnabled;

    private byte _savedRange;

    public PickupSyncInfo Info
    {
        get => _info;
        set
        {
            SetSyncVarDirtyBit(1);
            _info = value;
        }
    }

    public bool SavedEnabled
    {
        get => _savedEnabled;
        set
        {
            SetSyncVarDirtyBit(2);
            _savedEnabled = value;
        }
    }

    public byte SavedRange
    {
        get => _savedRange;
        set
        {
            SetSyncVarDirtyBit(4);
            _savedRange = value;
        }
    }

    public RadioPickupComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<byte>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WritePickupSyncInfo(_info);
            writer.WriteBool(_savedEnabled);
            writer.WriteByte(_savedRange);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WritePickupSyncInfo(_info);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteBool(_savedEnabled);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteByte(_savedRange);
        }
    }
}
