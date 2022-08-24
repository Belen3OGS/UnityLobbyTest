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
        await UnityServices.InitializeAsync();

        // Log in a player for this game client
        Player loggedInPlayer = await GetPlayerFromAnonymousLoginAsync();

        log.Write("Creating Relay Object");

        Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxPlayers);

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
        _hostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

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

        NetworkManager.singleton.networkAddress = _hostData.IPv4Address;
        NetworkManager.singleton.GetComponent<kcp2k.KcpTransport>().Port = _hostData.Port;

        NetworkManager.singleton.StartHost();



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
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(
                lobbyId: currentLobbyList[0].Id,
                options: new JoinLobbyByIdOptions()
                {
                    Player = loggedInPlayer
                });

            log.Write("Joined lobby " + currentLobby.Name);

            string joinCode = currentLobby.Data["JoinCode"].Value;

            log.Write("Join code is " + joinCode);

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            log.Write("Conected");

            // Create Object
            _joinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            NetworkManager.singleton.networkAddress = _joinData.IPv4Address;
            NetworkManager.singleton.GetComponent<kcp2k.KcpTransport>().Port = _joinData.Port;

            NetworkManager.singleton.StartClient();

            Debug.Log(_joinData.IPv4Address);

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
}
