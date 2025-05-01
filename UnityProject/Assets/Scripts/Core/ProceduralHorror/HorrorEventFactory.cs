using System;
using System.Collections.Generic;
using UnityEngine;

namespace CosmicHorror.ProceduralHorror
{
    /// <summary>
    /// Factory class to create and register horror events
    /// </summary>
    public class HorrorEventFactory : MonoBehaviour
    {
        // Singleton instance
        private static HorrorEventFactory _instance;
        public static HorrorEventFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject factoryObject = new GameObject("HorrorEventFactory");
                    _instance = factoryObject.AddComponent<HorrorEventFactory>();
                    DontDestroyOnLoad(factoryObject);
                }
                return _instance;
            }
        }
        
        // Event registry
        private Dictionary<string, Type> registeredEventTypes = new Dictionary<string, Type>();
        
        // Event icons
        private Dictionary<string, Sprite> eventIcons = new Dictionary<string, Sprite>();
        
        // Event category mappings
        private Dictionary<string, HorrorEventType> eventCategories = new Dictionary<string, HorrorEventType>();
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Register all horror events
            RegisterDefaultEvents();
            LoadEventIcons();
        }
        
        /// <summary>
        /// Register all default horror events
        /// </summary>
        private void RegisterDefaultEvents()
        {
            // Environmental events
            RegisterEventType<FlickeringLightsEvent>(HorrorEventType.Environmental);
            RegisterEventType<StrangeSoundsEvent>(HorrorEventType.Environmental);
            RegisterEventType<RoomDistortionEvent>(HorrorEventType.Environmental);
            RegisterEventType<BloodPoolsEvent>(HorrorEventType.Environmental);
            RegisterEventType<FogManifestationEvent>(HorrorEventType.Environmental);
            RegisterEventType<WhisperingWallsEvent>(HorrorEventType.Environmental);
            
            // Enemy events
            RegisterEventType<ShadowCreatureEvent>(HorrorEventType.Enemy);
            RegisterEventType<EnemyDuplicationEvent>(HorrorEventType.Enemy);
            RegisterEventType<EnemyTransfigurationEvent>(HorrorEventType.Enemy);
            RegisterEventType<PhantomEnemyEvent>(HorrorEventType.Enemy);
            RegisterEventType<EntityMergeEvent>(HorrorEventType.Enemy);
            
            // Hallucination events
            RegisterEventType<FalseEnemyEvent>(HorrorEventType.Hallucination);
            RegisterEventType<FalseItemEvent>(HorrorEventType.Hallucination);
            RegisterEventType<PhantomDamageEvent>(HorrorEventType.Hallucination);
            RegisterEventType<FalseExitEvent>(HorrorEventType.Hallucination);
            RegisterEventType<MirrorPlayerEvent>(HorrorEventType.Hallucination);
            
            // Zodiac-specific events
            RegisterEventType<AriesBurningVisionsEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<TaurusEarthquakeEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<GeminiDoppelgangerEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<CancerShellHorrorEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<LeoShadowLionEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<VirgoCorruptionEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<LibraBalanceShiftEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<ScorpioVenomVisionEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<SagittariusPhantomArrowsEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<CapricornTerrainWarpEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<AquariusDrowningEvent>(HorrorEventType.ZodiacSpecific);
            RegisterEventType<PiscesAbyssalVisionEvent>(HorrorEventType.ZodiacSpecific);
        }
        
        /// <summary>
        /// Register a horror event type
        /// </summary>
        public void RegisterEventType<T>(HorrorEventType category) where T : HorrorEventBase
        {
            string typeName = typeof(T).Name;
            
            // Check if already registered
            if (registeredEventTypes.ContainsKey(typeName))
            {
                Debug.LogWarning($"Horror event type {typeName} is already registered!");
                return;
            }
            
            // Register the event type
            registeredEventTypes.Add(typeName, typeof(T));
            eventCategories.Add(typeName, category);
            
            Debug.Log($"Registered horror event: {typeName} in category {category}");
        }
        
        /// <summary>
        /// Load icons for all registered events
        /// </summary>
        private void LoadEventIcons()
        {
            foreach (string eventName in registeredEventTypes.Keys)
            {
                // Try to load icon from resources
                Sprite icon = Resources.Load<Sprite>($"UI/EventIcons/{eventName}Icon");
                
                if (icon != null)
                {
                    eventIcons.Add(eventName, icon);
                }
            }
        }
        
        /// <summary>
        /// Create an instance of a horror event
        /// </summary>
        public HorrorEventBase CreateEvent(string eventTypeName)
        {
            // Check if type is registered
            if (!registeredEventTypes.TryGetValue(eventTypeName, out Type eventType))
            {
                Debug.LogError($"Horror event type {eventTypeName} is not registered!");
                return null;
            }
            
            // Create instance
            try
            {
                HorrorEventBase eventInstance = (HorrorEventBase)Activator.CreateInstance(eventType);
                return eventInstance;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create horror event of type {eventTypeName}: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get the icon for a horror event
        /// </summary>
        public Sprite GetEventIcon(string eventTypeName)
        {
            if (eventIcons.TryGetValue(eventTypeName, out Sprite icon))
            {
                return icon;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get all registered event types
        /// </summary>
        public IEnumerable<string> GetAllEventTypeNames()
        {
            return registeredEventTypes.Keys;
        }
        
        /// <summary>
        /// Get all event types in a specific category
        /// </summary>
        public List<string> GetEventTypeNamesByCategory(HorrorEventType category)
        {
            List<string> result = new List<string>();
            
            foreach (var kvp in eventCategories)
            {
                if (kvp.Value == category)
                {
                    result.Add(kvp.Key);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get the category for a horror event type
        /// </summary>
        public HorrorEventType GetEventCategory(string eventTypeName)
        {
            if (eventCategories.TryGetValue(eventTypeName, out HorrorEventType category))
            {
                return category;
            }
            
            return HorrorEventType.Environmental; // Default
        }
    }
}