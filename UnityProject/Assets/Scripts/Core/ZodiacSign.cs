using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Enum defining the 12 zodiac signs used in the game for player traits and transformations
    /// </summary>
    public enum ZodiacSign
    {
        Aries,      // Fire sign - The Ram
        Taurus,     // Earth sign - The Bull
        Gemini,     // Air sign - The Twins
        Cancer,     // Water sign - The Crab
        Leo,        // Fire sign - The Lion
        Virgo,      // Earth sign - The Virgin
        Libra,      // Air sign - The Scales
        Scorpio,    // Water sign - The Scorpion
        Sagittarius, // Fire sign - The Archer
        Capricorn,  // Earth sign - The Goat
        Aquarius,   // Air sign - The Water Bearer
        Pisces      // Water sign - The Fish
    }
    
    /// <summary>
    /// Extension methods for the ZodiacSign enum
    /// </summary>
    public static class ZodiacSignExtensions
    {
        /// <summary>
        /// Returns the element associated with the zodiac sign
        /// </summary>
        public static Element GetElement(this ZodiacSign sign)
        {
            switch (sign)
            {
                case ZodiacSign.Aries:
                case ZodiacSign.Leo:
                case ZodiacSign.Sagittarius:
                    return Element.Fire;
                
                case ZodiacSign.Taurus:
                case ZodiacSign.Virgo:
                case ZodiacSign.Capricorn:
                    return Element.Earth;
                
                case ZodiacSign.Gemini:
                case ZodiacSign.Libra:
                case ZodiacSign.Aquarius:
                    return Element.Air;
                
                case ZodiacSign.Cancer:
                case ZodiacSign.Scorpio:
                case ZodiacSign.Pisces:
                    return Element.Water;
                
                default:
                    Debug.LogError($"Unknown zodiac sign: {sign}");
                    return Element.None;
            }
        }
        
        /// <summary>
        /// Returns the modality associated with the zodiac sign
        /// </summary>
        public static Modality GetModality(this ZodiacSign sign)
        {
            switch (sign)
            {
                case ZodiacSign.Aries:
                case ZodiacSign.Cancer:
                case ZodiacSign.Libra:
                case ZodiacSign.Capricorn:
                    return Modality.Cardinal;
                
                case ZodiacSign.Taurus:
                case ZodiacSign.Leo:
                case ZodiacSign.Scorpio:
                case ZodiacSign.Aquarius:
                    return Modality.Fixed;
                
                case ZodiacSign.Gemini:
                case ZodiacSign.Virgo:
                case ZodiacSign.Sagittarius:
                case ZodiacSign.Pisces:
                    return Modality.Mutable;
                
                default:
                    Debug.LogError($"Unknown zodiac sign: {sign}");
                    return Modality.None;
            }
        }
        
        /// <summary>
        /// Returns the name of the zodiac sign as a string
        /// </summary>
        public static string GetDisplayName(this ZodiacSign sign)
        {
            return sign.ToString();
        }
        
        /// <summary>
        /// Returns a description of the zodiac sign's traits
        /// </summary>
        public static string GetDescription(this ZodiacSign sign)
        {
            switch (sign)
            {
                case ZodiacSign.Aries:
                    return "The Ram - Bold and ambitious, Aries rushes in headfirst with raw courage and transformation through flame.";
                case ZodiacSign.Taurus:
                    return "The Bull - Persistent and grounded, Taurus harnesses stability and endurance with earth-based transformations.";
                case ZodiacSign.Gemini:
                    return "The Twins - Quick-witted and versatile, Gemini utilizes duality and air manipulation in their transformations.";
                case ZodiacSign.Cancer:
                    return "The Crab - Intuitive and protective, Cancer channels deep emotions and water-based restorative abilities.";
                case ZodiacSign.Leo:
                    return "The Lion - Charismatic and proud, Leo's transformations enhance leadership and manifest through radiant fire energy.";
                case ZodiacSign.Virgo:
                    return "The Virgin - Analytical and precise, Virgo's transformations focus on purification and earth-based healing.";
                case ZodiacSign.Libra:
                    return "The Scales - Balanced and diplomatic, Libra manipulates opposing forces through air-based transformations.";
                case ZodiacSign.Scorpio:
                    return "The Scorpion - Intense and transformative, Scorpio channels death and rebirth through water-based mutations.";
                case ZodiacSign.Sagittarius:
                    return "The Archer - Adventurous and philosophical, Sagittarius harnesses distant fire and cosmic knowledge.";
                case ZodiacSign.Capricorn:
                    return "The Goat - Disciplined and ambitious, Capricorn's earth-based transformations focus on endurance and amplification.";
                case ZodiacSign.Aquarius:
                    return "The Water Bearer - Innovative and humanitarian, Aquarius channels cosmic air energy for revolutionary transformations.";
                case ZodiacSign.Pisces:
                    return "The Fish - Compassionate and intuitive, Pisces dissolves boundaries between realities through water transformations.";
                default:
                    return "Unknown zodiac sign";
            }
        }
        
        /// <summary>
        /// Returns the primary sanity transformation associated with this zodiac sign
        /// </summary>
        public static SanityTransformationType GetPrimaryTransformation(this ZodiacSign sign)
        {
            switch (sign)
            {
                case ZodiacSign.Aries:
                    return SanityTransformationType.BurningRage;
                case ZodiacSign.Taurus:
                    return SanityTransformationType.EarthBound;
                case ZodiacSign.Gemini:
                    return SanityTransformationType.DualEntity;
                case ZodiacSign.Cancer:
                    return SanityTransformationType.LunarShift;
                case ZodiacSign.Leo:
                    return SanityTransformationType.SolarFlare;
                case ZodiacSign.Virgo:
                    return SanityTransformationType.PurificationVessel;
                case ZodiacSign.Libra:
                    return SanityTransformationType.CosmicBalance;
                case ZodiacSign.Scorpio:
                    return SanityTransformationType.VenomousEmbrace;
                case ZodiacSign.Sagittarius:
                    return SanityTransformationType.StarChaser;
                case ZodiacSign.Capricorn:
                    return SanityTransformationType.MountainForm;
                case ZodiacSign.Aquarius:
                    return SanityTransformationType.VoidWalker;
                case ZodiacSign.Pisces:
                    return SanityTransformationType.AbyssalMerge;
                default:
                    Debug.LogError($"Unknown zodiac sign: {sign}");
                    return SanityTransformationType.None;
            }
        }
        
        /// <summary>
        /// Returns the secondary sanity transformation associated with this zodiac sign
        /// </summary>
        public static SanityTransformationType GetSecondaryTransformation(this ZodiacSign sign)
        {
            switch (sign)
            {
                case ZodiacSign.Aries:
                    return SanityTransformationType.InfernalVessel;
                case ZodiacSign.Taurus:
                    return SanityTransformationType.StoneFlesh;
                case ZodiacSign.Gemini:
                    return SanityTransformationType.MirrorSelf;
                case ZodiacSign.Cancer:
                    return SanityTransformationType.TidalForm;
                case ZodiacSign.Leo:
                    return SanityTransformationType.CelestialMane;
                case ZodiacSign.Virgo:
                    return SanityTransformationType.PerfectionSeeker;
                case ZodiacSign.Libra:
                    return SanityTransformationType.JusticeIncarnate;
                case ZodiacSign.Scorpio:
                    return SanityTransformationType.DeathRebirth;
                case ZodiacSign.Sagittarius:
                    return SanityTransformationType.CosmicHunter;
                case ZodiacSign.Capricorn:
                    return SanityTransformationType.AbyssalGoat;
                case ZodiacSign.Aquarius:
                    return SanityTransformationType.OtherworldlyVessel;
                case ZodiacSign.Pisces:
                    return SanityTransformationType.CosmicOcean;
                default:
                    Debug.LogError($"Unknown zodiac sign: {sign}");
                    return SanityTransformationType.None;
            }
        }
    }
    
    /// <summary>
    /// Represents the four elements associated with zodiac signs
    /// </summary>
    public enum Element
    {
        None,
        Fire,
        Earth,
        Air,
        Water
    }
    
    /// <summary>
    /// Represents the three modalities associated with zodiac signs
    /// </summary>
    public enum Modality
    {
        None,
        Cardinal, // Initiators, leaders
        Fixed,    // Stabilizers, maintainers
        Mutable   // Adapters, finishers
    }
    
    /// <summary>
    /// Types of sanity transformations that can occur based on zodiac sign
    /// </summary>
    public enum SanityTransformationType
    {
        None,
        
        // Aries Transformations
        BurningRage,       // Primary: Enhances attack power but decreases control
        InfernalVessel,    // Secondary: Absorbs and redirects damage as fire
        
        // Taurus Transformations
        EarthBound,        // Primary: Greatly increases defense but reduces mobility
        StoneFlesh,        // Secondary: Temporarily grants invulnerability with reduced movement
        
        // Gemini Transformations
        DualEntity,        // Primary: Creates a mirror image that mirrors player actions
        MirrorSelf,        // Secondary: Switches places with enemy, transferring status effects
        
        // Cancer Transformations
        LunarShift,        // Primary: Changes abilities based on lunar cycle, healing focus
        TidalForm,         // Secondary: Fluid form grants evasion but vulnerability to certain attacks
        
        // Leo Transformations
        SolarFlare,        // Primary: Radiates damaging aura that increases with sanity loss
        CelestialMane,     // Secondary: Grants temporary invulnerability and increased attack
        
        // Virgo Transformations
        PurificationVessel, // Primary: Cleanses area of corruption, reveals secrets
        PerfectionSeeker,   // Secondary: Enhanced critical hits but vulnerability to chaos
        
        // Libra Transformations
        CosmicBalance,     // Primary: Equalizes stats between player and enemies
        JusticeIncarnate,   // Secondary: Returns damage to attackers
        
        // Scorpio Transformations
        VenomousEmbrace,   // Primary: Poisons attacks and gains health from affected enemies
        DeathRebirth,      // Secondary: Can resurrect once with temporary invulnerability
        
        // Sagittarius Transformations
        StarChaser,        // Primary: Enhanced range and speed
        CosmicHunter,      // Secondary: Marks enemies for increased damage and tracking
        
        // Capricorn Transformations
        MountainForm,      // Primary: Massive defense increase but speed reduction
        AbyssalGoat,       // Secondary: Can traverse any terrain and see hidden paths
        
        // Aquarius Transformations
        VoidWalker,        // Primary: Can phase through objects and enemies briefly
        OtherworldlyVessel, // Secondary: Channels cosmic energy for area attacks
        
        // Pisces Transformations
        AbyssalMerge,      // Primary: Can merge with water/cosmic fluids for stealth
        CosmicOcean        // Secondary: Creates tidal wave of cosmic energy
    }
}