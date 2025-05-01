using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Core
{
    // Class for the Aquarius Zodiac Sign's slowing puddle effect
    public class SlowingPuddle : MonoBehaviour
    {
        [SerializeField] private float duration = 3.0f;
        [SerializeField] private float radius = 3.0f;
        [SerializeField] private float slowAmount = 0.3f; // 30% slow
        [SerializeField] private LayerMask affectedLayers;
        [SerializeField] private bool affectEnemiesOnly = true;
        [SerializeField] private ParticleSystem puddleParticles;
        [SerializeField] private AudioSource audioSource;
        
        private float _remainingDuration;
        private HashSet<int> _affectedEntityIds = new HashSet<int>();
        private List<SlowEffect> _activeSlowEffects = new List<SlowEffect>();
        
        private void OnEnable()
        {
            _remainingDuration = duration;
            
            // Scale the visuals to match the radius
            transform.localScale = new Vector3(radius * 2, 1, radius * 2);
            
            // Start the puddle effect
            if (puddleParticles != null)
            {
                var main = puddleParticles.main;
                main.duration = duration;
                puddleParticles.Play();
            }
            
            // Play sound if available
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
        
        private void Update()
        {
            // Update remaining duration
            _remainingDuration -= Time.deltaTime;
            
            // Check for new entities to affect
            CheckForEntities();
            
            // Update existing slow effects
            UpdateSlowEffects();
            
            // Scale down effect near the end
            if (_remainingDuration < 1.0f)
            {
                float scale = Mathf.Lerp(0, radius * 2, _remainingDuration);
                transform.localScale = new Vector3(scale, transform.localScale.y, scale);
            }
            
            // Destroy when duration expires
            if (_remainingDuration <= 0)
            {
                Destroy(gameObject);
            }
        }
        
        private void CheckForEntities()
        {
            // Find all entities within the radius
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, affectedLayers);
            
            foreach (var hitCollider in hitColliders)
            {
                // Skip if already affected
                int entityId = hitCollider.GetInstanceID();
                if (_affectedEntityIds.Contains(entityId))
                    continue;
                
                // Check if we should affect this entity
                if (affectEnemiesOnly)
                {
                    EnemyController enemy = hitCollider.GetComponent<EnemyController>();
                    if (enemy == null)
                        continue; // Skip non-enemies
                    
                    // Apply slow to enemy
                    ApplySlowToEnemy(enemy, entityId);
                }
                else
                {
                    // Could affect both players and enemies
                    EnemyController enemy = hitCollider.GetComponent<EnemyController>();
                    if (enemy != null)
                    {
                        ApplySlowToEnemy(enemy, entityId);
                        continue;
                    }
                    
                    PlayerController player = hitCollider.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        ApplySlowToPlayer(player, entityId);
                    }
                }
            }
        }
        
        private void ApplySlowToEnemy(EnemyController enemy, int entityId)
        {
            if (enemy == null) return;
            
            // Remember we've affected this entity
            _affectedEntityIds.Add(entityId);
            
            // Apply slow effect
            enemy.ModifySpeed(-slowAmount);
            
            // Track the effect so we can undo it later
            _activeSlowEffects.Add(new SlowEffect
            {
                EntityId = entityId,
                Entity = enemy.gameObject,
                IsEnemy = true,
                SlowAmount = slowAmount
            });
        }
        
        private void ApplySlowToPlayer(PlayerController player, int entityId)
        {
            if (player == null) return;
            
            // Remember we've affected this entity
            _affectedEntityIds.Add(entityId);
            
            // Apply slow effect
            player.ModifySpeed(-slowAmount);
            
            // Track the effect so we can undo it later
            _activeSlowEffects.Add(new SlowEffect
            {
                EntityId = entityId,
                Entity = player.gameObject,
                IsEnemy = false,
                SlowAmount = slowAmount
            });
        }
        
        private void UpdateSlowEffects()
        {
            // Check if any affected entities have left the puddle
            for (int i = _activeSlowEffects.Count - 1; i >= 0; i--)
            {
                SlowEffect effect = _activeSlowEffects[i];
                
                // Skip if entity no longer exists
                if (effect.Entity == null)
                {
                    _activeSlowEffects.RemoveAt(i);
                    _affectedEntityIds.Remove(effect.EntityId);
                    continue;
                }
                
                // Check if entity is still in range
                float distance = Vector3.Distance(transform.position, effect.Entity.transform.position);
                if (distance > radius)
                {
                    // Remove slow effect
                    if (effect.IsEnemy)
                    {
                        EnemyController enemy = effect.Entity.GetComponent<EnemyController>();
                        if (enemy != null)
                        {
                            enemy.ModifySpeed(effect.SlowAmount); // Undo the slow
                        }
                    }
                    else
                    {
                        PlayerController player = effect.Entity.GetComponent<PlayerController>();
                        if (player != null)
                        {
                            player.ModifySpeed(effect.SlowAmount); // Undo the slow
                        }
                    }
                    
                    // Remove from tracking
                    _activeSlowEffects.RemoveAt(i);
                    _affectedEntityIds.Remove(effect.EntityId);
                }
            }
        }
        
        private void OnDestroy()
        {
            // Remove all slow effects
            foreach (var effect in _activeSlowEffects)
            {
                if (effect.Entity != null)
                {
                    if (effect.IsEnemy)
                    {
                        EnemyController enemy = effect.Entity.GetComponent<EnemyController>();
                        if (enemy != null)
                        {
                            enemy.ModifySpeed(effect.SlowAmount); // Undo the slow
                        }
                    }
                    else
                    {
                        PlayerController player = effect.Entity.GetComponent<PlayerController>();
                        if (player != null)
                        {
                            player.ModifySpeed(effect.SlowAmount); // Undo the slow
                        }
                    }
                }
            }
            
            _activeSlowEffects.Clear();
            _affectedEntityIds.Clear();
        }
        
        // Initialize puddle parameters
        public void Initialize(float newDuration, float newRadius, float newSlowAmount)
        {
            duration = newDuration;
            radius = newRadius;
            slowAmount = newSlowAmount;
            _remainingDuration = duration;
            
            // Update the scale
            transform.localScale = new Vector3(radius * 2, 1, radius * 2);
            
            // Update particle settings if available
            if (puddleParticles != null)
            {
                var main = puddleParticles.main;
                main.duration = duration;
                
                // Adjust particle system shape to match radius
                var shape = puddleParticles.shape;
                if (shape.shapeType == ParticleSystemShapeType.Circle ||
                    shape.shapeType == ParticleSystemShapeType.Cone)
                {
                    shape.radius = radius;
                }
            }
        }
        
        // Draw gizmo for puddle radius
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, radius);
        }
        
        // Struct to track active slow effects
        private struct SlowEffect
        {
            public int EntityId;
            public GameObject Entity;
            public bool IsEnemy;
            public float SlowAmount;
        }
    }
}