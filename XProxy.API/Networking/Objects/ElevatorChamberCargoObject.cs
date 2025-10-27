
namespace XProxy.API.Networking.Objects;

//
// Name: ElevatorChamberCargo
// NetworkID: 0
// AssetID: 1323017091
// SceneID: 0
// Path: ElevatorChamberCargo
//
public class ElevatorChamberCargoObject : NetworkObject
{
    public const uint ObjectAssetId = 1323017091;

    public override uint AssetId { get; } = ObjectAssetId;
    public ElevatorChamberComponent ElevatorChamber { get; }
    public ElevatorSquishComponent ElevatorSquish { get; }

    public ElevatorChamberCargoObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[2];

        ElevatorChamber = new ElevatorChamberComponent(this);
        Behaviours[0] = ElevatorChamber;

        ElevatorSquish = new ElevatorSquishComponent(this);
        Behaviours[1] = ElevatorSquish;
    }
}
