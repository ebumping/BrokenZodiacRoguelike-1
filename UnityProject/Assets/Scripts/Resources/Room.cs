using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
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

    public enum LootType
    {
        Motes,
        Weapon,
        TarotCard,
        Mutagen,
        HealthPotion
    }

    [System.Serializable]
    public class RoomData
    {
        public int Id;
        public string TemplateName;
        public RoomType Type;
        public float DifficultyMultiplier;
        public bool IsCleared;
        public Vector2 Position;
        public List<EnemySpawnData> Enemies;
        public List<LootData> Loot;
    }

    [System.Serializable]
    public class EnemySpawnData
    {
        public string Type;
        public Vector2 Position;
        public float Health;
        public float Damage;
    }

    [System.Serializable]
    public class LootData
    {
        public LootType Type;
        public int Value;
        public Vector2 Position;
    }
    
    public class RoomController : MonoBehaviour
    {
        [SerializeField] private Transform enemySpawnParent;
        [SerializeField] private Transform lootSpawnParent;

        private RoomData _roomData;
        private bool _isPlayerInRoom = false;
        private List<GameObject> _spawnedEnemies = new List<GameObject>();
        private List<GameObject> _spawnedLoot = new List<GameObject>();

        public void SetupRoom(RoomData roomData)
        {
            _roomData = roomData;
            
            // Add doors based on connections
            SetupDoors();
            
            // Spawn enemies if not cleared
            if (!_roomData.IsCleared)
            {
                SpawnEnemies();
            }
            
            // Spawn loot
            SpawnLoot();
        }

        private void SetupDoors()
        {
            // This would connect to the ProcGenManager to determine which doors to enable
            var connections = Core.ProcGenManager.Instance.GetRoomConnections(_roomData.Id);
            
            // Find door game objects and activate them based on connections
            // Implementation would depend on your room prefab structure
        }

        private void SpawnEnemies()
        {
            if (_roomData.Enemies == null || enemySpawnParent == null) return;
            
            foreach (var enemyData in _roomData.Enemies)
            {
                // Get the prefab based on enemy type
                GameObject enemyPrefab = GetEnemyPrefab(enemyData.Type);
                if (enemyPrefab == null) continue;
                
                // Instantiate at the specified position
                GameObject enemy = Instantiate(enemyPrefab, enemySpawnParent);
                enemy.transform.localPosition = enemyData.Position;
                
                // Setup enemy properties
                EnemyController controller = enemy.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.SetupEnemy(enemyData.Health, enemyData.Damage);
                }
                
                _spawnedEnemies.Add(enemy);
            }
        }

        private GameObject GetEnemyPrefab(string enemyType)
        {
            // In a real implementation, this would use a prefab dictionary or load from Resources
            return null;
        }

        private void SpawnLoot()
        {
            if (_roomData.Loot == null || lootSpawnParent == null) return;
            
            foreach (var lootData in _roomData.Loot)
            {
                // Get the prefab based on loot type
                GameObject lootPrefab = GetLootPrefab(lootData.Type);
                if (lootPrefab == null) continue;
                
                // Instantiate at the specified position
                GameObject loot = Instantiate(lootPrefab, lootSpawnParent);
                loot.transform.localPosition = lootData.Position;
                
                // Setup loot properties
                LootItem lootItem = loot.GetComponent<LootItem>();
                if (lootItem != null)
                {
                    lootItem.SetupLoot(lootData.Type, lootData.Value);
                }
                
                _spawnedLoot.Add(loot);
            }
        }

        private GameObject GetLootPrefab(LootType lootType)
        {
            // In a real implementation, this would use a prefab dictionary or load from Resources
            return null;
        }

        public void PlayerEntered()
        {
            _isPlayerInRoom = true;
            
            // Activate enemies if not cleared
            if (!_roomData.IsCleared && _spawnedEnemies.Count > 0)
            {
                foreach (var enemy in _spawnedEnemies)
                {
                    if (enemy != null)
                    {
                        EnemyController controller = enemy.GetComponent<EnemyController>();
                        if (controller != null)
                        {
                            controller.Activate();
                        }
                    }
                }
            }
        }

        public void PlayerExited()
        {
            _isPlayerInRoom = false;
            
            // Deactivate enemies
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null)
                {
                    EnemyController controller = enemy.GetComponent<EnemyController>();
                    if (controller != null)
                    {
                        controller.Deactivate();
                    }
                }
            }
        }

        public void CheckRoomClear()
        {
            // Check if all enemies are defeated
            bool allDefeated = true;
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null && enemy.activeInHierarchy)
                {
                    allDefeated = false;
                    break;
                }
            }
            
            if (allDefeated && !_roomData.IsCleared)
            {
                _roomData.IsCleared = true;
                Core.ProcGenManager.Instance.MarkRoomAsCleared(_roomData.Id);
                OnRoomCleared();
            }
        }

        private void OnRoomCleared()
        {
            Debug.Log($"Room {_roomData.Id} cleared!");
            
            // Open doors, play effects, etc.
            
            // Notify player for special rewards
            foreach (var player in FindObjectsOfType<Core.PlayerController>())
            {
                player.OnRoomCleared(_roomData.Id);
            }
        }
    }
    
    public class LootItem : MonoBehaviour
    {
        private LootType _lootType;
        private int _value;
        
        public void SetupLoot(LootType type, int value)
        {
            _lootType = type;
            _value = value;
            
            // Configure appearance based on type
            UpdateAppearance();
        }
        
        private void UpdateAppearance()
        {
            // Implementation would depend on your visual setup
            // This might update sprites, colors, etc.
        }
        
        public void Collect(GameObject collector)
        {
            // Give the loot to the collector (usually a player)
            Core.PlayerController player = collector.GetComponent<Core.PlayerController>();
            if (player != null)
            {
                switch (_lootType)
                {
                    case LootType.Motes:
                        Core.GameManager.Instance.CollectMotes(_value);
                        break;
                        
                    case LootType.Weapon:
                        player.PickupWeapon(_value);
                        break;
                        
                    case LootType.TarotCard:
                        player.PickupTarotCard(_value);
                        break;
                        
                    case LootType.Mutagen:
                        player.PickupMutagen(_value);
                        break;
                        
                    case LootType.HealthPotion:
                        player.RestoreHealth(_value);
                        break;
                }
                
                // Destroy the loot object
                Destroy(gameObject);
            }
        }
    }
}