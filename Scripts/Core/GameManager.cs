using Godot;
using System;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Core
{
    public partial class GameManager : Node
    {
        // Singleton instance
        public static GameManager Instance { get; private set; }

        [Signal]
        public delegate void GameStateChangedEventHandler(GameState newState);

        [Signal]
        public delegate void PlayerJoinedEventHandler(int playerId, string playerName);

        [Signal]
        public delegate void PlayerLeftEventHandler(int playerId);

        [Signal]
        public delegate void MotesCollectedEventHandler(int amount);

        // Game state tracking
        public enum GameState
        {
            MainMenu,
            Connecting,
            Playing,
            Paused,
            GameOver,
            Victory
        }

        private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    EmitSignal(SignalName.GameStateChanged, (int)_currentState);
                }
            }
        }

        // Player tracking
        private Dictionary<int, PlayerInfo> _players = new Dictionary<int, PlayerInfo>();
        
        // Run statistics
        public int CurrentLevel { get; private set; } = 1;
        public int MotesCollected { get; private set; } = 0;
        public int EnemiesDefeated { get; private set; } = 0;
        public float RunTime { get; private set; } = 0f;
        
        // Current run settings
        public int RandomSeed { get; private set; } = 0;
        public bool IsDailyRun { get; private set; } = false;

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
            GD.Print("GameManager initialized");
        }

        public override void _Process(double delta)
        {
            if (CurrentState == GameState.Playing)
            {
                RunTime += (float)delta;
            }
        }

        public void StartNewGame(bool isHost = true, bool isDailyRun = false)
        {
            _players.Clear();
            CurrentLevel = 1;
            MotesCollected = 0;
            EnemiesDefeated = 0;
            RunTime = 0f;
            
            this.IsDailyRun = isDailyRun;
            
            if (isDailyRun)
            {
                // Daily run seeds are based on the date
                DateTime today = DateTime.Today;
                RandomSeed = today.Year * 10000 + today.Month * 100 + today.Day;
            }
            else
            {
                // Random seed for normal runs
                RandomSeed = new Random().Next();
            }
            
            // Initialize the random number generator with our seed
            GD.Randomize();
            GD.Seed((ulong)RandomSeed);
            
            if (isHost)
            {
                // Tell the network manager to start hosting
                Node networkManager = GetNode<Node>("/root/NetworkManager");
                networkManager.Call("StartHosting");
            }
            
            // Change game state after setup
            CurrentState = GameState.Playing;
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                GetTree().Paused = true;
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                GetTree().Paused = false;
            }
        }

        public void EndGame(bool victory)
        {
            CurrentState = victory ? GameState.Victory : GameState.GameOver;
            GetTree().Paused = true;
        }

        public void ReturnToMainMenu()
        {
            GetTree().Paused = false;
            CurrentState = GameState.MainMenu;
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }

        public void AddPlayer(int playerId, string playerName, string playerClass)
        {
            if (!_players.ContainsKey(playerId))
            {
                _players[playerId] = new PlayerInfo
                {
                    Id = playerId,
                    Name = playerName,
                    Class = playerClass,
                    IsAlive = true
                };
                
                EmitSignal(SignalName.PlayerJoined, playerId, playerName);
                GD.Print($"Player joined: {playerName} (ID: {playerId}) as {playerClass}");
            }
        }

        public void RemovePlayer(int playerId)
        {
            if (_players.ContainsKey(playerId))
            {
                string playerName = _players[playerId].Name;
                _players.Remove(playerId);
                
                EmitSignal(SignalName.PlayerLeft, playerId);
                GD.Print($"Player left: {playerName} (ID: {playerId})");
                
                // Check if all players are gone
                if (_players.Count == 0 && CurrentState == GameState.Playing)
                {
                    EndGame(false);
                }
            }
        }

        public void PlayerDied(int playerId)
        {
            if (_players.ContainsKey(playerId))
            {
                _players[playerId].IsAlive = false;
                
                // Check if all players are dead
                bool allDead = true;
                foreach (var player in _players.Values)
                {
                    if (player.IsAlive)
                    {
                        allDead = false;
                        break;
                    }
                }
                
                if (allDead)
                {
                    EndGame(false);
                }
            }
        }

        public void PlayerRevived(int playerId)
        {
            if (_players.ContainsKey(playerId))
            {
                _players[playerId].IsAlive = true;
            }
        }

        public void CollectMotes(int amount)
        {
            MotesCollected += amount;
            EmitSignal(SignalName.MotesCollected, amount);
        }

        public void EnemyDefeated()
        {
            EnemiesDefeated++;
        }

        public void AdvanceToNextLevel()
        {
            CurrentLevel++;
            GD.Print($"Advancing to level {CurrentLevel}");
            
            // Tell the procedural generation manager to create the next level
            GetNode<Node>("/root/ProcGenManager").Call("GenerateLevel", CurrentLevel);
        }
    }

    public class PlayerInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public bool IsAlive { get; set; }
    }
}
