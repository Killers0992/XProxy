
namespace XProxy.API.Networking.Objects;

//
// Name: Broken Electrical Box Open Connector
// NetworkID: 0
// AssetID: 3999209566
// SceneID: 0
// Path: Broken Electrical Box Open Connector
//
public class BrokenElectricalBoxOpenConnectorObject : NetworkObject
{
    public const uint ObjectAssetId = 3999209566;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableRoomConnectorComponent SpawnableRoomConnector { get; }

    public BrokenElectricalBoxOpenConnectorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableRoomConnector = new SpawnableRoomConnectorComponent(this);
        Behaviours[0] = SpawnableRoomConnector;
    }
}
