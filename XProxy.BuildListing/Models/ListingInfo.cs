namespace XProxy.Models
{
    public class ListingInfo
    {
        public Dictionary<string, BuildInfo> Versions { get; set; } = new Dictionary<string, BuildInfo>();
    }
}
