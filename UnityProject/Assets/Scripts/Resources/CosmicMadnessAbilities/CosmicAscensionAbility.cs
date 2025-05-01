using UnityEngine;
using System.Collections;
using CodexOfTheBrokenZodiac.Core;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources.CosmicMadnessAbilities
{
    // Ultimate ability that grants player transcendence and vastly increased power
    [CreateAssetMenu(fileName = "Cosmic Ascension", menuName = "Codex/Cosmic Madness/Abilities/Cosmic Ascension")]
    public class CosmicAscensionAbility : CosmicMadnessAbility
    {
        [Header("Ascension Settings")]
        [SerializeField] private float ascensionDuration = 20f;
        [SerializeField] private float ascensionCooldown = 300f; // 5 minutes
        [SerializeField] private float sanityCost = 50f;
        [SerializeField] private bool requireFullSanity = false;
        
        [Header("Power Amplification")]
        [SerializeField] private float damageMultiplier = 3f;
        [SerializeField] private float speedMultiplier = 1.5f;
        [SerializeField] private float fireRateMultiplier = 2f;
        [SerializeField] private float projectileSizeMultiplier = 1.5f;
        [SerializeField] private bool grantInvulnerability = true;
        [SerializeField] private bool removeResourceCosts = true; // No mana/ammo costs
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject ascensionVFXPrefab;
        [SerializeField] private Material ascensionMaterial;
        [SerializeField] private AudioClip ascensionSound;
        [SerializeField] private AudioClip ascensionAmbientLoop;
        [SerializeField] private Color ascensionAura = new Color(1f, 1f, 1f, 0.7f);
        [SerializeField] private float auraSize = 3f;
        [SerializeField] private float auraPulseSpeed = 1f;
        
        [Header("Zodiac Enhancements")]
        [SerializeField] private ZodiacAscensionEnhancement[] zodiacEnhancements;
        
        // Runtime variables
        private CosmicAscensionComponent _ascensionComponent;
        
        // Apply the ascension ability
        protected override void ApplyEffect(PlayerController player)
        {
            if (player == null) return;
            
            // Add ascension component if not already present
            _ascensionComponent = player.gameObject.GetComponent<CosmicAscensionComponent>();
            if (_ascensionComponent == null)
            {
                _ascensionComponent = player.gameObject.AddComponent<CosmicAscensionComponent>();
            }
            
            // Get zodiac-specific enhancement
            ZodiacSign playerSign = player.GetZodiacSign();
            ZodiacAscensionEnhancement enhancement = GetZodiacEnhancement(playerSign);
            
            // Initialize the component
            _ascensionComponent.Initialize(
                ascensionDuration,
                ascensionCooldown,
                sanityCost,
                requireFullSanity,
                damageMultiplier,
                speedMultiplier,
                fireRateMultiplier,
                projectileSizeMultiplier,
                grantInvulnerability,
                removeResourceCosts,
                ascensionVFXPrefab,
                ascensionMaterial,
                ascensionSound,
                ascensionAmbientLoop,
                ascensionAura,
                auraSize,
                auraPulseSpeed,
                enhancement
            );
            
            // Enable the ability
            _ascensionComponent.EnableAscension();
        }
        
        // Remove the ascension ability
        protected override void RemoveEffect(PlayerController player)
        {
            if (player == null) return;
            
            // Disable ascension component
            CosmicAscensionComponent ascensionComponent = player.gameObject.GetComponent<CosmicAscensionComponent>();
            if (ascensionComponent != null)
            {
                ascensionComponent.DisableAscension();
            }
        }
        
        // Get zodiac-specific enhancement
        private ZodiacAscensionEnhancement GetZodiacEnhancement(ZodiacSign sign)
        {
            if (zodiacEnhancements == null)
                return null;
                
            foreach (var enhancement in zodiacEnhancements)
            {
                if (enhancement.ZodiacSign == sign)
                    return enhancement;
            }
            
            return null; // No specific enhancement found
        }
        
        // Return zodiac-specific details
        protected override string GetZodiacSpecificDetails(ZodiacSign sign)
        {
            ZodiacAscensionEnhancement enhancement = GetZodiacEnhancement(sign);
            if (enhancement == null)
                return "";
                
            return $"{enhancement.EnhancementName}: {enhancement.Description}";
        }
    }
    
    // Data structure for zodiac-specific ascension enhancements
    [System.Serializable]
    public class ZodiacAscensionEnhancement
    {
        [SerializeField] private ZodiacSign zodiacSign;
        [SerializeField] private string enhancementName;
        [SerializeField] private string description;
        
        [Header("Enhancement Type")]
        [SerializeField] private AscensionEnhancementType enhancementType;
        
        [Header("Beam Enhancement")]
        [SerializeField] private float beamDamagePerSecond = 100f;
        [SerializeField] private float beamRange = 20f;
        [SerializeField] private float beamWidth = 1f;
        [SerializeField] private DamageType beamDamageType = DamageType.Void;
        [SerializeField] private Color beamColor = Color.white;
        
        [Header("Time Enhancement")]
        [SerializeField] private float timeSlowFactor = 0.3f;
        [SerializeField] private float timeSlowRadius = 15f;
        [SerializeField] private bool affectProjectiles = true;
        
        [Header("Minion Enhancement")]
        [SerializeField] private int minionCount = 4;
        [SerializeField] private float minionDamageMultiplier = 2f;
        [SerializeField] private float minionHealthMultiplier = 2f;
        [SerializeField] private bool minionsSacrificeOnDamage = true;
        
        [Header("Elemental Enhancement")]
        [SerializeField] private ElementType elementType;
        [SerializeField] private float elementalDamageMultiplier = 2f;
        [SerializeField] private float elementalRadiusMultiplier = 1.5f;
        [SerializeField] private float elementalDuration = 8f;
        
        // Properties
        public ZodiacSign ZodiacSign => zodiacSign;
        public string EnhancementName => enhancementName;
        public string Description => description;
        public AscensionEnhancementType EnhancementType => enhancementType;
        public float BeamDamagePerSecond => beamDamagePerSecond;
        public float BeamRange => beamRange;
        public float BeamWidth => beamWidth;
        public DamageType BeamDamageType => beamDamageType;
        public Color BeamColor => beamColor;
        public float TimeSlowFactor => timeSlowFactor;
        public float TimeSlowRadius => timeSlowRadius;
        public bool AffectProjectiles => affectProjectiles;
        public int MinionCount => minionCount;
        public float MinionDamageMultiplier => minionDamageMultiplier;
        public float MinionHealthMultiplier => minionHealthMultiplier;
        public bool MinionsSacrificeOnDamage => minionsSacrificeOnDamage;
        public ElementType ElementType => elementType;
        public float ElementalDamageMultiplier => elementalDamageMultiplier;
        public float ElementalRadiusMultiplier => elementalRadiusMultiplier;
        public float ElementalDuration => elementalDuration;
    }
    
    // Types of ascension enhancements
    public enum AscensionEnhancementType
    {
        DestructionBeam,   // Aries, Leo, Scorpio
        TimeManipulation,  // Capricorn, Aquarius, Pisces
        MinionSummoning,   // Cancer, Gemini, Virgo
        ElementalMastery   // Taurus, Libra, Sagittarius
    }
    
    // Elemental types
    public enum ElementType
    {
        Fire,
        Ice,
        Lightning,
        Void,
        Cosmic
    }
    
    // Component that handles the cosmic ascension
    public class CosmicAscensionComponent : MonoBehaviour
    {
        // Configuration
        private float _ascensionDuration;
        private float _ascensionCooldown;
        private float _sanityCost;
        private bool _requireFullSanity;
        private float _damageMultiplier;
        private float _speedMultiplier;
        private float _fireRateMultiplier;
        private float _projectileSizeMultiplier;
        private bool _grantInvulnerability;
        private bool _removeResourceCosts;
        private GameObject _ascensionVFXPrefab;
        private Material _ascensionMaterial;
        private AudioClip _ascensionSound;
        private AudioClip _ascensionAmbientLoop;
        private Color _ascensionAura;
        private float _auraSize;
        private float _auraPulseSpeed;
        private ZodiacAscensionEnhancement _zodiacEnhancement;
        
        // Runtime variables
        private bool _isEnabled = false;
        private bool _isAscended = false;
        private float _lastAscensionTime = -999f;
        private List<GameObject> _activeEffects = new List<GameObject>();
        private AudioSource _audioSource;
        private AudioSource _ambientAudioSource;
        private PlayerController _player;
        private DamageModifier _damageMod;
        private List<DamageModifier> _activeModifiers = new List<DamageModifier>();
        private Renderer[] _renderers;
        private Material[] _originalMaterials;
        private GameObject _auraEffect;
        private LineRenderer _beamRenderer;
        private float _beamDamageCounter = 0f;
        private List<GameObject> _summonedMinions = new List<GameObject>();
        private GameObject _timeSlowEffect;
        private List<TimeSlowedEntity> _slowedEntities = new List<TimeSlowedEntity>();
        private GameObject _elementalEffect;
        
        // Initialize the component
        public void Initialize(
            float ascensionDuration,
            float ascensionCooldown,
            float sanityCost,
            bool requireFullSanity,
            float damageMultiplier,
            float speedMultiplier,
            float fireRateMultiplier,
            float projectileSizeMultiplier,
            bool grantInvulnerability,
            bool removeResourceCosts,
            GameObject ascensionVFXPrefab,
            Material ascensionMaterial,
            AudioClip ascensionSound,
            AudioClip ascensionAmbientLoop,
            Color ascensionAura,
            float auraSize,
            float auraPulseSpeed,
            ZodiacAscensionEnhancement zodiacEnhancement)
        {
            _ascensionDuration = ascensionDuration;
            _ascensionCooldown = ascensionCooldown;
            _sanityCost = sanityCost;
            _requireFullSanity = requireFullSanity;
            _damageMultiplier = damageMultiplier;
            _speedMultiplier = speedMultiplier;
            _fireRateMultiplier = fireRateMultiplier;
            _projectileSizeMultiplier = projectileSizeMultiplier;
            _grantInvulnerability = grantInvulnerability;
            _removeResourceCosts = removeResourceCosts;
            _ascensionVFXPrefab = ascensionVFXPrefab;
            _ascensionMaterial = ascensionMaterial;
            _ascensionSound = ascensionSound;
            _ascensionAmbientLoop = ascensionAmbientLoop;
            _ascensionAura = ascensionAura;
            _auraSize = auraSize;
            _auraPulseSpeed = auraPulseSpeed;
            _zodiacEnhancement = zodiacEnhancement;
            
            // Get player reference
            _player = GetComponent<PlayerController>();
            
            // Create audio sources
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 1f;
                _audioSource.volume = 0.8f;
            }
            
            if (_ambientAudioSource == null)
            {
                GameObject audioObj = new GameObject("AmbientAudio");
                audioObj.transform.SetParent(transform);
                audioObj.transform.localPosition = Vector3.zero;
                
                _ambientAudioSource = audioObj.AddComponent<AudioSource>();
                _ambientAudioSource.spatialBlend = 1f;
                _ambientAudioSource.volume = 0.4f;
                _ambientAudioSource.loop = true;
            }
            
            // Cache renderer references
            _renderers = GetComponentsInChildren<Renderer>();
            _originalMaterials = new Material[_renderers.Length];
            
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _originalMaterials[i] = _renderers[i].material;
                }
            }
        }
        
        private void Update()
        {
            if (!_isEnabled) return;
            
            // Check for ascension activation input
            if (Input.GetKeyDown(KeyCode.Z) && CanAscend())
            {
                StartCoroutine(Ascend());
            }
            
            // Update ascension effects
            if (_isAscended)
            {
                UpdateAscensionEffects();
            }
        }
        
        // Check if ascension is available
        private bool CanAscend()
        {
            if (_isAscended) return false;
            
            // Check cooldown
            if (Time.time - _lastAscensionTime < _ascensionCooldown)
                return false;
                
            // Check sanity requirements
            if (_player != null && _requireFullSanity)
            {
                float sanityPercentage = SanitySystem.Instance?.GetPlayerSanityPercentage(_player.NetworkId) ?? 0f;
                if (sanityPercentage < 0.9f) // Require 90%+ sanity
                    return false;
            }
            
            return true;
        }
        
        // Enable ascension ability
        public void EnableAscension()
        {
            _isEnabled = true;
            
            // Add UI notification
            string enhancementName = _zodiacEnhancement != null ? _zodiacEnhancement.EnhancementName : "Cosmic Ascension";
            GameManager.Instance?.ShowNotification($"Cosmic Madness: {enhancementName} available. Press Z to ascend to cosmic form.");
        }
        
        // Disable ascension ability
        public void DisableAscension()
        {
            _isEnabled = false;
            
            // Revert if ascended
            if (_isAscended)
            {
                StopAllCoroutines();
                EndAscension();
            }
        }
        
        // Ascend to cosmic form
        private IEnumerator Ascend()
        {
            if (_isAscended || _player == null)
                yield break;
                
            // Consume sanity
            if (_sanityCost > 0)
            {
                SanitySystem.Instance?.ModifyPlayerSanity(_player.NetworkId, -_sanityCost);
            }
            
            // Show notification
            string enhancementName = _zodiacEnhancement != null ? _zodiacEnhancement.EnhancementName : "Cosmic Ascension";
            GameManager.Instance?.ShowNotification($"Ascending to {enhancementName} form!");
            
            // Play ascension effect
            if (_ascensionVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_ascensionVFXPrefab, transform.position, Quaternion.identity);
                vfx.transform.SetParent(transform);
                vfx.transform.localPosition = Vector3.zero;
                _activeEffects.Add(vfx);
            }
            
            // Play ascension sound
            if (_ascensionSound != null && _audioSource != null)
            {
                _audioSource.clip = _ascensionSound;
                _audioSource.Play();
            }
            
            // Freeze player during transformation
            if (_player != null)
            {
                _player.SetMovementEnabled(false);
            }
            
            // Wait for ascension animation
            yield return new WaitForSeconds(2f);
            
            // Enable movement
            if (_player != null)
            {
                _player.SetMovementEnabled(true);
            }
            
            // Apply ascension effects
            ApplyAscensionEffects();
            
            // Update state
            _isAscended = true;
            _lastAscensionTime = Time.time;
            
            // Start ambient sound
            if (_ascensionAmbientLoop != null && _ambientAudioSource != null)
            {
                _ambientAudioSource.clip = _ascensionAmbientLoop;
                _ambientAudioSource.Play();
            }
            
            // Schedule automatic end
            StartCoroutine(AutoEndAscension());
        }
        
        // End ascension state
        private void EndAscension()
        {
            if (!_isAscended) return;
            
            // Remove ascension effects
            RemoveAscensionEffects();
            
            // Play reversion effect
            if (_ascensionVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_ascensionVFXPrefab, transform.position, Quaternion.identity);
                
                // Different color for reversion
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = new Color(0.5f, 0.5f, 1f);
                }
                
                // Destroy after effect
                Destroy(vfx, 3f);
            }
            
            // Play sound
            if (_ascensionSound != null && _audioSource != null)
            {
                _audioSource.pitch = 0.8f; // Lower pitch for reversion
                _audioSource.PlayOneShot(_ascensionSound);
                _audioSource.pitch = 1f;
            }
            
            // Update state
            _isAscended = false;
            
            // Show notification
            GameManager.Instance?.ShowNotification("Returning to normal form.");
        }
        
        // Automatically end ascension after duration
        private IEnumerator AutoEndAscension()
        {
            yield return new WaitForSeconds(_ascensionDuration);
            EndAscension();
        }
        
        // Apply ascension effects
        private void ApplyAscensionEffects()
        {
            // Apply stat modifications
            if (_player != null)
            {
                // Damage multiplier
                if (_damageMultiplier > 1f)
                {
                    DamageModifier mod = new DamageModifier
                    {
                        ModifierType = ModifierType.Outgoing,
                        Value = _damageMultiplier,
                        IsMultiplier = true
                    };
                    
                    _player.AddDamageModifier(mod);
                    _activeModifiers.Add(mod);
                }
                
                // Speed multiplier
                if (_speedMultiplier > 1f)
                {
                    float speedBonus = _player.MoveSpeed * (_speedMultiplier - 1f);
                    _player.ModifySpeed(speedBonus);
                }
                
                // Fire rate multiplier
                if (_fireRateMultiplier > 1f)
                {
                    _player.ModifyFireRate(_fireRateMultiplier);
                }
                
                // Projectile size multiplier
                if (_projectileSizeMultiplier > 1f)
                {
                    _player.ModifyProjectileSize(_projectileSizeMultiplier);
                }
                
                // Invulnerability
                if (_grantInvulnerability)
                {
                    DamageModifier mod = new DamageModifier
                    {
                        ModifierType = ModifierType.Incoming,
                        Value = 0f,
                        IsMultiplier = true
                    };
                    
                    _player.AddDamageModifier(mod);
                    _activeModifiers.Add(mod);
                }
                
                // Remove resource costs
                if (_removeResourceCosts)
                {
                    _player.SetResourceCostMultiplier(0f);
                }
            }
            
            // Apply visual effects
            ApplyVisualEffects();
            
            // Apply zodiac-specific enhancement
            if (_zodiacEnhancement != null)
            {
                ApplyZodiacEnhancement();
            }
        }
        
        // Apply visual effects
        private void ApplyVisualEffects()
        {
            // Apply material change
            if (_ascensionMaterial != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null)
                    {
                        _renderers[i].material = _ascensionMaterial;
                    }
                }
            }
            
            // Create aura effect
            if (_auraSize > 0)
            {
                _auraEffect = new GameObject("AscensionAura");
                _auraEffect.transform.SetParent(transform);
                _auraEffect.transform.localPosition = Vector3.zero;
                
                // Add sphere mesh and renderer
                MeshFilter meshFilter = _auraEffect.AddComponent<MeshFilter>();
                meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                
                MeshRenderer renderer = _auraEffect.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
                renderer.material.color = _ascensionAura;
                
                // Set size
                _auraEffect.transform.localScale = Vector3.one * _auraSize * 2f;
                
                // Add to active effects
                _activeEffects.Add(_auraEffect);
            }
        }
        
        // Apply zodiac-specific enhancement
        private void ApplyZodiacEnhancement()
        {
            if (_zodiacEnhancement == null)
                return;
                
            switch (_zodiacEnhancement.EnhancementType)
            {
                case AscensionEnhancementType.DestructionBeam:
                    CreateDestructionBeam();
                    break;
                    
                case AscensionEnhancementType.TimeManipulation:
                    CreateTimeSlowField();
                    break;
                    
                case AscensionEnhancementType.MinionSummoning:
                    SummonMinions();
                    break;
                    
                case AscensionEnhancementType.ElementalMastery:
                    CreateElementalEffect();
                    break;
            }
        }
        
        // Create destruction beam
        private void CreateDestructionBeam()
        {
            // Create beam object
            GameObject beamObj = new GameObject("DestructionBeam");
            beamObj.transform.SetParent(transform);
            beamObj.transform.localPosition = new Vector3(0, 1f, 0); // Slightly above player
            
            // Add line renderer
            _beamRenderer = beamObj.AddComponent<LineRenderer>();
            _beamRenderer.startWidth = _zodiacEnhancement.BeamWidth;
            _beamRenderer.endWidth = _zodiacEnhancement.BeamWidth * 0.8f;
            _beamRenderer.material = new Material(Shader.Find("Particles/Additive"));
            _beamRenderer.material.color = _zodiacEnhancement.BeamColor;
            _beamRenderer.positionCount = 2;
            _beamRenderer.enabled = false; // Start disabled
            
            // Add to active effects
            _activeEffects.Add(beamObj);
        }
        
        // Create time slow field
        private void CreateTimeSlowField()
        {
            // Create field visualization
            _timeSlowEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _timeSlowEffect.transform.SetParent(transform);
            _timeSlowEffect.transform.localPosition = Vector3.zero;
            _timeSlowEffect.transform.localScale = Vector3.one * _zodiacEnhancement.TimeSlowRadius * 2f;
            
            // Set material
            Renderer renderer = _timeSlowEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
                renderer.material.color = new Color(0, 0.5f, 1f, 0.2f);
            }
            
            // Remove collider
            Collider collider = _timeSlowEffect.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            
            // Add to active effects
            _activeEffects.Add(_timeSlowEffect);
        }
        
        // Summon minions
        private void SummonMinions()
        {
            int count = _zodiacEnhancement.MinionCount;
            
            for (int i = 0; i < count; i++)
            {
                // Calculate position around player in a circle
                float angle = i * (360f / count) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 2f;
                Vector3 position = transform.position + offset;
                
                // Create minion
                GameObject minionObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                minionObj.transform.position = position;
                minionObj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                
                // Replace sphere collider with a capsule for better collision
                Destroy(minionObj.GetComponent<SphereCollider>());
                CapsuleCollider capsule = minionObj.AddComponent<CapsuleCollider>();
                capsule.radius = 0.35f;
                capsule.height = 1f;
                
                // Add rigidbody
                Rigidbody rb = minionObj.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                
                // Set material
                Renderer renderer = minionObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.color = _ascensionAura;
                    renderer.material.SetColor("_EmissionColor", _ascensionAura * 0.5f);
                    renderer.material.EnableKeyword("_EMISSION");
                }
                
                // Add controller
                AscensionMinionController controller = minionObj.AddComponent<AscensionMinionController>();
                controller.Initialize(
                    _player,
                    _ascensionDuration,
                    _damageMultiplier * _zodiacEnhancement.MinionDamageMultiplier,
                    _zodiacEnhancement.MinionHealthMultiplier,
                    _zodiacEnhancement.MinionsSacrificeOnDamage
                );
                
                // Add to list
                _summonedMinions.Add(minionObj);
                
                // Create spawn effect
                GameObject spawnVfx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spawnVfx.transform.position = position;
                spawnVfx.transform.localScale = Vector3.zero;
                
                // Remove collider
                Destroy(spawnVfx.GetComponent<Collider>());
                
                // Set material
                Renderer vfxRenderer = spawnVfx.GetComponent<Renderer>();
                if (vfxRenderer != null)
                {
                    vfxRenderer.material = new Material(Shader.Find("Particles/Additive"));
                    vfxRenderer.material.color = _ascensionAura;
                }
                
                // Animate spawn
                StartCoroutine(AnimateMinionSpawn(spawnVfx));
            }
        }
        
        // Animate minion spawn effect
        private IEnumerator AnimateMinionSpawn(GameObject spawnVfx)
        {
            if (spawnVfx == null) yield break;
            
            float duration = 0.5f;
            float startTime = Time.time;
            
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                float scale = 2f * (1f - (t * t));
                spawnVfx.transform.localScale = new Vector3(scale, scale, scale);
                
                // Fade out at the end
                Renderer renderer = spawnVfx.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = 1f - t;
                    renderer.material.color = color;
                }
                
                yield return null;
            }
            
            Destroy(spawnVfx);
        }
        
        // Create elemental effect
        private void CreateElementalEffect()
        {
            // Create elemental aura
            _elementalEffect = new GameObject("ElementalEffect");
            _elementalEffect.transform.SetParent(transform);
            _elementalEffect.transform.localPosition = Vector3.zero;
            
            // Add particle system
            ParticleSystem ps = _elementalEffect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 3f;
            main.startSize = 0.5f;
            main.maxParticles = 100;
            
            // Set color based on element type
            Color elementColor = GetElementColor(_zodiacEnhancement.ElementType);
            main.startColor = elementColor;
            
            // Emission
            var emission = ps.emission;
            emission.rateOverTime = 30f;
            
            // Shape
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;
            shape.radiusThickness = 0f; // Emit from surface
            
            // Add to active effects
            _activeEffects.Add(_elementalEffect);
            
            // Create element controller
            ElementalEffectController elemController = _elementalEffect.AddComponent<ElementalEffectController>();
            elemController.Initialize(
                _player,
                _zodiacEnhancement.ElementType,
                _zodiacEnhancement.ElementalDamageMultiplier,
                _zodiacEnhancement.ElementalRadiusMultiplier,
                _zodiacEnhancement.ElementalDuration
            );
        }
        
        // Get element color
        private Color GetElementColor(ElementType type)
        {
            switch (type)
            {
                case ElementType.Fire:
                    return new Color(1f, 0.3f, 0f);
                case ElementType.Ice:
                    return new Color(0f, 0.7f, 1f);
                case ElementType.Lightning:
                    return new Color(0.7f, 0.7f, 1f);
                case ElementType.Void:
                    return new Color(0.5f, 0f, 0.5f);
                case ElementType.Cosmic:
                    return new Color(1f, 1f, 1f);
                default:
                    return Color.white;
            }
        }
        
        // Update ascension effects (called every frame while ascended)
        private void UpdateAscensionEffects()
        {
            // Pulse aura effect
            if (_auraEffect != null)
            {
                float pulse = 1f + 0.2f * Mathf.Sin(Time.time * _auraPulseSpeed);
                _auraEffect.transform.localScale = Vector3.one * _auraSize * 2f * pulse;
            }
            
            // Update zodiac enhancement
            if (_zodiacEnhancement != null)
            {
                switch (_zodiacEnhancement.EnhancementType)
                {
                    case AscensionEnhancementType.DestructionBeam:
                        UpdateDestructionBeam();
                        break;
                        
                    case AscensionEnhancementType.TimeManipulation:
                        UpdateTimeSlowField();
                        break;
                        
                    case AscensionEnhancementType.MinionSummoning:
                        UpdateMinions();
                        break;
                        
                    case AscensionEnhancementType.ElementalMastery:
                        // Handled by the ElementalEffectController
                        break;
                }
            }
        }
        
        // Update destruction beam
        private void UpdateDestructionBeam()
        {
            // Fire beam while mouse button is held
            if (Input.GetMouseButton(1) && _beamRenderer != null)
            {
                // Enable beam if not already
                if (!_beamRenderer.enabled)
                {
                    _beamRenderer.enabled = true;
                }
                
                // Get beam start position
                Vector3 startPos = transform.position + Vector3.up * 1f;
                
                // Get beam direction from camera
                Camera cam = Camera.main;
                if (cam == null) return;
                
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Vector3 endPos;
                
                if (Physics.Raycast(ray, out hit, _zodiacEnhancement.BeamRange))
                {
                    endPos = hit.point;
                    
                    // Apply damage to hit target
                    DamageableEntity entity = hit.collider.GetComponent<DamageableEntity>();
                    if (entity != null && entity != _player)
                    {
                        // Accumulate damage over time
                        _beamDamageCounter += Time.deltaTime;
                        
                        // Apply damage 10 times per second
                        if (_beamDamageCounter >= 0.1f)
                        {
                            float damage = _zodiacEnhancement.BeamDamagePerSecond * 0.1f;
                            entity.TakeDamage(damage, _zodiacEnhancement.BeamDamageType, _player?.gameObject);
                            _beamDamageCounter = 0f;
                        }
                    }
                }
                else
                {
                    // No hit, extend beam to max range
                    endPos = ray.origin + ray.direction * _zodiacEnhancement.BeamRange;
                }
                
                // Update beam positions
                _beamRenderer.SetPosition(0, startPos);
                _beamRenderer.SetPosition(1, endPos);
            }
            else if (_beamRenderer != null && _beamRenderer.enabled)
            {
                // Disable beam when not firing
                _beamRenderer.enabled = false;
                _beamDamageCounter = 0f;
            }
        }
        
        // Update time slow field
        private void UpdateTimeSlowField()
        {
            if (_timeSlowEffect == null) return;
            
            // Pulse effect
            float pulse = 1f + 0.1f * Mathf.Sin(Time.time * 2f);
            _timeSlowEffect.transform.localScale = Vector3.one * _zodiacEnhancement.TimeSlowRadius * 2f * pulse;
            
            // Find entities in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, _zodiacEnhancement.TimeSlowRadius);
            
            // Track which entities are still in range
            HashSet<GameObject> entitiesInRange = new HashSet<GameObject>();
            
            foreach (var collider in colliders)
            {
                // Skip player and player projectiles
                if (collider.gameObject == gameObject) continue;
                if (_player != null && collider.gameObject.layer == _player.gameObject.layer) continue;
                if (!_zodiacEnhancement.AffectProjectiles && collider.CompareTag("Projectile")) continue;
                
                entitiesInRange.Add(collider.gameObject);
                
                // Check if already slowed
                bool alreadySlowed = false;
                foreach (var slowedEntity in _slowedEntities)
                {
                    if (slowedEntity.GameObject == collider.gameObject)
                    {
                        alreadySlowed = true;
                        break;
                    }
                }
                
                // Apply slow if not already slowed
                if (!alreadySlowed)
                {
                    ApplyTimeSlow(collider.gameObject);
                }
            }
            
            // Remove entities no longer in range
            for (int i = _slowedEntities.Count - 1; i >= 0; i--)
            {
                if (!entitiesInRange.Contains(_slowedEntities[i].GameObject))
                {
                    RemoveTimeSlow(_slowedEntities[i]);
                    _slowedEntities.RemoveAt(i);
                }
            }
        }
        
        // Apply time slow effect to an entity
        private void ApplyTimeSlow(GameObject entity)
        {
            if (entity == null) return;
            
            // Create new slowed entity
            TimeSlowedEntity slowedEntity = new TimeSlowedEntity
            {
                GameObject = entity,
                OriginalTimeScale = 1f
            };
            
            // Get components that might be affected by time
            Animator animator = entity.GetComponent<Animator>();
            if (animator != null)
            {
                slowedEntity.Animator = animator;
                slowedEntity.OriginalAnimatorSpeed = animator.speed;
                animator.speed *= _zodiacEnhancement.TimeSlowFactor;
            }
            
            // Slow rigidbody
            Rigidbody rb = entity.GetComponent<Rigidbody>();
            if (rb != null)
            {
                slowedEntity.Rigidbody = rb;
                slowedEntity.OriginalDrag = rb.drag;
                slowedEntity.OriginalVelocity = rb.velocity;
                
                rb.drag *= (1f / _zodiacEnhancement.TimeSlowFactor);
                rb.velocity *= _zodiacEnhancement.TimeSlowFactor;
            }
            
            // Slow projectile
            ProjectileController projectile = entity.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                slowedEntity.ProjectileController = projectile;
                projectile.SetSpeedMultiplier(_zodiacEnhancement.TimeSlowFactor);
            }
            
            // Slow enemy
            EnemyController enemy = entity.GetComponent<EnemyController>();
            if (enemy != null)
            {
                slowedEntity.EnemyController = enemy;
                enemy.SetTimeScale(_zodiacEnhancement.TimeSlowFactor);
            }
            
            // Add to list
            _slowedEntities.Add(slowedEntity);
            
            // Add visual effect
            GameObject slowVfx = new GameObject("SlowEffect");
            slowVfx.transform.SetParent(entity.transform);
            slowVfx.transform.localPosition = Vector3.zero;
            
            // Add particle system
            ParticleSystem ps = slowVfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 0.2f;
            main.startSize = 0.2f;
            main.startColor = new Color(0, 0.5f, 1f, 0.5f);
            
            // Emission
            var emission = ps.emission;
            emission.rateOverTime = 10f;
            
            // Shape
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            
            slowedEntity.VisualEffect = slowVfx;
        }
        
        // Remove time slow effect
        private void RemoveTimeSlow(TimeSlowedEntity entity)
        {
            if (entity.GameObject == null) return;
            
            // Restore animator speed
            if (entity.Animator != null)
            {
                entity.Animator.speed = entity.OriginalAnimatorSpeed;
            }
            
            // Restore rigidbody
            if (entity.Rigidbody != null)
            {
                entity.Rigidbody.drag = entity.OriginalDrag;
                
                // Only restore velocity if object still exists
                if (entity.GameObject != null && entity.GameObject.activeInHierarchy)
                {
                    entity.Rigidbody.velocity = entity.OriginalVelocity;
                }
            }
            
            // Restore projectile
            if (entity.ProjectileController != null)
            {
                entity.ProjectileController.SetSpeedMultiplier(1f);
            }
            
            // Restore enemy
            if (entity.EnemyController != null)
            {
                entity.EnemyController.SetTimeScale(1f);
            }
            
            // Remove visual effect
            if (entity.VisualEffect != null)
            {
                Destroy(entity.VisualEffect);
            }
        }
        
        // Update minions
        private void UpdateMinions()
        {
            // Check if any minions are destroyed
            for (int i = _summonedMinions.Count - 1; i >= 0; i--)
            {
                if (_summonedMinions[i] == null)
                {
                    _summonedMinions.RemoveAt(i);
                }
            }
            
            // Respawn if below count
            if (_summonedMinions.Count < _zodiacEnhancement.MinionCount)
            {
                int toSpawn = _zodiacEnhancement.MinionCount - _summonedMinions.Count;
                
                for (int i = 0; i < toSpawn; i++)
                {
                    // Calculate position around player
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 2f;
                    Vector3 position = transform.position + offset;
                    
                    // Create minion
                    GameObject minionObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    minionObj.transform.position = position;
                    minionObj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                    
                    // Replace sphere collider with a capsule
                    Destroy(minionObj.GetComponent<SphereCollider>());
                    CapsuleCollider capsule = minionObj.AddComponent<CapsuleCollider>();
                    capsule.radius = 0.35f;
                    capsule.height = 1f;
                    
                    // Add rigidbody
                    Rigidbody rb = minionObj.AddComponent<Rigidbody>();
                    rb.useGravity = false;
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                    
                    // Set material
                    Renderer renderer = minionObj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = new Material(Shader.Find("Standard"));
                        renderer.material.color = _ascensionAura;
                        renderer.material.SetColor("_EmissionColor", _ascensionAura * 0.5f);
                        renderer.material.EnableKeyword("_EMISSION");
                    }
                    
                    // Add controller
                    AscensionMinionController controller = minionObj.AddComponent<AscensionMinionController>();
                    controller.Initialize(
                        _player,
                        _ascensionDuration,
                        _damageMultiplier * _zodiacEnhancement.MinionDamageMultiplier,
                        _zodiacEnhancement.MinionHealthMultiplier,
                        _zodiacEnhancement.MinionsSacrificeOnDamage
                    );
                    
                    // Add to list
                    _summonedMinions.Add(minionObj);
                }
            }
        }
        
        // Remove ascension effects
        private void RemoveAscensionEffects()
        {
            // Destroy active effects
            foreach (var effect in _activeEffects)
            {
                if (effect != null)
                {
                    Destroy(effect);
                }
            }
            _activeEffects.Clear();
            _auraEffect = null;
            _beamRenderer = null;
            
            // Reset time-slowed entities
            foreach (var entity in _slowedEntities)
            {
                RemoveTimeSlow(entity);
            }
            _slowedEntities.Clear();
            
            // Destroy minions
            foreach (var minion in _summonedMinions)
            {
                if (minion != null)
                {
                    Destroy(minion);
                }
            }
            _summonedMinions.Clear();
            
            // Remove stat modifications
            if (_player != null)
            {
                // Remove damage modifiers
                foreach (var mod in _activeModifiers)
                {
                    _player.RemoveDamageModifier(mod);
                }
                _activeModifiers.Clear();
                
                // Restore speed
                if (_speedMultiplier > 1f)
                {
                    float speedReduction = _player.MoveSpeed * (1f - (1f / _speedMultiplier));
                    _player.ModifySpeed(-speedReduction);
                }
                
                // Restore fire rate
                if (_fireRateMultiplier > 1f)
                {
                    _player.ModifyFireRate(1f / _fireRateMultiplier);
                }
                
                // Restore projectile size
                if (_projectileSizeMultiplier > 1f)
                {
                    _player.ModifyProjectileSize(1f / _projectileSizeMultiplier);
                }
                
                // Restore resource costs
                if (_removeResourceCosts)
                {
                    _player.SetResourceCostMultiplier(1f);
                }
            }
            
            // Restore original materials
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && i < _originalMaterials.Length && _originalMaterials[i] != null)
                {
                    _renderers[i].material = _originalMaterials[i];
                }
            }
            
            // Stop ambient sound
            if (_ambientAudioSource != null && _ambientAudioSource.isPlaying)
            {
                _ambientAudioSource.Stop();
            }
        }
        
        private void OnDestroy()
        {
            // Ensure we clean up all effects
            if (_isAscended)
            {
                RemoveAscensionEffects();
            }
        }
    }
    
    // Data structure for time-slowed entities
    public class TimeSlowedEntity
    {
        public GameObject GameObject;
        public float OriginalTimeScale;
        public Animator Animator;
        public float OriginalAnimatorSpeed;
        public Rigidbody Rigidbody;
        public float OriginalDrag;
        public Vector3 OriginalVelocity;
        public ProjectileController ProjectileController;
        public EnemyController EnemyController;
        public GameObject VisualEffect;
    }
    
    // Controller for elemental effects
    public class ElementalEffectController : MonoBehaviour
    {
        private PlayerController _owner;
        private ElementType _elementType;
        private float _damageMultiplier;
        private float _radiusMultiplier;
        private float _duration;
        
        private float _nextEffectTime;
        private float _effectRadius = 5f;
        private float _effectDamage = 20f;
        private DamageType _damageType;
        
        public void Initialize(PlayerController owner, ElementType elementType, float damageMultiplier, float radiusMultiplier, float duration)
        {
            _owner = owner;
            _elementType = elementType;
            _damageMultiplier = damageMultiplier;
            _radiusMultiplier = radiusMultiplier;
            _duration = duration;
            
            // Set damage type based on element
            _damageType = GetDamageType(_elementType);
            
            // Set effect radius
            _effectRadius = 5f * _radiusMultiplier;
            
            // Set damage based on player damage and multiplier
            if (_owner != null)
            {
                _effectDamage = _owner.BaseDamage * _damageMultiplier;
            }
            
            // Start with delay
            _nextEffectTime = Time.time + 1f;
        }
        
        private void Update()
        {
            // Apply elemental effect periodically
            if (Time.time >= _nextEffectTime)
            {
                ApplyElementalEffect();
                _nextEffectTime = Time.time + _duration;
            }
        }
        
        // Apply elemental effect
        private void ApplyElementalEffect()
        {
            // Create effect visualization
            GameObject effectObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effectObj.transform.position = transform.position;
            effectObj.transform.localScale = Vector3.zero;
            
            // Remove collider
            Destroy(effectObj.GetComponent<Collider>());
            
            // Set material based on element
            Renderer renderer = effectObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Particles/Additive"));
                renderer.material.color = GetElementColor(_elementType);
            }
            
            // Animate effect
            StartCoroutine(AnimateElementalEffect(effectObj));
            
            // Apply damage in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, _effectRadius);
            
            foreach (var collider in colliders)
            {
                // Skip owner
                if (collider.gameObject == gameObject || (_owner != null && collider.gameObject == _owner.gameObject))
                    continue;
                    
                // Apply damage to damageable entities
                DamageableEntity entity = collider.GetComponent<DamageableEntity>();
                if (entity != null)
                {
                    entity.TakeDamage(_effectDamage, _damageType, _owner?.gameObject);
                    
                    // Apply status effect based on element
                    EnemyController enemy = entity as EnemyController;
                    if (enemy != null)
                    {
                        ApplyElementalStatusEffect(enemy);
                    }
                }
            }
        }
        
        // Animate elemental effect
        private IEnumerator AnimateElementalEffect(GameObject effectObj)
        {
            if (effectObj == null) yield break;
            
            float duration = 1.5f;
            float startTime = Time.time;
            
            // Expand phase
            float expandDuration = 0.3f;
            while (Time.time < startTime + expandDuration)
            {
                float t = (Time.time - startTime) / expandDuration;
                float scale = _effectRadius * 2f * t;
                effectObj.transform.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }
            
            // Hold at full size briefly
            effectObj.transform.localScale = new Vector3(_effectRadius * 2f, _effectRadius * 2f, _effectRadius * 2f);
            yield return new WaitForSeconds(0.2f);
            
            // Fade out phase
            float fadeDuration = 1f;
            float fadeStartTime = Time.time;
            
            while (Time.time < fadeStartTime + fadeDuration)
            {
                float t = (Time.time - fadeStartTime) / fadeDuration;
                
                // Fade out and expand slightly
                float scale = _effectRadius * 2f * (1f + t * 0.2f);
                effectObj.transform.localScale = new Vector3(scale, scale, scale);
                
                Renderer renderer = effectObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = 1f - t;
                    renderer.material.color = color;
                }
                
                yield return null;
            }
            
            // Destroy effect
            Destroy(effectObj);
        }
        
        // Apply elemental status effect
        private void ApplyElementalStatusEffect(EnemyController enemy)
        {
            if (enemy == null) return;
            
            // Apply status effect based on element type
            switch (_elementType)
            {
                case ElementType.Fire:
                    enemy.ApplyStatusEffect(StatusEffectType.Burning, 5f);
                    break;
                    
                case ElementType.Ice:
                    enemy.ApplyStatusEffect(StatusEffectType.Frozen, 3f);
                    break;
                    
                case ElementType.Lightning:
                    enemy.ApplyStatusEffect(StatusEffectType.Shocked, 2f);
                    break;
                    
                case ElementType.Void:
                    enemy.ApplyStatusEffect(StatusEffectType.Weakened, 8f);
                    break;
                    
                case ElementType.Cosmic:
                    // Apply multiple effects for cosmic
                    enemy.ApplyStatusEffect(StatusEffectType.Stunned, 1f);
                    enemy.ApplyStatusEffect(StatusEffectType.Weakened, 4f);
                    break;
            }
        }
        
        // Get damage type from element type
        private DamageType GetDamageType(ElementType elementType)
        {
            switch (elementType)
            {
                case ElementType.Fire:
                    return DamageType.Fire;
                case ElementType.Ice:
                    return DamageType.Frost;
                case ElementType.Lightning:
                    return DamageType.Lightning;
                case ElementType.Void:
                    return DamageType.Void;
                case ElementType.Cosmic:
                    return DamageType.Cosmic;
                default:
                    return DamageType.Physical;
            }
        }
        
        // Get element color
        private Color GetElementColor(ElementType type)
        {
            switch (type)
            {
                case ElementType.Fire:
                    return new Color(1f, 0.3f, 0f);
                case ElementType.Ice:
                    return new Color(0f, 0.7f, 1f);
                case ElementType.Lightning:
                    return new Color(0.7f, 0.7f, 1f);
                case ElementType.Void:
                    return new Color(0.5f, 0f, 0.5f);
                case ElementType.Cosmic:
                    return new Color(1f, 1f, 1f);
                default:
                    return Color.white;
            }
        }
    }
    
    // Controller for ascension minions
    public class AscensionMinionController : MonoBehaviour
    {
        private PlayerController _owner;
        private float _duration;
        private float _damage;
        private float _healthMultiplier;
        private bool _sacrificeOnDamage;
        
        private float _spawnTime;
        private float _health = 50f;
        private float _maxHealth = 50f;
        private GameObject _target;
        private float _moveSpeed = 5f;
        private float _attackRange = 2f;
        private float _attackCooldown = 0.5f;
        private float _lastAttackTime;
        
        public void Initialize(PlayerController owner, float duration, float damageMultiplier, float healthMultiplier, bool sacrificeOnDamage)
        {
            _owner = owner;
            _duration = duration;
            _damage = owner != null ? owner.BaseDamage * damageMultiplier : 10f;
            _healthMultiplier = healthMultiplier;
            _sacrificeOnDamage = sacrificeOnDamage;
            
            _spawnTime = Time.time;
            _maxHealth = 50f * _healthMultiplier;
            _health = _maxHealth;
            _lastAttackTime = Time.time;
        }
        
        private void Update()
        {
            // Check if expired
            if (Time.time > _spawnTime + _duration)
            {
                // Fade out and destroy
                StartCoroutine(FadeOutAndDestroy());
                return;
            }
            
            // Find target if none
            if (_target == null)
            {
                FindNearestTarget();
            }
            
            // Move towards target
            if (_target != null)
            {
                MoveTowardsTarget();
                
                // Attack if in range
                if (Vector3.Distance(transform.position, _target.transform.position) <= _attackRange)
                {
                    if (Time.time > _lastAttackTime + _attackCooldown)
                    {
                        Attack();
                    }
                }
            }
            else
            {
                // No target, follow owner
                if (_owner != null)
                {
                    float distance = Vector3.Distance(transform.position, _owner.transform.position);
                    
                    // Only move if too far from owner
                    if (distance > 3f)
                    {
                        Vector3 direction = (_owner.transform.position - transform.position).normalized;
                        transform.position += direction * _moveSpeed * Time.deltaTime;
                    }
                }
            }
        }
        
        // Find the nearest enemy
        private void FindNearestTarget()
        {
            // Find all enemies in scene
            EnemyController[] enemies = FindObjectsOfType<EnemyController>();
            
            // Find closest
            float closestDistance = float.MaxValue;
            EnemyController closestEnemy = null;
            
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDead)
                    continue;
                    
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
            
            // Set target
            _target = closestEnemy?.gameObject;
        }
        
        // Move towards the target
        private void MoveTowardsTarget()
        {
            if (_target == null) return;
            
            Vector3 direction = (_target.transform.position - transform.position).normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;
            
            // Look at target
            transform.LookAt(_target.transform);
        }
        
        // Attack the target
        private void Attack()
        {
            if (_target == null) return;
            
            // Apply damage
            DamageableEntity entity = _target.GetComponent<DamageableEntity>();
            if (entity != null)
            {
                entity.TakeDamage(_damage, DamageType.Cosmic, _owner?.gameObject);
            }
            
            // Visual effect
            StartCoroutine(AttackEffect());
            
            // Update cooldown
            _lastAttackTime = Time.time;
        }
        
        // Attack visual effect
        private IEnumerator AttackEffect()
        {
            // Create attack line
            GameObject attackLine = new GameObject("AttackLine");
            LineRenderer line = attackLine.AddComponent<LineRenderer>();
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.positionCount = 2;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, _target.transform.position);
            
            // Set material
            line.material = new Material(Shader.Find("Particles/Additive"));
            line.startColor = GetComponent<Renderer>().material.color;
            line.endColor = new Color(1f, 1f, 1f, 0f);
            
            // Fade out
            float duration = 0.2f;
            float startTime = Time.time;
            
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                
                Color startColor = line.startColor;
                startColor.a = 1f - t;
                line.startColor = startColor;
                
                yield return null;
            }
            
            // Destroy line
            Destroy(attackLine);
        }
        
        // Take damage
        public void TakeDamage(float amount)
        {
            _health -= amount;
            
            // Check if dead
            if (_health <= 0)
            {
                // Sacrificial explosion if enabled
                if (_sacrificeOnDamage)
                {
                    SacrificialExplosion();
                }
                
                // Destroy
                Destroy(gameObject);
            }
        }
        
        // Sacrifice the minion to damage nearby enemies
        private void SacrificialExplosion()
        {
            // Create explosion effect
            GameObject explosionVfx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            explosionVfx.transform.position = transform.position;
            explosionVfx.transform.localScale = Vector3.one * 0.1f;
            
            // Remove collider
            Destroy(explosionVfx.GetComponent<Collider>());
            
            // Set material
            Renderer renderer = explosionVfx.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Particles/Additive"));
                renderer.material.color = new Color(1f, 0.3f, 0.3f, 0.7f);
            }
            
            // Animate explosion
            StartCoroutine(AnimateExplosion(explosionVfx));
            
            // Damage nearby enemies
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
            
            foreach (var collider in colliders)
            {
                if (collider.gameObject == gameObject) continue;
                
                // Apply damage to enemy
                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(_damage * 2f, DamageType.Cosmic, _owner?.gameObject);
                }
            }
        }
        
        // Animate explosion
        private IEnumerator AnimateExplosion(GameObject explosionVfx)
        {
            if (explosionVfx == null) yield break;
            
            float duration = 0.5f;
            float startTime = Time.time;
            
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                float scale = Mathf.Lerp(0.1f, 5f, t);
                
                explosionVfx.transform.localScale = Vector3.one * scale;
                
                // Fade out towards the end
                if (t > 0.7f)
                {
                    Renderer renderer = explosionVfx.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Color color = renderer.material.color;
                        color.a = 1f - ((t - 0.7f) / 0.3f);
                        renderer.material.color = color;
                    }
                }
                
                yield return null;
            }
            
            // Destroy effect
            Destroy(explosionVfx);
        }
        
        // Fade out and destroy
        private IEnumerator FadeOutAndDestroy()
        {
            float duration = 0.5f;
            float startTime = Time.time;
            
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.material.color;
                
                while (Time.time < startTime + duration)
                {
                    float t = (Time.time - startTime) / duration;
                    
                    Color color = originalColor;
                    color.a = 1f - t;
                    renderer.material.color = color;
                    
                    yield return null;
                }
            }
            
            // Destroy gameobject
            Destroy(gameObject);
        }
    }
}