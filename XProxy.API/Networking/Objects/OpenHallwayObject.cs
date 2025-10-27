
namespace XProxy.API.Networking.Objects;

//
// Name: OpenHallway
// NetworkID: 0
// AssetID: 3343949480
// SceneID: 0
// Path: OpenHallway
//
public class OpenHallwayObject : NetworkObject
{
    public const uint ObjectAssetId = 3343949480;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableRoomConnectorComponent SpawnableRoomConnector { get; }

    public OpenHallwayObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableRoomConnector = new SpawnableRoomConnectorComponent(this);
        Behaviours[0] = SpawnableRoomConnector;
    }
}
