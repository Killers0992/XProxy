using Lobby.Worlds;
using XProxy.Core;
using XProxy.Networking;

namespace Lobby;

public class LobbyServer : Server
{
    public LobbyServer() : base("Lobby", "-lobby-", 7777, true, false) { }

    public override bool OnClientConnecting(BaseClient client) => true;
    public override void OnClientConnected(BaseClient client) => client.SendToScene("Facility");
    public override void OnClientReady(BaseClient client) => client.SpawnObjects();
    public override void OnClientSpawnPlayer(BaseClient client) => client.World = new LobbyWorld();
}
