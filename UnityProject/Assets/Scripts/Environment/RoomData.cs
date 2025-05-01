using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Data definition for a room in the procedural generation system
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public string roomId;
        public string roomName;
        public RoomType roomType;
        public Vector2Int size = new Vector2Int(10, 10);
        public GameObject roomPrefab;
        public List<DoorLocation> doors = new List<DoorLocation>();
        public List<EnemySpawnData> enemySpawns = new List<EnemySpawnData>();
        public List<ItemSpawnData> itemSpawns = new List<ItemSpawnData>();
        public List<EnvironmentHazard> hazards = new List<EnvironmentHazard>();
        public ZodiacInfluence zodiacInfluence = ZodiacInfluence.None;
        public float sanityEffect = 0f;
        public float minDistanceFromStart = 0;
        public float maxDistanceFromStart = float.MaxValue;
        
        // Special room features
        public bool hasSecretWalls = false;
        public bool hasTraps = false;
        public bool hasSanityEffects = false;
        public bool isRequired = false;
        
        // Visual and audio settings
        public AudioClip ambientSound;
        public Color roomLighting = Color.white;
        
        // Tag for special room types
        public string specialRoomTag = "";
        
        /// <summary>
        /// Check if this room can connect to another room at a specific position
        /// </summary>
        public bool CanConnectTo(RoomData otherRoom, Direction direction)
        {
            // Get the door in this room that faces the given direction
            DoorLocation thisDoor = doors.Find(d => d.direction == direction);
            
            // Get the door in the other room that faces the opposite direction
            Direction oppositeDirection = GetOppositeDirection(direction);
            DoorLocation otherDoor = otherRoom.doors.Find(d => d.direction == oppositeDirection);
            
            // Both rooms must have doors facing each other
            return thisDoor != null && otherDoor != null;
        }
        
        /// <summary>
        /// Get the opposite direction
        /// </summary>
        private Direction GetOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East: return Direction.West;
                case Direction.West: return Direction.East;
                default: return Direction.None;
            }
        }
    }
    
    /// <summary>
    /// Types of rooms in the game
    /// </summary>
    public enum RoomType
    {
        Standard,       // Basic room with enemies and items
        Entrance,       // Starting room
        Exit,           // Level exit
        Boss,           // Boss room
        Shop,           // Merchant room
        Treasure,       // Room with valuable items
        Puzzle,         // Room with puzzle mechanics
        Sanctuary,      // Room that restores health/sanity
        Library,        // Room with lore and knowledge
        RelicChamber,   // Room with powerful artifacts
        Ritual,         // Room for performing rituals
        Secret,         // Hidden room
        Hallucination   // Room that only appears at certain sanity levels
    }
    
    /// <summary>
    /// Cardinal directions for door placement
    /// </summary>
    public enum Direction
    {
        None,
        North,
        South,
        East,
        West
    }
    
    /// <summary>
    /// Represents a door location on a room
    /// </summary>
    [System.Serializable]
    public class DoorLocation
    {
        public Direction direction;
        public Vector2Int position; // Relative to room origin
        public DoorType doorType = DoorType.Normal;
        public bool isLocked = false;
        public string keyId = ""; // ID of key needed to unlock
        
        public DoorLocation(Direction dir, Vector2Int pos)
        {
            direction = dir;
            position = pos;
        }
    }
    
    /// <summary>
    /// Types of doors
    /// </summary>
    public enum DoorType
    {
        Normal,     // Standard door
        Hidden,     // Secret door that needs to be discovered
        Locked,     // Door that requires a key
        Sealed,     // Door that requires a special action to open
        Portal      // Teleportation door
    }
    
    /// <summary>
    /// Data for enemy spawns in a room
    /// </summary>
    [System.Serializable]
    public class EnemySpawnData
    {
        public string enemyId;
        public Vector2 position; // Relative to room origin
        public float spawnChance = 1.0f;
        public int minCount = 1;
        public int maxCount = 1;
    }
    
    /// <summary>
    /// Data for item spawns in a room
    /// </summary>
    [System.Serializable]
    public class ItemSpawnData
    {
        public string itemId;
        public Vector2 position; // Relative to room origin
        public float spawnChance = 1.0f;
    }
    
    /// <summary>
    /// Represents a hazard in the environment
    /// </summary>
    [System.Serializable]
    public class EnvironmentHazard
    {
        public string hazardId;
        public Vector2 position; // Relative to room origin
        public float damage;
        public DamageType damageType;
        public float radius;
        public bool isPermanent = true;
    }
    
    /// <summary>
    /// Types of zodiac influences on a room
    /// </summary>
    public enum ZodiacInfluence
    {
        None,
        Aries,
        Taurus,
        Gemini,
        Cancer,
        Leo,
        Virgo,
        Libra,
        Scorpio,
        Sagittarius,
        Capricorn,
        Aquarius,
        Pisces,
        Cosmic // Influenced by all signs
    }
}