namespace XProxy.API.Commands;

public static class CommandsManager
{
    public static Dictionary<string, CommandDelegate> RegisteredCommands { get; private set; } = new Dictionary<string, CommandDelegate>();

    public static void RegisterConsoleCommandsInAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var ev = method.GetCustomAttribute<ConsoleCommand>();

                if (ev == null)
                    continue;

                string commandName = ev.Name.ToLower();

                if (RegisteredCommands.ContainsKey(commandName))
                {
                    ProxyLogger.Info($"Command '{commandName}' is already registered, skipping duplicate.", "CommandsManager");
                    continue;
                }

                CommandDelegate del = (CommandDelegate)Delegate.CreateDelegate(typeof(CommandDelegate), method);
                RegisteredCommands.Add(commandName, del);

                ProxyLogger.Info($"Command '{commandName}' registered!", "CommandsManager");
            }
        }
    }

    public static void Initialize()
    {
        RegisterConsoleCommandsInAssembly(typeof(CommandsManager).Assembly);
    }
}
