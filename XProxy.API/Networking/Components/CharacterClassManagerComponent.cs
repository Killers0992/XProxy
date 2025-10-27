using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class CharacterClassManagerComponent : BehaviourComponent
{

    private string _pastebin;

    private ushort _maxPlayers;

    private bool _roundStarted;

    public string Pastebin
    {
        get => _pastebin;
        set
        {
            SetSyncVarDirtyBit(1);
            _pastebin = value;
        }
    }

    public ushort MaxPlayers
    {
        get => _maxPlayers;
        set
        {
            SetSyncVarDirtyBit(2);
            _maxPlayers = value;
        }
    }

    public bool RoundStarted
    {
        get => _roundStarted;
        set
        {
            SetSyncVarDirtyBit(4);
            _roundStarted = value;
        }
    }

    public CharacterClassManagerComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteString(_pastebin);
            writer.WriteUShort(_maxPlayers);
            writer.WriteBool(_roundStarted);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteString(_pastebin);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteUShort(_maxPlayers);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteBool(_roundStarted);
        }
    }
}
