
namespace XProxy.API.Networking.Objects;

//
// Name: Boxes Ladder Open Connector
// NetworkID: 0
// AssetID: 1102032353
// SceneID: 0
// Path: Boxes Ladder Open Connector
//
public class BoxesLadderOpenConnectorObject : NetworkObject
{
    public const uint ObjectAssetId = 1102032353;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableClutterConnectorComponent SpawnableClutterConnector { get; }

    public BoxesLadderOpenConnectorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableClutterConnector = new SpawnableClutterConnectorComponent(this);
        Behaviours[0] = SpawnableClutterConnector;
    }
}
