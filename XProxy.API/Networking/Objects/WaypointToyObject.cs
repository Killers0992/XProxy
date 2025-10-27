
namespace XProxy.API.Networking.Objects;

//
// Name: WaypointToy
// NetworkID: 0
// AssetID: 3938583646
// SceneID: 0
// Path: WaypointToy
//
public class WaypointToyObject : NetworkObject
{
    public const uint ObjectAssetId = 3938583646;

    public override uint AssetId { get; } = ObjectAssetId;
    public WaypointToyComponent WaypointToy { get; }

    public WaypointToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        WaypointToy = new WaypointToyComponent(this);
        Behaviours[0] = WaypointToy;
    }
}
