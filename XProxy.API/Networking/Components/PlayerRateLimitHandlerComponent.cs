
namespace XProxy.API.Networking.Components;

public class PlayerRateLimitHandlerComponent : BehaviourComponent
{

    public PlayerRateLimitHandlerComponent(NetworkObject networkObject) : base(networkObject)
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
