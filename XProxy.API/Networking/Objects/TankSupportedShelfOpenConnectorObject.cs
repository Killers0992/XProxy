
namespace XProxy.API.Networking.Objects;

//
// Name: Tank-Supported Shelf Open Connector
// NetworkID: 0
// AssetID: 2490430134
// SceneID: 0
// Path: Tank-Supported Shelf Open Connector
//
public class TankSupportedShelfOpenConnectorObject : NetworkObject
{
    public const uint ObjectAssetId = 2490430134;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableClutterConnectorComponent SpawnableClutterConnector { get; }

    public TankSupportedShelfOpenConnectorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableClutterConnector = new SpawnableClutterConnectorComponent(this);
        Behaviours[0] = SpawnableClutterConnector;
    }
}
