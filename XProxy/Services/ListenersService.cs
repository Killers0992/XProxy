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
            foreach (Server server in Server.RegisteredServers.Values)
            {
                try
                {
                    server.OnUpdate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            
            await Task.Delay(10, token);
        }

    }
}
