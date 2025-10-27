
namespace XProxy.API.Networking.Objects;

//
// Name: PrimitiveObjectToy
// NetworkID: 0
// AssetID: 1321952889
// SceneID: 0
// Path: PrimitiveObjectToy
//
public class PrimitiveObjectToyObject : NetworkObject
{
    public const uint ObjectAssetId = 1321952889;

    public override uint AssetId { get; } = ObjectAssetId;
    public PrimitiveObjectToyComponent PrimitiveObjectToy { get; }

    public PrimitiveObjectToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        PrimitiveObjectToy = new PrimitiveObjectToyComponent(this);
        Behaviours[0] = PrimitiveObjectToy;
    }
}
