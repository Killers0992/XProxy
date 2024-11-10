namespace XProxy.Shared.Models
{
    public class MessagesModel
    {
        public string ProxyVersion { get; set; } = "Running version (f=green)%version%(f=white), supported game version (f=green)%gameVersion%(f=white)";
        public string ProxyIsUpToDate { get; set; } = "Proxy is up to date!";
        public string ProxyIsOutdated { get; set; } = "Proxy is outdated, new version (f=green)%version%(f=white)";
        public string DownloadingUpdate { get; set; } = "Downloading update (f=green)%percentage%%(f=white)...";
        public string DownloadedUpdate { get; set; } = "Downloaded update!";
        public string PlayerTag { get; set; } = "(f=white)[(f=darkcyan)%serverIpPort%(f=white)] [(f=cyan)%server%(f=white)]";
        public string PlayerErrorTag { get; set; } = "(f=red)[(f=darkcyan)%serverIpPort%(f=red)] [(f=cyan)%server%(f=red)]";
        public string Proxy { get; set; } = "proxy";
        public string CurrentServer { get; set; } = "current server";
        public string ServerIsOfflineKickMessage { get; set; } = "Server %server% is offline!";
        public string ProxyStartedListeningMessage { get; set; } = "Listening on server (f=green)0.0.0.0:%port%(f=white), accepting clients with game version (f=green)%version%(f=white)";
        public string ProxyClientClosedConnectionMessage { get; set; } = "%tag% Client (f=green)%address%(f=white) ((f=green)%userid%(f=white)) closed connection.";
        public string ProxyClientDisconnectedWithReasonMessage { get; set; } = "%tag% Client (f=green)%address%(f=white) ((f=green)%userid%(f=white)) disconnected with reason (f=green)%reason%(f=white).";
        public string CommandRegisteredMessage { get; set; } = "Command (f=green)%name%(f=white) registered!";
        public string CommandAlreadyRegisteredMessage { get; set; } = "Command (f=green)%name%(f=yellow) is already registered!";
        public string CommandNotExistsMessage { get; set; } = "Unknown command %name%, type \"help\"";
        public string ConfigLoadedMessage { get; set; } = "Loaded config!";
        public string ConfigSavedMessage { get; set; } = "Saved config!";
        public string TokenLoadedMessage { get; set; } = "Verification token loaded! Server probably will be listed on public list.";
        public string TokenReloadedMessage { get; set; } = "Verification token reloaded.";
        public string CentralCommandMessage { get; set; } = "[(f=green)%command%(f=white)] %message%";
        public string VerificationChallengeObtainedMessage { get; set; } = "Verification challenge and response have been obtained.";
        public string FailedToUpdateMessage { get; set; } = "Could not update server data on server list - %error%";
        public string ReceivedTokenMessage { get; set; } = "Received verification token from central server.";
        public string MessageFromCentralsMessage { get; set; } = "[(f=green)MESSAGE FROM CENTRAL SERVER(f=white)] %message%";
        public string TokenSavedMessage { get; set; } = "New verification token saved.";
        public string TokenFailedToSaveMessage { get; set; } = "New verification token could not be saved: (f=green)%error%(f=red)";
        public string PasswordSavedMessage { get; set; } = "New password saved.";
        public string PasswordFailedToSaveMessage { get; set; } = "New password could not be saved.";
        public string CantUpdateDataMessage { get; set; } = "Could not update data on server list.";
        public string CantRefreshPublicKeyMessage { get; set; } = "Can't refresh central server public key - invalid signature!";
        public string CantRefreshPublicKey2Message { get; set; } = "Can't refresh central server public key - %message% %response%!";
        public string ObtainedPublicKeyMessage { get; set; } = "Downloaded public key from central server.";
        public string ServerListedMessage { get; set; } = "Server is listed on serverlist!";
        public string NoDependenciesToLoadMessage { get; set; } = "No dependencies to load.";
        public string LoadedAllDependenciesMesssage { get; set; } = "Successfully loaded all (f=green)%loaded%(f=white) dependencies!";
        public string PluginDisabledMessage { get; set; } = "[(f=darkcyan)%current%/%max%(f=white)] Plugin (f=green)%name%(f=white) is disabled!";
        public string PluginHasMissingDependenciesMessage { get; set; } = "[(f=darkcyan)%current%/%max%(f=white)] Plugin (f=green)%name%(f=white) failed to load, missing dependencies";
        public string PluginMissingDependencyMessage { get; set; } = "                                          - (f=green)%name%(f=white) %version%";
        public string PluginInvalidEntrypointMessage { get; set; } = "[(f=darkcyan)%current%/%max%(f=white)] Plugin (f=green)%name%(f=white) don't have any valid entrypoints!";
        public string PluginLoadedMessage { get; set; } = "[(f=darkcyan)%current%/%max%(f=white)] Plugin (f=green)%name%(f=white) loaded!";
        public string NoPluginsToLoadMessage { get; set; } = "No plugins to load.";
        public string LoadedAllPluginsMesssage { get; set; } = "Successfully loaded all (f=green)%loaded%(f=white) plugins!";
        public string PluginsLoadedAndFailedToLoadMessage { get; set; } = "Loaded (f=green)%loaded%(f=white) plugins and (f=red)%failed%(f=white) failed to load!";
        public string LoadedPublicKeyFromCache { get; set; } = "Loaded central server public key from cache.";
        public string DownloadPublicKeyFromCentrals { get; set; } = "Downloading public key from central server...";
        public string PreAuthIsInvalidMessage { get; set; } = $"Preauth is invalid for connection (f=green)%address%(f=yellow) failed on (f=green)%failed%(f=yellow)";
        public string MaintenanceDisconnectMessage { get; set; } = "Client (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server is (f=yellow)under maintenance(f=white)!";
        public string PlayerRedirectToMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) redirect to (f=green)%server%(f=white)!";
        public string PlayerRoundRestartMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) roundrestart, time %time%.";
        public string PlayerSentChallengeMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) sent (f=green)challenge(f=white)!";
        public string PlayerIsConnectingMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) is connecting...";
        public string PlayerConnectedMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) connected!";
        public string PlayerNetworkExceptionMessage { get; set; } = "%tag% Exception while updating network for client (f=green)%address%(f=red) ((f=green)%userid%(f=red)), %message%";
        public string PlayerUnbatchingExceptionMessage { get; set; } = "%tag% Exception while unbatching message for (f=green)%address%(f=red) ((f=green)%userid%(f=red)) from %condition%, %message%";
        public string PlayerExceptionSendToProxyMessage { get; set; } = "%tag% Exception while sending message to proxy for (f=green)%address%(f=red) ((f=green)%userid%(f=red)), %message%";
        public string PlayerExceptionSendToServerMessage { get; set; } = "%tag% Exception while sending message to current server for (f=green)%address%(f=red) ((f=green)%userid%(f=red)), %message%";
        public string PlayerServerIsOfflineMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server is offline!";
        public string PlayerDelayedConnectionMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) delayed for (f=green)%time%(f=white) seconds.";
        public string PlayerServerIsFullMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server is full.";
        public string PlayerBannedMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%useid%(f=white)) banned for (f=red)%reason%(f=white), expires (f=red)%date% %time%(f=white).";
        public string PlayerReceivedChallengeMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) received (f=green)challenge(f=white).";
        public string PlayerDisconnectedMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) disconnected, reason (f=green)%reason%(f=white)";
        public string PlayerDisconnectedWithReasonMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) disconnected, reason (f=green)%reason%(f=white)";
        public string PlayerServerTimeoutMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server timeout.";
        public string PlayerServerShutdownMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server shutdown.";
        public string LobbyConnectedMessage { get; set; } = "%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) connected to lobby!";
        public string LobbyConnectingToServerHint { get; set; } = "Connecting to <color=green>%server%</color>...";
        public string[] LobbyMainHint { get; set; } = new string[]
        {
            "<b>LOBBY</b>",
            "<color=orange>%proxyOnlinePlayers%</color>/<color=orange>%proxyMaxPlayers%</color>",
            "",
            "Servers",
            "%serversLine1%",
            "%serversLine2%",
            "",
            "You will join server <color=green>%server%</color>",
            "",
            "Press <color=orange>LeftAlt</color> to change server which you want to join, hold <color=orange><b>Q</b></color> to join."
        };
        public string LobbyServerLine1 { get; set; } = "<color=%selectedColor%>%server%</color>";
        public string SelectedServerColor { get; set; } = "green";
        public string DefaultServerColor { get; set; } = "white";
        public string LobbyServerLine2 { get; set; } = "<color=orange>%onlinePlayers%</color>/<color=orange>%maxPlayers%</color>";
        public string PositionInQueue { get; set; } = "Position <color=orange><b>%position%</b></color>/<color=orange>%totalInQueue%</color>";
        public string PriorityPositionInQueue { get; set; } = "PRIORITY Position <color=orange><b>%position%</b></color>/<color=orange>%totalInQueue%</color>";
        public string FirstPositionInQueue { get; set; } = "<color=green>Position</color> <color=orange><b>%position%</b></color>/<color=orange>%totalInQueue%</color>";
        public string LostConnectionHint { get; set; } = "<color=yellow><b>XProxy</b></color>\nServer is not responding for <color=green><b>%time%</b></color> seconds...";
        public string SearchingForFallbackServerHint { get; set; } = "<color=yellow><b>XProxy</b></color>\nSearching for fallback server...";
        public string OnlineServerNotFoundHint { get; set; } = "<color=yellow><b>XProxy</b></color>\n<color=red>Can't find any online servers, disconnecting...</color>";
        public string ConnectingToServerHint { get; set; } = "<color=yellow><b>XProxy</b></color>\nConnecting to server <color=green>%server%</color>...";
        public string MaintenanceKickMessage { get; set; } = "Server is under maintenance!";
        public string ProxyIsFull { get; set; } = "Client (f=green)%address%(f=white) ((f=green)%userid%(f=white)) proxy is full!";
        public string WrongVersion { get; set; } = "Client (f=green)%address%(f=white) ((f=green)%userid%(f=white)) tried joining proxy with wrong version (f=green)%version%(f=white)!";
    }
}
