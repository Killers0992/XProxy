
namespace XProxy.API.Networking.Objects;

//
// Name: ElevatorChamber Gates
// NetworkID: 0
// AssetID: 1757973841
// SceneID: 0
// Path: ElevatorChamber Gates
//
public class ElevatorChamberGatesObject : NetworkObject
{
    public const uint ObjectAssetId = 1757973841;

    public override uint AssetId { get; } = ObjectAssetId;
    public ElevatorChamberComponent ElevatorChamber { get; }

    public ElevatorChamberGatesObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        ElevatorChamber = new ElevatorChamberComponent(this);
        Behaviours[0] = ElevatorChamber;
    }
}
