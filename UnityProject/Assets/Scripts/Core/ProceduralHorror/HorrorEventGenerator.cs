using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CosmicHorror.ProceduralHorror
{
    /// <summary>
    /// Types of horror events that can be generated
    /// </summary>
    public enum HorrorEventType
    {
        Environmental,
        Enemy,
        Hallucination,
        ZodiacSpecific
    }
    
    /// <summary>
    /// Parameters for generating and configuring horror events
    /// </summary>
    public struct EventParameters
    {
        public float Intensity;        // Base intensity of the event (0-1)
        public float Duration;         // Duration modifier (1.0 = normal)
        public ZodiacSign PlayerZodiac; // Player's zodiac sign
        public float SanityLevel;      // Current player sanity (0-100)
        public Vector3 PlayerPosition; // Current player position
        public bool IsInCombat;        // Whether player is in combat
        public float DepthLevel;       // Current depth level (affects intensity)
    }
    
    /// <summary>
    /// Manages the procedural generation and execution of horror events based on player state
    /// </summary>
    public class HorrorEventGenerator : MonoBehaviour
    {
        [Header("Event Generation Settings")]
        [SerializeField] private float baseEventFrequency = 30f; // Seconds between events
        [SerializeField] private float minEventFrequency = 10f;  // Minimum seconds between events
        [SerializeField] private float frequencySanityModifier = 0.5f; // How much sanity affects frequency
        [SerializeField] private AnimationCurve frequencySanityCurve;  // Curve mapping sanity to frequency
        
        [Header("Event Selection Weights")]
        [SerializeField] private float environmentalWeight = 1.0f;
        [SerializeField] private float enemyWeight = 1.0f;
        [SerializeField] private float hallucinationWeight = 1.0f;
        [SerializeField] private float zodiacWeight = 1.5f;
        
        [Header("Intensity Settings")]
        [SerializeField] private float baseEventIntensity = 0.5f;
        [SerializeField] private float maxEventIntensity = 1.0f;
        [SerializeField] private float intensitySanityModifier = 0.5f;
        [SerializeField] private AnimationCurve intensitySanityCurve;
        [SerializeField] private float depthIntensityModifier = 0.05f; // Per level
        
        // Event tracking
        private float nextEventTime;
        private List<HorrorEventBase> activeEvents = new List<HorrorEventBase>();
        private HorrorEventFactory eventFactory;
        
        // Player state references
        private SanitySystem playerSanity;
        private ZodiacSign playerZodiac;
        private Transform playerTransform;
        private float currentDepth = 1;
        private bool isPlayerInCombat = false;
        
        // UI Reference
        private UI.HorrorEventUI eventUI;
        
        private void Awake()
        {
            // Get event factory
            eventFactory = HorrorEventFactory.Instance;
            
            // Get UI reference
            eventUI = FindObjectOfType<UI.HorrorEventUI>();
            
            // Initialize curves if not set
            if (frequencySanityCurve == null || frequencySanityCurve.keys.Length == 0)
            {
                frequencySanityCurve = new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(0.5f, 0.5f),
                    new Keyframe(1f, 0.1f)
                );
            }
            
            if (intensitySanityCurve == null || intensitySanityCurve.keys.Length == 0)
            {
                intensitySanityCurve = new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(0.5f, 0.6f),
                    new Keyframe(1f, 0.2f)
                );
            }
        }
        
        private void Start()
        {
            // Find player references
            playerSanity = FindObjectOfType<SanitySystem>();
            playerTransform = GameObject.FindObjectOfType<PlayerController>()?.transform;
            
            // Get player zodiac sign
            playerZodiac = FindObjectOfType<ZodiacSignil>()?.CurrentZodiacSign ?? ZodiacSign.Aries;
            
            // Set initial event time
            ResetEventTimer();
        }
        
        private void Update()
        {
            // Check if it's time for a new event
            if (Time.time >= nextEventTime)
            {
                GenerateHorrorEvent();
                ResetEventTimer();
            }
            
            // Update active events
            UpdateActiveEvents();
            
            // Clean up completed events
            CleanupEvents();
        }
        
        /// <summary>
        /// Generate a new horror event based on current player state
        /// </summary>
        private void GenerateHorrorEvent()
        {
            // Check if we have player data
            if (playerSanity == null || playerTransform == null)
            {
                Debug.LogWarning("HorrorEventGenerator: Missing player references");
                return;
            }
            
            // Get current player state
            float sanity = playerSanity.CurrentSanity;
            Vector3 position = playerTransform.position;
            
            // Calculate event parameters
            EventParameters parameters = new EventParameters
            {
                Intensity = CalculateEventIntensity(sanity),
                Duration = 1.0f, // Normal duration
                PlayerZodiac = playerZodiac,
                SanityLevel = sanity,
                PlayerPosition = position,
                IsInCombat = isPlayerInCombat,
                DepthLevel = currentDepth
            };
            
            // Select event type based on weights
            HorrorEventType eventType = SelectEventType(sanity);
            
            // Get appropriate event for that type
            HorrorEventBase newEvent = CreateEventOfType(eventType, parameters);
            
            if (newEvent != null)
            {
                // Start the event
                newEvent.StartEvent(parameters);
                
                // Add to active events
                activeEvents.Add(newEvent);
                
                // Show UI notification if we have UI
                if (eventUI != null)
                {
                    Sprite eventIcon = eventFactory.GetEventIcon(newEvent.GetType().Name);
                    eventUI.ShowEventNotification(newEvent.EventName, newEvent.EventDescription, eventIcon);
                }
                
                Debug.Log($"Generated new horror event: {newEvent.EventName} with intensity {parameters.Intensity}");
            }
        }
        
        /// <summary>
        /// Create a specific horror event from the factory
        /// </summary>
        private HorrorEventBase CreateEventOfType(HorrorEventType eventType, EventParameters parameters)
        {
            // Get all events of the given type
            List<string> eventNames = eventFactory.GetEventTypeNamesByCategory(eventType);
            
            if (eventNames.Count == 0)
            {
                Debug.LogWarning($"No horror events found for category: {eventType}");
                return null;
            }
            
            // For zodiac-specific events, try to find one matching the player's zodiac
            if (eventType == HorrorEventType.ZodiacSpecific)
            {
                // Find event matching player zodiac
                string matchingEvent = eventNames.Find(name => name.Contains(parameters.PlayerZodiac.ToString()));
                
                if (!string.IsNullOrEmpty(matchingEvent))
                {
                    return eventFactory.CreateEvent(matchingEvent);
                }
                
                // Fall back to random zodiac event if no match found
            }
            
            // Select random event from the list
            string randomEventName = eventNames[Random.Range(0, eventNames.Count)];
            return eventFactory.CreateEvent(randomEventName);
        }
        
        /// <summary>
        /// Select a horror event type based on weights and sanity
        /// </summary>
        private HorrorEventType SelectEventType(float sanity)
        {
            // Normalize sanity to 0-1 range
            float normalizedSanity = sanity / 100f;
            
            // Adjust weights based on sanity
            float envWeight = environmentalWeight;
            float enmWeight = enemyWeight * (1f - normalizedSanity * 0.5f); // More enemy events at lower sanity
            float halWeight = hallucinationWeight * (1f - normalizedSanity); // More hallucinations at lower sanity
            float zodWeight = zodiacWeight * (1f - normalizedSanity * 0.7f); // More zodiac events at lower sanity
            
            // Calculate total weight
            float totalWeight = envWeight + enmWeight + halWeight + zodWeight;
            
            // Generate random value
            float randomValue = Random.Range(0f, totalWeight);
            
            // Select event type based on random value
            if (randomValue < envWeight)
            {
                return HorrorEventType.Environmental;
            }
            else if (randomValue < envWeight + enmWeight)
            {
                return HorrorEventType.Enemy;
            }
            else if (randomValue < envWeight + enmWeight + halWeight)
            {
                return HorrorEventType.Hallucination;
            }
            else
            {
                return HorrorEventType.ZodiacSpecific;
            }
        }
        
        /// <summary>
        /// Calculate the intensity of the next event based on player state
        /// </summary>
        private float CalculateEventIntensity(float sanity)
        {
            // Normalize sanity to 0-1 range
            float normalizedSanity = sanity / 100f;
            
            // Base intensity modified by sanity
            float intensity = baseEventIntensity + 
                (intensitySanityModifier * (1f - intensitySanityCurve.Evaluate(normalizedSanity)));
            
            // Add depth modifier
            intensity += (currentDepth - 1) * depthIntensityModifier;
            
            // Add combat modifier
            if (isPlayerInCombat)
            {
                intensity += 0.1f;
            }
            
            // Clamp to max intensity
            return Mathf.Clamp(intensity, 0f, maxEventIntensity);
        }
        
        /// <summary>
        /// Reset the timer for the next event
        /// </summary>
        private void ResetEventTimer()
        {
            float sanity = playerSanity != null ? playerSanity.CurrentSanity : 100f;
            float normalizedSanity = sanity / 100f;
            
            // Calculate time to next event based on sanity
            float sanityFactor = frequencySanityCurve.Evaluate(normalizedSanity);
            float timeToNextEvent = baseEventFrequency * (1f - (frequencySanityModifier * (1f - sanityFactor)));
            
            // Ensure minimum time between events
            timeToNextEvent = Mathf.Max(timeToNextEvent, minEventFrequency);
            
            // Add small random variation
            timeToNextEvent += Random.Range(-timeToNextEvent * 0.2f, timeToNextEvent * 0.2f);
            
            // Set next event time
            nextEventTime = Time.time + timeToNextEvent;
            
            Debug.Log($"Next horror event in {timeToNextEvent:F1} seconds");
        }
        
        /// <summary>
        /// Update all active events
        /// </summary>
        private void UpdateActiveEvents()
        {
            foreach (HorrorEventBase horrorEvent in activeEvents)
            {
                if (horrorEvent != null && horrorEvent.IsActive)
                {
                    horrorEvent.UpdateEvent();
                }
            }
        }
        
        /// <summary>
        /// Clean up completed events
        /// </summary>
        private void CleanupEvents()
        {
            activeEvents.RemoveAll(e => e == null || !e.IsActive);
        }
        
        /// <summary>
        /// Forcibly end all active events
        /// </summary>
        public void EndAllEvents()
        {
            foreach (HorrorEventBase horrorEvent in activeEvents)
            {
                if (horrorEvent != null && horrorEvent.IsActive)
                {
                    horrorEvent.EndEvent();
                }
            }
            
            activeEvents.Clear();
        }
        
        /// <summary>
        /// Set the player's current depth level
        /// </summary>
        public void SetDepthLevel(float depth)
        {
            currentDepth = depth;
        }
        
        /// <summary>
        /// Update the player's combat state
        /// </summary>
        public void SetPlayerCombatState(bool inCombat)
        {
            isPlayerInCombat = inCombat;
        }
        
        /// <summary>
        /// Trigger a specific horror event immediately
        /// </summary>
        public void TriggerSpecificEvent(string eventName, float intensityOverride = -1f)
        {
            // Create event parameters
            EventParameters parameters = new EventParameters
            {
                Intensity = intensityOverride > 0 ? intensityOverride : CalculateEventIntensity(playerSanity.CurrentSanity),
                Duration = 1.0f,
                PlayerZodiac = playerZodiac,
                SanityLevel = playerSanity != null ? playerSanity.CurrentSanity : 100f,
                PlayerPosition = playerTransform != null ? playerTransform.position : Vector3.zero,
                IsInCombat = isPlayerInCombat,
                DepthLevel = currentDepth
            };
            
            // Create and start event
            HorrorEventBase newEvent = eventFactory.CreateEvent(eventName);
            
            if (newEvent != null)
            {
                newEvent.StartEvent(parameters);
                activeEvents.Add(newEvent);
                
                // Show UI notification
                if (eventUI != null)
                {
                    Sprite eventIcon = eventFactory.GetEventIcon(newEvent.GetType().Name);
                    eventUI.ShowEventNotification(newEvent.EventName, newEvent.EventDescription, eventIcon);
                }
                
                Debug.Log($"Triggered specific horror event: {newEvent.EventName}");
            }
            else
            {
                Debug.LogError($"Failed to create horror event: {eventName}");
            }
        }
        
        /// <summary>
        /// Get a list of currently active horror events
        /// </summary>
        public List<HorrorEventBase> GetActiveEvents()
        {
            return new List<HorrorEventBase>(activeEvents);
        }
    }
}