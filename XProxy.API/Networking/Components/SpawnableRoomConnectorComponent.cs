
namespace XProxy.API.Networking.Components;

public class SpawnableRoomConnectorComponent : BehaviourComponent
{

    public SpawnableRoomConnectorComponent(NetworkObject networkObject) : base(networkObject)
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
