
namespace XProxy.API.Networking.Objects;

//
// Name: EzArmCameraToy
// NetworkID: 0
// AssetID: 1824808402
// SceneID: 0
// Path: EzArmCameraToy
//
public class EzArmCameraToyObject : NetworkObject
{
    public const uint ObjectAssetId = 1824808402;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp079CameraToyComponent Scp079CameraToy { get; }

    public EzArmCameraToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp079CameraToy = new Scp079CameraToyComponent(this);
        Behaviours[0] = Scp079CameraToy;
    }
}
