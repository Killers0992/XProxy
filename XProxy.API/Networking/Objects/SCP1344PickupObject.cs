
namespace XProxy.API.Networking.Objects;

//
// Name: SCP1344Pickup
// NetworkID: 0
// AssetID: 4143962266
// SceneID: 0
// Path: SCP1344Pickup
//
public class SCP1344PickupObject : NetworkObject
{
    public const uint ObjectAssetId = 4143962266;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public SCP1344PickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
