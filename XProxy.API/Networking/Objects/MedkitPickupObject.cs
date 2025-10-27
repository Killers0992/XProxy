
namespace XProxy.API.Networking.Objects;

//
// Name: MedkitPickup
// NetworkID: 0
// AssetID: 2808038258
// SceneID: 0
// Path: MedkitPickup
//
public class MedkitPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 2808038258;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public MedkitPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
