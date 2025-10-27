namespace XProxy.API.Structs;

public struct ConnectionFailedInfo
{
    public ConnectionFailedInfo(string message, DisconnectType response)
    {
        Message = message;
        Response = response;
    }

    public string Message { get; }
    public DisconnectType Response { get; }
}
