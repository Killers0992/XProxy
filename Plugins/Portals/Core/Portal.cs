using UnityEngine;
using XProxy.Core;
using XProxy.Networking;
using XProxy.Objects;

namespace Portals.Core;

public class Portal
{
    private DateTime _nextCheck;
    private TextToyObject _text;
    private string _textFormat;

    public static Dictionary<World, List<Portal>> SpawnedPortals = new Dictionary<World, List<Portal>>();

    public const float MinimumDistanceToActivePortal = 1.5f;

    public World World { get; }
    public Server Server => Server.Get<Server>(name: TargetServer);
    public string TargetServer { get; }
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }

    public Portal(World world, string targetServer, string textFormat, Vector3 position, Quaternion rotation)
    {
        _textFormat = textFormat;

        World = world;
        TargetServer = targetServer;
        Position = position;
        Rotation = rotation;

        if (!SpawnedPortals.TryGetValue(world, out List<Portal> portals))
        {
            portals = new List<Portal>();
            SpawnedPortals.Add(world, portals);
        }

        _text = new TextToyObject(world, FormatText());
        _text.Position = position;
        _text.Rotation = rotation;
        _text.DisplaySize = new Vector2(150f, 50f);

        portals.Add(this);
    }

    public string FormatText()
    {
        string tempText = _textFormat;

        Dictionary<string, Func<string>> placeHolders = new Dictionary<string, Func<string>>()
        {
            { "%serverName%", () =>
                {
                    return Server.Name;
                }
            },
        };

        foreach(var placeholder in placeHolders)
        {
            tempText = tempText.Replace(placeholder.Key, placeholder.Value.Invoke());
        }

        return tempText;
    }

    public void Update()
    {
        if (_nextCheck > DateTime.Now)
            return;

        foreach(BaseClient client in World.GetClientsSnapshot())
        {
            float distance = Vector3.Distance(World.GetPosition(client), Position);

            if (distance < MinimumDistanceToActivePortal)
                PlayerActivatedPortal(client);
        }

        _nextCheck = DateTime.Now.AddSeconds(1);
    }

    void PlayerActivatedPortal(BaseClient client)
    {
        client.Connect(TargetServer);
    }
}
