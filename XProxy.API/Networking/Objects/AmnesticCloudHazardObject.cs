
namespace XProxy.API.Networking.Objects;

//
// Name: Amnestic Cloud Hazard
// NetworkID: 0
// AssetID: 825024811
// SceneID: 0
// Path: Amnestic Cloud Hazard
//
public class AmnesticCloudHazardObject : NetworkObject
{
    public const uint ObjectAssetId = 825024811;

    public override uint AssetId { get; } = ObjectAssetId;
    public Scp939AmnesticCloudInstanceComponent Scp939AmnesticCloudInstance { get; }

    public AmnesticCloudHazardObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        Scp939AmnesticCloudInstance = new Scp939AmnesticCloudInstanceComponent(this);
        Behaviours[0] = Scp939AmnesticCloudInstance;
    }
}
