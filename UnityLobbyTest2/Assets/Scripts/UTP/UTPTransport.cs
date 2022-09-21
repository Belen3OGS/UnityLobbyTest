using UnityEngine;
using System;
using Mirror;

public class UTPTransport : Transport
{
    [SerializeField] private RelayClient _client;
    [SerializeField] private RelayServer _server;
    [HideInInspector] public string joinCode;

    private void Awake()
    {
        _server.OnServerReady += OnRelayServerReady;
    }

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
        
        StartCoroutine(_client.InitClient(address));
    }

    public override bool ClientConnected()
    {
        return _client.connected;
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
        StartCoroutine(_server.InitHost());
    }

    public override void ServerStop()
    {
        _server.Shutdown();
    }

    public override Uri ServerUri()
    {
        if (_server.IsRelayServerConnected)
            return new Uri.;
        else return null;
    }

    public override void Shutdown()
    {
        //TODO: Tambien para el cliente!!!!!!!!!
        if(_server != null)
            _server.Shutdown();
        if(_client != null)
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
    #endregion
}