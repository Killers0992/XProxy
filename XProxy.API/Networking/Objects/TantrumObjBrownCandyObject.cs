
namespace XProxy.API.Networking.Objects;

//
// Name: TantrumObj (Brown Candy)
// NetworkID: 0
// AssetID: 2157375951
// SceneID: 0
// Path: TantrumObj (Brown Candy)
//
public class TantrumObjBrownCandyObject : NetworkObject
{
    public const uint ObjectAssetId = 2157375951;

    public override uint AssetId { get; } = ObjectAssetId;

    public TantrumObjBrownCandyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[0];
    }
}
