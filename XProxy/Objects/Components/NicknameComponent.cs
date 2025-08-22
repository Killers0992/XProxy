
namespace XProxy.Objects.Components;

public class NicknameComponent : BehaviourInfo
{
    string _nickname;

    public string Nickname
    {
        get => _nickname;
        set
        {
            // If name is not null then set displayname instead real name.
            if (_nickname != null)
                SetSyncVarDirtyBit(16);
            else
                SetSyncVarDirtyBit(8);

            _nickname = value;
        }
    }

    public NicknameComponent(SpawnableObject owner) : base(owner)
    {
        this.OnSerializeSyncVars += SerializeSyncVars;
    }

    public override void OnReceiveCommand(ushort functionHash, ArraySegment<byte> payload = default)
    {
        switch (functionHash)
        {
            case NetworkingMessages.NicknameSync.Commands.SetNick:
                NetworkReader reader = new NetworkReader(payload);
                string nickname = reader.ReadString();

                Nickname = nickname;
                Owner.SendUpdate(Owner.Owner);
                break;
        }
    }

    void SerializeSyncVars(NetworkWriter writer, bool intial)
    {
        if ((SyncVarDirtyBits & 8) != 0)
        {
            writer.WriteString(_nickname);
        }

        if ((SyncVarDirtyBits & 16) != 0)
        {
            writer.WriteString(_nickname);
        }
    }
}
