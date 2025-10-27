using Mirror;
using System;

namespace XProxy.API.Networking.Components;

public class ExperimentalWeaponLockerComponent : BehaviourComponent
{

    private ushort _openedChambers;

    public ushort OpenedChambers
    {
        get => _openedChambers;
        set
        {
            SetSyncVarDirtyBit(1);
            _openedChambers = value;
        }
    }

    public ExperimentalWeaponLockerComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteUShort(_openedChambers);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteUShort(_openedChambers);
        }
    }
}
