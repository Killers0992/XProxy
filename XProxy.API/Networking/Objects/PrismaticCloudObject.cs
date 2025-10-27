
namespace XProxy.API.Networking.Objects;

//
// Name: PrismaticCloud
// NetworkID: 0
// AssetID: 1891631329
// SceneID: 0
// Path: PrismaticCloud
//
public class PrismaticCloudObject : NetworkObject
{
    public const uint ObjectAssetId = 1891631329;

    public override uint AssetId { get; } = ObjectAssetId;

    public PrismaticCloudObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[0];
    }
}
