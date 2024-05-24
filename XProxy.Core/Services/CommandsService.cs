using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Attributes;

namespace XProxy.Services
{
    public class CommandsService : BackgroundService
    {
        public delegate void CommandDelegate(CommandsService service, string[] args);

        public ConfigService Config { get; private set; }
        public static Dictionary<string, Delegate> Commands { get; private set; } = new Dictionary<string, Delegate>();

        public CommandsService(ConfigService config)
        {
            Config = config;
        }

        public static void RegisterConsoleCommandsInAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    var ev = method.GetCustomAttribute<ConsoleCommand>();

                    if (ev == null) continue;

                    if (Commands.ContainsKey(ev.Name.ToLower()))
                    {
                        Logger.Warn(ConfigService.Instance.Messages.CommandAlreadyRegisteredMessage.Replace("%name%", ev.Name), "CommandsService");
                        continue;
                    }

                    Delegate del = Delegate.CreateDelegate(typeof(CommandDelegate), method);
                    Commands.Add(ev.Name.ToLower(), del);
                    Logger.Info(ConfigService.Instance.Messages.CommandRegisteredMessage.Replace("%name%", ev.Name), "CommandsService");
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RegisterConsoleCommandsInAssembly(Assembly.GetExecutingAssembly());

            while (true)
            {
                string cmd = Console.ReadLine();

                if (string.IsNullOrEmpty(cmd)) continue;

                string[] args = cmd.Split(' ');

                if (Commands.TryGetValue(args[0].ToLower(), out Delegate del))
                {
                    if (del is CommandDelegate d2)
                    {
                        d2?.Invoke(this, args.Skip(1).ToArray());
                    }
                }
                else
                {
                    Logger.Info(Config.Messages.CommandNotExistsMessage.Replace("%name%", args[0]), "CommandsService");
                }

                await Task.Delay(1);
            }
        }
    }
}
