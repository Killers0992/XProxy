
namespace XProxy.API.Networking.Components;

public class PlayerRoleManagerComponent : BehaviourComponent
{

    public PlayerRoleManagerComponent(NetworkObject networkObject) : base(networkObject)
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
