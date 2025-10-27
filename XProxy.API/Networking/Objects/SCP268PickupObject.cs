
namespace XProxy.API.Networking.Objects;

//
// Name: SCP268Pickup
// NetworkID: 0
// AssetID: 3711531185
// SceneID: 0
// Path: SCP268Pickup
//
public class SCP268PickupObject : NetworkObject
{
    public const uint ObjectAssetId = 3711531185;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public SCP268PickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
