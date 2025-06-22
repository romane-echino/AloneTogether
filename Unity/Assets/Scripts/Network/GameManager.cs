using UnityEngine;
using FishNet.Object;
using FishNet;
using FishNet.Connection;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        // Ici, nous pourrions initialiser le monde, les PNJ, etc.
        Debug.Log("Le serveur a démarré la partie");
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (IsServer) return; // Le serveur gère déjà son propre joueur
        
        // Créer un joueur local
        Debug.Log("Client prêt, en attente du spawn du joueur");
    }
    
    // Spawn un joueur quand un client se connecte
    /*public override void OnSpawnPlayer(NetworkConnection conn)
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);

        GameObject playerObj = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(playerObj, conn);

        Debug.Log($"Joueur créé pour la connexion {conn.ClientId}");
    }*/
}