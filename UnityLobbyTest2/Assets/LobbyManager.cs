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
    [SerializeField] RelayServer relayServer;
    [SerializeField] RelayClient relayClient;


    private Lobby currentLobby;
    private Log log;
    private string lobbyID;
    private List<Lobby> currentLobbyList;

    public NetworkDriver HostDriver;
    public NetworkDriver PlayerDriver;

    public bool isRelayServerConnected { get; private set; }
    public Player loggedInPlayer { get; private set; }
    public Guid playerAllocationId { get; private set; }
    public Unity.Networking.Transport.NetworkConnection clientConnection { get; private set; }
   

    // Start is called before the first frame update
    void Start()
    {
        UILogManager.log = new Log(debugText);
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

        await relayServer.InitHost(maxPlayers);

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
            ["JoinCode"] = new DataObject(DataObject.VisibilityOptions.Member, relayServer.hostData.JoinCode, DataObject.IndexOptions.S3),
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

                await relayClient.InitClient(joinCode);
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

}
