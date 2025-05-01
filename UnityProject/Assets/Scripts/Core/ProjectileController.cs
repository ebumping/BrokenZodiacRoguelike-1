using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Controls projectile behavior, movement, and collision
    /// </summary>
    public class ProjectileController : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private LayerMask collisionLayers;
        
        [Header("Effects")]
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private bool destroyOnImpact = true;
        
        // State
        private bool initialized = false;
        private GameObject owner;
        private Vector3 direction;
        private float lifeTimer;
        
        private void Start()
        {
            // Set default direction if not initialized externally
            if (!initialized)
            {
                direction = transform.forward;
            }
            
            // Start lifetime countdown
            lifeTimer = maxLifetime;
        }
        
        private void Update()
        {
            // Move projectile
            transform.position += direction * speed * Time.deltaTime;
            
            // Update lifetime
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0)
            {
                Destroy(gameObject);
            }
        }
        
        private void FixedUpdate()
        {
            // Cast ray to detect collisions
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, speed * Time.fixedDeltaTime, collisionLayers))
            {
                HandleCollision(hit);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Skip collision with owner
            if (owner != null && other.gameObject == owner)
            {
                return;
            }
            
            // Get hit point
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            
            // Handle collision
            HandleCollision(other, hitPoint);
        }
        
        /// <summary>
        /// Initialize projectile properties
        /// </summary>
        public void Initialize(float newDamage, float newSpeed, DamageType newDamageType, GameObject newOwner)
        {
            damage = newDamage;
            speed = newSpeed;
            damageType = newDamageType;
            owner = newOwner;
            direction = transform.forward;
            initialized = true;
        }
        
        /// <summary>
        /// Initialize projectile with direction
        /// </summary>
        public void Initialize(float newDamage, float newSpeed, DamageType newDamageType, GameObject newOwner, Vector3 newDirection)
        {
            damage = newDamage;
            speed = newSpeed;
            damageType = newDamageType;
            owner = newOwner;
            direction = newDirection.normalized;
            transform.forward = direction;
            initialized = true;
        }
        
        /// <summary>
        /// Handle collision with a raycast hit
        /// </summary>
        private void HandleCollision(RaycastHit hit)
        {
            // Apply damage if target has a health system
            HealthSystem healthSystem = hit.collider.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.ApplyDamage(damage, damageType, hit.point, owner);
            }
            
            // Spawn impact effect
            SpawnImpactEffect(hit.point, hit.normal);
            
            // Play impact sound
            PlayImpactSound(hit.point);
            
            // Destroy projectile if set to do so
            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Handle collision with a trigger
        /// </summary>
        private void HandleCollision(Collider other, Vector3 hitPoint)
        {
            // Apply damage if target has a health system
            HealthSystem healthSystem = other.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.ApplyDamage(damage, damageType, hitPoint, owner);
            }
            
            // Calculate normal (simplified)
            Vector3 normal = (transform.position - hitPoint).normalized;
            
            // Spawn impact effect
            SpawnImpactEffect(hitPoint, normal);
            
            // Play impact sound
            PlayImpactSound(hitPoint);
            
            // Destroy projectile if set to do so
            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Spawn impact effect at the hit point
        /// </summary>
        private void SpawnImpactEffect(Vector3 position, Vector3 normal)
        {
            if (impactEffectPrefab != null)
            {
                // Create impact effect
                GameObject effect = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
                
                // Destroy after a set time
                Destroy(effect, 2f);
            }
        }
        
        /// <summary>
        /// Play impact sound at the hit point
        /// </summary>
        private void PlayImpactSound(Vector3 position)
        {
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, position);
            }
        }
    }
}