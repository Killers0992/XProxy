
namespace XProxy.API.Networking.Objects;

//
// Name: Warhead Controller
// NetworkID: 430
// AssetID: 180257209
// SceneID: 3656837582862294845
// Path: GameManager/Announcement Sources/Warhead Controller
//
public class WarheadControllerObject : NetworkObject
{
    public const uint ObjectAssetId = 180257209;
    public const ulong ObjectSceneId = 3656837582862294845;

    public override uint NetworkId { get; set; } = 430;
    public override uint AssetId { get; } = ObjectAssetId;
    public override ulong SceneId { get; } = ObjectSceneId;
    public AlphaWarheadControllerComponent AlphaWarheadController { get; }

    public WarheadControllerObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        AlphaWarheadController = new AlphaWarheadControllerComponent(this);
        Behaviours[0] = AlphaWarheadController;
    }
}
