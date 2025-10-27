
namespace XProxy.API.Networking.Objects;

//
// Name: Config Synchronizer
// NetworkID: 434
// AssetID: 180257209
// SceneID: 3656837585730004444
// Path: GameManager/Config Synchronizer
//
public class ConfigSynchronizerObject : NetworkObject
{
    public const uint ObjectAssetId = 180257209;
    public const ulong ObjectSceneId = 3656837585730004444;

    public override uint NetworkId { get; set; } = 434;
    public override uint AssetId { get; } = ObjectAssetId;
    public override ulong SceneId { get; } = ObjectSceneId;
    public ServerConfigSynchronizerComponent ServerConfigSynchronizer { get; }

    public ConfigSynchronizerObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        ServerConfigSynchronizer = new ServerConfigSynchronizerComponent(this);
        Behaviours[0] = ServerConfigSynchronizer;
    }
}
