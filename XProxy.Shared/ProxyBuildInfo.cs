using Newtonsoft.Json;
using System.Text;
using XProxy.Shared.Models;
using XProxy.Shared.Properties;

namespace XProxy.Shared
{
    public static class ProxyBuildInfo
    {
        static ReleaseInfo _releaseInfo;
        static Version _version, _gameVersion;

        public static ReleaseInfo ReleaseInfo
        {
            get
            {
                if (_releaseInfo == null)
                    _releaseInfo = JsonConvert.DeserializeObject<ReleaseInfo>(Encoding.UTF8.GetString(Resources.releaseInfo));

                return _releaseInfo;
            }
        }

        public static Version Version
        {
            get
            {
                if (_version == null)
                    System.Version.TryParse(ReleaseInfo.Version, out _version);

                return _version;
            }
        }

        public static Version SupportedGameVersion
        {
            get
            {
                if (_gameVersion == null)
                    System.Version.TryParse(ReleaseInfo.GameVersion, out _gameVersion);

                return _gameVersion;
            }
        }
    }
}
