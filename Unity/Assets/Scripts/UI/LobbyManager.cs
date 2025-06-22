using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.SceneManagement;
using System;

public class LobbyManager : NetworkBehaviour
{
    [Header("Player List")]
    [SerializeField] private Transform _playerListContainer;
    [SerializeField] private GameObject _playerEntryPrefab;
    
    [Header("Controls")]
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _leaveButton;
    [SerializeField] private TMP_Text _roomCodeText;
    
    private readonly SyncList<PlayerInfo> _players = new SyncList<PlayerInfo>();
    
    private readonly SyncVar<bool> _gameStarted = new SyncVar<bool>(false);
    
    private MultiplayerManager _networkManager;
    private bool _isReady = false;
    
    private void Start()
    {
        _networkManager = MultiplayerManager.Instance;
        Invoke("SetupUI", 0.5f);
    }

    private void SetupUI()
    {
        // Configurer les boutons
        _startGameButton.gameObject.SetActive(_networkManager.IsHost);
        _startGameButton.onClick.AddListener(StartGame);
        
        _readyButton.onClick.AddListener(ToggleReady);
        _leaveButton.onClick.AddListener(LeaveLobby);
        
        Debug.Log($"LobbyManager started. IsHost: {_networkManager.IsHost}, IsServer: {_networkManager.IsServer}");
        // Afficher le code d'invitation
        if (_networkManager.IsHost)
        {
            _roomCodeText.text = $"Code d'invitation: {_networkManager.CurrentRoomCode}";
        }
        
        // S'inscrire aux événements
        _players.OnChange += UpdatePlayerList;
        
        // Ajouter ce joueur
        if (_networkManager.IsServer)
        {
            AddPlayerServerRpc(PlayerPrefs.GetString("PlayerName", "Joueur"));
        }
    }
    
    private void OnDestroy()
    {
        _players.OnChange -= UpdatePlayerList;
    }
    
    // Mise à jour de la liste des joueurs dans l'UI
    private void UpdatePlayerList(SyncListOperation op, int index, PlayerInfo oldItem, PlayerInfo newItem, bool asServer)
    {
        // Nettoyer les entrées existantes
        foreach (Transform child in _playerListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Recréer la liste des joueurs
        foreach (var player in _players)
        {
            GameObject entry = Instantiate(_playerEntryPrefab, _playerListContainer);
            
            TMP_Text nameText = entry.transform.Find("NameText").GetComponent<TMP_Text>();
            TMP_Text statusText = entry.transform.Find("StatusText").GetComponent<TMP_Text>();
            
            nameText.text = player.Name;
            statusText.text = player.IsReady ? "Prêt" : "En attente";
            statusText.color = player.IsReady ? Color.green : Color.red;
        }
        
        // Activer le bouton de démarrage si tous les joueurs sont prêts
        if (_networkManager.IsHost)
        {
            bool allReady = true;
            foreach (var player in _players)
            {
                if (player.Connection != 0 && !player.IsReady) // Ignorer l'hôte
                {
                    allReady = false;
                    break;
                }
            }
            
            _startGameButton.interactable = allReady && _players.Count > 0;
        }
    }
    
    private void ToggleReady()
    {
        _isReady = !_isReady;
        _readyButton.GetComponentInChildren<TMP_Text>().text = _isReady ? "Annuler prêt" : "Prêt";
        
        SetReadyServerRpc(_isReady);
    }
    
    private void StartGame()
    {
        if (_networkManager.IsHost)
        {
            StartGameServerRpc();
        }
    }
    
    private void LeaveLobby()
    {
        _networkManager.StopNetwork();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerServerRpc(string playerName, FishNet.Connection.NetworkConnection conn = null)
    {
        PlayerInfo newPlayer = new PlayerInfo
        {
            Connection = conn != null ? conn.ClientId : 0,
            Name = playerName,
            IsReady = false
        };
        
        _players.Add(newPlayer);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(bool isReady, FishNet.Connection.NetworkConnection conn = null)
    {
        if (conn == null) return;
        
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].Connection == conn.ClientId)
            {
                PlayerInfo updatedInfo = _players[i];
                updatedInfo.IsReady = isReady;
                _players[i] = updatedInfo;
                break;
            }
        }
    }
    
    [ServerRpc(RequireOwnership = true)]
    private void StartGameServerRpc()
    {
        if (!IsServer) return;
        
        _gameStarted.Value = true;
        LoadGameSceneObserversRpc();
    }

    [ObserversRpc]
    private void LoadGameSceneObserversRpc()
    {

        UnityEngine.SceneManagement.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
}

// Structure pour stocker les informations des joueurs dans le lobby
public struct PlayerInfo
{
    public int Connection;
    public string Name;
    public bool IsReady;
}