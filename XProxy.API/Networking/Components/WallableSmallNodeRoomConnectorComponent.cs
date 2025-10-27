using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class WallableSmallNodeRoomConnectorComponent : BehaviourComponent
{

    private byte _syncBitmask;

    public byte SyncBitmask
    {
        get => _syncBitmask;
        set
        {
            SetSyncVarDirtyBit(1);
            _syncBitmask = value;
        }
    }

    public WallableSmallNodeRoomConnectorComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteByte(_syncBitmask);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteByte(_syncBitmask);
        }
    }
}
