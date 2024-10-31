using System.ComponentModel;
using YamlDotNet.Serialization;

namespace XProxy.Shared.Models
{
    public class ScpServerList
    {
        [Description("If you want to list your server on SCPSL ServerList set this to true.")]
        public bool UseScpServerList { get; set; }

        [Description("This IP is used for listing your server on SCPSL Server List ( auto = automatically gets public ip )")]
        public string AddressIp { get; set; } = "auto";

        [Description("This NAME is used for listing your server on SCPSL Server List.")]
        [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.Literal)]
        public string Name { get; set; } =
@" Example server name.
  Proxy Server";

        [Description("This PASTEBIN is used for server information on SCPSL Server List.")]
        public string Pastebin { get; set; } = "7wV681fT";


        [Description("Setting this value for example to lobby it will take PlayerCount + MaxPlayerCount and it will use for displaying on serverlist.")]
        public string TakePlayerCountFromServer { get; set; } = string.Empty;
    }
}
