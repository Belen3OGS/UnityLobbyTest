using UnityEngine;
using System;
using Mirror;
using Multiplayer.RelayManagement;

namespace Multiplayer.MirrorCustom
{
    public class UTPTransport : Transport
    {
        [SerializeField] private RelayClient _client;
        [SerializeField] private RelayServer _server;
        [HideInInspector] public string joinCode;

        #region LoopMethods
        public override void ServerEarlyUpdate()
        {
            _server.HostEarlyUpdate();
        }
        public override void ServerLateUpdate()
        {
            _server.HostLateUpdate();
        }

        public override void ClientEarlyUpdate()
        {
            _client.ClientEarlyUpdate();
        }
        public override void ClientLateUpdate()
        {
            _client.ClientLateUpdate();
        }

        #endregion

        #region TransportMethods
        public override bool Available()
        {
            return true;
        }

        public override void ClientConnect(string address)
        {
            DontDestroyOnLoad(_client);
            InitRelayClientEvents();
            StartCoroutine(_client.InitClient(address));
        }

        public override bool ClientConnected()
        {
            return _client.IsClientConnected;
        }

        public override void ClientDisconnect()
        {
            _client.Shutdown();
        }

        public override void ClientSend(ArraySegment<byte> segment, int channelId = 0)
        {
            _client.SendToServer(segment, channelId);
        }

        public override int GetMaxPacketSize(int channelId = 0)
        {
            return _server.MaxPacketSize;
        }

        public override bool ServerActive()
        {
            return _server.IsRelayServerConnected;
        }

        public override void ServerDisconnect(int connectionId)
        {
            _server.DisconnectPlayer(connectionId);
        }

        public override string ServerGetClientAddress(int connectionId)
        {
            return connectionId.ToString();
        }

        public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = 0)
        {
            _server.SendToClient(connectionId, segment, channelId);
        }

        public override void ServerStart()
        {
            DontDestroyOnLoad(_server);
            InitRelayServerEvents();
            StartCoroutine(_server.InitHost());
        }

        public override void ServerStop()
        {
            _server.Shutdown();
        }

        public override Uri ServerUri()
        {
            if (_server.IsRelayServerConnected)
                return new Uri(_server.hostData.JoinCode, UriKind.Relative);
            else return null;
        }

        public override void Shutdown()
        {
            //TODO: Tambien para el cliente!!!!!!!!!
            if (_server != null)
                _server.Shutdown();
            if (_client != null)
                _client.Shutdown();
        }
        #endregion

        #region CustomMethods
        //CUSTOM METHODS
        private void OnRelayServerReady(string joinCode)
        {
            NetworkManager.singleton.networkAddress = joinCode;
            NetworkManager.singleton.OnStartHost();
        }

        private void InitRelayServerEvents()
        {
            _server.OnServerReady += OnRelayServerReady;
            _server.OnServerConnected += OnServerConnected;
            _server.OnServerDisconnected += OnServerDisconnected;
            _server.OnServerDataReceived += OnServerDataReceived;
            _server.OnServerDataSent += OnServerDataSent;
        }
        private void InitRelayClientEvents()
        {
            _client.OnConnected += OnClientConnected;
            _client.OnDataReceived += OnClientDataReceived;
            _client.OnDataSent += OnClientDataSent;
            _client.OnDisconnected += OnClientDisconnected;
        }
        #endregion

        private void OnDestroy()
        {
            if (_client != null)
                Destroy(_client.gameObject);
            if (_server != null)
                Destroy(_server.gameObject);
        }
    } 
}