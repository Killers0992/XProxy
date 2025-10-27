
namespace XProxy.API.Networking.Objects;

//
// Name: Ammo556mmPickup
// NetworkID: 0
// AssetID: 2474630775
// SceneID: 0
// Path: Ammo556mmPickup
//
public class Ammo556mmPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 2474630775;

    public override uint AssetId { get; } = ObjectAssetId;
    public AmmoPickupComponent AmmoPickup { get; }

    public Ammo556mmPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        AmmoPickup = new AmmoPickupComponent(this);
        Behaviours[0] = AmmoPickup;
    }
}
