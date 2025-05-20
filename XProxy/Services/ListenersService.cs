using XProxy.Core;
using XProxy.Misc;

namespace XProxy.Services;

public class ListenersService : BackgroundService
{
    public List<Listener> Listeners = new List<Listener>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        NetDebug.Logger = new CustomNetLogger();

        Listeners.Add(new Listener("0.0.0.0", 7785, stoppingToken));

        await RunServerUpdater(stoppingToken);
    }

    private async Task RunServerUpdater(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1000, token);
        }

    }
}
