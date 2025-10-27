
namespace XProxy.API.Networking.Objects;

//
// Name: SCP207Pickup
// NetworkID: 0
// AssetID: 689511071
// SceneID: 0
// Path: SCP207Pickup
//
public class SCP207PickupObject : NetworkObject
{
    public const uint ObjectAssetId = 689511071;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public SCP207PickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
