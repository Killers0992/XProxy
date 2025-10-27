
namespace XProxy.API.Networking.Objects;

//
// Name: Pipes Long Open Connector
// NetworkID: 0
// AssetID: 38976586
// SceneID: 0
// Path: Pipes Long Open Connector
//
public class PipesLongOpenConnectorObject : NetworkObject
{
    public const uint ObjectAssetId = 38976586;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableClutterConnectorComponent SpawnableClutterConnector { get; }

    public PipesLongOpenConnectorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableClutterConnector = new SpawnableClutterConnectorComponent(this);
        Behaviours[0] = SpawnableClutterConnector;
    }
}
