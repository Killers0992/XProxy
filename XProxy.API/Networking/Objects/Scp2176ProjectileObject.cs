
namespace XProxy.API.Networking.Objects;

//
// Name: Scp2176Projectile
// NetworkID: 0
// AssetID: 1983050408
// SceneID: 0
// Path: Scp2176Projectile
//
public class Scp2176ProjectileObject : NetworkObject
{
    public const uint ObjectAssetId = 1983050408;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp2176ProjectileComponent Scp2176Projectile { get; }

    public Scp2176ProjectileObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp2176Projectile = new Scp2176ProjectileComponent(this);
        Behaviours[0] = Scp2176Projectile;
    }
}
