using System.Reflection;

namespace XProxy.Services;

public class PluginsService
{
    public List<Assembly> Dependencies = new List<Assembly>();
    public Dictionary<Assembly, Plugin> AssemblyToPlugin = new Dictionary<Assembly, Plugin>();

    string _pluginsPath => Path.Combine("Plugins");
    string _dependenciesPath => Path.Combine("Dependencies");

    IServiceCollection _serviceCollection;

    public PluginsService(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;

        if (!Directory.Exists(_pluginsPath))
            Directory.CreateDirectory(_pluginsPath);

        if (!Directory.Exists(_dependenciesPath))
            Directory.CreateDirectory(_dependenciesPath);

        LoadDependencies();
        LoadPlugins();
    }

    public void LoadDependencies()
    {
        string[] dependencies = Directory.GetFiles(_dependenciesPath, "*.dll");

        int loaded = 0;

        for (int x = 0; x < dependencies.Length; x++)
        {
            Dependencies.Add(Assembly.LoadFrom(dependencies[x]));
            loaded++;
        }
    }

    public void LoadPlugins()
    {
        string[] plugins = Directory.GetFiles(_pluginsPath, "*.dll");

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
            {
                continue;
            }

            name = plugin.Name;

            plugin.PluginDirectory = Path.Combine(_pluginsPath, $"{name}");

            try
            {
                plugin.LoadConfig();

                plugin.OnLoad(_serviceCollection);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed loading plugin {name}\n{ex}", "PluginsService");
            }
        }
    }
}