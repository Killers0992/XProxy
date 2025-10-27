
namespace XProxy.API.Networking.Objects;

//
// Name: AdrenalineMedkitStructure
// NetworkID: 0
// AssetID: 2525847434
// SceneID: 0
// Path: AdrenalineMedkitStructure
//
public class AdrenalineMedkitStructureObject : NetworkObject
{
    public const uint ObjectAssetId = 2525847434;

    public override uint AssetId { get; } = ObjectAssetId;
    public LockerComponent Locker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public AdrenalineMedkitStructureObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        Locker = new LockerComponent(this);
        Behaviours[0] = Locker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
