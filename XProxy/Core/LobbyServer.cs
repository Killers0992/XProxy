using Mirror;
using PlayerRoles;
using System;
using UnityEngine;

namespace XProxy.Core
{
    public class LobbyServer : Server
    {
        DateTime _nextSend = DateTime.Now;

        public LobbyServer() : base("Lobby", "127.0.0.1", 8888, true)
        {
        }

        // Accept all clients connecting to the lobby server
        public override bool OnClientConnecting(BaseClient client) => true;

        public override void OnClientConnected(BaseClient client) => client.SendToScene("Facility");
        public override void OnClientReady(BaseClient client) => client.SpawnObjects();

        public override void OnClientSpawnPlayer(BaseClient client)
        {
            client.Spawn();
            client.SetRole(RoleTypeId.Tutorial);
            client.SetHealth(100f);
            client.SetSeed(350);

            client.Object.UserId = client.PreAuth.UserId;
            client.Object.Nickname = $"[{client.NetworkIdentityId}] Proxy";
            client.Object.SendUpdate(client);
        }

        public override void OnUpdate()
        {
            if (_nextSend < DateTime.Now)
            {
                _nextSend = DateTime.Now.AddSeconds(1);
                
                foreach (BaseClient client in Clients)
                {
                    if (client.Object == null)
                        continue;

                    client.Object.Nickname = $"[{client.NetworkIdentityId}] {DateTime.Now.TimeOfDay}";
                    client.Object.SendUpdate(client);
                }
            }
        }
    }
}
