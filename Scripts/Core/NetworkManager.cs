using Godot;
using System;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Core
{
    public partial class NetworkManager : Node
    {
        // Singleton instance
        public static NetworkManager Instance { get; private set; }
        
        // Signals
        [Signal]
        public delegate void ConnectionEstablishedEventHandler();
        
        [Signal]
        public delegate void ConnectionFailedEventHandler(string reason);
        
        [Signal]
        public delegate void PlayerConnectedEventHandler(int peerId);
        
        [Signal]
        public delegate void PlayerDisconnectedEventHandler(int peerId);
        
        [Signal]
        public delegate void ServerDisconnectedEventHandler();

        // Network configuration
        private const int DEFAULT_PORT = 31400;
        private const int MAX_CLIENTS = 4;
        private const string DEFAULT_IP = "127.0.0.1";

        // Multiplayer configuration
        private ENetMultiplayerPeer _peer;
        private SceneMultiplayer _multiplayerInstance;
        
        // Player info tracking
        private Dictionary<int, Dictionary<string, Variant>> _playerInfo = new();
        private int _localPlayerId = 1; // Default for singleplayer
        
        // Network state
        public enum NetworkMode { Offline, Server, Client }
        private NetworkMode _currentMode = NetworkMode.Offline;
        public NetworkMode CurrentMode => _currentMode;
        
        public bool IsNetworkActive => _multiplayerInstance != null && _multiplayerInstance.HasMultiplayerPeer();
        public bool IsServer => _currentMode == NetworkMode.Server;
        public bool IsConnected => IsNetworkActive && _multiplayerInstance.MultiplierPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;
        public int LocalPlayerId => _localPlayerId;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        public override void _Ready()
        {
            GD.Print("NetworkManager initialized");
            
            // Setup default multiplayer instance
            _multiplayerInstance = GetTree().GetMultiplayer();
            
            // Connect multiplayer signals
            GetTree().ConnectToSignal("connected_to_server", Callable.From(OnConnectedToServer));
            GetTree().ConnectToSignal("connection_failed", Callable.From(OnConnectionFailed));
            GetTree().ConnectToSignal("server_disconnected", Callable.From(OnServerDisconnected));
            
            // Register RPC methods
            _multiplayerInstance.ConnectToSignal("peer_connected", Callable.From(OnPeerConnected));
            _multiplayerInstance.ConnectToSignal("peer_disconnected", Callable.From(OnPeerDisconnected));
        }

        public void StartHosting(string playerName = "Host", string playerClass = "OccultDetective")
        {
            GD.Print("Starting server...");
            
            // Create server as ENet peer
            _peer = new ENetMultiplayerPeer();
            Error error = _peer.CreateServer(DEFAULT_PORT, MAX_CLIENTS);
            
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to create server: {error}");
                EmitSignal(SignalName.ConnectionFailed, $"Could not create server: {error}");
                return;
            }
            
            // Setup multiplayer API
            _multiplayerInstance.MultiplayerPeer = _peer;
            _currentMode = NetworkMode.Server;
            _localPlayerId = 1; // Server is always ID 1
            
            // Register local player info
            RegisterPlayerInfo(_localPlayerId, playerName, playerClass);
            
            // Store host player in GameManager
            GameManager.Instance.AddPlayer(_localPlayerId, playerName, playerClass);
            
            GD.Print($"Server started on port {DEFAULT_PORT}");
            EmitSignal(SignalName.ConnectionEstablished);
        }

        public void JoinGame(string address = DEFAULT_IP, int port = DEFAULT_PORT, string playerName = "Player", string playerClass = "OccultDetective")
        {
            GD.Print($"Connecting to {address}:{port}...");
            
            // Create client as ENet peer
            _peer = new ENetMultiplayerPeer();
            Error error = _peer.CreateClient(address, port);
            
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to create client: {error}");
                EmitSignal(SignalName.ConnectionFailed, $"Could not connect to server: {error}");
                return;
            }
            
            // Setup multiplayer API
            _multiplayerInstance.MultiplayerPeer = _peer;
            _currentMode = NetworkMode.Client;
            
            // Store player info to send to server when connected
            _playerInfo.Clear();
            Dictionary<string, Variant> info = new Dictionary<string, Variant>
            {
                { "name", playerName },
                { "class", playerClass }
            };
            _playerInfo[_multiplayerInstance.GetUniqueId()] = info;
        }

        public void Disconnect()
        {
            if (IsNetworkActive)
            {
                _multiplayerInstance.MultiplayerPeer = null;
                
                if (_peer != null)
                {
                    _peer.Close();
                    _peer = null;
                }
                
                _currentMode = NetworkMode.Offline;
                _playerInfo.Clear();
                
                GD.Print("Disconnected from network");
            }
        }

        private void OnConnectedToServer()
        {
            GD.Print("Connected to server");
            _localPlayerId = _multiplayerInstance.GetUniqueId();
            
            // Send player info to server
            Dictionary<string, Variant> myInfo = _playerInfo[_localPlayerId];
            Rpc(MethodName.RegisterPlayer, _localPlayerId, myInfo["name"].AsString(), myInfo["class"].AsString());
            
            EmitSignal(SignalName.ConnectionEstablished);
        }

        private void OnConnectionFailed()
        {
            GD.PrintErr("Connection failed");
            _currentMode = NetworkMode.Offline;
            EmitSignal(SignalName.ConnectionFailed, "Connection failed or timed out");
        }

        private void OnServerDisconnected()
        {
            GD.Print("Server disconnected");
            _currentMode = NetworkMode.Offline;
            EmitSignal(SignalName.ServerDisconnected);
            
            // Return to main menu
            GameManager.Instance.ReturnToMainMenu();
        }

        private void OnPeerConnected(int id)
        {
            GD.Print($"Peer connected: {id}");
            EmitSignal(SignalName.PlayerConnected, id);
            
            // If we're the server, tell the new peer about existing players
            if (IsServer)
            {
                foreach (int peerId in _playerInfo.Keys)
                {
                    Dictionary<string, Variant> peerInfo = _playerInfo[peerId];
                    RpcId(id, MethodName.RegisterPlayer, peerId, peerInfo["name"].AsString(), peerInfo["class"].AsString());
                }
            }
        }

        private void OnPeerDisconnected(int id)
        {
            GD.Print($"Peer disconnected: {id}");
            
            if (_playerInfo.ContainsKey(id))
            {
                string playerName = _playerInfo[id]["name"].AsString();
                GD.Print($"Player disconnected: {playerName} (ID: {id})");
                _playerInfo.Remove(id);
                
                // Update GameManager
                GameManager.Instance.RemovePlayer(id);
            }
            
            EmitSignal(SignalName.PlayerDisconnected, id);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        private void RegisterPlayer(int id, string name, string playerClass)
        {
            GD.Print($"Registering player: {name} (ID: {id}) as {playerClass}");
            
            RegisterPlayerInfo(id, name, playerClass);
            
            // If we're the server, relay this to all clients
            if (IsServer && id != 1) // Not server itself
            {
                Rpc(MethodName.RegisterPlayer, id, name, playerClass);
            }
            
            // Add player to GameManager
            GameManager.Instance.AddPlayer(id, name, playerClass);
        }

        private void RegisterPlayerInfo(int id, string name, string playerClass)
        {
            Dictionary<string, Variant> info = new Dictionary<string, Variant>
            {
                { "name", name },
                { "class", playerClass }
            };
            _playerInfo[id] = info;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void SyncGameState(int level, int motesCollected, float runTime)
        {
            // Only respond to server messages for game state
            if (_multiplayerInstance.GetRemoteSenderId() != 1) return;
            
            // Update game state for clients
            GameManager.Instance.CurrentLevel = level;
            GameManager.Instance.MotesCollected = motesCollected;
            GameManager.Instance.RunTime = runTime;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void PlayerDied(int playerId)
        {
            GameManager.Instance.PlayerDied(playerId);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void PlayerRevived(int playerId)
        {
            GameManager.Instance.PlayerRevived(playerId);
        }
    }
}
