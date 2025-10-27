
namespace XProxy.API.Networking.Objects;

//
// Name: Angled Fences Open Connector
// NetworkID: 0
// AssetID: 2673083832
// SceneID: 0
// Path: Angled Fences Open Connector
//
public class AngledFencesOpenConnectorObject : NetworkObject
{
    public const uint ObjectAssetId = 2673083832;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableClutterConnectorComponent SpawnableClutterConnector { get; }

    public AngledFencesOpenConnectorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableClutterConnector = new SpawnableClutterConnectorComponent(this);
        Behaviours[0] = SpawnableClutterConnector;
    }
}
