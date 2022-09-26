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

namespace Multiplayer.LobbyManagement
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField]
        private InputField lobbyName;
        public bool IsConnectedToLobby { get; private set; } = false;
        public Player LoggedInPlayer { get; private set; }
        public Guid PlayerAllocationId { get; private set; }

        public event Action<string> OnLobbyJoined;
        public event Action OnLobbyCreated;

        private Lobby _currentLobby;
        private List<Lobby> _currentLobbyList;
        private bool _unityServicesInitialized = false;

        #region Initialization

        void Start()
        {
            StartCoroutine(InitializeUnityServices());
        }

        public bool IsUnityServicesInitialized()
        {
            if (!_unityServicesInitialized)
            {
                Debug.LogError("Unity Services have not been initialized!");
                return false;
            }
            else 
            {
                return true;
            }
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
                Debug.LogError("Failed Player Log-in!");
                yield break;
            }
            else
                LoggedInPlayer = logInTask.Result;

            _unityServicesInitialized = true;
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
            if (!IsUnityServicesInitialized()) yield break;

            // Add some data to our player
            // This data will be included in a lobby under players -> player.data
            LoggedInPlayer.Data.Add("Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "No"));

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
                    Player = LoggedInPlayer
                });
            while (!createLobby.IsCompleted)
                yield return null;
            if (createLobby.IsFaulted)
            {
                Debug.LogError("Lobby Creation Failed!");
                yield break;
            }
            _currentLobby = createLobby.Result;
            StartCoroutine(HeartbeatLobbyCoroutine(_currentLobby.Id, 15));
            IsConnectedToLobby = true;
            OnLobbyCreated?.Invoke();
        }

        private IEnumerator FindLobbys()
        {
            if (!IsUnityServicesInitialized()) yield break;

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
                Debug.LogError("Lobby list retrieval failed!");
            }
            QueryResponse response = findLobbyQuery.Result;

            _currentLobbyList = response.Results;
        }

        private IEnumerator JoinFirstLobby()
        {
            if (!IsUnityServicesInitialized()) yield break;

            if (_currentLobbyList.Count > 0)
            {
                var joinLobbyTask = LobbyService.Instance.JoinLobbyByIdAsync(
                    lobbyId: _currentLobbyList[0].Id,
                    options: new JoinLobbyByIdOptions()
                    {
                        Player = LoggedInPlayer
                    });
                while (!joinLobbyTask.IsCompleted)
                {
                    yield return null;
                }
                if (joinLobbyTask.IsFaulted)
                {
                    Debug.LogError("Join Lobby request failed");
                    yield break;
                }
                _currentLobby = joinLobbyTask.Result;
                string joinCode = _currentLobby.Data["Address"].Value;
                OnLobbyJoined?.Invoke(joinCode);
            }
        }
        #endregion

        #region Helper Functions
        private async Task<Player> GetPlayerFromAnonymousLoginAsync()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                // Use Unity Authentication to log in
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    throw new InvalidOperationException("Player was not signed in successfully; unable to continue without a logged in player");
                }
            }

            // Player objects have Get-only properties, so you need to initialize the data bag here if you want to use it
            return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>());
        }

        private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);
            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }
        #endregion

        #region Disposal

        public void DisconnectFromLobby()
        {
            if (IsConnectedToLobby && _currentLobby != null)
            {
                LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, LoggedInPlayer.Id);
                IsConnectedToLobby = false;
            }
        }

        public void StopLobby()
        {
            StopAllCoroutines();
            if (IsConnectedToLobby && _currentLobby != null && _currentLobby.HostId == LoggedInPlayer.Id)
            {
                Lobbies.Instance.DeleteLobbyAsync(_currentLobby.Id);
                IsConnectedToLobby = false;
            }
        }

        #endregion
    } 
}
