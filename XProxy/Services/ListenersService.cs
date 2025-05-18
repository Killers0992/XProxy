using XProxy.Core;

namespace XProxy.Services;

public class ListenersService : BackgroundService
{
    public List<Listener> Listeners = new List<Listener>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Listeners.Add(new Listener("127.0.0.1", 7777, stoppingToken));

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
