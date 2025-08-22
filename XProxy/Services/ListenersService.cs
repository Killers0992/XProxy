using XProxy.Servers;

namespace XProxy.Services;

public class ListenersService : BackgroundService
{
    public static List<Listener> Listeners = new List<Listener>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach(ServerSettings server in Settings.Singleton.Servers)
        {
            Server.Register(new RemoteServer(server.Name, server.Address, server.Port, false, server.ForwardIpAddress));
        }

        foreach(ListenerSettings listener in Settings.Singleton.Listeners)
        {
            Listeners.Add(new Listener(listener, stoppingToken));
        }

        await RunServerUpdater(stoppingToken);
    }

    private async Task RunServerUpdater(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            foreach (var server in Server.RegisteredServers.Values)
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
