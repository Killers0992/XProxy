using Newtonsoft.Json;

namespace XProxy.Models
{
    public class BuildInfo
    {
        private Version _parsedVersion;

        [JsonIgnore]
        public Version ParsedVersion
        {
            get
            {
                if (_parsedVersion == null)
                    System.Version.TryParse(Version, out _parsedVersion);

                return _parsedVersion;
            }
        }

        public string Version { get; set; } = "0.0.0";
        public string[] SupportedGameVersions { get; set; } = new string[0];
        public string[] Changelogs { get; set; } = new string[0];
        public string CoreUrl { get; set; }
        public string DependenciesUrl { get; set; }
    }
}
