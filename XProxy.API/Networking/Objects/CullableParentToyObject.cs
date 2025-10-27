
namespace XProxy.API.Networking.Objects;

//
// Name: CullableParentToy
// NetworkID: 0
// AssetID: 2332883846
// SceneID: 0
// Path: CullableParentToy
//
public class CullableParentToyObject : NetworkObject
{
    public const uint ObjectAssetId = 2332883846;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableCullingParentComponent SpawnableCullingParent { get; }

    public CullableParentToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpawnableCullingParent = new SpawnableCullingParentComponent(this);
        Behaviours[0] = SpawnableCullingParent;
    }
}
