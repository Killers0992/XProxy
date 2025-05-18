using XProxy.Networking;

namespace XProxy.Core;

public class Listener : BaseListener
{
    public Listener(string listenIp, int listenPort, CancellationToken token) : base(listenIp, listenPort, token) 
    {
    }

    public override void OnClientConnected(Client client)
    {
        Console.WriteLine($"[{ListenIpAddress}:{ListenPort}] [{client.PreAuth.UserId}] connected!");

        client.Connect(new Server("207.174.43.245", 7783));
    }

    public override void OnClientDisconneted(Client client)
    {
        Console.WriteLine($"[{ListenIpAddress}:{ListenPort}] [{client.PreAuth.UserId}] disconnected!");
    }
}
