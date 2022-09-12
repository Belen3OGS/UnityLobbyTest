using UnityEngine;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System;
using Unity.Networking.Transport;
using System.Collections;

public class RelayClient : MonoBehaviour
{
    RelayServerData relayServerData;
    public RelayHelper.RelayJoinData joinData;
    public JoinAllocation JoinAllocation { get; private set; }
    public Guid PlayerAllocationId { get; private set; }
    public NetworkDriver PlayerDriver { get; private set; }
    public bool connected { get; internal set; }

    public UTPTransport transport;

    private NetworkConnection clientConnection;

    public IEnumerator InitClient(string joinCode)
    {
        UILogManager.log.Write("Join code is " + joinCode);
        yield return ClientBindAndConnect(joinCode);
    }
    private void Update()
    {
        if (PlayerDriver.IsCreated && clientConnection.IsCreated)
        {
            ClientUpdate();
        }
    }
    void ClientUpdate()
    {
        PlayerDriver.ScheduleUpdate().Complete();

        DataStreamReader stream;

        //Resolve event queue
        NetworkEvent.Type eventType;
        while ((eventType = clientConnection.PopEvent(PlayerDriver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (eventType == NetworkEvent.Type.Connect)
            {
                UILogManager.log.Write("Client connected to the server");
                Debug.Log("We are now connected to the server");
                connected = true;
                transport.OnClientConnected?.Invoke();
            }
            else if (eventType == NetworkEvent.Type.Disconnect)
            {
                UILogManager.log.Write("Client got disconnected from server");
                Debug.Log("Client disconnected from the server");
                clientConnection = default(NetworkConnection);
            }
            else if (eventType == NetworkEvent.Type.Data)
            {
                byte[] array = new byte[stream.Length];
                for (int i=0; i < array.Length; i++)
                {
                    array[i] = stream.ReadByte();
                }
                ArraySegment<byte> segment = new ArraySegment<byte>(array);
                transport.OnClientDataReceived?.Invoke(segment, 0);
                Debug.Log("I Received Data");
            }
        }
    }

    public void SendToServer(ArraySegment<byte> segment, int channelId)
    {
        DataStreamWriter writer;
        PlayerDriver.BeginSend(clientConnection, out writer);
        foreach (byte b in segment)
            writer.WriteByte(b);
        PlayerDriver.EndSend(writer);
        transport.OnClientDataSent?.Invoke(segment,0);
    }

    public void Disconnect()
    {
        clientConnection.Disconnect(PlayerDriver);
    }

    public void Shutdown()
    {
        Disconnect();
        PlayerDriver.Dispose();
    }

    private void OnDestroy()
    {
        Shutdown();
    }

    private IEnumerator ClientBindAndConnect(string relayJoinCode)
    {
        var joinTask = RelayService.Instance.JoinAllocationAsync(relayJoinCode);

        while (!joinTask.IsCompleted)
            yield return null;

        if (joinTask.IsFaulted)
        {
            UILogManager.log.Write("Join Relay request failed");
            yield break;
        }

        // Collect and convert the Relay data from the join response
        JoinAllocation = joinTask.Result;


        // Send the join request to the Relay service
        UILogManager.log.Write("Attempting to join allocation with join code... " + relayJoinCode);

        PlayerAllocationId = JoinAllocation.AllocationId;
        UILogManager.log.Write($"Player allocated with allocation Id: {PlayerAllocationId}");

        // Format the server data, based on desired connectionType
        relayServerData = RelayHelper.PlayerRelayData(JoinAllocation, "udp");

        // Create the NetworkSettings with Relay parameters
        var networkSettings = new NetworkSettings();
        networkSettings.WithRelayParameters(serverData: ref relayServerData);

        // Create the NetworkDriver using the Relay parameters
        PlayerDriver = NetworkDriver.Create(networkSettings);

        // Bind the NetworkDriver to the available local endpoint.
        // This will send the bind request to the Relay server
        if (PlayerDriver.Bind(NetworkEndPoint.AnyIpv4) != 0)
        {
            UILogManager.log.Write("Client failed to bind");
        }
        else
        {
            while (!PlayerDriver.Bound)
            {
                PlayerDriver.ScheduleUpdate().Complete();
                yield return null;
            }

            // Once the client is bound to the Relay server, you can send a connection request
            clientConnection = PlayerDriver.Connect(relayServerData.Endpoint);
        }

        UILogManager.log.Write("Conected");

        // Create Object
        joinData = new RelayHelper.RelayJoinData
        {
            Key = JoinAllocation.Key,
            Port = (ushort)JoinAllocation.RelayServer.Port,
            AllocationID = JoinAllocation.AllocationId,
            AllocationIDBytes = JoinAllocation.AllocationIdBytes,
            ConnectionData = JoinAllocation.ConnectionData,
            HostConnectionData = JoinAllocation.HostConnectionData,
            IPv4Address = JoinAllocation.RelayServer.IpV4
        };
    }
}
