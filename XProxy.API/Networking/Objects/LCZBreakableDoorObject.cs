
namespace XProxy.API.Networking.Objects;

//
// Name: LCZ BreakableDoor
// NetworkID: 0
// AssetID: 3038351124
// SceneID: 0
// Path: LCZ BreakableDoor
//
public class LCZBreakableDoorObject : NetworkObject
{
    public const uint ObjectAssetId = 3038351124;

    public override uint AssetId { get; } = ObjectAssetId;
    public BreakableDoorComponent BreakableDoor { get; }
    public SpawnableRoomConnectorComponent SpawnableRoomConnector { get; }

    public LCZBreakableDoorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        BreakableDoor = new BreakableDoorComponent(this);
        Behaviours[0] = BreakableDoor;

        SpawnableRoomConnector = new SpawnableRoomConnectorComponent(this);
        Behaviours[1] = SpawnableRoomConnector;
    }
}
