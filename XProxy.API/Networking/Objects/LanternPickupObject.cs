
namespace XProxy.API.Networking.Objects;

//
// Name: LanternPickup
// NetworkID: 0
// AssetID: 3532394942
// SceneID: 0
// Path: LanternPickup
//
public class LanternPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 3532394942;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public LanternPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
