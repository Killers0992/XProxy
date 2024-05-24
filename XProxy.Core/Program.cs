using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XProxy.Services;

[assembly: AssemblyVersion("1.0.5")]

namespace XProxy
{
    class Program
    {
        static async Task Main(string[] args) => await RunApplication(BuildApplication());
        
        static HostApplicationBuilder BuildApplication()
        {
            Logger.Ansi = new AnsiVtConsole.NetCore.AnsiVtConsole();

            var builder = Host.CreateApplicationBuilder();

            builder.Logging.SetMinimumLevel(LogLevel.None);

            SetupServices(builder.Services);

            return builder;
        }

        static void SetupServices(IServiceCollection services)
        {
            services.AddSingleton<ConfigService>(); 
            services.AddHostedService<ProxyService>();
            services.AddHostedService<PublicKeyService>();
            services.AddHostedService<ListService>();
            services.AddHostedService<ClientsUpdaterService>();
            services.AddSingleton<PluginsService>();
            services.AddHostedService<CommandsService>();
        }

        static async Task RunApplication(HostApplicationBuilder app)
        {
            IHost host = app.Build();

            await host.RunAsync();
        }
    }
}