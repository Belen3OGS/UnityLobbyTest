using UnityEngine;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Threading.Tasks;
using System;
using Unity.Networking.Transport;

public class RelayServer : MonoBehaviour
{
    NetworkDriver driver;
    public RelayHelper.RelayHostData hostData;
    RelayServerData relayServerData;
    private int maxPlayers;

    public bool isRelayServerConnected { get; private set; }
    public JoinAllocation joinAllocation { get; private set; }
    public Guid playerAllocationId { get; private set; }

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
        driver = NetworkDriver.Create(networkSettings);

        // Bind the NetworkDriver to the local endpoint
        if (driver.Bind(NetworkEndPoint.AnyIpv4) != 0)
        {
            UILogManager.log.Write("Server failed to bind");
        }
        else
        {
            // The binding process is an async operation; wait until bound
            while (!driver.Bound)
            {
                driver.ScheduleUpdate().Complete();
                await Task.Delay(10);
            }

            // Once the driver is bound you can start listening for connection requests
            if (driver.Listen() != 0)
            {
                UILogManager.log.Write("Server failed to listen");
            }
            else
            {
                isRelayServerConnected = true;
            }
        }
        UILogManager.log.Write("Server bound.");
    }
}
