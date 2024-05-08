using System.Text.Json.Serialization;

namespace XProxy.Models
{
    public class BuildInfo
    {
        private Version _parsedVersion, _parsedGameVersion;

        public string Version { get; set; }

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

        public string GameVersion { get; set; }

        [JsonIgnore]
        public Version ParsedGameVersion
        {
            get
            {
                if (_parsedGameVersion == null)
                    System.Version.TryParse(GameVersion, out _parsedGameVersion);

                return _parsedGameVersion;
            }
        }

        public Dictionary<string, BuildFileInfo> Files { get; set; } = new Dictionary<string, BuildFileInfo>();
    }
}