namespace XProxy.Misc;

public static class ProxyUtils
{
    public static Dictionary<ushort, Type> FindNetworkMessageTypes()
    {
        Dictionary<ushort, Type> messages = new Dictionary<ushort, Type>();

        Get(typeof(ServerConsole), ref messages);
        Get(typeof(Mirror.Batcher), ref messages);

        return messages;
    }

    static void Get(Type type, ref Dictionary<ushort, Type> messages)
    {
        var types = type.Assembly.GetTypes();

        var messageTypes = types.Where(t => typeof(NetworkMessage).IsAssignableFrom(t));

        foreach (var messageType in messageTypes)
        {
            ushort hash = (ushort)messageType.FullName.GetStableHashCode();
            messages.Add(hash, messageType);
        }
    }
}
