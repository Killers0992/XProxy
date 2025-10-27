
namespace XProxy.API.Networking.Objects;

//
// Name: Huge Orange Pipes Open Connector
// NetworkID: 0
// AssetID: 2536312960
// SceneID: 0
// Path: Huge Orange Pipes Open Connector
//
public class HugeOrangePipesOpenConnectorObject : NetworkObject
{
    public const uint ObjectAssetId = 2536312960;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableClutterConnectorComponent SpawnableClutterConnector { get; }

    public HugeOrangePipesOpenConnectorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableClutterConnector = new SpawnableClutterConnectorComponent(this);
        Behaviours[0] = SpawnableClutterConnector;
    }
}
