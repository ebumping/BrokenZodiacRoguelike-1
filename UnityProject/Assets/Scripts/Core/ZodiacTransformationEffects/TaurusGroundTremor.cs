using UnityEngine;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Core.ZodiacTransformationEffects
{
    // Special effect for Taurus transformation that creates ground tremors that stagger enemies
    public class TaurusGroundTremor : MonoBehaviour
    {
        [Header("Tremor Settings")]
        [SerializeField] private float tremorCooldown = 3f; // Time between tremors
        [SerializeField] private float tremorRadius = 5f; // Radius of effect
        [SerializeField] private float tremorDamage = 10f; // Damage dealt by tremor
        [SerializeField] private float knockbackForce = 5f; // Force applied to enemies
        [SerializeField] private float stunDuration = 1.5f; // Duration of stun effect
        [SerializeField] private bool requireMovement = true; // Only tremor when moving
        [SerializeField] private float movementThreshold = 0.1f; // Minimum movement to trigger
        [SerializeField] private bool requireGrounded = true; // Only tremor when grounded
        
        [Header("Visual and Audio Effects")]
        [SerializeField] private GameObject tremorVFXPrefab; // Prefab for tremor visual
        [SerializeField] private AudioClip tremorSound; // Sound effect
        [SerializeField] private float cameraShakeIntensity = 0.3f; // Intensity of camera shake
        [SerializeField] private float cameraShakeDuration = 0.5f; // Duration of camera shake
        
        // Runtime variables
        private float _nextTremorTime;
        private PlayerController _player;
        private Vector3 _lastPosition;
        private bool _isGrounded = true;
        
        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _lastPosition = transform.position;
            _nextTremorTime = Time.time + tremorCooldown * 0.5f; // Initial delay
        }
        
        private void Update()
        {
            // Check if tremor is ready and conditions are met
            if (Time.time >= _nextTremorTime && CanCreateTremor())
            {
                CreateTremor();
                _nextTremorTime = Time.time + tremorCooldown;
            }
            
            // Update last position
            _lastPosition = transform.position;
        }
        
        // Check if tremor can be created
        private bool CanCreateTremor()
        {
            // Check if moving
            if (requireMovement)
            {
                float movement = Vector3.Distance(_lastPosition, transform.position);
                if (movement < movementThreshold)
                    return false;
            }
            
            // Check if grounded
            if (requireGrounded && !_isGrounded)
                return false;
                
            return true;
        }
        
        // Create a ground tremor effect
        private void CreateTremor()
        {
            // Create visual effect
            if (tremorVFXPrefab != null)
            {
                GameObject vfx = Instantiate(tremorVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 3f); // Clean up after effect
            }
            else
            {
                // Create simple effect if no prefab available
                CreateSimpleTremorEffect();
            }
            
            // Play sound
            if (tremorSound != null)
            {
                AudioSource.PlayClipAtPoint(tremorSound, transform.position);
            }
            
            // Apply camera shake
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShakeCamera(cameraShakeIntensity, cameraShakeDuration);
            }
            
            // Apply effects to nearby enemies
            AffectNearbyEnemies();
        }
        
        // Create a simple tremor visual effect
        private void CreateSimpleTremorEffect()
        {
            // Create expanding ring
            GameObject ringObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringObj.transform.position = transform.position + Vector3.up * 0.1f; // Slightly above ground
            ringObj.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f); // Thin disk
            
            // Remove collider
            Destroy(ringObj.GetComponent<Collider>());
            
            // Set material
            Renderer renderer = ringObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Transparent/Diffuse"));
                material.color = new Color(0.6f, 0.4f, 0.2f, 0.7f); // Earthy color
                renderer.material = material;
            }
            
            // Add expansion behavior
            StartCoroutine(ExpandRing(ringObj));
            
            // Add cracks in the ground
            CreateGroundCracks();
        }
        
        // Create ground crack effects
        private void CreateGroundCracks()
        {
            int crackCount = Random.Range(3, 6);
            
            for (int i = 0; i < crackCount; i++)
            {
                // Calculate random direction
                float angle = Random.Range(0f, Mathf.PI * 2f);
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                
                // Create crack line
                GameObject crackObj = new GameObject("GroundCrack");
                crackObj.transform.position = transform.position;
                
                // Add line renderer
                LineRenderer line = crackObj.AddComponent<LineRenderer>();
                line.startWidth = 0.1f;
                line.endWidth = 0.05f;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = new Color(0.3f, 0.2f, 0.1f, 0.8f);
                line.endColor = new Color(0.3f, 0.2f, 0.1f, 0f);
                
                // Generate jagged line
                int segments = Random.Range(5, 10);
                Vector3[] positions = new Vector3[segments];
                
                for (int j = 0; j < segments; j++)
                {
                    float distance = (j / (float)(segments - 1)) * tremorRadius;
                    
                    // Add some randomness to direction
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-0.5f, 0.5f),
                        0,
                        Random.Range(-0.5f, 0.5f)
                    ) * (distance * 0.2f);
                    
                    positions[j] = transform.position + (direction * distance) + randomOffset;
                    positions[j].y = 0.05f; // Just above ground
                }
                
                line.positionCount = segments;
                line.SetPositions(positions);
                
                // Destroy after effect
                Destroy(crackObj, 2f);
            }
        }
        
        // Expand the ring effect
        private System.Collections.IEnumerator ExpandRing(GameObject ringObj)
        {
            if (ringObj == null) yield break;
            
            float duration = 1.5f;
            float startTime = Time.time;
            
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                
                // Expand ring
                float scale = Mathf.Lerp(0.5f, tremorRadius * 2f, t);
                ringObj.transform.localScale = new Vector3(scale, 0.05f, scale);
                
                // Fade out towards the end
                if (t > 0.6f)
                {
                    Renderer renderer = ringObj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Color color = renderer.material.color;
                        color.a = Mathf.Lerp(0.7f, 0f, (t - 0.6f) / 0.4f);
                        renderer.material.color = color;
                    }
                }
                
                yield return null;
            }
            
            // Destroy ring
            Destroy(ringObj);
        }
        
        // Apply effects to nearby enemies
        private void AffectNearbyEnemies()
        {
            // Find all entities in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, tremorRadius);
            
            foreach (var collider in colliders)
            {
                // Skip self
                if (collider.gameObject == gameObject)
                    continue;
                    
                // Apply effects to enemies
                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    // Apply damage
                    enemy.TakeDamage(tremorDamage, DamageType.Physical, _player?.gameObject);
                    
                    // Apply knockback
                    Vector3 direction = (enemy.transform.position - transform.position).normalized;
                    enemy.ApplyKnockback(direction, knockbackForce);
                    
                    // Apply stun
                    enemy.ApplyStatusEffect(StatusEffectType.Stunned, stunDuration);
                }
                
                // Apply effects to environmental objects (future expansion)
            }
        }
        
        // Set grounded state from player controller
        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
        }
        
        // Display gizmos in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.brown;
            Gizmos.DrawWireSphere(transform.position, tremorRadius);
        }
    }
}