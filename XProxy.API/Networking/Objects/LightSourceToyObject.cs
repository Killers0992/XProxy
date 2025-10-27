
namespace XProxy.API.Networking.Objects;

//
// Name: LightSourceToy
// NetworkID: 0
// AssetID: 3956448839
// SceneID: 0
// Path: LightSourceToy
//
public class LightSourceToyObject : NetworkObject
{
    public const uint ObjectAssetId = 3956448839;

    public override uint AssetId { get; } = ObjectAssetId;
    public LightSourceToyComponent LightSourceToy { get; }

    public LightSourceToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        LightSourceToy = new LightSourceToyComponent(this);
        Behaviours[0] = LightSourceToy;
    }
}
