
namespace XProxy.API.Networking.Objects;

//
// Name: MiscLocker
// NetworkID: 0
// AssetID: 1964083310
// SceneID: 0
// Path: MiscLocker
//
public class MiscLockerObject : NetworkObject
{
    public const uint ObjectAssetId = 1964083310;

    public override uint AssetId { get; } = ObjectAssetId;
    public LockerComponent Locker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public MiscLockerObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        Locker = new LockerComponent(this);
        Behaviours[0] = Locker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
