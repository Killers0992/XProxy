
namespace XProxy.API.Networking.Components;

public class PlayerStatsComponent : BehaviourComponent
{

    public PlayerStatsComponent(NetworkObject networkObject) : base(networkObject)
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
