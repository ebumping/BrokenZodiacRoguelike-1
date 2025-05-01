using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Controls enemy behavior, AI, and combat
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] private string enemyName = "Unknown Entity";
        [SerializeField] private float movementSpeed = 3f;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private int moteDropMin = 1;
        [SerializeField] private int moteDropMax = 3;
        
        [Header("AI Settings")]
        [SerializeField] private EnemyType enemyType = EnemyType.Melee;
        [SerializeField] private AIBehavior aiBehavior = AIBehavior.Aggressive;
        [SerializeField] private float wanderRadius = 5f;
        [SerializeField] private float steeringSpeed = 120f;
        [SerializeField] private bool canSensePlayerThroughWalls = false;
        
        [Header("References")]
        [SerializeField] private Transform enemyModel;
        [SerializeField] private GameObject attackEffectPrefab;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private Canvas healthBarCanvas;
        [SerializeField] private Image healthBarImage;
        
        // Components
        private HealthSystem healthSystem;
        private NavMeshAgent navMeshAgent;
        private Animator animator;
        private AudioSource audioSource;
        
        // State
        private Transform playerTarget;
        private Vector3 wanderTarget;
        private float attackTimer;
        private bool isAttacking;
        private bool isStunned;
        private bool isAggro;
        private bool isWandering;
        private EnemyState currentState = EnemyState.Idle;
        
        // Cache
        private Dictionary<ZodiacSign, float> zodiacResistances = new Dictionary<ZodiacSign, float>();
        
        private void Awake()
        {
            // Get components
            healthSystem = GetComponent<HealthSystem>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
                audioSource.maxDistance = 20f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
            }
            
            // Set up NavMeshAgent
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = movementSpeed;
                navMeshAgent.angularSpeed = steeringSpeed;
                navMeshAgent.stoppingDistance = attackRange * 0.8f;
            }
            
            // Set up health system events
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged.AddListener(UpdateHealthBar);
                healthSystem.OnDeath.AddListener(Die);
            }
            
            // Initialize resistances based on enemy type
            InitializeResistances();
        }
        
        private void Start()
        {
            // Find player
            FindPlayer();
            
            // Start in idle state
            EnterState(EnemyState.Idle);
            
            // Generate initial wander target
            SetNewWanderTarget();
        }
        
        private void Update()
        {
            if (isStunned)
                return;
            
            // Update attack timer
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
            
            // Update state machine
            UpdateCurrentState();
            
            // Update health bar rotation to face camera
            if (healthBarCanvas != null)
            {
                healthBarCanvas.transform.rotation = Quaternion.LookRotation(healthBarCanvas.transform.position - Camera.main.transform.position);
            }
        }
        
        #region State Machine
        
        /// <summary>
        /// Enter a new enemy state
        /// </summary>
        private void EnterState(EnemyState newState)
        {
            // Exit current state
            ExitState(currentState);
            
            // Set new state
            currentState = newState;
            
            // Enter new state
            switch (currentState)
            {
                case EnemyState.Idle:
                    EnterIdleState();
                    break;
                case EnemyState.Wander:
                    EnterWanderState();
                    break;
                case EnemyState.Chase:
                    EnterChaseState();
                    break;
                case EnemyState.Attack:
                    EnterAttackState();
                    break;
                case EnemyState.Flee:
                    EnterFleeState();
                    break;
            }
            
            // Update animator
            if (animator != null)
            {
                animator.SetInteger("State", (int)currentState);
            }
        }
        
        /// <summary>
        /// Exit the current enemy state
        /// </summary>
        private void ExitState(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Idle:
                    ExitIdleState();
                    break;
                case EnemyState.Wander:
                    ExitWanderState();
                    break;
                case EnemyState.Chase:
                    ExitChaseState();
                    break;
                case EnemyState.Attack:
                    ExitAttackState();
                    break;
                case EnemyState.Flee:
                    ExitFleeState();
                    break;
            }
        }
        
        /// <summary>
        /// Update the current state based on conditions
        /// </summary>
        private void UpdateCurrentState()
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdleState();
                    break;
                case EnemyState.Wander:
                    UpdateWanderState();
                    break;
                case EnemyState.Chase:
                    UpdateChaseState();
                    break;
                case EnemyState.Attack:
                    UpdateAttackState();
                    break;
                case EnemyState.Flee:
                    UpdateFleeState();
                    break;
            }
        }
        
        #endregion
        
        #region Idle State
        
        private void EnterIdleState()
        {
            // Stop movement
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
            }
        }
        
        private void UpdateIdleState()
        {
            // Check if player is in detection range
            if (CanDetectPlayer())
            {
                // Choose between chase and flee based on AI behavior
                if (ShouldFlee())
                {
                    EnterState(EnemyState.Flee);
                }
                else
                {
                    EnterState(EnemyState.Chase);
                }
            }
            // Randomly start wandering
            else if (Random.value < 0.01f) // 1% chance per frame
            {
                EnterState(EnemyState.Wander);
            }
        }
        
        private void ExitIdleState()
        {
            // Nothing specific to do when exiting idle
        }
        
        #endregion
        
        #region Wander State
        
        private void EnterWanderState()
        {
            isWandering = true;
            
            // Start movement
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(wanderTarget);
            }
        }
        
        private void UpdateWanderState()
        {
            // Check if player is in detection range
            if (CanDetectPlayer())
            {
                // Choose between chase and flee based on AI behavior
                if (ShouldFlee())
                {
                    EnterState(EnemyState.Flee);
                }
                else
                {
                    EnterState(EnemyState.Chase);
                }
                return;
            }
            
            // Check if we've reached the wander target
            if (navMeshAgent != null && !navMeshAgent.pathPending)
            {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    // Choose between setting a new wander target or going idle
                    if (Random.value < 0.7f) // 70% chance to keep wandering
                    {
                        SetNewWanderTarget();
                        navMeshAgent.SetDestination(wanderTarget);
                    }
                    else
                    {
                        EnterState(EnemyState.Idle);
                    }
                }
            }
        }
        
        private void ExitWanderState()
        {
            isWandering = false;
        }
        
        #endregion
        
        #region Chase State
        
        private void EnterChaseState()
        {
            isAggro = true;
            
            // Start movement
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
                if (playerTarget != null)
                {
                    navMeshAgent.SetDestination(playerTarget.position);
                }
            }
        }
        
        private void UpdateChaseState()
        {
            // If player is null or dead, go back to idle
            if (playerTarget == null)
            {
                FindPlayer();
                if (playerTarget == null)
                {
                    EnterState(EnemyState.Idle);
                    return;
                }
            }
            
            // Update destination to player position
            if (navMeshAgent != null && playerTarget != null)
            {
                navMeshAgent.SetDestination(playerTarget.position);
            }
            
            // Check if player is in attack range
            if (IsInAttackRange())
            {
                EnterState(EnemyState.Attack);
                return;
            }
            
            // Check if player is out of detection range
            if (!CanDetectPlayer())
            {
                // Return to idle or wander
                if (Random.value < 0.5f)
                {
                    EnterState(EnemyState.Idle);
                }
                else
                {
                    EnterState(EnemyState.Wander);
                }
            }
            
            // Check if we should flee based on health
            if (ShouldFlee())
            {
                EnterState(EnemyState.Flee);
            }
        }
        
        private void ExitChaseState()
        {
            // Nothing specific to do when exiting chase
        }
        
        #endregion
        
        #region Attack State
        
        private void EnterAttackState()
        {
            // Stop movement during attack
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
            }
            
            // Face the player
            if (playerTarget != null && enemyModel != null)
            {
                Vector3 direction = playerTarget.position - transform.position;
                direction.y = 0;
                enemyModel.rotation = Quaternion.LookRotation(direction);
            }
            
            // Start attack if cooldown is ready
            if (attackTimer <= 0)
            {
                PerformAttack();
            }
        }
        
        private void UpdateAttackState()
        {
            // If player is null or dead, go back to idle
            if (playerTarget == null)
            {
                FindPlayer();
                if (playerTarget == null)
                {
                    EnterState(EnemyState.Idle);
                    return;
                }
            }
            
            // If attack is complete and player is out of range, chase again
            if (!isAttacking && !IsInAttackRange())
            {
                EnterState(EnemyState.Chase);
                return;
            }
            
            // If attack is complete and cooldown is ready, attack again
            if (!isAttacking && attackTimer <= 0 && IsInAttackRange())
            {
                PerformAttack();
            }
            
            // Check if we should flee based on health
            if (ShouldFlee())
            {
                EnterState(EnemyState.Flee);
            }
        }
        
        private void ExitAttackState()
        {
            isAttacking = false;
        }
        
        #endregion
        
        #region Flee State
        
        private void EnterFleeState()
        {
            // Start movement
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
                
                if (playerTarget != null)
                {
                    // Calculate direction away from player
                    Vector3 fleeDirection = transform.position - playerTarget.position;
                    fleeDirection.y = 0;
                    fleeDirection = fleeDirection.normalized;
                    
                    // Set destination to a point away from player
                    Vector3 fleeTarget = transform.position + fleeDirection * 10f;
                    
                    // Find a valid position on the NavMesh
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(fleeTarget, out hit, 10f, NavMesh.AllAreas))
                    {
                        navMeshAgent.SetDestination(hit.position);
                    }
                }
            }
        }
        
        private void UpdateFleeState()
        {
            // If player is null, go back to idle
            if (playerTarget == null)
            {
                EnterState(EnemyState.Idle);
                return;
            }
            
            // Check if we've reached the flee target
            if (navMeshAgent != null && !navMeshAgent.pathPending)
            {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    // Calculate a new flee direction
                    EnterFleeState();
                }
            }
            
            // Check if player is out of detection range
            if (!CanDetectPlayer())
            {
                // Return to idle or wander
                if (Random.value < 0.5f)
                {
                    EnterState(EnemyState.Idle);
                }
                else
                {
                    EnterState(EnemyState.Wander);
                }
            }
            
            // Check if we should stop fleeing based on health recovery
            if (!ShouldFlee())
            {
                EnterState(EnemyState.Chase);
            }
        }
        
        private void ExitFleeState()
        {
            // Nothing specific to do when exiting flee
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Find the player in the scene
        /// </summary>
        private void FindPlayer()
        {
            PlayerController player = GameObject.FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }
        
        /// <summary>
        /// Set a new random wander target
        /// </summary>
        private void SetNewWanderTarget()
        {
            // Get a random direction
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0;
            
            // Add to current position
            Vector3 targetPosition = transform.position + randomDirection;
            
            // Find a valid position on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, wanderRadius, NavMesh.AllAreas))
            {
                wanderTarget = hit.position;
            }
            else
            {
                // If no valid position found, use a closer radius
                randomDirection = Random.insideUnitSphere * (wanderRadius * 0.5f);
                randomDirection.y = 0;
                targetPosition = transform.position + randomDirection;
                
                if (NavMesh.SamplePosition(targetPosition, out hit, wanderRadius * 0.5f, NavMesh.AllAreas))
                {
                    wanderTarget = hit.position;
                }
                else
                {
                    // If still no valid position, stay in place
                    wanderTarget = transform.position;
                }
            }
        }
        
        /// <summary>
        /// Check if player is in detection range
        /// </summary>
        private bool CanDetectPlayer()
        {
            if (playerTarget == null)
                return false;
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            
            // Check distance
            if (distanceToPlayer > detectionRange)
                return false;
            
            // Check line of sight if needed
            if (!canSensePlayerThroughWalls)
            {
                RaycastHit hit;
                Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
                
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
                {
                    // Check if the ray hit the player or something else
                    PlayerController player = hit.collider.GetComponent<PlayerController>();
                    if (player == null)
                    {
                        // Hit something that's not the player
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if player is in attack range
        /// </summary>
        private bool IsInAttackRange()
        {
            if (playerTarget == null)
                return false;
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            return distanceToPlayer <= attackRange;
        }
        
        /// <summary>
        /// Check if the enemy should flee based on health and behavior
        /// </summary>
        private bool ShouldFlee()
        {
            if (healthSystem == null)
                return false;
            
            // Based on AI behavior
            switch (aiBehavior)
            {
                case AIBehavior.Cowardly:
                    // Flee if health is below 70%
                    return healthSystem.GetHealthPercentage() < 0.7f;
                    
                case AIBehavior.Cautious:
                    // Flee if health is below 30%
                    return healthSystem.GetHealthPercentage() < 0.3f;
                    
                case AIBehavior.Aggressive:
                case AIBehavior.Frenzied:
                    // Never flee
                    return false;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Perform an attack based on enemy type
        /// </summary>
        private void PerformAttack()
        {
            isAttacking = true;
            attackTimer = attackCooldown;
            
            // Play attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // Play attack sound
            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
            
            // Perform attack based on enemy type
            switch (enemyType)
            {
                case EnemyType.Melee:
                    StartCoroutine(PerformMeleeAttack());
                    break;
                    
                case EnemyType.Ranged:
                    PerformRangedAttack();
                    break;
                    
                case EnemyType.AOE:
                    PerformAOEAttack();
                    break;
                    
                case EnemyType.Caster:
                    StartCoroutine(PerformCastAttack());
                    break;
                    
                case EnemyType.SanityAttacker:
                    PerformSanityAttack();
                    break;
            }
        }
        
        /// <summary>
        /// Perform a melee attack
        /// </summary>
        private IEnumerator PerformMeleeAttack()
        {
            // Wait for attack animation to reach damage point
            yield return new WaitForSeconds(0.3f);
            
            // Check if player is still in range
            if (playerTarget != null && IsInAttackRange())
            {
                // Get the player's health system
                HealthSystem playerHealth = playerTarget.GetComponent<HealthSystem>();
                if (playerHealth != null)
                {
                    // Apply damage to player
                    playerHealth.ApplyDamage(damage, damageType, playerTarget.position, gameObject);
                }
            }
            
            // Show attack effect
            if (attackEffectPrefab != null)
            {
                Instantiate(attackEffectPrefab, transform.position + transform.forward * 1.5f, transform.rotation);
            }
            
            // Wait for attack animation to finish
            yield return new WaitForSeconds(0.5f);
            
            isAttacking = false;
        }
        
        /// <summary>
        /// Perform a ranged attack
        /// </summary>
        private void PerformRangedAttack()
        {
            if (projectilePrefab != null && projectileSpawnPoint != null && playerTarget != null)
            {
                // Create projectile
                GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                
                // Set up projectile
                ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                if (projectileController != null)
                {
                    // Calculate direction to player
                    Vector3 directionToPlayer = (playerTarget.position - projectileSpawnPoint.position).normalized;
                    
                    // Initialize projectile
                    projectileController.Initialize(damage, 10f, damageType, gameObject, directionToPlayer);
                }
            }
            
            // End attack immediately since projectile is fire and forget
            isAttacking = false;
        }
        
        /// <summary>
        /// Perform an area of effect attack
        /// </summary>
        private void PerformAOEAttack()
        {
            // Create explosion effect
            if (attackEffectPrefab != null)
            {
                Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Damage all players in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
            foreach (Collider hitCollider in hitColliders)
            {
                // Check if it's a player
                PlayerController player = hitCollider.GetComponent<PlayerController>();
                if (player != null)
                {
                    // Get the player's health system
                    HealthSystem playerHealth = player.GetComponent<HealthSystem>();
                    if (playerHealth != null)
                    {
                        // Apply damage to player
                        playerHealth.ApplyDamage(damage, damageType, player.transform.position, gameObject);
                    }
                }
            }
            
            // End attack immediately since AOE is instant
            isAttacking = false;
        }
        
        /// <summary>
        /// Perform a cast attack with charge-up time
        /// </summary>
        private IEnumerator PerformCastAttack()
        {
            // Show charging effect
            GameObject chargingEffect = null;
            if (attackEffectPrefab != null)
            {
                chargingEffect = Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);
                chargingEffect.transform.SetParent(transform);
            }
            
            // Wait for cast time
            yield return new WaitForSeconds(1f);
            
            // Destroy charging effect
            if (chargingEffect != null)
            {
                Destroy(chargingEffect);
            }
            
            // Perform actual attack (similar to ranged attack)
            if (projectilePrefab != null && projectileSpawnPoint != null && playerTarget != null)
            {
                // Create projectile
                GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                
                // Set up projectile
                ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                if (projectileController != null)
                {
                    // Calculate direction to player
                    Vector3 directionToPlayer = (playerTarget.position - projectileSpawnPoint.position).normalized;
                    
                    // Initialize projectile with increased damage due to cast time
                    projectileController.Initialize(damage * 1.5f, 8f, damageType, gameObject, directionToPlayer);
                }
            }
            
            // End attack
            isAttacking = false;
        }
        
        /// <summary>
        /// Perform a sanity attack that affects player's sanity
        /// </summary>
        private void PerformSanityAttack()
        {
            if (playerTarget != null)
            {
                // Get the player's sanity system
                SanitySystem playerSanity = playerTarget.GetComponent<SanitySystem>();
                if (playerSanity != null)
                {
                    // Apply sanity damage
                    playerSanity.ApplySanityDamage(damage * 0.5f);
                }
                
                // Also apply some regular damage
                HealthSystem playerHealth = playerTarget.GetComponent<HealthSystem>();
                if (playerHealth != null)
                {
                    playerHealth.ApplyDamage(damage * 0.3f, DamageType.Sanity, playerTarget.position, gameObject);
                }
            }
            
            // Show sanity attack effect
            if (attackEffectPrefab != null)
            {
                Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // End attack
            isAttacking = false;
        }
        
        /// <summary>
        /// Apply a stun effect to the enemy
        /// </summary>
        public void ApplyStun(float duration)
        {
            StartCoroutine(StunCoroutine(duration));
        }
        
        /// <summary>
        /// Coroutine to handle stunning
        /// </summary>
        private IEnumerator StunCoroutine(float duration)
        {
            isStunned = true;
            
            // Stop movement
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
            }
            
            // Update animator
            if (animator != null)
            {
                animator.SetBool("Stunned", true);
            }
            
            yield return new WaitForSeconds(duration);
            
            isStunned = false;
            
            // Resume movement
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
            }
            
            // Update animator
            if (animator != null)
            {
                animator.SetBool("Stunned", false);
            }
        }
        
        /// <summary>
        /// Initialize resistances based on enemy type
        /// </summary>
        private void InitializeResistances()
        {
            // Default resistances (1.0 = normal damage)
            foreach (ZodiacSign sign in System.Enum.GetValues(typeof(ZodiacSign)))
            {
                zodiacResistances[sign] = 1.0f;
            }
            
            // Apply specific resistances based on enemy type
            switch (enemyType)
            {
                case EnemyType.Melee:
                    zodiacResistances[ZodiacSign.Taurus] = 0.7f; // Resistant to Taurus (earth/physical)
                    zodiacResistances[ZodiacSign.Gemini] = 1.3f; // Weak to Gemini (air/mobility)
                    break;
                    
                case EnemyType.Ranged:
                    zodiacResistances[ZodiacSign.Sagittarius] = 0.7f; // Resistant to Sagittarius (range)
                    zodiacResistances[ZodiacSign.Aries] = 1.3f; // Weak to Aries (speed/aggression)
                    break;
                    
                case EnemyType.AOE:
                    zodiacResistances[ZodiacSign.Leo] = 0.7f; // Resistant to Leo (fire/aoe)
                    zodiacResistances[ZodiacSign.Scorpio] = 1.3f; // Weak to Scorpio (water/poison)
                    break;
                    
                case EnemyType.Caster:
                    zodiacResistances[ZodiacSign.Pisces] = 0.7f; // Resistant to Pisces (water/magic)
                    zodiacResistances[ZodiacSign.Virgo] = 1.3f; // Weak to Virgo (earth/purity)
                    break;
                    
                case EnemyType.SanityAttacker:
                    zodiacResistances[ZodiacSign.Aquarius] = 0.7f; // Resistant to Aquarius (air/mind)
                    zodiacResistances[ZodiacSign.Libra] = 1.3f; // Weak to Libra (air/balance)
                    break;
            }
        }
        
        /// <summary>
        /// Get the resistance multiplier for a specific zodiac sign
        /// </summary>
        public float GetResistanceForZodiacSign(ZodiacSign sign)
        {
            if (zodiacResistances.ContainsKey(sign))
            {
                return zodiacResistances[sign];
            }
            return 1.0f;
        }
        
        /// <summary>
        /// Update the health bar UI
        /// </summary>
        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthBarImage != null)
            {
                healthBarImage.fillAmount = currentHealth / maxHealth;
            }
        }
        
        /// <summary>
        /// Handle death
        /// </summary>
        private void Die()
        {
            // Play death sound
            if (audioSource != null && deathSound != null)
            {
                AudioSource.PlayClipAtPoint(deathSound, transform.position);
            }
            
            // Stop movement
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.enabled = false;
            }
            
            // Play death animation
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
            
            // Disable collider
            Collider enemyCollider = GetComponent<Collider>();
            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }
            
            // Disable this script
            enabled = false;
            
            // Drop loot
            DropLoot();
            
            // Destroy game object after delay
            Destroy(gameObject, 3f);
        }
        
        /// <summary>
        /// Drop loot when enemy dies
        /// </summary>
        private void DropLoot()
        {
            // Implement loot dropping logic here
            // For example, instantiate mote prefabs at enemy position
            
            // TODO: Replace with actual mote prefab and dropping logic
            int moteCount = Random.Range(moteDropMin, moteDropMax + 1);
            Debug.Log($"{enemyName} dropped {moteCount} motes");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Types of enemies
    /// </summary>
    public enum EnemyType
    {
        Melee,          // Close-range attackers
        Ranged,         // Shoot projectiles from a distance
        AOE,            // Area of effect attacks
        Caster,         // Cast spells with charge-up time
        SanityAttacker  // Attacks that primarily affect sanity
    }
    
    /// <summary>
    /// AI behaviors for enemies
    /// </summary>
    public enum AIBehavior
    {
        Aggressive,     // Always attack, never retreat
        Cautious,       // Retreat when low on health
        Cowardly,       // Retreat frequently and keep distance
        Frenzied        // More aggressive as health decreases
    }
    
    /// <summary>
    /// States for the enemy state machine
    /// </summary>
    public enum EnemyState
    {
        Idle,           // Not moving or doing anything
        Wander,         // Moving around randomly
        Chase,          // Pursuing the player
        Attack,         // Attacking the player
        Flee            // Running away from the player
    }
}