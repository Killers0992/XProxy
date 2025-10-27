
namespace XProxy.API.Networking.Objects;

//
// Name: Simple Boxes Open Connector
// NetworkID: 0
// AssetID: 1687661105
// SceneID: 0
// Path: Simple Boxes Open Connector
//
public class SimpleBoxesOpenConnectorObject : NetworkObject
{
    public const uint ObjectAssetId = 1687661105;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableClutterConnectorComponent SpawnableClutterConnector { get; }

    public SimpleBoxesOpenConnectorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableClutterConnector = new SpawnableClutterConnectorComponent(this);
        Behaviours[0] = SpawnableClutterConnector;
    }
}
