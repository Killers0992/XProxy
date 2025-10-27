
namespace XProxy.API.Networking.Components;

public class GameConsoleTransmissionComponent : BehaviourComponent
{

    public GameConsoleTransmissionComponent(NetworkObject networkObject) : base(networkObject)
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
