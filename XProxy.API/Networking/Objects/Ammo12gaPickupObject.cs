
namespace XProxy.API.Networking.Objects;

//
// Name: Ammo12gaPickup
// NetworkID: 0
// AssetID: 4056235189
// SceneID: 0
// Path: Ammo12gaPickup
//
public class Ammo12gaPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 4056235189;

    public override uint AssetId { get; } = ObjectAssetId;
    public AmmoPickupComponent AmmoPickup { get; }

    public Ammo12gaPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        AmmoPickup = new AmmoPickupComponent(this);
        Behaviours[0] = AmmoPickup;
    }
}
