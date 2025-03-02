using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using XProxy.Misc;
using XProxy.Models;
using XProxy.Services;

namespace XProxy
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HostApplicationBuilder applicationBuild = await BuildApplication();

            SetupServices(applicationBuild);

            if (!SetupProxyServices(applicationBuild))
            {
                ConsoleLogger.Info($"Press any key to exit.", "XProxy");
                Console.ReadKey();
                return;
            }

            await RunApplication(applicationBuild);
        }

        static async Task<HostApplicationBuilder> BuildApplication()
        {
            var builder = Host.CreateApplicationBuilder();

            LauncherSettings.Load();
            await UpdaterService.IntialRun();

            builder.Logging.SetMinimumLevel(LogLevel.None);

            return builder;
        }

        static void SetupServices(HostApplicationBuilder builder)
        {
            IServiceCollection services = builder.Services;

            services.AddHostedService<UpdaterService>();
        }

        static bool SetupProxyServices(HostApplicationBuilder builder)
        {
            IServiceCollection services = builder.Services;

            if (Directory.Exists(UpdaterService.DependenciesFolder))
            {
                foreach (var dependency in Directory.GetFiles(UpdaterService.DependenciesFolder))
                    Assembly.LoadFrom(dependency);
            }

            if (!File.Exists(UpdaterService.ProxyFile))
            {
                ConsoleLogger.Error($"XProxy.Core.dll file not found, aborting startup!", "XProxy");
                return false;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (o, e) =>
            {
                AssemblyName aName = new AssemblyName(e.Name);

                switch (aName.Name)
                {
                    case "XProxy.Core":
                        return Assembly.LoadFrom(UpdaterService.ProxyFile);
                    default:
                        Console.WriteLine($"Failed to resolve assembly for " + aName.Name);
                        return null;
                }
            };

            
            try
            {
                InitializeProxy(services);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error($"Failed to setup services for XProxy, aborting startup!\n{ex}", "XProxy");
                return false;
            }

            return true;
        }

        static void InitializeProxy(IServiceCollection services)
        {
            Logger.AnsiDisabled = LauncherSettings.Value.DisableAnsiColors;
            Initializer.SetupServices(services);
        }

        static async Task RunApplication(HostApplicationBuilder app)
        {
            IHost host = app.Build();

            await host.RunAsync();
        }
    }
}
