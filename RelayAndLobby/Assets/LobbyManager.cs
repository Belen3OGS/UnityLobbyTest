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
using Unity.Networking.Transport.Relay;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine.Assertions;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] InputField lobbyName;
    [SerializeField] Text debugText;
    [SerializeField] int maxPlayers = 8;
    [SerializeField] bool isPrivate = false;

    private Log log;
    private Player loggedInPlayer;

    public NetworkDriver HostDriver;
    public NetworkDriver PlayerDriver;
    public string JoinCode;

    private NetworkConnection clientConnection;
    private bool isRelayServerConnected = false;
    private Lobby currentLobby;
    private string lobbyID;
    private List<Lobby> currentLobbyList;
    private NativeList<NetworkConnection> serverConnections;


    void Start()
    {
        log = new Log(debugText);
        serverConnections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

    }

    void Update()
    {
        if (HostDriver.IsCreated && isRelayServerConnected)
        {
            HostUpdate();
        }

        if (PlayerDriver.IsCreated && clientConnection.IsCreated)
        {
            ClientUpdate();
        }
    }

    public async void CreateButton()
    {
        await UnityServices.InitializeAsync();

        loggedInPlayer = await GetPlayerFromAnonymousLoginAsync();
        StartCoroutine(StartingNetworkDriverAsHost());
    }


    public async void JoinButton()
    {
        await UnityServices.InitializeAsync();

        loggedInPlayer = await GetPlayerFromAnonymousLoginAsync();

        await FindLobbys();
        StartCoroutine(JoinFirstLobby());
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
    IEnumerator StartingNetworkDriverAsHost()
    {
        // Request list of valid regions
        var regionsTask = RelayService.Instance.ListRegionsAsync();

        while (!regionsTask.IsCompleted)
        {
            yield return null;
        }

        if (regionsTask.IsFaulted)
        {
            log.Write("List regions request failed");
            yield break;
        }

        var regionList = regionsTask.Result;
        // pick a region from the list
        var targetRegion = regionList[0].Id;

        // Request an allocation to the Relay service
        // with a maximum of 5 peer connections, for a maximum of 6 players.
        var relayMaxConnections = 5;
        var allocationTask = RelayService.Instance.CreateAllocationAsync(relayMaxConnections, targetRegion);

        while (!allocationTask.IsCompleted)
        {
            yield return null;
        }

        if (allocationTask.IsFaulted)
        {
            log.Write("Create allocation request failed");
            yield break;
        }

        var allocation = allocationTask.Result;

        // Request the join code to the Relay service
        var joinCodeTask = RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        while (!joinCodeTask.IsCompleted)
        {
            yield return null;
        }

        if (joinCodeTask.IsFaulted)
        {
            log.Write("Create join code request failed");
            yield break;
        }

        // Get the Join Code, you can then share it with the clients so they can join
        JoinCode = joinCodeTask.Result;

        // Format the server data, based on desired connectionType
        var relayServerData = HostRelayData(allocation, "dtls");

        // Bind and listen to the Relay server
        yield return BindAndListenAsHostPlayer(relayServerData);
    }
    IEnumerator BindAndListenAsHostPlayer(RelayServerData relayServerData)
    {
        // Create the NetworkDriver using the Relay server data
        var settings = new NetworkSettings();
        settings.WithRelayParameters(serverData: ref relayServerData);
        HostDriver = NetworkDriver.Create(settings);

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
                yield return null;
            }

            // Once the driver is bound you can start listening for connection requests
            if (HostDriver.Listen() != 0)
            {
                log.Write("Server failed to listen");
            }
            else
            {
                isRelayServerConnected = true;
                CreateLobby();
            }
        }
    }
    async void CreateLobby()
    {
        //Creating Lobby
        var lobbyData = new Dictionary<string, DataObject>()
        {
            ["Test"] = new DataObject(DataObject.VisibilityOptions.Public, "true", DataObject.IndexOptions.S1),
            ["GameMode"] = new DataObject(DataObject.VisibilityOptions.Public, "ctf", DataObject.IndexOptions.S2),
            ["Skill"] = new DataObject(DataObject.VisibilityOptions.Public, Random.Range(1, 51).ToString(), DataObject.IndexOptions.N1),
            ["Rank"] = new DataObject(DataObject.VisibilityOptions.Public, Random.Range(1, 51).ToString()),
            ["JoinCode"] = new DataObject(DataObject.VisibilityOptions.Member, JoinCode, DataObject.IndexOptions.S3),
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

        StartCoroutine(HeartbeatLobbyCoroutine(lobbyID,8));

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
    private async Task FindLobbys()
    {
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
    }
    IEnumerator JoinFirstLobby()
    {
        if (currentLobbyList.Count > 0)
        {
            var joinLobbyTask = LobbyService.Instance.JoinLobbyByIdAsync(
                lobbyId: currentLobbyList[0].Id,
                options: new JoinLobbyByIdOptions()
                {
                    Player = loggedInPlayer
                });
            while (!joinLobbyTask.IsCompleted)
            {
                yield return null;
            }
            if (joinLobbyTask.IsFaulted)
            {
                log.Write("Join Lobby request failed");
                yield break;
            }
            currentLobby = joinLobbyTask.Result;

            log.Write("Joined lobby " + currentLobby.Name);

            string joinCode = currentLobby.Data["JoinCode"].Value;

            log.Write("Join code is " + joinCode);

            yield return StartNetworkDriverAsConnectingPlayer(joinCode);
        }
    }
    IEnumerator StartNetworkDriverAsConnectingPlayer(string relayJoinCode)
    {
        // Send the join request to the Relay service
        var joinTask = RelayService.Instance.JoinAllocationAsync(relayJoinCode);

        while (!joinTask.IsCompleted)
            yield return null;

        if (joinTask.IsFaulted)
        {
            log.Write("Join Relay request failed");
            yield break;
        }

        // Collect and convert the Relay data from the join response
        var allocation = joinTask.Result;

        // Format the server data, based on desired connectionType
        var relayServerData = PlayerRelayData(allocation, "dtls");

        yield return BindAndConnectToHost(relayServerData);
    }
    IEnumerator BindAndConnectToHost(RelayServerData relayServerData)
    {
        // Create the NetworkDriver using the Relay server data
        var settings = new NetworkSettings();
        settings.WithRelayParameters(serverData: ref relayServerData);
        PlayerDriver = NetworkDriver.Create(settings);

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
                yield return null;
            }

            // Once the client is bound to the Relay server, you can send a connection request
            clientConnection = PlayerDriver.Connect(relayServerData.Endpoint);
            log.Write("Connected");
        }
    }
    public void HostUpdate()
    {
        HostDriver.ScheduleUpdate().Complete();

        // Clean up stale connections
        for (int i = 0; i < serverConnections.Length; i++)
        {
            if (!serverConnections[i].IsCreated)
            {
                serverConnections.RemoveAtSwapBack(i);
                --i;
            }
        }

        //Accept incoming client connections
        NetworkConnection incomingConnection;
        while ((incomingConnection = HostDriver.Accept()) != default(NetworkConnection))
        {
            serverConnections.Add(incomingConnection);
            Debug.Log("Accepted an incoming connection.");
        }

        //Process events from all connections
        for (int i = 0; i < serverConnections.Length; i++)
        {
            Assert.IsTrue(serverConnections[i].IsCreated);

            NetworkEvent.Type eventType;
            while ((eventType = HostDriver.PopEventForConnection(serverConnections[i], out _)) != NetworkEvent.Type.Empty)
            {
                if (eventType == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    serverConnections[i] = default(NetworkConnection);
                }
            }
        }
    }
    public void ClientUpdate()
    {
        PlayerDriver.ScheduleUpdate().Complete();

        //Resolve event queue
        NetworkEvent.Type eventType;
        while ((eventType = clientConnection.PopEvent(PlayerDriver, out _)) != NetworkEvent.Type.Empty)
        {
            if (eventType == NetworkEvent.Type.Connect)
            {
                Debug.Log("Client connected to the server");
            }
            else if (eventType == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                clientConnection = default(NetworkConnection);
            }
        }
    }
    private void OnDestroy()
    {
        // We need to delete the lobby when we're not using it
        Lobbies.Instance.DeleteLobbyAsync(lobbyID);
        HostDriver.Dispose();
        PlayerDriver.Dispose();
    }



    #region Utilities
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
    public static RelayServerData HostRelayData(Allocation allocation, string connectionType = "dtls")
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

    public static RelayServerData PlayerRelayData(JoinAllocation allocation, string connectionType = "dtls")
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
    #endregion
}
