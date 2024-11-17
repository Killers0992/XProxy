using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using XProxy.Plugin;
using XProxy.Plugin.Core;

public class MainClass
{
    public static MainClass Singleton;

    [PluginConfig]
    public Config Config;

    [PluginEntryPoint("XProxy.Plugin", "1.0.0", "Plugin made for connecting to proxy making better exchange of information between server and proxy.", "Killers0992")]
    public void Entry()
    {
        Singleton = this;
        EventManager.RegisterAllEvents(this);
        UnityEngine.Object.FindObjectOfType<ServerStatic>().gameObject.AddComponent<ProxyConnection>();
    }
}