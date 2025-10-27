
namespace XProxy.API.Networking.Objects;

//
// Name: SpeakerToy
// NetworkID: 0
// AssetID: 712426663
// SceneID: 0
// Path: SpeakerToy
//
public class SpeakerToyObject : NetworkObject
{
    public const uint ObjectAssetId = 712426663;

    public override uint AssetId { get; } = ObjectAssetId;
    public SpeakerToyComponent SpeakerToy { get; }

    public SpeakerToyObject(World world, Client owner = null, uint networkId = 0) : base(world, owner, networkId)
    {
        //
        Behaviours = new BehaviourComponent[1];

        SpeakerToy = new SpeakerToyComponent(this);
        Behaviours[0] = SpeakerToy;
    }
}
