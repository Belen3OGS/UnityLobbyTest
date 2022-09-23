using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Collections;
using Mirror;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] InputField lobbyName;
    [SerializeField] NetworkManager mirrorManager;
    public event Action<string> OnLobbyJoined;
    public event Action OnLobbyCreated;

    public Player loggedInPlayer { get; private set; }
    public Guid playerAllocationId { get; private set; }

    private Lobby currentLobby;
    private List<Lobby> currentLobbyList;
    private bool unityServicesInitialized = false;
    private bool IsConnectedToLobby = false;

    #region Initialization

    void Start()
    {
        StartCoroutine(InitializeUnityServices());
    }

    public bool IsUnityServicesInitialized()
    {
        if (!unityServicesInitialized)
        {
            Debug.LogError("Unity Services have not been initialized!!!");
            return false;
        }
        else
            return true;
    }

    private IEnumerator InitializeUnityServices()
    {
        //Initialize Unity Services
        var initTask = UnityServices.InitializeAsync();
        while (!initTask.IsCompleted)
        {
            yield return null;
        }
        if (initTask.IsFaulted)
        {
            UILogManager.log.Write("Unity Services Initialization Failed");
            Debug.LogError("Unity Services Initialization Failed");
            yield break;
        }

        //Log in player
        var logInTask = GetPlayerFromAnonymousLoginAsync();
        while (!logInTask.IsCompleted)
        {
            yield return null;
        }
        if (logInTask.IsFaulted)
        {
            UILogManager.log.Write("Failed Player Log-in");
            Debug.LogError("Failed Player Log-in!");
            yield break;
        }
        else
            loggedInPlayer = logInTask.Result;

        unityServicesInitialized = true;
    } 
    #endregion

    #region Button Functions
    public void FindLobbysButton()
    {
        StartCoroutine(FindLobbys());
    }

    public void JoinLobbyButton()
    {
        StartCoroutine(JoinFirstLobby());
    }
    #endregion

    #region Lobby creation and joining
    public IEnumerator CreateLobby(string address, int maxPlayers, bool isPrivate = false)
    {
        if (!IsUnityServicesInitialized())
        {
            yield break;
        }

        UILogManager.log.Write("Creating a Lobby");

        // Add some data to our player
        // This data will be included in a lobby under players -> player.data
        loggedInPlayer.Data.Add("Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "No"));

        //Creating Lobby Data
        var lobbyData = new Dictionary<string, DataObject>()
        {
            ["Test"] = new DataObject(DataObject.VisibilityOptions.Public, "true", DataObject.IndexOptions.S1),
            ["GameMode"] = new DataObject(DataObject.VisibilityOptions.Public, "ctf", DataObject.IndexOptions.S2),
            ["Skill"] = new DataObject(DataObject.VisibilityOptions.Public, Random.Range(1, 51).ToString(), DataObject.IndexOptions.N1),
            ["Rank"] = new DataObject(DataObject.VisibilityOptions.Public, Random.Range(1, 51).ToString()),
            ["Address"] = new DataObject(DataObject.VisibilityOptions.Member, address, DataObject.IndexOptions.S3),
        };

        // Create a new lobby
        var createLobby = LobbyService.Instance.CreateLobbyAsync(
            lobbyName: lobbyName.text,
            maxPlayers: maxPlayers,
            options: new CreateLobbyOptions()
            {
                Data = lobbyData,
                IsPrivate = isPrivate,
                Player = loggedInPlayer
            });
        while (!createLobby.IsCompleted)
            yield return null;
        if (createLobby.IsFaulted)
        {
            Debug.LogError("Lobby Creation Failed!!");
            yield break;
        }
        currentLobby = createLobby.Result;

        UILogManager.log.Write("Created new lobby " + currentLobby.Name + " " + currentLobby.Id);

        StartCoroutine(HeartbeatLobbyCoroutine(currentLobby.Id, 15));

        IsConnectedToLobby = true;
    }

    private IEnumerator FindLobbys()
    {
        if (!IsUnityServicesInitialized())
            yield break;

        UILogManager.log.Write("Creating filters to join");
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

        //Find Lobby Query
        var findLobbyQuery = LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions()
        {
            Count = 100, // Override default number of results to return
            Filters = queryFilters,
            Order = queryOrdering,
        });
        while (!findLobbyQuery.IsCompleted)
        {
            yield return null;
        }
        if (findLobbyQuery.IsFaulted)
        {
            UILogManager.log.Write("Lobby list retrieval failed!");
            Debug.LogError("Lobby list retrieval failed!");
        }
        QueryResponse response = findLobbyQuery.Result;

        currentLobbyList = response.Results;

        UILogManager.log.Write("Found " + currentLobbyList.Count + " results");
    }

    private IEnumerator JoinFirstLobby()
    {
        if (!IsUnityServicesInitialized())
            yield break;

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
                UILogManager.log.Write("Join Lobby request failed");
                yield break;
            }
            currentLobby = joinLobbyTask.Result;

            UILogManager.log.Write("Joined lobby " + currentLobby.Name);

            string joinCode = currentLobby.Data["Address"].Value;

            UILogManager.log.Write("Join code is " + joinCode);

            OnLobbyJoined?.Invoke(joinCode);
        }
    }
    #endregion

    #region Helper Functions
    private async Task<Player> GetPlayerFromAnonymousLoginAsync()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            UILogManager.log.Write("Trying to log in a player ...");

            // Use Unity Authentication to log in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                UILogManager.log.Write("Player was not signed in successfully; unable to continue without a logged in player");
                throw new InvalidOperationException("Player was not signed in successfully; unable to continue without a logged in player");
            }
        }
        UILogManager.log.Write("Player signed in as " + AuthenticationService.Instance.PlayerId);

        // Player objects have Get-only properties, so you need to initialize the data bag here if you want to use it
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>());
    }

    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        UILogManager.log.Write("Lobby Heartbeat");
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Debug.Log("Heartbeat");
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
    #endregion

    #region Disposal

    public void DisconnectFromLobby()
    {
        if(IsConnectedToLobby && currentLobby != null)
        {
            LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, loggedInPlayer.Id);
            IsConnectedToLobby = false;
        }
    }

    public void StopLobby()
    {
        StopAllCoroutines();
        if (IsConnectedToLobby && currentLobby != null && currentLobby.HostId == loggedInPlayer.Id)
        {
            Lobbies.Instance.DeleteLobbyAsync(currentLobby.Id);
            IsConnectedToLobby = false;
        }
    }

    #endregion
}
