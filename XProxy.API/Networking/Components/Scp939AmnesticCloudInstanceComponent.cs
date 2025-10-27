using Mirror;
using System;
using RelativePositioning;

namespace XProxy.API.Networking.Components;

public class Scp939AmnesticCloudInstanceComponent : BehaviourComponent
{

    private byte _syncHoldTime;

    private byte _syncState;

    private uint _syncOwner;

    private RelativePosition _syncPos;

    public byte SyncHoldTime
    {
        get => _syncHoldTime;
        set
        {
            SetSyncVarDirtyBit(1);
            _syncHoldTime = value;
        }
    }

    public byte SyncState
    {
        get => _syncState;
        set
        {
            SetSyncVarDirtyBit(2);
            _syncState = value;
        }
    }

    public uint SyncOwner
    {
        get => _syncOwner;
        set
        {
            SetSyncVarDirtyBit(4);
            _syncOwner = value;
        }
    }

    public RelativePosition SyncPos
    {
        get => _syncPos;
        set
        {
            SetSyncVarDirtyBit(8);
            _syncPos = value;
        }
    }

    public Scp939AmnesticCloudInstanceComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteByte(_syncHoldTime);
            writer.WriteByte(_syncState);
            writer.WriteUInt(_syncOwner);
            writer.WriteRelativePosition(_syncPos);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteByte(_syncHoldTime);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteByte(_syncState);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteUInt(_syncOwner);
        }

        if ((SyncVarDirtyBits & 8U) != 0)
        {
            writer.WriteRelativePosition(_syncPos);
        }
    }
}
