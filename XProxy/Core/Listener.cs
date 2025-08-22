namespace XProxy.Core;

public class Listener : BaseListener
{
    public ListenerSettings Settings;

    public bool ForceServerListUpdate;

    public bool ServerListUpdate;
    public int ServerListCycle;

    public Listener(ListenerSettings settings, CancellationToken token) : base(settings.ShortName, settings.ListenAddress, settings.ListenPort, settings.GameVersion, settings.Priorities, settings.Address, token) 
    {
        Settings = settings;

        Logger.Info($"{Tag} Started listening ( Game version: (f=green){GameVersion}(f=white) )", "Listener");
    }

    public override void OnClientConnected(BaseClient client)
    {
        Logger.Info($"{client.Tag} Connected", "Listener");

        client.Connect(Priorities);
    }
}
