using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Manages visual and audio effects related to sanity state changes
    /// </summary>
    public class SanityEffectsManager : MonoBehaviour
    {
        [Header("Post-Processing Effects")]
        [SerializeField] private Volume postProcessingVolume;
        
        [Header("Audio Effects")]
        [SerializeField] private AudioSource sanityAmbientSource;
        [SerializeField] private AudioClip[] sanityStateAmbience;
        [SerializeField] private AudioClip[] sanityStateTransitions;
        [SerializeField] private float transitionFadeDuration = 1.5f;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject[] visualHallucinations;
        [SerializeField] private float hallucinationMinInterval = 5f;
        [SerializeField] private float hallucinationMaxInterval = 15f;
        
        // Post-processing effect references
        private Vignette vignette;
        private ChromaticAberration chromaticAberration;
        private LensDistortion lensDistortion;
        private ColorAdjustments colorAdjustments;
        private FilmGrain filmGrain;
        
        // Current sanity state tracking
        private SanitySystem.SanityState currentSanityState;
        private float currentSanityNormalized = 1f;
        
        // Hallucination coroutine reference
        private Coroutine hallucinationCoroutine;
        
        private void Awake()
        {
            // Get post-processing effect references
            if (postProcessingVolume != null && postProcessingVolume.profile != null)
            {
                postProcessingVolume.profile.TryGet(out vignette);
                postProcessingVolume.profile.TryGet(out chromaticAberration);
                postProcessingVolume.profile.TryGet(out lensDistortion);
                postProcessingVolume.profile.TryGet(out colorAdjustments);
                postProcessingVolume.profile.TryGet(out filmGrain);
            }
        }
        
        /// <summary>
        /// Apply effects based on the current sanity state
        /// </summary>
        public void ApplySanityEffects(SanitySystem.SanityState state, float normalizedSanity)
        {
            // Track state changes
            SanitySystem.SanityState previousState = currentSanityState;
            currentSanityState = state;
            currentSanityNormalized = normalizedSanity;
            
            // Apply visual effects based on sanity state
            ApplyVisualEffects(state, normalizedSanity);
            
            // Apply audio effects based on sanity state
            ApplyAudioEffects(state, previousState);
            
            // Manage hallucinations based on sanity state
            ManageHallucinations(state);
        }
        
        /// <summary>
        /// Apply visual post-processing effects based on sanity state
        /// </summary>
        private void ApplyVisualEffects(SanitySystem.SanityState state, float normalizedSanity)
        {
            if (postProcessingVolume == null || !postProcessingVolume.profile)
            {
                return;
            }
            
            // Calculate intensity based on sanity state and normalized value
            float intensity = 0f;
            
            switch (state)
            {
                case SanitySystem.SanityState.Stable:
                    intensity = 0f;
                    break;
                case SanitySystem.SanityState.Unsettled:
                    intensity = Mathf.Lerp(0.1f, 0.3f, 1f - NormalizeInRange(normalizedSanity, 0.6f, 0.8f));
                    break;
                case SanitySystem.SanityState.Disturbed:
                    intensity = Mathf.Lerp(0.3f, 0.5f, 1f - NormalizeInRange(normalizedSanity, 0.4f, 0.6f));
                    break;
                case SanitySystem.SanityState.Fractured:
                    intensity = Mathf.Lerp(0.5f, 0.7f, 1f - NormalizeInRange(normalizedSanity, 0.2f, 0.4f));
                    break;
                case SanitySystem.SanityState.Shattered:
                    intensity = Mathf.Lerp(0.7f, 0.9f, 1f - NormalizeInRange(normalizedSanity, 0.01f, 0.2f));
                    break;
                case SanitySystem.SanityState.Transcended:
                    intensity = 1f;
                    break;
            }
            
            // Apply effects with appropriate intensity values
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(0.2f, 0.7f, intensity);
                vignette.color.value = Color.Lerp(Color.black, Color.red, intensity * 0.7f);
            }
            
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(0f, 1f, intensity);
            }
            
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = Mathf.Lerp(0f, 0.5f, intensity);
                lensDistortion.scale.value = Mathf.Lerp(1f, 0.85f, intensity);
            }
            
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = Mathf.Lerp(0f, -50f, intensity);
                colorAdjustments.contrast.value = Mathf.Lerp(0f, 20f, intensity);
            }
            
            if (filmGrain != null)
            {
                filmGrain.intensity.value = Mathf.Lerp(0f, 1f, intensity);
            }
        }
        
        /// <summary>
        /// Apply audio effects based on sanity state
        /// </summary>
        private void ApplyAudioEffects(SanitySystem.SanityState newState, SanitySystem.SanityState previousState)
        {
            if (sanityAmbientSource == null || sanityStateAmbience == null || sanityStateAmbience.Length == 0)
            {
                return;
            }
            
            // If state changed, play transition sound and change ambience
            if (newState != previousState)
            {
                // Play transition sound if available
                int transitionIndex = (int)newState;
                if (sanityStateTransitions != null && transitionIndex < sanityStateTransitions.Length && transitionIndex >= 0)
                {
                    AudioClip transitionClip = sanityStateTransitions[transitionIndex];
                    if (transitionClip != null)
                    {
                        AudioSource.PlayClipAtPoint(transitionClip, Camera.main.transform.position);
                    }
                }
                
                // Change ambient sound
                StartCoroutine(TransitionAmbientSound(newState));
            }
        }
        
        /// <summary>
        /// Smoothly transition ambient sound based on sanity state
        /// </summary>
        private IEnumerator TransitionAmbientSound(SanitySystem.SanityState state)
        {
            // Store original volume
            float originalVolume = sanityAmbientSource.volume;
            
            // Fade out current sound
            float elapsed = 0f;
            while (elapsed < transitionFadeDuration * 0.5f)
            {
                sanityAmbientSource.volume = Mathf.Lerp(originalVolume, 0f, elapsed / (transitionFadeDuration * 0.5f));
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Change clip
            int stateIndex = (int)state;
            if (stateIndex >= 0 && stateIndex < sanityStateAmbience.Length && sanityStateAmbience[stateIndex] != null)
            {
                sanityAmbientSource.clip = sanityStateAmbience[stateIndex];
                sanityAmbientSource.Play();
            }
            
            // Fade in new sound
            elapsed = 0f;
            while (elapsed < transitionFadeDuration * 0.5f)
            {
                sanityAmbientSource.volume = Mathf.Lerp(0f, originalVolume, elapsed / (transitionFadeDuration * 0.5f));
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Ensure final volume is correct
            sanityAmbientSource.volume = originalVolume;
        }
        
        /// <summary>
        /// Manage hallucination effects based on sanity state
        /// </summary>
        private void ManageHallucinations(SanitySystem.SanityState state)
        {
            // Stop any existing hallucination coroutine
            if (hallucinationCoroutine != null)
            {
                StopCoroutine(hallucinationCoroutine);
                hallucinationCoroutine = null;
            }
            
            // Start hallucinations if sanity is low enough
            if (state == SanitySystem.SanityState.Disturbed ||
                state == SanitySystem.SanityState.Fractured ||
                state == SanitySystem.SanityState.Shattered ||
                state == SanitySystem.SanityState.Transcended)
            {
                hallucinationCoroutine = StartCoroutine(ShowRandomHallucinations(state));
            }
        }
        
        /// <summary>
        /// Coroutine to show random hallucinations based on sanity state
        /// </summary>
        private IEnumerator ShowRandomHallucinations(SanitySystem.SanityState state)
        {
            if (visualHallucinations == null || visualHallucinations.Length == 0)
            {
                yield break;
            }
            
            // Determine frequency based on sanity state
            float frequency = 0f;
            switch (state)
            {
                case SanitySystem.SanityState.Disturbed:
                    frequency = 0.1f;
                    break;
                case SanitySystem.SanityState.Fractured:
                    frequency = 0.3f;
                    break;
                case SanitySystem.SanityState.Shattered:
                    frequency = 0.6f;
                    break;
                case SanitySystem.SanityState.Transcended:
                    frequency = 1f;
                    break;
                default:
                    frequency = 0f;
                    break;
            }
            
            while (true)
            {
                // Wait for a random interval
                float waitTime = Mathf.Lerp(hallucinationMaxInterval, hallucinationMinInterval, frequency);
                yield return new WaitForSeconds(waitTime);
                
                // Show a random hallucination if state is still valid
                if (currentSanityState == state)
                {
                    int hallIndex = Random.Range(0, visualHallucinations.Length);
                    GameObject hallucination = visualHallucinations[hallIndex];
                    
                    if (hallucination != null)
                    {
                        // Position the hallucination in the player's view
                        Vector3 hallPos = GetRandomHallucinationPosition();
                        hallucination.transform.position = hallPos;
                        
                        // Show the hallucination
                        hallucination.SetActive(true);
                        
                        // Hide it after a random duration
                        float duration = Random.Range(0.5f, 3f) * (1f - frequency * 0.5f);
                        yield return new WaitForSeconds(duration);
                        
                        hallucination.SetActive(false);
                    }
                }
                else
                {
                    // Sanity state changed, stop coroutine
                    break;
                }
            }
        }
        
        /// <summary>
        /// Get a random position for a hallucination to appear
        /// </summary>
        private Vector3 GetRandomHallucinationPosition()
        {
            // Placeholder implementation - in a real game, this would position hallucinations
            // at positions visible to the player but not obstructing gameplay
            Camera mainCamera = Camera.main;
            
            if (mainCamera != null)
            {
                // Get a random point in the camera's view frustum
                float distance = Random.Range(5f, 15f);
                float viewportX = Random.Range(0.1f, 0.9f);
                float viewportY = Random.Range(0.1f, 0.9f);
                
                Vector3 viewportPoint = new Vector3(viewportX, viewportY, distance);
                return mainCamera.ViewportToWorldPoint(viewportPoint);
            }
            
            // Fallback
            return transform.position + Random.insideUnitSphere * 10f;
        }
        
        /// <summary>
        /// Normalize a value within a specific range
        /// </summary>
        private float NormalizeInRange(float value, float min, float max)
        {
            return Mathf.Clamp01((value - min) / (max - min));
        }
    }
}