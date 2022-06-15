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

public class LobbyManager : MonoBehaviour
{
    [SerializeField] InputField lobbyName;
    [SerializeField] Text debugText;
    [SerializeField] int maxPlayers = 8;
    [SerializeField] bool isPrivate = false;

    private Lobby currentLobby;
    private Log log;

    // Start is called before the first frame update
    void Start()
    {
        log = new Log(debugText);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Button Functions

    public void OnCreateLobby()
    {

    }
    async Task CreateLobby()
    {
        await UnityServices.InitializeAsync();

        // Log in a player for this game client
        Player loggedInPlayer = await GetPlayerFromAnonymousLoginAsync();

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

        log.Write("Created new lobby " + currentLobby.Name + " " +currentLobby.Id);


    }
    public void OnJoinLobby()
    {

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
}
