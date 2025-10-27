
namespace XProxy.API.Networking.Objects;

//
// Name: sportTargetPrefab
// NetworkID: 0
// AssetID: 1704345398
// SceneID: 0
// Path: sportTargetPrefab
//
public class SportTargetPrefabObject : NetworkObject
{
    public const uint ObjectAssetId = 1704345398;

    public override uint AssetId { get; } = ObjectAssetId;
    public ShootingTargetComponent ShootingTarget { get; }

    public SportTargetPrefabObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        ShootingTarget = new ShootingTargetComponent(this);
        Behaviours[0] = ShootingTarget;
    }
}
