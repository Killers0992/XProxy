
namespace XProxy.API.Networking.Objects;

//
// Name: Scp2176PedestalStructure Variant
// NetworkID: 0
// AssetID: 3578915554
// SceneID: 0
// Path: Scp2176PedestalStructure Variant
//
public class Scp2176PedestalStructureVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 3578915554;

    public override uint AssetId { get; } = ObjectAssetId;
    public PedestalScpLockerComponent PedestalScpLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public Scp2176PedestalStructureVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PedestalScpLocker = new PedestalScpLockerComponent(this);
        Behaviours[0] = PedestalScpLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
