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
1. Depending if you use linux or windows download proper build
   - [Windows x64](https://github.com/Killers0992/XProxy/releases/latest/download/XProxy_win64.zip)
   - [Linux x64](https://github.com/Killers0992/XProxy/releases/latest/download/XProxy_linux64.zip)
2. Run proxy by using **XProxy.exe** on windows or **XProxy** on linux.
3. Configure **config.yml**
4. Every server under proxy needs to be hidden on serverlist **CENTRAL COMMAND** !private and **config_gameplay.txt** needs to have 
```yaml
enable_proxy_ip_passthrough: true
trusted_proxies_ip_addresses:
 - <IP OF YOUR PROXY>
```
  - Replace ``<IP OF YOUR PROXY>`` with public ip if your proxy is not running on same machine or with local ip, if you dont know which ip to set just before adding ip just connect to server via proxy and check console.

# Console Commands
| Command  | Arguments | Description |
| ------------- | ------------- | ------------- |
| servers  |   |  Sends you back to lobby.  |
| players  |   |  Shows players playing on servers.  |
| send | ``all/id@steam``  |  Sends all players or target to specific server.  |
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
```
