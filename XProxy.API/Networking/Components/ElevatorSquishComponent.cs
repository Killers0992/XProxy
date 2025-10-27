
namespace XProxy.API.Networking.Components;

public class ElevatorSquishComponent : BehaviourComponent
{

    public ElevatorSquishComponent(NetworkObject networkObject) : base(networkObject)
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
