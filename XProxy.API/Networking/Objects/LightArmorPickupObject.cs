
namespace XProxy.API.Networking.Objects;

//
// Name: Light Armor Pickup
// NetworkID: 0
// AssetID: 941440279
// SceneID: 0
// Path: Light Armor Pickup
//
public class LightArmorPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 941440279;

    public override uint AssetId { get; } = ObjectAssetId;
    public BodyArmorPickupComponent BodyArmorPickup { get; }

    public LightArmorPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        BodyArmorPickup = new BodyArmorPickupComponent(this);
        Behaviours[0] = BodyArmorPickup;
    }
}
