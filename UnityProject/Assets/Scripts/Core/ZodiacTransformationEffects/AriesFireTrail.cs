using UnityEngine;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Core.ZodiacTransformationEffects
{
    // Special effect for Aries transformation that creates a trail of fire behind the player
    public class AriesFireTrail : MonoBehaviour
    {
        [Header("Fire Trail Settings")]
        [SerializeField] private float spawnInterval = 0.2f; // How often to spawn fire (seconds)
        [SerializeField] private float trailLifetime = 3f; // How long trail elements last
        [SerializeField] private float trailDamage = 5f; // Damage per second
        [SerializeField] private float trailRadius = 1f; // Radius of trail effect
        [SerializeField] private float moveSpeedThreshold = 3f; // Min speed to create trail
        [SerializeField] private bool onlySprinting = true; // Only create trail when sprinting
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject fireTrailPrefab; // Prefab for fire effect
        [SerializeField] private float trailSizeVariation = 0.2f; // Random size variation
        [SerializeField] private Color trailColor = new Color(1f, 0.3f, 0f, 0.7f); // Fire color
        
        // Runtime variables
        private float _nextSpawnTime;
        private PlayerController _player;
        private List<FireTrailElement> _activeTrailElements = new List<FireTrailElement>();
        private Vector3 _lastPosition;
        private bool _isSprinting;
        
        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _lastPosition = transform.position;
        }
        
        private void OnEnable()
        {
            _nextSpawnTime = Time.time;
        }
        
        private void Update()
        {
            // Update sprinting state
            _isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            // Check if we should spawn a new trail element
            if (Time.time >= _nextSpawnTime && ShouldCreateTrail())
            {
                CreateTrailElement();
                _nextSpawnTime = Time.time + spawnInterval;
            }
            
            // Update active trail elements
            UpdateTrailElements();
            
            // Update last position
            _lastPosition = transform.position;
        }
        
        // Should we create a trail element?
        private bool ShouldCreateTrail()
        {
            // Check if player is moving fast enough
            float speed = Vector3.Distance(_lastPosition, transform.position) / Time.deltaTime;
            bool fastEnough = speed >= moveSpeedThreshold;
            
            // Check sprinting if required
            if (onlySprinting && !_isSprinting)
                return false;
                
            return fastEnough;
        }
        
        // Create a new trail element
        private void CreateTrailElement()
        {
            Vector3 position = transform.position;
            position.y = 0.1f; // Slightly above ground
            
            GameObject elementObj;
            
            // Use prefab if available
            if (fireTrailPrefab != null)
            {
                elementObj = Instantiate(fireTrailPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create a simple object if no prefab
                elementObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                elementObj.transform.position = position;
                
                // Remove collider as we'll handle damage separately
                Destroy(elementObj.GetComponent<Collider>());
                
                // Set up material
                Renderer renderer = elementObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = new Material(Shader.Find("Particles/Additive"));
                    material.color = trailColor;
                    renderer.material = material;
                }
                
                // Random size variation
                float sizeVariation = 1f + Random.Range(-trailSizeVariation, trailSizeVariation);
                elementObj.transform.localScale = Vector3.one * trailRadius * 2f * sizeVariation;
                
                // Add particle effect for fire
                ParticleSystem ps = elementObj.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 1f;
                main.startSpeed = 1f;
                main.startSize = 0.3f;
                main.startColor = trailColor;
                
                // Emission module
                var emission = ps.emission;
                emission.rateOverTime = 20f;
                
                // Shape module
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = trailRadius * 0.5f;
            }
            
            // Create trail element data
            FireTrailElement element = new FireTrailElement
            {
                GameObject = elementObj,
                Position = position,
                CreationTime = Time.time,
                Lifetime = trailLifetime,
                Radius = trailRadius * elementObj.transform.localScale.x * 0.5f // Account for scale
            };
            
            // Add to active elements
            _activeTrailElements.Add(element);
        }
        
        // Update all active trail elements
        private void UpdateTrailElements()
        {
            // Process each trail element
            for (int i = _activeTrailElements.Count - 1; i >= 0; i--)
            {
                FireTrailElement element = _activeTrailElements[i];
                
                // Check if expired
                if (Time.time >= element.CreationTime + element.Lifetime)
                {
                    // Remove and destroy
                    if (element.GameObject != null)
                    {
                        Destroy(element.GameObject);
                    }
                    
                    _activeTrailElements.RemoveAt(i);
                    continue;
                }
                
                // Apply damage to nearby enemies
                ApplyDamageFromElement(element);
                
                // Update visual effect (fade out towards end of life)
                UpdateElementVisual(element);
            }
        }
        
        // Apply damage from a trail element
        private void ApplyDamageFromElement(FireTrailElement element)
        {
            // Skip if null or too young (allow time to avoid immediately damaging)  
            if (element.GameObject == null || Time.time < element.CreationTime + 0.1f)
                return;
                
            // Find all entities in radius
            Collider[] colliders = Physics.OverlapSphere(element.Position, element.Radius);
            
            foreach (var collider in colliders)
            {
                // Skip self
                if (collider.gameObject == gameObject)
                    continue;
                    
                // Check if it's a damageable entity
                DamageableEntity entity = collider.GetComponent<DamageableEntity>();
                if (entity != null)
                {
                    // Calculate damage based on Time.deltaTime for consistent DPS
                    float damage = trailDamage * Time.deltaTime;
                    entity.TakeDamage(damage, DamageType.Fire, _player?.gameObject);
                    
                    // Apply burning status to enemies
                    EnemyController enemy = entity as EnemyController;
                    if (enemy != null)
                    {
                        enemy.ApplyStatusEffect(StatusEffectType.Burning, 2f);
                    }
                }
            }
        }
        
        // Update visual effects for a trail element
        private void UpdateElementVisual(FireTrailElement element)
        {
            if (element.GameObject == null)
                return;
                
            // Calculate age ratio (0-1)
            float age = Time.time - element.CreationTime;
            float ageRatio = age / element.Lifetime;
            
            // Fade out and scale down towards end of life
            if (ageRatio > 0.7f)
            {
                float fadeRatio = (ageRatio - 0.7f) / 0.3f; // 0-1 for last 30% of life
                
                // Scale down
                float scale = Mathf.Lerp(1f, 0.2f, fadeRatio);
                element.GameObject.transform.localScale = element.GameObject.transform.localScale * scale;
                
                // Fade out renderer
                Renderer renderer = element.GameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = Mathf.Lerp(color.a, 0f, fadeRatio * 0.1f); // Gradual fade
                    renderer.material.color = color;
                }
                
                // Fade out particles
                ParticleSystem ps = element.GameObject.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    Color particleColor = main.startColor.color;
                    particleColor.a = Mathf.Lerp(particleColor.a, 0f, fadeRatio * 0.1f);
                    main.startColor = particleColor;
                    
                    // Slow down emission
                    var emission = ps.emission;
                    emission.rateOverTime = Mathf.Lerp(20f, 0f, fadeRatio);
                }
            }
        }
        
        private void OnDisable()
        {
            // Clean up all trail elements
            foreach (var element in _activeTrailElements)
            {
                if (element.GameObject != null)
                {
                    Destroy(element.GameObject);
                }
            }
            
            _activeTrailElements.Clear();
        }
        
        // Gizmos for debugging
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            
            // Draw trail elements
            foreach (var element in _activeTrailElements)
            {
                Gizmos.DrawWireSphere(element.Position, element.Radius);
            }
        }
    }
    
    // Data class for fire trail elements
    public class FireTrailElement
    {
        public GameObject GameObject;
        public Vector3 Position;
        public float CreationTime;
        public float Lifetime;
        public float Radius;
    }
}