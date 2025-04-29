using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    public class ResourceManager : MonoBehaviour
    {
        // Singleton instance
        public static ResourceManager Instance { get; private set; }
        
        // Player classes
        [Header("Classes")]
        [SerializeField] private List<PlayerClass> playerClasses = new List<PlayerClass>();
        
        // Weapons
        [Header("Weapons")]
        [SerializeField] private List<Weapon> weapons = new List<Weapon>();
        
        // Tarot cards
        [Header("Tarot Cards")]
        [SerializeField] private List<TarotCard> tarotCards = new List<TarotCard>();
        
        // Room prefabs
        [Header("Rooms")]
        [SerializeField] private GameObject startingRoomPrefab;
        [SerializeField] private GameObject bossRoomPrefab;
        [SerializeField] private List<GameObject> standardRoomPrefabs = new List<GameObject>();
        [SerializeField] private List<GameObject> specialRoomPrefabs = new List<GameObject>();
        
        // Enemy prefabs
        [Header("Enemies")]
        [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();
        
        // Projectile prefabs
        [Header("Projectiles")]
        [SerializeField] private List<GameObject> projectilePrefabs = new List<GameObject>();
        
        // Effect prefabs
        [Header("Effects")]
        [SerializeField] private List<GameObject> effectPrefabs = new List<GameObject>();
        
        // Dictionaries for quick lookups
        private Dictionary<string, PlayerClass> _classesDict = new Dictionary<string, PlayerClass>();
        private Dictionary<string, Weapon> _weaponsDict = new Dictionary<string, Weapon>();
        private Dictionary<string, TarotCard> _tarotCardsDict = new Dictionary<string, TarotCard>();
        private Dictionary<string, GameObject> _roomPrefabsDict = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> _enemyPrefabsDict = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> _projectilePrefabsDict = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> _effectPrefabsDict = new Dictionary<string, GameObject>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDictionaries();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeDictionaries()
        {
            // Initialize player classes dictionary
            foreach (var playerClass in playerClasses)
            {
                if (playerClass != null && !string.IsNullOrEmpty(playerClass.ClassName))
                {
                    _classesDict[playerClass.ClassName] = playerClass;
                }
            }
            
            // Initialize weapons dictionary
            foreach (var weapon in weapons)
            {
                if (weapon != null && !string.IsNullOrEmpty(weapon.WeaponName))
                {
                    _weaponsDict[weapon.WeaponName] = weapon;
                }
            }
            
            // Initialize tarot cards dictionary
            foreach (var card in tarotCards)
            {
                if (card != null && !string.IsNullOrEmpty(card.CardName))
                {
                    _tarotCardsDict[card.CardName] = card;
                }
            }
            
            // Initialize room prefabs dictionary
            if (startingRoomPrefab != null)
            {
                _roomPrefabsDict["StartingRoom"] = startingRoomPrefab;
            }
            
            if (bossRoomPrefab != null)
            {
                _roomPrefabsDict["BossRoom"] = bossRoomPrefab;
            }
            
            // Add standard room prefabs
            for (int i = 0; i < standardRoomPrefabs.Count; i++)
            {
                string roomName = i < 4 ? new string[] {"RoomTemplate", "SmallRoom", "MediumRoom", "LargeRoom"}[i] : $"StandardRoom{i}";
                _roomPrefabsDict[roomName] = standardRoomPrefabs[i];
            }
            
            // Add special room prefabs
            for (int i = 0; i < specialRoomPrefabs.Count; i++)
            {
                string roomName = i < 3 ? new string[] {"ShrineRoom", "TarotRoom", "TreasureRoom"}[i] : $"SpecialRoom{i}";
                _roomPrefabsDict[roomName] = specialRoomPrefabs[i];
            }
            
            // Initialize enemy prefabs dictionary
            foreach (var enemy in enemyPrefabs)
            {
                if (enemy != null)
                {
                    string enemyName = enemy.name.Replace("Prefab", "");
                    _enemyPrefabsDict[enemyName] = enemy;
                }
            }
            
            // Initialize projectile prefabs dictionary
            foreach (var projectile in projectilePrefabs)
            {
                if (projectile != null)
                {
                    string projectileName = projectile.name.Replace("Prefab", "");
                    _projectilePrefabsDict[projectileName] = projectile;
                }
            }
            
            // Initialize effect prefabs dictionary
            foreach (var effect in effectPrefabs)
            {
                if (effect != null)
                {
                    string effectName = effect.name.Replace("Prefab", "");
                    _effectPrefabsDict[effectName] = effect;
                }
            }
            
            Debug.Log("Resource Manager initialized");
        }
        
        // Methods to get resources
        public PlayerClass GetPlayerClass(string className)
        {
            if (_classesDict.TryGetValue(className, out var playerClass))
            {
                return playerClass;
            }
            
            Debug.LogWarning($"Player class not found: {className}");
            return null;
        }
        
        public Weapon GetWeapon(string weaponName)
        {
            if (_weaponsDict.TryGetValue(weaponName, out var weapon))
            {
                return weapon;
            }
            
            Debug.LogWarning($"Weapon not found: {weaponName}");
            return null;
        }
        
        public TarotCard GetTarotCard(string cardName)
        {
            if (_tarotCardsDict.TryGetValue(cardName, out var card))
            {
                return card;
            }
            
            Debug.LogWarning($"Tarot card not found: {cardName}");
            return null;
        }
        
        public GameObject GetRoomPrefab(string roomName)
        {
            if (_roomPrefabsDict.TryGetValue(roomName, out var roomPrefab))
            {
                return roomPrefab;
            }
            
            Debug.LogWarning($"Room prefab not found: {roomName}");
            return null;
        }
        
        public GameObject GetEnemyPrefab(string enemyName)
        {
            if (_enemyPrefabsDict.TryGetValue(enemyName, out var enemyPrefab))
            {
                return enemyPrefab;
            }
            
            Debug.LogWarning($"Enemy prefab not found: {enemyName}");
            return null;
        }
        
        public GameObject GetProjectilePrefab(string projectileName)
        {
            if (_projectilePrefabsDict.TryGetValue(projectileName, out var projectilePrefab))
            {
                return projectilePrefab;
            }
            
            Debug.LogWarning($"Projectile prefab not found: {projectileName}");
            return null;
        }
        
        public GameObject GetEffectPrefab(string effectName)
        {
            if (_effectPrefabsDict.TryGetValue(effectName, out var effectPrefab))
            {
                return effectPrefab;
            }
            
            Debug.LogWarning($"Effect prefab not found: {effectName}");
            return null;
        }
        
        // Load card from resources folder
        public TarotCard LoadTarotCard(string resourcePath)
        {
            TarotCard card = Resources.Load<TarotCard>(resourcePath);
            if (card != null)
            {
                if (!_tarotCardsDict.ContainsKey(card.CardName))
                {
                    _tarotCardsDict[card.CardName] = card;
                }
                return card;
            }
            
            Debug.LogWarning($"Failed to load tarot card from path: {resourcePath}");
            return null;
        }
        
        // Load weapon from resources folder
        public Weapon LoadWeapon(string resourcePath)
        {
            Weapon weapon = Resources.Load<Weapon>(resourcePath);
            if (weapon != null)
            {
                if (!_weaponsDict.ContainsKey(weapon.WeaponName))
                {
                    _weaponsDict[weapon.WeaponName] = weapon;
                }
                return weapon;
            }
            
            Debug.LogWarning($"Failed to load weapon from path: {resourcePath}");
            return null;
        }
    }
}