using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace XProxy.Plugin.Core
{
    public class ProxyConnection : UnityEngine.MonoBehaviour
    {
        private NetManager _manager;
        private EventBasedNetListener _listener;

        private bool _isConnecting;
        private DateTime _nextConnectiobRetry = DateTime.Now;
        private DateTime _nextStatusUpdate = DateTime.MinValue; 
        void Awake()
        {
            _listener = new EventBasedNetListener();

            _listener.PeerDisconnectedEvent += OnDisconnected;
            _listener.PeerConnectedEvent += OnConnected;

            _manager = new NetManager(_listener);
            _manager.Start();
        }

        void SendIntialData()
        {
            SendStatus();
        }

        void SendStatus()
        {
            if (!_manager.IsRunning || _manager.FirstPeer == null)
                return;

            NetDataWriter wr = new NetDataWriter();
            wr.Put((byte)0);
            wr.Put((byte)MainClass.Status);

            _manager.FirstPeer.Send(wr, DeliveryMethod.ReliableOrdered);
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

                    Logger.Info($"Connecting to proxy {MainClass.Singleton.Config.ProxyIP}:{MainClass.Singleton.Config.ProxyPort}...");
                    _manager.Connect(MainClass.Singleton.Config.ProxyIP, MainClass.Singleton.Config.ProxyPort, writer);
                    _isConnecting = true;
                }
            }
            
            if (_manager.FirstPeer != null && DateTime.Now >= _nextStatusUpdate)
            {
                SendStatus();
                _nextStatusUpdate = DateTime.Now.AddSeconds(10); 
            }
        }


        void OnConnected(NetPeer peer)
        {
            Logger.Info($"Connected!");
            SendIntialData();
        }

        void OnDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _nextConnectiobRetry = DateTime.Now.AddSeconds(5);
            _isConnecting = false;
            Logger.Info($"Disconnected from proxy with reason {disconnectInfo.Reason}, reconnecting in 5 seconds...");
        }
    }
}
