using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance { get; private set; }

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private Tugboat _transport;
    
    [Header("Room Settings")]
    [SerializeField] private ushort _defaultPort = 7777;
    [SerializeField] private int _maxPlayers = 6;
    [SerializeField] private string _localIp = "127.0.0.1";
    
    private string _currentRoomCode = "";
    private const int CODE_LENGTH = 6;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
        // Référencer les composants
        if (_networkManager == null)
            _networkManager = GetComponent<NetworkManager>();
        if (_transport == null)
            _transport = GetComponent<Tugboat>();
    }

    // Générer un code d'invitation aléatoire
    public string GenerateInviteCode()
    {
        string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Évite les caractères ambigus
        char[] codeArray = new char[CODE_LENGTH];
        
        for (int i = 0; i < CODE_LENGTH; i++)
        {
            codeArray[i] = chars[Random.Range(0, chars.Length)];
        }
        
        _currentRoomCode = new string(codeArray);
        return _currentRoomCode;
    }
    
    // L'hôte crée une session
    public void StartHost()
    {
        _transport.SetServerBindAddress(_localIp, FishNet.Transporting.IPAddressType.IPv4);
        _transport.SetPort(_defaultPort);
        //_transport.ServerMaximumClients = (ushort)_maxPlayers;
        
        _networkManager.ServerManager.StartConnection();
        _networkManager.ClientManager.StartConnection();
        
        GenerateInviteCode();
        Debug.Log($"Hébergement démarré avec le code: {_currentRoomCode}");
        
        // Pour un vrai système, vous stockeriez ce code dans une base de données
        // avec l'adresse IP de l'hôte
    }
    
    // Un client rejoint une session
    public void JoinGame(string inviteCode)
    {
        // Pour simplifier, nous utilisons le même port pour tout le monde
        // Dans un système réel, vous traduiriez le code en adresse IP via une base de données
        _transport.SetClientAddress(_localIp);
        _transport.SetPort(_defaultPort);
        
        _networkManager.ClientManager.StartConnection();
        Debug.Log($"Connexion au jeu avec le code: {inviteCode}");
    }
    
    // Arrêter la connexion réseau
    public void StopNetwork()
    {
        _networkManager.ServerManager.StopConnection(true);
        _networkManager.ClientManager.StopConnection();
    }
    
    // Propriétés utiles
    public bool IsHost => _networkManager.IsHostStarted;
    public bool IsServer => _networkManager.IsServerStarted;
    public bool IsClient => _networkManager.IsClientStarted;
    public string CurrentRoomCode => _currentRoomCode;
}