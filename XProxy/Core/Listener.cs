namespace XProxy.Core;

public class Listener : BaseListener
{
    public Listener(string listenIp, int listenPort, CancellationToken token) : base(listenIp, listenPort, token) 
    {
    }

    public override void OnClientConnected(BaseClient client)
    {
        Logger.Info($"{client.PlayerTag} Connected", "Listener");

        client.Connect(new Server("207.174.43.245", 7783));
    }

    public override void OnClientDisconneted(BaseClient client)
    {
        Logger.Info($"{client.PlayerTag} Disconnected", "Listener");
    }
}
