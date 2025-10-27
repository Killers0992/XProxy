
namespace XProxy.API.Networking.Objects;

//
// Name: DecontaminationManager
// NetworkID: 433
// AssetID: 180257209
// SceneID: 3656837584228160779
// Path: GameManager/Announcement Sources/DecontaminationManager
//
public class DecontaminationManagerObject : NetworkObject
{
    public const uint ObjectAssetId = 180257209;
    public const ulong ObjectSceneId = 3656837584228160779;

    public override uint NetworkId { get; set; } = 433;
    public override uint AssetId { get; } = ObjectAssetId;
    public override ulong SceneId { get; } = ObjectSceneId;
    public DecontaminationControllerComponent DecontaminationController { get; }

    public DecontaminationManagerObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        DecontaminationController = new DecontaminationControllerComponent(this);
        Behaviours[0] = DecontaminationController;
    }
}
