using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Main player controller handling movement, input, and general player functionality
    /// This is marked as partial to allow for separation of concerns across multiple files
    /// </summary>
    public partial class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float rotationSpeed = 10f;
        
        [Header("References")]
        [SerializeField] private Transform playerModel;
        [SerializeField] private Animator animator;
        
        // Components
        private PlayerStats playerStats;
        private WeaponSystem weaponSystem;
        private SanitySystem sanitySystem;
        private HealthSystem healthSystem;
        
        // Input and movement
        private Vector2 moveInput;
        private Vector2 aimInput;
        private bool isSprinting;
        private Vector3 moveDirection;
        
        // State
        private bool isMoving;
        private bool isAttacking;
        private bool isRolling;
        private bool isStunned;
        
        private void Awake()
        {
            // Get required components
            playerStats = GetComponent<PlayerStats>();
            weaponSystem = GetComponent<WeaponSystem>();
            sanitySystem = GetComponent<SanitySystem>();
            healthSystem = GetComponent<HealthSystem>();
            
            if (!playerStats || !weaponSystem || !sanitySystem || !healthSystem)
            {
                Debug.LogError("Missing required components on PlayerController!");
            }
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (isStunned) return;
            
            HandleMovement();
            HandleRotation();
            UpdateAnimations();
        }
        
        /// <summary>
        /// Initialize the player controller and all its systems
        /// </summary>
        private void Initialize()
        {
            // Any initialization code here
        }
        
        /// <summary>
        /// Handle player movement based on input
        /// </summary>
        private void HandleMovement()
        {
            if (isRolling) return;
            
            // Calculate movement direction from input
            moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            
            if (moveDirection.magnitude > 0.1f)
            {
                // Apply movement
                float currentSpeed = moveSpeed;
                if (isSprinting)
                {
                    currentSpeed *= sprintMultiplier;
                }
                
                // Apply any modifiers from sanity or zodiac abilities
                currentSpeed *= playerStats.GetMovementSpeedModifier();
                
                // Move the player
                transform.position += moveDirection * currentSpeed * Time.deltaTime;
                
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
        }
        
        /// <summary>
        /// Handle player rotation based on aim input or movement direction
        /// </summary>
        private void HandleRotation()
        {
            if (isRolling) return;
            
            // Determine rotation target based on input type
            Vector3 targetDirection;
            
            // If using controller for aiming
            if (aimInput.magnitude > 0.1f)
            {
                targetDirection = new Vector3(aimInput.x, 0f, aimInput.y).normalized;
            }
            // Otherwise use movement direction
            else if (moveDirection.magnitude > 0.1f)
            {
                targetDirection = moveDirection;
            }
            else
            {
                return; // No rotation needed
            }
            
            // Apply rotation
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        /// <summary>
        /// Update animation parameters based on player state
        /// </summary>
        private void UpdateAnimations()
        {
            if (animator)
            {
                animator.SetBool("IsMoving", isMoving);
                animator.SetBool("IsSprinting", isSprinting && isMoving);
                animator.SetBool("IsAttacking", isAttacking);
            }
        }
        
        #region Input Handlers
        
        /// <summary>
        /// Handle move input from the Input System
        /// </summary>
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }
        
        /// <summary>
        /// Handle aim input from the Input System
        /// </summary>
        public void OnAim(InputValue value)
        {
            aimInput = value.Get<Vector2>();
        }
        
        /// <summary>
        /// Handle sprint input from the Input System
        /// </summary>
        public void OnSprint(InputValue value)
        {
            isSprinting = value.isPressed;
        }
        
        /// <summary>
        /// Handle attack input from the Input System
        /// </summary>
        public void OnAttack(InputValue value)
        {
            if (value.isPressed && !isAttacking && !isRolling && !isStunned)
            {
                StartCoroutine(PerformAttack());
            }
        }
        
        /// <summary>
        /// Handle dodge/roll input from the Input System
        /// </summary>
        public void OnDodge(InputValue value)
        {
            if (value.isPressed && !isRolling && !isStunned)
            {
                StartCoroutine(PerformRoll());
            }
        }
        
        /// <summary>
        /// Handle interaction input from the Input System
        /// </summary>
        public void OnInteract(InputValue value)
        {
            if (value.isPressed && !isStunned)
            {
                TryInteract();
            }
        }
        
        /// <summary>
        /// Handle ability activation input from the Input System
        /// </summary>
        public void OnAbility(InputValue value)
        {
            if (value.isPressed && !isStunned)
            {
                TryActivateAbility();
            }
        }
        
        #endregion
        
        #region Actions
        
        /// <summary>
        /// Perform attack action
        /// </summary>
        private IEnumerator PerformAttack()
        {
            isAttacking = true;
            
            // Animation and weapon logic would go here
            if (weaponSystem)
            {
                weaponSystem.Attack();
            }
            
            // Wait for attack animation/cooldown
            yield return new WaitForSeconds(0.5f); // Replace with actual attack duration
            
            isAttacking = false;
        }
        
        /// <summary>
        /// Perform dodge roll action
        /// </summary>
        private IEnumerator PerformRoll()
        {
            isRolling = true;
            
            // Get roll direction
            Vector3 rollDirection = moveDirection.magnitude > 0.1f ? moveDirection : playerModel.forward;
            
            // Animation and roll movement logic
            float rollDuration = 0.5f;
            float rollDistance = 5f;
            float rollSpeed = rollDistance / rollDuration;
            
            if (animator)
            {
                animator.SetTrigger("Roll");
            }
            
            // Apply any i-frames or damage reduction
            healthSystem.SetInvulnerable(true);
            
            // Perform the roll movement
            float elapsed = 0f;
            while (elapsed < rollDuration)
            {
                transform.position += rollDirection * rollSpeed * Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            healthSystem.SetInvulnerable(false);
            isRolling = false;
        }
        
        /// <summary>
        /// Try to interact with nearby objects
        /// </summary>
        private void TryInteract()
        {
            // Raycast or sphere cast to find interactable objects
            Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
            
            foreach (Collider collider in colliders)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Try to activate zodiac ability based on current sanity state
        /// </summary>
        private void TryActivateAbility()
        {
            if (sanitySystem)
            {
                sanitySystem.ActivateZodiacAbility();
            }
        }
        
        /// <summary>
        /// Apply a stun effect to the player
        /// </summary>
        public void ApplyStun(float duration)
        {
            StartCoroutine(StunCoroutine(duration));
        }
        
        /// <summary>
        /// Coroutine to handle player stunning
        /// </summary>
        private IEnumerator StunCoroutine(float duration)
        {
            isStunned = true;
            
            if (animator)
            {
                animator.SetBool("IsStunned", true);
            }
            
            yield return new WaitForSeconds(duration);
            
            isStunned = false;
            
            if (animator)
            {
                animator.SetBool("IsStunned", false);
            }
        }
        
        #endregion
        
        #region Interface Implementations
        
        // This region is for interface implementations that would be defined in other partial class files
        // Example: IDamageable, IHealable, etc.
        
        #endregion
    }
    
    /// <summary>
    /// Interface for interactable objects in the game world
    /// </summary>
    public interface IInteractable
    {
        void Interact(PlayerController player);
    }
}