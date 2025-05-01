using UnityEngine;
using System.Collections;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.Resources.CosmicMadnessAbilities
{
    // Ability that allows the player to commune with the void and gain knowledge or attack enemies
    [CreateAssetMenu(fileName = "Void Whisper", menuName = "Codex/Cosmic Madness/Abilities/Void Whisper")]
    public class VoidWhisperAbility : CosmicMadnessAbility
    {
        [Header("Whisper Settings")]
        [SerializeField] private float channelTime = 2f;
        [SerializeField] private float whisperRadius = 10f;
        [SerializeField] private float knowledgeChance = 0.6f;
        [SerializeField] private float attackChance = 0.4f;
        
        [Header("Knowledge Effects")]
        [SerializeField] private bool revealMapRoom = true;
        [SerializeField] private bool revealEnemiesTemporarily = true;
        [SerializeField] private bool revealItemsTemporarily = true;
        [SerializeField] private float tempRevealDuration = 10f;
        
        [Header("Attack Effects")]
        [SerializeField] private float attackDamage = 20f;
        [SerializeField] private float attackRadius = 5f;
        [SerializeField] private float attackSanityDrain = 5f;
        [SerializeField] private DamageType damageType = DamageType.Void;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject channelVFXPrefab;
        [SerializeField] private GameObject knowledgeVFXPrefab;
        [SerializeField] private GameObject attackVFXPrefab;
        [SerializeField] private AudioClip channelSFX;
        [SerializeField] private AudioClip knowledgeSFX;
        [SerializeField] private AudioClip attackSFX;
        
        [Header("Zodiac Effects")]
        [SerializeField] private float cancerDefensiveBonus = 0.5f; // +50% defense during channel
        [SerializeField] private float scorpioDamageBonus = 0.3f; // +30% attack damage
        [SerializeField] private float leoKnowledgeBonus = 0.2f; // +20% knowledge chance
        [SerializeField] private float capricornChannelReduction = 0.3f; // -30% channel time
        [SerializeField] private float aquariusRadiusBonus = 0.5f; // +50% effect radius
        
        // Runtime variables
        private VoidWhisperComponent _whisperComponent;
        
        // Apply the whisper effect
        protected override void ApplyEffect(PlayerController player)
        {
            if (player == null) return;
            
            // Add whisper component if not already present
            _whisperComponent = player.gameObject.GetComponent<VoidWhisperComponent>();
            if (_whisperComponent == null)
            {
                _whisperComponent = player.gameObject.AddComponent<VoidWhisperComponent>();
            }
            
            // Configure based on zodiac sign
            ZodiacSign playerSign = player.GetZodiacSign();
            float modifiedChannelTime = channelTime;
            float modifiedWhisperRadius = whisperRadius;
            float modifiedKnowledgeChance = knowledgeChance;
            float modifiedAttackChance = attackChance;
            float modifiedAttackDamage = attackDamage;
            float modifiedDefenseBonus = 0f;
            
            // Apply zodiac-specific effects
            switch (playerSign)
            {
                case ZodiacSign.Cancer: // More defensive during channeling
                    modifiedDefenseBonus = cancerDefensiveBonus;
                    break;
                    
                case ZodiacSign.Scorpio: // More damage on attacks
                    modifiedAttackDamage *= (1 + scorpioDamageBonus);
                    break;
                    
                case ZodiacSign.Leo: // Better chance for knowledge
                    modifiedKnowledgeChance += leoKnowledgeBonus;
                    modifiedAttackChance = 1 - modifiedKnowledgeChance; // Ensure they sum to 1
                    break;
                    
                case ZodiacSign.Capricorn: // Faster channeling
                    modifiedChannelTime *= (1 - capricornChannelReduction);
                    break;
                    
                case ZodiacSign.Aquarius: // Larger radius
                    modifiedWhisperRadius *= (1 + aquariusRadiusBonus);
                    break;
            }
            
            // Configure the component
            _whisperComponent.Initialize(
                modifiedChannelTime,
                modifiedWhisperRadius,
                modifiedKnowledgeChance,
                modifiedAttackChance,
                modifiedAttackDamage,
                attackRadius,
                attackSanityDrain,
                damageType,
                revealMapRoom,
                revealEnemiesTemporarily,
                revealItemsTemporarily,
                tempRevealDuration,
                modifiedDefenseBonus,
                channelVFXPrefab,
                knowledgeVFXPrefab,
                attackVFXPrefab,
                channelSFX,
                knowledgeSFX,
                attackSFX
            );
            
            // Enable the ability
            _whisperComponent.EnableWhisper();
        }
        
        // Remove the whisper effect
        protected override void RemoveEffect(PlayerController player)
        {
            if (player == null) return;
            
            // Disable whisper component
            VoidWhisperComponent whisperComponent = player.gameObject.GetComponent<VoidWhisperComponent>();
            if (whisperComponent != null)
            {
                whisperComponent.DisableWhisper();
            }
        }
        
        // Return zodiac-specific details
        protected override string GetZodiacSpecificDetails(ZodiacSign sign)
        {
            switch (sign)
            {
                case ZodiacSign.Cancer:
                    return $"Cancer: +{cancerDefensiveBonus * 100}% defense while channeling the void.";
                    
                case ZodiacSign.Scorpio:
                    return $"Scorpio: +{scorpioDamageBonus * 100}% damage with void attacks.";
                    
                case ZodiacSign.Leo:
                    return $"Leo: +{leoKnowledgeBonus * 100}% chance to gain knowledge instead of attacking.";
                    
                case ZodiacSign.Capricorn:
                    return $"Capricorn: -{capricornChannelReduction * 100}% channeling time.";
                    
                case ZodiacSign.Aquarius:
                    return $"Aquarius: +{aquariusRadiusBonus * 100}% effect radius.";
                    
                default:
                    return "";
            }
        }
    }
    
    // Component that handles the void whisper effect
    public class VoidWhisperComponent : MonoBehaviour
    {
        // Configuration
        private float _channelTime;
        private float _whisperRadius;
        private float _knowledgeChance;
        private float _attackChance;
        private float _attackDamage;
        private float _attackRadius;
        private float _attackSanityDrain;
        private DamageType _damageType;
        private bool _revealMapRoom;
        private bool _revealEnemiesTemporarily;
        private bool _revealItemsTemporarily;
        private float _tempRevealDuration;
        private float _defenseBonus;
        private GameObject _channelVFXPrefab;
        private GameObject _knowledgeVFXPrefab;
        private GameObject _attackVFXPrefab;
        private AudioClip _channelSFX;
        private AudioClip _knowledgeSFX;
        private AudioClip _attackSFX;
        
        // Runtime variables
        private bool _isEnabled = false;
        private bool _isChanneling = false;
        private GameObject _channelVFX;
        private AudioSource _audioSource;
        private PlayerController _player;
        private DamageModifier _defenseMod;
        
        // Initialize the component
        public void Initialize(
            float channelTime,
            float whisperRadius,
            float knowledgeChance,
            float attackChance,
            float attackDamage,
            float attackRadius,
            float attackSanityDrain,
            DamageType damageType,
            bool revealMapRoom,
            bool revealEnemiesTemporarily,
            bool revealItemsTemporarily,
            float tempRevealDuration,
            float defenseBonus,
            GameObject channelVFXPrefab,
            GameObject knowledgeVFXPrefab,
            GameObject attackVFXPrefab,
            AudioClip channelSFX,
            AudioClip knowledgeSFX,
            AudioClip attackSFX)
        {
            _channelTime = channelTime;
            _whisperRadius = whisperRadius;
            _knowledgeChance = knowledgeChance;
            _attackChance = attackChance;
            _attackDamage = attackDamage;
            _attackRadius = attackRadius;
            _attackSanityDrain = attackSanityDrain;
            _damageType = damageType;
            _revealMapRoom = revealMapRoom;
            _revealEnemiesTemporarily = revealEnemiesTemporarily;
            _revealItemsTemporarily = revealItemsTemporarily;
            _tempRevealDuration = tempRevealDuration;
            _defenseBonus = defenseBonus;
            _channelVFXPrefab = channelVFXPrefab;
            _knowledgeVFXPrefab = knowledgeVFXPrefab;
            _attackVFXPrefab = attackVFXPrefab;
            _channelSFX = channelSFX;
            _knowledgeSFX = knowledgeSFX;
            _attackSFX = attackSFX;
            
            // Get player reference
            _player = GetComponent<PlayerController>();
            
            // Create audio source if needed
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 1f;
                _audioSource.volume = 0.8f;
            }
        }
        
        private void Update()
        {
            if (!_isEnabled) return;
            
            // Check for activation input
            if (Input.GetKeyDown(KeyCode.V))
            {
                // Start channeling the void
                if (!_isChanneling)
                {
                    StartCoroutine(ChannelVoid());
                }
            }
        }
        
        // Enable the whisper ability
        public void EnableWhisper()
        {
            _isEnabled = true;
            
            // Add UI notification to let the player know they can use this ability
            GameManager.Instance?.ShowNotification("Cosmic Madness: Void Whisper activated. Press V to channel the void.");
        }
        
        // Disable the whisper ability
        public void DisableWhisper()
        {
            _isEnabled = false;
            
            // Stop channeling if in progress
            if (_isChanneling)
            {
                StopAllCoroutines();
                CleanupChannelEffects();
            }
        }
        
        // Channel the void
        private IEnumerator ChannelVoid()
        {
            if (_isChanneling || _player == null)
                yield break;
                
            _isChanneling = true;
            
            // Apply defensive bonus if applicable
            if (_defenseBonus > 0)
            {
                _defenseMod = new DamageModifier
                {
                    ModifierType = ModifierType.Incoming,
                    Value = 1f - _defenseBonus,
                    IsMultiplier = true
                };
                
                _player.AddDamageModifier(_defenseMod);
            }
            
            // Create channel VFX
            if (_channelVFXPrefab != null)
            {
                _channelVFX = Instantiate(_channelVFXPrefab, transform.position, Quaternion.identity);
                _channelVFX.transform.SetParent(transform);
                _channelVFX.transform.localPosition = Vector3.up * 1f; // Slightly above the player
            }
            
            // Play channel sound
            if (_channelSFX != null && _audioSource != null)
            {
                _audioSource.clip = _channelSFX;
                _audioSource.loop = true;
                _audioSource.Play();
            }
            
            // Wait for channel time
            float startTime = Time.time;
            while (Time.time < startTime + _channelTime)
            {
                // Check if player is still alive and ability is still active
                if (_player == null || _player.IsDead || !_isEnabled)
                {
                    CleanupChannelEffects();
                    _isChanneling = false;
                    yield break;
                }
                
                // Update channel effect scale or intensity
                if (_channelVFX != null)
                {
                    float progress = (Time.time - startTime) / _channelTime;
                    float scale = 1f + progress;
                    _channelVFX.transform.localScale = new Vector3(scale, scale, scale);
                }
                
                yield return null;
            }
            
            // Cleanup channel effects
            CleanupChannelEffects();
            
            // Release the void energy
            ReleaseVoidEnergy();
            
            _isChanneling = false;
        }
        
        // Clean up channel effects
        private void CleanupChannelEffects()
        {
            // Destroy VFX
            if (_channelVFX != null)
            {
                Destroy(_channelVFX);
                _channelVFX = null;
            }
            
            // Stop sound
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
                _audioSource.loop = false;
            }
            
            // Remove defense modifier
            if (_defenseMod != null && _player != null)
            {
                _player.RemoveDamageModifier(_defenseMod);
                _defenseMod = null;
            }
        }
        
        // Release void energy as either knowledge or attack
        private void ReleaseVoidEnergy()
        {
            if (_player == null) return;
            
            // Determine effect type
            float roll = Random.value;
            bool isKnowledge = roll <= _knowledgeChance;
            
            if (isKnowledge)
            {
                // Release knowledge
                ReleaseKnowledge();
            }
            else
            {
                // Release attack
                ReleaseAttack();
            }
        }
        
        // Release knowledge effect
        private void ReleaseKnowledge()
        {
            // Play VFX
            if (_knowledgeVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_knowledgeVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 3f);
            }
            
            // Play sound
            if (_knowledgeSFX != null && _audioSource != null)
            {
                _audioSource.clip = _knowledgeSFX;
                _audioSource.loop = false;
                _audioSource.Play();
            }
            
            // Apply knowledge effects
            if (_revealMapRoom)
            {
                RevealCurrentRoom();
            }
            
            if (_revealEnemiesTemporarily)
            {
                RevealEnemiesTemporarily();
            }
            
            if (_revealItemsTemporarily)
            {
                RevealItemsTemporarily();
            }
            
            // Show notification
            GameManager.Instance?.ShowNotification("The void reveals its secrets...");
        }
        
        // Reveal the current room on the map
        private void RevealCurrentRoom()
        {
            // Get current room
            Room currentRoom = ProcGenManager.Instance?.GetRoomAtPosition(transform.position);
            if (currentRoom == null) return;
            
            // Reveal room
            currentRoom.Reveal();
            
            // Update minimap
            MiniMapController miniMap = FindObjectOfType<MiniMapController>();
            if (miniMap != null)
            {
                miniMap.UpdateRoomVisibility(currentRoom);
            }
        }
        
        // Temporarily reveal nearby enemies
        private void RevealEnemiesTemporarily()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _whisperRadius);
            
            foreach (var collider in colliders)
            {
                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    // Add a temporary reveal effect
                    StartCoroutine(TemporarilyRevealEnemy(enemy));
                }
            }
        }
        
        // Temporarily reveal enemy
        private IEnumerator TemporarilyRevealEnemy(EnemyController enemy)
        {
            if (enemy == null) yield break;
            
            // Add glow effect
            Renderer renderer = enemy.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Material originalMaterial = renderer.material;
                
                // Create glow material
                Material glowMaterial = new Material(Shader.Find("Standard"));
                glowMaterial.SetColor("_EmissionColor", Color.cyan * 0.5f);
                glowMaterial.EnableKeyword("_EMISSION");
                
                // Apply glow material
                renderer.material = glowMaterial;
                
                // Wait for duration
                yield return new WaitForSeconds(_tempRevealDuration);
                
                // Restore original material if enemy still exists
                if (enemy != null && renderer != null)
                {
                    renderer.material = originalMaterial;
                }
            }
        }
        
        // Temporarily reveal nearby items
        private void RevealItemsTemporarily()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _whisperRadius);
            
            foreach (var collider in colliders)
            {
                PickupItem item = collider.GetComponent<PickupItem>();
                if (item != null)
                {
                    // Add a temporary reveal effect
                    StartCoroutine(TemporarilyRevealItem(item));
                }
            }
        }
        
        // Temporarily reveal item
        private IEnumerator TemporarilyRevealItem(PickupItem item)
        {
            if (item == null) yield break;
            
            // Add glow effect
            Renderer renderer = item.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Material originalMaterial = renderer.material;
                
                // Create glow material
                Material glowMaterial = new Material(Shader.Find("Standard"));
                glowMaterial.SetColor("_EmissionColor", Color.yellow * 0.5f);
                glowMaterial.EnableKeyword("_EMISSION");
                
                // Apply glow material
                renderer.material = glowMaterial;
                
                // Wait for duration
                yield return new WaitForSeconds(_tempRevealDuration);
                
                // Restore original material if item still exists
                if (item != null && renderer != null)
                {
                    renderer.material = originalMaterial;
                }
            }
        }
        
        // Release attack effect
        private void ReleaseAttack()
        {
            // Play VFX
            if (_attackVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_attackVFXPrefab, transform.position, Quaternion.identity);
                vfx.transform.localScale = new Vector3(_attackRadius * 2, _attackRadius * 2, _attackRadius * 2);
                Destroy(vfx, 3f);
            }
            
            // Play sound
            if (_attackSFX != null && _audioSource != null)
            {
                _audioSource.clip = _attackSFX;
                _audioSource.loop = false;
                _audioSource.Play();
            }
            
            // Apply attack effects
            Collider[] colliders = Physics.OverlapSphere(transform.position, _attackRadius);
            int hitCount = 0;
            
            foreach (var collider in colliders)
            {
                // Check if it's a damageable entity
                DamageableEntity entity = collider.GetComponent<DamageableEntity>();
                if (entity != null && entity != _player)
                {
                    // Apply damage
                    entity.TakeDamage(_attackDamage, _damageType, _player.gameObject);
                    hitCount++;
                    
                    // Apply additional effect based on entity type
                    EnemyController enemy = entity as EnemyController;
                    if (enemy != null)
                    {
                        // Apply a void stun effect
                        enemy.ApplyStatusEffect(StatusEffectType.Stun, 2f);
                    }
                }
            }
            
            // Drain sanity based on use
            if (_attackSanityDrain > 0 && _player != null)
            {
                int playerId = _player.NetworkId;
                SanitySystem.Instance?.ModifyPlayerSanity(playerId, -_attackSanityDrain);
            }
            
            // Show notification
            GameManager.Instance?.ShowNotification($"The void strikes! {hitCount} enemies hit.");
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw radius gizmos for debugging
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _whisperRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRadius);
        }
    }
}