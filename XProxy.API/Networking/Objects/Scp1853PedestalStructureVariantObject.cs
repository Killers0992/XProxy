
namespace XProxy.API.Networking.Objects;

//
// Name: Scp1853PedestalStructure Variant
// NetworkID: 0
// AssetID: 3962534659
// SceneID: 0
// Path: Scp1853PedestalStructure Variant
//
public class Scp1853PedestalStructureVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 3962534659;

    public override uint AssetId { get; } = ObjectAssetId;
    public PedestalScpLockerComponent PedestalScpLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public Scp1853PedestalStructureVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PedestalScpLocker = new PedestalScpLockerComponent(this);
        Behaviours[0] = PedestalScpLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
