
namespace XProxy.API.Networking.Objects;

//
// Name: binaryTargetPrefab
// NetworkID: 0
// AssetID: 3613149668
// SceneID: 0
// Path: binaryTargetPrefab
//
public class BinaryTargetPrefabObject : NetworkObject
{
    public const uint ObjectAssetId = 3613149668;

    public override uint AssetId { get; } = ObjectAssetId;
    public ShootingTargetComponent ShootingTarget { get; }

    public BinaryTargetPrefabObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        ShootingTarget = new ShootingTargetComponent(this);
        Behaviours[0] = ShootingTarget;
    }
}
