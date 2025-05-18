namespace XProxy.Responses;

public class BannedResponse : BaseResponse
{
    public BannedResponse(string reason, DateTime banExpireDate)
    {
        Reason = reason;
        BanExpiration = banExpireDate;
    }

    public string Reason { get; }

    public DateTime BanExpiration { get; }
}
