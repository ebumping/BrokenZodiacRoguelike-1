using UnityEngine;

namespace CosmicHorror.ProceduralHorror
{
    /// <summary>
    /// Base class for all horror events in the game
    /// </summary>
    public abstract class HorrorEventBase
    {
        // Event properties (must be implemented by derived classes)
        public abstract string EventName { get; }
        public abstract string EventDescription { get; }
        public abstract float BaseDuration { get; }
        public abstract float BaseIntensity { get; }
        
        // Event state
        protected float eventDuration;
        protected float eventIntensity;
        protected float eventStartTime;
        protected float eventProgress = 0f;
        protected bool isActive = false;
        protected EventParameters eventParameters;
        protected ZodiacSign playerZodiac;
        
        // Event state properties
        public bool IsActive => isActive;
        public float Progress => eventProgress;
        public float RemainingTime => Mathf.Max(0f, (eventStartTime + eventDuration) - Time.time);
        public float Intensity => eventIntensity;
        
        /// <summary>
        /// Start the horror event with the given parameters
        /// </summary>
        public virtual void StartEvent(EventParameters parameters)
        {
            if (isActive)
            {
                Debug.LogWarning($"Horror event {EventName} is already active");
                return;
            }
            
            // Store parameters
            eventParameters = parameters;
            playerZodiac = parameters.PlayerZodiac;
            
            // Set event properties
            eventDuration = BaseDuration * parameters.Duration;
            eventIntensity = Mathf.Clamp(BaseIntensity * parameters.Intensity, 0f, 1f);
            eventStartTime = Time.time;
            eventProgress = 0f;
            isActive = true;
            
            // Call derived class implementation
            OnEventStart();
            
            Debug.Log($"Horror event {EventName} started with intensity {eventIntensity:F2} and duration {eventDuration:F1}s");
        }
        
        /// <summary>
        /// Update the horror event
        /// </summary>
        public virtual void UpdateEvent()
        {
            if (!isActive) return;
            
            // Calculate progress (0 to 1)
            float elapsedTime = Time.time - eventStartTime;
            eventProgress = Mathf.Clamp01(elapsedTime / eventDuration);
            
            // Check if event has ended
            if (eventProgress >= 1f)
            {
                EndEvent();
                return;
            }
            
            // Call derived class implementation
            OnEventUpdate(eventProgress);
        }
        
        /// <summary>
        /// End the horror event
        /// </summary>
        public virtual void EndEvent()
        {
            if (!isActive) return;
            
            // Call derived class implementation
            OnEventEnd();
            
            // Set inactive
            isActive = false;
            
            Debug.Log($"Horror event {EventName} ended");
        }
        
        /// <summary>
        /// Force the event to end immediately
        /// </summary>
        public virtual void ForceEnd()
        {
            if (!isActive) return;
            
            // Skip to the end
            eventProgress = 1f;
            EndEvent();
        }
        
        /// <summary>
        /// Calculate the current intensity based on progress
        /// </summary>
        protected virtual float GetCurrentIntensity(float progress)
        {
            // Default implementation: Fade in, hold, fade out
            float fadeInEnd = 0.2f;
            float fadeOutStart = 0.8f;
            
            if (progress < fadeInEnd)
            {
                // Fade in
                return Mathf.Lerp(0f, eventIntensity, progress / fadeInEnd);
            }
            else if (progress > fadeOutStart)
            {
                // Fade out
                return Mathf.Lerp(eventIntensity, 0f, (progress - fadeOutStart) / (1f - fadeOutStart));
            }
            
            // Hold
            return eventIntensity;
        }
        
        /// <summary>
        /// Called when the event starts
        /// </summary>
        protected abstract void OnEventStart();
        
        /// <summary>
        /// Called when the event updates
        /// </summary>
        protected abstract void OnEventUpdate(float progress);
        
        /// <summary>
        /// Called when the event ends
        /// </summary>
        protected abstract void OnEventEnd();
    }
    
    /// <summary>
    /// Zodiac sign enumeration for the game
    /// </summary>
    public enum ZodiacSign
    {
        Aries,
        Taurus,
        Gemini,
        Cancer,
        Leo,
        Virgo,
        Libra,
        Scorpio,
        Sagittarius,
        Capricorn,
        Aquarius,
        Pisces
    }
}