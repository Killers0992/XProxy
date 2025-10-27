
namespace XProxy.API.Networking.Objects;

//
// Name: SCP244BPickup Variant
// NetworkID: 0
// AssetID: 3030062014
// SceneID: 0
// Path: SCP244BPickup Variant
//
public class SCP244BPickupVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 3030062014;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp244DeployablePickupComponent Scp244DeployablePickup { get; }

    public SCP244BPickupVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp244DeployablePickup = new Scp244DeployablePickupComponent(this);
        Behaviours[0] = Scp244DeployablePickup;
    }
}
