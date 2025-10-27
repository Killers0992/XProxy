using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class NicknameSyncComponent : BehaviourComponent
{

    private float _viewRange;

    private string _customPlayerInfoString;

    private PlayerInfoArea _playerInfoToShow;

    private string _myNickSync;

    private string _displayName;

    public float ViewRange
    {
        get => _viewRange;
        set
        {
            SetSyncVarDirtyBit(1);
            _viewRange = value;
        }
    }

    public string CustomPlayerInfoString
    {
        get => _customPlayerInfoString;
        set
        {
            SetSyncVarDirtyBit(2);
            _customPlayerInfoString = value;
        }
    }

    public PlayerInfoArea PlayerInfoToShow
    {
        get => _playerInfoToShow;
        set
        {
            SetSyncVarDirtyBit(4);
            _playerInfoToShow = value;
        }
    }

    public string MyNickSync
    {
        get => _myNickSync;
        set
        {
            SetSyncVarDirtyBit(8);
            _myNickSync = value;
        }
    }

    public string DisplayName
    {
        get => _displayName;
        set
        {
            SetSyncVarDirtyBit(16);
            _displayName = value;
        }
    }

    public NicknameSyncComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteFloat(_viewRange);
            writer.WriteString(_customPlayerInfoString);
            writer.Write(_playerInfoToShow);
            writer.WriteString(_myNickSync);
            writer.WriteString(_displayName);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteFloat(_viewRange);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteString(_customPlayerInfoString);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.Write(_playerInfoToShow);
        }

        if ((SyncVarDirtyBits & 8U) != 0)
        {
            writer.WriteString(_myNickSync);
        }

        if ((SyncVarDirtyBits & 16U) != 0)
        {
            writer.WriteString(_displayName);
        }
    }
}
