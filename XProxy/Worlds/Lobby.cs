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

        TextToy = new TextToyObject(this, $"<size=3><color=red><b>Kings Playground");
        TextToy.Position = new Vector3(0f, -298f, 3.3f);
        TextToy.DisplaySize = new Vector2(150f, 25f);

        TextToy2 = new TextToyObject(this, "<color=red><size=3>Official server\n<color=green>Online</color></size></color>");
        TextToy2.Position = new Vector3(0f, -298f, -4f);
        TextToy2.Rotation = new Quaternion(0f, 180f, 0f, 0f);
        TextToy2.DisplaySize = new Vector2(150f, 50f);
    }

    Vector3 Portal1 = new Vector3(0f, -298f, -4f);

    public override void Update()
    {
        if (TextToy2 == null)
            return;

        if (_next < DateTime.Now)
        {
            _next = DateTime.Now.AddSeconds(0.1);

            foreach(BaseClient client in GetClientsSnapshot())
            {
                float distance = Vector3.Distance(GetPosition(client), Portal1);

                if (distance < 1.5f)
                {
                    client.Connect<Server>(Server.Get<Server>("official1"));
                }

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
