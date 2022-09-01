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
    private NetworkConnection clientConnection;

    public IEnumerator InitClient(string joinCode)
    {
        UILogManager.log.Write("Join code is " + joinCode);
        yield return ClientBindAndConnect(joinCode);
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

    public void Dispose()
    {
        PlayerDriver.Dispose();
    }
    private void OnDestroy()
    {
        Dispose();
    }
}
