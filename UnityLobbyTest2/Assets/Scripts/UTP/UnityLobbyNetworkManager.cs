using Mirror;
using System;
using UnityEngine;

public class UnityLobbyNetworkManager : NetworkManager
{
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private bool privateServer;

    public override void Start()
    {
        base.Start();
        lobbyManager.OnLobbyJoined += OnLobbyJoined;
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        lobbyManager.DisconnectFromLobby();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        lobbyManager.StopLobby();
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
            if (uri != null)
                networkAddress = uri.Host;
            else
                Debug.LogError("URI was null");
            StartCoroutine(lobbyManager.CreateLobby(networkAddress, maxConnections, privateServer));
            Debug.Log("SERVER DOUBLE READY!!!!!!!!");
        }
    }

    private void OnLobbyJoined(string joinCode)
    {
        networkAddress = joinCode;
        StartClient();
    }


}