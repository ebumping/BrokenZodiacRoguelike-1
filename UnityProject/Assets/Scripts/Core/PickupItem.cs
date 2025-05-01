using UnityEngine;
using System;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    // This class represents an item that can be picked up by the player
    public class PickupItem : MonoBehaviour
    {
        // Item properties
        [Header("Item Properties")]
        [SerializeField] private string itemId = System.Guid.NewGuid().ToString();
        [SerializeField] private string itemName = "Unknown Item";
        [SerializeField] private string itemDescription = "An unknown item";
        [SerializeField] private ItemType itemType = ItemType.Consumable;
        [SerializeField] private ItemRarity itemRarity = ItemRarity.Common;
        [SerializeField] private Sprite itemIcon;
        [SerializeField] private GameObject pickupEffectPrefab;
        
        // Behavior settings
        [Header("Behavior")]
        [SerializeField] private bool autoRotate = true;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private bool bobUpAndDown = true;
        [SerializeField] private float bobHeight = 0.2f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private bool fadeWhenNear = true;
        [SerializeField] private float fadeDistance = 1.5f;
        [SerializeField] private bool showTooltip = true;
        
        // Interaction settings
        [Header("Interaction")]
        [SerializeField] private bool collectOnTriggerEnter = true;
        [SerializeField] private bool requireKeyPress = false;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float interactionRadius = 2f;
        [SerializeField] private bool destroyOnCollect = true;
        
        // Effects settings
        [Header("Effects")]
        [SerializeField] private bool playSound = true;
        [SerializeField] private string soundName = "item_pickup";
        [SerializeField] private bool showParticles = true;
        [SerializeField] private bool flashSprite = true;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.2f;
        
        // References
        private SpriteRenderer _spriteRenderer;
        private Collider _collider;
        private Vector3 _startPosition;
        private Material _originalMaterial;
        private Material _flashMaterial;
        
        // State
        private bool _isCollected = false;
        private float _bobTimer = 0f;
        private float _flashTimer = 0f;
        private bool _isInteractable = true;
        private bool _isFlashing = false;
        
        // Events
        public event Action<PickupItem> OnItemCollected;
        public event Action<PickupItem> OnItemInteracted;
        
        // Properties
        public string ItemId => itemId;
        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public ItemType ItemType => itemType;
        public ItemRarity ItemRarity => itemRarity;
        public Sprite ItemIcon => itemIcon;
        public bool IsCollected => _isCollected;
        
        private void Awake()
        {
            // Cache references
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _collider = GetComponent<Collider>();
            
            // Set up materials if necessary
            if (_spriteRenderer != null && flashSprite)
            {
                _originalMaterial = _spriteRenderer.material;
                _flashMaterial = new Material(_originalMaterial);
                _flashMaterial.SetColor("_Color", flashColor);
            }
            
            // Store starting position for bob effect
            _startPosition = transform.position;
            
            // Randomize bob timer for variety
            _bobTimer = UnityEngine.Random.Range(0f, 6.28f);
        }
        
        private void Update()
        {
            if (_isCollected) return;
            
            // Handle automatic animations
            if (autoRotate)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
            
            if (bobUpAndDown)
            {
                _bobTimer += Time.deltaTime * bobSpeed;
                Vector3 newPos = _startPosition;
                newPos.y += Mathf.Sin(_bobTimer) * bobHeight;
                transform.position = newPos;
            }
            
            // Handle player interaction
            if (requireKeyPress && _isInteractable)
            {
                HandleInteraction();
            }
            
            // Handle sprite fade when player is near
            if (fadeWhenNear && _spriteRenderer != null)
            {
                HandleProximityFade();
            }
            
            // Handle flash effect
            if (_isFlashing && _spriteRenderer != null)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0)
                {
                    _isFlashing = false;
                    _spriteRenderer.material = _originalMaterial;
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if this is a player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && collectOnTriggerEnter && !_isCollected)
            {
                CollectItem(player);
            }
        }
        
        private void HandleInteraction()
        {
            // Find nearby players
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRadius, LayerMask.GetMask("Player"));
            foreach (var hitCollider in hitColliders)
            {
                PlayerController player = hitCollider.GetComponent<PlayerController>();
                if (player != null && Input.GetKeyDown(interactKey))
                {
                    CollectItem(player);
                    break;
                }
            }
        }
        
        private void HandleProximityFade()
        {
            // Find closest player
            PlayerController closestPlayer = FindClosestPlayer();
            if (closestPlayer != null)
            {
                float distance = Vector3.Distance(transform.position, closestPlayer.transform.position);
                if (distance < fadeDistance)
                {
                    // Calculate alpha based on distance
                    float alpha = Mathf.Clamp01(distance / fadeDistance);
                    Color color = _spriteRenderer.color;
                    color.a = alpha;
                    _spriteRenderer.color = color;
                }
                else
                {
                    // Reset alpha
                    Color color = _spriteRenderer.color;
                    color.a = 1f;
                    _spriteRenderer.color = color;
                }
            }
        }
        
        private PlayerController FindClosestPlayer()
        {
            PlayerController closest = null;
            float closestDistance = float.MaxValue;
            
            // Find all players
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            foreach (var player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = player;
                }
            }
            
            return closest;
        }
        
        // Public method to collect this item
        public void CollectItem(PlayerController player)
        {
            if (_isCollected) return;
            
            _isCollected = true;
            _isInteractable = false;
            
            // Trigger effects
            if (flashSprite && _spriteRenderer != null)
            {
                _flashTimer = flashDuration;
                _isFlashing = true;
                _spriteRenderer.material = _flashMaterial;
            }
            
            if (showParticles && pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }
            
            if (playSound)
            {
                // Play sound via Audio Manager
                GameManager.Instance?.PlaySound(soundName);
            }
            
            // Add item to player's inventory
            player.AddItem(this);
            
            // Trigger events
            OnItemCollected?.Invoke(this);
            
            // Remove collider
            if (_collider != null)
            {
                _collider.enabled = false;
            }
            
            // Handle destruction if needed
            if (destroyOnCollect)
            {
                // Allow time for effects before destroying
                Invoke(nameof(DestroyItem), flashDuration + 0.1f);
            }
            else
            {
                // Just hide the sprite
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.enabled = false;
                }
            }
        }
        
        private void DestroyItem()
        {
            Destroy(gameObject);
        }
        
        // Called when player interacts but doesn't collect
        public void Interact(PlayerController player)
        {
            if (_isCollected) return;
            
            OnItemInteracted?.Invoke(this);
        }
        
        // Editor tools
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
    
    // Item type enumeration
    public enum ItemType
    {
        Consumable,
        Equipment,
        Weapon,
        TarotCard,
        ZodiacSignil,
        Key,
        QuestItem,
        Currency
    }
    
    // Item rarity enumeration
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Cosmic
    }
}