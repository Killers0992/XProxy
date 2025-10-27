
namespace XProxy.API.Networking.Components;

public class HintDisplayComponent : BehaviourComponent
{

    public HintDisplayComponent(NetworkObject networkObject) : base(networkObject)
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
