![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/Killers0992/XProxy/total?label=Downloads&labelColor=2e343e&color=00FFFF&style=for-the-badge)
[![Discord](https://img.shields.io/discord/1216429195232673964?label=Discord&labelColor=2e343e&color=00FFFF&style=for-the-badge)](https://discord.gg/czQCAsDMHa)
# XProxy
Proxy for SCP: Secret Laboratory allowing you to link multiple servers into one!

Features
- Virtual lobby - Allowing you to select specific server which you want to connect to.
- Queue system - If server is full you will be added to queue and when system detects if theres free slot and then you will be connected.
- Fallback system - Server shutdowns or timeouts without any reason you will be connected to available server.

[Builds API](https://killers0992.github.io/XProxy/builds.json)

# Setup
1. Depending if you use linux or windows download proper installer
   - [Windows x64](https://github.com/Killers0992/XProxy/releases/latest/download/XProxy.exe)
   - [Linux x64](https://github.com/Killers0992/XProxy/releases/latest/download/XProxy)
2. Run proxy by using **XProxy.exe** on windows or **XProxy** on linux.
3. Configure **config.yml** inside **Data** folder, if you have verkey you need to create **verkey.txt** and put verkey here.
4. Every server under proxy needs to be hidden on serverlist **CENTRAL COMMAND** !private and **config_gameplay.txt** needs to have 
```yaml
enable_proxy_ip_passthrough: true
trusted_proxies_ip_addresses:
 - <IP OF YOUR PROXY>
```
  - Replace ``<IP OF YOUR PROXY>`` with public ip if your proxy is not running on same machine or with local ip, if you dont know which ip to set just before adding ip just connect to server via proxy and check console.
  - 
# FAQ
- If you see logs like that ![image](https://github.com/Killers0992/XProxy/assets/38152961/0e7c4374-021a-4618-bb2e-b268286fd3cf) this means your console is not supporting ANSI colors !
- 
  Inside ``config_patcher.yml`` change ``AnsiColors`` to ``false`` !

- Make sure to set proper game version in ``config_patcher.yml`` or ``config.yml`` because auto updater will be not downloading proper builds and you will be not able to connect to this server.

# Console Commands
| Command  | Arguments | Description |
| ------------- | ------------- | ------------- |
| servers  |   |  Sends you back to lobby.  |
| players  |   |  Shows players playing on servers.  |
| send | ``all/id@steam`` ``serverName``  |  Sends all players or target to specific server.  |
| maintenance toggle  |  |  Toggles maintenance |
| maintenance servername  | ``name`` |  Changes server name set when maintenance is enabled.  |
| reload  |   |  Reloads configs.  |
| sendhint  | ``message``  |  Sends hint to all players.  |
| broadcast  | ``message`` | Sends broadcast to all players.  |

# Lobby Commands
| Command  | Arguments | Description |
| ------------- | ------------- | ------------- |
| .connect  | ``serverName``  |  Connects you to specific server.  |
| .servers  |   | Shows all servers. |

# Queue Commands
| Command  | Arguments | Description |
| ------------- | ------------- | ------------- |
| .hub / .lobby  |   |  Sends you back to lobby.  |

# Default config
```yaml
# Enables debug logs.
debug: false
# Language of messages.
langauge: 'en'
# Port which proxy will use to listen for connections.
port: 7777
# Email used for listing your server on SCP SL serverlist.
email: 'example@gmail.com'
# Server name.
serverName: 'Example server name.'
# Server information.
pastebin: 'm4DqS5r0'
# Version of game.
gameVersion: '13.4.2'
# Maximum amount of players which can connect to your proxy.
maxPlayers: 50
# Priority servers used for first connection and fallback servers.
priorities:
- 'lobby'
# Available servers.
servers:
  'lobby':
    # Name of server.
    name: 'Lobby'
    # IP Address of target server.
    ip: '127.0.0.1'
    # Port of target server.
    port: 7777
    # Maximum amount of players which can connect to server.
    maxPlayers: 50
    # Connection type set to Proxied will connect players to specific server, simulation needs to have Simulation set to specific type example lobby
    connectionType: 'Simulated'
    # Simulation set when player connects to server, plugins can register custom ones and you need to specify type here.
    simulation: 'lobby'
    # PreAuth will contain IP Address of client and target server will set this ip address to that one only if enable_proxy_ip_passthrough is set to true and trusted_proxies_ip_addresses has your proxy ip!
    sendIpAddressInPreAuth: false
  'vanilla':
    # Name of server.
    name: 'Vanilla'
    # IP Address of target server.
    ip: '127.0.0.1'
    # Port of target server.
    port: 7778
    # Maximum amount of players which can connect to server.
    maxPlayers: 20
    # Connection type set to Proxied will connect players to specific server, simulation needs to have Simulation set to specific type example lobby
    connectionType: 'Proxied'
    # Simulation set when player connects to server, plugins can register custom ones and you need to specify type here.
    simulation: '-'
    # PreAuth will contain IP Address of client and target server will set this ip address to that one only if enable_proxy_ip_passthrough is set to true and trusted_proxies_ip_addresses has your proxy ip!
    sendIpAddressInPreAuth: true
# User permissions
users:
  'admin@admin':
    # If player can join when maintenance is enabled.
    ignoreMaintenance: true
# If maintenance mode is enabled.
maintenanceMode: false
# Name of server visbile on serverlist when maintenance mode is enabled.
maintenanceServerName: 'Maintenance mode'
# Auto updates proxy if needed.
autoUpdater: true
```

Default language ( you can submit translations via issues )
```yaml
proxyVersion: 'Running version (f=green)%version%(f=white) for game version (f=green)%gameVersion%(f=white)'
proxyIsUpToDate: 'Proxy is up to date!'
proxyIsOutdated: 'Proxy is outdated, new version (f=green)%version%(f=white)'
downloadingUpdate: 'Downloading update (f=green)%percentage%%(f=white)...'
downloadedUpdate: 'Downloaded update!'
playerTag: '(f=white)[(f=darkcyan)%serverIpPort%(f=white)] [(f=cyan)%server%(f=white)]'
playerErrorTag: '(f=red)[(f=darkcyan)%serverIpPort%(f=red)] [(f=cyan)%server%(f=red)]'
proxy: 'proxy'
currentServer: 'current server'
serverIsOfflineKickMessage: 'Server %server% is offline!'
proxyStartedListeningMessage: 'Listening on server (f=green)0.0.0.0:%port%(f=white)'
proxyClientClosedConnectionMessage: '%tag% Client (f=green)%address%(f=white) ((f=green)%userid%(f=white)) closed connection."'
proxyClientDisconnectedWithReasonMessage: '%tag% Client (f=green)%address%(f=white) ((f=green)%userid%(f=white)) disconnected with reason (f=green)%reason%(f=white).'
commandRegisteredMessage: 'Command (f=green)%name%(f=white) registered!'
commandAlreadyRegisteredMessage: 'Command (f=green)%name%(f=yellow) is already registered!'
commandNotExistsMessage: 'Unknown command %name%, type "help"'
configLoadedMessage: 'Loaded config!'
configSavedMessage: 'Saved config!'
tokenLoadedMessage: 'Verification token loaded! Server probably will be listed on public list.'
tokenReloadedMessage: 'Verification token reloaded.'
centralCommandMessage: '[(f=green)%command%(f=white)] %message%'
verificationChallengeObtainedMessage: 'Verification challenge and response have been obtained.'
failedToUpdateMessage: 'Could not update server data on server list - %error%'
receivedTokenMessage: 'Received verification token from central server.'
messageFromCentralsMessage: '[(f=green)MESSAGE FROM CENTRAL SERVER(f=white)] %message%'
tokenSavedMessage: 'New verification token saved.'
tokenFailedToSaveMessage: 'New verification token could not be saved: (f=green)%error%(f=red)'
passwordSavedMessage: 'New password saved.'
passwordFailedToSaveMessage: 'New password could not be saved.'
cantUpdateDataMessage: 'Could not update data on server list.'
cantRefreshPublicKeyMessage: 'Can''t refresh central server public key - invalid signature!'
cantRefreshPublicKey2Message: 'Can''t refresh central server public key - %message% %response%!'
obtainedPublicKeyMessage: 'Downloaded public key from central server.'
serverListedMessage: 'Server is listed on serverlist!'
noDependenciesToLoadMessage: 'No dependencies to load.'
loadedAllDependenciesMesssage: 'Successfully loaded all (f=green)%loaded%(f=white) dependencies!'
pluginDisabledMessage: '[(f=darkcyan)%current%/%max%(f=white)] Plugin (f=green)%name%(f=white) is disabled!'
pluginHasMissingDependenciesMessage: '[(f=darkcyan)%current%/%max%(f=white)] Plugin (f=green)%name%(f=white) failed to load, missing dependencies'
pluginMissingDependencyMessage: '                                          - (f=green)%name%(f=white) %version%'
pluginInvalidEntrypointMessage: '[(f=darkcyan)%current%/%max%(f=white)] Plugin (f=green)%name%(f=white) don''t have any valid entrypoints!'
pluginLoadedMessage: '[(f=darkcyan)%current%/%max%(f=white)] Plugin (f=green)%name%(f=white) loaded!'
noPluginsToLoadMessage: 'No plugins to load.'
loadedAllPluginsMesssage: 'Successfully loaded all (f=green)%loaded%(f=white) plugins!'
pluginsLoadedAndFailedToLoadMessage: 'Loaded (f=green)%loaded%(f=white) plugins and (f=red)%failed%(f=white) failed to load!'
loadedPublicKeyFromCache: 'Loaded central server public key from cache.'
downloadPublicKeyFromCentrals: 'Downloading public key from central server...'
preAuthIsInvalidMessage: 'Preauth is invalid for connection (f=green)%address%(f=yellow) failed on (f=green)%failed%(f=yellow)'
maintenanceDisconnectMessage: 'Client (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server is (f=yellow)under maintenance(f=white)!'
playerRedirectToMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) redirect to (f=green)%server%(f=white)!'
playerRoundRestartMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) roundrestart, time %time%.'
playerSentChallengeMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) sent (f=green)challenge(f=white)!'
playerIsConnectingMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) is connecting...'
playerConnectedMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) connected!'
playerNetworkExceptionMessage: '%tag% Exception while updating network for client (f=green)%address%(f=red) ((f=green)%userid%(f=red)), %message%'
playerUnbatchingExceptionMessage: '%tag% Exception while unbatching message for (f=green)%address%(f=red) ((f=green)%userid%(f=red)) from %condition%, %message%'
playerExceptionSendToProxyMessage: '%tag% Exception while sending message to proxy for (f=green)%address%(f=red) ((f=green)%userid%(f=red)), %message%'
playerExceptionSendToServerMessage: '%tag% Exception while sending message to current server for (f=green)%address%(f=red) ((f=green)%userid%(f=red)), %message%'
playerServerIsOfflineMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server is offline!'
playerDelayedConnectionMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) delayed for (f=green)%time%(f=white) seconds.'
playerServerIsFullMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server is full.'
playerBannedMessage: '%tag% (f=green)%address%(f=white) ((f=green)%useid%(f=white)) banned for (f=red)%reason%(f=white), expires (f=red)%date% %time%(f=white).'
playerReceivedChallengeMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) received (f=green)challenge(f=white).'
playerDisconnectedMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) disconnected, reason (f=green)%reason%(f=white)'
playerDisconnectedWithReasonMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) disconnected, reason (f=green)%reason%(f=white)'
playerServerTimeoutMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server timeout.'
playerServerShutdownMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) server shutdown.'
lobbyConnectedMessage: '%tag% (f=green)%address%(f=white) ((f=green)%userid%(f=white)) connected to lobby!'
lobbyConnectingToServerHint: 'Connecting to <color=green>%server%</color>...'
lobbyMainHint:
- '<b>LOBBY</b>'
- '<color=orange>%proxyOnlinePlayers%</color>/<color=orange>%proxyMaxPlayers%</color>'
- ''
- 'Servers'
- '%serversLine1%'
- '%serversLine2%'
- ''
- 'You will join server <color=green>%server%</color>'
- ''
- 'Press <color=orange>Q</color> to change server which you want to join, hold <color=orange><b>Q</b></color> to join.'
lobbyServerLine1: '<color=%selectedColor%>%server%</color>'
selectedServerColor: 'green'
defaultServerColor: 'white'
lobbyServerLine2: '<color=orange>%onlinePlayers%</color>/<color=orange>%maxPlayers%</color>'
positionInQueue: 'Position <color=orange><b>%position%</b></color>/<color=orange>%totalInQueue%</color>'
firstPositionInQueue: '<color=green>Position</color> <color=orange><b>%position%</b></color>/<color=orange>%totalInQueue%</color>'
lostConnectionHint: '<color=yellow><b>XProxy</b></color>

  Server is not responding for <color=green><b>%time%</b></color> seconds...'
searchingForFallbackServerHint: '<color=yellow><b>XProxy</b></color>

  Searching for fallback server...'
onlineServerNotFoundHint: '<color=yellow><b>XProxy</b></color>

  <color=red>Can''t find any online servers, disconnecting...</color>'
connectingToServerHint: '<color=yellow><b>XProxy</b></color>

  Connecting to server <color=green>%server%</color>...'
maintenanceKickMessage: 'Server is under maintenance!'
```
