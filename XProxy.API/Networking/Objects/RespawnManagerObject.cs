
namespace XProxy.API.Networking.Objects;

//
// Name: RespawnManager
// NetworkID: 432
// AssetID: 180257209
// SceneID: 3656837585036799549
// Path: GameManager/RespawnManager
//
public class RespawnManagerObject : NetworkObject
{
    public const uint ObjectAssetId = 180257209;
    public const ulong ObjectSceneId = 3656837585036799549;

    public override uint NetworkId { get; set; } = 432;
    public override uint AssetId { get; } = ObjectAssetId;
    public override ulong SceneId { get; } = ObjectSceneId;

    public RespawnManagerObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[0];
    }
}
