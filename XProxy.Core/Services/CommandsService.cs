using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Attributes;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace XProxy.Services
{
    public class CommandsService : BackgroundService
    {
        public delegate void CommandDelegate(CommandsService service, string[] args);

        public static Dictionary<string, Delegate> Commands { get; private set; } = new Dictionary<string, Delegate>();

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
                        Logger.Warn(ConfigService.Singleton.Messages.CommandAlreadyRegisteredMessage.Replace("%name%", ev.Name), "CommandsService");
                        continue;
                    }

                    Delegate del = Delegate.CreateDelegate(typeof(CommandDelegate), method);
                    Commands.Add(ev.Name.ToLower(), del);
                    Logger.Info(ConfigService.Singleton.Messages.CommandRegisteredMessage.Replace("%name%", ev.Name), "CommandsService");
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string cmd = Console.ReadLine();

                if (string.IsNullOrEmpty(cmd)) continue;

                string[] args = cmd.Split(' ');

                if (Commands.TryGetValue(args[0].ToLower(), out Delegate del))
                {
                    if (del is CommandDelegate d2)
                    {
                        try
                        {
                            d2?.Invoke(this, args.Skip(1).ToArray());
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to execute command {cmd}\n" + ex);
                        }
                    }
                }
                else
                {
                    Logger.Info(ConfigService.Singleton.Messages.CommandNotExistsMessage.Replace("%name%", args[0]), "CommandsService");
                }

                await Task.Delay(15);
            }
        }

        public async Task RunCentralServerCommand(Listener server, string cmd, string args)
        {
            cmd = cmd.ToLower();

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "ip", server.Settings.PublicIp },
                { "port", $"{server.Settings.Port}" },
                { "cmd", ListService.Base64Encode(cmd) },
                { "args", ListService.Base64Encode(args) },
            };

            if (!string.IsNullOrEmpty(ListService.Password))
                data.Add("passcode", ListService.Password);

            using (var response = await server.Settings.Http.PostAsync($"https://api.scpslgame.com/centralcommands/{cmd}.php", new FormUrlEncodedContent(data)))
            {
                string text = await response.Content.ReadAsStringAsync();

                Logger.Info(ConfigService.Singleton.Messages.CentralCommandMessage.Replace("%command%", cmd).Replace("%message%", text), $"ListService");
            }
        }

    }
}
