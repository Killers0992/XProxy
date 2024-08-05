using Newtonsoft.Json;
using System.Text;
using XProxy.Shared.Models;
using XProxy.Shared.Properties;

namespace XProxy.Shared
{
    public static class ProxyBuildInfo
    {
        static ReleaseInfo _releaseInfo;

        public static ReleaseInfo ReleaseInfo
        {
            get
            {
                if (_releaseInfo == null)
                    _releaseInfo = JsonConvert.DeserializeObject<ReleaseInfo>(Encoding.UTF8.GetString(Resources.releaseInfo));

                return _releaseInfo;
            }
        }
    }
}
