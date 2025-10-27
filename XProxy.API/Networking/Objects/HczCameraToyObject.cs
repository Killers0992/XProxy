
namespace XProxy.API.Networking.Objects;

//
// Name: HczCameraToy
// NetworkID: 0
// AssetID: 144958943
// SceneID: 0
// Path: HczCameraToy
//
public class HczCameraToyObject : NetworkObject
{
    public const uint ObjectAssetId = 144958943;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp079CameraToyComponent Scp079CameraToy { get; }

    public HczCameraToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp079CameraToy = new Scp079CameraToyComponent(this);
        Behaviours[0] = Scp079CameraToy;
    }
}
