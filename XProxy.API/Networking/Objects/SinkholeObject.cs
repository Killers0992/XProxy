
namespace XProxy.API.Networking.Objects;

//
// Name: Sinkhole
// NetworkID: 0
// AssetID: 3539746802
// SceneID: 0
// Path: Sinkhole
//
public class SinkholeObject : NetworkObject
{
    public const uint ObjectAssetId = 3539746802;

    public override uint AssetId { get; } = ObjectAssetId;
    public SinkholeEnvironmentalHazardComponent SinkholeEnvironmentalHazard { get; }

    public SinkholeObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SinkholeEnvironmentalHazard = new SinkholeEnvironmentalHazardComponent(this);
        Behaviours[0] = SinkholeEnvironmentalHazard;
    }
}
