
namespace XProxy.API.Networking.Objects;

//
// Name: HegPickup
// NetworkID: 0
// AssetID: 1273232029
// SceneID: 0
// Path: HegPickup
//
public class HegPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 1273232029;

    public override uint AssetId { get; } = ObjectAssetId;
    public TimedGrenadePickupComponent TimedGrenadePickup { get; }

    public HegPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        TimedGrenadePickup = new TimedGrenadePickupComponent(this);
        Behaviours[0] = TimedGrenadePickup;
    }
}
