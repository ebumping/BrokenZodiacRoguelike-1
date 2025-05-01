using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Represents the various states of sanity a player can be in
    /// </summary>
    public enum SanityState
    {
        Stable,     // 80-100% sanity: Normal gameplay, minimal effects
        Unsettled,  // 60-80% sanity: Minor visual/audio distortions begin
        Disturbed,  // 40-60% sanity: Moderate hallucinations, enemies may appear different
        Fractured,  // 20-40% sanity: Major reality distortions, some zodiac abilities enhanced
        Shattered,  // 1-20% sanity: Reality breaking down, powerful zodiac abilities unlocked but high risk
        Transcended // 0% sanity or special condition: Complete transformation based on zodiac sign
    }
    
    /// <summary>
    /// Extension methods for SanityState
    /// </summary>
    public static class SanityStateExtensions
    {
        /// <summary>
        /// Returns the minimum sanity percentage for this state
        /// </summary>
        public static float GetMinSanity(this SanityState state)
        {
            switch (state)
            {
                case SanityState.Stable:
                    return 0.8f;
                case SanityState.Unsettled:
                    return 0.6f;
                case SanityState.Disturbed:
                    return 0.4f;
                case SanityState.Fractured:
                    return 0.2f;
                case SanityState.Shattered:
                    return 0.01f;
                case SanityState.Transcended:
                    return 0f;
                default:
                    return 0f;
            }
        }
        
        /// <summary>
        /// Returns the maximum sanity percentage for this state
        /// </summary>
        public static float GetMaxSanity(this SanityState state)
        {
            switch (state)
            {
                case SanityState.Stable:
                    return 1.0f;
                case SanityState.Unsettled:
                    return 0.8f;
                case SanityState.Disturbed:
                    return 0.6f;
                case SanityState.Fractured:
                    return 0.4f;
                case SanityState.Shattered:
                    return 0.2f;
                case SanityState.Transcended:
                    return 0.01f;
                default:
                    return 1.0f;
            }
        }
        
        /// <summary>
        /// Returns the sanity state for a given sanity percentage
        /// </summary>
        public static SanityState GetStateForSanity(float sanityPercentage)
        {
            if (sanityPercentage >= 0.8f)
                return SanityState.Stable;
            else if (sanityPercentage >= 0.6f)
                return SanityState.Unsettled;
            else if (sanityPercentage >= 0.4f)
                return SanityState.Disturbed;
            else if (sanityPercentage >= 0.2f)
                return SanityState.Fractured;
            else if (sanityPercentage >= 0.01f)
                return SanityState.Shattered;
            else
                return SanityState.Transcended;
        }
        
        /// <summary>
        /// Returns a description of the sanity state
        /// </summary>
        public static string GetDescription(this SanityState state)
        {
            switch (state)
            {
                case SanityState.Stable:
                    return "Your mind is clear, and reality appears normal.";
                case SanityState.Unsettled:
                    return "Something feels off. Minor visual distortions and whispers begin to manifest.";
                case SanityState.Disturbed:
                    return "Reality flickers. Hallucinations grow stronger and more frequent.";
                case SanityState.Fractured:
                    return "The boundary between reality and nightmare blurs. The cosmic void seeps into your mind.";
                case SanityState.Shattered:
                    return "Reality breaks down around you. The cosmic truth begins to reveal itself through your fractured psyche.";
                case SanityState.Transcended:
                    return "You have transcended human perception. Your zodiac essence transforms you into something beyond mortality.";
                default:
                    return "Unknown state of mind";
            }
        }
    }
}