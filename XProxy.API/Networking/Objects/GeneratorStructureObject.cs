
namespace XProxy.API.Networking.Objects;

//
// Name: GeneratorStructure
// NetworkID: 0
// AssetID: 2724603877
// SceneID: 0
// Path: GeneratorStructure
//
public class GeneratorStructureObject : NetworkObject
{
    public const uint ObjectAssetId = 2724603877;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp079GeneratorComponent Scp079Generator { get; }
    public StructurePositionSyncComponent StructurePositionSync { get; }

    public GeneratorStructureObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        Scp079Generator = new Scp079GeneratorComponent(this);
        Behaviours[0] = Scp079Generator;

        StructurePositionSync = new StructurePositionSyncComponent(this);
        Behaviours[1] = StructurePositionSync;
    }
}
