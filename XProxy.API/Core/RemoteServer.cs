using UserSettings.ServerSpecific;

namespace XProxy.Servers;

public class RemoteServer : Server
{
    Dictionary<int, string> _servers = new Dictionary<int, string>();
    ServerSpecificSettingBase[] _settings;

    public ServerSpecificSettingBase[] ServerSettings
    {
        get
        {
            if (_settings == null)
            {
                List<ServerSpecificSettingBase> settings = new List<ServerSpecificSettingBase>()
                {
                    new SSGroupHeader("Servers"),
                };

                int id = 0;
                foreach (string server in ProxySettings.Singleton.ServersInSelector)
                {
                    Server target = Get<Server>(name: server);

                    if (target == null)
                        continue;

                    settings.Add(new SSButton(id, target.Name, "Connect"));
                    _servers.Add(id, target.Name);
                    id++;
                }

                _settings = settings.ToArray();
            }
            return _settings;
        }
    }

    public RemoteServer(string name, string ip, int port, bool isSimulated, bool forwardIpAddress) : base(name, ip, port, isSimulated, forwardIpAddress) { }

    public override void OnClientSpawned(Client client) => client.SendServerSpecificEntries(ServerSettings);
    public override void OnClientSSSReponse(Client client, int id)
    {
        if (!_servers.TryGetValue(id, out string server))
            return;

        client.Connect(server);
    }
}
