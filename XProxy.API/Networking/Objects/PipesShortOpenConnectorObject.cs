
namespace XProxy.API.Networking.Objects;

//
// Name: Pipes Short Open Connector
// NetworkID: 0
// AssetID: 147203050
// SceneID: 0
// Path: Pipes Short Open Connector
//
public class PipesShortOpenConnectorObject : NetworkObject
{
    public const uint ObjectAssetId = 147203050;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableClutterConnectorComponent SpawnableClutterConnector { get; }

    public PipesShortOpenConnectorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableClutterConnector = new SpawnableClutterConnectorComponent(this);
        Behaviours[0] = SpawnableClutterConnector;
    }
}
