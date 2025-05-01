using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Manages entity health, damage, healing, and related effects
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private bool isInvulnerable = false;
        
        [Header("Damage Effects")]
        [SerializeField] private GameObject damageEffectPrefab;
        [SerializeField] private AudioClip damageSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private float damageFlashDuration = 0.1f;
        
        [Header("References")]
        [SerializeField] private Renderer entityRenderer;
        [SerializeField] private AudioSource audioSource;
        
        // Components
        private PlayerStats playerStats;
        
        // Events
        public UnityEvent<float, float> OnHealthChanged;
        public UnityEvent<DamageType, Vector3> OnDamaged;
        public UnityEvent<float> OnHealed;
        public UnityEvent OnDeath;
        
        private void Awake()
        {
            // Try to get references if not set in inspector
            if (entityRenderer == null) entityRenderer = GetComponentInChildren<Renderer>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            
            // Get optional player stats component
            playerStats = GetComponent<PlayerStats>();
            
            // Initialize health from player stats if available
            if (playerStats != null)
            {
                maxHealth = playerStats.GetMaxHealth();
                currentHealth = maxHealth;
            }
        }
        
        /// <summary>
        /// Apply damage to the entity
        /// </summary>
        public void ApplyDamage(float amount, DamageType damageType, Vector3 hitPoint, GameObject damageSource = null)
        {
            if (isInvulnerable) return;
            
            // Calculate actual damage based on damage type, resistances, etc.
            float actualDamage = CalculateDamage(amount, damageType);
            
            // Apply damage
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);
            
            // Trigger damage effects
            if (actualDamage > 0)
            {
                StartCoroutine(DamageFlashEffect());
                PlayDamageEffect(hitPoint, damageType);
                PlayDamageSound();
            }
            
            // Invoke events
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnDamaged?.Invoke(damageType, hitPoint);
            
            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Apply healing to the entity
        /// </summary>
        public void Heal(float amount)
        {
            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            
            // Only invoke events if health actually changed
            if (currentHealth > previousHealth)
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                OnHealed?.Invoke(currentHealth - previousHealth);
            }
        }
        
        /// <summary>
        /// Set the entity's invulnerability state
        /// </summary>
        public void SetInvulnerable(bool invulnerable)
        {
            isInvulnerable = invulnerable;
        }
        
        /// <summary>
        /// Calculate damage based on damage type, resistances, etc.
        /// </summary>
        private float CalculateDamage(float amount, DamageType damageType)
        {
            float damageMultiplier = 1.0f;
            
            // Apply resistance/vulnerability based on damage type
            if (playerStats != null)
            {
                // Apply defense reduction
                float defense = playerStats.GetDefense();
                amount = Mathf.Max(1, amount - defense * 0.5f);
                
                // For now, just a simple calculation. This would be expanded based on zodiac sign resistances.
                switch (damageType)
                {
                    case DamageType.Physical:
                        // Physical damage directly reduced by defense
                        break;
                    case DamageType.Fire:
                        // Fire damage calculations
                        if (playerStats.GetZodiacSign() == ZodiacSign.Aries ||
                            playerStats.GetZodiacSign() == ZodiacSign.Leo ||
                            playerStats.GetZodiacSign() == ZodiacSign.Sagittarius)
                        {
                            damageMultiplier = 0.75f; // Fire signs resist fire damage
                        }
                        break;
                    case DamageType.Water:
                        // Water damage calculations
                        if (playerStats.GetZodiacSign() == ZodiacSign.Cancer ||
                            playerStats.GetZodiacSign() == ZodiacSign.Scorpio ||
                            playerStats.GetZodiacSign() == ZodiacSign.Pisces)
                        {
                            damageMultiplier = 0.75f; // Water signs resist water damage
                        }
                        break;
                    case DamageType.Earth:
                        // Earth damage calculations
                        if (playerStats.GetZodiacSign() == ZodiacSign.Taurus ||
                            playerStats.GetZodiacSign() == ZodiacSign.Virgo ||
                            playerStats.GetZodiacSign() == ZodiacSign.Capricorn)
                        {
                            damageMultiplier = 0.75f; // Earth signs resist earth damage
                        }
                        break;
                    case DamageType.Air:
                        // Air damage calculations
                        if (playerStats.GetZodiacSign() == ZodiacSign.Gemini ||
                            playerStats.GetZodiacSign() == ZodiacSign.Libra ||
                            playerStats.GetZodiacSign() == ZodiacSign.Aquarius)
                        {
                            damageMultiplier = 0.75f; // Air signs resist air damage
                        }
                        break;
                    case DamageType.Cosmic:
                        // Cosmic damage ignores some defense
                        damageMultiplier = 1.25f;
                        break;
                    case DamageType.Sanity:
                        // Sanity damage is handled separately by SanitySystem
                        return 0;
                }
            }
            
            return amount * damageMultiplier;
        }
        
        /// <summary>
        /// Play damage visual effect
        /// </summary>
        private void PlayDamageEffect(Vector3 hitPoint, DamageType damageType)
        {
            if (damageEffectPrefab != null)
            {
                GameObject effect = Instantiate(damageEffectPrefab, hitPoint, Quaternion.identity);
                
                // Set effect properties based on damage type
                // This would be expanded in a full implementation
                ParticleSystem particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var main = particles.main;
                    
                    switch (damageType)
                    {
                        case DamageType.Fire:
                            main.startColor = new Color(1f, 0.5f, 0f, 1f); // Orange
                            break;
                        case DamageType.Water:
                            main.startColor = new Color(0f, 0.5f, 1f, 1f); // Blue
                            break;
                        case DamageType.Earth:
                            main.startColor = new Color(0.5f, 0.3f, 0.1f, 1f); // Brown
                            break;
                        case DamageType.Air:
                            main.startColor = new Color(0.8f, 0.8f, 1f, 1f); // Light blue
                            break;
                        case DamageType.Cosmic:
                            main.startColor = new Color(0.8f, 0f, 1f, 1f); // Purple
                            break;
                        case DamageType.Sanity:
                            main.startColor = new Color(0f, 0.8f, 0f, 1f); // Green
                            break;
                        default:
                            main.startColor = Color.red; // Default red
                            break;
                    }
                }
                
                // Destroy effect after duration
                Destroy(effect, 2f);
            }
        }
        
        /// <summary>
        /// Play damage sound effect
        /// </summary>
        private void PlayDamageSound()
        {
            if (audioSource != null && damageSound != null)
            {
                audioSource.PlayOneShot(damageSound);
            }
        }
        
        /// <summary>
        /// Flash the entity renderer to indicate damage
        /// </summary>
        private IEnumerator DamageFlashEffect()
        {
            if (entityRenderer != null)
            {
                // Store original materials
                Material[] originalMaterials = entityRenderer.materials;
                Material[] flashMaterials = new Material[originalMaterials.Length];
                
                // Create flash materials
                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    flashMaterials[i] = new Material(originalMaterials[i]);
                    flashMaterials[i].color = Color.red;
                }
                
                // Apply flash materials
                entityRenderer.materials = flashMaterials;
                
                // Wait for flash duration
                yield return new WaitForSeconds(damageFlashDuration);
                
                // Restore original materials
                entityRenderer.materials = originalMaterials;
                
                // Clean up flash materials
                for (int i = 0; i < flashMaterials.Length; i++)
                {
                    Destroy(flashMaterials[i]);
                }
            }
            else
            {
                yield return null;
            }
        }
        
        /// <summary>
        /// Handle entity death
        /// </summary>
        private void Die()
        {
            // Play death sound
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            // Invoke death event
            OnDeath?.Invoke();
            
            // Additional death logic would be implemented here
            // e.g., play death animation, spawn loot, etc.
        }
        
        #region Getter Methods
        
        /// <summary>
        /// Get the current health
        /// </summary>
        public float GetCurrentHealth()
        {
            return currentHealth;
        }
        
        /// <summary>
        /// Get the maximum health
        /// </summary>
        public float GetMaxHealth()
        {
            return maxHealth;
        }
        
        /// <summary>
        /// Get the current health as a percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }
        
        /// <summary>
        /// Check if entity is alive
        /// </summary>
        public bool IsAlive()
        {
            return currentHealth > 0;
        }
        
        /// <summary>
        /// Check if entity is invulnerable
        /// </summary>
        public bool IsInvulnerable()
        {
            return isInvulnerable;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Types of damage that can be dealt
    /// </summary>
    public enum DamageType
    {
        Physical,
        Fire,
        Water,
        Earth,
        Air,
        Cosmic,
        Sanity
    }
}