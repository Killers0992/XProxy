using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace XProxy.Services
{
    public class ClientsUpdaterService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach(var client in ProxyService.Singleton.Players.Values)
                {
                    client.InternalUpdate();
                }
                await Task.Delay(1000);
            }
        }
    }
}
