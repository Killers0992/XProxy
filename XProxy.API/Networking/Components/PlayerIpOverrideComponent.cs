
namespace XProxy.API.Networking.Components;

public class PlayerIpOverrideComponent : BehaviourComponent
{

    public PlayerIpOverrideComponent(NetworkObject networkObject) : base(networkObject)
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
