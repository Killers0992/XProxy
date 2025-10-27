
namespace XProxy.API.Networking.Objects;

//
// Name: Scp330Pickup
// NetworkID: 0
// AssetID: 464602874
// SceneID: 0
// Path: Scp330Pickup
//
public class Scp330PickupObject : NetworkObject
{
    public const uint ObjectAssetId = 464602874;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp330PickupComponent Scp330Pickup { get; }

    public Scp330PickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp330Pickup = new Scp330PickupComponent(this);
        Behaviours[0] = Scp330Pickup;
    }
}
