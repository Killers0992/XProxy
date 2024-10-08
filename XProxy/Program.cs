﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XProxy.Patcher.Services;
using XProxy.Services;
using XProxy.Shared.Services;

namespace XProxy.Patcher
{
    internal class Program
    {
        static async Task Main(string[] args) => await RunApplication(BuildApplication());

        static HostApplicationBuilder BuildApplication()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Logging.SetMinimumLevel(LogLevel.None);

            SetupServices(builder.Services);

            return builder;
        }

        static void SetupServices(IServiceCollection services)
        {
            services.AddSingleton<ConfigService>();
            services.AddHostedService<LoggingService>();
            services.AddHostedService<UpdaterService>();
            services.AddHostedService<MainProcessService>();
        }

        static async Task RunApplication(HostApplicationBuilder app)
        {
            IHost host = app.Build();

            await host.RunAsync();
        }
    }
}
