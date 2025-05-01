using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Manages player statistics, including base stats, modifiers, and buffs/debuffs
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private float baseHealth = 100f;
        [SerializeField] private float baseSanity = 100f;
        [SerializeField] private float baseMovementSpeed = 5f;
        [SerializeField] private float baseAttackPower = 10f;
        [SerializeField] private float baseAttackSpeed = 1f;
        [SerializeField] private float baseDefense = 5f;
        
        [Header("Zodiac Sign")]
        [SerializeField] private ZodiacSign zodiacSign = ZodiacSign.Aries;
        
        // Current modifier values
        private float healthModifier = 1f;
        private float sanityModifier = 1f;
        private float movementSpeedModifier = 1f;
        private float attackPowerModifier = 1f;
        private float attackSpeedModifier = 1f;
        private float defenseModifier = 1f;
        
        // Temporary buffs/debuffs
        private List<StatModifier> activeModifiers = new List<StatModifier>();
        
        private void Awake()
        {
            ApplyZodiacBaseModifiers();
        }
        
        private void Update()
        {
            UpdateModifiers();
        }
        
        /// <summary>
        /// Apply base stat modifiers based on the player's zodiac sign
        /// </summary>
        private void ApplyZodiacBaseModifiers()
        {
            switch (zodiacSign)
            {
                case ZodiacSign.Aries:
                    // Aries: Increased attack power, decreased defense
                    attackPowerModifier *= 1.2f;
                    defenseModifier *= 0.9f;
                    break;
                    
                case ZodiacSign.Taurus:
                    // Taurus: Increased defense, decreased movement speed
                    defenseModifier *= 1.3f;
                    movementSpeedModifier *= 0.9f;
                    break;
                    
                case ZodiacSign.Gemini:
                    // Gemini: Increased attack speed, decreased attack power
                    attackSpeedModifier *= 1.2f;
                    attackPowerModifier *= 0.9f;
                    break;
                    
                case ZodiacSign.Cancer:
                    // Cancer: Increased health, decreased movement speed
                    healthModifier *= 1.2f;
                    movementSpeedModifier *= 0.9f;
                    break;
                    
                case ZodiacSign.Leo:
                    // Leo: Increased attack power, decreased sanity stability
                    attackPowerModifier *= 1.15f;
                    sanityModifier *= 0.9f;
                    break;
                    
                case ZodiacSign.Virgo:
                    // Virgo: Increased sanity stability, decreased attack power
                    sanityModifier *= 1.25f;
                    attackPowerModifier *= 0.9f;
                    break;
                    
                case ZodiacSign.Libra:
                    // Libra: Balanced stats
                    // No modifiers
                    break;
                    
                case ZodiacSign.Scorpio:
                    // Scorpio: Increased attack power, decreased defense
                    attackPowerModifier *= 1.25f;
                    defenseModifier *= 0.85f;
                    break;
                    
                case ZodiacSign.Sagittarius:
                    // Sagittarius: Increased movement speed, decreased defense
                    movementSpeedModifier *= 1.2f;
                    defenseModifier *= 0.9f;
                    break;
                    
                case ZodiacSign.Capricorn:
                    // Capricorn: Increased defense, decreased attack speed
                    defenseModifier *= 1.25f;
                    attackSpeedModifier *= 0.9f;
                    break;
                    
                case ZodiacSign.Aquarius:
                    // Aquarius: Increased sanity effects, decreased health
                    sanityModifier *= 1.3f;
                    healthModifier *= 0.9f;
                    break;
                    
                case ZodiacSign.Pisces:
                    // Pisces: Increased sanity threshold, decreased attack power
                    sanityModifier *= 1.2f;
                    attackPowerModifier *= 0.9f;
                    break;
            }
        }
        
        /// <summary>
        /// Update all active stat modifiers and remove expired ones
        /// </summary>
        private void UpdateModifiers()
        {
            for (int i = activeModifiers.Count - 1; i >= 0; i--)
            {
                StatModifier modifier = activeModifiers[i];
                
                modifier.RemainingDuration -= Time.deltaTime;
                
                if (modifier.RemainingDuration <= 0f)
                {
                    RemoveModifier(modifier);
                    activeModifiers.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Add a temporary stat modifier
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            activeModifiers.Add(modifier);
            
            // Apply the modifier effect
            switch (modifier.StatType)
            {
                case StatType.Health:
                    healthModifier *= modifier.Multiplier;
                    break;
                case StatType.Sanity:
                    sanityModifier *= modifier.Multiplier;
                    break;
                case StatType.MovementSpeed:
                    movementSpeedModifier *= modifier.Multiplier;
                    break;
                case StatType.AttackPower:
                    attackPowerModifier *= modifier.Multiplier;
                    break;
                case StatType.AttackSpeed:
                    attackSpeedModifier *= modifier.Multiplier;
                    break;
                case StatType.Defense:
                    defenseModifier *= modifier.Multiplier;
                    break;
            }
        }
        
        /// <summary>
        /// Remove a stat modifier
        /// </summary>
        private void RemoveModifier(StatModifier modifier)
        {
            // Remove the modifier effect
            switch (modifier.StatType)
            {
                case StatType.Health:
                    healthModifier /= modifier.Multiplier;
                    break;
                case StatType.Sanity:
                    sanityModifier /= modifier.Multiplier;
                    break;
                case StatType.MovementSpeed:
                    movementSpeedModifier /= modifier.Multiplier;
                    break;
                case StatType.AttackPower:
                    attackPowerModifier /= modifier.Multiplier;
                    break;
                case StatType.AttackSpeed:
                    attackSpeedModifier /= modifier.Multiplier;
                    break;
                case StatType.Defense:
                    defenseModifier /= modifier.Multiplier;
                    break;
            }
        }
        
        #region Getter Methods
        
        /// <summary>
        /// Get the player's zodiac sign
        /// </summary>
        public ZodiacSign GetZodiacSign()
        {
            return zodiacSign;
        }
        
        /// <summary>
        /// Get the maximum health value including modifiers
        /// </summary>
        public float GetMaxHealth()
        {
            return baseHealth * healthModifier;
        }
        
        /// <summary>
        /// Get the maximum sanity value including modifiers
        /// </summary>
        public float GetMaxSanity()
        {
            return baseSanity * sanityModifier;
        }
        
        /// <summary>
        /// Get the movement speed modifier
        /// </summary>
        public float GetMovementSpeedModifier()
        {
            return movementSpeedModifier;
        }
        
        /// <summary>
        /// Get the attack power value including modifiers
        /// </summary>
        public float GetAttackPower()
        {
            return baseAttackPower * attackPowerModifier;
        }
        
        /// <summary>
        /// Get the attack speed modifier
        /// </summary>
        public float GetAttackSpeedModifier()
        {
            return attackSpeedModifier;
        }
        
        /// <summary>
        /// Get the defense value including modifiers
        /// </summary>
        public float GetDefense()
        {
            return baseDefense * defenseModifier;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a temporary modification to a player stat
    /// </summary>
    [System.Serializable]
    public class StatModifier
    {
        public StatType StatType;
        public float Multiplier;
        public float Duration;
        public float RemainingDuration;
        public string Source;
        
        public StatModifier(StatType statType, float multiplier, float duration, string source)
        {
            StatType = statType;
            Multiplier = multiplier;
            Duration = duration;
            RemainingDuration = duration;
            Source = source;
        }
    }
    
    /// <summary>
    /// Types of stats that can be modified
    /// </summary>
    public enum StatType
    {
        Health,
        Sanity,
        MovementSpeed,
        AttackPower,
        AttackSpeed,
        Defense
    }
}