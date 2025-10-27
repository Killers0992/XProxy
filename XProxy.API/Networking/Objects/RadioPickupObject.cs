
namespace XProxy.API.Networking.Objects;

//
// Name: RadioPickup
// NetworkID: 0
// AssetID: 248357067
// SceneID: 0
// Path: RadioPickup
//
public class RadioPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 248357067;

    public override uint AssetId { get; } = ObjectAssetId;
    public RadioPickupComponent RadioPickup { get; }

    public RadioPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        RadioPickup = new RadioPickupComponent(this);
        Behaviours[0] = RadioPickup;
    }
}
