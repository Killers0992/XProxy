using System;
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

        public override async void Update()
        {
            int pos = Player.PositionInQueue;

            if (!Player.IsInQueue)
            {
                await Player.JoinQueue();
                return;
            }

            if (pos == 1)
            {
                if (Player.RedirectTo(Player.CurrentServer, true))
                {
                }
                else
                {
                    Player.SendHint(Player.Proxy._config.Messages.FirstPositionInQueue
                        .Replace("%position%", $"{pos}")
                        .Replace("%totalInQueue%", $"{Player.CurrentServer.PlayersInQueueCount}"), 1);
                }
            }
            else
            {
                bool isPriority = await Player.IsPriorityPlayer(Player.UserId);
                string queueMessage;

                if (isPriority)
                {
                    queueMessage = Player.Proxy._config.Messages.PriorityPositionInQueue
                        .Replace("%position%", $"{pos}")
                        .Replace("%totalInQueue%", $"{Player.CurrentServer.PlayersInQueueCount}");
                }
                else
                {
                    queueMessage = Player.Proxy._config.Messages.PositionInQueue
                        .Replace("%position%", $"{pos}")
                        .Replace("%totalInQueue%", $"{Player.CurrentServer.PlayersInQueueCount}");
                }

                Player.SendHint(queueMessage, 1);
            }
        }

        public override void OnReceiveGameConsoleCommand(string command, string[] args)
        {
            switch (command.ToLower())
            {
                case "hub":
                case "lobby":
                    var rng = Player.Proxy.GetRandomServerFromPriorities();

                    if (rng.Settings.ConnectionType == ConnectionType.Simulated)
                    {
                        if (Listener.Simulations.TryGetValue(rng.Settings.Simulation, out Type simType))
                        {
                            Player.CurrentServer = rng;
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
