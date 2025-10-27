using Mirror;
using InventorySystem.Items;
using System;

namespace XProxy.API.Networking.Components;

public class InventoryComponent : BehaviourComponent
{

    private ItemIdentifier _curItem;

    private float _syncStaminaModifier;

    private float _syncMovementLimiter;

    private float _syncMovementMultiplier;

    public ItemIdentifier CurItem
    {
        get => _curItem;
        set
        {
            SetSyncVarDirtyBit(1);
            _curItem = value;
        }
    }

    public float SyncStaminaModifier
    {
        get => _syncStaminaModifier;
        set
        {
            SetSyncVarDirtyBit(2);
            _syncStaminaModifier = value;
        }
    }

    public float SyncMovementLimiter
    {
        get => _syncMovementLimiter;
        set
        {
            SetSyncVarDirtyBit(4);
            _syncMovementLimiter = value;
        }
    }

    public float SyncMovementMultiplier
    {
        get => _syncMovementMultiplier;
        set
        {
            SetSyncVarDirtyBit(8);
            _syncMovementMultiplier = value;
        }
    }

    public InventoryComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.Write(_curItem);
            writer.WriteFloat(_syncStaminaModifier);
            writer.WriteFloat(_syncMovementLimiter);
            writer.WriteFloat(_syncMovementMultiplier);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.Write(_curItem);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteFloat(_syncStaminaModifier);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteFloat(_syncMovementLimiter);
        }

        if ((SyncVarDirtyBits & 8U) != 0)
        {
            writer.WriteFloat(_syncMovementMultiplier);
        }
    }
}
