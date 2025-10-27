using XProxy.API.Commands;
using XProxy.API.Plugins;

namespace XProxy.API;

public class ProxyAPI
{
    public static void Initialize(IServiceCollection collection)
    {
        CommandsManager.Initialize();

        PluginsManager.Initialize(collection);
    }
}
