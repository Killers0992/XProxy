using XProxy.Worlds;

namespace XProxy.Servers;

public class LobbyServer : Server
{
    public LobbyServer() : base("Lobby", "127.0.0.1", 8888, true, false)
    {

    }

    // Accept all clients connecting to the lobby server
    public override bool OnClientConnecting(BaseClient client) => true;

    public override void OnClientConnected(BaseClient client)
    {
        client.SendToScene("Facility");
    }

    public override void OnClientReady(BaseClient client) => client.SpawnObjects();

    public override void OnClientSpawnPlayer(BaseClient client) => client.World = new Lobby();
}
