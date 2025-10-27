
namespace XProxy.API.Networking.Objects;

//
// Name: LczCameraToy
// NetworkID: 0
// AssetID: 2026969629
// SceneID: 0
// Path: LczCameraToy
//
public class LczCameraToyObject : NetworkObject
{
    public const uint ObjectAssetId = 2026969629;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp079CameraToyComponent Scp079CameraToy { get; }

    public LczCameraToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp079CameraToy = new Scp079CameraToyComponent(this);
        Behaviours[0] = Scp079CameraToy;
    }
}
