namespace XProxy.Models
{
    public class BuildInfo
    {
        public string Version { get; set; }

        public string GameVersion { get; set; }

        public Dictionary<string, BuildFileInfo> Files { get; set; } = new Dictionary<string, BuildFileInfo>();
    }
}