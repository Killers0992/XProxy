
namespace XProxy.API.Networking.Objects;

//
// Name: Scp244PedestalStructure Variant
// NetworkID: 0
// AssetID: 3724306703
// SceneID: 0
// Path: Scp244PedestalStructure Variant
//
public class Scp244PedestalStructureVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 3724306703;

    public override uint AssetId { get; } = ObjectAssetId;
    public PedestalScpLockerComponent PedestalScpLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public Scp244PedestalStructureVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PedestalScpLocker = new PedestalScpLockerComponent(this);
        Behaviours[0] = PedestalScpLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
