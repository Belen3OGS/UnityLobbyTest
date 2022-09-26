using UnityEngine;
using System;
using Mirror;
using Multiplayer.RelayManagement;

namespace Multiplayer.MirrorCustom
{
    public class UTPTransport : Transport
    {
        [HideInInspector]
        public string joinCode;

        [SerializeField]
        private RelayClient _client;
        [SerializeField]
        private RelayServer _server;

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
            _server.DisconnectPlayer(TranslateToRelayId(connectionId));
        }

        public override string ServerGetClientAddress(int connectionId)
        {
            return connectionId.ToString();
        }

        public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = 0)
        {
            _server.SendToClient(TranslateToRelayId(connectionId), segment, channelId);
        }

        public override void ServerStart()
        {
            DontDestroyOnLoad(_server);
            InitRelayServerEvents();
            StartCoroutine(_server.InitHost(NetworkManager.singleton.maxConnections));
        }

        public override void ServerStop()
        {
            _server.Shutdown();
        }

        public override Uri ServerUri()
        {
            if (_server.IsRelayServerConnected)
                return new Uri(_server.HostData.JoinCode, UriKind.Relative);
            else return null;
        }

        public override void Shutdown()
        {
            if (_client != null)
                _client.Shutdown();
            if (_server != null)
                _server.Shutdown();
        }
        #endregion

        #region CustomMethods
        //ID TRANSLATION
        private int TranslateFromRelayId(int relayId)
        {
            return relayId + 1;
        }

        private int TranslateToRelayId(int connectionId)
        {
            return (connectionId - 1);
        }

        //INTERNAL EVENTS
        private void OnRelayServerReady(string joinCode)
        {
            NetworkManager.singleton.networkAddress = joinCode;
            NetworkManager.singleton.OnStartHost();
        }

        private void OnServerConnectedInternal(int relayId)
        {
            OnServerConnected?.Invoke(TranslateFromRelayId(relayId));
        }

        private void OnServerDisconnectedInternal(int relayId)
        {
            OnServerDisconnected?.Invoke(TranslateFromRelayId(relayId));
        }

        private void OnServerDataReceivedInternal(int relayId, ArraySegment<byte> data, int channelId)
        {
            OnServerDataReceived?.Invoke(TranslateFromRelayId(relayId), data, channelId);
        }

        private void OnServerDataSentInternal(int connectionId, ArraySegment<byte> data, int channelId)
        {
            OnServerDataSent?.Invoke(TranslateFromRelayId(connectionId), data, channelId);
        }

        private void InitRelayServerEvents()
        {
            _server.OnServerReady += OnRelayServerReady;
            _server.OnServerConnected += OnServerConnectedInternal;
            _server.OnServerDisconnected += OnServerDisconnectedInternal;
            _server.OnServerDataReceived += OnServerDataReceivedInternal;
            _server.OnServerDataSent += OnServerDataSentInternal;
        }

        private void InitRelayClientEvents()
        {
            _client.OnConnected += OnClientConnected;
            _client.OnDataReceived += OnClientDataReceived;
            _client.OnDataSent += OnClientDataSent;
            _client.OnDisconnected += OnClientDisconnected;
        }
        #endregion

        //DISPOSAL
        private void OnDestroy()
        {
            if (_client != null)
                Destroy(_client.gameObject);
            if (_server != null)
                Destroy(_server.gameObject);
        }
    } 
}