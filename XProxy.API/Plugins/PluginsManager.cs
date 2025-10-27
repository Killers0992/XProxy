namespace XProxy.API.Plugins;

public static class PluginsManager
{
    private static IServiceCollection _serviceCollection;

    public static string PluginsPath => Path.Combine("Plugins");
    public static string DependenciesPath => Path.Combine("Dependencies");

    public static List<Assembly> Dependencies = new List<Assembly>();

    public static Dictionary<Assembly, Plugin> AssemblyToPlugin = new Dictionary<Assembly, Plugin>();
    public static Dictionary<string, Assembly> NameToAssembly = new Dictionary<string, Assembly>();

    public static void Initialize(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;

        if (!Directory.Exists(PluginsPath))
            Directory.CreateDirectory(PluginsPath);

        if (!Directory.Exists(DependenciesPath))
            Directory.CreateDirectory(DependenciesPath);

        LoadDependencies();
        LoadPlugins();

        AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
    }

    public static void LoadDependencies()
    {
        string[] dependencies = Directory.GetFiles(DependenciesPath, "*.dll");

        int loaded = 0;

        for (int x = 0; x < dependencies.Length; x++)
        {
            Dependencies.Add(Assembly.LoadFrom(dependencies[x]));
            loaded++;
        }
    }

    public static void LoadPlugins()
    {
        string[] plugins = Directory.GetFiles(PluginsPath, "*.dll");

        ProxyLogger.Info($"Loading (f=yellow){plugins.Length}(f=white) plugins", "PluginsService");

        for (int x = 0; x < plugins.Length; x++)
        {
            string name = Path.GetFileName(plugins[x]);

            if (name.StartsWith("-"))
                continue;

            byte[] data = File.ReadAllBytes(plugins[x]);
            Assembly assembly = Assembly.Load(data);

            Dictionary<string, AssemblyName> loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName()).ToDictionary(x => x.Name, y => y);
            Dictionary<string, AssemblyName> pluginReferences = assembly.GetReferencedAssemblies().ToDictionary(x => x.Name, y => y);

            var missingAssemblies = pluginReferences.Where(x => !loadedAssemblies.ContainsKey(x.Key)).ToList();

            Type[] types = assembly.GetTypes();

            Plugin plugin = null;

            foreach (var type in types)
            {
                if (!type.IsSubclassOf(typeof(Plugin)))
                    continue;

                plugin = (Plugin)Activator.CreateInstance(type);
                AssemblyToPlugin.Add(assembly, plugin);
                break;
            }

            if (plugin == null)
                continue;

            NameToAssembly.Add(assembly.FullName, assembly);

            Load(plugin);
        }
    }

    public static void Load(Plugin plugin)
    {
        plugin.PluginDirectory = Path.Combine(PluginsPath, $"{plugin.Name}");

        try
        {
            plugin.LoadConfig();

            plugin.OnLoad(_serviceCollection);

            ProxyLogger.Info($"Plugin (f=yellow){plugin.Name}(f=white) loaded", "PluginsService");
        }
        catch (Exception ex)
        {
            ProxyLogger.Error($"Failed loading plugin {plugin.Name}\n{ex}", "PluginsService");
        }
    }

    private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
    {
        if (NameToAssembly.TryGetValue(args.Name, out Assembly assembly))
            return assembly;

        //AssemblyNameInfo nameInfo = new AssemblyNameInfo(args.Name);

        //Console.WriteLine(nameInfo.Version);
        return null;
    }
}
