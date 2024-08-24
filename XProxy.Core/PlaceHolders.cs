using System.Text.RegularExpressions;
using XProxy.Models;
using XProxy.Services;

namespace XProxy.Core
{
    public class PlaceHolders
    {
        public static string ReplacePlaceholders(string text)
        {
            return Regex.Replace(text, @"%(\w+?)(?:_(\w+))?%", match =>
            {
                string placeholderType = match.Groups[1].Value.ToLower();
                string serverName = match.Groups[2].Success ? match.Groups[2].Value : null;

                ServerInfo serverInstance = ProxyService.Singleton.GetServerByName(serverName);

                switch (placeholderType.ToLower())
                {
                    case "playersinqueue":
                        if (serverInstance == null)
                            return "-1";

                        return $"{serverInstance.PlayersInQueue}";
                    case "onlineplayers":
                        if (serverInstance == null)
                            return "-1";

                        return $"{serverInstance.PlayersOnline}";
                    case "maxplayers":
                        if (serverInstance == null)
                            return "-1";

                        return $"{serverInstance.PlayersOnline}";
                    case "proxyonlineplayers":
                        return ProxyService.Singleton.Players.Count.ToString();
                    case "proxymaxplayers":
                        return ConfigService.Singleton.Value.MaxPlayers.ToString();
                    default:
                        return "%placeholder_not_found%";
                }
            });
        }
    }
}
