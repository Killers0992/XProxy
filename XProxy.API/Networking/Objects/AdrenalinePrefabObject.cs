
namespace XProxy.API.Networking.Objects;

//
// Name: AdrenalinePrefab
// NetworkID: 0
// AssetID: 1573779433
// SceneID: 0
// Path: AdrenalinePrefab
//
public class AdrenalinePrefabObject : NetworkObject
{
    public const uint ObjectAssetId = 1573779433;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public AdrenalinePrefabObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
