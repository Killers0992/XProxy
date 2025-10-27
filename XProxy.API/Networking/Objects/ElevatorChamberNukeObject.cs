
namespace XProxy.API.Networking.Objects;

//
// Name: ElevatorChamberNuke
// NetworkID: 0
// AssetID: 912031041
// SceneID: 0
// Path: ElevatorChamberNuke
//
public class ElevatorChamberNukeObject : NetworkObject
{
    public const uint ObjectAssetId = 912031041;

    public override uint AssetId { get; } = ObjectAssetId;
    public ElevatorChamberComponent ElevatorChamber { get; }
    public ElevatorSquishComponent ElevatorSquish { get; }

    public ElevatorChamberNukeObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        ElevatorChamber = new ElevatorChamberComponent(this);
        Behaviours[0] = ElevatorChamber;

        ElevatorSquish = new ElevatorSquishComponent(this);
        Behaviours[1] = ElevatorSquish;
    }
}
