using UnityEngine;
using System;
using System.Collections.Generic;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.Resources
{
    // Base scriptable object for Cosmic Madness abilities
    [CreateAssetMenu(fileName = "New Cosmic Ability", menuName = "Codex/Cosmic Madness/Ability")]
    public class CosmicMadnessAbility : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string abilityName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private CosmicMadnessCategory category;
        
        [Header("Requirements")]
        [SerializeField] private int requiredPoints = 1;
        [SerializeField] private SanityState minimumSanityState = SanityState.Disturbed;
        [SerializeField] private CosmicMadnessAbility[] prerequisites;
        [SerializeField] private bool isZodiacLocked = false;
        [SerializeField] private ZodiacSign[] compatibleZodiacSigns;
        
        [Header("Activation")]
        [SerializeField] private float sanityActivationThreshold = 0.3f; // Activates below this sanity percentage
        [SerializeField] private float activationCooldown = 60f; // Cooldown between activations in seconds
        [SerializeField] private bool isPermanentOnUnlock = false; // If true, effect is always active once unlocked
        [SerializeField] private bool consumesSanity = true; // If true, activation consumes sanity
        [SerializeField] private float sanityCost = 10f; // Amount of sanity consumed on activation
        
        [Header("Effects")]
        [SerializeField] private bool hasZodiacVariants = false; // If true, effect varies by zodiac sign
        [SerializeField] private GameObject activationVFXPrefab;
        [SerializeField] private AudioClip activationSFX;
        
        // Properties
        public string AbilityName => abilityName;
        public string Description => description;
        public Sprite Icon => icon;
        public CosmicMadnessCategory Category => category;
        public int RequiredPoints => requiredPoints;
        public SanityState MinimumSanityState => minimumSanityState;
        public CosmicMadnessAbility[] Prerequisites => prerequisites;
        public bool IsZodiacLocked => isZodiacLocked;
        public ZodiacSign[] CompatibleZodiacSigns => compatibleZodiacSigns;
        public float SanityActivationThreshold => sanityActivationThreshold;
        public float ActivationCooldown => activationCooldown;
        public bool IsPermanentOnUnlock => isPermanentOnUnlock;
        public bool ConsumesSanity => consumesSanity;
        public float SanityCost => sanityCost;
        public bool HasZodiacVariants => hasZodiacVariants;
        
        // Runtime properties
        private bool _isUnlocked = false;
        private bool _isActive = false;
        private float _lastActivationTime = -999f;
        
        public bool IsUnlocked => _isUnlocked;
        public bool IsActive => _isActive;
        public bool CanActivate => _isUnlocked && (Time.time - _lastActivationTime > activationCooldown);
        
        // Called when the ability is unlocked in the skill tree
        public virtual void Unlock()
        {
            _isUnlocked = true;
            
            // If permanent, activate immediately
            if (isPermanentOnUnlock)
            {
                Activate(null);
            }
            
            Debug.Log($"Cosmic Ability '{abilityName}' unlocked!");
        }
        
        // Called when the ability is activated by low sanity
        public virtual bool Activate(PlayerController player)
        {
            if (!_isUnlocked || player == null)
                return false;
                
            // Check cooldown
            if (Time.time - _lastActivationTime < activationCooldown)
                return false;
                
            // Check zodiac compatibility if locked
            if (isZodiacLocked && compatibleZodiacSigns != null && compatibleZodiacSigns.Length > 0)
            {
                bool isCompatible = false;
                ZodiacSign playerSign = player.GetZodiacSign();
                
                foreach (var sign in compatibleZodiacSigns)
                {
                    if (sign == playerSign)
                    {
                        isCompatible = true;
                        break;
                    }
                }
                
                if (!isCompatible)
                    return false;
            }
            
            // Update state
            _isActive = true;
            _lastActivationTime = Time.time;
            
            // Apply effect
            ApplyEffect(player);
            
            // Play VFX
            if (activationVFXPrefab != null)
            {
                GameObject vfx = Instantiate(activationVFXPrefab, player.transform.position, Quaternion.identity);
                vfx.transform.SetParent(player.transform);
                Destroy(vfx, 5f);
            }
            
            // Play SFX
            if (activationSFX != null)
            {
                AudioSource.PlayClipAtPoint(activationSFX, player.transform.position);
            }
            
            // Consume sanity if needed
            if (consumesSanity)
            {
                SanitySystem.Instance?.ModifyPlayerSanity(player.NetworkId, -sanityCost);
            }
            
            Debug.Log($"Cosmic Ability '{abilityName}' activated!");
            return true;
        }
        
        // Called when the ability effect ends
        public virtual void Deactivate(PlayerController player)
        {
            if (!_isActive || player == null)
                return;
                
            // Skip deactivation for permanent abilities
            if (isPermanentOnUnlock)
                return;
                
            _isActive = false;
            
            // Remove effect
            RemoveEffect(player);
            
            Debug.Log($"Cosmic Ability '{abilityName}' deactivated.");
        }
        
        // Apply the ability's effect to the player
        protected virtual void ApplyEffect(PlayerController player)
        {
            // Base class implementation is empty
            // Derived classes should override this to implement specific effects
        }
        
        // Remove the ability's effect from the player
        protected virtual void RemoveEffect(PlayerController player)
        {
            // Base class implementation is empty
            // Derived classes should override this to implement specific effect removal
        }
        
        // Check if this ability is available to a specific player (based on zodiac, etc.)
        public virtual bool IsAvailableTo(PlayerController player)
        {
            if (player == null)
                return false;
                
            // Check zodiac compatibility if locked
            if (isZodiacLocked && compatibleZodiacSigns != null && compatibleZodiacSigns.Length > 0)
            {
                ZodiacSign playerSign = player.GetZodiacSign();
                
                bool isCompatible = false;
                foreach (var sign in compatibleZodiacSigns)
                {
                    if (sign == playerSign)
                    {
                        isCompatible = true;
                        break;
                    }
                }
                
                if (!isCompatible)
                    return false;
            }
            
            return true;
        }
        
        // Check if all prerequisites are unlocked
        public bool ArePrerequisitesMet(CosmicMadnessManager manager)
        {
            if (prerequisites == null || prerequisites.Length == 0)
                return true;
                
            foreach (var prereq in prerequisites)
            {
                if (prereq != null && !manager.IsAbilityUnlocked(prereq))
                    return false;
            }
            
            return true;
        }
        
        // Get a description with zodiac-specific details if applicable
        public string GetDetailedDescription(ZodiacSign playerSign)
        {
            if (!hasZodiacVariants)
                return description;
                
            // Check if this is for a compatible zodiac sign
            bool isCompatible = false;
            if (compatibleZodiacSigns != null)
            {
                foreach (var sign in compatibleZodiacSigns)
                {
                    if (sign == playerSign)
                    {
                        isCompatible = true;
                        break;
                    }
                }
            }
            
            // Add zodiac-specific details
            string zodiacDetails = GetZodiacSpecificDetails(playerSign);
            
            if (!isCompatible)
            {
                return description + "\n\n<color=red>Your zodiac sign is not compatible with this ability.</color>";
            }
            else if (!string.IsNullOrEmpty(zodiacDetails))
            {
                return description + "\n\n<color=yellow>" + zodiacDetails + "</color>";
            }
            
            return description;
        }
        
        // Get zodiac-specific details for the ability
        protected virtual string GetZodiacSpecificDetails(ZodiacSign sign)
        {
            // Base implementation returns empty string
            // Derived classes can override to provide zodiac-specific descriptions
            return "";
        }
    }
    
    // Categories of Cosmic Madness abilities
    public enum CosmicMadnessCategory
    {
        Transformation,   // Physical changes to the player
        Perception,       // Changes to how the player perceives the world
        Manifestation,    // Creating things from nothing
        Destruction,      // Enhanced destruction abilities
        Manipulation,     // Manipulating the environment
        Transcendence     // Beyond physical limitations
    }
}