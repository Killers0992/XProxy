using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using System;
using XProxy.Core.Enums;
using XProxy.Plugin;
using XProxy.Plugin.Core;

public class MainClass : Plugin<Config>
{
    public static MainClass Singleton;

    public static ServerStatus Status;

    public override string Name { get; } = "XProxy.Plugin";

    public override string Description { get; } = "Plugin made for connecting to proxy making better exchange of information between server and proxy.";

    public override string Author { get; } = "Killers0992";

    public override Version Version { get; } = new Version(1, 0, 0);

    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

    public override void Enable()
    {
        Singleton = this;
        UnityEngine.Object.FindObjectOfType<ServerStatic>().gameObject.AddComponent<ProxyConnection>();

        ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
        ServerEvents.RoundStarted += OnRoundStarted;
        ServerEvents.RoundEnded += OnRoundEnded;
        ServerEvents.RoundRestarted += OnRoundRestarted;
        PlayerEvents.PreAuthenticating += OnPreAuth;
    }

    private void OnPreAuth(PlayerPreAuthenticatingEventArgs ev)
    {
        Logger.Info($"Player {ev?.UserId} is trying to connect from {ev?.IpAddress}.");
        Logger.Info($"ProxyIP: {Singleton.Config.ProxyIP}"); // Config.ProxyIP doesn't work for some reason
        Logger.Info($"OnlyAllowProxyConnections: {Singleton.Config.OnlyAllowProxyConnections}");

        if (ev.IpAddress != Singleton.Config.ProxyIP && Singleton.Config.OnlyAllowProxyConnections)
        {
            string proxyRejectionMessage = Singleton.Config.RejectionMessage.Replace("%ProxyIP%", Singleton.Config.ProxyIP).Replace("%ProxyPort%", Singleton.Config.ProxyPort.ToString());
            
            Logger.Info($"Player tried to connect from {ev.IpAddress} which is not proxy IP.");
            ev.RejectCustom(proxyRejectionMessage);
            return;
        }
    }

    private void OnRoundRestarted() => Status = ServerStatus.RoundRestart;

    private void OnRoundEnded(RoundEndedEventArgs ev) => Status = ServerStatus.RoundEnding;

    private void OnRoundStarted() => Status = ServerStatus.RoundInProgress;

    private void OnWaitingForPlayers() => Status = ServerStatus.WaitingForPlayers;

    public override void Disable()
    {
        ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
        ServerEvents.RoundStarted -= OnRoundStarted;
        ServerEvents.RoundEnded -= OnRoundEnded;
        ServerEvents.RoundRestarted -= OnRoundRestarted;
        PlayerEvents.PreAuthenticating -= OnPreAuth;
    }
}