
namespace XProxy.API.Networking.Objects;

//
// Name: SzCameraToy
// NetworkID: 0
// AssetID: 1734743361
// SceneID: 0
// Path: SzCameraToy
//
public class SzCameraToyObject : NetworkObject
{
    public const uint ObjectAssetId = 1734743361;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp079CameraToyComponent Scp079CameraToy { get; }

    public SzCameraToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp079CameraToy = new Scp079CameraToyComponent(this);
        Behaviours[0] = Scp079CameraToy;
    }
}
