
namespace XProxy.API.Networking.Objects;

//
// Name: LargeGunLockerStructure
// NetworkID: 0
// AssetID: 2830750618
// SceneID: 0
// Path: LargeGunLockerStructure
//
public class LargeGunLockerStructureObject : NetworkObject
{
    public const uint ObjectAssetId = 2830750618;

    public override uint AssetId { get; } = ObjectAssetId;
    public LockerComponent Locker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public LargeGunLockerStructureObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        Locker = new LockerComponent(this);
        Behaviours[0] = Locker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
