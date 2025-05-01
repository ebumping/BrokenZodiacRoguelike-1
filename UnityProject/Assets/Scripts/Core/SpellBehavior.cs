using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    // Base class for all spell behaviors
    public abstract class SpellBehavior : MonoBehaviour
    {
        // References
        protected Spell _spell;
        protected PlayerController _caster;
        protected Vector3 _targetPosition;
        protected GameObject _targetObject;
        protected int _spellLevel;
        protected SpellModifiers _modifiers;
        
        // State
        protected float _startTime;
        protected float _lifetime;
        protected bool _isFinished = false;
        protected List<GameObject> _hitTargets = new List<GameObject>(); // For tracking what's been hit
        
        // Properties
        public bool IsFinished => _isFinished;
        
        // Initialize the spell with all necessary data
        public virtual void Initialize(Spell spell, PlayerController caster, Vector3 targetPosition, 
                                GameObject targetObject, int spellLevel, SpellModifiers modifiers)
        {
            _spell = spell;
            _caster = caster;
            _targetPosition = targetPosition;
            _targetObject = targetObject;
            _spellLevel = spellLevel;
            _modifiers = modifiers;
            
            _startTime = Time.time;
            _lifetime = spell.EffectDuration * modifiers.DurationMultiplier;
            
            OnInitialize();
        }
        
        // Override in derived classes for specific initialization
        protected virtual void OnInitialize() { }
        
        // Called every frame to update the spell
        public virtual void UpdateSpell(float deltaTime)
        {
            // Check for lifetime expiration
            if (_lifetime > 0 && Time.time - _startTime >= _lifetime)
            {
                FinishSpell();
            }
        }
        
        // Process hitting a target
        protected virtual void ProcessHit(GameObject target)
        {
            if (_hitTargets.Contains(target)) return; // Already hit this target
            
            // Add to hit targets if this spell shouldn't hit the same target multiple times
            if (!CanHitTargetMultipleTimes())
            {
                _hitTargets.Add(target);
            }
            
            // Create the spell effect
            SpellEffect effect = CreateSpellEffect();
            
            // Apply effect via SpellManager
            SpellManager.Instance.ApplySpellEffect(effect, target);
            
            // Show impact effect
            SpellManager.Instance.CreateImpactEffect(_spell, target.transform.position, Quaternion.identity);
            
            // Process specific reactions based on the hit target
            OnProcessHit(target);
        }
        
        // Override in derived classes for specific hit processing
        protected virtual void OnProcessHit(GameObject target) { }
        
        // Create a spell effect with damage, healing, and status effects
        protected virtual SpellEffect CreateSpellEffect()
        {
            // Calculate base damage and healing
            float damage = _spell.GetDamage(_spellLevel) * _modifiers.DamageMultiplier;
            float healing = _spell.GetHealing(_spellLevel) * _modifiers.HealingMultiplier;
            
            // Convert status effect data to actual status effects
            List<StatusEffect> statusEffects = new List<StatusEffect>();
            foreach (var effectData in _spell.StatusEffects)
            {
                statusEffects.Add(new StatusEffect
                {
                    Type = effectData.Type,
                    Power = effectData.Power,
                    Duration = effectData.Duration * _modifiers.DurationMultiplier,
                    TickRate = effectData.TickRate,
                    EffectColor = effectData.EffectColor,
                    EffectPrefab = effectData.EffectPrefab,
                    IsBeneficial = IsStatusEffectBeneficial(effectData.Type),
                    SourceSpell = _spell,
                    Caster = _caster
                });
            }
            
            return new SpellEffect
            {
                Caster = _caster,
                SourceSpell = _spell,
                Damage = damage,
                Healing = healing,
                IsFriendly = IsHealingSpell(),
                StatusEffects = statusEffects
            };
        }
        
        // Check if a status effect is beneficial
        protected bool IsStatusEffectBeneficial(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Haste:
                case StatusEffectType.Shield:
                case StatusEffectType.Regeneration:
                case StatusEffectType.ManaFlow:
                case StatusEffectType.Reflect:
                case StatusEffectType.Invulnerable:
                    return true;
                    
                default:
                    return false;
            }
        }
        
        // Check if this is primarily a healing spell
        protected bool IsHealingSpell()
        {
            return _spell.BaseHealing > 0 && _spell.BaseDamage <= 0;
        }
        
        // Check if the spell can hit the same target multiple times
        protected virtual bool CanHitTargetMultipleTimes()
        {
            // Default behavior: area and beam spells can hit multiple times, others cannot
            return _spell.Type == SpellType.Area || _spell.Type == SpellType.Beam || _spell.Type == SpellType.Channel;
        }
        
        // Mark the spell as finished
        protected virtual void FinishSpell()
        {
            _isFinished = true;
            OnFinishSpell();
        }
        
        // Override in derived classes for specific cleanup
        protected virtual void OnFinishSpell() { }
    }
    
    // Implementation for projectile spells (fireballs, magic missiles, etc.)
    public class ProjectileSpellBehavior : SpellBehavior
    {
        private Rigidbody2D _rigidbody;
        private Vector2 _direction;
        private float _speed;
        private float _maxDistance;
        
        protected override void OnInitialize()
        {
            // Get or add a rigidbody
            _rigidbody = GetComponent<Rigidbody2D>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody2D>();
                _rigidbody.gravityScale = 0f;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            
            // Calculate direction
            Vector3 direction = (_targetPosition - transform.position).normalized;
            _direction = new Vector2(direction.x, direction.y);
            
            // Set rotation to face direction
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // Set velocity
            _speed = 10f; // Default speed - should be set in Spell data
            if (_spell != null)
            {
                _speed = _spell.Type == SpellType.Projectile ? 15f : 8f;
            }
            
            _rigidbody.velocity = _direction * _speed;
            
            // Set maximum travel distance
            _maxDistance = _spell.Range * _modifiers.RangeMultiplier;
        }
        
        public override void UpdateSpell(float deltaTime)
        {
            base.UpdateSpell(deltaTime);
            
            // Check for maximum distance traveled
            float distanceTraveled = Vector3.Distance(transform.position, _caster.transform.position);
            if (distanceTraveled > _maxDistance)
            {
                FinishSpell();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Skip caster
            if (other.gameObject == _caster.gameObject) return;
            
            // Process hit
            ProcessHit(other.gameObject);
            
            // Projectiles are typically destroyed on impact, unless they pierce
            bool canPierce = false; // Should be defined by spell properties
            if (!canPierce)
            {
                FinishSpell();
            }
        }
    }
    
    // Implementation for beam spells (lightning, laser, etc.)
    public class BeamSpellBehavior : SpellBehavior
    {
        private LineRenderer _lineRenderer;
        private float _beamWidth;
        private LayerMask _targetLayers;
        private RaycastHit2D[] _hitResults = new RaycastHit2D[10]; // Buffer for raycasts
        private float _tickRate = 0.1f; // How often to check for targets
        private float _lastTickTime;
        
        protected override void OnInitialize()
        {
            // Get or add a line renderer
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
                _lineRenderer.startWidth = 0.5f;
                _lineRenderer.endWidth = 0.5f;
                _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
            
            // Set beam properties
            _beamWidth = 0.5f * _modifiers.AreaMultiplier;
            _lineRenderer.startWidth = _beamWidth;
            _lineRenderer.endWidth = _beamWidth;
            
            // Set color based on element
            SetBeamColor();
            
            // Set target layers
            _targetLayers = LayerMask.GetMask("Enemy", "Environment");
            
            // Set initial positions
            UpdateBeamPositions();
            
            // Set last tick time for periodic damage
            _lastTickTime = Time.time;
        }
        
        private void SetBeamColor()
        {
            Color beamColor = Color.white;
            
            switch (_spell.Element)
            {
                case SpellElement.Fire:
                    beamColor = new Color(1f, 0.5f, 0f);
                    break;
                case SpellElement.Ice:
                    beamColor = new Color(0.5f, 0.8f, 1f);
                    break;
                case SpellElement.Lightning:
                    beamColor = new Color(0.8f, 0.8f, 1f);
                    break;
                case SpellElement.Arcane:
                    beamColor = new Color(0.8f, 0.5f, 1f);
                    break;
                case SpellElement.Light:
                    beamColor = new Color(1f, 1f, 0.8f);
                    break;
                case SpellElement.Shadow:
                    beamColor = new Color(0.3f, 0.3f, 0.4f);
                    break;
                default:
                    beamColor = Color.white;
                    break;
            }
            
            _lineRenderer.startColor = beamColor;
            _lineRenderer.endColor = beamColor;
        }
        
        public override void UpdateSpell(float deltaTime)
        {
            base.UpdateSpell(deltaTime);
            
            // Update beam positions
            UpdateBeamPositions();
            
            // Check for hits periodically
            if (Time.time - _lastTickTime >= _tickRate)
            {
                CheckForHits();
                _lastTickTime = Time.time;
            }
        }
        
        private void UpdateBeamPositions()
        {
            if (_caster == null) return;
            
            // Calculate start and end positions
            Vector3 startPos = transform.position;
            
            // Calculate max beam length
            float maxLength = _spell.Range * _modifiers.RangeMultiplier;
            
            // Direction from caster to target
            Vector3 direction = (_targetPosition - startPos).normalized;
            
            // Check if the beam hits anything
            int hitCount = Physics2D.RaycastNonAlloc(startPos, direction, _hitResults, maxLength, _targetLayers);
            
            Vector3 endPos;
            if (hitCount > 0)
            {
                // Use the closest hit point
                float closestDist = float.MaxValue;
                int closestHitIndex = -1;
                
                for (int i = 0; i < hitCount; i++)
                {
                    // Skip the caster
                    if (_hitResults[i].collider.gameObject == _caster.gameObject) continue;
                    
                    if (_hitResults[i].distance < closestDist)
                    {
                        closestDist = _hitResults[i].distance;
                        closestHitIndex = i;
                    }
                }
                
                if (closestHitIndex >= 0)
                {
                    endPos = _hitResults[closestHitIndex].point;
                }
                else
                {
                    // No valid hits, use max length
                    endPos = startPos + direction * maxLength;
                }
            }
            else
            {
                // No hits, use max length
                endPos = startPos + direction * maxLength;
            }
            
            // Update line renderer
            _lineRenderer.SetPosition(0, startPos);
            _lineRenderer.SetPosition(1, endPos);
        }
        
        private void CheckForHits()
        {
            if (_lineRenderer == null || _lineRenderer.positionCount < 2) return;
            
            // Get beam start and end positions
            Vector3 startPos = _lineRenderer.GetPosition(0);
            Vector3 endPos = _lineRenderer.GetPosition(1);
            
            // Direction and distance
            Vector3 direction = (endPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, endPos);
            
            // Cast along the beam
            int hitCount = Physics2D.RaycastNonAlloc(startPos, direction, _hitResults, distance, _targetLayers);
            
            for (int i = 0; i < hitCount; i++)
            {
                // Skip the caster
                if (_hitResults[i].collider.gameObject == _caster.gameObject) continue;
                
                // Process hit
                ProcessHit(_hitResults[i].collider.gameObject);
            }
        }
    }
    
    // Implementation for area effect spells (explosions, frost nova, etc.)
    public class AreaSpellBehavior : SpellBehavior
    {
        private float _areaRadius;
        private float _expansionRate = 10f; // How fast the area expands
        private float _currentRadius = 0f;
        private bool _expandOverTime = true;
        private bool _damageOverTime = false;
        private float _tickRate = 0.5f; // For damage-over-time areas
        private float _lastTickTime;
        
        // Visual elements
        private SpriteRenderer _areaVisual;
        
        protected override void OnInitialize()
        {
            // Set area properties
            _areaRadius = _spell.AreaOfEffect * _modifiers.AreaMultiplier;
            
            // Position at target location
            transform.position = _targetPosition;
            
            // Setup visuals
            SetupVisuals();
            
            // Set expansion behavior based on spell type
            SetExpansionBehavior();
            
            // Initialize tick time for damage-over-time areas
            _lastTickTime = Time.time;
        }
        
        private void SetupVisuals()
        {
            // Get or add a sprite renderer
            _areaVisual = GetComponentInChildren<SpriteRenderer>();
            if (_areaVisual == null)
            {
                // Create a child object for the visual
                GameObject visualObj = new GameObject("AreaVisual");
                visualObj.transform.SetParent(transform);
                visualObj.transform.localPosition = Vector3.zero;
                
                _areaVisual = visualObj.AddComponent<SpriteRenderer>();
                
                // Create a circle sprite dynamically
                _areaVisual.sprite = CreateCircleSprite();
            }
            
            // Set initial scale
            _areaVisual.transform.localScale = Vector3.zero;
            
            // Set color based on element
            SetAreaColor();
        }
        
        private Sprite CreateCircleSprite()
        {
            // Create a simple circle texture
            int resolution = 128;
            Texture2D texture = new Texture2D(resolution, resolution);
            texture.filterMode = FilterMode.Bilinear;
            
            Color[] colors = new Color[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float distFromCenter = Vector2.Distance(new Vector2(x, y), 
                                            new Vector2(resolution / 2, resolution / 2));
                    float normalizedDist = distFromCenter / (resolution / 2);
                    
                    // Create a nice falloff at the edge
                    float alphaValue = normalizedDist <= 0.8f ? 0.8f : 1f - (normalizedDist - 0.8f) * 5f;
                    alphaValue = Mathf.Clamp01(alphaValue);
                    
                    colors[y * resolution + x] = new Color(1f, 1f, 1f, alphaValue);
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), 
                        new Vector2(0.5f, 0.5f), resolution / 2f);
        }
        
        private void SetAreaColor()
        {
            if (_areaVisual == null) return;
            
            Color areaColor = Color.white;
            
            switch (_spell.Element)
            {
                case SpellElement.Fire:
                    areaColor = new Color(1f, 0.5f, 0f, 0.7f);
                    break;
                case SpellElement.Ice:
                    areaColor = new Color(0.5f, 0.8f, 1f, 0.7f);
                    break;
                case SpellElement.Lightning:
                    areaColor = new Color(0.8f, 0.8f, 1f, 0.7f);
                    break;
                case SpellElement.Earth:
                    areaColor = new Color(0.6f, 0.4f, 0.2f, 0.7f);
                    break;
                case SpellElement.Water:
                    areaColor = new Color(0.3f, 0.5f, 1f, 0.7f);
                    break;
                case SpellElement.Wind:
                    areaColor = new Color(0.9f, 0.9f, 1f, 0.5f);
                    break;
                case SpellElement.Light:
                    areaColor = new Color(1f, 1f, 0.8f, 0.7f);
                    break;
                case SpellElement.Shadow:
                    areaColor = new Color(0.3f, 0.3f, 0.4f, 0.7f);
                    break;
                default:
                    areaColor = new Color(1f, 1f, 1f, 0.7f);
                    break;
            }
            
            _areaVisual.color = areaColor;
        }
        
        private void SetExpansionBehavior()
        {
            // Determine behavior based on spell properties
            // For example, an explosion expands quickly then disappears
            // A persistent AOE might expand to full size and stay
            
            switch (_spell.Element)
            {
                case SpellElement.Fire:
                    _expandOverTime = true;
                    _expansionRate = 15f;
                    _damageOverTime = true;
                    _tickRate = 0.5f;
                    break;
                    
                case SpellElement.Ice:
                    _expandOverTime = true;
                    _expansionRate = 10f;
                    _damageOverTime = true;
                    _tickRate = 1f;
                    break;
                    
                case SpellElement.Lightning:
                    _expandOverTime = true;
                    _expansionRate = 25f;
                    _damageOverTime = false;
                    break;
                    
                default:
                    _expandOverTime = true;
                    _expansionRate = 10f;
                    _damageOverTime = false;
                    break;
            }
        }
        
        public override void UpdateSpell(float deltaTime)
        {
            base.UpdateSpell(deltaTime);
            
            // Update area size
            if (_expandOverTime && _currentRadius < _areaRadius)
            {
                _currentRadius = Mathf.Min(_currentRadius + _expansionRate * deltaTime, _areaRadius);
                
                // Update visual scale
                if (_areaVisual != null)
                {
                    float visualScale = _currentRadius * 2f; // Double because it's diameter
                    _areaVisual.transform.localScale = new Vector3(visualScale, visualScale, 1);
                }
                
                // Check for hits as we expand
                CheckForHits();
            }
            
            // Check for periodic damage for persistent areas
            if (_damageOverTime && Time.time - _lastTickTime >= _tickRate)
            {
                CheckForHits();
                _lastTickTime = Time.time;
            }
        }
        
        private void CheckForHits()
        {
            // Find all colliders within the area
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _currentRadius);
            
            foreach (var collider in colliders)
            {
                // Skip the caster
                if (collider.gameObject == _caster.gameObject) continue;
                
                // Process hit
                ProcessHit(collider.gameObject);
            }
        }
    }
    
    // Default implementation for other spell types
    public class DefaultSpellBehavior : SpellBehavior
    {
        protected override void OnInitialize()
        {
            // Basic initialization
            transform.position = _targetPosition;
        }
        
        public override void UpdateSpell(float deltaTime)
        {
            base.UpdateSpell(deltaTime);
            
            // Basic behavior - just exist for the duration
        }
    }
}