
namespace XProxy.API.Networking.Objects;

//
// Name: Scp500PedestalStructure Variant
// NetworkID: 0
// AssetID: 373821065
// SceneID: 0
// Path: Scp500PedestalStructure Variant
//
public class Scp500PedestalStructureVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 373821065;

    public override uint AssetId { get; } = ObjectAssetId;
    public PedestalScpLockerComponent PedestalScpLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public Scp500PedestalStructureVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PedestalScpLocker = new PedestalScpLockerComponent(this);
        Behaviours[0] = PedestalScpLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
