using System.Collections.Generic;
using UnityEngine;
using System;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    // This class defines core combat systems including dodge, parry, and i-frames
    public class CombatSystem : MonoBehaviour
    {
        // Singleton instance
        public static CombatSystem Instance { get; private set; }
        
        [Header("Dodge Settings")]
        [SerializeField] private float baseDodgeDuration = 0.5f;
        [SerializeField] private float baseDodgeCooldown = 1.5f;
        [SerializeField] private float baseDodgeDistance = 5f;
        [SerializeField] private float baseInvincibilityFrames = 0.3f;
        [SerializeField] private float dodgeStaminaCost = 20f;
        [SerializeField] private AnimationCurve dodgeSpeedCurve;
        [SerializeField] private GameObject dodgeEffectPrefab;
        [SerializeField] private AudioClip dodgeSound;
        [SerializeField] private GameObject invincibilityEffectPrefab;
        
        [Header("Parry Settings")]
        [SerializeField] private float parryWindow = 0.2f;
        [SerializeField] private float parryResetTime = 0.5f;
        [SerializeField] private float parryReflectDamageMultiplier = 1.5f;
        [SerializeField] private float parryStunDuration = 1.0f;
        [SerializeField] private float parryStaminaCost = 15f;
        [SerializeField] private GameObject parryEffectPrefab;
        [SerializeField] private AudioClip parrySound;
        [SerializeField] private AudioClip parryFailSound;
        
        [Header("Armor System")]
        [SerializeField] private AnimationCurve armorEffectivenessCurve;
        [SerializeField] private float armorDamageReductionMultiplier = 0.5f;
        [SerializeField] private float armorIFramesBonusMultiplier = 0.05f; // Each armor point adds this much to iframes
        [SerializeField] private float maxArmorIFramesBonus = 0.5f; // Cap on extra iframes from armor
        
        // Zodiac sigil integration
        [Header("Zodiac Integration")]
        [SerializeField] private float sagittariusDodgeCooldownReduction = 0.2f;
        [SerializeField] private float aquariusSlowingPuddleDuration = 3.0f;
        [SerializeField] private float aquariusSlowingPuddleRadius = 3.0f;
        [SerializeField] private float aquariusSlowingPuddleSlowAmount = 0.3f;
        [SerializeField] private GameObject aquariusSlowingPuddlePrefab;
        
        // Events
        public event Action<PlayerController, Vector3> OnPlayerDodge;
        public event Action<PlayerController, bool> OnPlayerParry;
        public event Action<PlayerController, GameObject> OnPlayerAquariusPuddle;
        
        private void Awake()
        {
            // Setup singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCombatSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeCombatSystem()
        {
            // Set default dodge speed curve if not defined
            if (dodgeSpeedCurve == null || dodgeSpeedCurve.keys.Length == 0)
            {
                dodgeSpeedCurve = new AnimationCurve(
                    new Keyframe(0, 0.2f),
                    new Keyframe(0.1f, 1.0f),
                    new Keyframe(0.7f, 0.8f),
                    new Keyframe(1.0f, 0.0f)
                );
            }
            
            // Set default armor effectiveness curve if not defined
            if (armorEffectivenessCurve == null || armorEffectivenessCurve.keys.Length == 0)
            {
                armorEffectivenessCurve = new AnimationCurve(
                    new Keyframe(0, 0),
                    new Keyframe(50, 0.4f),
                    new Keyframe(100, 0.65f),
                    new Keyframe(200, 0.8f)
                );
                // Diminishing returns on armor effectiveness
            }
            
            Debug.Log("Combat System initialized");
        }
        
        #region Dodge System
        
        // Calculate dodge parameters for a player
        public DodgeParameters CalculateDodgeParameters(PlayerController player)
        {
            DodgeParameters parameters = new DodgeParameters
            {
                Duration = baseDodgeDuration,
                Cooldown = baseDodgeCooldown,
                Distance = baseDodgeDistance,
                InvincibilityFrames = baseInvincibilityFrames,
                StaminaCost = dodgeStaminaCost,
                SpeedCurve = dodgeSpeedCurve
            };
            
            // Apply player class modifiers
            if (player.PlayerClass != null)
            {
                parameters.Duration *= player.PlayerClass.DodgeDurationMultiplier;
                parameters.Cooldown *= player.PlayerClass.DodgeCooldownMultiplier;
                parameters.Distance *= player.PlayerClass.DodgeDistanceMultiplier;
                parameters.InvincibilityFrames *= player.PlayerClass.InvincibilityFramesMultiplier;
                parameters.StaminaCost *= player.PlayerClass.StaminaCostMultiplier;
            }
            
            // Apply armor-based iframe bonus
            float armorValue = player.ArmorValue;
            float iframeBonus = Mathf.Min(armorValue * armorIFramesBonusMultiplier, maxArmorIFramesBonus);
            parameters.InvincibilityFrames += iframeBonus;
            
            // Apply Zodiac sigil effects
            if (player.HasZodiacSignil("Sagittarius"))
            {
                parameters.Cooldown *= (1 - sagittariusDodgeCooldownReduction);
            }
            
            // Apply any tarot card effects
            ApplyTarotCardEffectsToDodge(player, ref parameters);
            
            return parameters;
        }
        
        // Apply tarot card effects to dodge parameters
        private void ApplyTarotCardEffectsToDodge(PlayerController player, ref DodgeParameters parameters)
        {
            // Example tarot card effects
            if (player.HasTarotCard("The Fool"))
            {
                parameters.InvincibilityFrames *= 1.2f; // 20% more invincibility
            }
            
            if (player.HasTarotCard("The Wheel of Fortune"))
            {
                parameters.Cooldown *= 0.8f; // 20% less cooldown
            }
            
            if (player.HasTarotCard("The Chariot"))
            {
                parameters.Distance *= 1.3f; // 30% more distance
            }
        }
        
        // Perform a dodge for the player
        public bool PerformDodge(PlayerController player, Vector2 direction)
        {
            if (player == null || !player.CanDodge)
                return false;
            
            // Get dodge parameters
            DodgeParameters parameters = CalculateDodgeParameters(player);
            
            // Check if player has enough stamina
            if (player.CurrentStamina < parameters.StaminaCost)
            {
                Debug.Log("Not enough stamina to dodge");
                return false;
            }
            
            // Consume stamina
            player.ConsumeStamina(parameters.StaminaCost);
            
            // Normalize direction if it's not zero
            if (direction.magnitude > 0)
            {
                direction.Normalize();
            }
            else
            {
                // Default to facing direction if no input
                direction = player.LastMoveDirection;
            }
            
            // Start dodge coroutine
            player.StartDodge(direction, parameters);
            
            // Create effects
            if (dodgeEffectPrefab != null)
            {
                Instantiate(dodgeEffectPrefab, player.transform.position, Quaternion.identity);
            }
            
            // Play sound
            if (dodgeSound != null)
            {
                GameManager.Instance?.PlaySound(dodgeSound);
            }
            
            // Create Aquarius puddle effect if applicable
            if (player.HasZodiacSignil("Aquarius"))
            {
                CreateAquariusPuddle(player);
            }
            
            // Trigger event
            OnPlayerDodge?.Invoke(player, new Vector3(direction.x, 0, direction.y));
            
            return true;
        }
        
        // Create Aquarius slowing puddle
        private void CreateAquariusPuddle(PlayerController player)
        {
            if (aquariusSlowingPuddlePrefab == null)
                return;
            
            // Create puddle at player position
            GameObject puddle = Instantiate(aquariusSlowingPuddlePrefab, player.transform.position, Quaternion.identity);
            
            // Set up puddle parameters
            SlowingPuddle puddleComponent = puddle.GetComponent<SlowingPuddle>();
            if (puddleComponent != null)
            {
                puddleComponent.Initialize(aquariusSlowingPuddleDuration, aquariusSlowingPuddleRadius, aquariusSlowingPuddleSlowAmount);
            }
            
            // Trigger event
            OnPlayerAquariusPuddle?.Invoke(player, puddle);
        }
        
        #endregion
        
        #region Parry System
        
        // Calculate parry parameters for a player
        public ParryParameters CalculateParryParameters(PlayerController player)
        {
            ParryParameters parameters = new ParryParameters
            {
                WindowDuration = parryWindow,
                ResetTime = parryResetTime,
                ReflectDamageMultiplier = parryReflectDamageMultiplier,
                StunDuration = parryStunDuration,
                StaminaCost = parryStaminaCost
            };
            
            // Apply player class modifiers
            if (player.PlayerClass != null)
            {
                parameters.WindowDuration *= player.PlayerClass.ParryWindowMultiplier;
                parameters.ResetTime *= player.PlayerClass.ParryResetMultiplier;
                parameters.ReflectDamageMultiplier *= player.PlayerClass.ParryDamageMultiplier;
                parameters.StunDuration *= player.PlayerClass.ParryStunMultiplier;
                parameters.StaminaCost *= player.PlayerClass.StaminaCostMultiplier;
            }
            
            // Apply tarot card effects to parry parameters
            ApplyTarotCardEffectsToParry(player, ref parameters);
            
            return parameters;
        }
        
        // Apply tarot card effects to parry parameters
        private void ApplyTarotCardEffectsToParry(PlayerController player, ref ParryParameters parameters)
        {
            // Example tarot card effects
            if (player.HasTarotCard("Justice"))
            {
                parameters.ReflectDamageMultiplier *= 1.25f; // 25% more reflect damage
            }
            
            if (player.HasTarotCard("The Sun"))
            {
                parameters.WindowDuration *= 1.3f; // 30% larger parry window
            }
        }
        
        // Attempt to parry for the player
        public bool AttemptParry(PlayerController player)
        {
            if (player == null || !player.CanParry)
                return false;
            
            // Get parry parameters
            ParryParameters parameters = CalculateParryParameters(player);
            
            // Check if player has enough stamina
            if (player.CurrentStamina < parameters.StaminaCost)
            {
                Debug.Log("Not enough stamina to parry");
                return false;
            }
            
            // Consume stamina
            player.ConsumeStamina(parameters.StaminaCost);
            
            // Start parry coroutine
            player.StartParry(parameters);
            
            return true;
        }
        
        // Handle parry result
        public void HandleParryResult(PlayerController player, bool successful, GameObject parryTarget = null)
        {
            // Create effects
            if (successful && parryEffectPrefab != null)
            {
                Instantiate(parryEffectPrefab, player.transform.position, Quaternion.identity);
                
                // Play successful parry sound
                if (parrySound != null)
                {
                    GameManager.Instance?.PlaySound(parrySound);
                }
                
                // Handle reflecting damage to the target
                if (parryTarget != null)
                {
                    ParryParameters parameters = CalculateParryParameters(player);
                    
                    // Get the component that can take damage
                    DamageableEntity targetEntity = parryTarget.GetComponent<DamageableEntity>();
                    if (targetEntity != null)
                    {
                        // Apply reflected damage
                        float reflectedDamage = targetEntity.LastAttackDamage * parameters.ReflectDamageMultiplier;
                        targetEntity.TakeDamage(reflectedDamage, DamageType.Reflected, player.gameObject);
                        
                        // Apply stun if applicable
                        EnemyController enemyController = parryTarget.GetComponent<EnemyController>();
                        if (enemyController != null)
                        {
                            enemyController.Stun(parameters.StunDuration);
                        }
                    }
                }
            }
            else if (!successful && parryFailSound != null)
            {
                // Play failed parry sound
                GameManager.Instance?.PlaySound(parryFailSound);
            }
            
            // Trigger event
            OnPlayerParry?.Invoke(player, successful);
        }
        
        #endregion
        
        #region Armor System
        
        // Calculate damage reduction from armor
        public float CalculateArmorDamageReduction(float armorValue, float incomingDamage)
        {
            // Use the armor effectiveness curve to determine damage reduction percentage
            float reductionPercentage = armorEffectivenessCurve.Evaluate(armorValue) * armorDamageReductionMultiplier;
            
            // Calculate reduced damage
            float reducedDamage = incomingDamage * reductionPercentage;
            
            // Ensure we don't reduce damage below zero
            return Mathf.Min(reducedDamage, incomingDamage);
        }
        
        #endregion
    }
    
    // Struct to hold dodge parameters
    [System.Serializable]
    public struct DodgeParameters
    {
        public float Duration;
        public float Cooldown;
        public float Distance;
        public float InvincibilityFrames;
        public float StaminaCost;
        public AnimationCurve SpeedCurve;
    }
    
    // Struct to hold parry parameters
    [System.Serializable]
    public struct ParryParameters
    {
        public float WindowDuration;
        public float ResetTime;
        public float ReflectDamageMultiplier;
        public float StunDuration;
        public float StaminaCost;
    }
    
    // Enum for damage types
    public enum DamageType
    {
        Physical,
        Fire,
        Frost,
        Lightning,
        Poison,
        Arcane,
        Void,
        Mind,
        Reflected,
        True // Ignores armor
    }
}