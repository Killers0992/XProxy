using Mirror;
using System;
using static ServerConfigSynchronizer;

namespace XProxy.API.Networking.Components;

public class ServerConfigSynchronizerComponent : BehaviourComponent
{

    private byte _mainBoolsSync;

    private string _serverName;

    private bool _enableRemoteAdminPredefinedBanTemplates;

    private string _remoteAdminExternalPlayerLookupMode;

    private string _remoteAdminExternalPlayerLookupURL;

    public byte MainBoolsSync
    {
        get => _mainBoolsSync;
        set
        {
            SetSyncVarDirtyBit(1);
            _mainBoolsSync = value;
        }
    }

    public string ServerName
    {
        get => _serverName;
        set
        {
            SetSyncVarDirtyBit(2);
            _serverName = value;
        }
    }

    public bool EnableRemoteAdminPredefinedBanTemplates
    {
        get => _enableRemoteAdminPredefinedBanTemplates;
        set
        {
            SetSyncVarDirtyBit(4);
            _enableRemoteAdminPredefinedBanTemplates = value;
        }
    }

    public string RemoteAdminExternalPlayerLookupMode
    {
        get => _remoteAdminExternalPlayerLookupMode;
        set
        {
            SetSyncVarDirtyBit(8);
            _remoteAdminExternalPlayerLookupMode = value;
        }
    }

    public string RemoteAdminExternalPlayerLookupURL
    {
        get => _remoteAdminExternalPlayerLookupURL;
        set
        {
            SetSyncVarDirtyBit(16);
            _remoteAdminExternalPlayerLookupURL = value;
        }
    }

    public ServerConfigSynchronizerComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<sbyte>(), new SyncListObject<AmmoLimit>(), new SyncListObject<PredefinedBanTemplate>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteByte(_mainBoolsSync);
            writer.WriteString(_serverName);
            writer.WriteBool(_enableRemoteAdminPredefinedBanTemplates);
            writer.WriteString(_remoteAdminExternalPlayerLookupMode);
            writer.WriteString(_remoteAdminExternalPlayerLookupURL);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteByte(_mainBoolsSync);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteString(_serverName);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteBool(_enableRemoteAdminPredefinedBanTemplates);
        }

        if ((SyncVarDirtyBits & 8U) != 0)
        {
            writer.WriteString(_remoteAdminExternalPlayerLookupMode);
        }

        if ((SyncVarDirtyBits & 16U) != 0)
        {
            writer.WriteString(_remoteAdminExternalPlayerLookupURL);
        }
    }
}
