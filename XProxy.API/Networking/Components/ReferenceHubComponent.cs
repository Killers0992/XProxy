
namespace XProxy.API.Networking.Components;

public class ReferenceHubComponent : BehaviourComponent
{

    private RecyclablePlayerId _playerId;

    public RecyclablePlayerId PlayerId
    {
        get => _playerId;
        set
        {
            SetSyncVarDirtyBit(1);
            _playerId = value;
        }
    }

    public ReferenceHubComponent(NetworkObject networkObject) : base(networkObject)
    {
        //
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    void SerializeSyncVars(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WriteRecyclablePlayerId(_playerId);
            return;
        }

        if ((SyncVarDirtyBits & 1U) != 0)
        {
            writer.WriteRecyclablePlayerId(_playerId);
        }
    }
}
