namespace XProxy.Core;

public class Listener : BaseListener
{
    public Listener(string listenIp, int listenPort, CancellationToken token) : base(listenIp, listenPort, token) 
    {
        Server.Register(new LobbyServer());
    }

    public override void OnClientConnected(BaseClient client)
    {
        Logger.Info($"{client.PlayerTag} Connected", "Listener");

        client.Connect<LobbyServer>();
    }

    public override void OnClientDisconneted(BaseClient client, DisconnectReason reason)
    {
        switch (reason)
        {
            case DisconnectReason.RemoteConnectionClose:
                Logger.Info($"{client.PlayerTag} Client closed the connection", "Listener");
                break;
            default:
                Logger.Info($"{client.PlayerTag} Disconnected", "Listener");
                break;
        }
    }
}
