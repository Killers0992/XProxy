using Mirror;

namespace XProxy.CustomMessages
{
    public struct EncryptionKeys : NetworkMessage
    {
        public EncryptionKeys(byte[] gameConsoleEncryptionKey)
        {
            GameConsoleEncryptionKey = gameConsoleEncryptionKey;
        }

        public byte[] GameConsoleEncryptionKey;
    }
}
