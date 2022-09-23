using UnityEngine;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Threading.Tasks;
using System;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine.Assertions;
using System.Collections;

public class RelayServer : MonoBehaviour
{
    [SerializeField] public int MaxPacketSize;
    NetworkDriver serverDriver;
    public RelayHelper.RelayHostData hostData;
    RelayServerData relayServerData;

    public event Action<string> OnServerReady;
    public event Action<int> OnServerConnected;
    public event Action<int> OnServerDisconnected;
    public event Action<int,ArraySegment<byte>,int> OnServerDataReceived;
    public event Action<int,ArraySegment<byte>,int> OnServerDataSent;

    public bool IsRelayServerConnected { get; private set; }
    public JoinAllocation JoinAllocation { get; private set; }
    public Guid PlayerAllocationId { get; private set; }
    private NativeList<NetworkConnection> connections;
    public int maxPlayers;

    public IEnumerator InitHost()
    {
        UILogManager.log.Write("Creating Relay Object");

        connections = new NativeList<NetworkConnection>(maxPlayers, Allocator.Persistent);

        var createAllocationTask =RelayService.Instance.CreateAllocationAsync(maxPlayers);
        while(!createAllocationTask.IsCompleted)
            yield return null;
        if (createAllocationTask.IsFaulted)
        {
            Debug.LogError("Could not create Relay server Allocation");
            yield break;
        }
        Allocation allocation = createAllocationTask.Result;

        Debug.Log("Alocation: " + allocation);

        hostData = new RelayHelper.RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            IPv4Address = allocation.RelayServer.IpV4
        };

        var getJoinCodeTask = RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        while (!getJoinCodeTask.IsCompleted)
            yield return null;
        if (getJoinCodeTask.IsFaulted)
        {
            Debug.LogError("Could not obtain Relay server Join Code");
            yield break;
        }
        // Retrieve JoinCode, with this you can join later
        hostData.JoinCode = getJoinCodeTask.Result;

        relayServerData = RelayHelper.HostRelayData(allocation, "udp");

        var serverBindAsHotTask = ServerBindAndListenAsHostPlayer(relayServerData);
        while (!serverBindAsHotTask.IsCompleted)
            yield return null;
        if (serverBindAsHotTask.IsFaulted)
        {
            Debug.LogError("Failed to Bind to Server as Host");
            yield break;
        }

        IsRelayServerConnected = true;

        Debug.Log("RELAY READY!!");
        OnServerReady?.Invoke(hostData.JoinCode);
    }

    public void HostEarlyUpdate()
    {
        if (!(serverDriver.IsCreated && IsRelayServerConnected))
            return;

        serverDriver.ScheduleUpdate().Complete();

        //Accept incoming client connections
        NetworkConnection incomingConnection;
        while ((incomingConnection = serverDriver.Accept()) != default(NetworkConnection))
        {
            connections.Add(incomingConnection);
            UILogManager.log.Write("Accepted an incoming connection.");
            OnServerConnected?.Invoke(incomingConnection.InternalId + 1);
        }

        //Process events from all connections
        for (int i = 0; i < connections.Length; i++)
        {
            DataStreamReader stream;

            if (connections[i].IsCreated)
            {
                NetworkEvent.Type eventType;
                while ((eventType = serverDriver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    if (eventType == NetworkEvent.Type.Disconnect)
                    {
                        DisconnectPlayer(i + 1);
                        UILogManager.log.Write("Client disconnected from server");
                    }

                    if(eventType == NetworkEvent.Type.Connect)
                    {
                        Debug.Log("PLAYER CONNECTED?");
                    }

                    else if (eventType == NetworkEvent.Type.Data)
                    {
                        byte[] array = new byte[stream.Length];
                        for (int j = 0; j < array.Length; j++)
                        {
                            array[j] = stream.ReadByte();
                        }
                        ArraySegment<byte> segment = new ArraySegment<byte>(array);
                        OnServerDataReceived?.Invoke(i + 1, segment, 0);
                    }
                }
            }

        }
    }
    public void HostLateUpdate()
    {
        if (!(serverDriver.IsCreated && IsRelayServerConnected))
            return;
    }

    public void SendToClient(int i, ArraySegment<byte> segment, int channelId)
    {
        if (!connections[i - 1].IsCreated)
        {
            Debug.LogError("Client already disconnected");
            return;
        }

        DataStreamWriter writer;
        serverDriver.BeginSend(connections[i-1], out writer);
        foreach (byte b in segment)
            writer.WriteByte(b);
        serverDriver.EndSend(writer);
        OnServerDataSent?.Invoke(i, segment, 0);
    }

    private async Task ServerBindAndListenAsHostPlayer(RelayServerData relayNetworkParameter)
    {
        // Create the NetworkSettings with Relay parameters
        var networkSettings = new NetworkSettings();
        networkSettings.WithRelayParameters(serverData: ref relayNetworkParameter);

        // Create the NetworkDriver using NetworkSettings
        serverDriver = NetworkDriver.Create(networkSettings);

        // Bind the NetworkDriver to the local endpoint
        if (serverDriver.Bind(NetworkEndPoint.AnyIpv4) != 0)
        {
            UILogManager.log.Write("Server failed to bind");
        }
        else
        {
            // The binding process is an async operation; wait until bound
            while (!serverDriver.Bound)
            {
                serverDriver.ScheduleUpdate().Complete();
                await Task.Delay(10);
            }

            // Once the driver is bound you can start listening for connection requests
            if (serverDriver.Listen() != 0)
            {
                UILogManager.log.Write("Server failed to listen");
            }
            else
            {
                IsRelayServerConnected = true;
            }
        }
        UILogManager.log.Write("Server bound.");
    }

    public void DisconnectPlayer(int i) 
    {
        connections[i - 1] = default(NetworkConnection);
        OnServerDisconnected?.Invoke(i);
    }

    public void Shutdown()
    {
        if(serverDriver.IsCreated)
        {
            serverDriver.ScheduleUpdate().Complete();
            serverDriver.Dispose();
        }
        if (connections.IsCreated)
        {
            connections.Dispose();
        }
        IsRelayServerConnected = false;
    }
}
