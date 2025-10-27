
namespace XProxy.API.Networking.Objects;

//
// Name: SCP1576Pickup
// NetworkID: 0
// AssetID: 303271247
// SceneID: 0
// Path: SCP1576Pickup
//
public class SCP1576PickupObject : NetworkObject
{
    public const uint ObjectAssetId = 303271247;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp1576PickupComponent Scp1576Pickup { get; }

    public SCP1576PickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp1576Pickup = new Scp1576PickupComponent(this);
        Behaviours[0] = Scp1576Pickup;
    }
}
