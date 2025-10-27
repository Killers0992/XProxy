
namespace XProxy.API.Networking.Objects;

//
// Name: Scp018PedestalStructure Variant
// NetworkID: 0
// AssetID: 2286635216
// SceneID: 0
// Path: Scp018PedestalStructure Variant
//
public class Scp018PedestalStructureVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 2286635216;

    public override uint AssetId { get; } = ObjectAssetId;
    public PedestalScpLockerComponent PedestalScpLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public Scp018PedestalStructureVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PedestalScpLocker = new PedestalScpLockerComponent(this);
        Behaviours[0] = PedestalScpLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
