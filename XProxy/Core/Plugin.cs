namespace XProxy.Core;

public abstract class Plugin
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Author { get; }
    public abstract Version Version { get; }

    public string PluginDirectory { get; internal set; }

    public virtual void LoadConfig() { }
    public virtual void SaveConfig() { }

    public virtual void OnLoad(IServiceCollection collection) { }
    public virtual void OnUnload() { }
}
