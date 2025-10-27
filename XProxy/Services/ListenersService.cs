using XProxy.API.Models;
using XProxy.Servers;

namespace XProxy.Services;

public class ListenersService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach(ServerSettings server in ProxySettings.Singleton.Servers)
        {
            Server.Register(new RemoteServer(server.Name, server.Address, server.Port, false, server.ForwardIpAddress));
        }

        foreach(ListenerSettings settings in ProxySettings.Singleton.Listeners)
        {
            Listener.Register(new Listener(settings, stoppingToken));
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
