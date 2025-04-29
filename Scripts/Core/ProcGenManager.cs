using Godot;
using System;
using System.Collections.Generic;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    public partial class ProcGenManager : Node
    {
        // Singleton instance
        public static ProcGenManager Instance { get; private set; }
        
        [Signal]
        public delegate void LevelGeneratedEventHandler(int level);
        
        [Signal]
        public delegate void RoomEnteredEventHandler(int roomId);
        
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
        private Node2D _currentRoomInstance = null;

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
            GD.Print("ProcGenManager initialized");
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
            _totalRooms = baseRooms + (int)GD.Randi() % (variance * 2 + 1) - variance;
            
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
                string template = _standardRoomTemplates[(int)GD.Randi() % _standardRoomTemplates.Count];
                RoomData room = CreateRoom(_generatedRooms.Count, template, RoomType.Standard);
                _generatedRooms.Add(room);
            }
            
            // Generate special rooms
            for (int i = 0; i < specialRoomCount; i++)
            {
                string template = _specialRoomTemplates[(int)GD.Randi() % _specialRoomTemplates.Count];
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
            EmitSignal(SignalName.LevelGenerated, level);
            GD.Print($"Generated level {level} with {_totalRooms} rooms.");
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
            float xOffset = (float)GD.RandfRange(-0.3, 0.3);
            float yOffset = (float)GD.RandfRange(-0.3, 0.3);
            
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
            int actualEnemyCount = baseEnemyCount + (int)GD.Randi() % 3 - 1;
            actualEnemyCount = Mathf.Max(1, actualEnemyCount);
            
            // TODO: When we have actual enemy resources, load different types
            // For now, just add placeholder enemy data
            room.Enemies = new List<EnemySpawnData>();
            
            for (int i = 0; i < actualEnemyCount; i++)
            {
                EnemySpawnData enemy = new EnemySpawnData
                {
                    Type = "BasicEnemy",
                    Position = new Vector2((float)GD.RandfRange(-5, 5), (float)GD.RandfRange(-5, 5)),
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
                    Position = Vector2.Zero,
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
                Position = new Vector2((float)GD.RandfRange(-3, 3), (float)GD.RandfRange(-3, 3))
            };
            room.Loot.Add(motes);
            
            // Add weapon if luck favors it
            if (GD.Randf() < lootChance)
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
                    float roll = (float)GD.Randf();
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
                    Position = new Vector2((float)GD.RandfRange(-4, 4), (float)GD.RandfRange(-4, 4))
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
                int from = (int)GD.Randi() % (_generatedRooms.Count - 1);
                
                // Try to find a valid target that's not already connected
                int attempts = 0;
                while (attempts < 5) // Limit attempts to avoid infinite loops
                {
                    int to = (int)GD.Randi() % _generatedRooms.Count;
                    
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
                GD.Print($"Room {roomId} connects to: {connections}");
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
                GD.PrintErr($"Invalid room ID: {roomId}");
                return;
            }
            
            // Unload current room if any
            if (_currentRoomInstance != null)
            {
                _currentRoomInstance.QueueFree();
                _currentRoomInstance = null;
            }
            
            RoomData roomData = _generatedRooms[roomId];
            _currentRoomId = roomId;
            
            // Load room template
            PackedScene roomTemplate = ResourceManager.Instance.GetRoomTemplate(roomData.TemplateName);
            if (roomTemplate == null)
            {
                GD.PrintErr($"Room template not found: {roomData.TemplateName}");
                
                // Fall back to default template
                roomTemplate = ResourceManager.Instance.GetRoomTemplate("RoomTemplate");
                if (roomTemplate == null)
                {
                    GD.PrintErr("Cannot load fallback room template!");
                    return;
                }
            }
            
            // Instance the room
            _currentRoomInstance = roomTemplate.Instantiate<Node2D>();
            GetTree().CurrentScene.AddChild(_currentRoomInstance);
            
            // Configure room
            _currentRoomInstance.Name = $"Room_{roomId}";
            
            // If room has a setup method, call it with room data
            if (_currentRoomInstance.HasMethod("SetupRoom"))
            {
                _currentRoomInstance.Call("SetupRoom", roomData);
            }
            
            // Signal that we've entered a new room
            EmitSignal(SignalName.RoomEntered, roomId);
            GD.Print($"Loaded room {roomId} of type {roomData.Type}");
        }

        public void MarkRoomAsCleared(int roomId)
        {
            if (roomId >= 0 && roomId < _generatedRooms.Count)
            {
                _generatedRooms[roomId].IsCleared = true;
                
                // If this was a boss room and we're on a boss level, advance to next level
                if (_generatedRooms[roomId].Type == RoomType.Boss && _isBossLevel)
                {
                    GD.Print("Boss defeated! Advancing to next level.");
                    GameManager.Instance.AdvanceToNextLevel();
                }
            }
        }

        public List<int> GetConnectedRooms(int roomId)
        {
            if (_roomConnections.ContainsKey(roomId))
            {
                return _roomConnections[roomId];
            }
            
            return new List<int>();
        }

        public RoomData GetCurrentRoomData()
        {
            if (_currentRoomId >= 0 && _currentRoomId < _generatedRooms.Count)
            {
                return _generatedRooms[_currentRoomId];
            }
            
            return null;
        }

        public RoomData GetRoomData(int roomId)
        {
            if (roomId >= 0 && roomId < _generatedRooms.Count)
            {
                return _generatedRooms[roomId];
            }
            
            return null;
        }
    }

    public class RoomData
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public RoomType Type { get; set; }
        public float DifficultyMultiplier { get; set; }
        public bool IsCleared { get; set; }
        public Vector2 Position { get; set; }
        public List<EnemySpawnData> Enemies { get; set; }
        public List<LootData> Loot { get; set; }
    }

    public enum RoomType
    {
        Starting,
        Standard,
        Special,
        Shrine,
        Tarot,
        Treasure,
        Boss
    }

    public class EnemySpawnData
    {
        public string Type { get; set; }
        public Vector2 Position { get; set; }
        public float Health { get; set; }
        public float Damage { get; set; }
    }

    public class LootData
    {
        public LootType Type { get; set; }
        public int Value { get; set; }
        public Vector2 Position { get; set; }
    }

    public enum LootType
    {
        Motes,
        Weapon,
        TarotCard,
        Mutagen,
        ArmorPiece
    }
}
