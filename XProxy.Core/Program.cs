using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XProxy.Core.Services;
using XProxy.Services;
using XProxy.Shared.Models;

[assembly: AssemblyVersion("1.5.0")]

namespace XProxy
{
    class Program
    {
        public class Options
        {
            [Option('g', "gameversion", Required = true)]
            public string GameVersion { get; set; }

            [Option('p', "path", Required = false)]
            public string Path { get; set; }

            [Option("ansidisable", Required = false)]
            public bool AnsiDisable { get; set; } = false;
        }

        static async Task Main(string[] args) => await RunApplication(BuildApplication(args));
        
        static HostApplicationBuilder BuildApplication(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                if (o.Path != null)
                    ConfigService.MainDirectory = o.Path.Trim();

                Logger.AnsiDisabled = o.AnsiDisable;

                if (o.GameVersion != null)
                    ConfigModel.GameVersion = o.GameVersion.Trim();
            });

#if DEBUG
            ConfigModel.GameVersion = "13.6.9";
#endif

            if (string.IsNullOrEmpty(ConfigModel.GameVersion))
            {
                Logger.Info("Game version provided in commandline is empty!");
                Logger.Info(" 1) Make sure you have latest XProxy or XProxy.exe from https://github.com/Killers0992/XProxy#setup!");
                Logger.Info("    - If you use Pterodactyl EGG just reinstall server!");
                Logger.Info("");
                Thread.Sleep(5000);
                return null;
            }

            if (ConfigService.MainDirectory == null)
                ConfigService.MainDirectory = Environment.CurrentDirectory;

            var builder = Host.CreateApplicationBuilder();

            builder.Logging.SetMinimumLevel(LogLevel.None);

            SetupServices(builder.Services);

            return builder;
        }

        static PluginsService _plugins;

        static void SetupServices(IServiceCollection services)
        {
            ConfigService.Singleton = new ConfigService();
            _plugins = new PluginsService(services);

            services.AddHostedService<LoggingService>();
            services.AddHostedService<ProxyService>();
            services.AddHostedService<PublicKeyService>();
            services.AddHostedService<ListService>();
            services.AddHostedService<QueueService>();
            services.AddHostedService<ClientsUpdaterService>();
            services.AddHostedService<CleanupService>();
            services.AddHostedService<CommandsService>();
        }

        static async Task RunApplication(HostApplicationBuilder app)
        {
            if (app == null) return;

            IHost host = app.Build();

            try
            {
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "XProxy");
            }
        }
    }
}