
namespace XProxy.API.Networking.Objects;

//
// Name: RifleRackStructure
// NetworkID: 0
// AssetID: 3352879624
// SceneID: 0
// Path: RifleRackStructure
//
public class RifleRackStructureObject : NetworkObject
{
    public const uint ObjectAssetId = 3352879624;

    public override uint AssetId { get; } = ObjectAssetId;
    public LockerComponent Locker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public RifleRackStructureObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        Locker = new LockerComponent(this);
        Behaviours[0] = Locker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
