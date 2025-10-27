using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class ServerRolesComponent : BehaviourComponent
{

    private string _myText;

    private string _myColor;

    private string _globalBadge;

    private string _globalBadgeSignature;

    public string MyText
    {
        get => _myText;
        set
        {
            SetSyncVarDirtyBit(1);
            _myText = value;
        }
    }

    public string MyColor
    {
        get => _myColor;
        set
        {
            SetSyncVarDirtyBit(2);
            _myColor = value;
        }
    }

    public string GlobalBadge
    {
        get => _globalBadge;
        set
        {
            SetSyncVarDirtyBit(4);
            _globalBadge = value;
        }
    }

    public string GlobalBadgeSignature
    {
        get => _globalBadgeSignature;
        set
        {
            SetSyncVarDirtyBit(8);
            _globalBadgeSignature = value;
        }
    }

    public ServerRolesComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteString(_myText);
            writer.WriteString(_myColor);
            writer.WriteString(_globalBadge);
            writer.WriteString(_globalBadgeSignature);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteString(_myText);
        }

        if ((SyncVarDirtyBits & 2U) != 0)
        {
            writer.WriteString(_myColor);
        }

        if ((SyncVarDirtyBits & 4U) != 0)
        {
            writer.WriteString(_globalBadge);
        }

        if ((SyncVarDirtyBits & 8U) != 0)
        {
            writer.WriteString(_globalBadgeSignature);
        }
    }
}
