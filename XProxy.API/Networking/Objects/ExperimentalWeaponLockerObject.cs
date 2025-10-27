
namespace XProxy.API.Networking.Objects;

//
// Name: Experimental Weapon Locker
// NetworkID: 0
// AssetID: 2372810204
// SceneID: 0
// Path: Experimental Weapon Locker
//
public class ExperimentalWeaponLockerObject : NetworkObject
{
    public const uint ObjectAssetId = 2372810204;

    public override uint AssetId { get; } = ObjectAssetId;
    public ExperimentalWeaponLockerComponent ExperimentalWeaponLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public ExperimentalWeaponLockerObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        ExperimentalWeaponLocker = new ExperimentalWeaponLockerComponent(this);
        Behaviours[0] = ExperimentalWeaponLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
