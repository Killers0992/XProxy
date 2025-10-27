
namespace XProxy.API.Networking.Objects;

//
// Name: CoinPickup
// NetworkID: 0
// AssetID: 3134959991
// SceneID: 0
// Path: CoinPickup
//
public class CoinPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 3134959991;

    public override uint AssetId { get; } = ObjectAssetId;
    public CollisionDetectionPickupComponent CollisionDetectionPickup { get; }

    public CoinPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CollisionDetectionPickup = new CollisionDetectionPickupComponent(this);
        Behaviours[0] = CollisionDetectionPickup;
    }
}
