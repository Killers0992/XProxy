
namespace XProxy.API.Networking.Components;

public class CheaterReportComponent : BehaviourComponent
{

    public CheaterReportComponent(NetworkObject networkObject) : base(networkObject)
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
