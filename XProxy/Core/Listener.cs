using XProxy.Servers;

namespace XProxy.Core;

public class Listener : BaseListener
{
    public ListenerSettings Settings;

    public bool ForceServerListUpdate;

    public bool ServerListUpdate;
    public int ServerListCycle;

    public Listener(ListenerSettings settings, CancellationToken token) : base(settings.ShortName, settings.Address, settings.Port, settings.GameVersion, settings.Priorities, token) 
    {
        Server.Register(new LobbyServer());

        Settings = settings;

        Logger.Info($"{Tag} Started listening ( Game version: (f=green){GameVersion}(f=white) )", "Listener");
    }

    public override void OnClientConnected(BaseClient client)
    {
        Logger.Info($"{client.Tag} Connected", "Listener");

        client.Connect<LobbyServer>();

        //client.Connect(Priorities);
    }

    public override void OnClientDisconneted(BaseClient client, DisconnectReason reason)
    {
        switch (reason)
        {
            case DisconnectReason.RemoteConnectionClose:
                Logger.Info($"{client.Tag} Client closed the connection", "Listener");
                break;
            default:
                Logger.Info($"{client.Tag} Disconnected", "Listener");
                break;
        }
    }
}
