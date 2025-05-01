using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
    // This class represents a procedurally generated room in the game
    [System.Serializable]
    public class Room
    {        
        // Room identification
        public string RoomId { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        
        // Room properties
        public RoomType Type { get; set; } = RoomType.Normal;
        public int Size { get; set; } = 1;
        public float Difficulty { get; set; } = 1.0f;
        
        // Room state
        public bool IsDiscovered { get; set; } = false;
        public bool IsVisited { get; set; } = false;
        public bool IsCurrentRoom { get; set; } = false;
        public bool IsCleared { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool IsCorrupted { get; set; } = false;
        
        // Connections to other rooms
        public List<RoomConnection> Connections { get; private set; } = new List<RoomConnection>();
        
        // Room contents
        public List<string> EnemyIds { get; private set; } = new List<string>();
        public List<string> ItemIds { get; private set; } = new List<string>();
        public List<string> TrapIds { get; private set; } = new List<string>();
        
        // Cached properties
        private bool _isBossRoom;
        private bool _isSecretRoom;
        private bool _isStartRoom;
        private bool _isExitRoom;
        
        // Constructor
        public Room(string id, Vector2Int position, RoomType type = RoomType.Normal)
        {
            RoomId = id;
            GridPosition = position;
            Type = type;
            
            // Set room type-specific properties
            switch (type)
            {
                case RoomType.Boss:
                    _isBossRoom = true;
                    Difficulty = 2.0f;
                    Size = 2;
                    break;
                case RoomType.Secret:
                    _isSecretRoom = true;
                    IsDiscovered = false;
                    break;
                case RoomType.Start:
                    _isStartRoom = true;
                    IsDiscovered = true;
                    IsVisited = true;
                    break;
                case RoomType.Exit:
                    _isExitRoom = true;
                    break;
                case RoomType.Shop:
                    Difficulty = 0.5f;
                    break;
                case RoomType.Treasure:
                    Difficulty = 1.5f;
                    break;
            }
        }
        
        // Add a connection to another room
        public void AddConnection(string targetRoomId, Vector2 direction, bool isDiscovered = false, bool isLocked = false)
        {
            // Check if connection already exists
            foreach (var conn in Connections)
            {
                if (conn.TargetRoomId == targetRoomId)
                {
                    return; // Already connected
                }
            }
            
            // Create new connection
            RoomConnection connection = new RoomConnection
            {
                TargetRoomId = targetRoomId,
                Direction = direction,
                IsDiscovered = isDiscovered,
                IsLocked = isLocked
            };
            
            Connections.Add(connection);
        }
        
        // Remove a connection
        public void RemoveConnection(string targetRoomId)
        {
            Connections.RemoveAll(c => c.TargetRoomId == targetRoomId);
        }
        
        // Get a connection by target room ID
        public RoomConnection GetConnection(string targetRoomId)
        {
            return Connections.Find(c => c.TargetRoomId == targetRoomId);
        }
        
        // Discover this room
        public void Discover()
        {
            IsDiscovered = true;
        }
        
        // Visit this room
        public void Visit()
        {
            if (!IsVisited)
            {
                IsVisited = true;
                // Discover all connections but not the connected rooms themselves
                foreach (var connection in Connections)
                {
                    connection.IsDiscovered = true;
                }
            }
        }
        
        // Set this room as the current room
        public void SetAsCurrent(bool isCurrent)
        {
            IsCurrentRoom = isCurrent;
            if (isCurrent)
            {
                Visit(); // Automatically visit when setting as current
            }
        }
        
        // Add an enemy to the room
        public void AddEnemy(string enemyId)
        {
            if (!EnemyIds.Contains(enemyId))
            {
                EnemyIds.Add(enemyId);
            }
        }
        
        // Remove an enemy from the room
        public void RemoveEnemy(string enemyId)
        {
            EnemyIds.Remove(enemyId);
            
            // Check if room is cleared
            if (EnemyIds.Count == 0)
            {
                IsCleared = true;
            }
        }
        
        // Add an item to the room
        public void AddItem(string itemId)
        {
            if (!ItemIds.Contains(itemId))
            {
                ItemIds.Add(itemId);
            }
        }
        
        // Remove an item from the room
        public void RemoveItem(string itemId)
        {
            ItemIds.Remove(itemId);
        }
        
        // Check if this room is adjacent to a visited room
        public bool IsAdjacentToVisited { get; set; } = false;
        
        // Check if this is a boss room
        public bool IsBossRoom => _isBossRoom || Type == RoomType.Boss;
        
        // Check if this is a secret room
        public bool IsSecretRoom => _isSecretRoom || Type == RoomType.Secret;
        
        // Check if this is the starting room
        public bool IsStartRoom => _isStartRoom || Type == RoomType.Start;
        
        // Check if this is the exit room
        public bool IsExitRoom => _isExitRoom || Type == RoomType.Exit;
    }
    
    // Room connection class
    [System.Serializable]
    public class RoomConnection
    {
        public string TargetRoomId { get; set; }
        public Vector2 Direction { get; set; }
        public bool IsDiscovered { get; set; } = false;
        public bool IsLocked { get; set; } = false;
    }
    
    // Room types
    public enum RoomType
    {
        Normal,
        Boss,
        Secret,
        Start,
        Exit,
        Shop,
        Treasure,
        Lore
    }
}