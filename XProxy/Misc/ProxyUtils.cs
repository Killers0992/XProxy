namespace XProxy.Misc;

public static class ProxyUtils
{
    public static Dictionary<ushort, Type> FindNetworkMessageTypes()
    {
        Dictionary<ushort, Type> messages = new Dictionary<ushort, Type>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();

            var messageTypes = types.Where(t => typeof(NetworkMessage).IsAssignableFrom(t));

            foreach (var messageType in messageTypes)
            {
                ushort hash = (ushort)messageType.FullName.GetStableHashCode();
                messages.Add(hash, messageType);
            }
        }

        return messages;
    }
}
