
namespace XProxy.API.Networking.Components;

public class FastRoundRestartControllerComponent : BehaviourComponent
{

    public FastRoundRestartControllerComponent(NetworkObject networkObject) : base(networkObject)
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
