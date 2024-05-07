using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace XProxy.Services
{
    public class ProxyService : BackgroundService
    {
        public static ProxyServer Singleton { get; private set; }

        private ConfigService _config;

        public ProxyService(ConfigService config)
        {
            _config = config;
            Singleton = new ProxyServer(_config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Singleton.Run();
        }
    }
}
