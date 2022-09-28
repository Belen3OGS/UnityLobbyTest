using Mirror;
using System;
using UnityEngine;
using Multiplayer.LobbyManagement;
using System.Collections.Generic;

namespace Multiplayer.MirrorCustom
{
    public class UnityLobbyNetworkManager : NetworkManager
    {
        [SerializeField]
        private LobbyManager _lobbyManager;
        [SerializeField]
        private bool _privateServer;

        private Dictionary<int, string> _connectionIdToLobbyId = new Dictionary<int, string>();

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
            AddToLobbyIdsDictionary(conn);
        }

        public async void AddToLobbyIdsDictionary(NetworkConnectionToClient conn)
        {
            string newPlayerLobbyId;
            if (mode == NetworkManagerMode.ServerOnly || conn.connectionId != 0)
            {
                newPlayerLobbyId = await _lobbyManager.GetLastJoinedPlayerId();
            }
            else newPlayerLobbyId = "HOST";
            _connectionIdToLobbyId.Add(conn.connectionId, newPlayerLobbyId);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            string lobbyId = _connectionIdToLobbyId[conn.connectionId];
            if(!lobbyId.Equals("HOST")) _lobbyManager.DisconnectPlayer(lobbyId);
            _connectionIdToLobbyId.Remove(conn.connectionId);
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