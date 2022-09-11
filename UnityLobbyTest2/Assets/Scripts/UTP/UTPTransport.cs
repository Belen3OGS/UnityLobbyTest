using UnityEngine;
using System;
using Mirror;

public class UTPTransport : Transport
{
    private static RelayClient _client;
    private static RelayServer _server;

    #region TransportMethods
    public override bool Available()
    {
        return true;
    }

    public override void ClientConnect(string address)
    {
        _client.InitClient(address);
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
        return _server.Active;
    }

    public override void ServerDisconnect(int connectionId)
    {
        _server.Shutdown();
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
        _server.InitHost();
    }

    public override void ServerStop()
    {
        _server.Shutdown();
    }

    public override Uri ServerUri()
    {
        throw new NotImplementedException();
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

    #endregion
}