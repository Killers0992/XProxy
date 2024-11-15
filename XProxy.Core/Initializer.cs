using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using XProxy.Core.Services;
using XProxy.Services;

namespace XProxy
{
    public class Initializer
    {
        static Assembly _coreAssembly;
        static PluginsService _plugins;

        public static Assembly CoreAssembly
        {
            get
            {
                if (_coreAssembly == null )
                {
                    _coreAssembly = typeof(Initializer).Assembly;
                }

                return _coreAssembly;
            }
        }

        public static void SetupServices(IServiceCollection services)
        {
            ConfigService.MainDirectory = Environment.CurrentDirectory;

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
    }
}