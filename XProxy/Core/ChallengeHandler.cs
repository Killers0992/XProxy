namespace XProxy.Core;

public class ChallengeHandler
{
    public int ClientChallengeId;
    public ushort ClientChallengeSecretLen;
    public byte[] ClientChallenge, ClientChallengeBase, ClientChallengeResponse;

    public Connection Parent;

    public ChallengeHandler(Connection parent)
    {
        Parent = parent;
    }

    public void ProcessChallenge(NetPacketReader reader)
    {
        if (!reader.TryGetByte(out byte mode) || !reader.TryGetInt(out ClientChallengeId))
            return;

        ChallengeType challengeType = (ChallengeType)mode;

        switch (challengeType)
        {
            case ChallengeType.Reply:
                if (reader.TryGetBytesWithLength(out ClientChallengeResponse))
                {
                    Parent.Reconnect(Parent.Client.PreAuth.Create(false, ClientChallengeId, ClientChallengeResponse));
                }
                break;

            case ChallengeType.MD5:
            case ChallengeType.SHA1:
                if (reader.TryGetBytesWithLength(out ClientChallengeBase) &&
                    reader.TryGetUShort(out ClientChallengeSecretLen) &&
                    reader.TryGetBytesWithLength(out ClientChallenge))
                {
                    //Logger.Info($"Received challenge {challengeType} which is not supported");
                }
                break;
        }
    }
}
