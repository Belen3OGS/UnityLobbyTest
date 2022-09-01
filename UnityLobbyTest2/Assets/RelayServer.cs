using UnityEngine;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Threading.Tasks;
using System;
using Unity.Networking.Transport;
using Unity.Collections;

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
        this.maxPlayers = maxPlayers;

        UILogManager.log.Write("Creating Relay Object");

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

    private void OnDestroy()
    {
        Dispose();
    }

    public void Dispose()
    {
        serverDriver.Dispose();
    }
}
