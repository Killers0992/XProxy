
namespace XProxy.API.Networking.Objects;

//
// Name: Ammo762mmPickup
// NetworkID: 0
// AssetID: 3685499023
// SceneID: 0
// Path: Ammo762mmPickup
//
public class Ammo762mmPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 3685499023;

    public override uint AssetId { get; } = ObjectAssetId;
    public AmmoPickupComponent AmmoPickup { get; }

    public Ammo762mmPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        AmmoPickup = new AmmoPickupComponent(this);
        Behaviours[0] = AmmoPickup;
    }
}
