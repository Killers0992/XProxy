
namespace XProxy.API.Networking.Components;

public class SpawnableClutterConnectorComponent : BehaviourComponent
{

    public SpawnableClutterConnectorComponent(NetworkObject networkObject) : base(networkObject)
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
