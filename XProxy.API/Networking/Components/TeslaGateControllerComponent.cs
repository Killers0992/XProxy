
namespace XProxy.API.Networking.Components;

public class TeslaGateControllerComponent : BehaviourComponent
{

    public TeslaGateControllerComponent(NetworkObject networkObject) : base(networkObject)
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
