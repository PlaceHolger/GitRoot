using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField]
    private TMP_InputField m_PlayerName;
    [SerializeField]
    private TMP_InputField m_LobbyName;
    [SerializeField]
    private TMP_InputField m_NumPlayer;

    [SerializeField]
    private Transform m_LobbyListParent;
    [SerializeField]
    private GameObject m_LobbyUiPrefab;
    [SerializeField]
    private Button m_CreateLobbyButton;
    [SerializeField]
    private Button m_CloseLobbyButton;
    [SerializeField]
    private Button m_LeaveLobbyButton;
    [SerializeField]
    private Button m_UpdateLobbiesButton;
    [SerializeField]
    private Button m_StartGameButton;
    [SerializeField]
    private RelayManager m_RelayManager; 
    
    private Lobby m_CreatedLobby;
    private Lobby m_JoinedLobby;
    private float m_TimeForNextLobbyKeepAlivePing;
    private float m_TimeForNextLobbyRefresh;
    

    public UnityEvent OnGameStart; 
    
    private void Start()
    {
        m_PlayerName.text = $"Unnamed {UnityEngine.Random.Range(1, 100)}"; 
        m_CreateLobbyButton.onClick.AddListener(CreateLobby);
        m_CloseLobbyButton.onClick.AddListener(CloseLobby);
        m_StartGameButton.onClick.AddListener(StartGame);
        m_LeaveLobbyButton.onClick.AddListener(async () => await LeaveLobby());
        enabled = false;
    }
    
    private static void OnAuthenticationSignIn()
    {
        Debug.Log($"Signed in user '{AuthenticationService.Instance.PlayerId}'" );
    }

    public async void SignIn()
    {
        var initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(m_PlayerName.text.GetHashCode().ToString()); //we use a profile here, to archive that we can have multiple clients on the same PC
        await UnityServices.InitializeAsync(initializationOptions);
        AuthenticationService.Instance.SignedIn += OnAuthenticationSignIn;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        m_UpdateLobbiesButton.interactable = true;
        enabled = true;
    }

    private void Update()
    {
        KeepLobbyAlive();
        m_CreateLobbyButton.interactable = m_CreatedLobby == null && m_JoinedLobby == null;
        m_CloseLobbyButton.interactable = m_CreatedLobby != null;
        m_LeaveLobbyButton.interactable = m_JoinedLobby != null;

        if (m_TimeForNextLobbyRefresh < Time.time)
        {
            m_StartGameButton.interactable = (m_CreatedLobby != null && m_CreatedLobby.Players.Count > 1 && NetworkManager.Singleton.ConnectedClients.Count > 1);
            ListLobbies();
        }
    }

    private async void KeepLobbyAlive()
    {
        if (m_CreatedLobby != null && m_TimeForNextLobbyKeepAlivePing < Time.time)
        {
            m_TimeForNextLobbyKeepAlivePing = Time.time + 15; //we ping our created lobby every 15 seconds. Otherwise it will be closed after 30 seconds of inactivity
            await LobbyService.Instance.SendHeartbeatPingAsync(m_CreatedLobby.Id);
        }
    }

    private async void CreateLobby()
    {
        if (m_CreatedLobby != null)
            return;
        
        try
        {
            int numPlayers = int.Parse(m_NumPlayer.text);
            var relayCode = await m_RelayManager.SetupRelay(numPlayers);
            
            Debug.Log($"Creating Lobby '{m_LobbyName.text}, relay joincode {relayCode}'" );
            var lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            };
            m_CreatedLobby = await LobbyService.Instance.CreateLobbyAsync(m_LobbyName.text, numPlayers, lobbyOptions);
            Debug.Log($"Created Lobby '{m_CreatedLobby.Name}' for {m_CreatedLobby.MaxPlayers} players, lobbycode: {m_CreatedLobby.LobbyCode}" );
            m_CreateLobbyButton.interactable = false;
            NetworkManager.Singleton.StartHost();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"Exception '{e}'" );            
        }
    }

    public void StartGame()
    {
        m_StartGameButton.enabled = false;
        m_StartGameButton.GetComponentInChildren<TMP_Text>().text = "Starting in 3 seconds...";
        Invoke(nameof(StartGameServerRpc), 3);
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        Debug.Log("StartGameServerRpc");
        StartGameClientRpc();
    }

    [ClientRpc(Delivery = RpcDelivery.Reliable)]
    private void StartGameClientRpc()
    {
        Debug.Log("StartGameClientRpc");
        OnGameStart.Invoke();
    }

    private Unity.Services.Lobbies.Models.Player GetPlayer()
    {
        return new Unity.Services.Lobbies.Models.Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, m_PlayerName.text) }
            }
        };
    }

    private async void JoinLobbyById(string lobbyId)
    {
        await LeaveLobby();
        if (m_JoinedLobby == null)
        {
            CloseLobby(); //close our own, before joining another lobby
            try
            {
                Debug.Log($"Joining Lobby '{lobbyId}'" );
                JoinLobbyByIdOptions lobbyOptions = new JoinLobbyByIdOptions() { Player = GetPlayer() };
                m_JoinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, lobbyOptions);
                string relayCode = m_JoinedLobby.Data["RelayJoinCode"].Value;

                await m_RelayManager.JoinRelay(relayCode);
                NetworkManager.Singleton.StartClient();

                Debug.Log($"Joined Lobby '{m_JoinedLobby.Name}'" );
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Exception '{e}'" );
            }
        }
    }

    private async Task LeaveLobby()
    {
        if (m_JoinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(m_JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
                m_JoinedLobby = null;
                NetworkManager.Singleton.Shutdown();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Exception '{e}'" );
            }
        }
    }

    private async void CloseLobby()
    { 
        if (m_CreatedLobby != null)
        {
            Debug.Log($"Closing Lobby '{m_CreatedLobby.Name}'" );
            await LobbyService.Instance.DeleteLobbyAsync(m_CreatedLobby.Id);
            Debug.Log($"Closed lobby" );
            m_CreatedLobby = null;
            NetworkManager.Singleton.Shutdown();
        }
    }

    public async void ListLobbies()
    {
        m_TimeForNextLobbyRefresh = Time.time + 2;
        m_UpdateLobbiesButton.interactable = false;
        
        try
        {
            if (m_CreatedLobby != null)
                m_CreatedLobby = await Lobbies.Instance.GetLobbyAsync(m_CreatedLobby.Id);
            if (m_JoinedLobby != null)
                m_JoinedLobby = await Lobbies.Instance.GetLobbyAsync(m_JoinedLobby.Id);
            
            Debug.Log($"Querying Lobbies..." );
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log($"Done. Found {queryResponse.Results.Count} Lobbies." );
            foreach (Transform child in m_LobbyListParent) //first we delete all current lobby-Uis
                Destroy(child.gameObject);
            foreach (var lobby in queryResponse.Results)
            {
                var newLobbyUiGo = Instantiate(m_LobbyUiPrefab, m_LobbyListParent);
                if(m_JoinedLobby != null && lobby.Id == m_JoinedLobby.Id)
                    newLobbyUiGo.GetComponent<Image>().color = Color.green * 0.5f; //highlight joined lobby
                var textComp = newLobbyUiGo.GetComponentInChildren<TMP_Text>();
                textComp.text = $"Join '{lobby.Name}' by '{lobby.Players[0].Data["PlayerName"].Value}' ({lobby.Players.Count}/{lobby.MaxPlayers})";
                var buttonComp = newLobbyUiGo.GetComponentInChildren<Button>();
                if (m_CreatedLobby == null)
                {
                    buttonComp.onClick.AddListener(() => JoinLobbyById(lobby.Id));
                }
                else
                {
                    buttonComp.interactable = false;
                }
                // //debug print players in lobby 
                // Debug.Log($"Lobby {lobby.Name}: Players: ");
                // foreach (var lobbyPlayer in lobby.Players)
                // {
                //     Debug.Log($"{lobbyPlayer.Data["PlayerName"].Value}, Id: {lobbyPlayer.Id}");
                // }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"Exception '{e}'" );
        }

        await Task.Delay(1000);
        m_UpdateLobbiesButton.interactable = true;
    }
}
