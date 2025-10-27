
namespace XProxy.API.Networking.Objects;

//
// Name: EZ BreakableDoor
// NetworkID: 0
// AssetID: 1883254029
// SceneID: 0
// Path: EZ BreakableDoor
//
public class EZBreakableDoorObject : NetworkObject
{
    public const uint ObjectAssetId = 1883254029;

    public override uint AssetId { get; } = ObjectAssetId;
    public BreakableDoorComponent BreakableDoor { get; }
    public SpawnableRoomConnectorComponent SpawnableRoomConnector { get; }

    public EZBreakableDoorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        BreakableDoor = new BreakableDoorComponent(this);
        Behaviours[0] = BreakableDoor;

        SpawnableRoomConnector = new SpawnableRoomConnectorComponent(this);
        Behaviours[1] = SpawnableRoomConnector;
    }
}
