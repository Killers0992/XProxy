
namespace XProxy.API.Networking.Objects;

//
// Name: Spawnable Work Station Structure
// NetworkID: 0
// AssetID: 1783091262
// SceneID: 0
// Path: Spawnable Work Station Structure
//
public class SpawnableWorkStationStructureObject : NetworkObject
{
    public const uint ObjectAssetId = 1783091262;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpawnableStructureComponent SpawnableStructure { get; }
    public WorkstationControllerComponent WorkstationController { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public SpawnableWorkStationStructureObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[3];

        SpawnableStructure = new SpawnableStructureComponent(this);
        Behaviours[0] = SpawnableStructure;

        WorkstationController = new WorkstationControllerComponent(this);
        Behaviours[1] = WorkstationController;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[2] = StructurePositionSync;
    }
}
