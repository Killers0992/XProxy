using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace XProxy.Models
{
    public class ListenerServer
    {
        private Version _version;

        #region YAML Settings

        [Description("This IP is used for UDP Server to listen on.")]
        public string ListenIp { get; set; } = "0.0.0.0";

        [Description("This PORT is used for listing your server on SCPSL Server List and for UDP Server to listen on.")]
        public ushort Port { get; set; } = 7777;

        [Description("Maximum amount of players which can connect to server.")]
        public int MaxPlayers { get; set; } = 50;

        [Description("Version of game for which listener will run, this version is also used for listing your server on SCP Server List.")]
        public string Version { get; set; } = "13.6.9";

        [Description("Priority servers used for first connection and fallback servers.")]
        public List<string> Priorities { get; set; } = new List<string>() { "lobby" };

        [Description("Settings related to SCP Server List.")]
        public ScpServerList ServerList { get; set; } = new ScpServerList();
        #endregion

        /// <summary>
        /// Gets http client for this listener.
        /// </summary>
        public HttpClient Http;

        /// <summary>
        /// Gets final public ip of this listener.
        /// </summary>
        public string PublicIp;

        /// <summary>
        /// If data on serverlist should be updated.
        /// </summary>
        public bool ServerListUpdate;

        /// <summary>
        /// Gets cycle number of server listing.
        /// </summary>
        public int ServerListCycle;

        [YamlIgnore]
        public System.Version GameVersionParsed
        {
            get
            {
                if (_version == null)
                {
                    string text = Version.Contains("-") ? Version.Split('-')[0] : Version;

                    System.Version.TryParse(text, out _version);
                }

                return _version;
            }
        }

        /// <summary>
        /// Initializes this listener.
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            Http = new HttpClient();
            Http.DefaultRequestHeaders.Add("User-Agent", "SCP SL");
            Http.DefaultRequestHeaders.Add("Game-Version", Version);

            if (ServerList.AddressIp != "auto")
                PublicIp = ServerList.AddressIp;
            else
                PublicIp = await GetPublicIp();
        }

        async Task<string> GetPublicIp()
        {
            try
            {
                using (var response = await Http.GetAsync("https://api.scpslgame.com/ip.php"))
                {
                    string str = await response.Content.ReadAsStringAsync();

                    str = (str.EndsWith(".") ? str.Remove(str.Length - 1) : str);

                    return str;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ListService");
                return null;
            }
        }

    }
}
