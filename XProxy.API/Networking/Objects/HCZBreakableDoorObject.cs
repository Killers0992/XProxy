
namespace XProxy.API.Networking.Objects;

//
// Name: HCZ BreakableDoor
// NetworkID: 0
// AssetID: 2295511789
// SceneID: 0
// Path: HCZ BreakableDoor
//
public class HCZBreakableDoorObject : NetworkObject
{
    public const uint ObjectAssetId = 2295511789;

    public override uint AssetId { get; } = ObjectAssetId;
    public BreakableDoorComponent BreakableDoor { get; }
    public WallableSmallNodeRoomConnectorComponent WallableSmallNodeRoomConnector { get; }

    public HCZBreakableDoorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        BreakableDoor = new BreakableDoorComponent(this);
        Behaviours[0] = BreakableDoor;

        WallableSmallNodeRoomConnector = new WallableSmallNodeRoomConnectorComponent(this);
        Behaviours[1] = WallableSmallNodeRoomConnector;
    }
}
