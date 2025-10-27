
namespace XProxy.API.Networking.Components;

public class InteractionCoordinatorComponent : BehaviourComponent
{

    public InteractionCoordinatorComponent(NetworkObject networkObject) : base(networkObject)
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
