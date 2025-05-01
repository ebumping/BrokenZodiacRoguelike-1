using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using CosmicHorror.ProceduralHorror;

namespace CosmicHorror.UI
{
    /// <summary>
    /// UI component for displaying horror event information and notifications
    /// </summary>
    public class HorrorEventUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup notificationPanel;
        [SerializeField] private TextMeshProUGUI eventNameText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private Image eventIconImage;
        [SerializeField] private Slider progressBar;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip notificationSound;
        
        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 1.0f;
        [SerializeField] private float displayDuration = 4.0f;
        [SerializeField] private float fadeOutDuration = 1.5f;
        
        private Coroutine activeNotificationCoroutine;
        private HorrorEventGenerator eventGenerator;
        
        private void Awake()
        {
            // Make sure panel starts hidden
            if (notificationPanel != null)
            {
                notificationPanel.alpha = 0;
                notificationPanel.gameObject.SetActive(false);
            }
        }
        
        private void Start()
        {
            // Find the horror event generator
            eventGenerator = FindObjectOfType<HorrorEventGenerator>();
            
            if (eventGenerator == null)
            {
                Debug.LogWarning("HorrorEventUI: No HorrorEventGenerator found in scene!");
            }
        }
        
        /// <summary>
        /// Show a notification for a horror event
        /// </summary>
        public void ShowEventNotification(string eventName, string eventDescription, Sprite eventIcon = null)
        {
            // Stop any active notification
            if (activeNotificationCoroutine != null)
            {
                StopCoroutine(activeNotificationCoroutine);
            }
            
            // Start new notification
            activeNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine(eventName, eventDescription, eventIcon));
        }
        
        /// <summary>
        /// Coroutine for showing and animating the notification
        /// </summary>
        private IEnumerator ShowNotificationCoroutine(string eventName, string eventDescription, Sprite eventIcon)
        {
            // Set up the notification
            if (eventNameText != null) eventNameText.text = eventName;
            if (eventDescriptionText != null) eventDescriptionText.text = eventDescription;
            
            if (eventIconImage != null)
            {
                eventIconImage.gameObject.SetActive(eventIcon != null);
                if (eventIcon != null)
                {
                    eventIconImage.sprite = eventIcon;
                }
            }
            
            // Reset progress bar
            if (progressBar != null)
            {
                progressBar.value = 0;
            }
            
            // Enable and fade in the panel
            notificationPanel.gameObject.SetActive(true);
            
            // Play notification sound
            if (audioSource != null && notificationSound != null)
            {
                audioSource.PlayOneShot(notificationSound);
            }
            
            // Fade in
            float startTime = Time.time;
            float endTime = startTime + fadeInDuration;
            
            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / fadeInDuration;
                notificationPanel.alpha = Mathf.Lerp(0, 1, t);
                yield return null;
            }
            
            notificationPanel.alpha = 1;
            
            // Display for duration
            startTime = Time.time;
            endTime = startTime + displayDuration;
            
            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / displayDuration;
                
                // Update progress bar
                if (progressBar != null)
                {
                    progressBar.value = t;
                }
                
                yield return null;
            }
            
            // Fade out
            startTime = Time.time;
            endTime = startTime + fadeOutDuration;
            
            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / fadeOutDuration;
                notificationPanel.alpha = Mathf.Lerp(1, 0, t);
                yield return null;
            }
            
            notificationPanel.alpha = 0;
            notificationPanel.gameObject.SetActive(false);
            
            activeNotificationCoroutine = null;
        }
        
        /// <summary>
        /// Show a persistent horror effect indicator
        /// </summary>
        public void ShowPersistentEffect(HorrorEventBase activeEvent)
        {
            // Implementation for persistent UI elements that show active horror effects
            // This would be used for longer-lasting effects that the player needs to be aware of
        }
        
        /// <summary>
        /// Hide the persistent horror effect indicator
        /// </summary>
        public void HidePersistentEffect()
        {
            // Hide any persistent UI elements
        }
        
        /// <summary>
        /// Create a screen effect pulse (used for sudden horror moments)
        /// </summary>
        public void PulseScreenEffect(float intensity)
        {
            // Implement screen pulse effect for jump scares or sudden events
        }
    }
}