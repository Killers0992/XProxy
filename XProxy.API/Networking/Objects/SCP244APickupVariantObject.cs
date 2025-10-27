
namespace XProxy.API.Networking.Objects;

//
// Name: SCP244APickup Variant
// NetworkID: 0
// AssetID: 2088018000
// SceneID: 0
// Path: SCP244APickup Variant
//
public class SCP244APickupVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 2088018000;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp244DeployablePickupComponent Scp244DeployablePickup { get; }

    public SCP244APickupVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp244DeployablePickup = new Scp244DeployablePickupComponent(this);
        Behaviours[0] = Scp244DeployablePickup;
    }
}
