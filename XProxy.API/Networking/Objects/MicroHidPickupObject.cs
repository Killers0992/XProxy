
namespace XProxy.API.Networking.Objects;

//
// Name: MicroHidPickup
// NetworkID: 0
// AssetID: 2974277164
// SceneID: 0
// Path: MicroHidPickup
//
public class MicroHidPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 2974277164;

    public override uint AssetId { get; } = ObjectAssetId;
    public MicroHIDPickupComponent MicroHIDPickup { get; }

    public MicroHidPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        MicroHIDPickup = new MicroHIDPickupComponent(this);
        Behaviours[0] = MicroHIDPickup;
    }
}
