using System;
using XProxy.Core.Services;
using XProxy.Shared.Enums;

namespace XProxy.Core.Connections
{
    public class QueueConnection : SimulatedConnection
    {
        public QueueConnection(Player plr) : base(plr)
        {
        }

        public override void OnConnected()
        {
            Player.SendToScene("Facility");
        }

        public override void OnClientReady()
        {
            Player.ObjectSpawnStarted();
            Player.ObjectSpawnFinished();
        }

        public override void OnAddPlayer()
        {
            Player.Spawn();
        }

        bool _addedToQueue = false;

        public override void Update()
        {
            int pos = Player.PositionInQueue;

            if (pos == -1)
            {
                if (!_addedToQueue)
                {
                    QueueService.AddPlayerToQueue(Player);
                    _addedToQueue = true;
                }
                return;
            }

            if (pos == 1)
            {
                if (Player.RedirectTo(Player.ServerInfo, true))
                {
                }
                else
                {
                    Player.SendHint(Player.Proxy._config.Messages.FirstPositionInQueue.Replace("%position%", $"{pos}").Replace("%totalInQueue%", $"{Player.ServerInfo.PlayersInQueue}"), 1);
                }
            }
            else
            {
                Player.SendHint(Player.Proxy._config.Messages.PositionInQueue.Replace("%position%", $"{pos}").Replace("%totalInQueue%", $"{Player.ServerInfo.PlayersInQueue}"), 1);
            }
        }

        public override void OnReceiveGameConsoleCommand(string command, string[] args)
        {
            switch (command.ToLower())
            {
                case "hub":
                case "lobby":
                    var rng = Player.Proxy.GetRandomServerFromPriorities();

                    if (rng.ConnectionType == ConnectionType.Simulated)
                    {
                        if (ProxyServer.Simulations.TryGetValue(rng.Simulation, out Type simType))
                        {
                            Player.ServerInfo = rng;
                            Player.Connection = (SimulatedConnection)Activator.CreateInstance(simType, args: Player);
                            Player.SendGameConsoleMessage("Switch to lobby...");
                        }
                    }
                    else
                    {
                        Player.SendGameConsoleMessage("Connecting to lobby...");
                        Player.RedirectToLobby();
                    }
                    break;
            }
        }
    }
}
