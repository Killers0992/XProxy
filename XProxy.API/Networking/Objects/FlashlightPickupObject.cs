
namespace XProxy.API.Networking.Objects;

//
// Name: FlashlightPickup
// NetworkID: 0
// AssetID: 2606539874
// SceneID: 0
// Path: FlashlightPickup
//
public class FlashlightPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 2606539874;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public FlashlightPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
