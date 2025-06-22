using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Component.Transforming;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Visual Elements")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private TextMesh _nameText;
    
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    
    // Variables synchronisées sur le réseau
    private readonly SyncVar<string> _playerName = new SyncVar<string>("Joueur");
    
    private readonly SyncVar<bool> _isMale = new SyncVar<bool>(true);
    
    // Composants
    private NetworkTransform _netTransform;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _netTransform = GetComponent<NetworkTransform>();
        _rb = GetComponent<Rigidbody2D>();
        _playerName.OnChange += OnPlayerNameChanged;
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // Configurer la caméra pour suivre ce joueur si c'est le local player
        if (IsOwner)
        {
            Camera.main.GetComponent<CameraFollow>()?.SetTarget(transform);
            
            // Récupérer et envoyer le nom du joueur
            string savedName = PlayerPrefs.GetString("PlayerName", "Joueur");
            SetPlayerNameServerRpc(savedName);
        }
        
        // Mettre à jour le nom affiché
        if (_nameText != null)
        {
            _nameText.text = _playerName.Value;
        }
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        
        // Gestion des entrées seulement pour le joueur local
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
        
        // Appliquer le mouvement
        _rb.linearVelocity = moveDirection * _moveSpeed;
    }
    
    // Appelé lorsque le nom du joueur change
    private void OnPlayerNameChanged(string oldValue, string newValue, bool asServer)
    {
        if (_nameText != null)
        {
            _nameText.text = newValue;
        }
    }
    
    [ServerRpc]
    private void SetPlayerNameServerRpc(string name)
    {
        _playerName.Value = name;
    }
    
    [ServerRpc]
    public void SetGenderServerRpc(bool isMale)
    {
        _isMale.Value = isMale;
        
        // Ici vous pourriez changer le sprite selon le genre
    }
}