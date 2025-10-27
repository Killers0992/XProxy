
namespace XProxy.API.Networking.Objects;

//
// Name: Last Human Tracker
// NetworkID: 435
// AssetID: 180257209
// SceneID: 3656837584072672767
// Path: GameManager/Last Human Tracker
//
public class LastHumanTrackerObject : NetworkObject
{
    public const uint ObjectAssetId = 180257209;
    public const ulong ObjectSceneId = 3656837584072672767;

    public override uint NetworkId { get; set; } = 435;
    public override uint AssetId { get; } = ObjectAssetId;
    public override ulong SceneId { get; } = ObjectSceneId;

    public LastHumanTrackerObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[0];
    }
}
