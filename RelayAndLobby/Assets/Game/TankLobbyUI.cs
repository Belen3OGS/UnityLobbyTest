using Epic.OnlineServices.Lobby;
using UnityEngine;
using System.Collections.Generic;
using EpicTransport;
using Mirror;
using UnityEngine.UI;
using System.Collections;

public class TankLobbyUI : EOSLobby
{
    public InputField lobbyName; 
    private bool showLobbyList = false;
    private bool showPlayerList = false;

    private List<LobbyDetails> foundLobbies = new List<LobbyDetails>();
    private List<Attribute> lobbyData = new List<Attribute>();

    //register events
    private void OnEnable()
    {
        //subscribe to events
        CreateLobbySucceeded += OnCreateLobbySuccess;
        JoinLobbySucceeded += OnJoinLobbySuccess;
        FindLobbiesSucceeded += OnFindLobbiesSuccess;
        LeaveLobbySucceeded += OnLeaveLobbySuccess;
    }

    //deregister events
    private void OnDisable()
    {
        //unsubscribe from events
        CreateLobbySucceeded -= OnCreateLobbySuccess;
        JoinLobbySucceeded -= OnJoinLobbySuccess;
        FindLobbiesSucceeded -= OnFindLobbiesSuccess;
        LeaveLobbySucceeded -= OnLeaveLobbySuccess;
    }

    //when the lobby is successfully created, start the host
    private void OnCreateLobbySuccess(List<Attribute> attributes)
    {
        lobbyData = attributes;
        showPlayerList = true;
        showLobbyList = false;

        GetComponent<NetworkManager>().StartHost();
    }

    //when the user joined the lobby successfully, set network address and connect
    private void OnJoinLobbySuccess(List<Attribute> attributes)
    {
        lobbyData = attributes;
        showPlayerList = true;
        showLobbyList = false;

        NetworkManager netManager = GetComponent<NetworkManager>();
        netManager.networkAddress = attributes.Find((x) => x.Data.Key == hostAddressKey).Data.Value.AsUtf8;
        netManager.StartClient();
    }

    //callback for FindLobbiesSucceeded
    private void OnFindLobbiesSuccess(List<LobbyDetails> lobbiesFound)
    {
        foundLobbies = lobbiesFound;
        showPlayerList = false;
        showLobbyList = true;
    }

    //when the lobby was left successfully, stop the host/client
    private void OnLeaveLobbySuccess()
    {
        NetworkManager netManager = GetComponent<NetworkManager>();
        netManager.StopHost();
        netManager.StopClient();
    }

    //ButtonFunctions

    public void CreateLobby()
    {
        CreateLobby(4, LobbyPermissionLevel.Publicadvertised, false, new AttributeData[] { new AttributeData { Key = AttributeKeys[0], Value = lobbyName.text }, });

    }
    public void JoinFirstLobby()
    {
        FindLobbies();

        StartCoroutine(JoinIEnumerator());

    }

    IEnumerator JoinIEnumerator()
    {
        while (foundLobbies.Count == 0)
        {
            yield return null;
        }
        JoinLobby(foundLobbies[0], AttributeKeys);
    }
}
