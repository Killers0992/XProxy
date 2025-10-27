
namespace XProxy.API.Networking.Objects;

//
// Name: Ammo44calPickup
// NetworkID: 0
// AssetID: 1499866827
// SceneID: 0
// Path: Ammo44calPickup
//
public class Ammo44calPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 1499866827;

    public override uint AssetId { get; } = ObjectAssetId;
    public AmmoPickupComponent AmmoPickup { get; }

    public Ammo44calPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        AmmoPickup = new AmmoPickupComponent(this);
        Behaviours[0] = AmmoPickup;
    }
}
