namespace XProxy.API.Commands;

public class SpawnTextCommand
{
    [ConsoleCommand("spawntext")]
    public static void OnSpawnTextCommand( string[] args)
    {
        string message = string.Join(" ", args);

        foreach(var listener in Listener.List)
        {
            foreach(var client in listener.ClientById.Values)
            {
                ProxyLogger.Info("Client pos " + client.Position.ToString(), "spawntext");

                TextToyObject text = new TextToyObject(client.World)
                {
                    Position = client.Position,
                };

                text.TextToy.TextFormat = message;
                text.TextToy.DisplaySize = new UnityEngine.Vector2(150f, 25f);

                text.SpawnWithPayload(client);


                ProxyLogger.Info("Spawned text with message " + message, "spawntext");
            }
        }
    }
}
