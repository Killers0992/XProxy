![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/Killers0992/XProxy/total?label=Downloads&labelColor=2e343e&color=00FFFF&style=for-the-badge)
[![Discord](https://img.shields.io/discord/1216429195232673964?label=Discord&labelColor=2e343e&color=00FFFF&style=for-the-badge)](https://discord.gg/czQCAsDMHa)
# XProxy
Proxy for SCP: Secret Laboratory allowing you to link multiple servers into one!

Features
- Virtual lobby - Allowing you to select specific server which you want to connect to.
- Queue system - If server is full you will be added to queue and when system detects if theres free slot and then you will be connected.
- Fallback system - Server shutdowns or timeouts without any reason you will be connected to available server.

**Pterodactyl Egg**
[proxy.json](https://github.com/Killers0992/XProxy/blob/master/Storage/egg-s-c-p--s-l-proxy.json)

# Setup
1. Depending if you use linux or windows download proper installer
   - [Windows x64](https://github.com/Killers0992/XProxy/releases/latest/download/XProxy.exe)
   - [Linux x64](https://github.com/Killers0992/XProxy/releases/latest/download/XProxy)

2. Make sure you have Dotnet 8.0 Runtime installed
   - [Windows](https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=x64&rid=win10-x64&apphost_version=8.0.0)
   - [Linux](https://learn.microsoft.com/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website)

3. Run proxy by using **XProxy.exe** on windows or **XProxy** on linux.
4. Configure **config.yml** inside **Data** folder, if you have verkey you need to create **verkey.txt** and put verkey here.
5. Every server under proxy needs to be hidden on serverlist **CENTRAL COMMAND** !private and **config_gameplay.txt** needs to have 
```yaml
enable_ip_ratelimit: false

enable_proxy_ip_passthrough: true
trusted_proxies_ip_addresses:
 - <IP OF YOUR PROXY>
```
  - Replace ``<IP OF YOUR PROXY>`` with public ip if your proxy is not running on same machine or with local ip, if you dont know which ip to set just before adding ip just connect to server via proxy and check console.

# Placeholders
These placeholders can be used in lobby hint or server name.

| Placeholder | Desc |
| ------------- | ------------- |
| ``%playersInQueue_<server>%`` | Shows amount of players in queue to specific server. |
| ``%onlinePlayers_<server>%`` | Shows amount of online players on specific server. |
| ``%maxPlayers_<server>%`` | Shows amount of maximum players on specific  |
| ``%proxyOnlinePlayers%`` | Shows total amount of connected players to proxy. |
| ``%proxyMaxPlayers_<listenerName>%`` | Shows maximum amount of player which can connect to specific listener. |

# FAQ
- If you see logs like that ![image](https://github.com/Killers0992/XProxy/assets/38152961/0e7c4374-021a-4618-bb2e-b268286fd3cf) this means your console is not supporting ANSI colors !
- 
  Inside ``config.yml`` change ``AnsiColors`` to ``false`` !

# Console Commands
| Command  | Arguments | Description |
| ------------- | ------------- | ------------- |
| servers  |   |  Shows all servers.  |
| players  |   |  Shows players playing on servers.  |
| listeners |  | Shows all listeners. |
| send | ``all/serverName/id@steam`` ``serverName``  |  Sends all players, entire population of a server or target to specific server.  |
| maintenance toggle  |  |  Toggles maintenance |
| maintenance servername  | ``name`` |  Changes server name set when maintenance is enabled.  |
| reload  |   |  Reloads configs.  |
| sendhint  | ``message``  |  Sends hint to all players.  |
| broadcast  | ``time`` ``message`` | Sends broadcast to all players.  |
| runcentralcmd | ``listenername`` ``command`` | Runs central command on specific listener. |

# Lobby Commands
| Command  | Arguments | Description |
| ------------- | ------------- | ------------- |
| .connect  | ``serverName``  |  Connects you to specific server.  |
| .servers  |   | Shows all servers. |

# Queue Commands
| Command  | Arguments | Description |
| ------------- | ------------- | ------------- |
| .hub / .lobby  |   |  Sends you back to lobby.  |
