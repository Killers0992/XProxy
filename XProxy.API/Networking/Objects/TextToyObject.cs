
namespace XProxy.API.Networking.Objects;

//
// Name: TextToy
// NetworkID: 0
// AssetID: 162530276
// SceneID: 0
// Path: TextToy
//
public class TextToyObject : NetworkObject
{
    public const uint ObjectAssetId = 162530276;

    public override uint AssetId { get; } = ObjectAssetId;
    public TextToyComponent TextToy { get; }

    public TextToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        TextToy = new TextToyComponent(this);
        Behaviours[0] = TextToy;
    }
}
