using UnityEngine;
using System;
using CodexOfTheBrokenZodiac.Resources;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Core
{
    // Base class for any entity that can take damage
    public class DamageableEntity : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float healthRegen = 0f;
        [SerializeField] protected bool canRegenHealth = false;
        [SerializeField] protected float regenDelay = 5f;
        [SerializeField] protected bool destroyOnDeath = false;
        
        [Header("Armor Settings")]
        [SerializeField] protected float baseArmorValue = 0f;
        [SerializeField] protected List<DamageResistance> damageResistances = new List<DamageResistance>();
        
        [Header("Hit Effects")]
        [SerializeField] protected GameObject hitEffectPrefab;
        [SerializeField] protected GameObject deathEffectPrefab;
        [SerializeField] protected AudioClip hitSound;
        [SerializeField] protected AudioClip deathSound;
        [SerializeField] protected float hitFlashDuration = 0.1f;
        [SerializeField] protected Color hitFlashColor = Color.red;
        
        // Runtime variables
        protected bool isDead = false;
        protected bool isInvulnerable = false;
        protected float lastDamageTime;
        protected float lastAttackDamage;
        protected DamageType lastDamageType;
        protected GameObject lastDamageSource;
        protected float healthRegenTimer;
        protected List<DamageModifier> temporaryDamageModifiers = new List<DamageModifier>();
        
        // Events
        public event Action<float, float> OnHealthChanged;
        public event Action<DamageInfo> OnDamageTaken;
        public event Action<DamageInfo> OnDamageBlocked;
        public event Action<GameObject> OnDeath;
        
        // Properties
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float HealthPercentage => currentHealth / maxHealth;
        public float ArmorValue => CalculateTotalArmor();
        public bool IsInvulnerable => isInvulnerable;
        public bool IsDead => isDead;
        public float LastAttackDamage => lastAttackDamage;
        
        protected virtual void Awake()
        {
            // Initialize health
            currentHealth = maxHealth;
            lastDamageTime = -regenDelay;
            healthRegenTimer = regenDelay;
        }
        
        protected virtual void Update()
        {
            // Handle health regeneration
            if (canRegenHealth && !isDead && currentHealth < maxHealth)
            {
                if (Time.time - lastDamageTime > regenDelay)
                {
                    currentHealth = Mathf.Min(currentHealth + (healthRegen * Time.deltaTime), maxHealth);
                    OnHealthChanged?.Invoke(currentHealth, maxHealth);
                }
            }
            
            // Update temporary modifiers
            UpdateTemporaryModifiers();
        }
        
        // Update and remove expired temporary modifiers
        protected virtual void UpdateTemporaryModifiers()
        {
            for (int i = temporaryDamageModifiers.Count - 1; i >= 0; i--)
            {
                var modifier = temporaryDamageModifiers[i];
                modifier.RemainingDuration -= Time.deltaTime;
                
                if (modifier.RemainingDuration <= 0)
                {
                    temporaryDamageModifiers.RemoveAt(i);
                }
            }
        }
        
        // Take damage with type information
        public virtual void TakeDamage(float amount, DamageType damageType = DamageType.Physical, GameObject source = null)
        {
            // Store information about this attack
            lastAttackDamage = amount;
            lastDamageType = damageType;
            lastDamageSource = source;
            lastDamageTime = Time.time;
            
            // Check if can take damage
            if (isDead || isInvulnerable)
            {
                // Create damage blocked info
                DamageInfo blockedInfo = new DamageInfo
                {
                    OriginalAmount = amount,
                    FinalAmount = 0,
                    DamageType = damageType,
                    Source = source,
                    WasBlocked = true,
                    BlockReason = isDead ? "Already Dead" : "Invulnerable"
                };
                
                OnDamageBlocked?.Invoke(blockedInfo);
                return;
            }
            
            // Calculate damage after resistances, armor, and modifiers
            float modifiedDamage = CalculateModifiedDamage(amount, damageType);
            
            // Apply damage
            ApplyDamage(modifiedDamage, damageType, source);
        }
        
        // Apply final damage
        protected virtual void ApplyDamage(float amount, DamageType damageType, GameObject source)
        {
            // Reduce health
            float oldHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - amount);
            
            // Create damage info for event
            DamageInfo damageInfo = new DamageInfo
            {
                OriginalAmount = lastAttackDamage,
                FinalAmount = amount,
                DamageType = damageType,
                Source = source,
                WasBlocked = false,
                TargetHealthBefore = oldHealth,
                TargetHealthAfter = currentHealth
            };
            
            // Invoke damage event
            OnDamageTaken?.Invoke(damageInfo);
            
            // Invoke health changed event
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Play hit effects
            PlayHitEffects(damageType, amount);
            
            // Check for death
            if (currentHealth <= 0 && !isDead)
            {
                Die(source);
            }
        }
        
        // Calculate final damage after resistances, armor, and modifiers
        protected virtual float CalculateModifiedDamage(float amount, DamageType damageType)
        {
            // Skip armor calculation for True damage
            if (damageType == DamageType.True)
                return amount;
            
            // Apply resistances
            float resistance = GetResistanceForDamageType(damageType);
            float afterResistance = amount * (1f - resistance);
            
            // Apply armor (for physical damage)
            float armorReduction = 0f;
            if (damageType == DamageType.Physical && ArmorValue > 0)
            {
                armorReduction = CombatSystem.Instance?.CalculateArmorDamageReduction(ArmorValue, afterResistance) ?? 0f;
                afterResistance -= armorReduction;
            }
            
            // Apply temporary modifiers
            float modifiedDamage = afterResistance;
            
            foreach (var modifier in temporaryDamageModifiers)
            {
                if (modifier.AffectsDamageType(damageType))
                {
                    if (modifier.IsMultiplier)
                    {
                        modifiedDamage *= modifier.Value;
                    }
                    else
                    {
                        modifiedDamage += modifier.Value;
                    }
                }
            }
            
            // Ensure damage is never negative
            return Mathf.Max(0, modifiedDamage);
        }
        
        // Get resistance value for a specific damage type
        protected virtual float GetResistanceForDamageType(DamageType damageType)
        {
            foreach (var resistance in damageResistances)
            {
                if (resistance.Type == damageType)
                {
                    return Mathf.Clamp01(resistance.ResistanceValue);
                }
            }
            
            return 0f; // No resistance
        }
        
        // Calculate total armor value including modifiers
        protected virtual float CalculateTotalArmor()
        {
            float totalArmor = baseArmorValue;
            
            // Add any armor modifiers
            foreach (var modifier in temporaryDamageModifiers)
            {
                if (modifier.ModifierType == ModifierType.Armor)
                {
                    if (modifier.IsMultiplier)
                    {
                        totalArmor *= modifier.Value;
                    }
                    else
                    {
                        totalArmor += modifier.Value;
                    }
                }
            }
            
            return Mathf.Max(0, totalArmor);
        }
        
        // Add a temporary damage modifier
        public virtual void AddDamageModifier(DamageModifier modifier)
        {
            temporaryDamageModifiers.Add(modifier);
        }
        
        // Remove matching modifiers
        public virtual void RemoveDamageModifiers(ModifierType type)
        {
            temporaryDamageModifiers.RemoveAll(mod => mod.ModifierType == type);
        }
        
        // Set invulnerability state
        public virtual void SetInvulnerable(bool value)
        {
            isInvulnerable = value;
        }
        
        // Set max health with appropriate scaling of current health
        public virtual void SetMaxHealth(float newMax, bool healToFull = false)
        {
            float ratio = maxHealth > 0 ? currentHealth / maxHealth : 0;
            maxHealth = newMax;
            
            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = maxHealth * ratio;
            }
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        // Heal by a specific amount
        public virtual void Heal(float amount)
        {
            if (isDead) return;
            
            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            
            if (currentHealth != oldHealth)
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }
        
        // Die
        protected virtual void Die(GameObject killer)
        {
            if (isDead) return;
            
            isDead = true;
            
            // Play death effects
            PlayDeathEffects();
            
            // Trigger death event
            OnDeath?.Invoke(killer);
            
            // Handle destruction if needed
            if (destroyOnDeath)
            {
                Destroy(gameObject, 2f); // Wait for effects
            }
        }
        
        // Play hit effects
        protected virtual void PlayHitEffects(DamageType damageType, float damageAmount)
        {
            // Instantiate hit effect if available
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Play hit sound if available
            if (hitSound != null)
            {
                GameManager.Instance?.PlaySound(hitSound);
            }
            
            // Flash the renderer if available
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                StartCoroutine(FlashCoroutine(renderer, hitFlashColor, hitFlashDuration));
            }
        }
        
        // Play death effects
        protected virtual void PlayDeathEffects()
        {
            // Instantiate death effect if available
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Play death sound if available
            if (deathSound != null)
            {
                GameManager.Instance?.PlaySound(deathSound);
            }
        }
        
        // Flash coroutine for hit feedback
        protected System.Collections.IEnumerator FlashCoroutine(Renderer renderer, Color flashColor, float duration)
        {
            // Store original colors
            Material[] materials = renderer.materials;
            Color[] originalColors = new Color[materials.Length];
            
            for (int i = 0; i < materials.Length; i++)
            {
                originalColors[i] = materials[i].color;
                materials[i].color = flashColor;
            }
            
            // Wait for flash duration
            yield return new WaitForSeconds(duration);
            
            // Restore original colors
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].color = originalColors[i];
            }
        }
    }
    
    // Struct for damage information
    [System.Serializable]
    public struct DamageInfo
    {
        public float OriginalAmount;
        public float FinalAmount;
        public DamageType DamageType;
        public GameObject Source;
        public bool WasBlocked;
        public string BlockReason;
        public float TargetHealthBefore;
        public float TargetHealthAfter;
    }
    
    // Struct for damage resistance
    [System.Serializable]
    public struct DamageResistance
    {
        public DamageType Type;
        [Range(0f, 1f)] public float ResistanceValue; // 0 = no resistance, 1 = immune
    }
    
    // Enum for damage modifier types
    public enum ModifierType
    {
        Incoming,   // Affects incoming damage
        Outgoing,   // Affects damage dealt
        Armor,      // Affects armor value
        Healing     // Affects healing received
    }
    
    // Class for temporary damage modifiers
    [System.Serializable]
    public class DamageModifier
    {
        public ModifierType ModifierType;
        public float Value;
        public bool IsMultiplier;
        public float RemainingDuration;
        public DamageType[] AffectedDamageTypes = null; // null affects all
        
        public bool AffectsDamageType(DamageType type)
        {
            if (AffectedDamageTypes == null || AffectedDamageTypes.Length == 0)
                return true;
            
            for (int i = 0; i < AffectedDamageTypes.Length; i++)
            {
                if (AffectedDamageTypes[i] == type)
                    return true;
            }
            
            return false;
        }
    }
}