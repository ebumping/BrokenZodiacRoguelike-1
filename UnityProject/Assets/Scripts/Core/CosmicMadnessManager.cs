using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CodexOfTheBrokenZodiac.Resources;
using System;

namespace CodexOfTheBrokenZodiac.Core
{
    // This class manages the Cosmic Madness skill tree and ability activation
    public class CosmicMadnessManager : MonoBehaviour
    {
        // Singleton instance
        public static CosmicMadnessManager Instance { get; private set; }
        
        [Header("Skill Tree Configuration")]
        [SerializeField] private CosmicMadnessAbility[] availableAbilities;
        [SerializeField] private int startingSkillPoints = 0;
        [SerializeField] private int maxSkillPoints = 10;
        [SerializeField] private bool autoActivateMadnessAbilities = true;
        [SerializeField] private float checkInterval = 1f; // How often to check for activations
        
        [Header("Progression")]
        [SerializeField] private bool earnPointsFromLowSanity = true;
        [SerializeField] private float pointsPerLowSanityMinute = 0.1f; // Points earned per minute at low sanity
        [SerializeField] private float lowSanityThreshold = 0.4f; // Below this is considered "low sanity"
        [SerializeField] private bool earnPointsFromBosses = true;
        [SerializeField] private float pointsPerBossKill = 1f;
        
        [Header("Effects")]
        [SerializeField] private GameObject skillUnlockVFXPrefab;
        [SerializeField] private AudioClip skillUnlockSFX;
        
        // Runtime data structure for players
        private Dictionary<int, PlayerMadnessData> _playerData = new Dictionary<int, PlayerMadnessData>();
        
        // Track unlocked abilities for each player
        private Dictionary<int, HashSet<CosmicMadnessAbility>> _unlockedAbilities = 
            new Dictionary<int, HashSet<CosmicMadnessAbility>>();
        
        // Track active abilities for each player
        private Dictionary<int, HashSet<CosmicMadnessAbility>> _activeAbilities = 
            new Dictionary<int, HashSet<CosmicMadnessAbility>>();
        
        // Events
        public event Action<int, CosmicMadnessAbility> OnAbilityUnlocked;
        public event Action<int, CosmicMadnessAbility> OnAbilityActivated;
        public event Action<int, CosmicMadnessAbility> OnAbilityDeactivated;
        public event Action<int, float, float> OnMadnessPointsChanged; // playerId, current, max
        
        private void Awake()
        {
            // Setup singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            // Subscribe to game events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerSpawned += RegisterPlayer;
                GameManager.Instance.OnPlayerDespawned += UnregisterPlayer;
                GameManager.Instance.OnEnemyKilled += OnEnemyKilled;
            }
            
            // Subscribe to sanity events
            if (SanitySystem.Instance != null)
            {
                SanitySystem.Instance.OnPlayerSanityChanged += OnSanityChanged;
            }
            
            // Start ability check coroutine
            if (autoActivateMadnessAbilities)
            {
                StartCoroutine(CheckMadnessAbilities());
            }
            
            Debug.Log("Cosmic Madness Manager initialized");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerSpawned -= RegisterPlayer;
                GameManager.Instance.OnPlayerDespawned -= UnregisterPlayer;
                GameManager.Instance.OnEnemyKilled -= OnEnemyKilled;
            }
            
            if (SanitySystem.Instance != null)
            {
                SanitySystem.Instance.OnPlayerSanityChanged -= OnSanityChanged;
            }
            
            // Clean up
            StopAllCoroutines();
        }
        
        #region Player Registration
        
        // Register a player with the madness system
        public void RegisterPlayer(PlayerController player)
        {
            if (player == null) return;
            
            int playerId = player.NetworkId;
            
            // Skip if already registered
            if (_playerData.ContainsKey(playerId))
                return;
            
            // Create data structure
            _playerData[playerId] = new PlayerMadnessData
            {
                MadnessPoints = startingSkillPoints,
                MaxMadnessPoints = maxSkillPoints,
                LowSanityStartTime = -1f,
                TimeSinceLastPointGain = 0f
            };
            
            // Create collections for abilities
            if (!_unlockedAbilities.ContainsKey(playerId))
                _unlockedAbilities[playerId] = new HashSet<CosmicMadnessAbility>();
                
            if (!_activeAbilities.ContainsKey(playerId))
                _activeAbilities[playerId] = new HashSet<CosmicMadnessAbility>();
            
            // Notify
            OnMadnessPointsChanged?.Invoke(playerId, startingSkillPoints, maxSkillPoints);
            Debug.Log($"Player {playerId} registered with Cosmic Madness Manager");
        }
        
        // Unregister a player
        public void UnregisterPlayer(PlayerController player)
        {
            if (player == null) return;
            
            int playerId = player.NetworkId;
            
            // Skip if not registered
            if (!_playerData.ContainsKey(playerId))
                return;
            
            // Deactivate all active abilities
            if (_activeAbilities.ContainsKey(playerId))
            {
                foreach (var ability in _activeAbilities[playerId])
                {
                    if (ability != null)
                        ability.Deactivate(player);
                }
                
                _activeAbilities[playerId].Clear();
            }
            
            // Remove player data
            _playerData.Remove(playerId);
            _unlockedAbilities.Remove(playerId);
            _activeAbilities.Remove(playerId);
            
            Debug.Log($"Player {playerId} unregistered from Cosmic Madness Manager");
        }
        
        #endregion
        
        #region Skill Tree Management
        
        // Get all available abilities for a player
        public List<CosmicMadnessAbility> GetAvailableAbilities(int playerId)
        {
            List<CosmicMadnessAbility> result = new List<CosmicMadnessAbility>();
            PlayerController player = GameManager.Instance?.GetPlayerByNetworkId(playerId);
            
            if (player == null) return result;
            
            foreach (var ability in availableAbilities)
            {
                if (ability != null && ability.IsAvailableTo(player))
                {
                    result.Add(ability);
                }
            }
            
            return result;
        }
        
        // Get all unlocked abilities for a player
        public List<CosmicMadnessAbility> GetUnlockedAbilities(int playerId)
        {
            List<CosmicMadnessAbility> result = new List<CosmicMadnessAbility>();
            
            if (_unlockedAbilities.ContainsKey(playerId))
            {
                result.AddRange(_unlockedAbilities[playerId]);
            }
            
            return result;
        }
        
        // Get all active abilities for a player
        public List<CosmicMadnessAbility> GetActiveAbilities(int playerId)
        {
            List<CosmicMadnessAbility> result = new List<CosmicMadnessAbility>();
            
            if (_activeAbilities.ContainsKey(playerId))
            {
                result.AddRange(_activeAbilities[playerId]);
            }
            
            return result;
        }
        
        // Check if an ability is unlocked for a player
        public bool IsAbilityUnlocked(int playerId, CosmicMadnessAbility ability)
        {
            if (ability == null) return false;
            
            return _unlockedAbilities.ContainsKey(playerId) && 
                   _unlockedAbilities[playerId].Contains(ability);
        }
        
        // General version to check if an ability is unlocked by any player
        public bool IsAbilityUnlocked(CosmicMadnessAbility ability)
        {
            if (ability == null) return false;
            
            foreach (var playerAbilities in _unlockedAbilities.Values)
            {
                if (playerAbilities.Contains(ability))
                    return true;
            }
            
            return false;
        }
        
        // Check if an ability is active for a player
        public bool IsAbilityActive(int playerId, CosmicMadnessAbility ability)
        {
            if (ability == null) return false;
            
            return _activeAbilities.ContainsKey(playerId) && 
                   _activeAbilities[playerId].Contains(ability);
        }
        
        // Get available skill points for a player
        public float GetAvailableMadnessPoints(int playerId)
        {
            if (_playerData.ContainsKey(playerId))
                return _playerData[playerId].MadnessPoints;
                
            return 0f;
        }
        
        // Get max skill points for a player
        public float GetMaxMadnessPoints(int playerId)
        {
            if (_playerData.ContainsKey(playerId))
                return _playerData[playerId].MaxMadnessPoints;
                
            return maxSkillPoints;
        }
        
        // Attempt to unlock an ability for a player
        public bool UnlockAbility(int playerId, CosmicMadnessAbility ability)
        {
            if (ability == null) return false;
            
            PlayerController player = GameManager.Instance?.GetPlayerByNetworkId(playerId);
            if (player == null) return false;
            
            // Check if already unlocked
            if (IsAbilityUnlocked(playerId, ability))
                return false;
                
            // Check if available to this player
            if (!ability.IsAvailableTo(player))
                return false;
                
            // Check if player meets sanity state requirement
            SanityState playerState = SanitySystem.Instance?.GetPlayerSanityState(playerId) ?? SanityState.Normal;
            if ((int)playerState < (int)ability.MinimumSanityState)
                return false;
                
            // Check prerequisites
            if (!ability.ArePrerequisitesMet(this))
                return false;
                
            // Check if player has enough points
            if (_playerData.ContainsKey(playerId))
            {
                var data = _playerData[playerId];
                
                if (data.MadnessPoints >= ability.RequiredPoints)
                {
                    // Deduct points
                    data.MadnessPoints -= ability.RequiredPoints;
                    OnMadnessPointsChanged?.Invoke(playerId, data.MadnessPoints, data.MaxMadnessPoints);
                    
                    // Unlock ability
                    _unlockedAbilities[playerId].Add(ability);
                    ability.Unlock();
                    
                    // Notify
                    OnAbilityUnlocked?.Invoke(playerId, ability);
                    
                    // Play effects
                    if (skillUnlockVFXPrefab != null)
                    {
                        GameObject vfx = Instantiate(skillUnlockVFXPrefab, player.transform.position, Quaternion.identity);
                        vfx.transform.SetParent(player.transform);
                        Destroy(vfx, 3f);
                    }
                    
                    if (skillUnlockSFX != null)
                    {
                        AudioSource.PlayClipAtPoint(skillUnlockSFX, player.transform.position);
                    }
                    
                    Debug.Log($"Player {playerId} unlocked cosmic ability '{ability.AbilityName}'!");
                    return true;
                }
            }
            
            return false;
        }
        
        // Add cosmic madness points to a player
        public void AddMadnessPoints(int playerId, float points)
        {
            if (!_playerData.ContainsKey(playerId))
                return;
                
            var data = _playerData[playerId];
            float oldPoints = data.MadnessPoints;
            
            // Add points (clamped to max)
            data.MadnessPoints = Mathf.Clamp(data.MadnessPoints + points, 0, data.MaxMadnessPoints);
            data.TimeSinceLastPointGain = 0f;
            
            // Notify if changed
            if (data.MadnessPoints != oldPoints)
            {
                OnMadnessPointsChanged?.Invoke(playerId, data.MadnessPoints, data.MaxMadnessPoints);
                Debug.Log($"Player {playerId} gained {points:F1} cosmic madness points. Total: {data.MadnessPoints:F1}");
            }
        }
        
        // Set the max madness points for a player
        public void SetMaxMadnessPoints(int playerId, float maxPoints)
        {
            if (!_playerData.ContainsKey(playerId))
                return;
                
            var data = _playerData[playerId];
            float oldMax = data.MaxMadnessPoints;
            
            data.MaxMadnessPoints = Mathf.Max(1, maxPoints);
            data.MadnessPoints = Mathf.Clamp(data.MadnessPoints, 0, data.MaxMadnessPoints);
            
            // Notify if changed
            if (data.MaxMadnessPoints != oldMax)
            {
                OnMadnessPointsChanged?.Invoke(playerId, data.MadnessPoints, data.MaxMadnessPoints);
            }
        }
        
        #endregion
        
        #region Ability Activation
        
        // Manually activate an ability for a player
        public bool ActivateAbility(int playerId, CosmicMadnessAbility ability)
        {
            if (ability == null) return false;
            
            PlayerController player = GameManager.Instance?.GetPlayerByNetworkId(playerId);
            if (player == null) return false;
            
            // Check if unlocked
            if (!IsAbilityUnlocked(playerId, ability))
                return false;
                
            // Attempt to activate
            if (ability.Activate(player))
            {
                // Add to active set
                _activeAbilities[playerId].Add(ability);
                
                // Notify
                OnAbilityActivated?.Invoke(playerId, ability);
                
                return true;
            }
            
            return false;
        }
        
        // Manually deactivate an ability for a player
        public bool DeactivateAbility(int playerId, CosmicMadnessAbility ability)
        {
            if (ability == null) return false;
            
            PlayerController player = GameManager.Instance?.GetPlayerByNetworkId(playerId);
            if (player == null) return false;
            
            // Check if active
            if (!IsAbilityActive(playerId, ability))
                return false;
                
            // Deactivate
            ability.Deactivate(player);
            
            // Remove from active set
            _activeAbilities[playerId].Remove(ability);
            
            // Notify
            OnAbilityDeactivated?.Invoke(playerId, ability);
            
            return true;
        }
        
        // Automatic check for ability activation based on sanity
        private IEnumerator CheckMadnessAbilities()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);
                
                // Check all players
                foreach (int playerId in _playerData.Keys)
                {
                    // Get player and sanity
                    PlayerController player = GameManager.Instance?.GetPlayerByNetworkId(playerId);
                    if (player == null || player.IsDead) continue;
                    
                    float sanity = SanitySystem.Instance?.GetPlayerSanityPercentage(playerId) ?? 1f;
                    
                    // Check all unlocked abilities
                    if (_unlockedAbilities.ContainsKey(playerId))
                    {
                        foreach (var ability in _unlockedAbilities[playerId])
                        {
                            // Skip already active abilities
                            if (IsAbilityActive(playerId, ability))
                                continue;
                                
                            // Check if sanity is below threshold
                            if (sanity <= ability.SanityActivationThreshold && ability.CanActivate)
                            {
                                ActivateAbility(playerId, ability);
                            }
                        }
                    }
                    
                    // Check for abilities that should deactivate
                    List<CosmicMadnessAbility> toDeactivate = new List<CosmicMadnessAbility>();
                    
                    if (_activeAbilities.ContainsKey(playerId))
                    {
                        foreach (var ability in _activeAbilities[playerId])
                        {
                            // Skip permanent abilities
                            if (ability.IsPermanentOnUnlock)
                                continue;
                                
                            // Check if sanity is above threshold
                            if (sanity > ability.SanityActivationThreshold)
                            {
                                toDeactivate.Add(ability);
                            }
                        }
                    }
                    
                    // Deactivate abilities
                    foreach (var ability in toDeactivate)
                    {
                        DeactivateAbility(playerId, ability);
                    }
                }
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        // Handle sanity changes
        private void OnSanityChanged(int playerId, float currentSanity, float maxSanity)
        {
            if (!_playerData.ContainsKey(playerId))
                return;
                
            var data = _playerData[playerId];
            float sanityPercentage = currentSanity / maxSanity;
            
            // Check for low sanity point gain
            if (earnPointsFromLowSanity && sanityPercentage <= lowSanityThreshold)
            {
                // Start tracking time if we just entered low sanity
                if (data.LowSanityStartTime < 0)
                {
                    data.LowSanityStartTime = Time.time;
                }
                
                // Calculate time spent at low sanity
                float timeAtLowSanity = Time.time - data.LowSanityStartTime;
                
                // Add points based on time
                float minutesAtLowSanity = timeAtLowSanity / 60f;
                data.TimeSinceLastPointGain += Time.deltaTime;
                
                // Add points gradually (once per minute)
                if (data.TimeSinceLastPointGain >= 60f)
                {
                    AddMadnessPoints(playerId, pointsPerLowSanityMinute);
                }
            }
            else
            {
                // Reset low sanity time if sanity improved
                data.LowSanityStartTime = -1f;
            }
        }
        
        // Handle enemy killed event
        private void OnEnemyKilled(EnemyController enemy, GameObject killer)
        {
            if (!earnPointsFromBosses || enemy == null || killer == null)
                return;
                
            // Only award points for boss kills
            if (!enemy.IsBoss)
                return;
                
            // Get player who killed the boss
            PlayerController player = killer.GetComponent<PlayerController>();
            if (player != null)
            {
                int playerId = player.NetworkId;
                AddMadnessPoints(playerId, pointsPerBossKill);
            }
        }
        
        #endregion
    }
    
    // Data structure for tracking player madness progression
    public class PlayerMadnessData
    {
        public float MadnessPoints;
        public float MaxMadnessPoints;
        public float LowSanityStartTime;
        public float TimeSinceLastPointGain;
    }
}