
namespace XProxy.API.Structs;

public struct BannedResponse : IDisconnectResponse
{
    public BannedResponse(string reason, DateTime banExpireDate)
    {
        Reason = reason;
        BanExpiration = banExpireDate;
    }

    public string Reason { get; }

    public DateTime BanExpiration { get; }
}
