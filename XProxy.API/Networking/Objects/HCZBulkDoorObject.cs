
namespace XProxy.API.Networking.Objects;

//
// Name: HCZ BulkDoor
// NetworkID: 0
// AssetID: 2176035362
// SceneID: 0
// Path: HCZ BulkDoor
//
public class HCZBulkDoorObject : NetworkObject
{
    public const uint ObjectAssetId = 2176035362;

    public override uint AssetId { get; } = ObjectAssetId;
    public PryableDoorComponent PryableDoor { get; }
    public SpawnableRoomConnectorComponent SpawnableRoomConnector { get; }

    public HCZBulkDoorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PryableDoor = new PryableDoorComponent(this);
        Behaviours[0] = PryableDoor;

        SpawnableRoomConnector = new SpawnableRoomConnectorComponent(this);
        Behaviours[1] = SpawnableRoomConnector;
    }
}
