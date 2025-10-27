using Lobby.Worlds;
using XProxy.API.Core;
using XProxy.API.Networking;

namespace Lobby;

public class LobbyServer : Server
{
    public LobbyServer() : base("Lobby", "-lobby-", 7777, true, false) { }

    public override bool OnClientConnecting(Client client) => true;
    public override void OnClientConnected(Client client) => client.SendToScene("Facility");
    public override void OnClientReady(Client client) => client.SpawnObjects();
    public override void OnClientSpawnPlayer(Client client) => client.World = new LobbyWorld();
}
