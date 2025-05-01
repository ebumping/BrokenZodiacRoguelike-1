using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Core system that manages player sanity levels and triggers appropriate effects based on current sanity state
    /// </summary>
    public class SanitySystem : MonoBehaviour
    {   
        public enum SanityState
        {
            Stable,      // 100%-80%: Normal gameplay with minimal effects
            Unsettled,   // 79%-60%: Minor visual distortions, occasional whispers
            Disturbed,   // 59%-40%: Moderate visual effects, enemy appearance distortion, hallucinations
            Fractured,   // 39%-20%: Severe visual warping, frequent hallucinations, map alterations
            Shattered,   // 19%-1%: Extreme reality distortion, constant hallucinations
            Transcended  // 0%: Final zodiac transformation, temporary loss of player control
        }
        
        [Header("Sanity Settings")]
        [Range(0f, 100f)]
        [SerializeField] private float currentSanity = 100f;
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float sanityDecayRate = 0.5f;
        [SerializeField] private float sanityRecoveryRate = 1f;
        
        [Header("Sanity State Thresholds")]
        [SerializeField] private float stableThreshold = 80f;
        [SerializeField] private float unsettledThreshold = 60f;
        [SerializeField] private float disturbedThreshold = 40f;
        [SerializeField] private float fracturedThreshold = 20f;
        [SerializeField] private float shatteredThreshold = 1f;
        
        [Header("References")]
        [SerializeField] private ZodiacSign playerZodiacSign;
        [SerializeField] private SanityEffectsManager effectsManager;
        [SerializeField] private ZodiacSanityTransformation transformationManager;
        
        // Properties
        public float CurrentSanity => currentSanity;
        public float MaxSanity => maxSanity;
        public SanityState CurrentSanityState { get; private set; } = SanityState.Stable;
        
        // Events
        public delegate void SanityChangedHandler(float newSanity, float maxSanity);
        public event SanityChangedHandler OnSanityChanged;
        
        public delegate void SanityStateChangedHandler(SanityState newState, SanityState oldState);
        public event SanityStateChangedHandler OnSanityStateChanged;
        
        private void Start()
        {   
            // Initialize sanity to maximum
            currentSanity = maxSanity;
            UpdateSanityState();
        }
        
        private void Update()
        {   
            // Optionally implement automatic sanity decay/recovery logic here
        }
        
        /// <summary>
        /// Decrease the player's sanity by the specified amount
        /// </summary>
        public void DecreaseSanity(float amount)
        {   
            float previousSanity = currentSanity;
            SanityState previousState = CurrentSanityState;
            
            currentSanity = Mathf.Max(0f, currentSanity - amount);
            
            if (currentSanity != previousSanity)
            {   
                OnSanityChanged?.Invoke(currentSanity, maxSanity);
                UpdateSanityState();
                
                if (CurrentSanityState != previousState)
                {   
                    OnSanityStateChanged?.Invoke(CurrentSanityState, previousState);
                    TriggerSanityEffects();
                }
            }
        }
        
        /// <summary>
        /// Increase the player's sanity by the specified amount
        /// </summary>
        public void IncreaseSanity(float amount)
        {   
            float previousSanity = currentSanity;
            SanityState previousState = CurrentSanityState;
            
            currentSanity = Mathf.Min(maxSanity, currentSanity + amount);
            
            if (currentSanity != previousSanity)
            {   
                OnSanityChanged?.Invoke(currentSanity, maxSanity);
                UpdateSanityState();
                
                if (CurrentSanityState != previousState)
                {   
                    OnSanityStateChanged?.Invoke(CurrentSanityState, previousState);
                    TriggerSanityEffects();
                }
            }
        }
        
        /// <summary>
        /// Set the player's sanity directly to a specific value
        /// </summary>
        public void SetSanity(float value)
        {   
            float previousSanity = currentSanity;
            SanityState previousState = CurrentSanityState;
            
            currentSanity = Mathf.Clamp(value, 0f, maxSanity);
            
            if (currentSanity != previousSanity)
            {   
                OnSanityChanged?.Invoke(currentSanity, maxSanity);
                UpdateSanityState();
                
                if (CurrentSanityState != previousState)
                {   
                    OnSanityStateChanged?.Invoke(CurrentSanityState, previousState);
                    TriggerSanityEffects();
                }
            }
        }
        
        /// <summary>
        /// Update the current sanity state based on sanity level
        /// </summary>
        private void UpdateSanityState()
        {   
            SanityState newState;
            
            if (currentSanity >= stableThreshold)
            {   
                newState = SanityState.Stable;
            }
            else if (currentSanity >= unsettledThreshold)
            {   
                newState = SanityState.Unsettled;
            }
            else if (currentSanity >= disturbedThreshold)
            {   
                newState = SanityState.Disturbed;
            }
            else if (currentSanity >= fracturedThreshold)
            {   
                newState = SanityState.Fractured;
            }
            else if (currentSanity > shatteredThreshold)
            {   
                newState = SanityState.Shattered;
            }
            else
            {   
                newState = SanityState.Transcended;
            }
            
            if (newState != CurrentSanityState)
            {   
                SanityState previousState = CurrentSanityState;
                CurrentSanityState = newState;
                OnSanityStateChanged?.Invoke(CurrentSanityState, previousState);
            }
        }
        
        /// <summary>
        /// Trigger effects based on current sanity state
        /// </summary>
        private void TriggerSanityEffects()
        {   
            // Apply visual and audio effects
            if (effectsManager != null)
            {   
                effectsManager.ApplySanityEffects(CurrentSanityState, currentSanity / maxSanity);
            }
            
            // Apply zodiac transformations if sanity is low enough
            if (transformationManager != null && playerZodiacSign != null)
            {   
                if (CurrentSanityState == SanityState.Fractured)
                {   
                    transformationManager.InitiateTransformation(1); // First stage
                }
                else if (CurrentSanityState == SanityState.Shattered)
                {   
                    transformationManager.InitiateTransformation(2); // Second stage
                }
                else if (CurrentSanityState == SanityState.Transcended)
                {   
                    transformationManager.InitiateTransformation(3); // Final stage
                }
                else
                {   
                    transformationManager.RevertTransformation(); // Revert if sanity recovers
                }
            }
        }
    }
}