
namespace XProxy.API.Networking.Objects;

//
// Name: Combat Armor Pickup
// NetworkID: 0
// AssetID: 3118088094
// SceneID: 0
// Path: Combat Armor Pickup
//
public class CombatArmorPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 3118088094;

    public override uint AssetId { get; } = ObjectAssetId;
    public BodyArmorPickupComponent BodyArmorPickup { get; }

    public CombatArmorPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        BodyArmorPickup = new BodyArmorPickupComponent(this);
        Behaviours[0] = BodyArmorPickup;
    }
}
