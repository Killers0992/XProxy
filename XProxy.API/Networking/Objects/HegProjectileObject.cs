
namespace XProxy.API.Networking.Objects;

//
// Name: HegProjectile
// NetworkID: 0
// AssetID: 427210814
// SceneID: 0
// Path: HegProjectile
//
public class HegProjectileObject : NetworkObject
{
    public const uint ObjectAssetId = 427210814;

    public override uint AssetId { get; } = ObjectAssetId;
    public ExplosionGrenadeComponent ExplosionGrenade { get; }

    public HegProjectileObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        ExplosionGrenade = new ExplosionGrenadeComponent(this);
        Behaviours[0] = ExplosionGrenade;
    }
}
