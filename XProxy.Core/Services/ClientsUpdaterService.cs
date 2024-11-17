using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Core;

namespace XProxy.Services
{
    public class ClientsUpdaterService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach(Player plr in Player.List)
                {
                    plr?.InternalUpdate();
                }

                await Task.Delay(1000);
            }
        }
    }
}
