
namespace XProxy.API.Networking.Objects;

//
// Name: Ammo9mmPickup
// NetworkID: 0
// AssetID: 2344368365
// SceneID: 0
// Path: Ammo9mmPickup
//
public class Ammo9mmPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 2344368365;

    public override uint AssetId { get; } = ObjectAssetId;
    public AmmoPickupComponent AmmoPickup { get; }

    public Ammo9mmPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        AmmoPickup = new AmmoPickupComponent(this);
        Behaviours[0] = AmmoPickup;
    }
}
