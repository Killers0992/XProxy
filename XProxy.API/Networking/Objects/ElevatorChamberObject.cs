
namespace XProxy.API.Networking.Objects;

//
// Name: ElevatorChamber
// NetworkID: 0
// AssetID: 2588580243
// SceneID: 0
// Path: ElevatorChamber
//
public class ElevatorChamberObject : NetworkObject
{
    public const uint ObjectAssetId = 2588580243;

    public override uint AssetId { get; } = ObjectAssetId;
    public ElevatorChamberComponent ElevatorChamber { get; }

    public ElevatorChamberObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        ElevatorChamber = new ElevatorChamberComponent(this);
        Behaviours[0] = ElevatorChamber;
    }
}
