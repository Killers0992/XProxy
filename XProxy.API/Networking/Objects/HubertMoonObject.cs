
namespace XProxy.API.Networking.Objects;

//
// Name: Hubert Moon
// NetworkID: 0
// AssetID: 1359696107
// SceneID: 0
// Path: Hubert Moon
//
public class HubertMoonObject : NetworkObject
{
    public const uint ObjectAssetId = 1359696107;

    public override uint AssetId { get; } = ObjectAssetId;
    public HubertMoonComponent HubertMoon { get; }

    public HubertMoonObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        HubertMoon = new HubertMoonComponent(this);
        Behaviours[0] = HubertMoon;
    }
}
