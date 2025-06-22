using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject _mainPanel;
    
    [Header("Host Panel")]
    [SerializeField] private GameObject _hostPanel;
    [SerializeField] private TMP_InputField _hostNameInput;
    [SerializeField] private Button _startHostButton;
    [SerializeField] private Button _backFromHostButton;
  
    
    [Header("Join Panel")]
    [SerializeField] private GameObject _joinPanel;
    [SerializeField] private TMP_InputField _joinNameInput;
    [SerializeField] private TMP_InputField _roomCodeInput;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _backFromJoinButton;
    
    private MultiplayerManager _networkManager;

    private void Start()
    {
        _networkManager = MultiplayerManager.Instance;
       foreach(Transform child in transform)
{
    Console.WriteLine($"Child: {child.name}");
}
        // Configurer les boutons
        Button hostButton = transform.Find("MainPanel/HostButton").GetComponent<Button>();
        Button joinButton = transform.Find("MainPanel/JoinButton").GetComponent<Button>();
        Button quitButton = transform.Find("MainPanel/QuitButton").GetComponent<Button>();
        
        hostButton.onClick.AddListener(ShowHostPanel);
        joinButton.onClick.AddListener(ShowJoinPanel);
        quitButton.onClick.AddListener(() => Application.Quit());
        
        _startHostButton.onClick.AddListener(StartHosting);
        _backFromHostButton.onClick.AddListener(() => ShowPanel(_mainPanel));
        
        _joinButton.onClick.AddListener(JoinGame);
        _backFromJoinButton.onClick.AddListener(() => ShowPanel(_mainPanel));
        
        // Afficher le panel principal au départ
        ShowPanel(_mainPanel);
    }

    private void ShowPanel(GameObject panel)
    {
        _mainPanel.SetActive(panel == _mainPanel);
        _hostPanel.SetActive(panel == _hostPanel);
        _joinPanel.SetActive(panel == _joinPanel);
    }
    
    private void ShowHostPanel() => ShowPanel(_hostPanel);
    private void ShowJoinPanel() => ShowPanel(_joinPanel);

    private void StartHosting()
    {
        string playerName = _hostNameInput.text;
        if (string.IsNullOrEmpty(playerName)) playerName = "Hôte";
        
        PlayerPrefs.SetString("PlayerName", playerName);
        
        // Démarrer l'hébergement
        _networkManager.StartHost();
        
        // Afficher le code
        string code = _networkManager.CurrentRoomCode;
        
        // Dans une vraie implémentation, nous chargerions la scène du lobby
        SceneManager.LoadScene("Lobby");
    }

    private void JoinGame()
    {
        string playerName = _joinNameInput.text;
        if (string.IsNullOrEmpty(playerName)) playerName = "Invité";
        
        string roomCode = _roomCodeInput.text.ToUpper();
        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6)
        {
            Debug.LogError("Code d'invitation invalide");
            return;
        }
        
        PlayerPrefs.SetString("PlayerName", playerName);
        
        // Se connecter au jeu
        _networkManager.JoinGame(roomCode);
        
        // Dans une vraie implémentation, nous chargerions la scène du lobby
        SceneManager.LoadScene("Lobby");
    }
}