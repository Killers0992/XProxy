using System;

namespace XProxy.API.Networking.Components;

public class PlayerEffectsControllerComponent : BehaviourComponent
{

    public PlayerEffectsControllerComponent(NetworkObject networkObject) : base(networkObject, new SyncListObject<byte>())
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            return;
        }
    }
}
