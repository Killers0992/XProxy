using System;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using XProxy.Core.Enums;
using XProxy.Plugin;
using XProxy.Plugin.Core;

public class MainClass
{
    public static MainClass Singleton;

    public static ServerStatus Status;

    [PluginConfig]
    public Config Config;

    [PluginEntryPoint("XProxy.Plugin", "1.0.0", "Plugin made for connecting to proxy making better exchange of information between server and proxy.", "Killers0992")]
    public void Entry()
    {
        Singleton = this;
        EventManager.RegisterAllEvents(this);
        UnityEngine.Object.FindObjectOfType<ServerStatic>().gameObject.AddComponent<ProxyConnection>();
    }

    [PluginEvent]
    public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
    {
        Status = ServerStatus.WaitingForPlayers;
    }

    [PluginEvent]
    public void OnWaitingForPlayers(RoundStartEvent ev)
    {
        Status = ServerStatus.RoundInProgress;
    }

    [PluginEvent]
    public void OnRoundEnd(RoundEndEvent ev)
    {
        Status = ServerStatus.RoundEnding;
    }

    [PluginEvent]
    public void OnRestart(RoundRestartEvent ev)
    {
        Status = ServerStatus.RoundRestart;
    }

    [PluginEvent]
    public void OnPlayerPreauth(PlayerPreauthEvent ev)
    {
        Log.Info($"Player {ev?.UserId} is trying to connect from {ev?.IpAddress}.");
        Log.Info($"ProxyIP: {Singleton.Config.ProxyIP}"); // Config.ProxyIP doesn't work for some reason
        Log.Info($"OnlyAllowProxyConnections: {Singleton.Config.OnlyAllowProxyConnections}");
        if (ev.IpAddress != Singleton.Config.ProxyIP && Singleton.Config.OnlyAllowProxyConnections)
        {
            Log.Info($"Player tried to connect from {ev.IpAddress} which is not proxy IP.");
            Server.BanPlayerByUserId(ev.UserId, "test", 0);
            string proxyRejectionMessage = Singleton.Config.RejectionMessage.Replace("%ProxyIP%", Singleton.Config.ProxyIP).Replace("%ProxyPort%", Singleton.Config.ProxyPort.ToString());
            CustomLiteNetLib4MirrorTransport.ProcessCancellationData(ev.ConnectionRequest, PreauthCancellationData.Reject(proxyRejectionMessage, true));

        }
    }
}