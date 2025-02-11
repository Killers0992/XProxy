using LiteNetLib;

namespace XProxy.Core.Core.Connections
{
    public class ConnectionValidator
    {
        public ConnectionHandler Parent;

        public ConnectionValidator(ConnectionHandler parent)
        {
            Parent = parent;
        }

        public int ClientChallengeId;

        public ushort ClientChallengeSecretLen;
        public byte[] ClientChallenge, ClientChallengeBase, ClientChallengeResponse;

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
                        Parent.Reconnect(Parent.Owner.PreAuth.CreateChallenge(ClientChallengeId, ClientChallengeResponse, Parent.Server.Settings.SendIpAddressInPreAuth));
                    }
                    break;

                case ChallengeType.MD5:
                case ChallengeType.SHA1:
                    if (reader.TryGetBytesWithLength(out ClientChallengeBase) &&
                        reader.TryGetUShort(out ClientChallengeSecretLen) &&
                        reader.TryGetBytesWithLength(out ClientChallenge))
                    {
                        Logger.Info($"Received challenge {challengeType} which is not supported");
                    }
                    break;
            }
        }
    }
}
