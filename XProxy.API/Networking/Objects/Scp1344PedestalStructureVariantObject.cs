
namespace XProxy.API.Networking.Objects;

//
// Name: Scp1344PedestalStructure Variant
// NetworkID: 0
// AssetID: 1763950070
// SceneID: 0
// Path: Scp1344PedestalStructure Variant
//
public class Scp1344PedestalStructureVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 1763950070;

    public override uint AssetId { get; } = ObjectAssetId;
    public PedestalScpLockerComponent PedestalScpLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public Scp1344PedestalStructureVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PedestalScpLocker = new PedestalScpLockerComponent(this);
        Behaviours[0] = PedestalScpLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
