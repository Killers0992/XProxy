
namespace XProxy.API.Networking.Objects;

//
// Name: KeycardPickup_Chaos
// NetworkID: 0
// AssetID: 2842703865
// SceneID: 0
// Path: KeycardPickup_Chaos
//
public class KeycardPickupChaosObject : NetworkObject
{
    public const uint ObjectAssetId = 2842703865;

    public override uint AssetId { get; } = ObjectAssetId;
    public KeycardPickupComponent KeycardPickup { get; }

    public KeycardPickupChaosObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        KeycardPickup = new KeycardPickupComponent(this);
        Behaviours[0] = KeycardPickup;
    }
}
