using System;
using XProxy.Enums;
using XProxy.Services;

namespace XProxy.Core.Connections
{
    public class QueueConnection : SimulatedConnection
    {
        public int NextAttempt = 5;

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

        public override void Update()
        {
            int pos = Player.PositionInQueue;

            if (!Player.IsInQueue)
            {
                Player.JoinQueue();
                return;
            }

            if (pos == 1)
            {
                if (NextAttempt == 0)
                {
                    Player.ConnectTo(Player.CurrentServer);
                    NextAttempt = 5;
                }
                else
                    NextAttempt--;

                Player.SendHint(ConfigService.Singleton.Messages.FirstPositionInQueue.Replace("%position%", $"{pos}").Replace("%totalInQueue%", $"{Player.CurrentServer.PlayersInQueueCount}"), 1);
            }
            else
            {
                Player.SendHint(ConfigService.Singleton.Messages.PositionInQueue.Replace("%position%", $"{pos}").Replace("%totalInQueue%", $"{Player.CurrentServer.PlayersInQueueCount}"), 1);
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
                    }
                    break;
            }
        }
    }
}
