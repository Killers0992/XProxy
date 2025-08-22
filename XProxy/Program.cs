using Microsoft.Extensions.Logging;
using XProxy;

//NetworkingMessagesGenerator.Generate();

Settings.Load();

NetDebug.Logger = new CustomNetLogger();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.None);

PluginsService plugins = new PluginsService(builder.Services);

builder.Services.AddHostedService<LoggingService>();
builder.Services.AddHostedService<ListenersService>();
builder.Services.AddHostedService<PublicKeyService>();
builder.Services.AddHostedService<ListService>();

IHost host = builder.Build();

host.Run();