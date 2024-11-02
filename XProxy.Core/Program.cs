using System;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XProxy.Core.Services;
using XProxy.Services;

[assembly: AssemblyVersion("1.6.8")]

namespace XProxy
{
    class Program
    {
        public class Options
        {
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
            });

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

            CommandsService.RegisterConsoleCommandsInAssembly(Assembly.GetExecutingAssembly());

            services.AddHostedService<LoggingService>();
            services.AddHostedService<ListenersService>();
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