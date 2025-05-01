using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// ScriptableObject that defines the properties and abilities of a zodiac sign
    /// </summary>
    [CreateAssetMenu(fileName = "New Zodiac Sign", menuName = "Codex/Zodiac/Sign")]
    public class ZodiacSign : ScriptableObject
    {   
        public enum ZodiacType
        {   
            Aries,      // The Ram
            Taurus,     // The Bull
            Gemini,     // The Twins
            Cancer,     // The Crab
            Leo,        // The Lion
            Virgo,      // The Maiden
            Libra,      // The Scales
            Scorpio,    // The Scorpion
            Sagittarius,// The Archer
            Capricorn,  // Now the Time Manipulator (was Sea-Goat)
            Aquarius,   // Now the Mind Connector (was Water-Bearer)
            Pisces      // The Fish
        }
        
        public enum ZodiacElement
        {   
            Fire,   // Aries, Leo, Sagittarius
            Earth,  // Taurus, Virgo, Capricorn
            Air,    // Gemini, Libra, Aquarius
            Water   // Cancer, Scorpio, Pisces
        }
        
        [Header("Basic Information")]
        [SerializeField] private ZodiacType zodiacType;
        [SerializeField] private ZodiacElement element;
        [SerializeField] private string signName;
        [SerializeField] private string description;
        [SerializeField] private Sprite signSymbol;
        
        [Header("Passive Abilities")]
        [SerializeField] private PassiveAbility[] passiveAbilities;
        
        [Header("Transformation Information")]
        [SerializeField] private string firstTransformationName;
        [SerializeField] private string secondTransformationName;
        [SerializeField] private string finalTransformationName;
        [SerializeField] private Sprite[] transformationSprites;
        
        // Properties
        public ZodiacType Type => zodiacType;
        public ZodiacElement Element => element;
        public string SignName => signName;
        public string Description => description;
        public Sprite SignSymbol => signSymbol;
        public PassiveAbility[] PassiveAbilities => passiveAbilities;
        public string FirstTransformationName => firstTransformationName;
        public string SecondTransformationName => secondTransformationName;
        public string FinalTransformationName => finalTransformationName;
        public Sprite[] TransformationSprites => transformationSprites;
        
        /// <summary>
        /// Get the transformation name for a specific stage
        /// </summary>
        public string GetTransformationName(int stage)
        {   
            switch(stage)
            {   
                case 1: return firstTransformationName;
                case 2: return secondTransformationName;
                case 3: return finalTransformationName;
                default: return signName;
            }
        }
        
        /// <summary>
        /// Get the transformation sprite for a specific stage
        /// </summary>
        public Sprite GetTransformationSprite(int stage)
        {   
            if (transformationSprites == null || stage <= 0 || stage > transformationSprites.Length)
            {   
                return signSymbol;
            }
            
            return transformationSprites[stage - 1];
        }
    }
    
    /// <summary>
    /// Defines a passive ability granted by a zodiac sign
    /// </summary>
    [System.Serializable]
    public class PassiveAbility
    {   
        public string abilityName;
        public string description;
        public Sprite icon;
        
        // Effect definitions would go here
        // These could be event modifiers, stat bonuses, etc.
        public float statModifierValue;
        public StatType statToModify;
        
        public enum StatType
        {   
            Health,
            Damage,
            Speed,
            CriticalChance,
            DamageReduction,
            SanityResistance,
            ManaEfficiency
        }
    }
}