
namespace XProxy.API.Networking.Objects;

//
// Name: AntiScp207PedestalStructure Variant
// NetworkID: 0
// AssetID: 2399831573
// SceneID: 0
// Path: AntiScp207PedestalStructure Variant
//
public class AntiScp207PedestalStructureVariantObject : NetworkObject
{
    public const uint ObjectAssetId = 2399831573;

    public override uint AssetId { get; } = ObjectAssetId;
    public PedestalScpLockerComponent PedestalScpLocker { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public AntiScp207PedestalStructureVariantObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        PedestalScpLocker = new PedestalScpLockerComponent(this);
        Behaviours[0] = PedestalScpLocker;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
