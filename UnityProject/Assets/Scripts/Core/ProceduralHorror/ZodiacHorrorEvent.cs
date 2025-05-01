using UnityEngine;

namespace CosmicHorror.ProceduralHorror
{
    /// <summary>
    /// Base class for zodiac-specific horror events
    /// </summary>
    public abstract class ZodiacHorrorEvent : HorrorEventBase
    {
        // The zodiac sign this event is associated with
        public abstract ZodiacSign ZodiacSign { get; }
        
        // Zodiac intensity multiplier
        protected float zodiacIntensityMultiplier = 1.5f;
        
        /// <summary>
        /// Checks if the event is compatible with the player's current zodiac sign
        /// </summary>
        public bool IsCompatibleWithPlayerZodiac(ZodiacSign playerZodiac)
        {
            return playerZodiac == ZodiacSign;
        }
        
        /// <summary>
        /// Start the horror event with the given parameters
        /// </summary>
        public override void StartEvent(EventParameters parameters)
        {
            // Apply extra intensity if this event matches the player's zodiac sign
            if (parameters.PlayerZodiac == ZodiacSign)
            {
                parameters.Intensity *= zodiacIntensityMultiplier;
            }
            
            base.StartEvent(parameters);
        }
        
        /// <summary>
        /// Calculate the current intensity based on progress and zodiac compatibility
        /// </summary>
        protected override float GetCurrentIntensity(float progress)
        {
            float intensity = base.GetCurrentIntensity(progress);
            
            // Apply extra intensity based on zodiac sign compatibility
            if (playerZodiac == ZodiacSign)
            {
                // More intense for matching zodiac
                return intensity * zodiacIntensityMultiplier;
            }
            
            return intensity;
        }
    }
}