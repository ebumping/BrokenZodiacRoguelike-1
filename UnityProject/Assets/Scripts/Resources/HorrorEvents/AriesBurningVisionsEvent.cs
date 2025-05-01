using UnityEngine;
using System.Collections.Generic;
using CosmicHorror.ProceduralHorror;

namespace CosmicHorror.HorrorEvents
{
    /// <summary>
    /// Aries-specific horror event that causes the player to see burning visions
    /// and leave temporary fire trails
    /// </summary>
    public class AriesBurningVisionsEvent : ZodiacHorrorEvent
    {
        // Event properties
        public override string EventName => "Burning Visions";
        public override string EventDescription => "Your vision blurs with heat as flames dance at the edges of your sight.";
        public override float BaseDuration => 25f;
        public override float BaseIntensity => 0.8f;
        public override ZodiacSign ZodiacSign => ZodiacSign.Aries;
        
        // Effect references
        private GameObject fireVfxPrefab;
        private Material burningScreenMaterial;
        private GameObject playerFireTrail;
        
        // Burning vision effect
        private Camera playerCamera;
        private List<GameObject> spawnedFireEffects = new List<GameObject>();
        
        // Screen effect control
        private float maxDistortionStrength = 0.15f;
        private float maxHeatHazeStrength = 0.3f;
        private float burnVignettePower = 2.5f;
        
        protected override void OnEventStart()
        {
            // Find player components
            playerCamera = GameObject.FindObjectOfType<Camera>();
            Transform playerTransform = GameObject.FindObjectOfType<PlayerController>()?.transform;
            
            if (playerCamera == null || playerTransform == null)
            {
                Debug.LogError("AriesBurningVisionsEvent: Required player components not found!");
                ForceEnd();
                return;
            }
            
            // Load resources
            fireVfxPrefab = Resources.Load<GameObject>("VFX/FireEffect");
            burningScreenMaterial = Resources.Load<Material>("Materials/BurningVisionMaterial");
            
            if (fireVfxPrefab == null || burningScreenMaterial == null)
            {
                Debug.LogError("AriesBurningVisionsEvent: Required resources not found!");
                ForceEnd();
                return;
            }
            
            // Apply post-processing material to camera
            if (playerCamera.gameObject.GetComponent<BurningVisionEffect>() == null)
            {
                BurningVisionEffect effect = playerCamera.gameObject.AddComponent<BurningVisionEffect>();
                effect.SetMaterial(burningScreenMaterial);
                effect.SetIntensity(0f);
            }
            
            // Create fire trail for player
            if (playerTransform != null)
            {
                playerFireTrail = new GameObject("PlayerFireTrail");
                playerFireTrail.transform.SetParent(playerTransform);
                playerFireTrail.transform.localPosition = Vector3.zero;
                
                // Add fire trail component
                FireTrailEffect trailEffect = playerFireTrail.AddComponent<FireTrailEffect>();
                trailEffect.Initialize(playerTransform, eventIntensity);
            }
            
            Debug.Log($"Horror Event Started: {EventName} - Aries Burning Visions");
        }
        
        protected override void OnEventUpdate(float progress)
        {
            // Calculate current event intensity based on progress
            float currentIntensity = GetCurrentIntensity(progress);
            
            // Update screen effect intensity
            BurningVisionEffect visionEffect = playerCamera?.GetComponent<BurningVisionEffect>();
            if (visionEffect != null)
            {
                visionEffect.SetIntensity(currentIntensity);
                
                // Update material properties based on intensity
                visionEffect.SetDistortion(maxDistortionStrength * currentIntensity);
                visionEffect.SetHeatHaze(maxHeatHazeStrength * currentIntensity);
                visionEffect.SetBurnVignette(burnVignettePower * currentIntensity);
            }
            
            // Randomly spawn fire effects in player's view
            if (Random.value < 0.05f * currentIntensity && fireVfxPrefab != null)
            {
                SpawnFireVisionEffect();
            }
            
            // Update fire trail intensity if it exists
            if (playerFireTrail != null)
            {
                FireTrailEffect trailEffect = playerFireTrail.GetComponent<FireTrailEffect>();
                if (trailEffect != null)
                {
                    trailEffect.SetIntensity(currentIntensity);
                }
            }
        }
        
        protected override void OnEventEnd()
        {
            // Fade out screen effect
            BurningVisionEffect visionEffect = playerCamera?.GetComponent<BurningVisionEffect>();
            if (visionEffect != null)
            {
                visionEffect.FadeOut(1.5f);
            }
            
            // Destroy all spawned fire effects
            foreach (GameObject effect in spawnedFireEffects)
            {
                if (effect != null)
                {
                    GameObject.Destroy(effect, 1.5f);
                }
            }
            
            // Destroy player fire trail
            if (playerFireTrail != null)
            {
                FireTrailEffect trailEffect = playerFireTrail.GetComponent<FireTrailEffect>();
                if (trailEffect != null)
                {
                    trailEffect.FadeOut(1.5f);
                }
                
                GameObject.Destroy(playerFireTrail, 2f);
            }
            
            spawnedFireEffects.Clear();
            
            Debug.Log($"Horror Event Ended: {EventName}");
        }
        
        /// <summary>
        /// Spawn a fire effect somewhere in the player's view
        /// </summary>
        private void SpawnFireVisionEffect()
        {
            if (playerCamera == null || fireVfxPrefab == null) return;
            
            // Calculate random position in player's view
            float randomX = Random.Range(-0.4f, 0.4f);
            float randomY = Random.Range(-0.3f, 0.3f);
            float distance = Random.Range(5f, 15f);
            
            Vector3 viewportPoint = new Vector3(0.5f + randomX, 0.5f + randomY, distance);
            Vector3 worldPoint = playerCamera.ViewportToWorldPoint(viewportPoint);
            
            // Spawn fire effect
            GameObject fireEffect = GameObject.Instantiate(fireVfxPrefab, worldPoint, Quaternion.identity);
            
            // Scale effect based on intensity
            float scale = 0.5f + (eventIntensity * 1.5f);
            fireEffect.transform.localScale = new Vector3(scale, scale, scale);
            
            // Add to tracking list
            spawnedFireEffects.Add(fireEffect);
            
            // Destroy after random time
            float lifetime = Random.Range(2f, 5f);
            GameObject.Destroy(fireEffect, lifetime);
        }
    }
    
    /// <summary>
    /// Component for applying the burning vision post-processing effect
    /// </summary>
    public class BurningVisionEffect : MonoBehaviour
    {
        private Material effectMaterial;
        private float intensity;
        private float fadeSpeed;
        private bool isFading;
        
        private static readonly int IntensityProperty = Shader.PropertyToID("_Intensity");
        private static readonly int DistortionProperty = Shader.PropertyToID("_DistortionStrength");
        private static readonly int HeatHazeProperty = Shader.PropertyToID("_HeatHazeStrength");
        private static readonly int BurnVignetteProperty = Shader.PropertyToID("_BurnVignettePower");
        
        public void SetMaterial(Material material)
        {
            effectMaterial = material;
        }
        
        public void SetIntensity(float value)
        {
            intensity = value;
            if (effectMaterial != null)
            {
                effectMaterial.SetFloat(IntensityProperty, intensity);
            }
        }
        
        public void SetDistortion(float value)
        {
            if (effectMaterial != null)
            {
                effectMaterial.SetFloat(DistortionProperty, value);
            }
        }
        
        public void SetHeatHaze(float value)
        {
            if (effectMaterial != null)
            {
                effectMaterial.SetFloat(HeatHazeProperty, value);
            }
        }
        
        public void SetBurnVignette(float value)
        {
            if (effectMaterial != null)
            {
                effectMaterial.SetFloat(BurnVignetteProperty, value);
            }
        }
        
        public void FadeOut(float duration)
        {
            isFading = true;
            fadeSpeed = intensity / duration;
        }
        
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (effectMaterial != null && intensity > 0)
            {
                Graphics.Blit(source, destination, effectMaterial);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
        
        private void Update()
        {
            if (isFading && intensity > 0)
            {
                intensity -= fadeSpeed * Time.deltaTime;
                if (intensity <= 0)
                {
                    intensity = 0;
                    isFading = false;
                    
                    // Remove component when fully faded
                    Destroy(this);
                }
                
                SetIntensity(intensity);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up material if needed
            if (effectMaterial != null && !Application.isPlaying)
            {
                Destroy(effectMaterial);
            }
        }
    }
    
    /// <summary>
    /// Component for creating a fire trail behind the player
    /// </summary>
    public class FireTrailEffect : MonoBehaviour
    {
        private Transform followTarget;
        private float intensity;
        private float emissionRate;
        private float trailLifetime;
        
        private float timeSinceLastEmission;
        private bool isFadingOut;
        private List<GameObject> activeTrailParticles = new List<GameObject>();
        
        private GameObject fireParticlePrefab;
        
        public void Initialize(Transform target, float initialIntensity)
        {
            followTarget = target;
            intensity = initialIntensity;
            
            // Load fire particle prefab
            fireParticlePrefab = Resources.Load<GameObject>("VFX/FireParticle");
            
            // Set initial parameters
            UpdateParameters();
        }
        
        public void SetIntensity(float value)
        {
            intensity = value;
            UpdateParameters();
        }
        
        public void FadeOut(float duration)
        {
            isFadingOut = true;
            
            // Gradually reduce intensity
            if (duration > 0)
            {
                emissionRate = 0; // Stop emitting new particles
            }
        }
        
        private void UpdateParameters()
        {
            // Update emission parameters based on intensity
            emissionRate = 0.2f - (0.15f * intensity); // Time between emissions
            trailLifetime = 1f + (intensity * 3f); // How long trail particles last
        }
        
        private void Update()
        {
            if (followTarget == null || fireParticlePrefab == null) return;
            
            // Update position to follow target
            transform.position = followTarget.position;
            
            // Emit trail particles
            timeSinceLastEmission += Time.deltaTime;
            
            if (!isFadingOut && timeSinceLastEmission >= emissionRate)
            {
                EmitTrailParticle();
                timeSinceLastEmission = 0;
            }
            
            // Fade out if needed
            if (isFadingOut && intensity > 0)
            {
                intensity -= Time.deltaTime;
                if (intensity <= 0)
                {
                    intensity = 0;
                }
                
                UpdateParameters();
            }
        }
        
        private void EmitTrailParticle()
        {
            // Spawn fire particle at current position
            GameObject particle = Instantiate(fireParticlePrefab, transform.position, Quaternion.identity);
            
            // Scale based on intensity
            float scale = 0.3f + (intensity * 0.7f);
            particle.transform.localScale = new Vector3(scale, scale, scale);
            
            // Add to tracking list
            activeTrailParticles.Add(particle);
            
            // Destroy after lifetime
            Destroy(particle, trailLifetime);
            
            // Remove from list when destroyed
            StartCoroutine(RemoveFromListAfterDelay(particle, trailLifetime));
        }
        
        private System.Collections.IEnumerator RemoveFromListAfterDelay(GameObject particle, float delay)
        {
            yield return new WaitForSeconds(delay);
            activeTrailParticles.Remove(particle);
        }
        
        private void OnDestroy()
        {
            // Clean up any remaining particles
            foreach (GameObject particle in activeTrailParticles)
            {
                if (particle != null)
                {
                    Destroy(particle);
                }
            }
            
            activeTrailParticles.Clear();
        }
    }
}