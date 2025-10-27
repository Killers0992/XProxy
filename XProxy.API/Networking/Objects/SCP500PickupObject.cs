
namespace XProxy.API.Networking.Objects;

//
// Name: SCP500Pickup
// NetworkID: 0
// AssetID: 1367360155
// SceneID: 0
// Path: SCP500Pickup
//
public class SCP500PickupObject : NetworkObject
{
    public const uint ObjectAssetId = 1367360155;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public SCP500PickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
