using Godot;
using System;
using System.Collections.Generic;
using CodexOfTheBrokenZodiac.Resources;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.AI
{
    public partial class EnemyController : CharacterBody2D
    {
        // Enemy configuration
        [Export] public string EnemyType { get; set; } = "BasicEnemy";
        [Export] public int Level { get; set; } = 1;
        [Export] public NodePath AIBehaviorPath { get; set; }
        
        // Node references
        private AIBehavior _aiBehavior;
        private AnimatedSprite2D _sprite;
        private ProgressBar _healthBar;
        private Area2D _detectionArea;
        private Timer _attackCooldownTimer;
        private Timer _specialAttackCooldownTimer;
        private AudioStreamPlayer2D _audioPlayer;
        private RayCast2D _lineOfSightRay;
        private Node2D _projectileSpawnPoint;
        
        // Enemy state
        private Enemy _enemyData;
        private float _currentHealth;
        private float _maxHealth;
        private bool _isAttacking = false;
        private bool _isSpecialAttacking = false;
        private bool _canAttack = true;
        private bool _canSpecialAttack = true;
        private Node2D _currentTarget;
        private Vector2 _lastKnownTargetPosition = Vector2.Zero;
        private bool _hasLineOfSight = false;
        private float _timeSinceLastSawTarget = 0.0f;
        private Vector2 _wanderTarget = Vector2.Zero;
        private float _wanderTimer = 0.0f;
        private const float WANDER_TIME = 3.0f;
        private bool _isDead = false;
        
        // Signals
        [Signal]
        public delegate void EnemyDefeatedEventHandler(Vector2 position, string enemyType);
        
        // State machine states
        public enum AIState { Idle, Patrol, Chase, Attack, SpecialAttack, Flee, TakeCover, Dead }
        private AIState _currentState = AIState.Idle;
        
        public override void _Ready()
        {
            // Get node references
            _sprite = GetNode<AnimatedSprite2D>("Sprite");
            _healthBar = GetNode<ProgressBar>("HealthBar");
            _detectionArea = GetNode<Area2D>("DetectionArea");
            _attackCooldownTimer = GetNode<Timer>("AttackCooldownTimer");
            _specialAttackCooldownTimer = GetNode<Timer>("SpecialAttackCooldownTimer");
            _audioPlayer = GetNode<AudioStreamPlayer2D>("AudioPlayer");
            _lineOfSightRay = GetNode<RayCast2D>("LineOfSightRay");
            _projectileSpawnPoint = GetNode<Node2D>("ProjectileSpawnPoint");
            
            if (AIBehaviorPath != null)
            {
                _aiBehavior = GetNode<AIBehavior>(AIBehaviorPath);
            }
            
            // Connect signals
            _detectionArea.BodyEntered += OnBodyEnteredDetectionArea;
            _detectionArea.BodyExited += OnBodyExitedDetectionArea;
            _attackCooldownTimer.Timeout += OnAttackCooldownTimeout;
            _specialAttackCooldownTimer.Timeout += OnSpecialAttackCooldownTimeout;
            
            // Load enemy data
            LoadEnemyData();
            
            // Initialize state
            SetState(AIState.Idle);
        }

        public override void _Process(double delta)
        {
            if (_isDead) return;
            
            UpdateHealthBar();
            UpdateLineOfSight();
            
            // Update wandering behavior
            if (_currentState == AIState.Patrol)
            {
                _wanderTimer -= (float)delta;
                if (_wanderTimer <= 0)
                {
                    SetNewWanderTarget();
                }
            }
            
            // Update target tracking timer
            if (_currentTarget != null && !_hasLineOfSight)
            {
                _timeSinceLastSawTarget += (float)delta;
                if (_timeSinceLastSawTarget > 5.0f)
                {
                    // Lost target for too long, go back to patrolling
                    SetState(AIState.Patrol);
                    _currentTarget = null;
                }
            }
            
            // Process AI behavior
            if (_aiBehavior != null)
            {
                _aiBehavior.Process(_currentState, (float)delta, this);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isDead) return;
            
            Vector2 velocity = Vector2.Zero;
            
            switch (_currentState)
            {
                case AIState.Idle:
                    // Do nothing, stay in place
                    velocity = Vector2.Zero;
                    break;
                    
                case AIState.Patrol:
                    // Move toward wander target
                    if (_wanderTarget != Vector2.Zero)
                    {
                        velocity = MoveTo(_wanderTarget, _enemyData.MoveSpeed * 0.5f, delta);
                    }
                    break;
                    
                case AIState.Chase:
                    // Chase target if we have one
                    if (_currentTarget != null)
                    {
                        // Use last known position if no line of sight
                        Vector2 targetPos = _hasLineOfSight ? _currentTarget.GlobalPosition : _lastKnownTargetPosition;
                        velocity = MoveTo(targetPos, _enemyData.MoveSpeed, delta);
                        
                        // If in attack range, switch to attack state
                        float distanceToTarget = GlobalPosition.DistanceTo(targetPos);
                        if (distanceToTarget <= _enemyData.AttackRange && _canAttack)
                        {
                            SetState(AIState.Attack);
                        }
                    }
                    else
                    {
                        SetState(AIState.Patrol);
                    }
                    break;
                    
                case AIState.Attack:
                    // For melee enemies, stay still during attack
                    if (_enemyData.Pattern == Enemy.AttackPattern.Melee)
                    {
                        velocity = Vector2.Zero;
                    }
                    // For ranged enemies, maintain distance
                    else if (_enemyData.Pattern == Enemy.AttackPattern.Ranged && _currentTarget != null)
                    {
                        float idealRange = _enemyData.AttackRange * 0.8f;
                        float currentDistance = GlobalPosition.DistanceTo(_currentTarget.GlobalPosition);
                        
                        if (currentDistance < idealRange)
                        {
                            // Move away from target
                            Vector2 awayDir = (GlobalPosition - _currentTarget.GlobalPosition).Normalized();
                            velocity = awayDir * _enemyData.MoveSpeed * 0.5f;
                        }
                    }
                    break;
                    
                case AIState.Flee:
                    // Move away from target
                    if (_currentTarget != null)
                    {
                        Vector2 fleeDirection = (GlobalPosition - _currentTarget.GlobalPosition).Normalized();
                        velocity = fleeDirection * _enemyData.MoveSpeed * 1.2f;
                    }
                    else
                    {
                        SetState(AIState.Patrol);
                    }
                    break;
                    
                case AIState.TakeCover:
                    // Find and move toward cover position
                    // This would be implemented in a more complex game
                    velocity = Vector2.Zero;
                    break;
                    
                case AIState.Dead:
                    velocity = Vector2.Zero;
                    break;
            }
            
            // Apply calculated velocity
            Velocity = velocity;
            MoveAndSlide();
            
            // Update sprite facing direction
            if (velocity != Vector2.Zero)
            {
                bool facingRight = velocity.X > 0;
                _sprite.FlipH = facingRight;
            }
        }

        private Vector2 MoveTo(Vector2 target, float speed, double delta)
        {
            Vector2 direction = (target - GlobalPosition).Normalized();
            return direction * speed;
        }

        private void LoadEnemyData()
        {
            _enemyData = ResourceManager.Instance.GetEnemyType(EnemyType);
            
            if (_enemyData == null)
            {
                GD.PrintErr($"Failed to load enemy data for type: {EnemyType}");
                // Use default values
                _maxHealth = 100.0f;
            }
            else
            {
                // Set enemy properties based on data
                _maxHealth = _enemyData.GetScaledHealth(Level);
                
                // Configure detection area
                CollisionShape2D detectionShape = _detectionArea.GetNode<CollisionShape2D>("CollisionShape2D");
                if (detectionShape != null)
                {
                    CircleShape2D circle = new CircleShape2D();
                    circle.Radius = _enemyData.DetectionRange;
                    detectionShape.Shape = circle;
                }
                
                // Configure attack timers
                _attackCooldownTimer.WaitTime = 1.0f / _enemyData.AttackSpeed;
                if (_enemyData.HasSpecialAttack)
                {
                    _specialAttackCooldownTimer.WaitTime = _enemyData.SpecialAttackCooldown;
                    _specialAttackCooldownTimer.Start();
                }
            }
            
            _currentHealth = _maxHealth;
            UpdateHealthBar();
        }

        private void SetState(AIState newState)
        {
            if (_currentState == newState) return;
            
            // Exit current state
            switch (_currentState)
            {
                case AIState.Attack:
                    if (_isAttacking)
                    {
                        StopAttack();
                    }
                    break;
                    
                case AIState.SpecialAttack:
                    if (_isSpecialAttacking)
                    {
                        StopSpecialAttack();
                    }
                    break;
            }
            
            // Enter new state
            _currentState = newState;
            
            switch (_currentState)
            {
                case AIState.Idle:
                    _sprite.Play("idle");
                    break;
                    
                case AIState.Patrol:
                    _sprite.Play("walk");
                    SetNewWanderTarget();
                    break;
                    
                case AIState.Chase:
                    _sprite.Play("walk");
                    // Play alert sound
                    if (!string.IsNullOrEmpty(_enemyData?.AlertSound))
                    {
                        PlaySound(_enemyData.AlertSound);
                    }
                    break;
                    
                case AIState.Attack:
                    StartAttack();
                    break;
                    
                case AIState.SpecialAttack:
                    if (_canSpecialAttack)
                    {
                        StartSpecialAttack();
                    }
                    else
                    {
                        // Fall back to regular attack if special attack is on cooldown
                        SetState(AIState.Attack);
                    }
                    break;
                    
                case AIState.Flee:
                    _sprite.Play("walk");
                    break;
                    
                case AIState.TakeCover:
                    _sprite.Play("walk");
                    break;
                    
                case AIState.Dead:
                    Die();
                    break;
            }
        }

        public void TakeDamage(float amount)
        {
            _currentHealth -= amount;
            UpdateHealthBar();
            
            // Play hit animation and sound
            _sprite.Play("hit");
            
            // Check if should flee
            if (_currentHealth / _maxHealth < _enemyData.FleeHealthThreshold && _enemyData.FleeHealthThreshold > 0)
            {
                SetState(AIState.Flee);
            }
            
            // Check for death
            if (_currentHealth <= 0)
            {
                SetState(AIState.Dead);
            }
            else
            {
                // If we weren't targeting anything, target the attacker
                if (_currentTarget == null && _currentState != AIState.Flee)
                {
                    // Find closest player and chase them
                    Node2D closestPlayer = FindClosestPlayer();
                    if (closestPlayer != null)
                    {
                        _currentTarget = closestPlayer;
                        SetState(AIState.Chase);
                    }
                }
            }
        }

        private void Die()
        {
            if (_isDead) return;
            
            _isDead = true;
            
            // Play death animation
            _sprite.Play("death");
            
            // Play death sound
            if (!string.IsNullOrEmpty(_enemyData?.DeathSound))
            {
                PlaySound(_enemyData.DeathSound);
            }
            
            // Disable collision
            SetCollisionLayerBit(0, false);
            SetCollisionMaskBit(0, false);
            
            // Disable detection area
            _detectionArea.SetDeferred("monitoring", false);
            _detectionArea.SetDeferred("monitorable", false);
            
            // Signal defeat to spawn loot, etc.
            EmitSignal(SignalName.EnemyDefeated, GlobalPosition, EnemyType);
            
            // Update game manager
            GameManager.Instance.EnemyDefeated();
            
            // Queue for deletion after animation finishes
            GetTree().CreateTimer(2.0f).Timeout += () =>
            {
                QueueFree();
            };
        }

        private void StartAttack()
        {
            if (!_canAttack) return;
            
            _isAttacking = true;
            _canAttack = false;
            
            // Play attack animation
            _sprite.Play("attack");
            
            // Play attack sound
            if (!string.IsNullOrEmpty(_enemyData?.AttackSound))
            {
                PlaySound(_enemyData.AttackSound);
            }
            
            // Perform attack based on attack pattern
            if (_currentTarget != null)
            {
                switch (_enemyData.Pattern)
                {
                    case Enemy.AttackPattern.Melee:
                        PerformMeleeAttack();
                        break;
                        
                    case Enemy.AttackPattern.Ranged:
                        PerformRangedAttack();
                        break;
                        
                    case Enemy.AttackPattern.AOE:
                        PerformAOEAttack();
                        break;
                        
                    case Enemy.AttackPattern.Summoner:
                        PerformSummonAttack();
                        break;
                        
                    case Enemy.AttackPattern.Support:
                        PerformSupportAction();
                        break;
                }
            }
            
            // Start attack cooldown
            _attackCooldownTimer.Start();
            
            // Go back to chase state after attack completes
            GetTree().CreateTimer(0.5f).Timeout += () =>
            {
                if (_isAttacking)
                {
                    StopAttack();
                    SetState(AIState.Chase);
                }
            };
        }

        private void StopAttack()
        {
            _isAttacking = false;
        }

        private void StartSpecialAttack()
        {
            if (!_canSpecialAttack) return;
            
            _isSpecialAttacking = true;
            _canSpecialAttack = false;
            
            // Play special attack animation
            _sprite.Play("special_attack");
            
            // Perform special attack
            // This would be implemented based on enemy type
            
            // Start special attack cooldown
            _specialAttackCooldownTimer.Start();
            
            // Go back to chase state after attack completes
            GetTree().CreateTimer(1.0f).Timeout += () =>
            {
                if (_isSpecialAttacking)
                {
                    StopSpecialAttack();
                    SetState(AIState.Chase);
                }
            };
        }

        private void StopSpecialAttack()
        {
            _isSpecialAttacking = false;
        }

        private void PerformMeleeAttack()
        {
            if (_currentTarget == null) return;
            
            // Check if target is in melee range
            float distanceToTarget = GlobalPosition.DistanceTo(_currentTarget.GlobalPosition);
            if (distanceToTarget <= _enemyData.AttackRange)
            {
                // Deal damage to target if it's a player
                if (_currentTarget.HasMethod("TakeDamage"))
                {
                    float damage = _enemyData.GetScaledDamage(Level);
                    _currentTarget.Call("TakeDamage", damage);
                }
            }
        }

        private void PerformRangedAttack()
        {
            if (_currentTarget == null) return;
            
            // Spawn projectile
            if (!string.IsNullOrEmpty(_enemyData.ProjectileScene))
            {
                Vector2 direction = (_currentTarget.GlobalPosition - GlobalPosition).Normalized();
                SpawnProjectile(_enemyData.ProjectileScene, _projectileSpawnPoint.GlobalPosition, direction);
            }
        }

        private void PerformAOEAttack()
        {
            // This would create an area effect attack
            // For now, just do a basic projectile spray
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.Pi / 3.0f;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnProjectile(_enemyData.ProjectileScene, _projectileSpawnPoint.GlobalPosition, direction);
            }
        }

        private void PerformSummonAttack()
        {
            // This would spawn minions
            // For a basic implementation, we'll just mention it
            GD.Print($"{EnemyType} would summon minions here if implemented");
        }

        private void PerformSupportAction()
        {
            // This would buff nearby allies
            // For a basic implementation, we'll just mention it
            GD.Print($"{EnemyType} would buff allies here if implemented");
        }

        private void SpawnProjectile(string projectilePath, Vector2 position, Vector2 direction)
        {
            if (string.IsNullOrEmpty(projectilePath)) return;
            
            // Load projectile scene
            PackedScene projectileScene = ResourceLoader.Load<PackedScene>(projectilePath);
            if (projectileScene == null)
            {
                // Fall back to default projectile
                projectileScene = ResourceLoader.Load<PackedScene>("res://Scenes/Items/EnemyProjectile.tscn");
                if (projectileScene == null)
                {
                    GD.PrintErr("Failed to load projectile scene");
                    return;
                }
            }
            
            // Instantiate projectile
            Node2D projectile = projectileScene.Instantiate<Node2D>();
            GetTree().CurrentScene.AddChild(projectile);
            
            // Set projectile properties
            projectile.GlobalPosition = position;
            projectile.Rotation = direction.Angle();
            
            // Set projectile metadata
            projectile.SetMeta("damage", _enemyData.GetScaledDamage(Level));
            projectile.SetMeta("speed", _enemyData.ProjectileSpeed);
            projectile.SetMeta("direction", direction);
            projectile.SetMeta("owner_id", GetInstanceId());
            
            // If the projectile has an initialize method, call it
            if (projectile.HasMethod("Initialize"))
            {
                projectile.Call("Initialize", direction, _enemyData.GetScaledDamage(Level), false);
            }
        }

        private void OnAttackCooldownTimeout()
        {
            _canAttack = true;
            
            // If we're chasing a target and in range, attack again
            if (_currentState == AIState.Chase && _currentTarget != null)
            {
                float distanceToTarget = GlobalPosition.DistanceTo(_currentTarget.GlobalPosition);
                if (distanceToTarget <= _enemyData.AttackRange)
                {
                    SetState(AIState.Attack);
                }
            }
        }

        private void OnSpecialAttackCooldownTimeout()
        {
            _canSpecialAttack = true;
        }

        private void OnBodyEnteredDetectionArea(Node2D body)
        {
            // Check if it's a player
            if (body.IsInGroup("Players") && !_isDead)
            {
                // Only target if we don't already have a target or if this one is closer
                if (_currentTarget == null)
                {
                    _currentTarget = body;
                    _lastKnownTargetPosition = body.GlobalPosition;
                    
                    if (_currentState == AIState.Idle || _currentState == AIState.Patrol)
                    {
                        SetState(AIState.Chase);
                    }
                }
                else
                {
                    float currentDistance = GlobalPosition.DistanceTo(_currentTarget.GlobalPosition);
                    float newDistance = GlobalPosition.DistanceTo(body.GlobalPosition);
                    
                    if (newDistance < currentDistance)
                    {
                        _currentTarget = body;
                        _lastKnownTargetPosition = body.GlobalPosition;
                    }
                }
            }
        }

        private void OnBodyExitedDetectionArea(Node2D body)
        {
            // If our target left detection area
            if (body == _currentTarget)
            {
                // Remember last position, but don't immediately lose target
                _lastKnownTargetPosition = body.GlobalPosition;
                _hasLineOfSight = false;
                _timeSinceLastSawTarget = 0;
                
                // Find a new target if possible
                Node2D newTarget = FindClosestPlayer();
                if (newTarget != null)
                {
                    _currentTarget = newTarget;
                    _lastKnownTargetPosition = newTarget.GlobalPosition;
                    _hasLineOfSight = true;
                }
            }
        }

        private Node2D FindClosestPlayer()
        {
            Node2D closestPlayer = null;
            float closestDistance = float.MaxValue;
            
            // Find all players in the scene
            Godot.Collections.Array<Node> players = GetTree().GetNodesInGroup("Players");
            foreach (Node player in players)
            {
                if (player is Node2D playerNode)
                {
                    float distance = GlobalPosition.DistanceTo(playerNode.GlobalPosition);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = playerNode;
                    }
                }
            }
            
            return closestPlayer;
        }

        private void UpdateLineOfSight()
        {
            if (_currentTarget == null) return;
            
            // Update line of sight ray
            _lineOfSightRay.TargetPosition = _currentTarget.GlobalPosition - GlobalPosition;
            
            // Check if we have line of sight to target
            bool previousLineOfSight = _hasLineOfSight;
            _hasLineOfSight = !_lineOfSightRay.IsColliding();
            
            // If we just gained line of sight
            if (_hasLineOfSight && !previousLineOfSight)
            {
                _timeSinceLastSawTarget = 0;
            }
            
            // Update last known position if we have line of sight
            if (_hasLineOfSight)
            {
                _lastKnownTargetPosition = _currentTarget.GlobalPosition;
                _timeSinceLastSawTarget = 0;
            }
        }

        private void SetNewWanderTarget()
        {
            // Set a random point within wandering radius
            float radius = _enemyData?.DetectionRange ?? 5.0f;
            float angle = (float)GD.RandfRange(0, Mathf.Pi * 2);
            float distance = (float)GD.RandfRange(radius * 0.3f, radius * 0.7f);
            
            _wanderTarget = GlobalPosition + new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );
            
            _wanderTimer = WANDER_TIME;
        }

        private void UpdateHealthBar()
        {
            if (_healthBar != null)
            {
                _healthBar.Value = (_currentHealth / _maxHealth) * 100;
                
                // Only show health bar when damaged
                _healthBar.Visible = _currentHealth < _maxHealth;
            }
        }

        private void PlaySound(string soundName)
        {
            if (_audioPlayer != null)
            {
                // This would load and play the appropriate sound file
                // For now, just log the sound
                GD.Print($"Enemy {EnemyType} plays sound: {soundName}");
            }
        }
    }
}
