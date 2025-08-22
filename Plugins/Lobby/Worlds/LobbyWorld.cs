using Lobby.Models;
using PlayerRoles;
using Portals.Core;
using UnityEngine;
using XProxy.Core;
using XProxy.Networking;
using XProxy.Objects;
using XProxy.Responses;
using Logger = XProxy.Misc.Logger;

namespace Lobby.Worlds;

public class LobbyWorld : World
{
    public ConfigSyncObject ConfigSync;
    public WaypointObject Waypoint;
    public TextToyObject TextToy;
    public TextToyObject TextToy2;

    public LobbyWorld() : base("Lobby")
    {
        DestroyOnEmpty = true;

        AddWaypoint(new Vector3(0f, -300f, 0f));

        ConfigSync = new ConfigSyncObject(this);

        foreach(TextInfo text in MainClass.Singleton.Config.Texts)
        {
            TextToyObject textObject = new TextToyObject(this, text.Text);
            textObject.Position = new Vector3(text.PositionX, text.PositionY, text.PositionZ);
            textObject.DisplaySize = new Vector2(150f, 25f);
        }

        foreach(PortalInfo portal in MainClass.Singleton.Config.Portals)
        {
            new Portal(this, portal.TargetServer, portal.Text, new Vector3(portal.PositionX, portal.PositionY, portal.PositionZ), new Quaternion(0f, portal.Rotation, 0f, 0f));
        }
    }

    public override void Update()
    {
        PortalController.Update(this);
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
        client.Object.SendUpdate(client);
    }

    public override void OnDestroy()
    {
        Portal.SpawnedPortals.Remove(this);
    }
}
