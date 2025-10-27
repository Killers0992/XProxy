
namespace XProxy.API.Networking.Objects;

//
// Name: Tesla Gate Controller
// NetworkID: 429
// AssetID: 180257209
// SceneID: 3656837582856477417
// Path: GameManager/Tesla Gate Controller
//
public class TeslaGateControllerObject : NetworkObject
{
    public const uint ObjectAssetId = 180257209;
    public const ulong ObjectSceneId = 3656837582856477417;

    public override uint NetworkId { get; set; } = 429;
    public override uint AssetId { get; } = ObjectAssetId;
    public override ulong SceneId { get; } = ObjectSceneId;
    public TeslaGateControllerComponent TeslaGateController { get; }

    public TeslaGateControllerObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        TeslaGateController = new TeslaGateControllerComponent(this);
        Behaviours[0] = TeslaGateController;
    }
}
