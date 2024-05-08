using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace XProxy.Services
{
    public class ClientsUpdaterService : BackgroundService
    {
        ConfigService _config;

        public ClientsUpdaterService(ConfigService config)
        {
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                foreach(var client in ProxyService.Singleton.Players.Values)
                {
                    client.InternalUpdate();
                }
                await Task.Delay(10);
            }
        }
    }
}
