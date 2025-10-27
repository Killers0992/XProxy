
namespace XProxy.API.Networking.Objects;

//
// Name: Heavy Armor Pickup
// NetworkID: 0
// AssetID: 3164421243
// SceneID: 0
// Path: Heavy Armor Pickup
//
public class HeavyArmorPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 3164421243;

    public override uint AssetId { get; } = ObjectAssetId;
    public BodyArmorPickupComponent BodyArmorPickup { get; }

    public HeavyArmorPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        BodyArmorPickup = new BodyArmorPickupComponent(this);
        Behaviours[0] = BodyArmorPickup;
    }
}
