
namespace XProxy.API.Networking.Objects;

//
// Name: TantrumObj
// NetworkID: 0
// AssetID: 1306864341
// SceneID: 0
// Path: TantrumObj
//
public class TantrumObjObject : NetworkObject
{
    public const uint ObjectAssetId = 1306864341;

    public override uint AssetId { get; } = ObjectAssetId;

    public TantrumObjObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[0];
    }
}
