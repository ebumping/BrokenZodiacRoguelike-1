using UnityEngine;
using CodexOfTheBrokenZodiac.Core;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources.CosmicMadnessAbilities
{
    // Grants the ability to see hidden enemies, items, and passages
    [CreateAssetMenu(fileName = "Eldritch Vision", menuName = "Codex/Cosmic Madness/Abilities/Eldritch Vision")]
    public class EldrithVisionAbility : CosmicMadnessAbility
    {
        [Header("Vision Settings")]
        [SerializeField] private float visionRange = 15f;
        [SerializeField] private float trapDetectionRange = 10f;
        [SerializeField] private float hiddenItemDetectionRange = 8f;
        [SerializeField] private bool revealHiddenEnemies = true;
        [SerializeField] private bool revealHiddenPassages = true;
        [SerializeField] private bool revealTraps = true;
        [SerializeField] private bool revealHiddenItems = true;
        
        [Header("Visual Effects")]
        [SerializeField] private Color enemyOutlineColor = new Color(1f, 0, 0, 0.7f);
        [SerializeField] private Color passageOutlineColor = new Color(0, 1f, 0.5f, 0.7f);
        [SerializeField] private Color trapOutlineColor = new Color(1f, 0.5f, 0, 0.7f);
        [SerializeField] private Color itemOutlineColor = new Color(0, 0.5f, 1f, 0.7f);
        [SerializeField] private float outlineWidth = 0.2f;
        [SerializeField] private float pulseSpeed = 1.5f;
        
        [Header("Zodiac Effects")]
        [SerializeField] private float geminiRangeBonus = 0.5f; // +50% range
        [SerializeField] private float sagittariusRangeBonus = 0.3f; // +30% range
        [SerializeField] private float piscesEnemySenseBonus = 1.0f; // Double enemy detection range
        [SerializeField] private float virgoItemSenseBonus = 0.5f; // +50% item detection range
        [SerializeField] private float libraRevealDuration = 2f; // Duration of revealed items for Libra
        
        // Runtime variables
        private EldrithVisionComponent _visionComponent;
        
        // Apply the vision effect to the player
        protected override void ApplyEffect(PlayerController player)
        {
            if (player == null) return;
            
            // Add vision component if not already present
            _visionComponent = player.gameObject.GetComponent<EldrithVisionComponent>();
            if (_visionComponent == null)
            {
                _visionComponent = player.gameObject.AddComponent<EldrithVisionComponent>();
            }
            
            // Configure the component
            ZodiacSign playerSign = player.GetZodiacSign();
            float rangeMultiplier = 1f;
            float itemRangeMultiplier = 1f;
            float enemyRangeMultiplier = 1f;
            bool revealTemporarily = false;
            
            // Apply zodiac-specific bonuses
            switch (playerSign)
            {
                case ZodiacSign.Gemini: // Better at seeing everything
                    rangeMultiplier += geminiRangeBonus;
                    break;
                    
                case ZodiacSign.Sagittarius: // Better at seeing distant things
                    rangeMultiplier += sagittariusRangeBonus;
                    break;
                    
                case ZodiacSign.Pisces: // Better at seeing enemies
                    enemyRangeMultiplier += piscesEnemySenseBonus;
                    break;
                    
                case ZodiacSign.Virgo: // Better at finding items
                    itemRangeMultiplier += virgoItemSenseBonus;
                    break;
                    
                case ZodiacSign.Libra: // Temporarily reveals things
                    revealTemporarily = true;
                    break;
            }
            
            // Apply settings
            _visionComponent.Initialize(
                visionRange * rangeMultiplier,
                trapDetectionRange * rangeMultiplier,
                hiddenItemDetectionRange * itemRangeMultiplier,
                visionRange * enemyRangeMultiplier, // Enemy range
                revealHiddenEnemies,
                revealHiddenPassages,
                revealTraps,
                revealHiddenItems,
                enemyOutlineColor,
                passageOutlineColor,
                trapOutlineColor,
                itemOutlineColor,
                outlineWidth,
                pulseSpeed,
                revealTemporarily,
                libraRevealDuration
            );
            
            // Start the effect
            _visionComponent.EnableVision();
        }
        
        // Remove the vision effect
        protected override void RemoveEffect(PlayerController player)
        {
            if (player == null) return;
            
            // Disable the vision component
            EldrithVisionComponent visionComponent = player.gameObject.GetComponent<EldrithVisionComponent>();
            if (visionComponent != null)
            {
                visionComponent.DisableVision();
            }
        }
        
        // Return zodiac-specific details for the tooltip
        protected override string GetZodiacSpecificDetails(ZodiacSign sign)
        {
            switch (sign)
            {
                case ZodiacSign.Gemini:
                    return $"Gemini: +{geminiRangeBonus * 100}% vision range for all detections.";
                    
                case ZodiacSign.Sagittarius:
                    return $"Sagittarius: +{sagittariusRangeBonus * 100}% vision range for all detections.";
                    
                case ZodiacSign.Pisces:
                    return $"Pisces: +{piscesEnemySenseBonus * 100}% enemy detection range.";
                    
                case ZodiacSign.Virgo:
                    return $"Virgo: +{virgoItemSenseBonus * 100}% hidden item detection range.";
                    
                case ZodiacSign.Libra:
                    return $"Libra: Temporarily reveals hidden objects for {libraRevealDuration} seconds.";
                    
                default:
                    return "";
            }
        }
    }
    
    // Component that handles the eldritch vision effect
    public class EldrithVisionComponent : MonoBehaviour
    {
        // Configuration
        private float _visionRange;
        private float _trapDetectionRange;
        private float _itemDetectionRange;
        private float _enemyDetectionRange;
        private bool _revealHiddenEnemies;
        private bool _revealHiddenPassages;
        private bool _revealTraps;
        private bool _revealHiddenItems;
        private Color _enemyOutlineColor;
        private Color _passageOutlineColor;
        private Color _trapOutlineColor;
        private Color _itemOutlineColor;
        private float _outlineWidth;
        private float _pulseSpeed;
        private bool _revealTemporarily;
        private float _revealDuration;
        
        // Runtime variables
        private bool _isActive = false;
        private List<GameObject> _revealedObjects = new List<GameObject>();
        private List<OutlineEffect> _outlineEffects = new List<OutlineEffect>();
        private float _timeSinceLastPulse = 0f;
        private const float _pulseInterval = 2f;
        
        // Initialize the component
        public void Initialize(
            float visionRange,
            float trapDetectionRange,
            float itemDetectionRange,
            float enemyDetectionRange,
            bool revealHiddenEnemies,
            bool revealHiddenPassages,
            bool revealTraps,
            bool revealHiddenItems,
            Color enemyOutlineColor,
            Color passageOutlineColor,
            Color trapOutlineColor,
            Color itemOutlineColor,
            float outlineWidth,
            float pulseSpeed,
            bool revealTemporarily,
            float revealDuration)
        {
            _visionRange = visionRange;
            _trapDetectionRange = trapDetectionRange;
            _itemDetectionRange = itemDetectionRange;
            _enemyDetectionRange = enemyDetectionRange;
            _revealHiddenEnemies = revealHiddenEnemies;
            _revealHiddenPassages = revealHiddenPassages;
            _revealTraps = revealTraps;
            _revealHiddenItems = revealHiddenItems;
            _enemyOutlineColor = enemyOutlineColor;
            _passageOutlineColor = passageOutlineColor;
            _trapOutlineColor = trapOutlineColor;
            _itemOutlineColor = itemOutlineColor;
            _outlineWidth = outlineWidth;
            _pulseSpeed = pulseSpeed;
            _revealTemporarily = revealTemporarily;
            _revealDuration = revealDuration;
        }
        
        private void Update()
        {
            if (!_isActive) return;
            
            // Pulse detection every few seconds
            _timeSinceLastPulse += Time.deltaTime;
            if (_timeSinceLastPulse >= _pulseInterval)
            {
                PulseDetection();
                _timeSinceLastPulse = 0f;
            }
            
            // Update outline effects
            UpdateOutlineEffects();
        }
        
        // Enable the vision effect
        public void EnableVision()
        {
            if (_isActive) return;
            
            _isActive = true;
            
            // Perform initial detection
            PulseDetection();
        }
        
        // Disable the vision effect
        public void DisableVision()
        {
            if (!_isActive) return;
            
            _isActive = false;
            
            // Remove all outline effects
            foreach (var effect in _outlineEffects)
            {
                if (effect != null && effect.gameObject != null)
                {
                    Destroy(effect);
                }
            }
            
            _outlineEffects.Clear();
            _revealedObjects.Clear();
        }
        
        // Pulse to detect hidden objects
        private void PulseDetection()
        {
            if (!_isActive) return;
            
            // Find all hidden objects in range
            if (_revealHiddenEnemies)
            {
                DetectHiddenEnemies();
            }
            
            if (_revealHiddenPassages)
            {
                DetectHiddenPassages();
            }
            
            if (_revealTraps)
            {
                DetectTraps();
            }
            
            if (_revealHiddenItems)
            {
                DetectHiddenItems();
            }
        }
        
        // Detect hidden enemies
        private void DetectHiddenEnemies()
        {
            // Find all enemies in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, _enemyDetectionRange);
            
            foreach (var collider in colliders)
            {
                // Check if it's a hidden enemy
                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy != null && enemy.IsHidden)
                {
                    // Apply outline effect
                    AddOutlineEffect(enemy.gameObject, _enemyOutlineColor, ObjectType.Enemy);
                }
            }
        }
        
        // Detect hidden passages
        private void DetectHiddenPassages()
        {
            // Find all hidden passages in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, _visionRange);
            
            foreach (var collider in colliders)
            {
                // Check if it's a hidden passage
                HiddenPassage passage = collider.GetComponent<HiddenPassage>();
                if (passage != null && !passage.IsRevealed)
                {
                    // Apply outline effect
                    AddOutlineEffect(passage.gameObject, _passageOutlineColor, ObjectType.Passage);
                }
            }
        }
        
        // Detect traps
        private void DetectTraps()
        {
            // Find all traps in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, _trapDetectionRange);
            
            foreach (var collider in colliders)
            {
                // Check if it's a trap
                TrapController trap = collider.GetComponent<TrapController>();
                if (trap != null && !trap.IsVisible)
                {
                    // Apply outline effect
                    AddOutlineEffect(trap.gameObject, _trapOutlineColor, ObjectType.Trap);
                }
            }
        }
        
        // Detect hidden items
        private void DetectHiddenItems()
        {
            // Find all hidden items in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, _itemDetectionRange);
            
            foreach (var collider in colliders)
            {
                // Check if it's a hidden item
                PickupItem item = collider.GetComponent<PickupItem>();
                if (item != null && item.IsHidden)
                {
                    // Apply outline effect
                    AddOutlineEffect(item.gameObject, _itemOutlineColor, ObjectType.Item);
                }
            }
        }
        
        // Add outline effect to an object
        private void AddOutlineEffect(GameObject obj, Color color, ObjectType type)
        {
            if (obj == null || _revealedObjects.Contains(obj))
                return;
                
            // Add to list
            _revealedObjects.Add(obj);
            
            // Add outline effect
            OutlineEffect effect = obj.GetComponent<OutlineEffect>();
            if (effect == null)
            {
                effect = obj.AddComponent<OutlineEffect>();
            }
            
            // Configure effect
            effect.OutlineColor = color;
            effect.OutlineWidth = _outlineWidth;
            effect.PulseSpeed = _pulseSpeed;
            effect.ObjectType = type;
            
            if (_revealTemporarily)
            {
                effect.IsTemporary = true;
                effect.RemainingTime = _revealDuration;
            }
            
            // Enable the effect
            effect.Enable();
            
            // Add to tracked effects
            _outlineEffects.Add(effect);
        }
        
        // Update outline effects
        private void UpdateOutlineEffects()
        {
            for (int i = _outlineEffects.Count - 1; i >= 0; i--)
            {
                OutlineEffect effect = _outlineEffects[i];
                
                if (effect == null || effect.gameObject == null)
                {
                    _outlineEffects.RemoveAt(i);
                    continue;
                }
                
                // Check if temporary effect has expired
                if (effect.IsTemporary && effect.RemainingTime <= 0)
                {
                    effect.Disable();
                    _revealedObjects.Remove(effect.gameObject);
                    _outlineEffects.RemoveAt(i);
                }
                
                // Check if object is out of range
                else if (!IsInRange(effect.gameObject, effect.ObjectType))
                {
                    effect.Disable();
                    _revealedObjects.Remove(effect.gameObject);
                    _outlineEffects.RemoveAt(i);
                }
            }
        }
        
        // Check if an object is in range
        private bool IsInRange(GameObject obj, ObjectType type)
        {
            if (obj == null) return false;
            
            float range = _visionRange;
            
            // Use specific range based on object type
            switch (type)
            {
                case ObjectType.Enemy:
                    range = _enemyDetectionRange;
                    break;
                case ObjectType.Trap:
                    range = _trapDetectionRange;
                    break;
                case ObjectType.Item:
                    range = _itemDetectionRange;
                    break;
            }
            
            return Vector3.Distance(transform.position, obj.transform.position) <= range;
        }
        
        private void OnDestroy()
        {
            // Clean up effects
            foreach (var effect in _outlineEffects)
            {
                if (effect != null)
                {
                    Destroy(effect);
                }
            }
        }
    }
    
    // Types of objects that can be revealed
    public enum ObjectType
    {
        Enemy,
        Passage,
        Trap,
        Item
    }
    
    // Component for rendering outline effects
    public class OutlineEffect : MonoBehaviour
    {
        public Color OutlineColor { get; set; } = Color.white;
        public float OutlineWidth { get; set; } = 0.1f;
        public float PulseSpeed { get; set; } = 1f;
        public ObjectType ObjectType { get; set; }
        public bool IsTemporary { get; set; } = false;
        public float RemainingTime { get; set; } = 0f;
        
        private Material _outlineMaterial;
        private Renderer[] _renderers;
        private Material[] _originalMaterials;
        
        public void Enable()
        {
            // Store original renderers and materials
            _renderers = GetComponentsInChildren<Renderer>();
            _originalMaterials = new Material[_renderers.Length];
            
            // Create outline material
            if (_outlineMaterial == null)
            {
                _outlineMaterial = new Material(Shader.Find("Standard"));
                _outlineMaterial.SetFloat("_Mode", 3); // Transparent mode
                _outlineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _outlineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _outlineMaterial.SetInt("_ZWrite", 0);
                _outlineMaterial.DisableKeyword("_ALPHATEST_ON");
                _outlineMaterial.EnableKeyword("_ALPHABLEND_ON");
                _outlineMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                _outlineMaterial.renderQueue = 3000;
            }
            
            // Apply outline material
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _originalMaterials[i] = _renderers[i].material;
                    
                    // Create instance of outline material
                    Material outlineMat = new Material(_outlineMaterial);
                    outlineMat.color = OutlineColor;
                    
                    // Apply to renderer
                    _renderers[i].material = outlineMat;
                }
            }
        }
        
        public void Disable()
        {
            // Restore original materials
            if (_renderers != null && _originalMaterials != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null && i < _originalMaterials.Length && _originalMaterials[i] != null)
                    {
                        _renderers[i].material = _originalMaterials[i];
                    }
                }
            }
            
            // Destroy self
            Destroy(this);
        }
        
        private void Update()
        {
            // Update temporary timer
            if (IsTemporary)
            {
                RemainingTime -= Time.deltaTime;
            }
            
            // Pulse the outline
            float pulse = 0.7f + 0.3f * Mathf.Sin(Time.time * PulseSpeed);
            
            // Update material color
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null)
                {
                    Color color = OutlineColor;
                    color.a *= pulse;
                    _renderers[i].material.color = color;
                }
            }
        }
    }
}