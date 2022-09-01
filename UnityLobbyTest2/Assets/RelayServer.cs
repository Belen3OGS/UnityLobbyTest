using UnityEngine;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Threading.Tasks;
using System;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine.Assertions;

public class RelayServer : MonoBehaviour
{
    NetworkDriver serverDriver;
    public RelayHelper.RelayHostData hostData;
    RelayServerData relayServerData;
    public bool IsRelayServerConnected { get; private set; }
    public JoinAllocation JoinAllocation { get; private set; }
    public Guid PlayerAllocationId { get; private set; }
    private NativeList<NetworkConnection> connections;

    public async Task InitHost(int maxPlayers)
    {
        UILogManager.log.Write("Creating Relay Object");

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

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

        // Retrieve JoinCode, with this you can join later
        hostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        relayServerData = RelayHelper.HostRelayData(allocation, "udp");

        await ServerBindAndListenAsHostPlayer(relayServerData);
    }
    private void Update()
    {
        if (serverDriver.IsCreated && IsRelayServerConnected)
        {
            HostUpdate();
        }
    }
    void HostUpdate()
    {
        serverDriver.ScheduleUpdate().Complete();

        // Clean up stale connections
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        //Accept incoming client connections
        NetworkConnection incomingConnection;
        while ((incomingConnection = serverDriver.Accept()) != default(NetworkConnection))
        {
            connections.Add(incomingConnection);
            UILogManager.log.Write("Accepted an incoming connection.");
        }

        //Process events from all connections
        for (int i = 0; i < connections.Length; i++)
        {
            Assert.IsTrue(connections[i].IsCreated);

            NetworkEvent.Type eventType;
            while ((eventType = serverDriver.PopEventForConnection(connections[i], out _)) != NetworkEvent.Type.Empty)
            {
                if (eventType == NetworkEvent.Type.Disconnect)
                {
                    UILogManager.log.Write("Client disconnected from server");
                    connections[i] = default(NetworkConnection);
                }
            }
        }
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
}
