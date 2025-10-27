
namespace XProxy.API.Networking.Objects;

//
// Name: JailbirdPickup Halloween
// NetworkID: 0
// AssetID: 3262457219
// SceneID: 0
// Path: JailbirdPickup Halloween
//
public class JailbirdPickupHalloweenObject : NetworkObject
{
    public const uint ObjectAssetId = 3262457219;

    public override uint AssetId { get; } = ObjectAssetId;
    public JailbirdPickupComponent JailbirdPickup { get; }

    public JailbirdPickupHalloweenObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        JailbirdPickup = new JailbirdPickupComponent(this);
        Behaviours[0] = JailbirdPickup;
    }
}
