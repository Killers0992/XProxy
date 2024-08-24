using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using XProxy.Core;

namespace XProxy.Services
{
    public class PluginsService
    {
        public List<Assembly> Dependencies = new List<Assembly>();
        public Dictionary<Assembly, Plugin> AssemblyToPlugin = new Dictionary<Assembly, Plugin>();

        string _pluginsPath =>  Path.Combine(ConfigService.MainDirectory, "Plugins");
        string _dependenciesPath => Path.Combine(ConfigService.MainDirectory, "Dependencies");

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
                byte[] data = File.ReadAllBytes(dependencies[x]);
                Dependencies.Add(Assembly.Load(data));
                loaded++;
            }

            if (dependencies.Length == 0)
            {
                Logger.Info(ConfigService.Singleton.Messages.NoDependenciesToLoadMessage, "PluginsService");
            }
            else
            {
                Logger.Info(ConfigService.Singleton.Messages.LoadedAllDependenciesMesssage.Replace("%loaded%", $"{loaded}"), "PluginsService");
            }
        }

        public void LoadPlugins()
        {
            string[] plugins = Directory.GetFiles(_pluginsPath, "*.dll");

            int loaded = 0;
            int failed = 0;

            for(int x = 0; x < plugins.Length; x++)
            {
                string name = Path.GetFileName(plugins[x]);

                if (name.StartsWith("-"))
                {
                    Logger.Info(ConfigService.Singleton.Messages.PluginDisabledMessage.Replace("%current%", $"{x+1}").Replace("%max%", $"{plugins.Length}").Replace("%name%", name), "PluginsService");
                    continue;
                }

                byte[] data = File.ReadAllBytes(plugins[x]);
                Assembly assembly = Assembly.Load(data);

                Dictionary<string, AssemblyName> loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName()).ToDictionary(x => x.Name, y => y);
                Dictionary<string, AssemblyName> pluginReferences = assembly.GetReferencedAssemblies().ToDictionary(x => x.Name, y => y);

                var missingAssemblies = pluginReferences.Where(x => !loadedAssemblies.ContainsKey(x.Key)).ToList();

                if (missingAssemblies.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(ConfigService.Singleton.Messages.PluginHasMissingDependenciesMessage.Replace("%current%", $"{x+1}").Replace("%max%", $"{plugins.Length}").Replace("%name%", name));
                    foreach(var dependency in missingAssemblies)
                    {
                        sb.AppendLine(ConfigService.Singleton.Messages.PluginMissingDependencyMessage.Replace("%name%", dependency.Key).Replace("%version%", dependency.Value.Version.ToString(3)));
                    }
                    Logger.Info(sb, "PluginsServices");
                    failed++;
                    //continue;
                }

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
                    Logger.Info(ConfigService.Singleton.Messages.PluginInvalidEntrypointMessage.Replace("%current%", $"{x+1}").Replace("%max%", $"{plugins.Length}").Replace("%name%", name), "PluginsService");
                    failed++;
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

                Logger.Info(ConfigService.Singleton.Messages.PluginLoadedMessage.Replace("%current%", $"{x+1}").Replace("%max%", $"{plugins.Length}").Replace("%name%", name), "PluginsService");
                loaded++;
            }

            if (plugins.Length == 0)
            {
                Logger.Info(ConfigService.Singleton.Messages.NoPluginsToLoadMessage, "PluginsService");
            }
            else if (plugins.Length == loaded)
            {
                Logger.Info(ConfigService.Singleton.Messages.LoadedAllPluginsMesssage.Replace("%loaded%", $"{loaded}"), "PluginsService");
            }
            else
            {
                Logger.Info(ConfigService.Singleton.Messages.PluginsLoadedAndFailedToLoadMessage.Replace("%loaded%", $"{loaded}").Replace("%failed%", $"{failed}"), "PluginsService");
            }
        }
    }
}
