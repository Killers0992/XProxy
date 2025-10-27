
namespace XProxy.API.Networking.Objects;

//
// Name: CapybaraToy
// NetworkID: 0
// AssetID: 3087007600
// SceneID: 0
// Path: CapybaraToy
//
public class CapybaraToyObject : NetworkObject
{
    public const uint ObjectAssetId = 3087007600;

    public override uint AssetId { get; } = ObjectAssetId;
    public CapybaraToyComponent CapybaraToy { get; }

    public CapybaraToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        CapybaraToy = new CapybaraToyComponent(this);
        Behaviours[0] = CapybaraToy;
    }
}
