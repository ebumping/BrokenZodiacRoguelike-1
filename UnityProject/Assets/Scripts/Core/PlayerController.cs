using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    public class PlayerController : MonoBehaviour
    {
        // Movement properties
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float acceleration = 50.0f;
        [SerializeField] private float friction = 30.0f;
        [SerializeField] private float dodgeSpeed = 15.0f;
        [SerializeField] private float dodgeDuration = 0.3f;
        [SerializeField] private float dodgeCooldown = 1.0f;

        // Combat properties
        [Header("Combat")]
        [SerializeField] private float maxHealth = 100.0f;
        [SerializeField] private float healthRegen = 0.5f;
        [SerializeField] private float maxMana = 100.0f;
        [SerializeField] private float manaRegen = 5.0f;
        [SerializeField] private float shootCooldown = 0.2f;
        [SerializeField] private float spellCooldown = 1.0f;
        [SerializeField] private Transform projectileSpawnPoint;
        
        // Networked properties
        [Header("Networking")]
        [SerializeField] private int networkId = 0;
        [SerializeField] private bool isLocalPlayer = true;

        // Component references
        private Rigidbody2D _rigidbody;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private AudioSource _audioSource;

        // UI references
        [Header("UI References")]
        [SerializeField] private Canvas playerCanvas;
        [SerializeField] private UnityEngine.UI.Slider healthBar;
        [SerializeField] private UnityEngine.UI.Slider manaBar;
        [SerializeField] private TMPro.TextMeshProUGUI nameLabel;
        
        // State tracking
        private Vector2 _inputDirection = Vector2.zero;
        private Vector2 _lastMoveDirection = Vector2.down;
        private Vector2 _aimDirection = Vector2.zero;
        private bool _isDodging = false;
        private bool _canDodge = true;
        private bool _canShoot = true;
        private bool _canCastSpell = true;
        private float _currentHealth;
        private float _currentMana;
        
        // Player class and equipment
        private PlayerClass _playerClass;
        private Weapon _equippedWeapon;
        private ZodiacSignil _zodiacSignil;
        private List<TarotCard> _tarotCards = new List<TarotCard>();
        private List<string> _spells = new List<string>();
        private int _currentSpellIndex = 0;

        // Timers
        private float _dodgeTimer;
        private float _dodgeCooldownTimer;
        private float _shootCooldownTimer;
        private float _spellCooldownTimer;

        // Special effect trackers
        private List<TarotCard> _onKillEffects = new List<TarotCard>();
        private List<TarotCard> _onHitEffects = new List<TarotCard>();
        private List<TarotCard> _onRoomClearEffects = new List<TarotCard>();

        // Events
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action OnPlayerDied;
        public event Action<Weapon> OnWeaponChanged;
        public event Action<string> OnSpellChanged;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            // Initialize state
            _currentHealth = maxHealth;
            _currentMana = maxMana;
            
            // Update UI
            UpdateHealthBar();
            UpdateManaBar();
            
            // Initialize local player
            if (isLocalPlayer)
            {
                // Setup camera
                Camera.main.GetComponent<CameraFollow>()?.SetTarget(transform);
                
                Debug.Log("Local player initialized");
            }
            else
            {
                // Hide HUD for non-local players
                if (playerCanvas != null)
                {
                    playerCanvas.enabled = false;
                }
                Debug.Log("Remote player initialized");
            }
            
            // Load default class and weapon
            SetPlayerClass("OccultDetective");
            EquipWeapon("MarksmanRifle");
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                HandleInput();
                HandleAiming();
                UpdateTimers();
            }
            
            // Handle health & mana regeneration
            RegenerateStats();
        }

        private void FixedUpdate()
        {
            if (_isDodging) return; // Skip movement during dodge
            
            // Apply movement
            Vector2 targetVelocity = _inputDirection * moveSpeed;
            
            if (_inputDirection != Vector2.zero)
            {
                // Accelerate when moving
                _rigidbody.velocity = Vector2.MoveTowards(
                    _rigidbody.velocity, 
                    targetVelocity, 
                    acceleration * Time.fixedDeltaTime);
                
                _lastMoveDirection = _inputDirection.normalized;
                
                // Play walking animation
                if (_animator != null)
                {
                    _animator.SetBool("IsMoving", true);
                }
            }
            else
            {
                // Apply friction when no input
                _rigidbody.velocity = Vector2.MoveTowards(
                    _rigidbody.velocity, 
                    Vector2.zero, 
                    friction * Time.fixedDeltaTime);
                
                // Play idle animation
                if (_animator != null)
                {
                    _animator.SetBool("IsMoving", false);
                }
            }
            
            // Face the direction we're aiming
            if (_aimDirection != Vector2.zero && _spriteRenderer != null)
            {
                bool facingRight = _aimDirection.x > 0;
                _spriteRenderer.flipX = facingRight;
            }
        }

        private void HandleInput()
        {
            // Get movement input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            _inputDirection = new Vector2(horizontal, vertical).normalized;
            
            // Convert to isometric direction if needed
            _inputDirection = ConvertToIsometric(_inputDirection);
            
            // Handle shooting
            if (Input.GetMouseButton(0) && _canShoot)
            {
                Shoot();
            }
            
            // Handle spell casting
            if (Input.GetMouseButton(1) && _canCastSpell)
            {
                CastSpell();
            }
            
            // Handle spell cycling
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                CycleSpell(1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                CycleSpell(-1);
            }
            
            // Handle dodge
            if (Input.GetKeyDown(KeyCode.Space) && _canDodge)
            {
                StartDodge();
            }
        }

        private void HandleAiming()
        {
            // Aim using mouse position
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _aimDirection = (mousePos - (Vector2)transform.position).normalized;
            
            // Rotate aim pivot to face aim direction
            if (projectileSpawnPoint != null)
            {
                float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
                projectileSpawnPoint.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private Vector2 ConvertToIsometric(Vector2 input)
        {
            // Convert screen space input to isometric space
            // This function can be adjusted based on the exact isometric angle
            float x = input.x - input.y;
            float y = (input.x + input.y) / 2;
            return new Vector2(x, y).normalized * input.magnitude;
        }

        private void UpdateTimers()
        {
            // Handle dodge timer
            if (_isDodging)
            {
                _dodgeTimer -= Time.deltaTime;
                if (_dodgeTimer <= 0)
                {
                    EndDodge();
                }
            }
            
            // Handle dodge cooldown
            if (!_canDodge && !_isDodging)
            {
                _dodgeCooldownTimer -= Time.deltaTime;
                if (_dodgeCooldownTimer <= 0)
                {
                    _canDodge = true;
                }
            }
            
            // Handle shoot cooldown
            if (!_canShoot)
            {
                _shootCooldownTimer -= Time.deltaTime;
                if (_shootCooldownTimer <= 0)
                {
                    _canShoot = true;
                }
            }
            
            // Handle spell cooldown
            if (!_canCastSpell)
            {
                _spellCooldownTimer -= Time.deltaTime;
                if (_spellCooldownTimer <= 0)
                {
                    _canCastSpell = true;
                }
            }
        }

        private void RegenerateStats()
        {
            // Health regeneration
            if (_currentHealth < maxHealth)
            {
                _currentHealth = Mathf.Min(maxHealth, _currentHealth + healthRegen * Time.deltaTime);
                UpdateHealthBar();
            }
            
            // Mana regeneration
            if (_currentMana < maxMana)
            {
                _currentMana = Mathf.Min(maxMana, _currentMana + manaRegen * Time.deltaTime);
                UpdateManaBar();
            }
        }

        private void StartDodge()
        {
            _isDodging = true;
            _canDodge = false;
            
            // Use last input direction or aim direction for dodge
            Vector2 dodgeDirection = _inputDirection.magnitude > 0.1f ? _inputDirection : _aimDirection;
            if (dodgeDirection.magnitude < 0.1f)
            {
                dodgeDirection = _lastMoveDirection;
            }
            
            // Apply dodge velocity
            _rigidbody.velocity = dodgeDirection.normalized * dodgeSpeed;
            
            // Start timers
            _dodgeTimer = dodgeDuration;
            _dodgeCooldownTimer = dodgeCooldown;
            
            // Play dodge animation
            if (_animator != null)
            {
                _animator.SetTrigger("Dodge");
            }
            
            // Become temporarily invulnerable during dodge
            // This would typically use layers to ignore collisions
            gameObject.layer = LayerMask.NameToLayer("PlayerInvulnerable");
            
            // Play sound
            PlaySound("dodge");
            
            // Networked players would sync dodge here
        }

        private void EndDodge()
        {
            _isDodging = false;
            
            // Stop velocity
            _rigidbody.velocity = Vector2.zero;
            
            // Reset layer (remove invulnerability)
            gameObject.layer = LayerMask.NameToLayer("Player");
            
            // Play idle/walk animation
            if (_animator != null)
            {
                _animator.SetBool("IsMoving", _inputDirection.magnitude > 0.1f);
            }
        }

        private void Shoot()
        {
            if (_equippedWeapon == null) return;
            
            _canShoot = false;
            _shootCooldownTimer = _equippedWeapon.FireRate;
            
            // Play shoot animation
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }
            
            // Get projectile spawn position
            Vector3 spawnPosition = projectileSpawnPoint != null ? 
                projectileSpawnPoint.position : transform.position;
            
            // Spawn projectile
            SpawnProjectile(_equippedWeapon.ProjectilePrefab, spawnPosition, _aimDirection);
            
            // Play sound
            PlaySound("shoot");
            
            // Networked players would sync shoot here
        }

        private void SpawnProjectile(GameObject projectilePrefab, Vector3 position, Vector2 direction, bool isVisualOnly = false)
        {
            if (projectilePrefab == null) return;
            
            // Instantiate projectile
            GameObject projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
            
            // Set projectile direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // Set projectile properties
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent != null)
            {
                projectileComponent.Initialize(
                    direction, 
                    _equippedWeapon.Damage, 
                    _equippedWeapon.ProjectileSpeed,
                    networkId,
                    isVisualOnly);
            }
        }

        private void CastSpell()
        {
            if (_spells.Count == 0 || _currentSpellIndex >= _spells.Count) return;
            
            string currentSpell = _spells[_currentSpellIndex];
            float manaCost = 20.0f; // Default mana cost
            
            // Check for sufficient mana
            if (_currentMana < manaCost)
            {
                Debug.Log("Not enough mana to cast spell");
                return;
            }
            
            _canCastSpell = false;
            _spellCooldownTimer = spellCooldown;
            
            // Consume mana
            _currentMana -= manaCost;
            UpdateManaBar();
            
            // Play cast animation
            if (_animator != null)
            {
                _animator.SetTrigger("Cast");
            }
            
            // Cast the spell based on type
            CastSpellByName(currentSpell);
            
            // Play sound
            PlaySound("cast");
            
            // Networked players would sync spell cast here
        }

        private void CastSpellByName(string spellName)
        {
            // TODO: Implement spell effects based on name
            Debug.Log($"Casting spell: {spellName}");
            
            // As an example
            switch (spellName)
            {
                case "Fireball":
                    // Spawn a fireball projectile
                    break;
                case "Teleport":
                    // Teleport player in aim direction
                    break;
                case "Shield":
                    // Create a protective shield
                    break;
                default:
                    Debug.LogWarning($"Unknown spell: {spellName}");
                    break;
            }
        }

        private void CycleSpell(int direction)
        {
            if (_spells.Count == 0) return;
            
            _currentSpellIndex = (_currentSpellIndex + direction + _spells.Count) % _spells.Count;
            
            // Notify about spell change
            OnSpellChanged?.Invoke(_spells[_currentSpellIndex]);
            
            Debug.Log($"Switched to spell: {_spells[_currentSpellIndex]}");
        }

        private void PlaySound(string soundName)
        {
            if (_audioSource == null) return;
            
            // TODO: Load and play the appropriate sound clip
            // _audioSource.PlayOneShot(Resources.Load<AudioClip>($"Sounds/{soundName}"));
        }

        private void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                healthBar.value = _currentHealth / maxHealth;
            }
            
            // Notify listeners
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        private void UpdateManaBar()
        {
            if (manaBar != null)
            {
                manaBar.value = _currentMana / maxMana;
            }
            
            // Notify listeners
            OnManaChanged?.Invoke(_currentMana, maxMana);
        }

        public void SetPlayerClass(string className)
        {
            // TODO: Load player class data from resource system
            Debug.Log($"Setting player class to {className}");
        }

        public void EquipWeapon(string weaponName)
        {
            // TODO: Load weapon from resource system
            Debug.Log($"Equipping weapon: {weaponName}");
        }

        public void TakeDamage(float amount)
        {
            if (_isDodging) return; // Invulnerable during dodge
            
            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            UpdateHealthBar();
            
            // Play hit animation
            if (_animator != null)
            {
                _animator.SetTrigger("Hit");
            }
            
            // Play hit sound
            PlaySound("hit");
            
            // Check for death
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void RestoreHealth(float amount)
        {
            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
            UpdateHealthBar();
            
            // Play heal effect
            // TODO: Visual effect for healing
        }

        public void RestoreMana(float amount)
        {
            _currentMana = Mathf.Min(maxMana, _currentMana + amount);
            UpdateManaBar();
            
            // Play mana restore effect
            // TODO: Visual effect for mana restore
        }

        private void Die()
        {
            // Play death animation
            if (_animator != null)
            {
                _animator.SetTrigger("Death");
            }
            
            // Play death sound
            PlaySound("death");
            
            // Disable controls
            enabled = false;
            
            // Notify listeners
            OnPlayerDied?.Invoke();
            
            // Notify game manager
            GameManager.Instance?.PlayerDied(networkId);
        }

        public void OnRoomCleared(int roomId)
        {
            // Apply any on-room-clear effects
            foreach (var card in _onRoomClearEffects)
            {
                // Apply special effect
                Debug.Log($"Applying room clear effect from {card.CardName}");
            }
        }

        public void PickupWeapon(int weaponId)
        {
            // TODO: Load weapon by ID and equip it
            Debug.Log($"Picked up weapon ID: {weaponId}");
        }

        public void PickupTarotCard(int cardId)
        {
            // TODO: Load tarot card by ID and apply its effects
            Debug.Log($"Picked up tarot card ID: {cardId}");
        }

        public void PickupMutagen(int mutagenId)
        {
            // TODO: Apply mutagen effect
            Debug.Log($"Picked up mutagen ID: {mutagenId}");
        }

        #region Effect Registration Methods
        public void RegisterOnKillEffect(TarotCard card)
        {
            if (!_onKillEffects.Contains(card))
            {
                _onKillEffects.Add(card);
            }
        }

        public void UnregisterOnKillEffect(TarotCard card)
        {
            _onKillEffects.Remove(card);
        }

        public void RegisterOnHitEffect(TarotCard card)
        {
            if (!_onHitEffects.Contains(card))
            {
                _onHitEffects.Add(card);
            }
        }

        public void UnregisterOnHitEffect(TarotCard card)
        {
            _onHitEffects.Remove(card);
        }

        public void RegisterOnRoomClearEffect(TarotCard card)
        {
            if (!_onRoomClearEffects.Contains(card))
            {
                _onRoomClearEffects.Add(card);
            }
        }

        public void UnregisterOnRoomClearEffect(TarotCard card)
        {
            _onRoomClearEffects.Remove(card);
        }
        #endregion

        #region Stat Modification Methods
        public void ModifyMaxHealth(float amount)
        {
            float originalMaxHealth = maxHealth;
            maxHealth += amount;
            
            // Adjust current health proportionally
            if (originalMaxHealth > 0)
            {
                _currentHealth = (_currentHealth / originalMaxHealth) * maxHealth;
            }
            
            UpdateHealthBar();
        }

        public void ModifyDamage(float amount)
        {
            // This would typically apply to weapon damage or base damage
            if (_equippedWeapon != null)
            {
                _equippedWeapon.ModifyDamage(amount);
            }
        }

        public void ModifySpeed(float amount)
        {
            moveSpeed += amount;
        }

        public void ModifyCooldown(float amount)
        {
            // Negative values make cooldowns shorter
            spellCooldown = Mathf.Max(0.1f, spellCooldown - amount);
            
            if (_equippedWeapon != null)
            {
                _equippedWeapon.ModifyFireRate(-amount); // Negative because lower fire rate = faster firing
            }
        }

        public void ModifyCriticalChance(float amount)
        {
            // This would typically apply to weapon critical hit chance
            if (_equippedWeapon != null)
            {
                _equippedWeapon.ModifyCriticalChance(amount);
            }
        }

        public void AddAbility(string abilityName)
        {
            if (!_spells.Contains(abilityName))
            {
                _spells.Add(abilityName);
                Debug.Log($"Added ability: {abilityName}");
                
                // If this is our first spell, select it
                if (_spells.Count == 1)
                {
                    _currentSpellIndex = 0;
                    OnSpellChanged?.Invoke(_spells[0]);
                }
            }
        }

        public void RemoveAbility(string abilityName)
        {
            if (_spells.Contains(abilityName))
            {
                int index = _spells.IndexOf(abilityName);
                _spells.Remove(abilityName);
                Debug.Log($"Removed ability: {abilityName}");
                
                // Adjust current spell index if needed
                if (_spells.Count > 0)
                {
                    if (_currentSpellIndex >= _spells.Count)
                    {
                        _currentSpellIndex = 0;
                    }
                    OnSpellChanged?.Invoke(_spells[_currentSpellIndex]);
                }
            }
        }
        #endregion
    }

    // A basic projectile class to work with PlayerController
    public class Projectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _damage;
        private float _speed;
        private int _ownerId;
        private bool _isVisualOnly;
        private float _lifetime = 5.0f;
        
        public void Initialize(Vector2 direction, float damage, float speed, int ownerId, bool isVisualOnly = false)
        {
            _direction = direction.normalized;
            _damage = damage;
            _speed = speed;
            _ownerId = ownerId;
            _isVisualOnly = isVisualOnly;
            
            Destroy(gameObject, _lifetime);
        }
        
        private void Update()
        {
            // Move the projectile
            transform.position += (Vector3)(_direction * _speed * Time.deltaTime);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isVisualOnly) return; // Visual-only projectiles don't do damage
            
            // Check if we hit an enemy
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
                DestroyProjectile();
                return;
            }
            
            // Check if we hit a player (that isn't the owner)
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.gameObject.GetInstanceID() != _ownerId)
            {
                player.TakeDamage(_damage);
                DestroyProjectile();
                return;
            }
            
            // Check for environment collision
            if (other.CompareTag("Environment"))
            {
                DestroyProjectile();
            }
        }
        
        private void DestroyProjectile()
        {
            // Play hit effect
            // TODO: Instantiate particle effect
            
            // Destroy the projectile
            Destroy(gameObject);
        }
    }
}