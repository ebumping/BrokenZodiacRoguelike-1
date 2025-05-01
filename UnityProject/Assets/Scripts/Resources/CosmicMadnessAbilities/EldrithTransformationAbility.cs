using UnityEngine;
using CodexOfTheBrokenZodiac.Core;
using System.Collections;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources.CosmicMadnessAbilities
{
    // Ability that transforms the player into an eldritch form with zodiac-specific effects
    [CreateAssetMenu(fileName = "Eldritch Transformation", menuName = "Codex/Cosmic Madness/Abilities/Eldritch Transformation")]
    public class EldrithTransformationAbility : CosmicMadnessAbility
    {
        [Header("Transformation Settings")]
        [SerializeField] private float transformationDuration = 30f;
        [SerializeField] private float transformationCooldown = 120f;
        [SerializeField] private bool drainSanityDuringTransformation = true;
        [SerializeField] private float sanityDrainPerSecond = 0.5f;
        
        [Header("Base Transformation Effects")]
        [SerializeField] private float moveSpeedModifier = 1.2f;
        [SerializeField] private float damageModifier = 1.5f;
        [SerializeField] private float defenseModifier = 1.3f;
        [SerializeField] private bool grantsDamageImmunity = false;
        [SerializeField] private StatusEffectType immunityType = StatusEffectType.None;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject transformationVFXPrefab;
        [SerializeField] private GameObject transformedVFXPrefab;
        [SerializeField] private Material transformedMaterial;
        [SerializeField] private Mesh transformedMesh;
        [SerializeField] private AudioClip transformationSound;
        [SerializeField] private AudioClip transformedAmbientSound;
        [SerializeField] private AudioClip revertSound;
        
        [Header("Zodiac Transformations")]
        [SerializeField] private ZodiacTransformation[] zodiacTransformations;
        
        // Runtime variables
        private EldrithTransformationComponent _transformComponent;
        
        // Apply the transformation effect
        protected override void ApplyEffect(PlayerController player)
        {
            if (player == null) return;
            
            // Add transformation component if not already present
            _transformComponent = player.gameObject.GetComponent<EldrithTransformationComponent>();
            if (_transformComponent == null)
            {
                _transformComponent = player.gameObject.AddComponent<EldrithTransformationComponent>();
            }
            
            // Get zodiac-specific transformation
            ZodiacSign playerSign = player.GetZodiacSign();
            ZodiacTransformation zodiacTransform = GetZodiacTransformation(playerSign);
            
            // Initialize the component
            _transformComponent.Initialize(
                transformationDuration,
                transformationCooldown,
                drainSanityDuringTransformation,
                sanityDrainPerSecond,
                moveSpeedModifier,
                damageModifier,
                defenseModifier,
                grantsDamageImmunity,
                immunityType,
                transformationVFXPrefab,
                transformedVFXPrefab,
                transformedMaterial,
                transformedMesh,
                transformationSound,
                transformedAmbientSound,
                revertSound,
                zodiacTransform
            );
            
            // Allow transformation activation
            _transformComponent.EnableTransformation();
        }
        
        // Remove the transformation effect
        protected override void RemoveEffect(PlayerController player)
        {
            if (player == null) return;
            
            // Disable transformation component
            EldrithTransformationComponent transformComponent = player.gameObject.GetComponent<EldrithTransformationComponent>();
            if (transformComponent != null)
            {
                transformComponent.DisableTransformation();
            }
        }
        
        // Get zodiac-specific transformation
        private ZodiacTransformation GetZodiacTransformation(ZodiacSign sign)
        {
            if (zodiacTransformations == null)
                return null;
                
            foreach (var transform in zodiacTransformations)
            {
                if (transform.ZodiacSign == sign)
                    return transform;
            }
            
            return null; // No specific transformation found
        }
        
        // Return zodiac-specific details
        protected override string GetZodiacSpecificDetails(ZodiacSign sign)
        {
            ZodiacTransformation zodiacTransform = GetZodiacTransformation(sign);
            if (zodiacTransform == null)
                return "";
                
            return zodiacTransform.Description;
        }
    }
    
    // Data structure for zodiac-specific transformations
    [System.Serializable]
    public class ZodiacTransformation
    {        
        [SerializeField] private ZodiacSign zodiacSign;
        [SerializeField] private string transformationName;
        [SerializeField] private string description;
        
        [Header("Unique Abilities")]
        [SerializeField] private bool canFly;
        [SerializeField] private bool canPhaseThrough;
        [SerializeField] private bool canTeleport;
        [SerializeField] private bool generateAura;
        [SerializeField] private bool createMinions;
        
        [Header("Special Effects")]
        [SerializeField] private DamageType primaryDamageType;
        [SerializeField] private StatusEffectType inflictedStatusEffect;
        [SerializeField] private float statusEffectDuration = 3f;
        [SerializeField] private float auraRadius = 5f;
        [SerializeField] private float auraDamagePerSecond = 10f;
        [SerializeField] private int maxMinions = 3;
        [SerializeField] private float minionDuration = 10f;
        [SerializeField] private float teleportDistance = 10f;
        [SerializeField] private float teleportCooldown = 5f;
        
        [Header("Visual Overrides")]
        [SerializeField] private Material customMaterial;
        [SerializeField] private Mesh customMesh;
        [SerializeField] private GameObject customVFXPrefab;
        [SerializeField] private AudioClip customSound;
        [SerializeField] private Color auraColor = Color.cyan;
        
        // Properties
        public ZodiacSign ZodiacSign => zodiacSign;
        public string TransformationName => transformationName;
        public string Description => description;
        public bool CanFly => canFly;
        public bool CanPhaseThrough => canPhaseThrough;
        public bool CanTeleport => canTeleport;
        public bool GenerateAura => generateAura;
        public bool CreateMinions => createMinions;
        public DamageType PrimaryDamageType => primaryDamageType;
        public StatusEffectType InflictedStatusEffect => inflictedStatusEffect;
        public float StatusEffectDuration => statusEffectDuration;
        public float AuraRadius => auraRadius;
        public float AuraDamagePerSecond => auraDamagePerSecond;
        public int MaxMinions => maxMinions;
        public float MinionDuration => minionDuration;
        public float TeleportDistance => teleportDistance;
        public float TeleportCooldown => teleportCooldown;
        public Material CustomMaterial => customMaterial;
        public Mesh CustomMesh => customMesh;
        public GameObject CustomVFXPrefab => customVFXPrefab;
        public AudioClip CustomSound => customSound;
        public Color AuraColor => auraColor;
    }
    
    // Component that handles the eldritch transformation
    public class EldrithTransformationComponent : MonoBehaviour
    {
        // Configuration
        private float _transformationDuration;
        private float _transformationCooldown;
        private bool _drainSanityDuringTransformation;
        private float _sanityDrainPerSecond;
        private float _moveSpeedModifier;
        private float _damageModifier;
        private float _defenseModifier;
        private bool _grantsDamageImmunity;
        private StatusEffectType _immunityType;
        private GameObject _transformationVFXPrefab;
        private GameObject _transformedVFXPrefab;
        private Material _transformedMaterial;
        private Mesh _transformedMesh;
        private AudioClip _transformationSound;
        private AudioClip _transformedAmbientSound;
        private AudioClip _revertSound;
        private ZodiacTransformation _zodiacTransformation;
        
        // Runtime variables
        private bool _isEnabled = false;
        private bool _isTransformed = false;
        private float _lastTransformationTime = -999f;
        private List<GameObject> _activeEffects = new List<GameObject>();
        private List<GameObject> _spawnedMinions = new List<GameObject>();
        private AudioSource _audioSource;
        private AudioSource _ambientAudioSource;
        private PlayerController _player;
        private DamageModifier _damageMod;
        private DamageModifier _defenseMod;
        private Renderer[] _renderers;
        private Material[] _originalMaterials;
        private MeshFilter _meshFilter;
        private Mesh _originalMesh;
        private float _teleportCooldownTimer = 0f;
        private float _minionSpawnTimer = 0f;
        private GameObject _auraEffect;
        
        // Initialize the component
        public void Initialize(
            float transformationDuration,
            float transformationCooldown,
            bool drainSanityDuringTransformation,
            float sanityDrainPerSecond,
            float moveSpeedModifier,
            float damageModifier,
            float defenseModifier,
            bool grantsDamageImmunity,
            StatusEffectType immunityType,
            GameObject transformationVFXPrefab,
            GameObject transformedVFXPrefab,
            Material transformedMaterial,
            Mesh transformedMesh,
            AudioClip transformationSound,
            AudioClip transformedAmbientSound,
            AudioClip revertSound,
            ZodiacTransformation zodiacTransformation)
        {
            _transformationDuration = transformationDuration;
            _transformationCooldown = transformationCooldown;
            _drainSanityDuringTransformation = drainSanityDuringTransformation;
            _sanityDrainPerSecond = sanityDrainPerSecond;
            _moveSpeedModifier = moveSpeedModifier;
            _damageModifier = damageModifier;
            _defenseModifier = defenseModifier;
            _grantsDamageImmunity = grantsDamageImmunity;
            _immunityType = immunityType;
            _transformationVFXPrefab = transformationVFXPrefab;
            _transformedVFXPrefab = transformedVFXPrefab;
            _transformedMaterial = transformedMaterial;
            _transformedMesh = transformedMesh;
            _transformationSound = transformationSound;
            _transformedAmbientSound = transformedAmbientSound;
            _revertSound = revertSound;
            _zodiacTransformation = zodiacTransformation;
            
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
            
            // Cache mesh reference
            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter != null)
            {
                _originalMesh = _meshFilter.mesh;
            }
        }
        
        private void Update()
        {
            if (!_isEnabled) return;
            
            // Check for transformation input
            if (Input.GetKeyDown(KeyCode.X) && CanTransform())
            {
                if (!_isTransformed)
                {
                    StartCoroutine(Transform());
                }
                else
                {
                    StartCoroutine(Revert());
                }
            }
            
            // Update transformation timers
            if (_isTransformed)
            {
                UpdateTransformationState();
            }
            
            // Update ability cooldowns
            if (_zodiacTransformation != null && _zodiacTransformation.CanTeleport && _teleportCooldownTimer > 0)
            {
                _teleportCooldownTimer -= Time.deltaTime;
            }
        }
        
        // Check if transformation is available
        private bool CanTransform()
        {
            if (_isTransformed) return true; // Can always revert
            
            // Check cooldown
            if (Time.time - _lastTransformationTime < _transformationCooldown)
                return false;
                
            return true;
        }
        
        // Enable transformation ability
        public void EnableTransformation()
        {
            _isEnabled = true;
            
            // Add UI notification
            string name = _zodiacTransformation != null ? _zodiacTransformation.TransformationName : "Eldritch Form";
            GameManager.Instance?.ShowNotification($"Cosmic Madness: {name} transformation available. Press X to transform.");
        }
        
        // Disable transformation ability
        public void DisableTransformation()
        {
            _isEnabled = false;
            
            // Revert if transformed
            if (_isTransformed)
            {
                StartCoroutine(Revert());
            }
        }
        
        // Transform into eldritch form
        private IEnumerator Transform()
        {
            if (_isTransformed || _player == null)
                yield break;
                
            // Update state
            _isTransformed = true;
            _lastTransformationTime = Time.time;
            
            // Play transformation effect
            if (_transformationVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_transformationVFXPrefab, transform.position, Quaternion.identity);
                vfx.transform.SetParent(transform);
                vfx.transform.localPosition = Vector3.zero;
                _activeEffects.Add(vfx);
                
                // Destroy after transformation
                Destroy(vfx, 3f);
            }
            
            // Play transformation sound
            if (_transformationSound != null && _audioSource != null)
            {
                _audioSource.clip = _transformationSound;
                _audioSource.loop = false;
                _audioSource.Play();
            }
            
            // Wait for initial transformation effect
            yield return new WaitForSeconds(1f);
            
            // Apply transformation effects
            ApplyTransformationEffects();
            
            // Show notification
            string name = _zodiacTransformation != null ? _zodiacTransformation.TransformationName : "Eldritch Form";
            GameManager.Instance?.ShowNotification($"Transformed into {name}!");
            
            // Schedule automatic reversion
            StartCoroutine(AutoRevert());
        }
        
        // Revert from eldritch form
        private IEnumerator Revert()
        {
            if (!_isTransformed || _player == null)
                yield break;
                
            // Stop auto revert if running
            StopCoroutine("AutoRevert");
            
            // Play revert effect
            if (_transformationVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_transformationVFXPrefab, transform.position, Quaternion.identity);
                vfx.transform.SetParent(transform);
                vfx.transform.localPosition = Vector3.zero;
                
                // Different color for reversion
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = new Color(1f, 0.5f, 0.5f);
                }
                
                // Destroy after reversion
                Destroy(vfx, 3f);
            }
            
            // Play revert sound
            if (_revertSound != null && _audioSource != null)
            {
                _audioSource.clip = _revertSound;
                _audioSource.loop = false;
                _audioSource.Play();
            }
            
            // Wait for revert effect
            yield return new WaitForSeconds(1f);
            
            // Remove transformation effects
            RemoveTransformationEffects();
            
            // Update state
            _isTransformed = false;
            
            // Show notification
            GameManager.Instance?.ShowNotification("Reverted to normal form.");
        }
        
        // Automatically revert after duration
        private IEnumerator AutoRevert()
        {
            yield return new WaitForSeconds(_transformationDuration);
            
            if (_isTransformed)
            {
                StartCoroutine(Revert());
            }
        }
        
        // Apply transformation effects
        private void ApplyTransformationEffects()
        {
            // Apply stat modifications
            if (_player != null)
            {
                // Speed modifier
                if (_moveSpeedModifier != 1f)
                {
                    _player.ModifySpeed(_player.MoveSpeed * (_moveSpeedModifier - 1f));
                }
                
                // Damage modifier
                if (_damageModifier != 1f)
                {
                    _damageMod = new DamageModifier
                    {
                        ModifierType = ModifierType.Outgoing,
                        Value = _damageModifier,
                        IsMultiplier = true
                    };
                    
                    _player.AddDamageModifier(_damageMod);
                }
                
                // Defense modifier
                if (_defenseModifier != 1f)
                {
                    _defenseMod = new DamageModifier
                    {
                        ModifierType = ModifierType.Incoming,
                        Value = 1f / _defenseModifier, // Invert for damage reduction
                        IsMultiplier = true
                    };
                    
                    _player.AddDamageModifier(_defenseMod);
                }
                
                // Status effect immunity
                if (_grantsDamageImmunity && _immunityType != StatusEffectType.None)
                {
                    _player.AddStatusEffectImmunity(_immunityType);
                }
            }
            
            // Apply visual changes
            ApplyVisualEffects();
            
            // Apply zodiac-specific effects
            if (_zodiacTransformation != null)
            {
                ApplyZodiacEffects();
            }
        }
        
        // Apply visual effects
        private void ApplyVisualEffects()
        {
            // Apply transformed VFX
            if (_transformedVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_transformedVFXPrefab, transform.position, Quaternion.identity);
                vfx.transform.SetParent(transform);
                vfx.transform.localPosition = Vector3.zero;
                _activeEffects.Add(vfx);
            }
            
            // Apply ambient sound
            if (_transformedAmbientSound != null && _ambientAudioSource != null)
            {
                _ambientAudioSource.clip = _transformedAmbientSound;
                _ambientAudioSource.Play();
            }
            
            // Apply material change
            Material materialToUse = _transformedMaterial;
            if (_zodiacTransformation != null && _zodiacTransformation.CustomMaterial != null)
            {
                materialToUse = _zodiacTransformation.CustomMaterial;
            }
            
            if (materialToUse != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null)
                    {
                        _renderers[i].material = materialToUse;
                    }
                }
            }
            
            // Apply mesh change
            Mesh meshToUse = _transformedMesh;
            if (_zodiacTransformation != null && _zodiacTransformation.CustomMesh != null)
            {
                meshToUse = _zodiacTransformation.CustomMesh;
            }
            
            if (meshToUse != null && _meshFilter != null)
            {
                _meshFilter.mesh = meshToUse;
            }
        }
        
        // Apply zodiac-specific effects
        private void ApplyZodiacEffects()
        {
            if (_zodiacTransformation == null)
                return;
                
            // Apply flying ability
            if (_zodiacTransformation.CanFly)
            {
                EnableFlying();
            }
            
            // Apply phase-through ability
            if (_zodiacTransformation.CanPhaseThrough)
            {
                EnablePhasing();
            }
            
            // Create damage aura
            if (_zodiacTransformation.GenerateAura)
            {
                CreateDamageAura();
            }
            
            // Create zodiac-specific VFX
            if (_zodiacTransformation.CustomVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_zodiacTransformation.CustomVFXPrefab, transform.position, Quaternion.identity);
                vfx.transform.SetParent(transform);
                vfx.transform.localPosition = Vector3.zero;
                _activeEffects.Add(vfx);
            }
            
            // Play zodiac-specific sound
            if (_zodiacTransformation.CustomSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_zodiacTransformation.CustomSound);
            }
        }
        
        // Enable flying ability
        private void EnableFlying()
        {
            if (_player == null)
                return;
                
            // Add flying component or enable flying mode
            FlyingMovement flying = gameObject.GetComponent<FlyingMovement>();
            if (flying == null)
            {
                flying = gameObject.AddComponent<FlyingMovement>();
            }
            
            // Enable
            flying.EnableFlying();
        }
        
        // Enable phase-through ability
        private void EnablePhasing()
        {
            if (_player == null)
                return;
                
            // Modify collider or layer for phasing
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                // Store original layer
                collider.tag = "PhasingEntity";
                
                // Ignore collision with environment
                Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Environment"), true);
            }
        }
        
        // Create damage aura
        private void CreateDamageAura()
        {
            if (_zodiacTransformation == null)
                return;
                
            // Create aura visual effect
            GameObject auraObj = new GameObject("DamageAura");
            auraObj.transform.SetParent(transform);
            auraObj.transform.localPosition = Vector3.zero;
            
            // Add sphere mesh and renderer
            MeshFilter meshFilter = auraObj.AddComponent<MeshFilter>();
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            
            MeshRenderer renderer = auraObj.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
            renderer.material.color = new Color(_zodiacTransformation.AuraColor.r, _zodiacTransformation.AuraColor.g, _zodiacTransformation.AuraColor.b, 0.3f);
            
            // Set size based on aura radius
            auraObj.transform.localScale = new Vector3(
                _zodiacTransformation.AuraRadius * 2,
                _zodiacTransformation.AuraRadius * 2,
                _zodiacTransformation.AuraRadius * 2);
                
            // Add to active effects
            _activeEffects.Add(auraObj);
            _auraEffect = auraObj;
        }
        
        // Update transformation state (called every frame while transformed)
        private void UpdateTransformationState()
        {
            if (!_isTransformed || _player == null)
                return;
                
            // Drain sanity if enabled
            if (_drainSanityDuringTransformation && _sanityDrainPerSecond > 0)
            {
                int playerId = _player.NetworkId;
                SanitySystem.Instance?.ModifyPlayerSanity(playerId, -_sanityDrainPerSecond * Time.deltaTime);
            }
            
            // Update zodiac-specific abilities
            if (_zodiacTransformation != null)
            {
                // Handle teleportation ability
                if (_zodiacTransformation.CanTeleport && Input.GetKeyDown(KeyCode.T) && _teleportCooldownTimer <= 0)
                {
                    TeleportPlayer();
                }
                
                // Handle damage aura
                if (_zodiacTransformation.GenerateAura && _auraEffect != null)
                {
                    ApplyAuraDamage();
                }
                
                // Handle minion creation
                if (_zodiacTransformation.CreateMinions)
                {
                    _minionSpawnTimer -= Time.deltaTime;
                    
                    if (_minionSpawnTimer <= 0 && _spawnedMinions.Count < _zodiacTransformation.MaxMinions)
                    {
                        SpawnMinion();
                        _minionSpawnTimer = 5f; // Cooldown between spawns
                    }
                    
                    // Clean up expired minions
                    CleanupExpiredMinions();
                }
            }
        }
        
        // Teleport the player
        private void TeleportPlayer()
        {
            if (_player == null || _zodiacTransformation == null)
                return;
                
            // Calculate teleport position based on look direction
            Vector3 lookDirection = _player.transform.forward;
            Vector3 teleportPosition = transform.position + lookDirection * _zodiacTransformation.TeleportDistance;
            
            // Adjust height if needed
            RaycastHit hit;
            if (Physics.Raycast(teleportPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Ground", "Environment")))
            {
                teleportPosition.y = hit.point.y + 1f;
            }
            
            // Teleport effect at start position
            if (_transformationVFXPrefab != null)
            {
                GameObject startVfx = Instantiate(_transformationVFXPrefab, transform.position, Quaternion.identity);
                Destroy(startVfx, 2f);
            }
            
            // Teleport the player
            transform.position = teleportPosition;
            
            // Teleport effect at end position
            if (_transformationVFXPrefab != null)
            {
                GameObject endVfx = Instantiate(_transformationVFXPrefab, transform.position, Quaternion.identity);
                Destroy(endVfx, 2f);
            }
            
            // Play sound
            if (_audioSource != null)
            {
                _audioSource.PlayOneShot(_transformationSound ?? _zodiacTransformation.CustomSound);
            }
            
            // Set cooldown
            _teleportCooldownTimer = _zodiacTransformation.TeleportCooldown;
        }
        
        // Apply damage from aura
        private void ApplyAuraDamage()
        {
            if (_player == null || _zodiacTransformation == null)
                return;
                
            // Find enemies in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, _zodiacTransformation.AuraRadius);
            
            foreach (var collider in colliders)
            {
                // Skip self
                if (collider.gameObject == gameObject)
                    continue;
                    
                // Check if it's a damageable entity
                DamageableEntity entity = collider.GetComponent<DamageableEntity>();
                if (entity != null && entity != _player)
                {
                    // Apply damage
                    float damage = _zodiacTransformation.AuraDamagePerSecond * Time.deltaTime;
                    entity.TakeDamage(damage, _zodiacTransformation.PrimaryDamageType, _player.gameObject);
                    
                    // Apply status effect
                    if (_zodiacTransformation.InflictedStatusEffect != StatusEffectType.None)
                    {
                        // Check if it's an enemy
                        EnemyController enemy = entity as EnemyController;
                        if (enemy != null)
                        {
                            enemy.ApplyStatusEffect(_zodiacTransformation.InflictedStatusEffect, _zodiacTransformation.StatusEffectDuration);
                        }
                    }
                }
            }
        }
        
        // Spawn a minion
        private void SpawnMinion()
        {
            if (_player == null || _zodiacTransformation == null)
                return;
                
            // Create minion
            GameObject minionObj = new GameObject("EldrithMinion");
            minionObj.transform.position = transform.position + UnityEngine.Random.insideUnitSphere * 3f;
            minionObj.transform.position = new Vector3(minionObj.transform.position.x, transform.position.y, minionObj.transform.position.z);
            
            // Add visuals
            MeshFilter meshFilter = minionObj.AddComponent<MeshFilter>();
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            
            MeshRenderer renderer = minionObj.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = _zodiacTransformation.AuraColor;
            
            // Add minion component
            EldrithMinionController minionController = minionObj.AddComponent<EldrithMinionController>();
            minionController.Initialize(
                _player, 
                _zodiacTransformation.MinionDuration, 
                _zodiacTransformation.PrimaryDamageType, 
                _attackDamage * 0.5f
            );
            
            // Add to list
            _spawnedMinions.Add(minionObj);
        }
        
        // Clean up expired minions
        private void CleanupExpiredMinions()
        {
            for (int i = _spawnedMinions.Count - 1; i >= 0; i--)
            {
                GameObject minion = _spawnedMinions[i];
                
                if (minion == null)
                {
                    _spawnedMinions.RemoveAt(i);
                    continue;
                }
                
                EldrithMinionController controller = minion.GetComponent<EldrithMinionController>();
                if (controller != null && controller.IsExpired)
                {
                    Destroy(minion);
                    _spawnedMinions.RemoveAt(i);
                }
            }
        }
        
        // Remove transformation effects
        private void RemoveTransformationEffects()
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
            
            // Destroy minions
            foreach (var minion in _spawnedMinions)
            {
                if (minion != null)
                {
                    Destroy(minion);
                }
            }
            _spawnedMinions.Clear();
            
            // Remove stat modifications
            if (_player != null)
            {
                // Speed modifier
                if (_moveSpeedModifier != 1f)
                {
                    _player.ModifySpeed(_player.MoveSpeed * (1f/_moveSpeedModifier - 1f));
                }
                
                // Remove damage modifier
                if (_damageMod != null)
                {
                    _player.RemoveDamageModifier(_damageMod);
                    _damageMod = null;
                }
                
                // Remove defense modifier
                if (_defenseMod != null)
                {
                    _player.RemoveDamageModifier(_defenseMod);
                    _defenseMod = null;
                }
                
                // Remove status effect immunity
                if (_grantsDamageImmunity && _immunityType != StatusEffectType.None)
                {
                    _player.RemoveStatusEffectImmunity(_immunityType);
                }
            }
            
            // Remove phasing
            Collider collider = GetComponent<Collider>();
            if (collider != null && collider.CompareTag("PhasingEntity"))
            {
                collider.tag = "Player";
                Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Environment"), false);
            }
            
            // Remove flying
            FlyingMovement flying = gameObject.GetComponent<FlyingMovement>();
            if (flying != null)
            {
                flying.DisableFlying();
            }
            
            // Restore original materials
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && i < _originalMaterials.Length && _originalMaterials[i] != null)
                {
                    _renderers[i].material = _originalMaterials[i];
                }
            }
            
            // Restore original mesh
            if (_meshFilter != null && _originalMesh != null)
            {
                _meshFilter.mesh = _originalMesh;
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
            if (_isTransformed)
            {
                RemoveTransformationEffects();
            }
        }
    }
    
    // Flying movement component
    public class FlyingMovement : MonoBehaviour
    {
        private bool _isFlying = false;
        private PlayerController _player;
        private float _originalGravity;
        private float _flySpeed = 10f;
        private CharacterController _controller;
        
        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _controller = GetComponent<CharacterController>();
        }
        
        public void EnableFlying()
        {
            if (_isFlying) return;
            
            _isFlying = true;
            
            if (_player != null)
            {
                // Store original gravity
                _originalGravity = _player.Gravity;
                
                // Disable gravity
                _player.SetGravity(0f);
            }
        }
        
        public void DisableFlying()
        {
            if (!_isFlying) return;
            
            _isFlying = false;
            
            if (_player != null)
            {
                // Restore original gravity
                _player.SetGravity(_originalGravity);
            }
        }
        
        private void Update()
        {
            if (!_isFlying) return;
            
            // Handle vertical movement
            float verticalInput = 0f;
            if (Input.GetKey(KeyCode.Space))
            {
                verticalInput = 1f;
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
            {
                verticalInput = -1f;
            }
            
            // Apply vertical movement
            if (_controller != null && verticalInput != 0)
            {
                Vector3 moveDir = new Vector3(0, verticalInput * _flySpeed * Time.deltaTime, 0);
                _controller.Move(moveDir);
            }
            else if (_player != null && verticalInput != 0)
            {
                // Fallback if no character controller
                Vector3 pos = transform.position;
                pos.y += verticalInput * _flySpeed * Time.deltaTime;
                transform.position = pos;
            }
        }
    }
    
    // Eldritch minion controller
    public class EldrithMinionController : MonoBehaviour
    {
        private PlayerController _owner;
        private float _duration;
        private float _spawnTime;
        private DamageType _damageType;
        private float _damage;
        private float _attackRadius = 2f;
        private float _attackCooldown = 1f;
        private float _lastAttackTime = -999f;
        private float _moveSpeed = 3f;
        private GameObject _target;
        
        public bool IsExpired => Time.time > _spawnTime + _duration;
        
        public void Initialize(PlayerController owner, float duration, DamageType damageType, float damage)
        {
            _owner = owner;
            _duration = duration;
            _spawnTime = Time.time;
            _damageType = damageType;
            _damage = damage;
            
            // Add collider
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            
            // Add rigidbody
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        private void Update()
        {
            // Fade out when nearing expiration
            float remainingTime = (_spawnTime + _duration) - Time.time;
            if (remainingTime < 1f)
            {
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = remainingTime;
                    renderer.material.color = color;
                }
            }
            
            // Find target if none
            if (_target == null)
            {
                FindTarget();
            }
            
            // Move towards target
            if (_target != null)
            {
                MoveTowardsTarget();
                
                // Attack if in range and cooldown elapsed
                if (Time.time > _lastAttackTime + _attackCooldown && IsInAttackRange())
                {
                    Attack();
                }
            }
            else
            {
                // No target, follow owner
                if (_owner != null)
                {
                    Vector3 directionToOwner = (_owner.transform.position - transform.position).normalized;
                    transform.position += directionToOwner * _moveSpeed * 0.5f * Time.deltaTime;
                }
            }
        }
        
        private void FindTarget()
        {
            // Find nearest enemy
            EnemyController[] enemies = FindObjectsOfType<EnemyController>();
            float closestDistance = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        _target = enemy.gameObject;
                    }
                }
            }
        }
        
        private void MoveTowardsTarget()
        {
            if (_target == null) return;
            
            Vector3 direction = (_target.transform.position - transform.position).normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;
            
            // Look at target
            transform.LookAt(_target.transform);
        }
        
        private bool IsInAttackRange()
        {
            if (_target == null) return false;
            
            return Vector3.Distance(transform.position, _target.transform.position) <= _attackRadius;
        }
        
        private void Attack()
        {
            if (_target == null) return;
            
            // Apply damage
            DamageableEntity entity = _target.GetComponent<DamageableEntity>();
            if (entity != null)
            {
                entity.TakeDamage(_damage, _damageType, _owner?.gameObject);
            }
            
            // Create attack effect
            GameObject attackEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            attackEffect.transform.position = transform.position;
            attackEffect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // Set material
            Renderer renderer = attackEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = GetComponent<Renderer>().material.color;
                renderer.material.SetColor("_EmissionColor", GetComponent<Renderer>().material.color * 2f);
                renderer.material.EnableKeyword("_EMISSION");
            }
            
            // Remove collider
            Collider collider = attackEffect.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            
            // Move towards target and self-destruct
            StartCoroutine(MoveEffectToTarget(attackEffect));
            
            // Update cooldown
            _lastAttackTime = Time.time;
        }
        
        private IEnumerator MoveEffectToTarget(GameObject effect)
        {
            if (effect == null || _target == null)
            {
                if (effect != null)
                    Destroy(effect);
                    
                yield break;
            }
            
            Vector3 startPos = effect.transform.position;
            Vector3 targetPos = _target.transform.position;
            float duration = 0.3f;
            float startTime = Time.time;
            
            while (Time.time < startTime + duration)
            {
                if (effect == null || _target == null)
                {
                    if (effect != null)
                        Destroy(effect);
                        
                    yield break;
                }
                
                float t = (Time.time - startTime) / duration;
                effect.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            
            // Create impact effect
            if (_target != null)
            {
                GameObject impactEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                impactEffect.transform.position = _target.transform.position;
                impactEffect.transform.localScale = new Vector3(1f, 1f, 1f);
                
                // Set material
                Renderer renderer = impactEffect.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.color = GetComponent<Renderer>().material.color;
                    renderer.material.SetColor("_EmissionColor", GetComponent<Renderer>().material.color * 3f);
                    renderer.material.EnableKeyword("_EMISSION");
                }
                
                // Remove collider
                Collider collider = impactEffect.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
                
                // Scale up and fade out
                StartCoroutine(ScaleAndFade(impactEffect));
            }
            
            // Destroy effect
            Destroy(effect);
        }
        
        private IEnumerator ScaleAndFade(GameObject effect)
        {
            if (effect == null)
                yield break;
                
            float duration = 0.5f;
            float startTime = Time.time;
            
            while (Time.time < startTime + duration)
            {
                if (effect == null)
                    yield break;
                    
                float t = (Time.time - startTime) / duration;
                effect.transform.localScale = Vector3.Lerp(new Vector3(1f, 1f, 1f), new Vector3(2f, 2f, 2f), t);
                
                Renderer renderer = effect.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = 1f - t;
                    renderer.material.color = color;
                }
                
                yield return null;
            }
            
            // Destroy effect
            Destroy(effect);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw attack radius gizmo
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRadius);
        }
    }
}