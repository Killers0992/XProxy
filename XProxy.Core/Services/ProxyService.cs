using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace XProxy.Services
{
    public class ProxyService : BackgroundService
    {
        public static ProxyServer Singleton { get; private set; }

        public ProxyService()
        {
            Singleton = new ProxyServer();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Singleton.Run(stoppingToken);
        }
    }
}
