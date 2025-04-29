using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CodexOfTheBrokenZodiac.Core
{
    public class NetworkManager : MonoBehaviour
    {
        // Singleton instance
        public static NetworkManager Instance { get; private set; }
        
        // Network settings
        [Header("Network Settings")]
        [SerializeField] private int port = 7777;
        [SerializeField] private int maxConnections = 4;
        [SerializeField] private GameObject playerPrefab;
        
        // Events
        public event Action<bool, string> OnConnectionStatusChanged;
        
        // Internal state
        private bool _isHost = false;
        private bool _isConnected = false;
        private int _localPlayerId = 0;
        private string _playerName = "Player";
        private string _playerClass = "OccultDetective";
        
        // Player tracking
        private Dictionary<int, GameObject> _players = new Dictionary<int, GameObject>();
        
        // Connection info
        public bool IsHost => _isHost;
        public bool IsConnected => _isConnected;
        public int LocalPlayerId => _localPlayerId;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // In Unity, we'd set up the actual networking here
            // This is a simplified version for the mock implementation
            _localPlayerId = UnityEngine.Random.Range(1000, 9999);
            Debug.Log($"NetworkManager initialized with local ID: {_localPlayerId}");
        }
        
        public void StartHosting()
        {
            _isHost = true;
            _isConnected = true;
            
            // Create host player
            SpawnLocalPlayer();
            
            Debug.Log("Started hosting game");
            OnConnectionStatusChanged?.Invoke(true, "Game server started");
        }
        
        public void ConnectToServer(string address, int serverPort, string playerName, string playerClass)
        {
            _playerName = playerName;
            _playerClass = playerClass;
            
            // Simulate connection after a delay
            StartCoroutine(SimulateConnection(address, serverPort));
        }
        
        private IEnumerator SimulateConnection(string address, int serverPort)
        {
            Debug.Log($"Connecting to {address}:{serverPort}...");
            OnConnectionStatusChanged?.Invoke(false, "Connecting...");
            
            // Simulate connection delay
            yield return new WaitForSeconds(1.5f);
            
            // Simulate successful connection
            _isConnected = true;
            _isHost = false;
            
            // Create player
            SpawnLocalPlayer();
            
            Debug.Log("Connected to server");
            OnConnectionStatusChanged?.Invoke(true, "Connected to server");
        }
        
        public void Disconnect()
        {
            if (!_isConnected) return;
            
            // Remove all players
            foreach (var player in _players.Values)
            {
                Destroy(player);
            }
            _players.Clear();
            
            // Reset state
            _isConnected = false;
            _isHost = false;
            
            Debug.Log("Disconnected from server");
            OnConnectionStatusChanged?.Invoke(false, "Disconnected from server");
        }
        
        private void SpawnLocalPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("Player prefab not set in NetworkManager");
                return;
            }
            
            // Find spawn point
            Transform spawnPoint = FindSpawnPoint();
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            
            // Instantiate player
            GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            
            // Setup player
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Set player properties
                playerController.SetNetworkId(_localPlayerId);
                playerController.SetPlayerName(_playerName);
                playerController.SetPlayerClass(_playerClass);
            }
            
            // Register with game manager
            GameManager.Instance?.AddPlayer(_localPlayerId, _playerName, _playerClass);
            
            // Add to tracked players
            _players[_localPlayerId] = playerObject;
            
            Debug.Log($"Spawned local player with ID: {_localPlayerId}");
        }
        
        private Transform FindSpawnPoint()
        {
            // Find a spawn point in the scene
            var spawnPoints = FindObjectsOfType<SpawnPoint>();
            if (spawnPoints.Length > 0)
            {
                return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform;
            }
            return null;
        }
        
        public void SendMessage(string message, params object[] data)
        {
            // In a real implementation, this would send network messages
            Debug.Log($"[NetworkMessage] {message}: {string.Join(", ", data)}");
        }
    }
    
    // Simple class to mark spawn points in the scene
    public class SpawnPoint : MonoBehaviour
    {
        public bool IsOccupied { get; set; } = false;
    }
}