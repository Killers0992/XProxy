
namespace XProxy.API.Networking.Objects;

//
// Name: KeycardPickup
// NetworkID: 0
// AssetID: 2672653014
// SceneID: 0
// Path: KeycardPickup
//
public class KeycardPickupObject : NetworkObject
{
    public const uint ObjectAssetId = 2672653014;

    public override uint AssetId { get; } = ObjectAssetId;
    public KeycardPickupComponent KeycardPickup { get; }

    public KeycardPickupObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        KeycardPickup = new KeycardPickupComponent(this);
        Behaviours[0] = KeycardPickup;
    }
}
