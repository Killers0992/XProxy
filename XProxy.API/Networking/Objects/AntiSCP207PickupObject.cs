
namespace XProxy.API.Networking.Objects;

//
// Name: AntiSCP207Pickup
// NetworkID: 0
// AssetID: 1209253563
// SceneID: 0
// Path: AntiSCP207Pickup
//
public class AntiSCP207PickupObject : NetworkObject
{
    public const uint ObjectAssetId = 1209253563;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public AntiSCP207PickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
