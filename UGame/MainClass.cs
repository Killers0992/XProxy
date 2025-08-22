using Microsoft.Extensions.DependencyInjection;
using XProxy.Core;
using XProxy.Misc;

namespace UGame;

public class MainClass : Plugin<Config>
{
    public override string Name { get; } = "UGame";

    public override string Description { get; } = "UGame shooter";

    public override string Author { get; } = "Killers0992";

    public override Version Version { get; } = new Version(1, 0, 0);

    public override void OnLoad(IServiceCollection collection)
    {
        Logger.Info("Initialize", "UGame");
    }
}
