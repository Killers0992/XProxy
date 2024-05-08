namespace XProxy.BuildListing.Models
{
    public class BuildInfo
    {
        public string Version { get; set; }
        public string GameVersion { get; set; }
        public Dictionary<string, FileInfo> Files { get; set; } = new Dictionary<string, FileInfo>();
    }
}