using System;
using System.Reflection;

[assembly: AssemblyVersion(
    "1.7.5"
)]

namespace XProxy.Core
{
    public static class BuildInformation
    {
        static Version _buildVersion;

        public static Version Version
        {
            get
            {
                if (_buildVersion == null)
                    _buildVersion = Initializer.CoreAssembly.GetName().Version;

                return _buildVersion;
            }
        }

        public static string VersionText => Version.ToString(3);

        public static string[] SupportedGameVersions =
        {
            "13.5.1",
        };

        public static string[] Changelogs = new string[0];
    }
}
