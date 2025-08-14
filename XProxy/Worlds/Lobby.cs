using PlayerRoles;
using UnityEngine;
using XProxy.Objects;

namespace XProxy.Worlds;

public class Lobby : World
{
    DateTime _next = DateTime.Now;

    public ConfigSyncObject ConfigSync;
    public WaypointObject Waypoint;
    public TextToyObject TextToy;
    public TextToyObject TextToy2;

    public Lobby() : base("Lobby")
    {
        DestroyOnEmpty = true;

        AddWaypoint(new Vector3(0f, -300f, 0f));

        ConfigSync = new ConfigSyncObject(this);

        TextToy = new TextToyObject(this, "Lobby");
        TextToy.Position = new Vector3(0f, -298f, 3.3f);
        TextToy.DisplaySize = new Vector2(150f, 25f);

        TextToy2 = new TextToyObject(this, "<color=red><size=3>Official server\n<color=green>Online</color></size></color>");
        TextToy2.Position = new Vector3(0f, -298f, -4f);
        TextToy2.Rotation = new Quaternion(0f, 180f, 0f, 0f);
        TextToy2.DisplaySize = new Vector2(150f, 50f);
    }

    public override void Update()
    {
        if (TextToy2 == null)
            return;

        if (_next < DateTime.Now)
        {
            _next = DateTime.Now.AddSeconds(0.1);

            TextToy2.Text = $"<size=5>{DateTime.Now.TimeOfDay.Ticks}</size>";

            foreach(BaseClient client in GetClientsSnapshot())
            {
                TextToy2.SendUpdate(client);
            }
        }
    }

    public override void OnLoad(BaseClient client)
    {
        client.SpawnPlayer();
    }

    public override void OnObjectsSpawned(BaseClient client)
    {
        client.SetRole(RoleTypeId.Tutorial);
        client.SetHealth(100f);
        client.SetSeed(350);

        client.Object.UserId = client.PreAuth.UserId;
        client.Object.Nickname = $"[{client.NetworkIdentityId}] Proxy";
        client.Object.SendUpdate(client);
    }
}
