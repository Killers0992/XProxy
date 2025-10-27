namespace XProxy.API.Structs;

public struct RoundRestartResponse : IDisconnectResponse
{
    public RoundRestartType Type { get; }
    public float TimeOffset { get; }

    public RoundRestartResponse(RoundRestartType type, float offset)
    {
        Type = type;
        TimeOffset = offset;
    }
}
