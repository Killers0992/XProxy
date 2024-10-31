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
                foreach(var listener in Listener.NamesByListener.Values)
                {
                    foreach(var player in listener.Players.Values)
                    {
                        player?.InternalUpdate();
                    }
                }

                await Task.Delay(1000);
            }
        }
    }
}
