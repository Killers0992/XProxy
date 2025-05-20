namespace XProxy.Core;

public class Client : BaseClient
{
    public Client(BaseListener listener, ConnectionRequest request, PreAuth preAuth) : base(listener, request, preAuth)
    {
    }

    public override void OnConnectedToServer(Server Server)
    {
        Logger.Info($"{PlayerTag} Connected.", "Client");
    }

    public override void OnDisconnectedFromServer(Server Server)
    {
    }
}
