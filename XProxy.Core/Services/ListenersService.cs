using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Core;

namespace XProxy.Services
{
    public class ListenersService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach(var listener in ConfigService.Singleton.Value.Listeners)
            {
                new Listener(listener.Key, stoppingToken);
            }

            await RunServerUpdater(stoppingToken);
        }

        private async Task RunServerUpdater(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (Server.UpdateServers)
                {
                    try
                    {
                        Server.Refresh(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                    Server.UpdateServers = false;
                }
                await Task.Delay(1000, token);
            }
        }
    }
}
