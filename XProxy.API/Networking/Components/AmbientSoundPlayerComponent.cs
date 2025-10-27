
namespace XProxy.API.Networking.Components;

public class AmbientSoundPlayerComponent : BehaviourComponent
{

    public AmbientSoundPlayerComponent(NetworkObject networkObject) : base(networkObject)
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
