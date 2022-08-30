using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Collections;
using Mirror;
using Unity.Networking.Transport.Relay;
using Unity.Networking.Transport;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] InputField lobbyName;
    [SerializeField] Text debugText;
    [SerializeField] int maxPlayers = 8;
    [SerializeField] bool isPrivate = false;

    private Lobby currentLobby;
    private Log log;
    private RelayHostData _hostData;
    private RelayJoinData _joinData;
    private string lobbyID;
    private List<Lobby> currentLobbyList;

    public NetworkDriver HostDriver;
    public NetworkDriver PlayerDriver;

    public bool isRelayServerConnected { get; private set; }
    public Player loggedInPlayer { get; private set; }
    public Guid playerAllocationId { get; private set; }
    public Unity.Networking.Transport.NetworkConnection clientConnection { get; private set; }
    public JoinAllocation joinAllocation { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        log = new Log(debugText);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Button Functions+



    public async void CreateLobby()
    {

        // Log in a player for this game client
        loggedInPlayer = await GetPlayerFromAnonymousLoginAsync();

        log.Write("Creating Relay Object");

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

        Debug.Log("Alocation: " + allocation);

        _hostData = new RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            IPv4Address = allocation.RelayServer.IpV4
        };

        // Retrieve JoinCode, with this you can join later
        _hostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        log.Write("Creating a Lobby");

        // Add some data to our player
        // This data will be included in a lobby under players -> player.data
        loggedInPlayer.Data.Add("Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "No"));

        //Creating Lobby
        var lobbyData = new Dictionary<string, DataObject>()
        {
            ["Test"] = new DataObject(DataObject.VisibilityOptions.Public, "true", DataObject.IndexOptions.S1),
            ["GameMode"] = new DataObject(DataObject.VisibilityOptions.Public, "ctf", DataObject.IndexOptions.S2),
            ["Skill"] = new DataObject(DataObject.VisibilityOptions.Public, Random.Range(1, 51).ToString(), DataObject.IndexOptions.N1),
            ["Rank"] = new DataObject(DataObject.VisibilityOptions.Public, Random.Range(1, 51).ToString()),
            ["JoinCode"] = new DataObject(DataObject.VisibilityOptions.Member, _hostData.JoinCode, DataObject.IndexOptions.S3),
        };

        // Create a new lobby
        currentLobby = await LobbyService.Instance.CreateLobbyAsync(
            lobbyName: lobbyName.text,
            maxPlayers: maxPlayers,
            options: new CreateLobbyOptions()
            {
                Data = lobbyData,
                IsPrivate = isPrivate,
                Player = loggedInPlayer
            });

        lobbyID = currentLobby.Id;

        log.Write("Created new lobby " + currentLobby.Name + " " + currentLobby.Id);

        StartCoroutine(HeartbeatLobbyCoroutine(lobbyID, 15));

        //TODO: We probably need to change UDP with something else in order to support Mirror
        var relayServerData = HostRelayData(allocation, "udp");

        await ServerBindAndListenAsHostPlayer(relayServerData);

    }

    public async void JoinLobby()
    {
        await UnityServices.InitializeAsync();

        // Log in a player for this game client
        Player loggedInPlayer = await GetPlayerFromAnonymousLoginAsync();

        log.Write("Creating filters to join");
        List<QueryFilter> queryFilters = new List<QueryFilter>
        {
            // Let's search for games with open slots (AvailableSlots greater than 0)
            new QueryFilter(
                field: QueryFilter.FieldOptions.AvailableSlots,
                op: QueryFilter.OpOptions.GT,
                value: "0"),
            new QueryFilter(QueryFilter.FieldOptions.Name,lobbyName.text,QueryFilter.OpOptions.EQ) 
            
        };
        List<QueryOrder> queryOrdering = new List<QueryOrder>
        {
            new QueryOrder(true, QueryOrder.FieldOptions.AvailableSlots),
            new QueryOrder(false, QueryOrder.FieldOptions.Created),
            new QueryOrder(false, QueryOrder.FieldOptions.Name),
        };
        QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions()
        {
            Count = 100, // Override default number of results to return
            Filters = queryFilters,
            Order = queryOrdering,
        });

        currentLobbyList = new List<Lobby>();
        currentLobbyList = response.Results;

        log.Write("Found " + currentLobbyList.Count + " results");

        if (response.Results.Count > 0)
        {
            var joinLobbyTask = LobbyService.Instance.JoinLobbyByIdAsync(
                lobbyId: currentLobbyList[0].Id,
                options: new JoinLobbyByIdOptions()
                {
                    Player = loggedInPlayer
                });
            while (!joinLobbyTask.IsCompleted)
            {
                await Task.Delay(10);
            }
            if (joinLobbyTask.IsCompleted)
            {
                currentLobby = joinLobbyTask.Result;

                log.Write("Joined lobby " + currentLobby.Name);

                string joinCode = currentLobby.Data["JoinCode"].Value;

                log.Write("Join code is " + joinCode);

                await ClientBindAndConnect(joinCode);

                log.Write("Conected");

                // Create Object
                _joinData = new RelayJoinData
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

        }

    }

    //Functions
    async Task<Player> GetPlayerFromAnonymousLoginAsync()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            log.Write("Trying to log in a player ...");

            // Use Unity Authentication to log in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                log.Write("Player was not signed in successfully; unable to continue without a logged in player");
                throw new InvalidOperationException("Player was not signed in successfully; unable to continue without a logged in player");
            }
        }
        log.Write("Player signed in as " + AuthenticationService.Instance.PlayerId);

        // Player objects have Get-only properties, so you need to initialize the data bag here if you want to use it
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>());
    }

    IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        log.Write("Lobby Heartbeat");
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        // We need to delete the lobby when we're not using it
        Lobbies.Instance.DeleteLobbyAsync(lobbyID);
        HostDriver.Dispose();
        PlayerDriver.Dispose();
    }
    /// <summary>
    /// RelayHostData represents the necessary informations
    /// for a Host to host a game on a Relay
    /// </summary>
    public struct RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }

    /// <summary>
    /// RelayHostData represents the necessary informations
    /// for a Host to host a game on a Relay
    /// </summary>
    public struct RelayJoinData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }

    public static RelayServerData HostRelayData(Allocation allocation, string connectionType = "udp")
    {
        // Select endpoint based on desired connectionType
        var endpoint = GetEndpointForConnectionType(allocation.ServerEndpoints, connectionType);
        if (endpoint == null)
        {
            throw new Exception($"endpoint for connectionType {connectionType} not found");
        }

        // Prepare the server endpoint using the Relay server IP and port
        var serverEndpoint = NetworkEndPoint.Parse(endpoint.Host, (ushort)endpoint.Port);

        // UTP uses pointers instead of managed arrays for performance reasons, so we use these helper functions to convert them
        var allocationIdBytes = ConvertFromAllocationIdBytes(allocation.AllocationIdBytes);
        var connectionData = ConvertConnectionData(allocation.ConnectionData);
        var key = ConvertFromHMAC(allocation.Key);

        // Prepare the Relay server data and compute the nonce value
        // The host passes its connectionData twice into this function
        var relayServerData = new RelayServerData(ref serverEndpoint, 0, ref allocationIdBytes, ref connectionData,
            ref connectionData, ref key, connectionType == "dtls");
        relayServerData.ComputeNewNonce();

        return relayServerData;
    }

    private static RelayServerEndpoint GetEndpointForConnectionType(List<RelayServerEndpoint> endpoints, string connectionType)
    {
        foreach (var endpoint in endpoints)
        {
            if (endpoint.ConnectionType == connectionType)
            {
                return endpoint;
            }
        }

        return null;
    }

    private static RelayAllocationId ConvertFromAllocationIdBytes(byte[] allocationIdBytes)
    {
        unsafe
        {
            fixed (byte* ptr = allocationIdBytes)
            {
                return RelayAllocationId.FromBytePointer(ptr, allocationIdBytes.Length);
            }
        }
    }

    private static RelayConnectionData ConvertConnectionData(byte[] connectionData)
    {
        unsafe
        {
            fixed (byte* ptr = connectionData)
            {
                return RelayConnectionData.FromBytePointer(ptr, RelayConnectionData.k_Length);
            }
        }
    }

    private static RelayHMACKey ConvertFromHMAC(byte[] hmac)
    {
        unsafe
        {
            fixed (byte* ptr = hmac)
            {
                return RelayHMACKey.FromBytePointer(ptr, RelayHMACKey.k_Length);
            }
        }
    }

    private async Task ServerBindAndListenAsHostPlayer(RelayServerData relayNetworkParameter)
    {
        // Create the NetworkSettings with Relay parameters
        var networkSettings = new NetworkSettings();
        networkSettings.WithRelayParameters(serverData : ref relayNetworkParameter);

        // Create the NetworkDriver using NetworkSettings
        HostDriver = NetworkDriver.Create(networkSettings);

        // Bind the NetworkDriver to the local endpoint
        if (HostDriver.Bind(NetworkEndPoint.AnyIpv4) != 0)
        {
            log.Write("Server failed to bind");
        }
        else
        {
            // The binding process is an async operation; wait until bound
            while (!HostDriver.Bound)
            {
                HostDriver.ScheduleUpdate().Complete();
                await Task.Delay(10);
            }

            // Once the driver is bound you can start listening for connection requests
            if (HostDriver.Listen() != 0)
            {
                log.Write("Server failed to listen");
            }
            else
            {
                isRelayServerConnected = true;
            }
        }

        log.Write("Server bound.");
    }

    private async Task ClientBindAndConnect(string relayJoinCode)
    {
        // Send the join request to the Relay service
        joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
        log.Write("Attempting to join allocation with join code... " + relayJoinCode);

        playerAllocationId = joinAllocation.AllocationId;
        log.Write($"Player allocated with allocation Id: {playerAllocationId}");

        // Format the server data, based on desired connectionType
        var relayServerData = PlayerRelayData(joinAllocation, "udp");

        // Create the NetworkSettings with Relay parameters
        var networkSettings = new NetworkSettings();
        networkSettings.WithRelayParameters(serverData : ref relayServerData);

        // Create the NetworkDriver using the Relay parameters
        PlayerDriver = NetworkDriver.Create(networkSettings);

        // Bind the NetworkDriver to the available local endpoint.
        // This will send the bind request to the Relay server
        if (PlayerDriver.Bind(NetworkEndPoint.AnyIpv4) != 0)
        {
           log.Write("Client failed to bind");
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

    public static RelayServerData PlayerRelayData(JoinAllocation allocation, string connectionType = "udp")
    {
        // Select endpoint based on desired connectionType
        var endpoint = GetEndpointForConnectionType(allocation.ServerEndpoints, connectionType);
        if (endpoint == null)
        {
            throw new Exception($"endpoint for connectionType {connectionType} not found");
        }

        // Prepare the server endpoint using the Relay server IP and port
        var serverEndpoint = NetworkEndPoint.Parse(endpoint.Host, (ushort)endpoint.Port);

        // UTP uses pointers instead of managed arrays for performance reasons, so we use these helper functions to convert them
        var allocationIdBytes = ConvertFromAllocationIdBytes(allocation.AllocationIdBytes);
        var connectionData = ConvertConnectionData(allocation.ConnectionData);
        var hostConnectionData = ConvertConnectionData(allocation.HostConnectionData);
        var key = ConvertFromHMAC(allocation.Key);

        // Prepare the Relay server data and compute the nonce values
        // A player joining the host passes its own connectionData as well as the host's
        var relayServerData = new RelayServerData(ref serverEndpoint, 0, ref allocationIdBytes, ref connectionData,
            ref hostConnectionData, ref key, connectionType == "dtls");
        relayServerData.ComputeNewNonce();

        return relayServerData;
    }

}
