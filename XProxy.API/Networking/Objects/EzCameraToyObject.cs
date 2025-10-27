
namespace XProxy.API.Networking.Objects;

//
// Name: EzCameraToy
// NetworkID: 0
// AssetID: 3375932423
// SceneID: 0
// Path: EzCameraToy
//
public class EzCameraToyObject : NetworkObject
{
    public const uint ObjectAssetId = 3375932423;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp079CameraToyComponent Scp079CameraToy { get; }

    public EzCameraToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp079CameraToy = new Scp079CameraToyComponent(this);
        Behaviours[0] = Scp079CameraToy;
    }
}
