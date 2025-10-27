namespace XProxy.API.Structs;

public struct DelayConnectionResponse : IDisconnectResponse
{
    public byte TimeInSeconds { get; }

    public DelayConnectionResponse(byte timeInSeconds)
    {
        TimeInSeconds = timeInSeconds;   
    }
}
