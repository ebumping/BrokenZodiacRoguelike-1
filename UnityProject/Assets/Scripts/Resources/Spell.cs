using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
    [CreateAssetMenu(fileName = "NewSpell", menuName = "Codex/Spell")]
    public class Spell : ScriptableObject
    {
        [Header("Basic Info")]
        public string SpellName;
        [TextArea(3, 5)]
        public string Description;
        
        [Header("Spell Properties")]
        public SpellType Type;
        public SpellElement Element;
        public float ManaCost = 20f;
        public float Cooldown = 5f;  // In seconds
        public float CastTime = 0.2f; // In seconds, 0 for instant cast
        public bool RequiresTarget = false;
        public float Range = 10f;
        
        [Header("Spell Effects")]
        public float BaseDamage;
        public float BaseHealing;
        public float EffectDuration;
        public float AreaOfEffect;
        
        [Header("Status Effects")]
        public List<StatusEffectData> StatusEffects = new List<StatusEffectData>();
        
        [Header("Visual and Audio")]
        public GameObject SpellPrefab; // The visual prefab to spawn
        public GameObject ImpactEffectPrefab;
        public Sprite SpellIcon;
        public AudioClip CastSound;
        public AudioClip ImpactSound;
        
        [Header("Upgrades")]
        public bool CanBeUpgraded = true;
        public int MaxLevel = 3;
        public float DamageIncreasePerLevel = 5f;
        public float HealingIncreasePerLevel = 5f;
        public float ManaCostReductionPerLevel = 2f;
        
        [Header("Tags and Requirements")]
        public List<string> SpellTags = new List<string>();
        public string UnlockRequirement;
        public bool IsUnlocked = false;
        
        // Calculated properties
        public float GetDamage(int level = 1, float spellPower = 1f)
        {
            float levelBonus = (level - 1) * DamageIncreasePerLevel;
            return (BaseDamage + levelBonus) * spellPower;
        }
        
        public float GetHealing(int level = 1, float healingPower = 1f)
        {
            float levelBonus = (level - 1) * HealingIncreasePerLevel;
            return (BaseHealing + levelBonus) * healingPower;
        }
        
        public float GetManaCost(int level = 1, float manaCostModifier = 1f)
        {
            float levelReduction = (level - 1) * ManaCostReductionPerLevel;
            return (ManaCost - levelReduction) * manaCostModifier;
        }
        
        // Check if spell can be cast
        public bool CanCast(float currentMana, float cooldownRemaining)
        {
            return currentMana >= ManaCost && cooldownRemaining <= 0f;
        }
    }
    
    // Spell classification enums
    [Serializable]
    public enum SpellType
    {
        Projectile,   // Single projectile (fireball, magic missile)
        Beam,         // Continuous beam (lightning)
        Area,         // Area effect (explosion, frost nova)
        Buff,         // Positive effect on self/allies
        Debuff,       // Negative effect on enemies
        Summoning,    // Summon allies/creatures
        Utility,      // Non-combat effects (teleport, invisibility)
        Channel,      // Channeled spell that continues while button held
        Trap,         // Placed trap that triggers later
        Shield,       // Defensive barrier
        Chain,        // Effect that chains between targets
        Transformation // Transform player or targets
    }
    
    [Serializable]
    public enum SpellElement
    {
        Arcane,
        Fire,
        Ice,
        Lightning,
        Earth,
        Water,
        Wind,
        Light,
        Shadow,
        Chaos,
        Void,
        Blood,
        Mind,
        Time,
        Cosmic
    }
    
    // Status effect data
    [Serializable]
    public class StatusEffectData
    {
        public StatusEffectType Type;
        public float Power; // Magnitude of the effect
        public float Duration; // How long it lasts in seconds
        public float TickRate = 1f; // For damage over time effects
        
        [Header("Visual")]
        public Color EffectColor = Color.white;
        public GameObject EffectPrefab; // Visual indicator
    }
    
    [Serializable]
    public enum StatusEffectType
    {
        Burn,        // Damage over time
        Freeze,       // Slows and damages
        Shock,        // Damage and chance to stun
        Poison,       // Damage over time
        Bleed,        // Physical damage over time
        Stun,         // Cannot act
        Slow,         // Reduced movement
        Root,         // Cannot move
        Silence,      // Cannot cast spells
        Weakness,     // Reduced damage
        Amplify,      // Increased damage taken
        Haste,        // Increased attack/cast speed
        Shield,       // Absorbs damage
        Regeneration, // Health over time
        ManaFlow,     // Mana regeneration
        Fear,         // Run away
        Charm,        // Temporarily ally
        Reflect,      // Reflect projectiles
        Confusion,    // Random movement/targeting
        Invulnerable  // Cannot take damage
    }
}