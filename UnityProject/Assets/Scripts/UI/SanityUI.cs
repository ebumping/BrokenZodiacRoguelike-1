using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodexOfTheBrokenZodiac.Core;
using System.Collections;

namespace CodexOfTheBrokenZodiac.UI
{
    // This class handles the UI display of sanity status
    public class SanityUI : MonoBehaviour
    {        
        [Header("UI References")]
        [SerializeField] private Image sanityFillImage;
        [SerializeField] private TextMeshProUGUI sanityStateText;
        [SerializeField] private RectTransform sanityIconContainer;
        [SerializeField] private Image sanityIconImage;
        [SerializeField] private CanvasGroup warningGroup;
        [SerializeField] private RectTransform pulseOverlay;
        
        [Header("Visual Settings")]
        [SerializeField] private Gradient sanityGradient;
        [SerializeField] private Sprite[] sanityIcons; // Icons for different states
        [SerializeField] private string[] sanityStateLabels = {
            "Stable",
            "Uneasy",
            "Disturbed",
            "Unstable",
            "Broken"
        };
        [SerializeField] private Color[] stateColors; // Colors for state text
        
        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float pulseIntensity = 0.2f;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private AnimationCurve shakeCurve;
        
        // Runtime variables
        private float _currentSanity = 1f;
        private SanityState _currentState = SanityState.Normal;
        private bool _isAnimating = false;
        private Coroutine _pulseCoroutine;
        private Coroutine _shakeCoroutine;
        private Coroutine _flashCoroutine;
        
        private void Start()
        {
            // Register for sanity events
            if (SanitySystem.Instance != null)
            {
                SanitySystem.Instance.OnPlayerSanityChanged += OnSanityChanged;
                SanitySystem.Instance.OnPlayerSanityStateChanged += OnSanityStateChanged;
                SanitySystem.Instance.OnPlayerHallucination += OnHallucination;
                
                // Initialize with current sanity if available
                int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
                if (localPlayerId != 0)
                {
                    float sanity = SanitySystem.Instance.GetPlayerSanityPercentage(localPlayerId);
                    SanityState state = SanitySystem.Instance.GetPlayerSanityState(localPlayerId);
                    UpdateSanityUI(sanity, state);
                }
            }
            
            // Initialize UI
            if (warningGroup != null)
            {
                warningGroup.alpha = 0f;
            }
        }
        
        private void OnDestroy()
        {
            // Unregister events
            if (SanitySystem.Instance != null)
            {
                SanitySystem.Instance.OnPlayerSanityChanged -= OnSanityChanged;
                SanitySystem.Instance.OnPlayerSanityStateChanged -= OnSanityStateChanged;
                SanitySystem.Instance.OnPlayerHallucination -= OnHallucination;
            }
            
            // Clean up coroutines
            StopAllCoroutines();
        }
        
        private void Update()
        {
            // Optional: Add ambient animations here
        }
        
        // Handle sanity changed event
        private void OnSanityChanged(int playerId, float currentSanity, float maxSanity)
        {
            // Only update for local player
            int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
            if (playerId != localPlayerId)
                return;
                
            UpdateSanityUI(currentSanity / maxSanity, _currentState);
        }
        
        // Handle sanity state changed event
        private void OnSanityStateChanged(int playerId, SanityState oldState, SanityState newState)
        {
            // Only update for local player
            int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
            if (playerId != localPlayerId)
                return;
                
            UpdateSanityUI(_currentSanity, newState);
            
            // Play state change animation
            PlayStateChangeAnimation(oldState, newState);
        }
        
        // Handle hallucination event
        private void OnHallucination(int playerId, SanityHallucination hallucinationType)
        {
            // Only update for local player
            int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
            if (playerId != localPlayerId)
                return;
                
            // Play hallucination effect on the UI
            PlayHallucinationEffect(hallucinationType);
        }
        
        // Update the UI with sanity values
        private void UpdateSanityUI(float sanityPercentage, SanityState state)
        {
            _currentSanity = sanityPercentage;
            _currentState = state;
            
            // Update fill bar
            if (sanityFillImage != null)
            {
                sanityFillImage.fillAmount = sanityPercentage;
                sanityFillImage.color = sanityGradient.Evaluate(1f - sanityPercentage);
            }
            
            // Update state text
            if (sanityStateText != null)
            {
                int stateIndex = (int)state;
                if (stateIndex < sanityStateLabels.Length)
                {
                    sanityStateText.text = sanityStateLabels[stateIndex];
                    
                    // Set text color if available
                    if (stateColors != null && stateIndex < stateColors.Length)
                    {
                        sanityStateText.color = stateColors[stateIndex];
                    }
                }
            }
            
            // Update icon
            if (sanityIconImage != null && sanityIcons != null)
            {
                int iconIndex = (int)state;
                if (iconIndex < sanityIcons.Length)
                {
                    sanityIconImage.sprite = sanityIcons[iconIndex];
                }
            }
            
            // Show warning overlay for low sanity
            if (warningGroup != null)
            {
                float targetAlpha = sanityPercentage < 0.3f ? 1f : 0f;
                
                // If we're in a bad state, show warning
                if (state >= SanityState.Disturbed)
                {
                    targetAlpha = 1f;
                }
                
                // Start fading if needed
                if (Mathf.Abs(warningGroup.alpha - targetAlpha) > 0.01f)
                {
                    StartCoroutine(FadeWarning(targetAlpha));
                }
            }
            
            // Start pulsing effect for low sanity
            if (pulseOverlay != null && sanityPercentage < 0.5f)
            {
                StartPulsingEffect();
            }
            else if (pulseOverlay != null)
            {
                StopPulsingEffect();
            }
        }
        
        // Play animation when sanity state changes
        private void PlayStateChangeAnimation(SanityState oldState, SanityState newState)
        {
            // Don't animate if getting better
            if (newState < oldState)
                return;
                
            // Different animations based on severity
            switch (newState)
            {
                case SanityState.Wary:
                    // Subtle pulse
                    if (sanityIconContainer != null)
                    {
                        StartCoroutine(PulseAnimation(sanityIconContainer, 1.1f, 0.3f));
                    }
                    break;
                    
                case SanityState.Disturbed:
                    // Flash the UI
                    if (sanityFillImage != null)
                    {
                        if (_flashCoroutine != null)
                            StopCoroutine(_flashCoroutine);
                            
                        _flashCoroutine = StartCoroutine(FlashAnimation(sanityFillImage, Color.red, Color.white, 0.5f));
                    }
                    break;
                    
                case SanityState.Unstable:
                    // Shake the UI
                    if (sanityIconContainer != null)
                    {
                        if (_shakeCoroutine != null)
                            StopCoroutine(_shakeCoroutine);
                            
                        _shakeCoroutine = StartCoroutine(ShakeAnimation(sanityIconContainer, 0.5f, 10f));
                    }
                    break;
                    
                case SanityState.Broken:
                    // Combined effects
                    if (sanityIconContainer != null)
                    {
                        StartCoroutine(PulseAnimation(sanityIconContainer, 1.3f, 0.5f));
                        
                        if (_shakeCoroutine != null)
                            StopCoroutine(_shakeCoroutine);
                            
                        _shakeCoroutine = StartCoroutine(ShakeAnimation(sanityIconContainer, 1f, 15f));
                    }
                    
                    if (sanityFillImage != null)
                    {
                        if (_flashCoroutine != null)
                            StopCoroutine(_flashCoroutine);
                            
                        _flashCoroutine = StartCoroutine(FlashAnimation(sanityFillImage, Color.red, Color.black, 1f));
                    }
                    break;
            }
        }
        
        // Play effect when hallucination occurs
        private void PlayHallucinationEffect(SanityHallucination type)
        {
            switch (type)
            {
                case SanityHallucination.VisualDistortion:
                    // Distort the whole UI
                    StartCoroutine(DistortUI(0.5f));
                    break;
                    
                case SanityHallucination.AudioHallucination:
                    // Just pulse the sanity meter
                    if (sanityIconContainer != null)
                    {
                        StartCoroutine(PulseAnimation(sanityIconContainer, 1.2f, 0.4f));
                    }
                    break;
                    
                case SanityHallucination.EnvironmentChange:
                    // Flash the UI briefly
                    if (transform as RectTransform != null)
                    {
                        StartCoroutine(FlashAnimation(transform.GetComponent<Image>(), new Color(0, 0, 0.5f, 0.5f), Color.clear, 0.3f));
                    }
                    break;
                    
                case SanityHallucination.FalseEnemy:
                    // Red warning flash
                    if (transform as RectTransform != null)
                    {
                        StartCoroutine(FlashAnimation(transform.GetComponent<Image>(), new Color(0.5f, 0, 0, 0.3f), Color.clear, 0.5f));
                    }
                    break;
                    
                case SanityHallucination.FalseAttack:
                    // Strong red flash and shake
                    if (transform as RectTransform != null)
                    {
                        StartCoroutine(FlashAnimation(transform.GetComponent<Image>(), new Color(1f, 0, 0, 0.5f), Color.clear, 0.2f));
                    }
                    
                    if (sanityIconContainer != null)
                    {
                        StartCoroutine(ShakeAnimation(sanityIconContainer, 0.3f, 20f));
                    }
                    break;
            }
        }
        
        #region Animation Coroutines
        
        // Start pulsing effect
        private void StartPulsingEffect()
        {
            if (_pulseCoroutine != null)
                return;
                
            _pulseCoroutine = StartCoroutine(PulseOverlay());
        }
        
        // Stop pulsing effect
        private void StopPulsingEffect()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
                
                // Reset overlay
                if (pulseOverlay != null)
                {
                    pulseOverlay.localScale = Vector3.one;
                }
            }
        }
        
        // Continuous pulsing animation
        private IEnumerator PulseOverlay()
        {
            float time = 0f;
            
            while (true)
            {
                time += Time.deltaTime * pulseSpeed;
                
                // Calculate pulse factor
                float pulse = 1f + Mathf.Sin(time * Mathf.PI * 2f) * pulseIntensity;
                
                // Apply to overlay
                if (pulseOverlay != null)
                {
                    pulseOverlay.localScale = new Vector3(pulse, pulse, 1f);
                }
                
                yield return null;
            }
        }
        
        // Fade the warning overlay
        private IEnumerator FadeWarning(float targetAlpha)
        {
            float startAlpha = warningGroup.alpha;
            float time = 0f;
            
            while (time < 1f)
            {
                time += Time.deltaTime * fadeSpeed;
                warningGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time);
                yield return null;
            }
            
            warningGroup.alpha = targetAlpha;
        }
        
        // One-time pulse animation
        private IEnumerator PulseAnimation(RectTransform target, float maxScale, float duration)
        {
            if (target == null) yield break;
            
            Vector3 originalScale = target.localScale;
            float time = 0f;
            
            while (time < duration)
            {
                float t = time / duration;
                float scale = 1f + (maxScale - 1f) * Mathf.Sin(t * Mathf.PI);
                target.localScale = originalScale * scale;
                
                time += Time.deltaTime;
                yield return null;
            }
            
            target.localScale = originalScale;
        }
        
        // Shake animation
        private IEnumerator ShakeAnimation(RectTransform target, float duration, float intensity)
        {
            if (target == null) yield break;
            
            Vector3 originalPosition = target.localPosition;
            float time = 0f;
            
            while (time < duration)
            {
                float progress = time / duration;
                float strength = (shakeCurve != null) ? shakeCurve.Evaluate(progress) : (1f - progress);
                
                Vector3 randomOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0
                ) * intensity * strength;
                
                target.localPosition = originalPosition + randomOffset;
                
                time += Time.deltaTime;
                yield return null;
            }
            
            target.localPosition = originalPosition;
        }
        
        // Flash color animation
        private IEnumerator FlashAnimation(Graphic target, Color flashColor, Color originalColor, float duration)
        {
            if (target == null) yield break;
            
            float time = 0f;
            
            while (time < duration)
            {
                float t = time / duration;
                target.color = Color.Lerp(flashColor, originalColor, t);
                
                time += Time.deltaTime;
                yield return null;
            }
            
            target.color = originalColor;
        }
        
        // UI distortion effect
        private IEnumerator DistortUI(float duration)
        {
            // This would ideally use a custom shader effect
            // For now, we'll just do a simple scale distortion
            
            Vector3 originalScale = transform.localScale;
            float time = 0f;
            
            while (time < duration)
            {
                float t = time / duration;
                float xScale = 1f + Mathf.Sin(t * Mathf.PI * 8f) * 0.05f;
                float yScale = 1f + Mathf.Cos(t * Mathf.PI * 7f) * 0.05f;
                
                transform.localScale = new Vector3(xScale, yScale, 1f);
                
                time += Time.deltaTime;
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        #endregion
    }
}