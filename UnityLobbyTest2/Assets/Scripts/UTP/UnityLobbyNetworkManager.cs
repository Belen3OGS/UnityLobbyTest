using Mirror;
using System;
using UnityEngine;
using Multiplayer.LobbyManagement;

namespace Multiplayer.MirrorCustom
{
    public class UnityLobbyNetworkManager : NetworkManager
    {
        [SerializeField]
        private LobbyManager _lobbyManager;
        [SerializeField]
        private bool _privateServer;

        public override void Start()
        {
            base.Start();
            DontDestroyOnLoad(_lobbyManager);
            _lobbyManager.OnLobbyJoined += OnLobbyJoined;
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        public override void OnStopClient()
        {
            _lobbyManager.DisconnectFromLobby();
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            if (mode == NetworkManagerMode.ClientOnly) _lobbyManager.DisconnectFromLobby();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            _lobbyManager.StopLobby();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
        }

        public override void OnStartHost()
        {
            base.OnStartHost();
            if (!networkAddress.Equals("localhost"))
            {
                Uri uri = transport.ServerUri();
                if (uri != null) networkAddress = uri.OriginalString;
                else
                    Debug.LogError("URI was null");
                StartCoroutine(_lobbyManager.CreateLobby(networkAddress, maxConnections, _privateServer));
                Debug.Log("SERVER DOUBLE READY!!!!!!!!");
            }
        }

        private void OnLobbyJoined(string joinCode)
        {
            networkAddress = joinCode;
            StartClient();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if(_lobbyManager != null) Destroy(_lobbyManager.gameObject);
        }
    }
}