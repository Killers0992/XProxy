
namespace XProxy.API.Networking.Objects;

//
// Name: Scp018Projectile Halloween
// NetworkID: 0
// AssetID: 4075838184
// SceneID: 0
// Path: Scp018Projectile Halloween
//
public class Scp018ProjectileHalloweenObject : NetworkObject
{
    public const uint ObjectAssetId = 4075838184;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp018ProjectileComponent Scp018Projectile { get; }

    public Scp018ProjectileHalloweenObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp018Projectile = new Scp018ProjectileComponent(this);
        Behaviours[0] = Scp018Projectile;
    }
}
