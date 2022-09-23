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
        DontDestroyOnLoad(lobbyManager);
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

    public override void OnStopClient()
    {
        lobbyManager.DisconnectFromLobby();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        if (mode == NetworkManagerMode.ClientOnly)
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
                networkAddress = uri.OriginalString;
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

    public override void OnDestroy()
    {
        base.OnDestroy();
        if(lobbyManager != null)
            Destroy(lobbyManager.gameObject);
    }
}