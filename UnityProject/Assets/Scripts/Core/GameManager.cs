using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodexOfTheBrokenZodiac.Core
{
    public class GameManager : MonoBehaviour
    {
        // Singleton instance
        public static GameManager Instance { get; private set; }

        // Events
        public event Action<GameState> OnGameStateChanged;
        public event Action<int, string> OnPlayerJoined;
        public event Action<int> OnPlayerLeft;
        public event Action<int> OnMotesCollected;

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
                    OnGameStateChanged?.Invoke(_currentState);
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
            Debug.Log("GameManager initialized");
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                RunTime += Time.deltaTime;
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
                RandomSeed = UnityEngine.Random.Range(0, int.MaxValue);
            }
            
            // Initialize the random number generator with our seed
            UnityEngine.Random.InitState(RandomSeed);
            
            if (isHost)
            {
                // Tell the network manager to start hosting
                NetworkManager networkManager = FindObjectOfType<NetworkManager>();
                if (networkManager != null)
                {
                    networkManager.StartHosting();
                }
            }
            
            // Change game state after setup
            CurrentState = GameState.Playing;
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Time.timeScale = 1f;
            }
        }

        public void EndGame(bool victory)
        {
            CurrentState = victory ? GameState.Victory : GameState.GameOver;
            Time.timeScale = 0f;
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            CurrentState = GameState.MainMenu;
            SceneManager.LoadScene("MainMenu"); // Scene name would match your Unity scene
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
                
                OnPlayerJoined?.Invoke(playerId, playerName);
                Debug.Log($"Player joined: {playerName} (ID: {playerId}) as {playerClass}");
            }
        }

        public void RemovePlayer(int playerId)
        {
            if (_players.ContainsKey(playerId))
            {
                string playerName = _players[playerId].Name;
                _players.Remove(playerId);
                
                OnPlayerLeft?.Invoke(playerId);
                Debug.Log($"Player left: {playerName} (ID: {playerId})");
                
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
            OnMotesCollected?.Invoke(amount);
        }

        public void EnemyDefeated()
        {
            EnemiesDefeated++;
        }

        public void AdvanceToNextLevel()
        {
            CurrentLevel++;
            Debug.Log($"Advancing to level {CurrentLevel}");
            
            // Tell the procedural generation manager to create the next level
            ProcGenManager procGenManager = FindObjectOfType<ProcGenManager>();
            if (procGenManager != null)
            {
                procGenManager.GenerateLevel(CurrentLevel);
            }
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