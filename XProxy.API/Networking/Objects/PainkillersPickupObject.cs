
namespace XProxy.API.Networking.Objects;

//
// Name: PainkillersPickup
// NetworkID: 0
// AssetID: 3124923193
// SceneID: 0
// Path: PainkillersPickup
//
public class PainkillersPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 3124923193;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public PainkillersPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
