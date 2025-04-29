using Godot;
using System;
using System.Collections.Generic;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    public partial class PlayerController : CharacterBody2D
    {
        // Multiplayer properties
        [ExportGroup("Networking")]
        [Export] private NodePath _syncTransformPath;
        
        // Movement properties
        [ExportGroup("Movement")]
        [Export] private float _moveSpeed = 200.0f;
        [Export] private float _acceleration = 2000.0f;
        [Export] private float _friction = 1000.0f;
        [Export] private float _dodgeSpeed = 500.0f;
        [Export] private float _dodgeDuration = 0.3f;
        [Export] private float _dodgeCooldown = 1.0f;

        // Combat properties
        [ExportGroup("Combat")]
        [Export] private float _maxHealth = 100.0f;
        [Export] private float _healthRegen = 0.5f;
        [Export] private float _manaMax = 100.0f;
        [Export] private float _manaRegen = 5.0f;
        [Export] private float _shootCooldown = 0.2f;
        [Export] private float _spellCooldown = 1.0f;
        [Export] private NodePath _projectileSpawnPoint;
        
        // Node references
        private AnimatedSprite2D _sprite;
        private Timer _dodgeTimer;
        private Timer _dodgeCooldownTimer;
        private Timer _shootCooldownTimer;
        private Timer _spellCooldownTimer;
        private Node2D _aimPivot;
        private AudioStreamPlayer2D _audioPlayer;
        private ProgressBar _healthBar;
        private ProgressBar _manaBar;
        private MultiplayerSynchronizer _syncTransform;
        
        // State tracking
        private Vector2 _inputDirection = Vector2.Zero;
        private Vector2 _lastMoveDirection = Vector2.Down;
        private Vector2 _aimDirection = Vector2.Zero;
        private bool _isDodging = false;
        private bool _canDodge = true;
        private bool _canShoot = true;
        private bool _canCastSpell = true;
        private float _currentHealth;
        private float _currentMana;
        private bool _isLocalPlayer = false;
        
        // Player class and equipment
        private PlayerClass _playerClass;
        private Weapon _equippedWeapon;
        private ZodiacSignil _zodiacSignil;
        private List<TarotCard> _tarotCards = new List<TarotCard>();
        private List<Resource> _spells = new List<Resource>();
        private int _currentSpellIndex = 0;

        // Signals
        [Signal]
        public delegate void HealthChangedEventHandler(float current, float max);
        
        [Signal]
        public delegate void ManaChangedEventHandler(float current, float max);
        
        [Signal]
        public delegate void PlayerDiedEventHandler();
        
        [Signal]
        public delegate void WeaponChangedEventHandler(Weapon weapon);
        
        [Signal]
        public delegate void SpellChangedEventHandler(Resource spell);
        
        public override void _Ready()
        {
            // Get node references
            _sprite = GetNode<AnimatedSprite2D>("Sprite");
            _dodgeTimer = GetNode<Timer>("DodgeTimer");
            _dodgeCooldownTimer = GetNode<Timer>("DodgeCooldownTimer");
            _shootCooldownTimer = GetNode<Timer>("ShootCooldownTimer");
            _spellCooldownTimer = GetNode<Timer>("SpellCooldownTimer");
            _aimPivot = GetNode<Node2D>("AimPivot");
            _audioPlayer = GetNode<AudioStreamPlayer2D>("AudioPlayer");
            _healthBar = GetNode<ProgressBar>("HUD/HealthBar");
            _manaBar = GetNode<ProgressBar>("HUD/ManaBar");
            
            if (_syncTransformPath != null)
            {
                _syncTransform = GetNode<MultiplayerSynchronizer>(_syncTransformPath);
            }
            
            // Initialize state
            _currentHealth = _maxHealth;
            _currentMana = _manaMax;
            
            // Setup timers
            _dodgeTimer.WaitTime = _dodgeDuration;
            _dodgeTimer.OneShot = true;
            _dodgeCooldownTimer.WaitTime = _dodgeCooldown;
            _dodgeCooldownTimer.OneShot = true;
            _shootCooldownTimer.WaitTime = _shootCooldown;
            _shootCooldownTimer.OneShot = true;
            _spellCooldownTimer.WaitTime = _spellCooldown;
            _spellCooldownTimer.OneShot = true;
            
            // Connect signals
            _dodgeTimer.Timeout += OnDodgeFinished;
            _dodgeCooldownTimer.Timeout += OnDodgeCooldownFinished;
            _shootCooldownTimer.Timeout += OnShootCooldownFinished;
            _spellCooldownTimer.Timeout += OnSpellCooldownFinished;
            
            // Check if we're the local player
            int localId = NetworkManager.Instance.LocalPlayerId;
            _isLocalPlayer = (int)GetMeta("network_id", localId) == localId;
            
            if (_isLocalPlayer)
            {
                // Setup camera for local player
                Camera2D camera = new Camera2D();
                camera.Current = true;
                AddChild(camera);
                
                GD.Print("Local player initialized");
            }
            else
            {
                // Hide HUD for non-local players
                GetNode("HUD").Visible = false;
                GD.Print("Remote player initialized");
            }
            
            // Load default class and weapon
            SetPlayerClass("OccultDetective");
            EquipWeapon("MarksmanRifle");
            
            UpdateHealthBar();
            UpdateManaBar();
        }

        public override void _Process(double delta)
        {
            if (_isLocalPlayer)
            {
                HandleAiming();
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isLocalPlayer)
            {
                HandleInput();
            }
            
            Vector2 velocity = Velocity;
            
            if (_isDodging)
            {
                // During dodge, maintain dodge velocity
                MoveAndSlide();
                return;
            }
            
            if (_inputDirection != Vector2.Zero)
            {
                // Accelerate when moving
                velocity = velocity.MoveToward(_inputDirection * _moveSpeed, (float)(_acceleration * delta));
                _lastMoveDirection = _inputDirection.Normalized();
                
                // Play walking animation
                if (_sprite.Animation != "walk")
                {
                    _sprite.Play("walk");
                }
            }
            else
            {
                // Apply friction when no input
                velocity = velocity.MoveToward(Vector2.Zero, (float)(_friction * delta));
                
                // Play idle animation
                if (_sprite.Animation != "idle")
                {
                    _sprite.Play("idle");
                }
            }
            
            // Face the direction we're aiming
            if (_aimDirection != Vector2.Zero)
            {
                bool facingRight = _aimDirection.X > 0;
                _sprite.FlipH = facingRight;
            }
            
            // Apply the velocity
            Velocity = velocity;
            MoveAndSlide();
            
            // Handle health regeneration
            if (_currentHealth < _maxHealth)
            {
                _currentHealth = Mathf.Min(_maxHealth, _currentHealth + _healthRegen * (float)delta);
                UpdateHealthBar();
            }
            
            // Handle mana regeneration
            if (_currentMana < _manaMax)
            {
                _currentMana = Mathf.Min(_manaMax, _currentMana + _manaRegen * (float)delta);
                UpdateManaBar();
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (!_isLocalPlayer) return;
            
            // Handle shooting
            if (@event.IsActionPressed("shoot") && _canShoot)
            {
                Shoot();
            }
            
            // Handle spell casting
            if (@event.IsActionPressed("cast_spell") && _canCastSpell)
            {
                CastSpell();
            }
            
            // Handle spell cycling
            if (@event.IsActionPressed("cycle_spell_up"))
            {
                CycleSpell(1);
            }
            else if (@event.IsActionPressed("cycle_spell_down"))
            {
                CycleSpell(-1);
            }
            
            // Handle dodge
            if (@event.IsActionPressed("dodge") && _canDodge)
            {
                StartDodge();
            }
        }

        private void HandleInput()
        {
            // Get movement input
            _inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            
            // Convert to isometric direction
            _inputDirection = ConvertToIsometric(_inputDirection);
        }

        private void HandleAiming()
        {
            if (Input.GetMouseMode() == Input.MouseMode.Visible)
            {
                // Aim using mouse position on desktop
                Vector2 mousePos = GetGlobalMousePosition();
                _aimDirection = (mousePos - GlobalPosition).Normalized();
            }
            else
            {
                // Aim using right virtual joystick on mobile
                // This would be handled by the mobile input controller
                // For now, just aim in the movement direction if no specific aim input
                if (_inputDirection != Vector2.Zero)
                {
                    _aimDirection = _inputDirection.Normalized();
                }
            }
            
            // Rotate aim pivot to face aim direction
            if (_aimDirection != Vector2.Zero)
            {
                _aimPivot.Rotation = _aimDirection.Angle();
            }
        }

        private Vector2 ConvertToIsometric(Vector2 input)
        {
            // Convert screen space input to isometric space
            float x = input.X - input.Y;
            float y = (input.X + input.Y) / 2;
            return new Vector2(x, y).Normalized() * input.Length();
        }

        private void StartDodge()
        {
            _isDodging = true;
            _canDodge = false;
            
            // Use last input direction or aim direction for dodge
            Vector2 dodgeDirection = _inputDirection.Length() > 0.1f ? _inputDirection : _aimDirection;
            if (dodgeDirection.Length() < 0.1f)
            {
                dodgeDirection = _lastMoveDirection;
            }
            
            // Apply dodge velocity
            Velocity = dodgeDirection.Normalized() * _dodgeSpeed;
            
            // Play dodge animation and sound
            _sprite.Play("dodge");
            //PlaySound("dodge");
            
            // Start timers
            _dodgeTimer.Start();
            
            // Become temporarily invulnerable during dodge
            SetCollisionLayerBit(0, false);
            SetCollisionMaskBit(0, false);
            
            Rpc(MethodName.SyncDodge, dodgeDirection);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        private void SyncDodge(Vector2 direction)
        {
            if (_isLocalPlayer) return; // Don't apply to local player, they handle it directly
            
            _isDodging = true;
            Velocity = direction.Normalized() * _dodgeSpeed;
            _sprite.Play("dodge");
            
            // Start timers
            _dodgeTimer.Start();
            
            // Become temporarily invulnerable during dodge
            SetCollisionLayerBit(0, false);
            SetCollisionMaskBit(0, false);
        }

        private void OnDodgeFinished()
        {
            _isDodging = false;
            _sprite.Play("idle");
            
            // Restore collisions
            SetCollisionLayerBit(0, true);
            SetCollisionMaskBit(0, true);
            
            // Start cooldown timer
            _dodgeCooldownTimer.Start();
        }

        private void OnDodgeCooldownFinished()
        {
            _canDodge = true;
        }

        private void Shoot()
        {
            if (_equippedWeapon == null) return;
            
            _canShoot = false;
            _shootCooldownTimer.WaitTime = _equippedWeapon.FireRate;
            _shootCooldownTimer.Start();
            
            // Play shoot animation
            _sprite.Play("attack");
            
            // Get projectile spawn position
            Node2D spawnPoint = _projectileSpawnPoint != null ? 
                GetNode<Node2D>(_projectileSpawnPoint) : _aimPivot;
            
            // Spawn projectile
            SpawnProjectile(_equippedWeapon.ProjectileScene, spawnPoint.GlobalPosition, _aimDirection);
            
            // Play sound
            //PlaySound("shoot");
            
            // Synchronize with network
            Rpc(MethodName.SyncShoot, _aimDirection);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        private void SyncShoot(Vector2 aimDir)
        {
            if (_isLocalPlayer) return; // Don't apply to local player, they handle it directly
            
            _aimDirection = aimDir;
            _aimPivot.Rotation = aimDir.Angle();
            
            // Play shoot animation
            _sprite.Play("attack");
            
            // Get projectile spawn position
            Node2D spawnPoint = _projectileSpawnPoint != null ? 
                GetNode<Node2D>(_projectileSpawnPoint) : _aimPivot;
            
            // Spawn projectile (visually only for network players)
            SpawnProjectile(_equippedWeapon.ProjectileScene, spawnPoint.GlobalPosition, aimDir, true);
            
            // Play sound
            //PlaySound("shoot");
        }

        private void SpawnProjectile(string projectilePath, Vector2 position, Vector2 direction, bool isVisualOnly = false)
        {
            if (string.IsNullOrEmpty(projectilePath)) return;
            
            // Load projectile scene
            PackedScene projectileScene = ResourceLoader.Load<PackedScene>(projectilePath);
            if (projectileScene == null)
            {
                GD.PrintErr($"Failed to load projectile scene: {projectilePath}");
                return;
            }
            
            // Instantiate projectile
            Node2D projectile = projectileScene.Instantiate<Node2D>();
            GetTree().CurrentScene.AddChild(projectile);
            
            // Set projectile properties
            projectile.GlobalPosition = position;
            projectile.Rotation = direction.Angle();
            
            // Set projectile metadata
            projectile.SetMeta("damage", _equippedWeapon.Damage);
            projectile.SetMeta("speed", _equippedWeapon.ProjectileSpeed);
            projectile.SetMeta("direction", direction);
            projectile.SetMeta("owner_id", GetMeta("network_id", 0));
            projectile.SetMeta("visual_only", isVisualOnly);
            
            // If the projectile has an initialize method, call it
            if (projectile.HasMethod("Initialize"))
            {
                projectile.Call("Initialize", direction, _equippedWeapon.Damage, isVisualOnly);
            }
        }

        private void OnShootCooldownFinished()
        {
            _canShoot = true;
        }

        private void CastSpell()
        {
            if (_spells.Count == 0 || _currentSpellIndex >= _spells.Count) return;
            
            Resource currentSpell = _spells[_currentSpellIndex];
            float manaCost = 20.0f; // Default mana cost
            
            // Check for sufficient mana
            if (_currentMana < manaCost)
            {
                // Play "not enough mana" feedback
                //PlaySound("no_mana");
                return;
            }
            
            _canCastSpell = false;
            _spellCooldownTimer.Start();
            
            // Consume mana
            _currentMana -= manaCost;
            UpdateManaBar();
            
            // Play spell cast animation
            _sprite.Play("cast");
            
            // Implement spell casting effect based on class and spell
            // This would be implemented based on the spell type
            
            // Play sound
            //PlaySound("cast_spell");
            
            // Synchronize with network
            Rpc(MethodName.SyncCastSpell, _currentSpellIndex, _aimDirection);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        private void SyncCastSpell(int spellIndex, Vector2 aimDir)
        {
            if (_isLocalPlayer) return; // Don't apply to local player, they handle it directly
            
            _currentSpellIndex = spellIndex;
            _aimDirection = aimDir;
            _aimPivot.Rotation = aimDir.Angle();
            
            // Play spell cast animation
            _sprite.Play("cast");
            
            // Play sound
            //PlaySound("cast_spell");
        }

        private void OnSpellCooldownFinished()
        {
            _canCastSpell = true;
        }

        private void CycleSpell(int direction)
        {
            if (_spells.Count == 0) return;
            
            _currentSpellIndex = (_currentSpellIndex + direction + _spells.Count) % _spells.Count;
            
            EmitSignal(SignalName.SpellChanged, _spells[_currentSpellIndex]);
        }

        public void SetPlayerClass(string className)
        {
            _playerClass = ResourceManager.Instance.GetPlayerClass(className);
            
            if (_playerClass != null)
            {
                // Apply class stats
                _maxHealth = _playerClass.BaseHealth;
                _currentHealth = _maxHealth;
                _manaMax = _playerClass.BaseMana;
                _currentMana = _manaMax;
                _moveSpeed = _playerClass.MoveSpeed;
                
                // Load class-specific spells
                _spells.Clear();
                foreach (string spellPath in _playerClass.Spells)
                {
                    Resource spell = ResourceManager.Instance.GetGenericResource(spellPath);
                    if (spell != null)
                    {
                        _spells.Add(spell);
                    }
                }
                
                _currentSpellIndex = 0;
                if (_spells.Count > 0)
                {
                    EmitSignal(SignalName.SpellChanged, _spells[_currentSpellIndex]);
                }
                
                // Update visuals
                UpdateHealthBar();
                UpdateManaBar();
            }
        }

        public void EquipWeapon(string weaponName)
        {
            _equippedWeapon = ResourceManager.Instance.GetWeapon(weaponName);
            
            if (_equippedWeapon != null)
            {
                _shootCooldownTimer.WaitTime = _equippedWeapon.FireRate;
                EmitSignal(SignalName.WeaponChanged, _equippedWeapon);
            }
        }

        public void SetZodiacSignil(string signilName)
        {
            _zodiacSignil = ResourceManager.Instance.GetZodiacSignil(signilName);
            
            if (_zodiacSignil != null)
            {
                // Apply zodiac sigil effects
                ApplyZodiacEffects();
            }
        }

        private void ApplyZodiacEffects()
        {
            if (_zodiacSignil == null) return;
            
            // Apply effects based on zodiac sign
            switch (_zodiacSignil.SignName)
            {
                case "Aries":
                    _moveSpeed *= 1.15f; // +15% sprint speed
                    break;
                case "Taurus":
                    _maxHealth *= 1.2f; // +20% max HP
                    _currentHealth = _maxHealth;
                    UpdateHealthBar();
                    break;
                // Add other zodiac effects as needed
            }
        }

        public void AddTarotCard(string cardName)
        {
            TarotCard card = ResourceManager.Instance.GetTarotCard(cardName);
            
            if (card != null)
            {
                _tarotCards.Add(card);
                
                // Apply card effect
                ApplyTarotCardEffect(card);
            }
        }

        private void ApplyTarotCardEffect(TarotCard card)
        {
            // Apply effects based on the card
            // This would be expanded based on the actual card effects
            GD.Print($"Applied tarot card effect: {card.CardName}");
        }

        public void TakeDamage(float amount)
        {
            // Check if we should handle damage (only server or singleplayer should process)
            if (NetworkManager.Instance.IsServer || NetworkManager.Instance.CurrentMode == NetworkManager.NetworkMode.Offline)
            {
                _currentHealth -= amount;
                
                // Play hit animation and sound
                _sprite.Play("hit");
                //PlaySound("hit");
                
                UpdateHealthBar();
                
                // Check for death
                if (_currentHealth <= 0)
                {
                    Die();
                }
                
                // Sync damage to all clients
                Rpc(MethodName.SyncDamage, amount, _currentHealth);
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        private void SyncDamage(float amount, float newHealth)
        {
            if (NetworkManager.Instance.IsServer) return; // Server already processed this
            
            _currentHealth = newHealth;
            
            // Play hit animation and sound
            _sprite.Play("hit");
            //PlaySound("hit");
            
            UpdateHealthBar();
            
            // Check for death
            if (_currentHealth <= 0 && !_isDead)
            {
                Die();
            }
        }

        private bool _isDead = false;
        
        private void Die()
        {
            if (_isDead) return;
            
            _isDead = true;
            
            // Play death animation
            _sprite.Play("death");
            
            // Disable collision
            SetCollisionLayerBit(0, false);
            SetCollisionMaskBit(0, false);
            
            // Disable input
            SetPhysicsProcess(false);
            SetProcessInput(false);
            
            // Emit signal
            EmitSignal(SignalName.PlayerDied);
            
            // Notify game manager
            int networkId = (int)GetMeta("network_id", 0);
            GameManager.Instance.PlayerDied(networkId);
            
            if (NetworkManager.Instance.IsServer)
            {
                NetworkManager.Instance.Call("PlayerDied", networkId);
            }
        }

        public void Revive()
        {
            if (!_isDead) return;
            
            _isDead = false;
            
            // Reset health
            _currentHealth = _maxHealth / 2; // Revive with half health
            UpdateHealthBar();
            
            // Enable collision
            SetCollisionLayerBit(0, true);
            SetCollisionMaskBit(0, true);
            
            // Enable input
            SetPhysicsProcess(true);
            SetProcessInput(true);
            
            // Play revive animation
            _sprite.Play("idle");
            
            // Notify game manager
            int networkId = (int)GetMeta("network_id", 0);
            GameManager.Instance.PlayerRevived(networkId);
            
            if (NetworkManager.Instance.IsServer)
            {
                NetworkManager.Instance.Call("PlayerRevived", networkId);
            }
        }

        public void Heal(float amount)
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            UpdateHealthBar();
        }

        public void RestoreMana(float amount)
        {
            _currentMana = Mathf.Min(_manaMax, _currentMana + amount);
            UpdateManaBar();
        }

        private void UpdateHealthBar()
        {
            if (_healthBar != null)
            {
                _healthBar.Value = (_currentHealth / _maxHealth) * 100;
            }
            
            EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);
        }

        private void UpdateManaBar()
        {
            if (_manaBar != null)
            {
                _manaBar.Value = (_currentMana / _manaMax) * 100;
            }
            
            EmitSignal(SignalName.ManaChanged, _currentMana, _manaMax);
        }

        private void PlaySound(string soundName)
        {
            // This would be implemented to play the appropriate sound
            // For now, just log that we would play a sound
            GD.Print($"Playing sound: {soundName}");
        }

        public float GetCurrentHealth()
        {
            return _currentHealth;
        }

        public float GetMaxHealth()
        {
            return _maxHealth;
        }

        public float GetCurrentMana()
        {
            return _currentMana;
        }

        public float GetMaxMana()
        {
            return _manaMax;
        }
    }
}
