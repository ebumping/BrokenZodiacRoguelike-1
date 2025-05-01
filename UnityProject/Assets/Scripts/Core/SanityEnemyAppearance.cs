using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Core
{
    // This component modifies enemy appearances based on player's sanity
    [RequireComponent(typeof(Renderer))]
    public class SanityEnemyAppearance : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyController enemyController;
        [SerializeField] private Renderer[] additionalRenderers;
        
        [Header("Sanity Appearance Settings")]
        [SerializeField] private bool useDistortedMesh = true;
        [SerializeField] private Mesh[] distortedMeshes; // Different meshes for different sanity levels
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material waryMaterial;
        [SerializeField] private Material disturbedMaterial;
        [SerializeField] private Material unstableMaterial;
        [SerializeField] private Material brokenMaterial;
        
        [Header("Animation Settings")]
        [SerializeField] private bool useAnimationDistortion = true;
        [SerializeField] private float normalAnimationSpeed = 1f;
        [SerializeField] private float lowSanityAnimationSpeed = 1.5f;
        [SerializeField] private float normalTransitionSpeed = 0.5f;
        [SerializeField] private float lowSanityTransitionSpeed = 0.2f;
        
        [Header("VFX Settings")]
        [SerializeField] private ParticleSystem[] normalParticles;
        [SerializeField] private ParticleSystem[] lowSanityParticles;
        [SerializeField] private GameObject[] normalVfx;
        [SerializeField] private GameObject[] lowSanityVfx;
        
        // Runtime variables
        private Renderer _mainRenderer;
        private SanityState _currentState = SanityState.Normal;
        private MeshFilter _meshFilter;
        private Mesh _originalMesh;
        private Material _originalMaterial;
        private Animator _animator;
        private float _originalAnimSpeed;
        private Dictionary<Renderer, Material> _originalMaterials = new Dictionary<Renderer, Material>();
        
        private void Awake()
        {
            // Get references
            _mainRenderer = GetComponent<Renderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _animator = GetComponent<Animator>();
            
            // Cache original values
            if (_meshFilter != null)
            {
                _originalMesh = _meshFilter.sharedMesh;
            }
            
            if (_mainRenderer != null)
            {
                _originalMaterial = _mainRenderer.sharedMaterial;
                _originalMaterials[_mainRenderer] = _originalMaterial;
            }
            
            // Cache additional renderer materials
            if (additionalRenderers != null)
            {
                foreach (var renderer in additionalRenderers)
                {
                    if (renderer != null)
                    {
                        _originalMaterials[renderer] = renderer.sharedMaterial;
                    }
                }
            }
            
            // Cache animation speed
            if (_animator != null)
            {
                _originalAnimSpeed = _animator.speed;
            }
            
            // Get enemy controller if not assigned
            if (enemyController == null)
            {
                enemyController = GetComponent<EnemyController>();
            }
        }
        
        private void OnEnable()
        {
            // Subscribe to sanity events
            if (SanitySystem.Instance != null)
            {
                SanitySystem.Instance.OnPlayerSanityStateChanged += OnSanityStateChanged;
                
                // Initialize with current state
                int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
                if (localPlayerId != 0)
                {
                    SanityState state = SanitySystem.Instance.GetPlayerSanityState(localPlayerId);
                    UpdateAppearance(state);
                }
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            if (SanitySystem.Instance != null)
            {
                SanitySystem.Instance.OnPlayerSanityStateChanged -= OnSanityStateChanged;
            }
            
            // Restore original appearance
            RestoreOriginalAppearance();
        }
        
        // Handle sanity state changed
        private void OnSanityStateChanged(int playerId, SanityState oldState, SanityState newState)
        {
            // Only respond to local player
            int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
            if (playerId != localPlayerId)
                return;
                
            UpdateAppearance(newState);
        }
        
        // Update enemy appearance based on sanity state
        private void UpdateAppearance(SanityState state)
        {
            // If enemy is dead or not active, don't change appearance
            if (enemyController != null && enemyController.IsDead)
                return;
                
            // Store the new state
            _currentState = state;
            
            // Set mesh if enabled
            if (useDistortedMesh && _meshFilter != null && distortedMeshes.Length > 0)
            {
                UpdateMesh(state);
            }
            
            // Set material based on state
            UpdateMaterial(state);
            
            // Update animation speed
            if (useAnimationDistortion && _animator != null)
            {
                UpdateAnimation(state);
            }
            
            // Update particles and VFX
            UpdateVFX(state);
        }
        
        // Update mesh based on sanity state
        private void UpdateMesh(SanityState state)
        {
            // Only change mesh for low sanity states
            if (state < SanityState.Disturbed)
            {
                _meshFilter.mesh = _originalMesh;
                return;
            }
            
            // Pick a mesh based on state
            int index = (int)state - (int)SanityState.Disturbed;
            if (index >= 0 && index < distortedMeshes.Length && distortedMeshes[index] != null)
            {
                _meshFilter.mesh = distortedMeshes[index];
            }
        }
        
        // Update material based on sanity state
        private void UpdateMaterial(SanityState state)
        {
            // Select material based on state
            Material targetMaterial = null;
            
            switch (state)
            {
                case SanityState.Normal:
                    targetMaterial = normalMaterial ?? _originalMaterial;
                    break;
                case SanityState.Wary:
                    targetMaterial = waryMaterial ?? _originalMaterial;
                    break;
                case SanityState.Disturbed:
                    targetMaterial = disturbedMaterial ?? _originalMaterial;
                    break;
                case SanityState.Unstable:
                    targetMaterial = unstableMaterial ?? _originalMaterial;
                    break;
                case SanityState.Broken:
                    targetMaterial = brokenMaterial ?? _originalMaterial;
                    break;
            }
            
            // Apply material to main renderer
            if (_mainRenderer != null && targetMaterial != null)
            {
                _mainRenderer.material = targetMaterial;
            }
            
            // Apply to additional renderers
            if (additionalRenderers != null)
            {
                foreach (var renderer in additionalRenderers)
                {
                    if (renderer != null && targetMaterial != null)
                    {
                        renderer.material = targetMaterial;
                    }
                }
            }
        }
        
        // Update animation based on sanity state
        private void UpdateAnimation(SanityState state)
        {
            if (_animator == null)
                return;
                
            // Adjust animation speed based on sanity
            switch (state)
            {
                case SanityState.Normal:
                case SanityState.Wary:
                    _animator.speed = normalAnimationSpeed;
                    break;
                    
                case SanityState.Disturbed:
                    _animator.speed = Mathf.Lerp(normalAnimationSpeed, lowSanityAnimationSpeed, 0.3f);
                    break;
                    
                case SanityState.Unstable:
                    _animator.speed = Mathf.Lerp(normalAnimationSpeed, lowSanityAnimationSpeed, 0.6f);
                    break;
                    
                case SanityState.Broken:
                    _animator.speed = lowSanityAnimationSpeed;
                    break;
            }
            
            // Maybe add some animation glitching for very low sanity
            if (state >= SanityState.Unstable)
            {
                // Start a coroutine to randomly glitch the animation
                StartCoroutine(GlitchAnimation());
            }
        }
        
        // Update VFX based on sanity state
        private void UpdateVFX(SanityState state)
        {
            // Normal particles
            if (normalParticles != null)
            {
                foreach (var particles in normalParticles)
                {
                    if (particles != null)
                    {
                        if (state <= SanityState.Wary)
                            particles.Play();
                        else
                            particles.Stop();
                    }
                }
            }
            
            // Low sanity particles
            if (lowSanityParticles != null)
            {
                foreach (var particles in lowSanityParticles)
                {
                    if (particles != null)
                    {
                        if (state >= SanityState.Disturbed)
                            particles.Play();
                        else
                            particles.Stop();
                    }
                }
            }
            
            // Normal VFX objects
            if (normalVfx != null)
            {
                foreach (var vfx in normalVfx)
                {
                    if (vfx != null)
                    {
                        vfx.SetActive(state <= SanityState.Wary);
                    }
                }
            }
            
            // Low sanity VFX objects
            if (lowSanityVfx != null)
            {
                foreach (var vfx in lowSanityVfx)
                {
                    if (vfx != null)
                    {
                        vfx.SetActive(state >= SanityState.Disturbed);
                    }
                }
            }
        }
        
        // Random animation glitching effect
        private IEnumerator GlitchAnimation()
        {
            if (_animator == null) yield break;
            
            // Store original speed
            float originalSpeed = _animator.speed;
            
            // Random times between glitches
            float glitchInterval = Random.Range(0.5f, 2.0f);
            yield return new WaitForSeconds(glitchInterval);
            
            // Only continue if we're still in a bad state
            if (_currentState < SanityState.Unstable) yield break;
            
            // Glitch the animation
            float glitchDuration = Random.Range(0.1f, 0.3f);
            
            // Randomly choose a glitch effect
            int glitchType = Random.Range(0, 3);
            
            switch (glitchType)
            {
                case 0: // Speed up
                    _animator.speed = originalSpeed * Random.Range(2f, 4f);
                    break;
                case 1: // Slow down
                    _animator.speed = originalSpeed * Random.Range(0.2f, 0.5f);
                    break;
                case 2: // Pause
                    _animator.speed = 0;
                    break;
            }
            
            yield return new WaitForSeconds(glitchDuration);
            
            // Restore speed
            _animator.speed = originalSpeed;
            
            // Possibly recurse for continuous glitching if still in a bad state
            if (_currentState >= SanityState.Unstable)
            {
                StartCoroutine(GlitchAnimation());
            }
        }
        
        // Restore original appearance
        private void RestoreOriginalAppearance()
        {
            // Restore mesh
            if (_meshFilter != null && _originalMesh != null)
            {
                _meshFilter.mesh = _originalMesh;
            }
            
            // Restore materials
            foreach (var entry in _originalMaterials)
            {
                if (entry.Key != null && entry.Value != null)
                {
                    entry.Key.material = entry.Value;
                }
            }
            
            // Restore animation speed
            if (_animator != null)
            {
                _animator.speed = _originalAnimSpeed;
            }
            
            // Stop all special VFX
            if (lowSanityParticles != null)
            {
                foreach (var particles in lowSanityParticles)
                {
                    if (particles != null)
                        particles.Stop();
                }
            }
            
            if (lowSanityVfx != null)
            {
                foreach (var vfx in lowSanityVfx)
                {
                    if (vfx != null)
                        vfx.SetActive(false);
                }
            }
        }
    }
}