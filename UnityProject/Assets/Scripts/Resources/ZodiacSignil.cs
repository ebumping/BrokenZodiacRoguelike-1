using UnityEngine;
using System;
using System.Collections.Generic;

namespace CosmicHorror
{
    /// <summary>
    /// Manages player zodiac sign and associated abilities
    /// </summary>
    public class ZodiacSignil : MonoBehaviour
    {
        [Header("Zodiac Assignment")]
        [SerializeField] private bool randomZodiacOnStart = true;
        [SerializeField] private ZodiacSign defaultZodiacSign = ZodiacSign.Aries;
        
        [Header("Zodiac Ability Parameters")]
        // Aries
        [SerializeField] private float ariesSprintSpeedBonus = 0.15f;
        [SerializeField] private float ariesFirstHitDamageBonus = 0.4f;
        
        // Taurus
        [SerializeField] private float taurusMaxHpBonus = 0.2f;
        [SerializeField] private float taurusKnockbackReduction = 0.5f;
        
        // Gemini
        [SerializeField] private float geminiDoppelDuration = 4f;
        
        // Cancer
        [SerializeField] private float cancerShellProtection = 30f;
        
        // Leo
        [SerializeField] private float leoCritBonus = 0.25f;
        
        // Virgo
        [SerializeField] private float virgoHealingBonus = 0.5f;
        [SerializeField] private float virgoDropPenalty = 0.2f;
        
        // Libra
        [SerializeField] private float libraEqualizationInterval = 5f;
        
        // Scorpio
        [SerializeField] private float scorpioToxinDamagePercent = 0.05f;
        
        // Sagittarius
        [SerializeField] private float sagittariusProjectileSpeedBonus = 0.1f;
        [SerializeField] private float sagittariusRollCooldownReduction = 0.2f;
        
        // Capricorn
        // No parameters needed (climbing costs zero stamina)
        
        // Aquarius
        [SerializeField] private float aquariusSlowPuddleDuration = 3f;
        [SerializeField] private float aquariusSlowPuddleSlowPercent = 0.3f;
        
        // Pisces
        [SerializeField] private float piscesLowHealthThreshold = 0.3f;
        [SerializeField] private int piscesChainLightningTargets = 2;
        
        [Header("Visual Elements")]
        [SerializeField] private Sprite[] zodiacIcons;
        
        // Current zodiac state
        private ZodiacSign currentZodiacSign;
        private Dictionary<ZodiacAbilityType, ZodiacAbility> activeAbilities = new Dictionary<ZodiacAbilityType, ZodiacAbility>();
        
        // Timers and state tracking
        private float libraEqualizationTimer = 0f;
        private bool hasActiveCancerShell = false;
        private bool isInLight = false;
        private bool isLowHealth = false;
        
        // Events
        public event Action<ZodiacSign> OnZodiacSignChanged;
        public event Action<ZodiacAbilityType> OnZodiacAbilityTriggered;
        
        // References
        private PlayerController playerController;
        private PlayerStats playerStats;
        private WeaponSystem weaponSystem;
        private HealthSystem healthSystem;
        
        // Properties
        public ZodiacSign CurrentZodiacSign => currentZodiacSign;
        public Sprite CurrentZodiacIcon => (zodiacIcons != null && zodiacIcons.Length > 0) ? 
            zodiacIcons[(int)currentZodiacSign] : null;
        
        private void Awake()
        {
            // Get references
            playerController = GetComponent<PlayerController>();
            playerStats = GetComponent<PlayerStats>();
            weaponSystem = GetComponent<WeaponSystem>();
            healthSystem = GetComponent<HealthSystem>();
            
            // Initialize zodiac sign
            if (randomZodiacOnStart)
            {
                AssignRandomZodiacSign();
            }
            else
            {
                SetZodiacSign(defaultZodiacSign);
            }
        }
        
        private void Start()
        {
            // Initialize abilities based on zodiac sign
            InitializeZodiacAbilities();
            
            // Apply passive bonuses
            ApplyPassiveZodiacBonuses();
            
            // Subscribe to events
            SubscribeToEvents();
        }
        
        private void Update()
        {
            // Update zodiac abilities
            UpdateZodiacAbilities();
        }
        
        /// <summary>
        /// Assign a random zodiac sign to the player
        /// </summary>
        public void AssignRandomZodiacSign()
        {
            // Get random zodiac sign
            ZodiacSign randomSign = (ZodiacSign)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ZodiacSign)).Length);
            SetZodiacSign(randomSign);
        }
        
        /// <summary>
        /// Set the player's zodiac sign
        /// </summary>
        public void SetZodiacSign(ZodiacSign sign)
        {
            // Clear existing abilities
            ClearZodiacAbilities();
            
            // Set new sign
            currentZodiacSign = sign;
            
            // Initialize abilities for the new sign
            InitializeZodiacAbilities();
            
            // Apply passive bonuses
            ApplyPassiveZodiacBonuses();
            
            // Fire event
            OnZodiacSignChanged?.Invoke(currentZodiacSign);
            
            Debug.Log($"Zodiac sign set to {currentZodiacSign}");
        }
        
        /// <summary>
        /// Initialize abilities based on current zodiac sign
        /// </summary>
        private void InitializeZodiacAbilities()
        {
            // Clear existing abilities
            activeAbilities.Clear();
            
            // Add abilities based on zodiac sign
            switch (currentZodiacSign)
            {
                case ZodiacSign.Aries:
                    // Sprint speed and first-hit damage bonus
                    activeAbilities.Add(ZodiacAbilityType.AriesSprintBoost, new ZodiacAbility(ZodiacAbilityType.AriesSprintBoost, true));
                    activeAbilities.Add(ZodiacAbilityType.AriesFirstHitBonus, new ZodiacAbility(ZodiacAbilityType.AriesFirstHitBonus, true));
                    break;
                    
                case ZodiacSign.Taurus:
                    // Max HP and knockback reduction
                    activeAbilities.Add(ZodiacAbilityType.TaurusHealthBonus, new ZodiacAbility(ZodiacAbilityType.TaurusHealthBonus, true));
                    activeAbilities.Add(ZodiacAbilityType.TaurusKnockbackReduction, new ZodiacAbility(ZodiacAbilityType.TaurusKnockbackReduction, true));
                    break;
                    
                case ZodiacSign.Gemini:
                    // Doppelganger on reload
                    activeAbilities.Add(ZodiacAbilityType.GeminiDoppelganger, new ZodiacAbility(ZodiacAbilityType.GeminiDoppelganger, false));
                    break;
                    
                case ZodiacSign.Cancer:
                    // Shell protection every room
                    activeAbilities.Add(ZodiacAbilityType.CancerShell, new ZodiacAbility(ZodiacAbilityType.CancerShell, false));
                    break;
                    
                case ZodiacSign.Leo:
                    // Crit bonus in light
                    activeAbilities.Add(ZodiacAbilityType.LeoCritBonus, new ZodiacAbility(ZodiacAbilityType.LeoCritBonus, false));
                    break;
                    
                case ZodiacSign.Virgo:
                    // Healing bonus and drop penalty
                    activeAbilities.Add(ZodiacAbilityType.VirgoHealingBonus, new ZodiacAbility(ZodiacAbilityType.VirgoHealingBonus, true));
                    activeAbilities.Add(ZodiacAbilityType.VirgoDropPenalty, new ZodiacAbility(ZodiacAbilityType.VirgoDropPenalty, true));
                    break;
                    
                case ZodiacSign.Libra:
                    // Damage/healing equalization
                    activeAbilities.Add(ZodiacAbilityType.LibraEqualization, new ZodiacAbility(ZodiacAbilityType.LibraEqualization, false));
                    break;
                    
                case ZodiacSign.Scorpio:
                    // Toxin application
                    activeAbilities.Add(ZodiacAbilityType.ScorpioToxin, new ZodiacAbility(ZodiacAbilityType.ScorpioToxin, true));
                    break;
                    
                case ZodiacSign.Sagittarius:
                    // Projectile speed and roll cooldown
                    activeAbilities.Add(ZodiacAbilityType.SagittariusProjectileBoost, new ZodiacAbility(ZodiacAbilityType.SagittariusProjectileBoost, true));
                    activeAbilities.Add(ZodiacAbilityType.SagittariusRollBoost, new ZodiacAbility(ZodiacAbilityType.SagittariusRollBoost, true));
                    break;
                    
                case ZodiacSign.Capricorn:
                    // Free climbing
                    activeAbilities.Add(ZodiacAbilityType.CapricornClimbing, new ZodiacAbility(ZodiacAbilityType.CapricornClimbing, true));
                    break;
                    
                case ZodiacSign.Aquarius:
                    // Slow puddle after dodge
                    activeAbilities.Add(ZodiacAbilityType.AquariusSlowPuddle, new ZodiacAbility(ZodiacAbilityType.AquariusSlowPuddle, false));
                    break;
                    
                case ZodiacSign.Pisces:
                    // Chain lightning at low health
                    activeAbilities.Add(ZodiacAbilityType.PiscesChainLightning, new ZodiacAbility(ZodiacAbilityType.PiscesChainLightning, false));
                    break;
            }
        }
        
        /// <summary>
        /// Apply passive bonuses from zodiac abilities
        /// </summary>
        private void ApplyPassiveZodiacBonuses()
        {
            if (playerStats == null) return;
            
            // Reset all bonuses first
            playerStats.ResetZodiacBonuses();
            
            // Apply bonuses based on zodiac sign
            switch (currentZodiacSign)
            {
                case ZodiacSign.Aries:
                    playerStats.ApplyZodiacBonus(StatType.MoveSpeed, ariesSprintSpeedBonus);
                    // First hit damage is handled in weapon system
                    break;
                    
                case ZodiacSign.Taurus:
                    playerStats.ApplyZodiacBonus(StatType.MaxHealth, taurusMaxHpBonus);
                    playerStats.ApplyZodiacBonus(StatType.KnockbackResistance, taurusKnockbackReduction);
                    break;
                    
                case ZodiacSign.Virgo:
                    playerStats.ApplyZodiacBonus(StatType.HealingReceived, virgoHealingBonus);
                    // Drop penalty is handled in loot system
                    break;
                    
                case ZodiacSign.Sagittarius:
                    playerStats.ApplyZodiacBonus(StatType.ProjectileSpeed, sagittariusProjectileSpeedBonus);
                    playerStats.ApplyZodiacBonus(StatType.DashCooldown, -sagittariusRollCooldownReduction);
                    break;
                    
                case ZodiacSign.Scorpio:
                    // Toxin application is handled in weapon system
                    break;
                    
                case ZodiacSign.Capricorn:
                    // Free climbing is handled in player controller
                    break;
            }
        }
        
        /// <summary>
        /// Subscribe to relevant events for zodiac abilities
        /// </summary>
        private void SubscribeToEvents()
        {
            if (weaponSystem != null)
            {
                weaponSystem.OnWeaponFired += OnWeaponFired;
                weaponSystem.OnWeaponReloaded += OnWeaponReloaded;
                weaponSystem.OnEnemyHit += OnEnemyHit;
            }
            
            if (playerController != null)
            {
                playerController.OnDashPerformed += OnDashPerformed;
                playerController.OnClimbStarted += OnClimbStarted;
            }
            
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += OnHealthChanged;
                healthSystem.OnDamageReceived += OnDamageReceived;
                healthSystem.OnHealingReceived += OnHealingReceived;
            }
            
            // Subscribe to room change events from level manager
            LevelManager levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
            {
                levelManager.OnRoomChanged += OnRoomChanged;
            }
        }
        
        /// <summary>
        /// Update zodiac abilities that need periodic checking
        /// </summary>
        private void UpdateZodiacAbilities()
        {
            // Check Libra equalization timer
            if (currentZodiacSign == ZodiacSign.Libra)
            {
                libraEqualizationTimer += Time.deltaTime;
                
                if (libraEqualizationTimer >= libraEqualizationInterval)
                {
                    TriggerLibraEqualization();
                    libraEqualizationTimer = 0f;
                }
            }
            
            // Check Leo light bonus
            if (currentZodiacSign == ZodiacSign.Leo)
            {
                bool isNowInLight = CheckIfInLight();
                
                if (isNowInLight != isInLight)
                {
                    isInLight = isNowInLight;
                    UpdateLeoCritBonus();
                }
            }
            
            // Check Pisces low health status
            if (currentZodiacSign == ZodiacSign.Pisces && healthSystem != null)
            {
                bool isNowLowHealth = healthSystem.GetHealthPercent() <= piscesLowHealthThreshold;
                
                if (isNowLowHealth != isLowHealth)
                {
                    isLowHealth = isNowLowHealth;
                    // Update visual effects for Pisces ability
                    UpdatePiscesVisualEffects();
                }
            }
        }
        
        /// <summary>
        /// Clear all active zodiac abilities
        /// </summary>
        private void ClearZodiacAbilities()
        {
            // Remove any active effects
            if (playerStats != null)
            {
                playerStats.ResetZodiacBonuses();
            }
            
            // Clear ability list
            activeAbilities.Clear();
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handle weapon fired event for zodiac abilities
        /// </summary>
        private void OnWeaponFired(WeaponData weaponData)
        {
            // Check for Aries first-hit bonus
            if (currentZodiacSign == ZodiacSign.Aries && weaponData.IsFirstShotInClip)
            {
                TriggerAbility(ZodiacAbilityType.AriesFirstHitBonus);
                // The damage bonus is applied in weapon system
            }
            
            // Check for Pisces chain lightning at low health
            if (currentZodiacSign == ZodiacSign.Pisces && isLowHealth)
            {
                TriggerAbility(ZodiacAbilityType.PiscesChainLightning);
                // The chain lightning is handled by weapon system
            }
        }
        
        /// <summary>
        /// Handle weapon reloaded event for zodiac abilities
        /// </summary>
        private void OnWeaponReloaded(WeaponData weaponData)
        {
            // Check for Gemini doppelganger
            if (currentZodiacSign == ZodiacSign.Gemini)
            {
                TriggerAbility(ZodiacAbilityType.GeminiDoppelganger);
                SpawnGeminiDoppelganger(geminiDoppelDuration);
            }
        }
        
        /// <summary>
        /// Handle enemy hit event for zodiac abilities
        /// </summary>
        private void OnEnemyHit(GameObject enemy, float damage, bool isCritical)
        {
            // Check for Scorpio toxin application
            if (currentZodiacSign == ZodiacSign.Scorpio)
            {
                TriggerAbility(ZodiacAbilityType.ScorpioToxin);
                ApplyScorpioToxin(enemy);
            }
        }
        
        /// <summary>
        /// Handle dash performed event for zodiac abilities
        /// </summary>
        private void OnDashPerformed()
        {
            // Check for Aquarius slow puddle
            if (currentZodiacSign == ZodiacSign.Aquarius)
            {
                TriggerAbility(ZodiacAbilityType.AquariusSlowPuddle);
                SpawnAquariusSlowPuddle();
            }
        }
        
        /// <summary>
        /// Handle climb started event for zodiac abilities
        /// </summary>
        private void OnClimbStarted(float staminaCost)
        {
            // Check for Capricorn free climbing
            if (currentZodiacSign == ZodiacSign.Capricorn)
            {
                TriggerAbility(ZodiacAbilityType.CapricornClimbing);
                // Zero stamina cost is handled in player controller
            }
        }
        
        /// <summary>
        /// Handle health changed event for zodiac abilities
        /// </summary>
        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            // Check for Pisces low health ability
            bool isNowLowHealth = (currentHealth / maxHealth) <= piscesLowHealthThreshold;
            
            if (isNowLowHealth != isLowHealth)
            {
                isLowHealth = isNowLowHealth;
                
                if (isLowHealth)
                {
                    // Enable Pisces chain lightning
                    TriggerAbility(ZodiacAbilityType.PiscesChainLightning);
                }
                
                // Update visual effects
                UpdatePiscesVisualEffects();
            }
        }
        
        /// <summary>
        /// Handle damage received event for zodiac abilities
        /// </summary>
        private void OnDamageReceived(float damageAmount, DamageType damageType)
        {
            // Check for Cancer shell protection
            if (currentZodiacSign == ZodiacSign.Cancer && hasActiveCancerShell)
            {
                TriggerAbility(ZodiacAbilityType.CancerShell);
                // Shell absorption is handled in health system
                
                // Mark shell as used
                hasActiveCancerShell = false;
            }
        }
        
        /// <summary>
        /// Handle healing received event for zodiac abilities
        /// </summary>
        private void OnHealingReceived(float healAmount)
        {
            // Check for Virgo healing bonus
            if (currentZodiacSign == ZodiacSign.Virgo)
            {
                TriggerAbility(ZodiacAbilityType.VirgoHealingBonus);
                // Bonus is applied in health system
            }
        }
        
        /// <summary>
        /// Handle room changed event for zodiac abilities
        /// </summary>
        private void OnRoomChanged(RoomData newRoom)
        {
            // Check for Cancer shell reset on new room
            if (currentZodiacSign == ZodiacSign.Cancer && !hasActiveCancerShell)
            {
                hasActiveCancerShell = true;
                TriggerAbility(ZodiacAbilityType.CancerShell);
                ActivateCancerShell();
            }
        }
        
        #endregion
        
        #region Ability Implementations
        
        /// <summary>
        /// Spawn a Gemini doppelganger that mirrors player shots
        /// </summary>
        private void SpawnGeminiDoppelganger(float duration)
        {
            if (playerController == null) return;
            
            // Create doppelganger GameObject
            GameObject doppelganger = new GameObject("GeminiDoppelganger");
            
            // Position slightly offset from player
            Vector3 offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized * 1.5f;
            doppelganger.transform.position = transform.position + offset;
            
            // Add doppelganger component
            GeminiDoppelganger geminiDoppel = doppelganger.AddComponent<GeminiDoppelganger>();
            geminiDoppel.Initialize(playerController, duration);
            
            Debug.Log($"Spawned Gemini doppelganger for {duration} seconds");
        }
        
        /// <summary>
        /// Trigger Libra's damage/healing equalization ability
        /// </summary>
        private void TriggerLibraEqualization()
        {
            // Find nearest ally
            PlayerController nearestAlly = FindNearestAlly();
            
            if (nearestAlly != null)
            {
                // Get ally's health system
                HealthSystem allyHealth = nearestAlly.GetComponent<HealthSystem>();
                
                if (allyHealth != null && healthSystem != null)
                {
                    // Calculate average health percentage
                    float playerHealthPct = healthSystem.GetHealthPercent();
                    float allyHealthPct = allyHealth.GetHealthPercent();
                    float avgHealthPct = (playerHealthPct + allyHealthPct) / 2f;
                    
                    // Apply equalization
                    healthSystem.SetHealthPercent(avgHealthPct);
                    allyHealth.SetHealthPercent(avgHealthPct);
                    
                    TriggerAbility(ZodiacAbilityType.LibraEqualization);
                    
                    Debug.Log($"Libra equalization triggered with {nearestAlly.name}, balanced at {avgHealthPct:P0}");
                }
            }
        }
        
        /// <summary>
        /// Find the nearest ally player
        /// </summary>
        private PlayerController FindNearestAlly()
        {
            // Find all player controllers
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            
            PlayerController nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (PlayerController player in players)
            {
                // Skip self
                if (player == playerController) continue;
                
                float distance = Vector3.Distance(transform.position, player.transform.position);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = player;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// Apply Scorpio's toxin effect to an enemy
        /// </summary>
        private void ApplyScorpioToxin(GameObject enemy)
        {
            // Get or add toxin component to enemy
            ScorpioToxin toxin = enemy.GetComponent<ScorpioToxin>();
            
            if (toxin == null)
            {
                toxin = enemy.AddComponent<ScorpioToxin>();
            }
            
            // Apply/refresh toxin
            toxin.ApplyToxin(scorpioToxinDamagePercent);
        }
        
        /// <summary>
        /// Spawn Aquarius slow puddle after dodge
        /// </summary>
        private void SpawnAquariusSlowPuddle()
        {
            // Create puddle at player position
            GameObject puddle = new GameObject("AquariusSlowPuddle");
            puddle.transform.position = transform.position;
            
            // Add slow puddle component
            AquariusSlowPuddle slowPuddle = puddle.AddComponent<AquariusSlowPuddle>();
            slowPuddle.Initialize(aquariusSlowPuddleDuration, aquariusSlowPuddleSlowPercent);
            
            Debug.Log($"Spawned Aquarius slow puddle for {aquariusSlowPuddleDuration} seconds");
        }
        
        /// <summary>
        /// Update Leo's crit bonus based on light status
        /// </summary>
        private void UpdateLeoCritBonus()
        {
            if (playerStats == null) return;
            
            if (isInLight)
            {
                playerStats.ApplyZodiacBonus(StatType.CriticalChance, leoCritBonus);
                TriggerAbility(ZodiacAbilityType.LeoCritBonus);
                Debug.Log($"Leo crit bonus activated: +{leoCritBonus:P0} crit chance");
            }
            else
            {
                playerStats.ApplyZodiacBonus(StatType.CriticalChance, 0f);
                Debug.Log("Leo crit bonus deactivated");
            }
        }
        
        /// <summary>
        /// Check if the player is in light for Leo's ability
        /// </summary>
        private bool CheckIfInLight()
        {
            // Check if player is in a lit area
            // This is a simplified implementation - a real implementation would use
            // light intensity at player position from scene lights
            
            // Get player position
            Vector3 playerPos = transform.position;
            
            // Find all lights in scene
            Light[] sceneLights = FindObjectsOfType<Light>();
            
            // Check each light
            foreach (Light light in sceneLights)
            {
                if (!light.enabled) continue;
                
                float distance = Vector3.Distance(playerPos, light.transform.position);
                
                // Check if player is within light range
                if (light.type == LightType.Point || light.type == LightType.Spot)
                {
                    if (distance <= light.range)
                    {
                        return true;
                    }
                }
                else if (light.type == LightType.Directional)
                {
                    // Directional lights illuminate everything
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Activate Cancer's protective shell
        /// </summary>
        private void ActivateCancerShell()
        {
            if (healthSystem == null) return;
            
            // Add shell protection to health system
            healthSystem.AddShellProtection(cancerShellProtection);
            
            // Spawn visual effect
            SpawnCancerShellEffect();
            
            Debug.Log($"Cancer shell activated with {cancerShellProtection} protection");
        }
        
        /// <summary>
        /// Spawn visual effect for Cancer's shell
        /// </summary>
        private void SpawnCancerShellEffect()
        {
            // Create shell effect object
            GameObject shellEffect = new GameObject("CancerShellEffect");
            shellEffect.transform.SetParent(transform);
            shellEffect.transform.localPosition = Vector3.zero;
            
            // Add visual component
            CancerShellEffect shellVisual = shellEffect.AddComponent<CancerShellEffect>();
            shellVisual.Initialize(playerController, healthSystem);
        }
        
        /// <summary>
        /// Update visual effects for Pisces ability
        /// </summary>
        private void UpdatePiscesVisualEffects()
        {
            // Get or create effect controller
            PiscesChainLightningEffect effect = GetComponent<PiscesChainLightningEffect>();
            
            if (effect == null && isLowHealth)
            {
                // Add effect when entering low health
                effect = gameObject.AddComponent<PiscesChainLightningEffect>();
                effect.Initialize(piscesChainLightningTargets);
            }
            else if (effect != null && !isLowHealth)
            {
                // Remove effect when exiting low health
                Destroy(effect);
            }
        }
        
        #endregion
        
        /// <summary>
        /// Trigger a zodiac ability, firing relevant events
        /// </summary>
        private void TriggerAbility(ZodiacAbilityType abilityType)
        {
            if (activeAbilities.TryGetValue(abilityType, out ZodiacAbility ability))
            {
                // Fire event
                OnZodiacAbilityTriggered?.Invoke(abilityType);
                
                Debug.Log($"Triggered zodiac ability: {abilityType}");
            }
        }
        
        /// <summary>
        /// Get the modifier value for a specific zodiac ability
        /// </summary>
        public float GetZodiacAbilityModifier(ZodiacAbilityType abilityType)
        {
            switch (abilityType)
            {
                case ZodiacAbilityType.AriesSprintBoost:
                    return ariesSprintSpeedBonus;
                    
                case ZodiacAbilityType.AriesFirstHitBonus:
                    return ariesFirstHitDamageBonus;
                    
                case ZodiacAbilityType.TaurusHealthBonus:
                    return taurusMaxHpBonus;
                    
                case ZodiacAbilityType.TaurusKnockbackReduction:
                    return taurusKnockbackReduction;
                    
                case ZodiacAbilityType.LeoCritBonus:
                    return leoCritBonus;
                    
                case ZodiacAbilityType.VirgoHealingBonus:
                    return virgoHealingBonus;
                    
                case ZodiacAbilityType.VirgoDropPenalty:
                    return virgoDropPenalty;
                    
                case ZodiacAbilityType.ScorpioToxin:
                    return scorpioToxinDamagePercent;
                    
                case ZodiacAbilityType.SagittariusProjectileBoost:
                    return sagittariusProjectileSpeedBonus;
                    
                case ZodiacAbilityType.SagittariusRollBoost:
                    return sagittariusRollCooldownReduction;
                    
                default:
                    return 0f;
            }
        }
        
        /// <summary>
        /// Check if the player has a specific zodiac ability
        /// </summary>
        public bool HasZodiacAbility(ZodiacAbilityType abilityType)
        {
            return activeAbilities.ContainsKey(abilityType);
        }
    }
    
    /// <summary>
    /// Data class for zodiac abilities
    /// </summary>
    public class ZodiacAbility
    {
        public ZodiacAbilityType Type { get; private set; }
        public bool IsPassive { get; private set; }
        
        public ZodiacAbility(ZodiacAbilityType type, bool isPassive)
        {
            Type = type;
            IsPassive = isPassive;
        }
    }
    
    /// <summary>
    /// Types of zodiac abilities
    /// </summary>
    public enum ZodiacAbilityType
    {
        AriesSprintBoost,
        AriesFirstHitBonus,
        TaurusHealthBonus,
        TaurusKnockbackReduction,
        GeminiDoppelganger,
        CancerShell,
        LeoCritBonus,
        VirgoHealingBonus,
        VirgoDropPenalty,
        LibraEqualization,
        ScorpioToxin,
        SagittariusProjectileBoost,
        SagittariusRollBoost,
        CapricornClimbing,
        AquariusSlowPuddle,
        PiscesChainLightning
    }
}