using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    public class ProcGenManager : MonoBehaviour
    {
        // Singleton instance
        public static ProcGenManager Instance { get; private set; }
        
        // Events
        public event Action<int> OnLevelGenerated;
        public event Action<int> OnRoomEntered;
        
        // Level generation parameters
        private const int MIN_ROOMS_PER_LEVEL = 5;
        private const int MAX_ROOMS_PER_LEVEL = 12;
        private const int BOSS_ROOM_LEVEL_INTERVAL = 5;
        
        // Current level tracking
        private int _currentLevel = 0;
        private int _currentRoomId = 0;
        private int _totalRooms = 0;
        private List<RoomData> _generatedRooms = new List<RoomData>();
        private bool _isBossLevel = false;
        
        // Room templates
        [SerializeField] private List<GameObject> standardRoomPrefabs;
        [SerializeField] private List<GameObject> specialRoomPrefabs;
        [SerializeField] private GameObject bossRoomPrefab;
        [SerializeField] private GameObject startingRoomPrefab;
        
        // Template names for reference
        private List<string> _standardRoomTemplates = new List<string>
        {
            "RoomTemplate",
            "SmallRoom",
            "MediumRoom",
            "LargeRoom"
        };
        
        private List<string> _specialRoomTemplates = new List<string>
        {
            "ShrineRoom",
            "TarotRoom",
            "TreasureRoom"
        };
        
        private const string BOSS_ROOM_TEMPLATE = "BossRoom";
        private const string STARTING_ROOM_TEMPLATE = "StartingRoom";
        
        // Room connection graph
        private Dictionary<int, List<int>> _roomConnections = new Dictionary<int, List<int>>();
        
        // Enemy difficulty scaling
        private float _difficultyMultiplier = 1.0f;
        
        // Current active room
        private GameObject _currentRoomInstance = null;

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
            Debug.Log("ProcGenManager initialized");
        }

        public void GenerateLevel(int level)
        {
            _currentLevel = level;
            _currentRoomId = 0;
            _totalRooms = 0;
            _generatedRooms.Clear();
            _roomConnections.Clear();
            
            // Scale difficulty with level
            _difficultyMultiplier = 1.0f + (level - 1) * 0.2f;
            
            // Determine if this is a boss level
            _isBossLevel = (level % BOSS_ROOM_LEVEL_INTERVAL == 0);
            
            // Calculate number of rooms based on level
            int baseRooms = Mathf.Min(MIN_ROOMS_PER_LEVEL + level, MAX_ROOMS_PER_LEVEL);
            int variance = Mathf.Min(2, baseRooms / 3);
            _totalRooms = baseRooms + UnityEngine.Random.Range(-variance, variance + 1);
            
            // Always add a starting room
            RoomData startingRoom = CreateRoom(0, STARTING_ROOM_TEMPLATE, RoomType.Starting);
            _generatedRooms.Add(startingRoom);
            
            // If boss level, add a boss room
            if (_isBossLevel)
            {
                RoomData bossRoom = CreateRoom(_totalRooms - 1, BOSS_ROOM_TEMPLATE, RoomType.Boss);
                _generatedRooms.Add(bossRoom);
            }
            
            // Generate standard rooms
            int specialRoomCount = Mathf.Max(1, _totalRooms / 4);
            int standardRoomCount = _totalRooms - specialRoomCount - 1 - (_isBossLevel ? 1 : 0);
            
            for (int i = 0; i < standardRoomCount; i++)
            {
                string template = _standardRoomTemplates[UnityEngine.Random.Range(0, _standardRoomTemplates.Count)];
                RoomData room = CreateRoom(_generatedRooms.Count, template, RoomType.Standard);
                _generatedRooms.Add(room);
            }
            
            // Generate special rooms
            for (int i = 0; i < specialRoomCount; i++)
            {
                string template = _specialRoomTemplates[UnityEngine.Random.Range(0, _specialRoomTemplates.Count)];
                RoomType type = RoomType.Special;
                
                if (template == "ShrineRoom") type = RoomType.Shrine;
                else if (template == "TarotRoom") type = RoomType.Tarot;
                else if (template == "TreasureRoom") type = RoomType.Treasure;
                
                RoomData room = CreateRoom(_generatedRooms.Count, template, type);
                _generatedRooms.Add(room);
            }
            
            // Generate room connections (simple linear path for now with branches)
            GenerateRoomConnections();
            
            // Load the first room
            LoadRoom(0);
            
            // Signal that generation is complete
            OnLevelGenerated?.Invoke(level);
            Debug.Log($"Generated level {level} with {_totalRooms} rooms.");
        }

        private RoomData CreateRoom(int id, string templateName, RoomType type)
        {
            // Create basic room data
            RoomData room = new RoomData
            {
                Id = id,
                TemplateName = templateName,
                Type = type,
                DifficultyMultiplier = _difficultyMultiplier,
                IsCleared = false,
                Position = CalculateRoomPosition(id)
            };
            
            // Generate enemies for this room based on type
            if (type != RoomType.Starting && type != RoomType.Shrine && type != RoomType.Tarot)
            {
                GenerateEnemiesForRoom(room);
            }
            
            // Generate loot for this room
            GenerateLootForRoom(room);
            
            return room;
        }

        private Vector2 CalculateRoomPosition(int id)
        {
            // For now, just a simple grid layout
            int gridSize = (int)Mathf.Sqrt(_totalRooms) + 1;
            int x = id % gridSize;
            int y = id / gridSize;
            
            // Add some variation
            float xOffset = UnityEngine.Random.Range(-0.3f, 0.3f);
            float yOffset = UnityEngine.Random.Range(-0.3f, 0.3f);
            
            return new Vector2(x + xOffset, y + yOffset);
        }

        private void GenerateEnemiesForRoom(RoomData room)
        {
            int baseEnemyCount = 0;
            
            switch (room.Type)
            {
                case RoomType.Standard:
                    baseEnemyCount = 4 + _currentLevel / 2;
                    break;
                case RoomType.Special:
                    baseEnemyCount = 3 + _currentLevel / 3;
                    break;
                case RoomType.Boss:
                    baseEnemyCount = 1; // Boss plus minions
                    break;
                case RoomType.Treasure:
                    baseEnemyCount = 6 + _currentLevel; // More enemies for treasure rooms
                    break;
                default:
                    baseEnemyCount = 0;
                    break;
            }
            
            // Apply randomness to enemy count
            int actualEnemyCount = baseEnemyCount + UnityEngine.Random.Range(-1, 2);
            actualEnemyCount = Mathf.Max(1, actualEnemyCount);
            
            // Initialize the enemies list
            room.Enemies = new List<EnemySpawnData>();
            
            for (int i = 0; i < actualEnemyCount; i++)
            {
                EnemySpawnData enemy = new EnemySpawnData
                {
                    Type = "BasicEnemy",
                    Position = new Vector2(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f)),
                    Health = 100 * room.DifficultyMultiplier,
                    Damage = 10 * room.DifficultyMultiplier
                };
                
                room.Enemies.Add(enemy);
            }
            
            // Add boss if it's a boss room
            if (room.Type == RoomType.Boss)
            {
                EnemySpawnData boss = new EnemySpawnData
                {
                    Type = "BossEnemy",
                    Position = Vector2.zero,
                    Health = 500 * room.DifficultyMultiplier,
                    Damage = 20 * room.DifficultyMultiplier
                };
                
                room.Enemies.Add(boss);
            }
        }

        private void GenerateLootForRoom(RoomData room)
        {
            room.Loot = new List<LootData>();
            
            // Base chance for loot
            float lootChance = 0.0f;
            int motesAmount = 0;
            
            switch (room.Type)
            {
                case RoomType.Standard:
                    lootChance = 0.3f;
                    motesAmount = 10 + _currentLevel * 2;
                    break;
                case RoomType.Special:
                    lootChance = 0.5f;
                    motesAmount = 15 + _currentLevel * 3;
                    break;
                case RoomType.Shrine:
                    lootChance = 0.0f; // No loot in shrine rooms
                    motesAmount = 5 + _currentLevel;
                    break;
                case RoomType.Tarot:
                    lootChance = 1.0f; // Guaranteed tarot card
                    motesAmount = 5 + _currentLevel;
                    break;
                case RoomType.Treasure:
                    lootChance = 1.0f; // Guaranteed loot
                    motesAmount = 30 + _currentLevel * 5;
                    break;
                case RoomType.Boss:
                    lootChance = 1.0f; // Guaranteed loot
                    motesAmount = 50 + _currentLevel * 10;
                    break;
                default:
                    lootChance = 0.0f;
                    motesAmount = 5;
                    break;
            }
            
            // Add motes of insight to all rooms
            LootData motes = new LootData
            {
                Type = LootType.Motes,
                Value = motesAmount,
                Position = new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f))
            };
            room.Loot.Add(motes);
            
            // Add weapon if luck favors it
            if (UnityEngine.Random.value < lootChance)
            {
                LootType type = LootType.Weapon;
                
                // For Tarot rooms, always spawn a Tarot card
                if (room.Type == RoomType.Tarot)
                {
                    type = LootType.TarotCard;
                }
                // For other rooms, randomize between weapon, tarot, or mutagen
                else 
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.6f)
                        type = LootType.Weapon;
                    else if (roll < 0.9f)
                        type = LootType.TarotCard;
                    else
                        type = LootType.Mutagen;
                }
                
                LootData loot = new LootData
                {
                    Type = type,
                    Value = 1,
                    Position = new Vector2(UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(-4f, 4f))
                };
                room.Loot.Add(loot);
            }
        }

        private void GenerateRoomConnections()
        {
            // Initialize connections dictionary
            for (int i = 0; i < _generatedRooms.Count; i++)
            {
                _roomConnections[i] = new List<int>();
            }
            
            // Create a simple linear path through all rooms
            for (int i = 0; i < _generatedRooms.Count - 1; i++)
            {
                ConnectRooms(i, i + 1);
            }
            
            // Add some random connections for branching paths
            int extraConnections = _generatedRooms.Count / 3;
            for (int i = 0; i < extraConnections; i++)
            {
                int from = UnityEngine.Random.Range(0, _generatedRooms.Count - 1);
                
                // Try to find a valid target that's not already connected
                int attempts = 0;
                while (attempts < 5) // Limit attempts to avoid infinite loops
                {
                    int to = UnityEngine.Random.Range(0, _generatedRooms.Count);
                    
                    // Ensure we don't connect to self or create duplicate connections
                    if (to != from && !_roomConnections[from].Contains(to) && 
                        Math.Abs(to - from) <= 3) // Only connect nearby rooms
                    {
                        ConnectRooms(from, to);
                        break;
                    }
                    
                    attempts++;
                }
            }
            
            // Debug print connections
            foreach (var roomId in _roomConnections.Keys)
            {
                string connections = string.Join(", ", _roomConnections[roomId]);
                Debug.Log($"Room {roomId} connects to: {connections}");
            }
        }

        private void ConnectRooms(int fromId, int toId)
        {
            if (!_roomConnections.ContainsKey(fromId))
                _roomConnections[fromId] = new List<int>();
                
            if (!_roomConnections.ContainsKey(toId))
                _roomConnections[toId] = new List<int>();
            
            // Bidirectional connection
            if (!_roomConnections[fromId].Contains(toId))
                _roomConnections[fromId].Add(toId);
                
            if (!_roomConnections[toId].Contains(fromId))
                _roomConnections[toId].Add(fromId);
        }

        public void LoadRoom(int roomId)
        {
            if (roomId < 0 || roomId >= _generatedRooms.Count)
            {
                Debug.LogError($"Invalid room ID: {roomId}");
                return;
            }
            
            // Unload current room if any
            if (_currentRoomInstance != null)
            {
                Destroy(_currentRoomInstance);
                _currentRoomInstance = null;
            }
            
            RoomData roomData = _generatedRooms[roomId];
            _currentRoomId = roomId;
            
            // Load room template based on type
            GameObject prefab = null;
            
            if (roomData.Type == RoomType.Starting)
            {
                prefab = startingRoomPrefab;
            }
            else if (roomData.Type == RoomType.Boss)
            {
                prefab = bossRoomPrefab;  
            }
            else if (roomData.Type == RoomType.Standard)
            {
                int index = _standardRoomTemplates.IndexOf(roomData.TemplateName);
                if (index >= 0 && index < standardRoomPrefabs.Count)
                {
                    prefab = standardRoomPrefabs[index];
                }
            }
            else // Special room types
            {
                int index = _specialRoomTemplates.IndexOf(roomData.TemplateName);
                if (index >= 0 && index < specialRoomPrefabs.Count)
                {
                    prefab = specialRoomPrefabs[index];
                }
            }
            
            // Fallback to a standard room if prefab not found
            if (prefab == null && standardRoomPrefabs.Count > 0)
            {
                prefab = standardRoomPrefabs[0];
                Debug.LogWarning($"Room template not found: {roomData.TemplateName}, using fallback");
            }
            
            if (prefab == null)
            {
                Debug.LogError("Cannot load room - no prefabs available");
                return;
            }
            
            // Instance the room
            _currentRoomInstance = Instantiate(prefab);
            _currentRoomInstance.name = $"Room_{roomId}";
            
            // Configure room if it has a RoomController component
            RoomController roomController = _currentRoomInstance.GetComponent<RoomController>();
            if (roomController != null)
            {
                roomController.SetupRoom(roomData);
            }
            
            // Signal that we've entered a new room
            OnRoomEntered?.Invoke(roomId);
            Debug.Log($"Loaded room {roomId} of type {roomData.Type}");
        }

        public void MarkRoomAsCleared(int roomId)
        {
            if (roomId >= 0 && roomId < _generatedRooms.Count)
            {
                _generatedRooms[roomId].IsCleared = true;
                Debug.Log($"Room {roomId} marked as cleared");
            }
        }

        public bool IsRoomCleared(int roomId)
        {
            if (roomId >= 0 && roomId < _generatedRooms.Count)
            {
                return _generatedRooms[roomId].IsCleared;
            }
            return false;
        }
        
        public List<int> GetRoomConnections(int roomId)
        {
            if (_roomConnections.ContainsKey(roomId))
            {
                return _roomConnections[roomId];
            }
            return new List<int>();
        }
        
        public Vector2 GetRoomPosition(int roomId)
        {
            if (roomId >= 0 && roomId < _generatedRooms.Count)
            {
                return _generatedRooms[roomId].Position;
            }
            return Vector2.zero;
        }
    }
}