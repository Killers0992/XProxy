namespace XProxy.Responses;

public class DelayConnectionResponse : BaseResponse
{
    public byte TimeInSeconds { get; }

    public DelayConnectionResponse(byte timeInSeconds)
    {
        TimeInSeconds = timeInSeconds;   
    }
}
