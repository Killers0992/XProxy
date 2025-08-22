namespace XProxy.Responses;

public class RoundRestartResponse : BaseResponse
{
    public RoundRestartType Type { get; }
    public float TimeOffset { get; }

    public RoundRestartResponse(RoundRestartType type, float offset)
    {
        Type = type;
        TimeOffset = offset;
    }
}
