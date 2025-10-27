
namespace XProxy.API.Networking.Objects;

//
// Name: Scp207PedestalStructure Variant
// NetworkID: 0
// AssetID: 664776131
// SceneID: 0
// Path: Scp207PedestalStructure Variant
//
public class Scp207PedestalStructureVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 664776131;

    public override uint AssetId { get; } = ObjectAssetId;
    public PedestalScpLockerComponent PedestalScpLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public Scp207PedestalStructureVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PedestalScpLocker = new PedestalScpLockerComponent(this);
        Behaviours[0] = PedestalScpLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
