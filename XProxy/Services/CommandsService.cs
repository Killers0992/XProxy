using System.Reflection;
using XProxy.Attributes;
using XProxy.Core;

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
                        continue;
                    }

                    Delegate del = Delegate.CreateDelegate(typeof(CommandDelegate), method);
                    Commands.Add(ev.Name.ToLower(), del);
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
                        }
                    }
                }
                else
                {
                }

                await Task.Delay(15);
            }
        }
    }
}
