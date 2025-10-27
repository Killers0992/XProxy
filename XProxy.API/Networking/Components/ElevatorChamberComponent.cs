using Mirror;
using Interactables.Interobjects;
using System;

namespace XProxy.API.Networking.Components;

public class ElevatorChamberComponent : BehaviourComponent
{

    private ElevatorGroup _assignedGroup;

    private byte _syncDestinationLevel;

    private byte _waypointId;

    public ElevatorGroup AssignedGroup
    {
        get => _assignedGroup;
        set
        {
            SetSyncVarDirtyBit(1);
            _assignedGroup = value;
        }
    }

    public byte SyncDestinationLevel
    {
        get => _syncDestinationLevel;
        set
        {
            SetSyncVarDirtyBit(2);
            _syncDestinationLevel = value;
        }
    }

    public byte WaypointId
    {
        get => _waypointId;
        set
        {
            SetSyncVarDirtyBit(4);
            _waypointId = value;
        }
    }

    public ElevatorChamberComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.Write(_assignedGroup);
            writer.WriteByte(_syncDestinationLevel);
            writer.WriteByte(_waypointId);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.Write(_assignedGroup);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteByte(_syncDestinationLevel);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteByte(_waypointId);
        }
    }
}
