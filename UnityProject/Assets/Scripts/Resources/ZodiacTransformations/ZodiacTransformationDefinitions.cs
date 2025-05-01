using UnityEngine;
using CodexOfTheBrokenZodiac.Core;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources
{
    // This class defines all zodiac-specific transformations that occur at low sanity
    [CreateAssetMenu(fileName = "ZodiacTransformations", menuName = "Codex/Resources/Zodiac Transformations")]
    public class ZodiacTransformationDefinitions : ScriptableObject
    {
        [Header("Zodiac Transformations")]
        [SerializeField] private ZodiacTransformationData[] transformationDefinitions;
        
        // Get a specific transformation by zodiac sign
        public ZodiacTransformationData GetTransformationForSign(ZodiacSign sign)
        {
            if (transformationDefinitions == null)
                return null;
                
            foreach (var transformation in transformationDefinitions)
            {
                if (transformation.ZodiacSign == sign)
                    return transformation;
            }
            
            return null;
        }
        
        // Create a default set of zodiac transformations
        public static ZodiacTransformationDefinitions CreateDefaultTransformations()
        {
            ZodiacTransformationDefinitions definitions = CreateInstance<ZodiacTransformationDefinitions>();
            
            List<ZodiacTransformationData> transformations = new List<ZodiacTransformationData>();
            
            // Add all zodiac transformations
            transformations.Add(CreateAriesTransformation());
            transformations.Add(CreateTaurusTransformation());
            transformations.Add(CreateGeminiTransformation());
            transformations.Add(CreateCancerTransformation());
            transformations.Add(CreateLeoTransformation());
            transformations.Add(CreateVirgoTransformation());
            transformations.Add(CreateLibraTransformation());
            transformations.Add(CreateScorpioTransformation());
            transformations.Add(CreateSagittariusTransformation());
            transformations.Add(CreateCapricornTransformation());
            transformations.Add(CreateAquariusTransformation());
            transformations.Add(CreatePiscesTransformation());
            
            definitions.transformationDefinitions = transformations.ToArray();
            return definitions;
        }
        
        #region Zodiac Transformation Definitions
        
        // Aries Transformation: The Flame Ram
        private static ZodiacTransformationData CreateAriesTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Aries,
                TransformationName = "The Flame Ram",
                TransformationDescription = "Your body ignites with celestial fire, granting incredible speed and aggression. Charging into enemies creates explosive impacts.",
                TransformationDuration = 45f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.5f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Damage, Value = 1.4f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Defense, Value = 0.7f, IsMultiplier = true } // More vulnerable
                },
                
                HasAura = true,
                AuraColor = new Color(1f, 0.3f, 0f, 0.4f), // Fiery orange
                AuraSize = 2f,
                
                HasDamageAura = true,
                AuraRange = 2f,
                AuraDamagePerSecond = 15f,
                AuraDamageType = DamageType.Fire,
                
                // Special abilities
                CanFly = false,
                CanPhaseWalls = false,
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = false,
                ImmuneSanityDrain = false
            };
        }
        
        // Taurus Transformation: The Stone Bull
        private static ZodiacTransformationData CreateTaurusTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Taurus,
                TransformationName = "The Stone Bull",
                TransformationDescription = "Your skin hardens to stone, massively increasing your defense but reducing mobility. Your footsteps cause tremors that stagger enemies.",
                TransformationDuration = 60f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.MoveSpeed, Value = 0.7f, IsMultiplier = true }, // Slower
                    new StatModifier { Type = StatType.Defense, Value = 3.0f, IsMultiplier = true }, // Much tougher
                    new StatModifier { Type = StatType.MaxHealth, Value = 1.5f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Damage, Value = 1.2f, IsMultiplier = true }
                },
                
                HasAura = false,
                
                // No damage aura but has tremor effect
                HasDamageAura = false,
                
                // Special abilities
                CanFly = false,
                CanPhaseWalls = false,
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = true, // Immune to status effects while transformed
                ImmuneSanityDrain = false
            };
        }
        
        // Gemini Transformation: The Phantom Twin
        private static ZodiacTransformationData CreateGeminiTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Gemini,
                TransformationName = "The Phantom Twin",
                TransformationDescription = "Your form splits into a spectral dual-state, allowing you to phase through objects. Mirror images of you appear and mimic your actions, confusing enemies.",
                TransformationDuration = 40f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.3f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Defense, Value = 0.8f, IsMultiplier = true }, // Less defense
                    new StatModifier { Type = StatType.Damage, Value = 1.1f, IsMultiplier = true },
                    new StatModifier { Type = StatType.CooldownReduction, Value = 0.7f, IsMultiplier = true } // Faster cooldowns
                },
                
                HasAura = true,
                AuraColor = new Color(0.5f, 0.5f, 1f, 0.3f), // Ghostly blue
                AuraSize = 1.5f,
                
                HasDamageAura = false,
                
                // Special abilities
                CanFly = false,
                CanPhaseWalls = true, // Can phase through walls
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = false,
                ImmuneSanityDrain = false,
                
                // Creates mirror images
                SpawnsMinions = true,
                MaxMinions = 2
            };
        }
        
        // Cancer Transformation: The Abyssal Crab
        private static ZodiacTransformationData CreateCancerTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Cancer,
                TransformationName = "The Abyssal Crab",
                TransformationDescription = "Your body develops a hardened carapace of cosmic chitin that regenerates when damaged. You can burrow through terrain and emerge elsewhere.",
                TransformationDuration = 50f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.Defense, Value = 2.0f, IsMultiplier = true },
                    new StatModifier { Type = StatType.MoveSpeed, Value = 0.8f, IsMultiplier = true }, // Slower
                    new StatModifier { Type = StatType.MaxHealth, Value = 1.3f, IsMultiplier = true }
                },
                
                HasAura = false,
                
                HasDamageAura = false,
                
                // Special abilities
                CanFly = false,
                CanPhaseWalls = true, // Can burrow through walls
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = false,
                ImmuneSanityDrain = true // Immune to sanity drain
            };
        }
        
        // Leo Transformation: The Solar Lion
        private static ZodiacTransformationData CreateLeoTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Leo,
                TransformationName = "The Solar Lion",
                TransformationDescription = "Your body radiates intense light that burns enemies and reveals secrets. Your roar can stun enemies and shatter objects.",
                TransformationDuration = 35f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.Damage, Value = 1.5f, IsMultiplier = true },
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.2f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Defense, Value = 1.1f, IsMultiplier = true }
                },
                
                HasAura = true,
                AuraColor = new Color(1f, 0.9f, 0.2f, 0.4f), // Solar gold
                AuraSize = 3f,
                
                HasDamageAura = true,
                AuraRange = 3f,
                AuraDamagePerSecond = 10f,
                AuraDamageType = DamageType.Fire,
                
                // Special abilities
                CanFly = false,
                CanPhaseWalls = false,
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = false,
                ImmuneSanityDrain = false
            };
        }
        
        // Virgo Transformation: The Cosmic Maiden
        private static ZodiacTransformationData CreateVirgoTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Virgo,
                TransformationName = "The Cosmic Maiden",
                TransformationDescription = "Your form becomes pure and celestial, creating healing energy. Projectiles split into multiple fragments and home in on targets.",
                TransformationDuration = 40f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.1f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Defense, Value = 1.2f, IsMultiplier = true },
                    new StatModifier { Type = StatType.MaxMana, Value = 2.0f, IsMultiplier = true }, // Double mana
                    new StatModifier { Type = StatType.CooldownReduction, Value = 0.6f, IsMultiplier = true } // Fast cooldowns
                },
                
                HasAura = true,
                AuraColor = new Color(0.8f, 1f, 0.8f, 0.4f), // Purifying green
                AuraSize = 2.5f,
                
                HasDamageAura = false, // Healing aura instead (applied in custom behavior)
                
                // Special abilities
                CanFly = false,
                CanPhaseWalls = false,
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = true,
                ImmuneSanityDrain = true
            };
        }
        
        // Libra Transformation: The Void Balance
        private static ZodiacTransformationData CreateLibraTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Libra,
                TransformationName = "The Void Balance",
                TransformationDescription = "Your form splits between light and dark, creating a perfect balance of cosmic forces. Enemies that deal damage to you receive equal damage in return.",
                TransformationDuration = 45f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.Defense, Value = 1.5f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Damage, Value = 1.3f, IsMultiplier = true },
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.0f, IsMultiplier = true } // Unchanged
                },
                
                HasAura = true,
                AuraColor = new Color(0.5f, 0.5f, 0.5f, 0.5f), // Balanced gray
                AuraSize = 2f,
                
                HasDamageAura = false, // Reflect damage instead
                
                // Special abilities
                CanFly = false,
                CanPhaseWalls = false,
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = false,
                ImmuneSanityDrain = false
            };
        }
        
        // Scorpio Transformation: The Void Scorpion
        private static ZodiacTransformationData CreateScorpioTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Scorpio,
                TransformationName = "The Void Scorpion",
                TransformationDescription = "Your body morphs into a chitinous form with a venomous sting. Attacks inflict cosmic poison that stacks and spreads between enemies.",
                TransformationDuration = 40f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.Damage, Value = 1.3f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Defense, Value = 1.4f, IsMultiplier = true },
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.1f, IsMultiplier = true }
                },
                
                HasAura = true,
                AuraColor = new Color(0.5f, 0f, 0.5f, 0.4f), // Venomous purple
                AuraSize = 1.5f,
                
                HasDamageAura = true,
                AuraRange = 2f,
                AuraDamagePerSecond = 8f, // Lower damage but applies poison
                AuraDamageType = DamageType.Void,
                
                // Special abilities
                CanFly = false,
                CanPhaseWalls = false,
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = true, // Immune to status effects
                ImmuneSanityDrain = false
            };
        }
        
        // Sagittarius Transformation: The Cosmic Archer
        private static ZodiacTransformationData CreateSagittariusTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Sagittarius,
                TransformationName = "The Cosmic Archer",
                TransformationDescription = "Your body becomes swift and ethereal, allowing you to dash through the air. Your attacks become piercing cosmic arrows that chain to multiple targets.",
                TransformationDuration = 30f, // Shorter duration
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.6f, IsMultiplier = true }, // Very fast
                    new StatModifier { Type = StatType.Damage, Value = 1.4f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Defense, Value = 0.7f, IsMultiplier = true }, // Less defense
                    new StatModifier { Type = StatType.CooldownReduction, Value = 0.5f, IsMultiplier = true } // Quick cooldowns
                },
                
                HasAura = true,
                AuraColor = new Color(0.2f, 0.4f, 1f, 0.3f), // Sky blue
                AuraSize = 1.5f,
                
                HasDamageAura = false,
                
                // Special abilities
                CanFly = true, // Can fly/float
                CanPhaseWalls = false,
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = false,
                ImmuneSanityDrain = false
            };
        }
        
        // Capricorn Transformation: The Cosmic Goat
        private static ZodiacTransformationData CreateCapricornTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Capricorn,
                TransformationName = "The Cosmic Goat",
                TransformationDescription = "Your form becomes reminiscent of a mountain goat with the power to scale any surface. Your attacks push enemies back and create fissures in the ground.",
                TransformationDuration = 55f, // Long duration
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.Defense, Value = 1.7f, IsMultiplier = true }, // Very tough
                    new StatModifier { Type = StatType.Damage, Value = 1.2f, IsMultiplier = true },
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.3f, IsMultiplier = true },
                    new StatModifier { Type = StatType.MaxHealth, Value = 1.3f, IsMultiplier = true }
                },
                
                HasAura = false,
                
                HasDamageAura = false,
                
                // Special abilities
                CanFly = false, // Not flying but can climb
                CanPhaseWalls = false,
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = false,
                ImmuneSanityDrain = false
            };
        }
        
        // Aquarius Transformation: The Void Current
        private static ZodiacTransformationData CreateAquariusTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Aquarius,
                TransformationName = "The Void Current",
                TransformationDescription = "Your body becomes a fluid current of cosmic energy that can flow through small spaces. You create puddles of slowing cosmic energy where you move.",
                TransformationDuration = 40f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.4f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Damage, Value = 1.2f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Defense, Value = 0.8f, IsMultiplier = true }, // Less defense
                    new StatModifier { Type = StatType.MaxMana, Value = 1.5f, IsMultiplier = true }
                },
                
                HasAura = true,
                AuraColor = new Color(0f, 0.7f, 1f, 0.4f), // Watery blue
                AuraSize = 2f,
                
                HasDamageAura = false, // Creates slowing pools instead
                
                // Special abilities
                CanFly = false,
                CanPhaseWalls = true, // Can phase through small gaps
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = false,
                ImmuneSanityDrain = false
            };
        }
        
        // Pisces Transformation: The Abyssal Fish
        private static ZodiacTransformationData CreatePiscesTransformation()
        {
            return new ZodiacTransformationData
            {
                ZodiacSign = ZodiacSign.Pisces,
                TransformationName = "The Abyssal Fish",
                TransformationDescription = "Your form becomes fluid and ethereal, allowing you to swim through the air. You can see through illusions and walls, and leave a trail of cosmic water that damages enemies.",
                TransformationDuration = 45f,
                
                StatModifiers = new StatModifier[]
                {
                    new StatModifier { Type = StatType.MoveSpeed, Value = 1.5f, IsMultiplier = true },
                    new StatModifier { Type = StatType.Defense, Value = 1.0f, IsMultiplier = true }, // Unchanged defense
                    new StatModifier { Type = StatType.Damage, Value = 1.2f, IsMultiplier = true },
                    new StatModifier { Type = StatType.MaxMana, Value = 2.0f, IsMultiplier = true } // Lots of mana
                },
                
                HasAura = true,
                AuraColor = new Color(0f, 0.5f, 0.7f, 0.3f), // Ocean blue
                AuraSize = 2f,
                
                HasDamageAura = true,
                AuraRange = 2.5f,
                AuraDamagePerSecond = 5f,
                AuraDamageType = DamageType.Water,
                
                // Special abilities
                CanFly = true, // Swimming through air
                CanPhaseWalls = false,
                GrantsDamageImmunity = false,
                ImmuneToStatusEffects = false,
                ImmuneSanityDrain = true // Immune to sanity drain
            };
        }
        
        #endregion
    }
}