
namespace XProxy.API.Networking.Objects;

//
// Name: FlashbangPickup
// NetworkID: 0
// AssetID: 3871663704
// SceneID: 0
// Path: FlashbangPickup
//
public class FlashbangPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 3871663704;

    public override uint AssetId { get; } = ObjectAssetId;
    public TimedGrenadePickupComponent TimedGrenadePickup { get; }

    public FlashbangPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        TimedGrenadePickup = new TimedGrenadePickupComponent(this);
        Behaviours[0] = TimedGrenadePickup;
    }
}
