
namespace XProxy.API.Networking.Components;

public class SinkholeEnvironmentalHazardComponent : BehaviourComponent
{

    public SinkholeEnvironmentalHazardComponent(NetworkObject networkObject) : base(networkObject)
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
