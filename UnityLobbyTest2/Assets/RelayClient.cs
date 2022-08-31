using UnityEngine;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Threading.Tasks;
using System;
using Unity.Networking.Transport;
using System.Collections.Generic;

public class RelayClient : MonoBehaviour
{
    NetworkDriver driver;
    RelayServerData relayServerData;
    bool isHost = false;
    private int maxPlayers;
    private RelayHelper.RelayJoinData joinData;
    public JoinAllocation joinAllocation { get; private set; }
    public Guid playerAllocationId { get; private set; }
    public NetworkDriver PlayerDriver { get; private set; }
    public NetworkConnection clientConnection { get; private set; }

    public async Task InitClient(string joinCode)
    {
        UILogManager.log.Write("Join code is " + joinCode);

        await ClientBindAndConnect(joinCode);

        UILogManager.log.Write("Conected");

        // Create Object
        joinData = new RelayHelper.RelayJoinData
        {
            Key = joinAllocation.Key,
            Port = (ushort)joinAllocation.RelayServer.Port,
            AllocationID = joinAllocation.AllocationId,
            AllocationIDBytes = joinAllocation.AllocationIdBytes,
            ConnectionData = joinAllocation.ConnectionData,
            HostConnectionData = joinAllocation.HostConnectionData,
            IPv4Address = joinAllocation.RelayServer.IpV4
        };

    }


    private async Task ClientBindAndConnect(string relayJoinCode)
    {
        // Send the join request to the Relay service
        joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
        UILogManager.log.Write("Attempting to join allocation with join code... " + relayJoinCode);

        playerAllocationId = joinAllocation.AllocationId;
        UILogManager.log.Write($"Player allocated with allocation Id: {playerAllocationId}");

        // Format the server data, based on desired connectionType
        relayServerData = RelayHelper.PlayerRelayData(joinAllocation, "udp");

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
                await Task.Delay(10);
            }

            // Once the client is bound to the Relay server, you can send a connection request
            clientConnection = PlayerDriver.Connect(relayServerData.Endpoint);
        }
    }
}
