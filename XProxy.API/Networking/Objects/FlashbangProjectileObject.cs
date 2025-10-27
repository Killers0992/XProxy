
namespace XProxy.API.Networking.Objects;

//
// Name: FlashbangProjectile
// NetworkID: 0
// AssetID: 2409733045
// SceneID: 0
// Path: FlashbangProjectile
//
public class FlashbangProjectileObject : NetworkObject
{
    public const uint ObjectAssetId = 2409733045;

    public override uint AssetId { get; } = ObjectAssetId;
    public FlashbangGrenadeComponent FlashbangGrenade { get; }

    public FlashbangProjectileObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        FlashbangGrenade = new FlashbangGrenadeComponent(this);
        Behaviours[0] = FlashbangGrenade;
    }
}
