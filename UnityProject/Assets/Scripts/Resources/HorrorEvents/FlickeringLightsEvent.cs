using UnityEngine;
using System.Collections.Generic;
using CosmicHorror.ProceduralHorror;

namespace CosmicHorror.HorrorEvents
{
    /// <summary>
    /// Horror event that causes lights in the level to flicker in an unsettling pattern
    /// </summary>
    public class FlickeringLightsEvent : HorrorEventBase
    {
        // Event properties
        public override string EventName => "Flickering Lights";
        public override string EventDescription => "The lights around you begin to flicker erratically, casting strange shadows.";
        public override float BaseDuration => 20f;
        public override float BaseIntensity => 0.7f;
        
        // Light control
        private List<Light> affectedLights = new List<Light>();
        private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
        
        // Flickering parameters
        private float flickerSpeed = 10f;
        private float minFlickerMultiplier = 0.1f;
        private float maxFlickerMultiplier = 1.2f;
        private float radius = 30f;
        
        protected override void OnEventStart()
        {
            // Find lights in the scene within radius
            Light[] sceneLights = GameObject.FindObjectsOfType<Light>();
            
            // Get player position
            Transform playerTransform = GameObject.FindObjectOfType<PlayerController>()?.transform;
            Vector3 centerPosition = playerTransform != null ? playerTransform.position : Vector3.zero;
            
            foreach (Light light in sceneLights)
            {
                // Only affect lights within radius of player
                if (Vector3.Distance(light.transform.position, centerPosition) <= radius)
                {
                    affectedLights.Add(light);
                    originalIntensities.Add(light, light.intensity);
                }
            }
            
            // Adjust flickering parameters based on intensity
            flickerSpeed = 8f + (eventIntensity * 7f); // 8-15 based on intensity
            minFlickerMultiplier = 0.3f - (eventIntensity * 0.3f); // 0.0-0.3 based on intensity
            
            // Log the event
            Debug.Log($"Horror Event Started: {EventName} - Affecting {affectedLights.Count} lights");
        }
        
        protected override void OnEventUpdate(float progress)
        {
            // Calculate current event intensity based on progress
            float currentIntensity = GetCurrentIntensity(progress);
            
            // Update each affected light
            foreach (Light light in affectedLights)
            {
                if (light == null) continue;
                
                // Calculate flicker factor using Perlin noise for more natural flickering
                float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, light.GetInstanceID() * 0.1f);
                float flickerFactor = Mathf.Lerp(minFlickerMultiplier, maxFlickerMultiplier, noise);
                
                // Apply flicker with intensity based on progress
                float targetIntensity = originalIntensities[light] * flickerFactor;
                
                // Lerp between original and flickering based on current intensity
                light.intensity = Mathf.Lerp(originalIntensities[light], targetIntensity, currentIntensity);
                
                // Rarely (based on intensity) turn light completely off for a brief moment
                if (Random.value < 0.01f * currentIntensity)
                {
                    light.intensity = 0;
                }
            }
        }
        
        protected override void OnEventEnd()
        {
            // Restore original light intensities
            foreach (Light light in affectedLights)
            {
                if (light == null) continue;
                
                // Gradually restore original intensity
                light.intensity = originalIntensities[light];
            }
            
            // Clear references
            affectedLights.Clear();
            originalIntensities.Clear();
            
            Debug.Log($"Horror Event Ended: {EventName}");
        }
    }
}