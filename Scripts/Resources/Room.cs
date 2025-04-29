using Godot;
using System;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources
{
    [GlobalClass]
    public partial class Room : Resource
    {
        public enum RoomCategory { Standard, Special, Boss, Entrance, Exit }
        public enum EnvironmentTheme { Crypt, Library, Cathedral, Laboratory, Abyss }
        
        [Export] public string RoomName { get; set; }
        [Export] public string Description { get; set; }
        [Export] public RoomCategory Category { get; set; } = RoomCategory.Standard;
        [Export] public EnvironmentTheme Theme { get; set; } = EnvironmentTheme.Crypt;
        
        // Room properties
        [Export] public Vector2I Size { get; set; } = new Vector2I(10, 10); // Room dimensions in tiles
        [Export] public int MinLevel { get; set; } = 1; // Minimum level to appear
        [Export] public float Weight { get; set; } = 1.0f; // Chance to appear in generation
        
        // Connections to other rooms
        [Export] public bool HasNorthDoor { get; set; } = true;
        [Export] public bool HasSouthDoor { get; set; } = true;
        [Export] public bool HasEastDoor { get; set; } = true;
        [Export] public bool HasWestDoor { get; set; } = true;
        
        // Room content details
        [Export] public int MinEnemies { get; set; } = 0;
        [Export] public int MaxEnemies { get; set; } = 5;
        [Export] public string[] PossibleEnemies { get; set; } = Array.Empty<string>();
        
        [Export] public int MinLootItems { get; set; } = 0;
        [Export] public int MaxLootItems { get; set; } = 2;
        [Export] public float TreasureChest { get; set; } = 0.25f; // Chance to spawn a chest
        [Export] public float ShrineChance { get; set; } = 0.1f; // Chance to spawn a shrine
        [Export] public bool GuaranteedTarotCard { get; set; } = false;
        
        // Visual elements
        [Export] public string TilesetPath { get; set; } = "";
        [Export] public string BackgroundPath { get; set; } = "";
        [Export] public string AmbientSound { get; set; } = "";
        [Export] public Color AmbientLight { get; set; } = new Color(0.5f, 0.5f, 0.5f);
        [Export] public float FogDensity { get; set; } = 0.0f;
        
        // Metadata for procedural generation
        [Export] public bool IsSpecialRoom { get; set; } = false;
        [Export] public bool RequiresClearance { get; set; } = true; // Whether enemies must be cleared to exit
        [Export] public string[] RequiredItems { get; set; } = Array.Empty<string>(); // Items needed to enter
        
        // Get available door directions
        public string[] GetAvailableDoors()
        {
            List<string> doors = new List<string>();
            
            if (HasNorthDoor) doors.Add("North");
            if (HasSouthDoor) doors.Add("South");
            if (HasEastDoor) doors.Add("East");
            if (HasWestDoor) doors.Add("West");
            
            return doors.ToArray();
        }
        
        // Check if room connects in a given direction
        public bool HasDoorInDirection(string direction)
        {
            switch (direction.ToLower())
            {
                case "north": return HasNorthDoor;
                case "south": return HasSouthDoor;
                case "east": return HasEastDoor;
                case "west": return HasWestDoor;
                default: return false;
            }
        }
        
        // Get opposite direction
        public static string GetOppositeDirection(string direction)
        {
            switch (direction.ToLower())
            {
                case "north": return "South";
                case "south": return "North";
                case "east": return "West";
                case "west": return "East";
                default: return string.Empty;
            }
        }
        
        // Serialization methods for JSON export/import
        public Dictionary<string, Variant> Serialize()
        {
            var data = new Dictionary<string, Variant>();
            data["roomName"] = RoomName;
            data["description"] = Description;
            data["category"] = (int)Category;
            data["theme"] = (int)Theme;
            
            data["size"] = Size;
            data["minLevel"] = MinLevel;
            data["weight"] = Weight;
            
            data["hasNorthDoor"] = HasNorthDoor;
            data["hasSouthDoor"] = HasSouthDoor;
            data["hasEastDoor"] = HasEastDoor;
            data["hasWestDoor"] = HasWestDoor;
            
            data["minEnemies"] = MinEnemies;
            data["maxEnemies"] = MaxEnemies;
            data["possibleEnemies"] = new Godot.Collections.Array<string>(PossibleEnemies);
            
            data["minLootItems"] = MinLootItems;
            data["maxLootItems"] = MaxLootItems;
            data["treasureChest"] = TreasureChest;
            data["shrineChance"] = ShrineChance;
            data["guaranteedTarotCard"] = GuaranteedTarotCard;
            
            data["tilesetPath"] = TilesetPath;
            data["backgroundPath"] = BackgroundPath;
            data["ambientSound"] = AmbientSound;
            data["ambientLight"] = AmbientLight;
            data["fogDensity"] = FogDensity;
            
            data["isSpecialRoom"] = IsSpecialRoom;
            data["requiresClearance"] = RequiresClearance;
            data["requiredItems"] = new Godot.Collections.Array<string>(RequiredItems);
            
            return data;
        }
        
        public void Deserialize(Dictionary<string, Variant> data)
        {
            if (data.TryGetValue("roomName", out var roomName))
                RoomName = roomName.AsString();
                
            if (data.TryGetValue("description", out var description))
                Description = description.AsString();
                
            if (data.TryGetValue("category", out var category))
                Category = (RoomCategory)category.AsInt32();
                
            if (data.TryGetValue("theme", out var theme))
                Theme = (EnvironmentTheme)theme.AsInt32();
                
            if (data.TryGetValue("size", out var size))
                Size = size.AsVector2I();
                
            if (data.TryGetValue("minLevel", out var minLevel))
                MinLevel = minLevel.AsInt32();
                
            if (data.TryGetValue("weight", out var weight))
                Weight = (float)weight.AsDouble();
                
            if (data.TryGetValue("hasNorthDoor", out var hasNorthDoor))
                HasNorthDoor = hasNorthDoor.AsBool();
                
            if (data.TryGetValue("hasSouthDoor", out var hasSouthDoor))
                HasSouthDoor = hasSouthDoor.AsBool();
                
            if (data.TryGetValue("hasEastDoor", out var hasEastDoor))
                HasEastDoor = hasEastDoor.AsBool();
                
            if (data.TryGetValue("hasWestDoor", out var hasWestDoor))
                HasWestDoor = hasWestDoor.AsBool();
                
            if (data.TryGetValue("minEnemies", out var minEnemies))
                MinEnemies = minEnemies.AsInt32();
                
            if (data.TryGetValue("maxEnemies", out var maxEnemies))
                MaxEnemies = maxEnemies.AsInt32();
                
            if (data.TryGetValue("possibleEnemies", out var possibleEnemies))
            {
                var enemiesArray = possibleEnemies.AsStringArray();
                PossibleEnemies = new string[enemiesArray.Length];
                for (int i = 0; i < enemiesArray.Length; i++)
                {
                    PossibleEnemies[i] = enemiesArray[i];
                }
            }
            
            if (data.TryGetValue("minLootItems", out var minLootItems))
                MinLootItems = minLootItems.AsInt32();
                
            if (data.TryGetValue("maxLootItems", out var maxLootItems))
                MaxLootItems = maxLootItems.AsInt32();
                
            if (data.TryGetValue("treasureChest", out var treasureChest))
                TreasureChest = (float)treasureChest.AsDouble();
                
            if (data.TryGetValue("shrineChance", out var shrineChance))
                ShrineChance = (float)shrineChance.AsDouble();
                
            if (data.TryGetValue("guaranteedTarotCard", out var guaranteedTarotCard))
                GuaranteedTarotCard = guaranteedTarotCard.AsBool();
                
            if (data.TryGetValue("tilesetPath", out var tilesetPath))
                TilesetPath = tilesetPath.AsString();
                
            if (data.TryGetValue("backgroundPath", out var backgroundPath))
                BackgroundPath = backgroundPath.AsString();
                
            if (data.TryGetValue("ambientSound", out var ambientSound))
                AmbientSound = ambientSound.AsString();
                
            if (data.TryGetValue("ambientLight", out var ambientLight))
                AmbientLight = ambientLight.AsColor();
                
            if (data.TryGetValue("fogDensity", out var fogDensity))
                FogDensity = (float)fogDensity.AsDouble();
                
            if (data.TryGetValue("isSpecialRoom", out var isSpecialRoom))
                IsSpecialRoom = isSpecialRoom.AsBool();
                
            if (data.TryGetValue("requiresClearance", out var requiresClearance))
                RequiresClearance = requiresClearance.AsBool();
                
            if (data.TryGetValue("requiredItems", out var requiredItems))
            {
                var itemsArray = requiredItems.AsStringArray();
                RequiredItems = new string[itemsArray.Length];
                for (int i = 0; i < itemsArray.Length; i++)
                {
                    RequiredItems[i] = itemsArray[i];
                }
            }
        }
    }
}
