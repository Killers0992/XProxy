using LiteNetLib;
using LiteNetLib.Utils;
using PluginAPI.Core;
using System;
using UnityEngine;

namespace XProxy.Plugin.Core
{
    public class ProxyConnection : MonoBehaviour
    {
        private NetManager _manager;
        private EventBasedNetListener _listener;

        private bool _isConnecting;
        private DateTime _nextConnectiobRetry = DateTime.Now;

        void Awake()
        {
            _listener = new EventBasedNetListener();

            _listener.PeerDisconnectedEvent += OnDisconnected;
            _listener.PeerConnectedEvent += OnConnected;

            _manager = new NetManager(_listener);
            _manager.Start();
        }

        void Update()
        {
            if (_manager == null)
                return;

            if (_manager.IsRunning)
                _manager.PollEvents();

            if (!_isConnecting)
            {
                if (_nextConnectiobRetry < DateTime.Now)
                {
                    NetDataWriter writer = new NetDataWriter();

                    // ClientType Proxy
                    writer.Put((byte)2);
                    writer.Put(MainClass.Singleton.Config.ConnectionKey);
                    writer.Put(Server.Port);

                    Log.Info($"Connecting to proxy {MainClass.Singleton.Config.ProxyIP}:{MainClass.Singleton.Config.ProxyPort}...", "XProxy");
                    _manager.Connect(MainClass.Singleton.Config.ProxyIP, MainClass.Singleton.Config.ProxyPort, writer);
                    _isConnecting = true;
                }
            }
        }

        void OnConnected(NetPeer peer)
        {
            Log.Info($"Connected!", "XProxy");
        }

        void OnDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _nextConnectiobRetry = DateTime.Now.AddSeconds(10);
            _isConnecting = false;
            Log.Info($"Disconnected from proxy with reason {disconnectInfo.Reason}, reconnecting in 10 seconds...", "XProxy");
        }
    }
}
