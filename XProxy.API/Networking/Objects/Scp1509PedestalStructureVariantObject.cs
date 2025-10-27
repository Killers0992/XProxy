
namespace XProxy.API.Networking.Objects;

//
// Name: Scp1509PedestalStructure Variant
// NetworkID: 0
// AssetID: 1712001893
// SceneID: 0
// Path: Scp1509PedestalStructure Variant
//
public class Scp1509PedestalStructureVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 1712001893;

    public override uint AssetId { get; } = ObjectAssetId;
    public PedestalScpLockerComponent PedestalScpLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public Scp1509PedestalStructureVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PedestalScpLocker = new PedestalScpLockerComponent(this);
        Behaviours[0] = PedestalScpLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
