
namespace XProxy.API.Networking.Objects;

//
// Name: StartRound
// NetworkID: 428
// AssetID: 2276867293
// SceneID: 3656837585546479635
// Path: Player Canvas/StartRound
//
public class StartRoundObject : NetworkObject
{
    public const uint ObjectAssetId = 2276867293;
    public const ulong ObjectSceneId = 3656837585546479635;

    public override uint NetworkId { get; set; } = 428;
    public override uint AssetId { get; } = ObjectAssetId;
    public override ulong SceneId { get; } = ObjectSceneId;
    public RoundStartComponent RoundStart { get; }

    public StartRoundObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        RoundStart = new RoundStartComponent(this);
        Behaviours[0] = RoundStart;
    }
}
