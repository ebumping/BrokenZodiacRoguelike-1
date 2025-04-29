using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
    [CreateAssetMenu(fileName = "NewZodiacSignil", menuName = "Codex/ZodiacSignil")]
    public class ZodiacSignil : ScriptableObject
    {
        [Header("Basic Info")]
        public string SignilName;
        public string Description;
        public string ZodiacSign;
        
        [Header("Visual Elements")]
        public Sprite SignilIcon;
        public Color SignilColor = Color.white;
        
        [Header("Celestial Alignment")]
        public DateTime AlignmentDate; // The date when this signil is most powerful
        public bool IsBroken = false; // If true, this signil represents a broken/corrupted zodiac sign
        
        [Header("Effects")]
        public List<SignilEffect> Effects = new List<SignilEffect>();
        
        // Passive effect that's always active
        [Header("Passive Effect")]
        public string PassiveEffectDescription;
        public float PassiveEffectPower = 1.0f;
        
        // Ultimate ability that charges over time
        [Header("Ultimate Ability")]
        public string UltimateAbilityName;
        public string UltimateAbilityDescription;
        public float UltimateChargeDuration = 60.0f;
        public float UltimateEffectDuration = 10.0f;
        
        // Compatibility with other zodiac signs
        [Header("Zodiac Compatibility")]
        public List<ZodiacCompatibility> Compatibilities = new List<ZodiacCompatibility>();
        
        // Get current power level based on date (proximity to alignment)
        public float GetCurrentPowerLevel()
        {
            // Base power is 1.0
            float power = 1.0f;
            
            // If we have an alignment date, calculate power boost/reduction based on current date
            if (AlignmentDate != default)
            {
                DateTime now = DateTime.Now;
                
                // Calculate days difference from alignment
                TimeSpan difference = AlignmentDate.Date - now.Date;
                int daysDifference = Math.Abs(difference.Days);
                
                // Maximum difference for adjustment (half year)
                const int MAX_DIFF_DAYS = 182;
                
                // If very close to alignment date, power is highest
                if (daysDifference < 7)
                {
                    power = 1.5f; // 50% boost during alignment week
                }
                // Otherwise scale based on proximity
                else
                {
                    // Power ranges from 0.8 to 1.2 based on proximity to alignment date
                    float proximityFactor = Mathf.Clamp01(1f - (float)daysDifference / MAX_DIFF_DAYS);
                    power = 0.8f + (proximityFactor * 0.4f);
                }
                
                // Broken signils have more unpredictable power - can be very strong or very weak
                if (IsBroken)
                {
                    // Random fluctuation between 0.6x and 1.8x
                    power *= UnityEngine.Random.Range(0.6f, 1.8f);
                }
            }
            
            return power;
        }
        
        // Apply this signil's effects to a player
        public void ApplyEffects(Core.PlayerController player)
        {
            if (player == null) return;
            
            float powerLevel = GetCurrentPowerLevel();
            
            // Apply each effect scaled by power level
            foreach (var effect in Effects)
            {
                ApplySignilEffect(player, effect, powerLevel);
            }
            
            Debug.Log($"Applied {SignilName} effects to player with power level {powerLevel}");
        }
        
        // Remove effects from a player
        public void RemoveEffects(Core.PlayerController player)
        {
            if (player == null) return;
            
            // This would undo the effects applied by this signil
            // Implementation depends on how effects are tracked and applied
            
            Debug.Log($"Removed {SignilName} effects from player");
        }
        
        // Get compatibility factor with another zodiac sign
        public float GetCompatibilityWith(string zodiacSign)
        {
            foreach (var compatibility in Compatibilities)
            {
                if (compatibility.ZodiacSign == zodiacSign)
                {
                    return compatibility.CompatibilityFactor;
                }
            }
            
            // Default neutral compatibility
            return 1.0f;
        }
        
        private void ApplySignilEffect(Core.PlayerController player, SignilEffect effect, float powerLevel)
        {
            float scaledValue = effect.Value * powerLevel;
            
            switch (effect.Type)
            {
                case EffectType.Health:
                    player.ModifyMaxHealth(scaledValue);
                    break;
                    
                case EffectType.HealthRegen:
                    player.SetHealthRegen(player.HealthRegen + scaledValue);
                    break;
                    
                case EffectType.Damage:
                    player.ModifyDamage(scaledValue);
                    break;
                    
                case EffectType.Speed:
                    player.ModifySpeed(scaledValue);
                    break;
                    
                case EffectType.CriticalChance:
                    player.ModifyCriticalChance(scaledValue);
                    break;
                    
                case EffectType.CooldownReduction:
                    player.ModifyCooldown(scaledValue);
                    break;
                    
                case EffectType.ManaMax:
                    player.SetMaxMana(player.MaxMana + scaledValue);
                    break;
                    
                case EffectType.ManaRegen:
                    player.SetManaRegen(player.ManaRegen + scaledValue);
                    break;
                    
                case EffectType.DodgeDistance:
                    // This would require a specific method in PlayerController
                    // player.ModifyDodgeDistance(scaledValue);
                    break;
                    
                case EffectType.SpecialAbility:
                    // Add special ability if string is provided in effect metadata
                    if (!string.IsNullOrEmpty(effect.AbilityName))
                    {
                        player.AddAbility(effect.AbilityName);
                    }
                    break;
            }
        }
    }
    
    [Serializable]
    public enum EffectType
    {
        Health,
        HealthRegen,
        Damage,
        Speed,
        CriticalChance,
        CooldownReduction,
        ManaMax,
        ManaRegen,
        DodgeDistance,
        SpecialAbility
    }
    
    [Serializable]
    public class SignilEffect
    {
        public EffectType Type;
        public float Value;
        public string Description;
        public string AbilityName; // Used for SpecialAbility type
    }
    
    [Serializable]
    public class ZodiacCompatibility
    {
        public string ZodiacSign;
        public float CompatibilityFactor = 1.0f; // 1.0 is neutral, >1 is synergy, <1 is conflict
        public string Description;
    }
}