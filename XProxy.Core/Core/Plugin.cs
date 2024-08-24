using Microsoft.Extensions.DependencyInjection;
using System;

namespace XProxy.Core
{
    public abstract class Plugin
    {
        public virtual string Name { get; }
        public virtual string Description { get; }
        public virtual string Author { get; }
        public virtual Version Version { get; }

        public string PluginDirectory { get; internal set; }

        public virtual void LoadConfig() { }
        public virtual void SaveConfig() { }

        public virtual void OnLoad(IServiceCollection serviceCollection)
        {

        }

        public virtual void OnUnload()
        {

        }
    }
}
