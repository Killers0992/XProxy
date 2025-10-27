
namespace XProxy.API.Networking.Objects;

//
// Name: InvisibleInteractableToy
// NetworkID: 0
// AssetID: 359728307
// SceneID: 0
// Path: InvisibleInteractableToy
//
public class InvisibleInteractableToyObject : NetworkObject
{
    public const uint ObjectAssetId = 359728307;

    public override uint AssetId { get; } = ObjectAssetId;
    public InvisibleInteractableToyComponent InvisibleInteractableToy { get; }

    public InvisibleInteractableToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        InvisibleInteractableToy = new InvisibleInteractableToyComponent(this);
        Behaviours[0] = InvisibleInteractableToy;
    }
}
