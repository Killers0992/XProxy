
namespace XProxy.API.Networking.Objects;

//
// Name: SCP1853Pickup
// NetworkID: 0
// AssetID: 2702950243
// SceneID: 0
// Path: SCP1853Pickup
//
public class SCP1853PickupObject : NetworkObject
{
    public const uint ObjectAssetId = 2702950243;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public SCP1853PickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
