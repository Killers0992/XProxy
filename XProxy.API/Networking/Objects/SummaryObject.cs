
namespace XProxy.API.Networking.Objects;

//
// Name: Summary
// NetworkID: 431
// AssetID: 2276867293
// SceneID: 3656837585203691429
// Path: Player Canvas/Summary
//
public class SummaryObject : NetworkObject
{
    public const uint ObjectAssetId = 2276867293;
    public const ulong ObjectSceneId = 3656837585203691429;

    public override uint NetworkId { get; set; } = 431;
    public override uint AssetId { get; } = ObjectAssetId;
    public override ulong SceneId { get; } = ObjectSceneId;
    public RoundSummaryComponent RoundSummary { get; }

    public SummaryObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        RoundSummary = new RoundSummaryComponent(this);
        Behaviours[0] = RoundSummary;
    }
}
