
namespace XProxy.API.Networking.Objects;

//
// Name: Scp1509Pickup
// NetworkID: 0
// AssetID: 1145481038
// SceneID: 0
// Path: Scp1509Pickup
//
public class Scp1509PickupObject : NetworkObject
{
    public const uint ObjectAssetId = 1145481038;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp1509PickupComponent Scp1509Pickup { get; }

    public Scp1509PickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp1509Pickup = new Scp1509PickupComponent(this);
        Behaviours[0] = Scp1509Pickup;
    }
}
