using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XProxy.Services
{
    public class ListenersService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Task> taskListeners = new List<Task>();

            foreach(var listener in ConfigService.Singleton.Value.Listeners)
            {
                Listener server = new Listener(listener.Key);
                taskListeners.Add(server.Run(stoppingToken));
            }

            await Task.WhenAll(taskListeners);
        }
    }
}
