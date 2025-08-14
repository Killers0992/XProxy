using XProxy.Objects.Components;

namespace XProxy.Objects;

public class ConfigSyncObject : SpawnableObject
{
    public string ServerName
    { 
        get => ServerSyncComponent.ServerName;
        set => ServerSyncComponent.ServerName = value;
    }

    public ServerSyncComponent ServerSyncComponent { get; private set; }

    public ConfigSyncObject(World world) : base(world, null, 180257209, 3656837585730004444, 424)
    {
        ServerSyncComponent = new ServerSyncComponent(this);
        Behaviours = new[]
        {
            ServerSyncComponent
        };
    }
}
