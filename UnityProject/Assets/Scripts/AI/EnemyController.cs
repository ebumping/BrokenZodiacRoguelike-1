using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.AI
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1.0f;
        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private bool isBoss = false;
        
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private UnityEngine.UI.Slider healthBar;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject projectilePrefab;
        
        // Internal state
        private float _currentHealth;
        private bool _isActive = false;
        private bool _canAttack = true;
        private float _attackCooldownTimer = 0f;
        private Vector3 _startPosition;
        private Transform _currentTarget;
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        
        // AI behavior
        public enum AIState { Idle, Patrol, Chase, Attack, Return, Stunned }
        
        [Header("AI")]
        [SerializeField] private AIState currentState = AIState.Idle;
        [SerializeField] private float patrolRadius = 5f;
        [SerializeField] private float patrolWaitTime = 2f;
        [SerializeField] private float returnThreshold = 15f;
        
        private Vector2 _patrolTarget;
        private float _patrolWaitTimer;
        private float _stunTimer;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }
        
        private void Start()
        {
            _currentHealth = maxHealth;
            _startPosition = transform.position;
            UpdateHealthBar();
        }
        
        private void Update()
        {
            if (!_isActive) return;
            
            // Update cooldowns
            if (!_canAttack)
            {
                _attackCooldownTimer -= Time.deltaTime;
                if (_attackCooldownTimer <= 0)
                {
                    _canAttack = true;
                }
            }
            
            // Update stun timer
            if (currentState == AIState.Stunned)
            {
                _stunTimer -= Time.deltaTime;
                if (_stunTimer <= 0)
                {
                    currentState = AIState.Idle;
                }
                return; // Don't process other states while stunned
            }
            
            // Update AI behavior based on state
            switch (currentState)
            {
                case AIState.Idle:
                    UpdateIdleState();
                    break;
                case AIState.Patrol:
                    UpdatePatrolState();
                    break;
                case AIState.Chase:
                    UpdateChaseState();
                    break;
                case AIState.Attack:
                    UpdateAttackState();
                    break;
                case AIState.Return:
                    UpdateReturnState();
                    break;
            }
            
            // Face the movement direction
            if (_rigidbody.velocity.x != 0 && _spriteRenderer != null)
            {
                _spriteRenderer.flipX = _rigidbody.velocity.x < 0;
            }
        }
        
        private void UpdateIdleState()
        {
            // Look for targets
            FindTarget();
            
            // If no target, start patrolling
            if (_currentTarget == null)
            {
                ChangeState(AIState.Patrol);
                SetPatrolTarget();
            }
            else
            {
                ChangeState(AIState.Chase);
            }
        }
        
        private void UpdatePatrolState()
        {
            // Look for targets while patrolling
            FindTarget();
            if (_currentTarget != null)
            {
                ChangeState(AIState.Chase);
                return;
            }
            
            // Move towards patrol point
            if (_patrolWaitTimer > 0)
            {
                _patrolWaitTimer -= Time.deltaTime;
                // Wait at patrol point
                _rigidbody.velocity = Vector2.zero;
                
                if (animator != null)
                {
                    animator.SetBool("IsMoving", false);
                }
            }
            else
            {
                // Move to patrol target
                Vector2 direction = ((Vector2)_patrolTarget - (Vector2)transform.position).normalized;
                _rigidbody.velocity = direction * moveSpeed;
                
                if (animator != null)
                {
                    animator.SetBool("IsMoving", true);
                }
                
                // Check if reached patrol point
                float distanceToTarget = Vector2.Distance(transform.position, _patrolTarget);
                if (distanceToTarget < 0.5f)
                {
                    // Reached patrol point, wait a bit
                    _patrolWaitTimer = patrolWaitTime;
                    
                    // Set new patrol target
                    SetPatrolTarget();
                }
            }
        }
        
        private void UpdateChaseState()
        {
            if (_currentTarget == null)
            {
                ChangeState(AIState.Return);
                return;
            }
            
            // Check if target is too far from start position
            float distanceFromStart = Vector2.Distance(transform.position, _startPosition);
            if (distanceFromStart > returnThreshold)
            {
                ChangeState(AIState.Return);
                return;
            }
            
            // Calculate distance to target
            float distanceToTarget = Vector2.Distance(transform.position, _currentTarget.position);
            
            // If within attack range, attack
            if (distanceToTarget <= attackRange)
            {
                ChangeState(AIState.Attack);
                return;
            }
            
            // Move towards target
            Vector2 direction = (_currentTarget.position - transform.position).normalized;
            _rigidbody.velocity = direction * moveSpeed;
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
        }
        
        private void UpdateAttackState()
        {
            // Stop moving when attacking
            _rigidbody.velocity = Vector2.zero;
            
            if (_currentTarget == null)
            {
                ChangeState(AIState.Idle);
                return;
            }
            
            // Check if target moved out of range
            float distanceToTarget = Vector2.Distance(transform.position, _currentTarget.position);
            if (distanceToTarget > attackRange)
            {
                ChangeState(AIState.Chase);
                return;
            }
            
            // Attack if possible
            if (_canAttack)
            {
                PerformAttack();
            }
        }
        
        private void UpdateReturnState()
        {
            // Check if we can chase a target again
            FindTarget();
            if (_currentTarget != null)
            {
                float distanceFromStart = Vector2.Distance(_currentTarget.position, _startPosition);
                if (distanceFromStart <= returnThreshold)
                {
                    ChangeState(AIState.Chase);
                    return;
                }
            }
            
            // Move back to start position
            Vector2 direction = (_startPosition - transform.position).normalized;
            _rigidbody.velocity = direction * moveSpeed;
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
            
            // Check if reached start position
            float distanceToStart = Vector2.Distance(transform.position, _startPosition);
            if (distanceToStart < 0.5f)
            {
                ChangeState(AIState.Idle);
                _rigidbody.velocity = Vector2.zero;
                
                if (animator != null)
                {
                    animator.SetBool("IsMoving", false);
                }
            }
        }
        
        private void FindTarget()
        {
            // Find closest player within detection range
            PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
            float closestDistance = detectionRange;
            Transform closestPlayer = null;
            
            foreach (var player in players)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player.transform;
                }
            }
            
            _currentTarget = closestPlayer;
        }
        
        private void SetPatrolTarget()
        {
            // Generate a random point within patrol radius
            Vector2 randomDirection = Random.insideUnitCircle.normalized * Random.Range(0f, patrolRadius);
            _patrolTarget = (Vector2)_startPosition + randomDirection;
        }
        
        private void PerformAttack()
        {
            _canAttack = false;
            _attackCooldownTimer = attackCooldown;
            
            // Play attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // If ranged enemy, spawn projectile
            if (projectilePrefab != null && projectileSpawnPoint != null)
            {
                // Aim towards target
                Vector2 direction = (_currentTarget.position - projectileSpawnPoint.position).normalized;
                GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
                
                // Setup projectile
                Projectile projectileComponent = projectile.GetComponent<Projectile>();
                if (projectileComponent != null)
                {
                    projectileComponent.Initialize(direction, damage, 10f, -1);
                }
            }
            else
            {
                // Melee attack - damage player directly if still in range
                PlayerController player = _currentTarget.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }
        }
        
        private void ChangeState(AIState newState)
        {
            if (currentState == newState) return;
            
            // Exit previous state
            switch (currentState)
            {
                case AIState.Attack:
                    if (animator != null)
                    {
                        animator.SetBool("IsAttacking", false);
                    }
                    break;
            }
            
            // Enter new state
            currentState = newState;
            
            switch (newState)
            {
                case AIState.Idle:
                    _rigidbody.velocity = Vector2.zero;
                    if (animator != null)
                    {
                        animator.SetBool("IsMoving", false);
                    }
                    break;
                case AIState.Attack:
                    _rigidbody.velocity = Vector2.zero;
                    if (animator != null)
                    {
                        animator.SetBool("IsAttacking", true);
                    }
                    break;
                case AIState.Stunned:
                    _rigidbody.velocity = Vector2.zero;
                    if (animator != null)
                    {
                        animator.SetTrigger("Stunned");
                    }
                    break;
            }
        }
        
        public void SetupEnemy(float health, float attackDamage)
        {
            maxHealth = health;
            _currentHealth = health;
            damage = attackDamage;
            
            UpdateHealthBar();
        }
        
        public void Activate()
        {
            _isActive = true;
            currentState = AIState.Idle;
        }
        
        public void Deactivate()
        {
            _isActive = false;
            _rigidbody.velocity = Vector2.zero;
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsAttacking", false);
            }
        }
        
        public void TakeDamage(float amount)
        {
            if (!_isActive) return;
            
            _currentHealth -= amount;
            UpdateHealthBar();
            
            // Play hit animation
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
            
            // Check for death
            if (_currentHealth <= 0)
            {
                Die();
                return;
            }
            
            // Chance to be stunned when hit
            if (!isBoss && Random.value < 0.2f)
            {
                Stun(0.5f);
            }
            
            // If this is the first hit from player, chase them
            if (currentState == AIState.Idle || currentState == AIState.Patrol)
            {
                FindTarget();
                if (_currentTarget != null)
                {
                    ChangeState(AIState.Chase);
                }
            }
        }
        
        public void Stun(float duration)
        {
            if (isBoss) return; // Bosses are immune to stun
            
            _stunTimer = duration;
            ChangeState(AIState.Stunned);
        }
        
        private void Die()
        {
            // Play death animation
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }
            
            // Disable components
            _isActive = false;
            GetComponent<Collider2D>().enabled = false;
            _rigidbody.velocity = Vector2.zero;
            
            // Register enemy defeat with game manager
            GameManager.Instance?.EnemyDefeated();
            
            // Drop loot - typically handled by a separate loot component
            
            // Destroy after animation
            StartCoroutine(DestroyAfterDelay(2f));
        }
        
        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Notify room controller of enemy death
            RoomController room = GetComponentInParent<RoomController>();
            if (room != null)
            {
                room.CheckRoomClear();
            }
            
            // Destroy the enemy
            Destroy(gameObject);
        }
        
        private void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                healthBar.value = _currentHealth / maxHealth;
            }
        }
        
        // For debugging enemy behavior
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            if (Application.isPlaying && _isActive && currentState == AIState.Patrol)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, _patrolTarget);
            }
        }
        
        // Used by Unity's animation events
        public void OnAttackAnimationHit()
        {
            // Called during animation to time the actual hit with the animation
            if (_currentTarget != null && Vector2.Distance(transform.position, _currentTarget.position) <= attackRange)
            {
                PlayerController player = _currentTarget.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }
        }
    }
    
    // Unity doesn't have a built-in Projectile class, so using the one from PlayerController
    // In a real implementation, you'd have a shared Projectile class used by both players and enemies
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
            
            // Rotate to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            
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
            if (enemy != null && _ownerId != -1) // -1 is used for enemy projectiles
            {
                enemy.TakeDamage(_damage);
                DestroyProjectile();
                return;
            }
            
            // Check if we hit a player (that isn't the owner)
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && _ownerId == -1) // Player can be hit by enemy projectiles
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