
namespace XProxy.API.Networking.Objects;

//
// Name: FirearmPickup
// NetworkID: 0
// AssetID: 1925130715
// SceneID: 0
// Path: FirearmPickup
//
public class FirearmPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 1925130715;

    public override uint AssetId { get; } = ObjectAssetId;
    public FirearmPickupComponent FirearmPickup { get; }

    public FirearmPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        FirearmPickup = new FirearmPickupComponent(this);
        Behaviours[0] = FirearmPickup;
    }
}
