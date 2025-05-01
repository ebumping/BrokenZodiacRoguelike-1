using System.Collections.Generic;
using UnityEngine;
using System;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    // This class manages procedural level generation and room information
    public class ProcGenManager : MonoBehaviour
    {        
        // Singleton instance
        public static ProcGenManager Instance { get; private set; }
        
        [Header("Generation Settings")]
        [SerializeField] private int seed = 0;
        [SerializeField] private bool randomizeSeed = true;
        [SerializeField] private int roomCount = 20;
        [SerializeField] private int minRoomCount = 15;
        [SerializeField] private int maxRoomCount = 30;
        [SerializeField] private float secretRoomChance = 0.3f;
        [SerializeField] private float treasureRoomChance = 0.2f;
        [SerializeField] private float shopRoomChance = 0.1f;
        [SerializeField] private bool generateBossRoom = true;
        [SerializeField] private bool generateExitRoom = true;
        
        [Header("Layout Settings")]
        [SerializeField] private float roomSpacing = 30f;
        [SerializeField] private int maxRoomSize = 2;
        [SerializeField] private float hallwayWidth = 4f;
        [SerializeField] private float wallThickness = 1f;
        [SerializeField] private Vector2Int startRoomPosition = Vector2Int.zero;
        
        [Header("Corruption Settings")]
        [SerializeField] private float baseCorruptionChance = 0.1f;
        [SerializeField] private float corruptionIncreasePerLevel = 0.05f;
        [SerializeField] private float maxCorruptionChance = 0.5f;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject normalRoomPrefab;
        [SerializeField] private GameObject bossRoomPrefab;
        [SerializeField] private GameObject secretRoomPrefab;
        [SerializeField] private GameObject shopRoomPrefab;
        [SerializeField] private GameObject treasureRoomPrefab;
        [SerializeField] private GameObject startRoomPrefab;
        [SerializeField] private GameObject exitRoomPrefab;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject doorPrefab;
        [SerializeField] private GameObject lockedDoorPrefab;
        [SerializeField] private GameObject secretDoorPrefab;
        
        // Runtime data
        private Dictionary<string, Room> _rooms = new Dictionary<string, Room>();
        private Dictionary<Vector2Int, string> _positionToRoomId = new Dictionary<Vector2Int, string>();
        private Dictionary<string, GameObject> _roomGameObjects = new Dictionary<string, GameObject>();
        private string _currentRoomId;
        private Room _currentRoom;
        private Room _startRoom;
        private Room _bossRoom;
        private Room _exitRoom;
        private int _currentLevel = 1;
        private float _corruptionLevel = 0f;
        private System.Random _random;
        
        // Events
        public event Action OnLevelGenerated;
        public event Action<Room> OnRoomDiscovered;
        public event Action<Room> OnRoomVisited;
        public event Action<string, string> OnRoomTransition;
        
        private void Awake()
        {
            // Setup singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Initialize random generator
            InitializeRandom();
        }
        
        private void Start()
        {
            // Generate the first level
            GenerateLevel();
        }
        
        // Initialize random number generator
        private void InitializeRandom()
        {
            if (randomizeSeed)
            {
                seed = UnityEngine.Random.Range(0, 999999);
            }
            
            _random = new System.Random(seed);
            
            // Set the corruption level based on current level
            _corruptionLevel = Mathf.Min(
                baseCorruptionChance + (corruptionIncreasePerLevel * (_currentLevel - 1)),
                maxCorruptionChance);
        }
        
        // Generate a complete level
        public void GenerateLevel()
        {
            // Clear any existing level
            ClearLevel();
            
            // Initialize random again (in case seed changed)
            InitializeRandom();
            
            // Determine actual room count with some randomness
            int actualRoomCount = roomCount;
            if (minRoomCount < maxRoomCount)
            {
                actualRoomCount = _random.Next(minRoomCount, maxRoomCount + 1);
            }
            
            // Generate room layout
            GenerateRoomLayout(actualRoomCount);
            
            // Instantiate rooms
            InstantiateRooms();
            
            // Populate rooms with content
            PopulateRooms();
            
            // Notify that level generation is complete
            OnLevelGenerated?.Invoke();
        }
        
        // Generate the layout of rooms
        private void GenerateRoomLayout(int targetRoomCount)
        {
            // Create start room
            _startRoom = CreateRoom("start", startRoomPosition, RoomType.Start);
            
            // Generate connected rooms using a simple algorithm
            List<Room> unprocessedRooms = new List<Room> { _startRoom };
            HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int> { _startRoom.GridPosition };
            
            int attempts = 0;
            int maxAttempts = targetRoomCount * 10; // Prevent infinite loops
            
            while (unprocessedRooms.Count > 0 && _rooms.Count < targetRoomCount && attempts < maxAttempts)
            {
                attempts++;
                
                // Pick a random unprocessed room
                int roomIndex = _random.Next(unprocessedRooms.Count);
                Room currentRoom = unprocessedRooms[roomIndex];
                
                // Try to add a new connected room
                if (TryAddConnectedRoom(currentRoom, occupiedPositions))
                {
                    // Successfully added a room, reset attempts
                    attempts = 0;
                }
                else
                {
                    // Failed to add a connected room, remove from unprocessed list
                    unprocessedRooms.RemoveAt(roomIndex);
                }
            }
            
            // Add special rooms
            AddSpecialRooms(occupiedPositions);
            
            // Update adjacency information for all rooms
            UpdateRoomAdjacency();
        }
        
        // Try to add a new room connected to the current room
        private bool TryAddConnectedRoom(Room sourceRoom, HashSet<Vector2Int> occupiedPositions)
        {
            // Define possible directions
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(1, 0),  // Right
                new Vector2Int(-1, 0), // Left
                new Vector2Int(0, 1),  // Up
                new Vector2Int(0, -1)  // Down
            };
            
            // Shuffle directions
            for (int i = 0; i < directions.Length; i++)
            {
                int j = _random.Next(i, directions.Length);
                Vector2Int temp = directions[i];
                directions[i] = directions[j];
                directions[j] = temp;
            }
            
            // Try each direction
            foreach (Vector2Int dir in directions)
            {
                Vector2Int newPos = sourceRoom.GridPosition + dir;
                
                // Check if position is already occupied
                if (!occupiedPositions.Contains(newPos))
                {
                    // Determine room type (mostly normal rooms)
                    RoomType roomType = RoomType.Normal;
                    
                    // Create a new room
                    string roomId = $"room_{_rooms.Count}";
                    Room newRoom = CreateRoom(roomId, newPos, roomType);
                    
                    // Add position to occupied set
                    occupiedPositions.Add(newPos);
                    
                    // Create connections between rooms
                    ConnectRooms(sourceRoom, newRoom, dir);
                    
                    // Add the new room to unprocessed list
                    return true;
                }
            }
            
            // Couldn't add a room in any direction
            return false;
        }
        
        // Add special rooms (boss, secret, treasure, etc.)
        private void AddSpecialRooms(HashSet<Vector2Int> occupiedPositions)
        {
            // Find suitable positions for special rooms
            List<Room> normalRooms = new List<Room>();
            foreach (var room in _rooms.Values)
            {
                if (room.Type == RoomType.Normal)
                {
                    normalRooms.Add(room);
                }
            }
            
            // Shuffle the list for randomness
            for (int i = 0; i < normalRooms.Count; i++)
            {
                int j = _random.Next(i, normalRooms.Count);
                Room temp = normalRooms[i];
                normalRooms[i] = normalRooms[j];
                normalRooms[j] = temp;
            }
            
            // Add boss room (far from start)
            if (generateBossRoom && normalRooms.Count > 0)
            {
                // Find room farthest from start
                Room farthestRoom = FindFarthestRoom(normalRooms, _startRoom.GridPosition);
                if (farthestRoom != null)
                {
                    ConvertRoomType(farthestRoom, RoomType.Boss);
                    _bossRoom = farthestRoom;
                    
                    // Remove from normal rooms list
                    normalRooms.Remove(farthestRoom);
                }
            }
            
            // Add exit room (near boss room)
            if (generateExitRoom && _bossRoom != null && normalRooms.Count > 0)
            {
                // Find room near boss room
                Room nearestRoom = FindNearestRoom(normalRooms, _bossRoom.GridPosition);
                if (nearestRoom != null)
                {
                    ConvertRoomType(nearestRoom, RoomType.Exit);
                    _exitRoom = nearestRoom;
                    
                    // Remove from normal rooms list
                    normalRooms.Remove(nearestRoom);
                }
            }
            
            // Add secret rooms
            int secretRoomsToAdd = Mathf.FloorToInt(normalRooms.Count * secretRoomChance);
            for (int i = 0; i < secretRoomsToAdd && i < normalRooms.Count; i++)
            {
                ConvertRoomType(normalRooms[i], RoomType.Secret);
            }
            
            // Remove converted rooms
            normalRooms.RemoveRange(0, Mathf.Min(secretRoomsToAdd, normalRooms.Count));
            
            // Add treasure rooms
            int treasureRoomsToAdd = Mathf.FloorToInt(normalRooms.Count * treasureRoomChance);
            for (int i = 0; i < treasureRoomsToAdd && i < normalRooms.Count; i++)
            {
                ConvertRoomType(normalRooms[i], RoomType.Treasure);
            }
            
            // Remove converted rooms
            normalRooms.RemoveRange(0, Mathf.Min(treasureRoomsToAdd, normalRooms.Count));
            
            // Add shop rooms
            int shopRoomsToAdd = Mathf.FloorToInt(normalRooms.Count * shopRoomChance);
            for (int i = 0; i < shopRoomsToAdd && i < normalRooms.Count; i++)
            {
                ConvertRoomType(normalRooms[i], RoomType.Shop);
            }
            
            // Apply corruption to some rooms
            ApplyCorruption();
        }
        
        // Apply corruption to some rooms
        private void ApplyCorruption()
        {
            foreach (var room in _rooms.Values)
            {
                // Don't corrupt special rooms
                if (room.Type == RoomType.Normal && _random.NextDouble() < _corruptionLevel)
                {
                    room.IsCorrupted = true;
                }
            }
        }
        
        // Update adjacency information
        private void UpdateRoomAdjacency()
        {
            foreach (var room in _rooms.Values)
            {
                // Check if any adjacent room is visited
                bool adjacentToVisited = false;
                
                foreach (var connection in room.Connections)
                {
                    if (_rooms.TryGetValue(connection.TargetRoomId, out Room targetRoom) && targetRoom.IsVisited)
                    {
                        adjacentToVisited = true;
                        break;
                    }
                }
                
                room.IsAdjacentToVisited = adjacentToVisited;
            }
        }
        
        // Find the room farthest from a given position
        private Room FindFarthestRoom(List<Room> rooms, Vector2Int fromPosition)
        {
            Room farthestRoom = null;
            float maxDistance = 0f;
            
            foreach (var room in rooms)
            {
                float distance = Vector2Int.Distance(fromPosition, room.GridPosition);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestRoom = room;
                }
            }
            
            return farthestRoom;
        }
        
        // Find the room nearest to a given position
        private Room FindNearestRoom(List<Room> rooms, Vector2Int fromPosition)
        {
            Room nearestRoom = null;
            float minDistance = float.MaxValue;
            
            foreach (var room in rooms)
            {
                float distance = Vector2Int.Distance(fromPosition, room.GridPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestRoom = room;
                }
            }
            
            return nearestRoom;
        }
        
        // Connect two rooms bidirectionally
        private void ConnectRooms(Room roomA, Room roomB, Vector2Int direction)
        {
            Vector2 dirVector = new Vector2(direction.x, direction.y);
            roomA.AddConnection(roomB.RoomId, dirVector);
            
            // Add the opposite connection
            Vector2 oppositeDir = new Vector2(-direction.x, -direction.y);
            roomB.AddConnection(roomA.RoomId, oppositeDir);
        }
        
        // Create a room with the given parameters
        private Room CreateRoom(string roomId, Vector2Int position, RoomType type = RoomType.Normal)
        {
            Room room = new Room(roomId, position, type);
            _rooms[roomId] = room;
            _positionToRoomId[position] = roomId;
            return room;
        }
        
        // Convert an existing room to a different type
        private void ConvertRoomType(Room room, RoomType newType)
        {
            room.Type = newType;
            
            // Set appropriate flags based on type
            if (newType == RoomType.Secret)
            {
                room.IsDiscovered = false;
                
                // Make connections to secret rooms locked and hidden
                foreach (var targetRoomId in _rooms.Keys)
                {
                    Room targetRoom = _rooms[targetRoomId];
                    RoomConnection connection = targetRoom.GetConnection(room.RoomId);
                    if (connection != null)
                    {
                        connection.IsLocked = true;
                        connection.IsDiscovered = false;
                    }
                }
            }
            else if (newType == RoomType.Boss)
            {
                // Make connections to boss rooms locked
                foreach (var targetRoomId in _rooms.Keys)
                {
                    Room targetRoom = _rooms[targetRoomId];
                    RoomConnection connection = targetRoom.GetConnection(room.RoomId);
                    if (connection != null)
                    {
                        connection.IsLocked = true;
                    }
                }
            }
        }
        
        // Instantiate all rooms in the scene
        private void InstantiateRooms()
        {
            // For Unity implementation, would instantiate room prefabs here
            // For this template, we'll just log the process
            Debug.Log($"Instantiating {_rooms.Count} rooms");
            
            foreach (var room in _rooms.Values)
            {
                // Calculate world position
                Vector3 position = new Vector3(
                    room.GridPosition.x * roomSpacing,
                    0,
                    room.GridPosition.y * roomSpacing
                );
                
                // Select prefab based on room type
                GameObject prefab = normalRoomPrefab; // Default
                
                switch (room.Type)
                {
                    case RoomType.Start:
                        prefab = startRoomPrefab != null ? startRoomPrefab : normalRoomPrefab;
                        break;
                    case RoomType.Boss:
                        prefab = bossRoomPrefab != null ? bossRoomPrefab : normalRoomPrefab;
                        break;
                    case RoomType.Secret:
                        prefab = secretRoomPrefab != null ? secretRoomPrefab : normalRoomPrefab;
                        break;
                    case RoomType.Shop:
                        prefab = shopRoomPrefab != null ? shopRoomPrefab : normalRoomPrefab;
                        break;
                    case RoomType.Treasure:
                        prefab = treasureRoomPrefab != null ? treasureRoomPrefab : normalRoomPrefab;
                        break;
                    case RoomType.Exit:
                        prefab = exitRoomPrefab != null ? exitRoomPrefab : normalRoomPrefab;
                        break;
                }
                
                // Instantiate room prefab (commented out for template)
                /*
                GameObject roomObj = Instantiate(prefab, position, Quaternion.identity);
                roomObj.name = $"Room_{room.RoomId}";
                
                // Store reference
                _roomGameObjects[room.RoomId] = roomObj;
                
                // Set up doors
                InstantiateDoorsForRoom(room, roomObj);
                */
                
                // For the template, just log
                Debug.Log($"Instantiated {room.Type} room at position {position}");
            }
            
            // Set the current room to the start room
            SetCurrentRoom(_startRoom.RoomId);
        }
        
        // Populate rooms with content
        private void PopulateRooms()
        {
            // For Unity implementation, would populate with enemies, items, etc.
            // For this template, we'll just log the process
            foreach (var room in _rooms.Values)
            {
                // Add enemies based on room type and difficulty
                int enemyCount = 0;
                
                switch (room.Type)
                {
                    case RoomType.Normal:
                        enemyCount = _random.Next(2, 5);
                        break;
                    case RoomType.Boss:
                        enemyCount = 1; // One boss
                        break;
                    case RoomType.Secret:
                        enemyCount = _random.Next(0, 2);
                        break;
                    case RoomType.Treasure:
                        enemyCount = _random.Next(0, 3);
                        break;
                    // Other room types might not have enemies
                }
                
                // Scale by difficulty and corruption
                enemyCount = Mathf.CeilToInt(enemyCount * room.Difficulty);
                if (room.IsCorrupted)
                {
                    enemyCount += _random.Next(1, 3);
                }
                
                // Add enemies (in a real implementation)
                for (int i = 0; i < enemyCount; i++)
                {
                    // Would spawn enemies here
                    string enemyId = $"enemy_{room.RoomId}_{i}";
                    room.AddEnemy(enemyId);
                }
                
                // Add items based on room type
                int itemCount = 0;
                
                switch (room.Type)
                {
                    case RoomType.Normal:
                        itemCount = _random.Next(0, 2);
                        break;
                    case RoomType.Boss:
                        itemCount = _random.Next(1, 3);
                        break;
                    case RoomType.Secret:
                        itemCount = _random.Next(1, 3);
                        break;
                    case RoomType.Treasure:
                        itemCount = _random.Next(2, 5);
                        break;
                    case RoomType.Shop:
                        itemCount = _random.Next(3, 6);
                        break;
                }
                
                // Add items (in a real implementation)
                for (int i = 0; i < itemCount; i++)
                {
                    // Would spawn items here
                    string itemId = $"item_{room.RoomId}_{i}";
                    room.AddItem(itemId);
                }
                
                Debug.Log($"Populated {room.Type} room with {enemyCount} enemies and {itemCount} items");
            }
        }
        
        // Clear the current level
        private void ClearLevel()
        {
            // Destroy all room game objects
            foreach (var gameObj in _roomGameObjects.Values)
            {
                if (gameObj != null)
                {
                    Destroy(gameObj);
                }
            }
            
            // Clear collections
            _rooms.Clear();
            _positionToRoomId.Clear();
            _roomGameObjects.Clear();
            _currentRoomId = null;
            _currentRoom = null;
            _startRoom = null;
            _bossRoom = null;
            _exitRoom = null;
        }
        
        // Set the current room
        public void SetCurrentRoom(string roomId)
        {
            // If trying to set the same room, do nothing
            if (_currentRoomId == roomId) return;
            
            string previousRoomId = _currentRoomId;
            
            // Update the old current room
            if (_currentRoom != null)
            {
                _currentRoom.SetAsCurrent(false);
            }
            
            // Set new current room
            if (_rooms.TryGetValue(roomId, out Room room))
            {
                _currentRoomId = roomId;
                _currentRoom = room;
                _currentRoom.SetAsCurrent(true);
                
                // Trigger events
                OnRoomVisited?.Invoke(_currentRoom);
                
                if (previousRoomId != null)
                {
                    OnRoomTransition?.Invoke(previousRoomId, roomId);
                }
                
                // Update adjacency information
                UpdateRoomAdjacency();
            }
        }
        
        // Discover a room without visiting it
        public void DiscoverRoom(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out Room room) && !room.IsDiscovered)
            {
                room.Discover();
                OnRoomDiscovered?.Invoke(room);
            }
        }
        
        // Get a room by ID
        public Room GetRoomById(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out Room room))
            {
                return room;
            }
            return null;
        }
        
        // Get a room at a grid position
        public Room GetRoomAtGridPosition(Vector2Int position)
        {
            if (_positionToRoomId.TryGetValue(position, out string roomId))
            {
                return GetRoomById(roomId);
            }
            return null;
        }
        
        // Get a room at a world position
        public Room GetRoomAtPosition(Vector3 worldPosition)
        {
            // Convert world position to grid position
            Vector2Int gridPos = new Vector2Int(
                Mathf.RoundToInt(worldPosition.x / roomSpacing),
                Mathf.RoundToInt(worldPosition.z / roomSpacing)
            );
            
            return GetRoomAtGridPosition(gridPos);
        }
        
        // Get all rooms
        public List<Room> GetAllRooms()
        {
            List<Room> roomList = new List<Room>();
            foreach (var room in _rooms.Values)
            {
                roomList.Add(room);
            }
            return roomList;
        }
        
        // Get all discovered rooms
        public List<Room> GetDiscoveredRooms()
        {
            List<Room> discoveredRooms = new List<Room>();
            foreach (var room in _rooms.Values)
            {
                if (room.IsDiscovered)
                {
                    discoveredRooms.Add(room);
                }
            }
            return discoveredRooms;
        }
        
        // Move to the next level
        public void MoveToNextLevel()
        {
            _currentLevel++;
            GenerateLevel();
        }
        
        // Public properties
        public int CurrentLevel => _currentLevel;
        public float CorruptionLevel => _corruptionLevel;
        public Room CurrentRoom => _currentRoom;
        public Room StartRoom => _startRoom;
        public Room BossRoom => _bossRoom;
        public Room ExitRoom => _exitRoom;
    }
}