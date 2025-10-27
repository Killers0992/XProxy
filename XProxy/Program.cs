using LiteNetLib;
using Microsoft.Extensions.Logging;
using XProxy.API;
using XProxy.Misc;
using XProxy.Services;

//NetworkingMessagesGenerator.Generate();

ProxySettings.Load();

ReadWriterInitializer.InitializeAll();

NetDebug.Logger = new CustomNetLogger();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.None);

builder.Services.AddHostedService<LoggingService>();
builder.Services.AddHostedService<ListenersService>();
builder.Services.AddHostedService<PublicKeyService>();
builder.Services.AddHostedService<ListService>();
builder.Services.AddHostedService<CommandsService>();

ProxyAPI.Initialize(builder.Services);

IHost host = builder.Build();

host.Run();