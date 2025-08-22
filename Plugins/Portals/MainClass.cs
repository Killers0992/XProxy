using Microsoft.Extensions.DependencyInjection;
using XProxy.Core;

namespace Portals;

public class MainClass : Plugin<Config>
{
    public override string Name { get; } = "Portals";

    public override string Description { get; } = "Adds ability to spawn portals which redirect player to servers";

    public override string Author { get; } = "Killers0992";

    public override Version Version { get; } = new Version(1, 0, 0);

    public override void OnLoad(IServiceCollection collection)
    {
        
    }
}
