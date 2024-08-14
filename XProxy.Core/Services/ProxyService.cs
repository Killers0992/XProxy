using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace XProxy.Services
{
    public class ProxyService : BackgroundService
    {
        public static ProxyServer Singleton { get; private set; }

        private ConfigService _config;

        private PluginsService _plugins;

        public ProxyService(ConfigService config)
        {
            _config = config;
            _plugins = new PluginsService();
            Singleton = new ProxyServer(_config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Singleton.Run();
        }
    }
}
