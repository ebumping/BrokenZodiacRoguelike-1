using UnityEngine;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Core.ZodiacTransformationEffects
{
    // Special effect for Libra transformation that reflects damage back to attackers
    public class LibraDamageReflection : MonoBehaviour
    {
        [SerializeField] private float reflectionPercentage = 1.0f; // Default 100% reflection
        [SerializeField] private float maxReflectionDamage = 50f; // Cap on reflected damage
        [SerializeField] private bool reflectToAll = false; // Reflect to all nearby enemies
        [SerializeField] private float reflectionRadius = 10f; // Radius for area reflection
        [SerializeField] private GameObject reflectionVFXPrefab;
        [SerializeField] private AudioClip reflectionSound;
        
        private PlayerController _player;
        private Dictionary<GameObject, float> _recentDamageBySource = new Dictionary<GameObject, float>();
        private float _reflectionCooldown = 0.5f;
        private float _lastReflectionTime = -999f;
        
        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }
        
        private void OnEnable()
        {
            // Subscribe to damage events
            if (_player != null)
            {
                _player.OnDamageTaken += OnPlayerDamageTaken;
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from damage events
            if (_player != null)
            {
                _player.OnDamageTaken -= OnPlayerDamageTaken;
            }
        }
        
        // Handle damage taken event
        private void OnPlayerDamageTaken(DamageInfo damageInfo)
        {
            if (Time.time - _lastReflectionTime < _reflectionCooldown)
                return;
                
            // Get attacker
            GameObject attacker = damageInfo.Source;
            if (attacker == null)
                return;
                
            // Calculate reflection damage
            float reflectedDamage = damageInfo.FinalAmount * reflectionPercentage;
            reflectedDamage = Mathf.Min(reflectedDamage, maxReflectionDamage);
            
            // Track damage by source
            if (_recentDamageBySource.ContainsKey(attacker))
            {
                _recentDamageBySource[attacker] += reflectedDamage;
            }
            else
            {
                _recentDamageBySource[attacker] = reflectedDamage;
            }
            
            // Reflect damage
            if (reflectToAll)
            {
                ReflectDamageToArea(reflectedDamage, damageInfo.DamageType);
            }
            else
            {
                ReflectDamageToAttacker(attacker, reflectedDamage, damageInfo.DamageType);
            }
            
            // Update cooldown
            _lastReflectionTime = Time.time;
        }
        
        // Reflect damage back to the attacker
        private void ReflectDamageToAttacker(GameObject attacker, float damage, DamageType damageType)
        {
            // Check if attacker is valid
            DamageableEntity attackerEntity = attacker.GetComponent<DamageableEntity>();
            if (attackerEntity == null)
                return;
                
            // Apply reflected damage
            attackerEntity.TakeDamage(damage, damageType, gameObject);
            
            // Create reflection effect
            CreateReflectionEffect(attacker.transform.position);
            
            Debug.Log($"Reflected {damage} damage back to {attacker.name}");
        }
        
        // Reflect damage to all enemies in an area
        private void ReflectDamageToArea(float damage, DamageType damageType)
        {
            // Find all entities in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, reflectionRadius);
            
            foreach (var collider in colliders)
            {
                // Skip self
                if (collider.gameObject == gameObject)
                    continue;
                    
                // Check if it's an enemy
                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    // Apply reflected damage
                    enemy.TakeDamage(damage, damageType, gameObject);
                    
                    // Create reflection effect
                    CreateReflectionEffect(enemy.transform.position);
                }
            }
        }
        
        // Create visual effect for reflection
        private void CreateReflectionEffect(Vector3 targetPosition)
        {
            // Create VFX if available
            if (reflectionVFXPrefab != null)
            {
                GameObject vfx = Instantiate(reflectionVFXPrefab, transform.position, Quaternion.identity);
                
                // Create effect moving from player to target
                StartCoroutine(AnimateReflectionEffect(vfx, targetPosition));
            }
            
            // Play sound if available
            if (reflectionSound != null)
            {
                AudioSource.PlayClipAtPoint(reflectionSound, transform.position);
            }
        }
        
        // Animate the reflection effect
        private System.Collections.IEnumerator AnimateReflectionEffect(GameObject effectObj, Vector3 targetPosition)
        {
            if (effectObj == null) yield break;
            
            float duration = 0.5f;
            float startTime = Time.time;
            Vector3 startPosition = transform.position;
            
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                effectObj.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            
            // Create impact effect at target
            CreateImpactEffect(targetPosition);
            
            // Destroy the effect object
            Destroy(effectObj);
        }
        
        // Create impact effect at target position
        private void CreateImpactEffect(Vector3 position)
        {
            GameObject impactObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            impactObj.transform.position = position;
            impactObj.transform.localScale = Vector3.one * 0.5f;
            
            // Remove collider
            Destroy(impactObj.GetComponent<Collider>());
            
            // Set material
            Renderer renderer = impactObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Particles/Additive"));
                material.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Gray for balance
                renderer.material = material;
            }
            
            // Expand and fade out
            StartCoroutine(ExpandAndFade(impactObj));
        }
        
        // Expand and fade effect
        private System.Collections.IEnumerator ExpandAndFade(GameObject obj)
        {
            if (obj == null) yield break;
            
            float duration = 0.5f;
            float startTime = Time.time;
            
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                float scale = Mathf.Lerp(0.5f, 2f, t);
                obj.transform.localScale = Vector3.one * scale;
                
                // Fade out towards the end
                if (t > 0.5f)
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Color color = renderer.material.color;
                        color.a = 1f - ((t - 0.5f) * 2f);
                        renderer.material.color = color;
                    }
                }
                
                yield return null;
            }
            
            // Destroy object
            Destroy(obj);
        }
        
        // Clean up old entries
        private void Update()
        {
            // Clear dictionary periodically
            if (_recentDamageBySource.Count > 0 && Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
            {
                _recentDamageBySource.Clear();
            }
        }
        
        // Get total reflected damage
        public float GetTotalReflectedDamage()
        {
            float total = 0f;
            foreach (var damage in _recentDamageBySource.Values)
            {
                total += damage;
            }
            
            return total;
        }
    }
}