using System.Linq;
using System.Text.RegularExpressions;
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

                Server.TryGetByName(serverName, out Server targetServer);

                switch (placeholderType.ToLower())
                {
                    case "playersinqueue":
                        if (targetServer == null)
                            return "-1";

                        return $"{targetServer.PlayersInQueueCount}";
                    case "onlineplayers":
                        if (targetServer == null)
                            return "-1";

                        return $"{targetServer.PlayersCount}";
                    case "maxplayers":
                        if (targetServer == null)
                            return "-1";

                        return $"{targetServer.PlayersCount}";
                    case "proxyonlineplayers":
                        return Listener.GetTotalPlayersOnline().ToString();
                    default:
                        return "%placeholder_not_found%";
                }
            });
        }
    }
}
