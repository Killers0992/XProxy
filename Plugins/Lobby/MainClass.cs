using Microsoft.Extensions.DependencyInjection;
using XProxy.API.Core;
using XProxy.API.Plugins;

namespace Lobby;

public class MainClass : Plugin<Config>
{
    public static MainClass Singleton { get; private set; }

    public override string Name { get; } = "Lobby";

    public override string Description { get; } = "Adds lobby to proxy";

    public override string Author { get; } = "Killers0992";

    public override Version Version { get; } = new Version(1, 0, 0);

    public override void OnLoad(IServiceCollection collection)
    {
        Singleton = this;
        Server.Register(new LobbyServer());
    }
}
