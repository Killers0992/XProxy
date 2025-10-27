
namespace XProxy.API.Networking.Components;

public class SpawnableStructureComponent : BehaviourComponent
{

    public SpawnableStructureComponent(NetworkObject networkObject) : base(networkObject)
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
