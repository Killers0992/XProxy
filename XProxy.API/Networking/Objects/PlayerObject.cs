
namespace XProxy.API.Networking.Objects;

//
// Name: Player
// NetworkID: 0
// AssetID: 3816198336
// SceneID: 0
// Path: Player
//
public class PlayerObject : NetworkObject
{
    public const uint ObjectAssetId = 3816198336;

    public override uint AssetId { get; } = ObjectAssetId;
    public ServerRolesComponent ServerRoles { get; }
    public CharacterClassManagerComponent CharacterClassManager { get; }
    public PlayerEffectsControllerComponent PlayerEffectsController { get; }
    public NicknameSyncComponent NicknameSync { get; }
    public HintDisplayComponent HintDisplay { get; }
    public AmbientSoundPlayerComponent AmbientSoundPlayer { get; }
    public BloodDrawerComponent BloodDrawer { get; }
    public PlayerAuthenticationManagerComponent PlayerAuthenticationManager { get; }
    public EncryptedChannelManagerComponent EncryptedChannelManager { get; }
    public QueryProcessorComponent QueryProcessor { get; }
    public BroadcastComponent Broadcast { get; }
    public GameConsoleTransmissionComponent GameConsoleTransmission { get; }
    public CheaterReportComponent CheaterReport { get; }
    public VersionCheckComponent VersionCheck { get; }
    public ServerTimeComponent ServerTime { get; }
    public PlayerRateLimitHandlerComponent PlayerRateLimitHandler { get; }
    public ReferenceHubComponent ReferenceHub { get; }
    public InteractionCoordinatorComponent InteractionCoordinator { get; }
    public FastRoundRestartControllerComponent FastRoundRestartController { get; }
    public InventoryComponent Inventory { get; }
    public SearchCoordinatorComponent SearchCoordinator { get; }
    public PlayerIpOverrideComponent PlayerIpOverride { get; }
    public PlayerStatsComponent PlayerStats { get; }
    public PlayerRoleManagerComponent PlayerRoleManager { get; }

    public PlayerObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[24];

        ServerRoles = new ServerRolesComponent(this);
        Behaviours[0] = ServerRoles;

        CharacterClassManager = new CharacterClassManagerComponent(this);
        Behaviours[1] = CharacterClassManager;

        PlayerEffectsController = new PlayerEffectsControllerComponent(this);
        Behaviours[2] = PlayerEffectsController;

        NicknameSync = new NicknameSyncComponent(this);
        Behaviours[3] = NicknameSync;

        HintDisplay = new HintDisplayComponent(this);
        Behaviours[4] = HintDisplay;

        AmbientSoundPlayer = new AmbientSoundPlayerComponent(this);
        Behaviours[5] = AmbientSoundPlayer;

        BloodDrawer = new BloodDrawerComponent(this);
        Behaviours[6] = BloodDrawer;

        PlayerAuthenticationManager = new PlayerAuthenticationManagerComponent(this);
        Behaviours[7] = PlayerAuthenticationManager;

        EncryptedChannelManager = new EncryptedChannelManagerComponent(this);
        Behaviours[8] = EncryptedChannelManager;

        QueryProcessor = new QueryProcessorComponent(this);
        Behaviours[9] = QueryProcessor;

        Broadcast = new BroadcastComponent(this);
        Behaviours[10] = Broadcast;

        GameConsoleTransmission = new GameConsoleTransmissionComponent(this);
        Behaviours[11] = GameConsoleTransmission;

        CheaterReport = new CheaterReportComponent(this);
        Behaviours[12] = CheaterReport;

        VersionCheck = new VersionCheckComponent(this);
        Behaviours[13] = VersionCheck;

        ServerTime = new ServerTimeComponent(this);
        Behaviours[14] = ServerTime;

        PlayerRateLimitHandler = new PlayerRateLimitHandlerComponent(this);
        Behaviours[15] = PlayerRateLimitHandler;

        ReferenceHub = new ReferenceHubComponent(this);
        Behaviours[16] = ReferenceHub;

        InteractionCoordinator = new InteractionCoordinatorComponent(this);
        Behaviours[17] = InteractionCoordinator;

        FastRoundRestartController = new FastRoundRestartControllerComponent(this);
        Behaviours[18] = FastRoundRestartController;

        Inventory = new InventoryComponent(this);
        Behaviours[19] = Inventory;

        SearchCoordinator = new SearchCoordinatorComponent(this);
        Behaviours[20] = SearchCoordinator;

        PlayerIpOverride = new PlayerIpOverrideComponent(this);
        Behaviours[21] = PlayerIpOverride;

        PlayerStats = new PlayerStatsComponent(this);
        Behaviours[22] = PlayerStats;

        PlayerRoleManager = new PlayerRoleManagerComponent(this);
        Behaviours[23] = PlayerRoleManager;
    }
}
