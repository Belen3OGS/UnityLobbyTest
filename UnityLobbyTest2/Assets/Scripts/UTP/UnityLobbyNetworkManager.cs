using Mirror;
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

    public override void OnStartHost()
    {
        base.OnStartHost();
        lobbyManager.CreateLobby(networkAddress, maxConnections, privateServer);
    }

    private void OnLobbyJoined(string joinCode)
    {
        networkAddress = joinCode;
        StartClient();
    }
}