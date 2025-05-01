using UnityEngine;
using System.Collections.Generic;
using CosmicHorror.ProceduralHorror;

namespace CosmicHorror.HorrorEvents
{
    /// <summary>
    /// Horror event that spawns hallucinated enemies that appear real but dissipate when attacked
    /// </summary>
    public class FalseEnemyEvent : HorrorEventBase
    {
        // Event properties
        public override string EventName => "Phantom Adversaries";
        public override string EventDescription => "Shadowy figures appear at the edge of your vision, but are they real?";
        public override float BaseDuration => 30f;
        public override float BaseIntensity => 0.65f;
        
        // Effect control
        private int maxHallucinatedEnemies = 5;
        private float spawnRadius = 15f;
        private float minSpawnDistance = 5f;
        private float spawnCooldown = 5f;
        private float lastSpawnTime = 0f;
        
        // Tracking
        private List<GameObject> hallucinatedEnemies = new List<GameObject>();
        private Transform playerTransform;
        private Camera playerCamera;
        
        // Prefab references
        private List<GameObject> enemyPrefabs = new List<GameObject>();
        
        protected override void OnEventStart()
        {
            // Find player components
            playerTransform = GameObject.FindObjectOfType<PlayerController>()?.transform;
            playerCamera = GameObject.FindObjectOfType<Camera>();
            
            if (playerTransform == null)
            {
                Debug.LogError("FalseEnemyEvent: Player transform not found!");
                ForceEnd();
                return;
            }
            
            // Load enemy prefabs from resources
            enemyPrefabs = new List<GameObject>(Resources.LoadAll<GameObject>("Prefabs/Enemies"));
            
            if (enemyPrefabs.Count == 0)
            {
                Debug.LogError("FalseEnemyEvent: No enemy prefabs found!");
                ForceEnd();
                return;
            }
            
            // Adjust spawn parameters based on intensity
            maxHallucinatedEnemies = Mathf.RoundToInt(3 + (eventIntensity * 4)); // 3-7 based on intensity
            spawnCooldown = 8f - (eventIntensity * 5f); // 3-8 seconds between spawns
            
            Debug.Log($"Horror Event Started: {EventName} - Max Enemies: {maxHallucinatedEnemies}");
        }
        
        protected override void OnEventUpdate(float progress)
        {
            if (playerTransform == null) return;
            
            // Calculate current event intensity based on progress
            float currentIntensity = GetCurrentIntensity(progress);
            
            // Update spawn timer
            lastSpawnTime += Time.deltaTime;
            
            // Try to spawn a new hallucinated enemy
            if (lastSpawnTime >= spawnCooldown && hallucinatedEnemies.Count < maxHallucinatedEnemies)
            {
                SpawnHallucinatedEnemy(currentIntensity);
                lastSpawnTime = 0f;
            }
            
            // Update existing hallucinated enemies
            UpdateHallucinatedEnemies(currentIntensity);
            
            // Clean up any null references
            hallucinatedEnemies.RemoveAll(e => e == null);
        }
        
        protected override void OnEventEnd()
        {
            // Fade out all hallucinated enemies
            foreach (GameObject enemy in hallucinatedEnemies)
            {
                if (enemy != null)
                {
                    FadeOutHallucinatedEnemy(enemy);
                }
            }
            
            // Clear list
            hallucinatedEnemies.Clear();
            
            Debug.Log($"Horror Event Ended: {EventName}");
        }
        
        /// <summary>
        /// Spawn a new hallucinated enemy
        /// </summary>
        private void SpawnHallucinatedEnemy(float intensity)
        {
            if (enemyPrefabs.Count == 0 || playerTransform == null) return;
            
            // Determine spawn position
            Vector3 spawnPosition;
            
            // 60% chance to spawn in player's view, 40% chance to spawn behind
            bool spawnInView = Random.value < 0.6f;
            
            if (spawnInView && playerCamera != null)
            {
                // Spawn at edge of screen
                float randomX = Random.Range(-0.5f, 0.5f);
                float randomY = Random.Range(-0.3f, 0.3f);
                
                // Make sure it's near the edges
                if (Mathf.Abs(randomX) < 0.4f && Mathf.Abs(randomY) < 0.2f)
                {
                    if (Mathf.Abs(randomX) > Mathf.Abs(randomY))
                    {
                        randomX = randomX > 0 ? 0.5f : -0.5f;
                    }
                    else
                    {
                        randomY = randomY > 0 ? 0.3f : -0.3f;
                    }
                }
                
                // Get world position from viewport
                Vector3 viewportPoint = new Vector3(0.5f + randomX, 0.5f + randomY, spawnRadius);
                spawnPosition = playerCamera.ViewportToWorldPoint(viewportPoint);
            }
            else
            {
                // Random position around player
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnDistance, spawnRadius);
                spawnPosition = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            }
            
            // Choose random enemy prefab
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            
            // Spawn hallucinated enemy
            GameObject hallucinatedEnemy = GameObject.Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            
            // Set up hallucination component
            HallucinatedEnemyComponent hallucination = hallucinatedEnemy.AddComponent<HallucinatedEnemyComponent>();
            hallucination.Initialize(playerTransform, eventIntensity);
            
            // Add to tracking list
            hallucinatedEnemies.Add(hallucinatedEnemy);
            
            // Apply slight modification to appearance to hint it's not real
            // (subtle distortion, color shift, transparency, etc.)
            ApplyHallucinationEffects(hallucinatedEnemy, intensity);
        }
        
        /// <summary>
        /// Apply visual effects to make the enemy look hallucinated
        /// </summary>
        private void ApplyHallucinationEffects(GameObject enemy, float intensity)
        {
            HallucinatedEnemyComponent hallucination = enemy.GetComponent<HallucinatedEnemyComponent>();
            if (hallucination != null)
            {
                hallucination.SetIntensity(intensity);
                
                // Apply visual distortion based on intensity
                hallucination.SetDistortionLevel(intensity * 0.7f);
                
                // Apply color shift
                hallucination.SetColorShift(new Color(1f, 0.7f, 0.7f, 0.8f + (0.2f * (1 - intensity))));
            }
        }
        
        /// <summary>
        /// Update all hallucinated enemies
        /// </summary>
        private void UpdateHallucinatedEnemies(float intensity)
        {
            foreach (GameObject enemy in hallucinatedEnemies)
            {
                if (enemy == null) continue;
                
                HallucinatedEnemyComponent hallucination = enemy.GetComponent<HallucinatedEnemyComponent>();
                if (hallucination != null)
                {
                    // Update hallucination intensity
                    hallucination.SetIntensity(intensity);
                    
                    // Check if hallucination should disappear
                    if (hallucination.ShouldDisappear())
                    {
                        FadeOutHallucinatedEnemy(enemy);
                    }
                }
            }
        }
        
        /// <summary>
        /// Fade out and destroy a hallucinated enemy
        /// </summary>
        private void FadeOutHallucinatedEnemy(GameObject enemy)
        {
            HallucinatedEnemyComponent hallucination = enemy.GetComponent<HallucinatedEnemyComponent>();
            if (hallucination != null)
            {
                hallucination.FadeOut(1.5f);
            }
            else
            {
                GameObject.Destroy(enemy, 0.1f);
            }
        }
    }
    
    /// <summary>
    /// Component for controlling hallucinated enemy behavior
    /// </summary>
    public class HallucinatedEnemyComponent : MonoBehaviour
    {
        // References
        private Transform playerTransform;
        
        // State
        private float intensity;
        private float distortionLevel;
        private float disappearChance;
        private bool isFading;
        private float fadeSpeed;
        private float alpha = 1.0f;
        
        // Appearance
        private Color colorShift = Color.white;
        private List<Renderer> enemyRenderers = new List<Renderer>();
        private List<Material> originalMaterials = new List<Material>();
        private List<Material> instancedMaterials = new List<Material>();
        
        // Movement
        private float moveSpeed;
        private float rotationSpeed;
        private float lastDistanceToPlayer;
        private Vector3 circlingDirection;
        private float circlingFactor;
        
        // Behavior
        private float attackRange = 3f;
        private float disappearDistance = 30f;
        private float timeAlive;
        private float maxLifetime = 20f;
        private bool wasAttacked;
        private float attackedDisappearDelay = 1.5f;
        private float attackedTime;
        
        public void Initialize(Transform target, float initialIntensity)
        {
            playerTransform = target;
            intensity = initialIntensity;
            
            // Cache initial distance to player
            lastDistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Set parameters based on intensity
            moveSpeed = 2f + (intensity * 3f); // 2-5 units per second
            disappearChance = 0.001f + (intensity * 0.003f); // 0.001-0.004 chance per frame
            maxLifetime = 15f + (Random.value * 15f); // 15-30 seconds
            
            // Cache all renderers
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            enemyRenderers.AddRange(renderers);
            
            // Create instanced materials for each renderer
            foreach (Renderer renderer in enemyRenderers)
            {
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    originalMaterials.Add(mats[i]);
                    Material instancedMat = new Material(mats[i]);
                    instancedMaterials.Add(instancedMat);
                    mats[i] = instancedMat;
                }
                renderer.materials = mats;
            }
            
            // Set random circling behavior
            circlingDirection = Random.value < 0.5f ? Vector3.right : Vector3.left;
            circlingFactor = Random.Range(0.2f, 0.6f);
            rotationSpeed = Random.Range(1f, 3f);
            
            // Disable any actual enemy behaviors/collisions
            DisableRealEnemyComponents();
        }
        
        /// <summary>
        /// Disable components that would make this a "real" enemy
        /// </summary>
        private void DisableRealEnemyComponents()
        {
            // Disable any AI, NavMesh, or combat components
            MonoBehaviour[] components = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                // Don't disable this component
                if (component == this) continue;
                
                // Disable all other behaviors
                component.enabled = false;
            }
            
            // Disable colliders (or make them triggers)
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;
            }
        }
        
        public void SetIntensity(float value)
        {
            intensity = value;
        }
        
        public void SetDistortionLevel(float value)
        {
            distortionLevel = value;
            
            // Apply distortion to materials
            foreach (Material mat in instancedMaterials)
            {
                if (mat.HasProperty("_DistortionAmount"))
                {
                    mat.SetFloat("_DistortionAmount", distortionLevel);
                }
            }
        }
        
        public void SetColorShift(Color color)
        {
            colorShift = color;
            
            // Apply color shift to materials
            foreach (Material mat in instancedMaterials)
            {
                if (mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", colorShift);
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", colorShift);
                }
            }
        }
        
        public void FadeOut(float duration)
        {
            isFading = true;
            fadeSpeed = 1f / duration;
        }
        
        /// <summary>
        /// Check if the hallucination should disappear
        /// </summary>
        public bool ShouldDisappear()
        {
            // Always disappear if attacked
            if (wasAttacked && Time.time - attackedTime > attackedDisappearDelay)
            {
                return true;
            }
            
            // Disappear if too far from player
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer > disappearDistance)
            {
                return true;
            }
            
            // Disappear if alive too long
            if (timeAlive > maxLifetime)
            {
                return true;
            }
            
            // Random chance to disappear when not visible
            if (!IsVisibleToPlayer() && Random.value < disappearChance)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if the hallucination is currently visible to the player
        /// </summary>
        private bool IsVisibleToPlayer()
        {
            if (playerTransform == null) return false;
            
            // Simple visibility check using raycasting
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;
            
            if (distanceToPlayer > disappearDistance) return false;
            
            Ray ray = new Ray(transform.position, directionToPlayer.normalized);
            if (Physics.Raycast(ray, out RaycastHit hit, distanceToPlayer))
            {
                // Something is blocking line of sight
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                return player != null;
            }
            
            return true;
        }
        
        /// <summary>
        /// Handle the hallucination being "attacked" by the player
        /// </summary>
        public void OnAttacked()
        {
            wasAttacked = true;
            attackedTime = Time.time;
            
            // Visual feedback - glitch/flicker effect
            StartCoroutine(GlitchEffect(attackedDisappearDelay));
        }
        
        private System.Collections.IEnumerator GlitchEffect(float duration)
        {
            float startTime = Time.time;
            float endTime = startTime + duration;
            
            while (Time.time < endTime)
            {
                // Random visibility
                bool visible = Random.value < 0.7f;
                SetRendererVisibility(visible);
                
                // Random position offset
                transform.position += new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0,
                    Random.Range(-0.5f, 0.5f)
                );
                
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
            
            // Ensure visible for final fade
            SetRendererVisibility(true);
        }
        
        private void SetRendererVisibility(bool visible)
        {
            foreach (Renderer renderer in enemyRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }
        }
        
        private void Update()
        {
            if (playerTransform == null) return;
            
            // Update lifetime
            timeAlive += Time.deltaTime;
            
            // Handle fading
            if (isFading)
            {
                alpha -= fadeSpeed * Time.deltaTime;
                if (alpha <= 0)
                {
                    alpha = 0;
                    Destroy(gameObject);
                }
                
                // Apply alpha to materials
                SetAlpha(alpha);
            }
            
            // Update movement
            UpdateMovement();
            
            // Update visuals based on distance to player
            UpdateVisuals();
            
            // Update last distance to player
            lastDistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        }
        
        private void UpdateMovement()
        {
            // Calculate direction to player
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;
            
            // Only move if not too close to player
            if (distanceToPlayer > attackRange)
            {
                // Basic movement toward player
                Vector3 movementDirection = directionToPlayer.normalized;
                
                // Add circling behavior
                if (distanceToPlayer < 10f)
                {
                    // As we get closer, increase circling
                    float circleFactor = circlingFactor * (1f - (distanceToPlayer / 10f));
                    Vector3 circleDir = Vector3.Cross(movementDirection, Vector3.up) * circleFactor;
                    movementDirection = (movementDirection + circleDir).normalized;
                }
                
                // Move toward target
                transform.position += movementDirection * moveSpeed * Time.deltaTime;
                
                // Rotate to face movement direction
                if (movementDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, 
                        targetRotation, 
                        rotationSpeed * Time.deltaTime
                    );
                }
            }
            else
            {
                // At attack range, perform "attack" behavior
                // For hallucinations, just look threatening
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * 2f * Time.deltaTime
                );
            }
        }
        
        private void UpdateVisuals()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Adjust distortion based on distance changes
            // More distortion when moving closer/further rapidly
            float distanceChange = Mathf.Abs(distanceToPlayer - lastDistanceToPlayer);
            float additionalDistortion = distanceChange * 0.5f;
            
            // Apply to base distortion
            SetDistortionLevel(distortionLevel + additionalDistortion);
        }
        
        private void SetAlpha(float a)
        {
            foreach (Material mat in instancedMaterials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color currentColor = mat.GetColor("_Color");
                    mat.SetColor("_Color", new Color(currentColor.r, currentColor.g, currentColor.b, a));
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    Color currentColor = mat.GetColor("_BaseColor");
                    mat.SetColor("_BaseColor", new Color(currentColor.r, currentColor.g, currentColor.b, a));
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if hit by player attack
            if (other.CompareTag("PlayerAttack") || other.CompareTag("PlayerProjectile"))
            {
                OnAttacked();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up instanced materials
            foreach (Material mat in instancedMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
        }
    }
}