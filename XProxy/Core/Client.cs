namespace XProxy.Core;

public class Client : BaseClient
{
    public Client(BaseListener listener, ConnectionRequest request, PreAuth preAuth) : base(listener, request, preAuth) { }

    public override bool OnDisconnectedFromServer(Server Server, ConnectionFailedInfo info)
    {
        switch (info.Response)
        {
            case DisconnectType.ServerIsFull:
                TakeServerAndTryConnect();
                return false;
        }

        return true;
    }
}
