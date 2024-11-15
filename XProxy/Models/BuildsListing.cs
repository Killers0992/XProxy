namespace XProxy.Models
{
    public class BuildsListing
    {
        public Dictionary<string, BuildInfo> Builds { get; set; } = new Dictionary<string, BuildInfo>();
    }
}
