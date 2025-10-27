
namespace XProxy.API.Networking.Objects;

//
// Name: Spawnable Unsecured Pryable GateDoor
// NetworkID: 0
// AssetID: 4046276968
// SceneID: 0
// Path: Spawnable Unsecured Pryable GateDoor
//
public class SpawnableUnsecuredPryableGateDoorObject : NetworkObject
{
    public const uint ObjectAssetId = 4046276968;

    public override uint AssetId { get; } = ObjectAssetId;
    public PryableDoorComponent PryableDoor { get; }

    public SpawnableUnsecuredPryableGateDoorObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        PryableDoor = new PryableDoorComponent(this);
        Behaviours[0] = PryableDoor;
    }
}
