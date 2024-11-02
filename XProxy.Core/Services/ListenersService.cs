using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

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

            while (!stoppingToken.IsCancellationRequested)
            {
                // Run 
                await Task.Delay(1000);
            }
        }
    }
}
