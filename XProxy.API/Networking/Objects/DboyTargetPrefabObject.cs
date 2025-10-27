
namespace XProxy.API.Networking.Objects;

//
// Name: dboyTargetPrefab
// NetworkID: 0
// AssetID: 858699872
// SceneID: 0
// Path: dboyTargetPrefab
//
public class DboyTargetPrefabObject : NetworkObject
{
    public const uint ObjectAssetId = 858699872;

    public override uint AssetId { get; } = ObjectAssetId;
    public ShootingTargetComponent ShootingTarget { get; }

    public DboyTargetPrefabObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        ShootingTarget = new ShootingTargetComponent(this);
        Behaviours[0] = ShootingTarget;
    }
}
