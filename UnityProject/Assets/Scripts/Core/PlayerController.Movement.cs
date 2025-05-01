using System.Collections;
using UnityEngine;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    // Partial class with movement-specific functionality for PlayerController
    public partial class PlayerController
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float strafeSpeed = 3f;
        [SerializeField] private float acceleration = 8f;
        [SerializeField] private float deceleration = 10f;
        [SerializeField] private bool useRootMotion = false;
        
        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float staminaRegenDelay = 1.5f;
        
        [Header("Dodge")]
        [SerializeField] private KeyCode dodgeKey = KeyCode.Space;
        [SerializeField] private bool useDodgeDoubleTap = true;
        [SerializeField] private float doubleTapWindow = 0.2f;
        [SerializeField] private LayerMask dodgeObstacleLayers;
        [SerializeField] private float capricornObstacleCostReduction = 1.0f; // For Capricorn zodiac
        
        [Header("Parry")]
        [SerializeField] private KeyCode parryKey = KeyCode.Q;
        [SerializeField] private bool requireDirectionalParry = false;
        [SerializeField] private float parryAngleThreshold = 45f; // For directional parry
        
        // Runtime variables
        private Vector2 _moveInput;
        private Vector2 _lastMoveInput;
        private Vector3 _moveVelocity = Vector3.zero;
        private float _currentStamina;
        private float _staminaRegenTimer;
        private Coroutine _dodgeCoroutine;
        private Coroutine _parryCoroutine;
        private bool _isParryActive;
        private bool _isInvulnerable;
        private float _dodgeCooldownTimer;
        private float _parryCooldownTimer;
        private float[] _doubleTapTimers = new float[4]; // For the four directions
        private int[] _doubleTapCounts = new int[4]; // Count of taps
        
        // Properties
        public Vector2 MoveInput => _moveInput;
        public Vector2 LastMoveDirection => _lastMoveDirection;
        public float CurrentStamina => _currentStamina;
        public float MaxStamina => maxStamina;
        public bool CanDodge => _dodgeCoroutine == null && _dodgeCooldownTimer <= 0 && !_isRooted;
        public bool CanParry => _parryCoroutine == null && _parryCooldownTimer <= 0 && !_isSilenced;
        public bool IsInvulnerable => _isInvulnerable;
        public bool IsParrying => _isParryActive;
        
        private void InitializeMovement()
        {
            _currentStamina = maxStamina;
            _staminaRegenTimer = 0f;
            _lastMoveDirection = Vector2.down; // Default facing down
        }
        
        // Handle movement input
        private void HandleMovementInput()
        {
            // Get input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            _moveInput = new Vector2(horizontal, vertical);
            
            // Normalize if moving diagonally
            if (_moveInput.magnitude > 1f)
            {
                _moveInput.Normalize();
            }
            
            // Check for dodge double-tap
            if (useDodgeDoubleTap)
            {
                CheckDoubleTap(horizontal, vertical);
            }
            
            // Check for dodge key
            if (Input.GetKeyDown(dodgeKey) && CanDodge)
            {
                TryDodge();
            }
            
            // Check for parry key
            if (Input.GetKeyDown(parryKey) && CanParry)
            {
                TryParry();
            }
            
            // Update cooldown timers
            UpdateCooldowns();
        }
        
        // Update movement
        private void UpdateMovement()
        {
            // Don't move if dodging or stunned
            if (_dodgeCoroutine != null || _isStunned || _isRooted)
                return;
                
            // Calculate target velocity
            Vector3 targetVelocity = new Vector3(_moveInput.x, 0, _moveInput.y) * moveSpeed;
            
            // Apply acceleration and deceleration
            if (_moveInput.magnitude > 0.1f)
            {
                // Accelerate towards target velocity
                _moveVelocity = Vector3.Lerp(_moveVelocity, targetVelocity, acceleration * Time.deltaTime);
                
                // Update last move direction if moving
                _lastMoveDirection = _moveInput;
            }
            else
            {
                // Decelerate to a stop
                _moveVelocity = Vector3.Lerp(_moveVelocity, Vector3.zero, deceleration * Time.deltaTime);
            }
            
            // Apply movement
            if (characterController != null)
            {
                characterController.Move(_moveVelocity * Time.deltaTime);
            }
            else
            {
                transform.position += _moveVelocity * Time.deltaTime;
            }
            
            // Handle rotation
            if (_moveVelocity.magnitude > 0.1f && !useRootMotion)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveVelocity.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        // Update cooldowns
        private void UpdateCooldowns()
        {
            // Update dodge cooldown
            if (_dodgeCooldownTimer > 0)
            {
                _dodgeCooldownTimer -= Time.deltaTime;
            }
            
            // Update parry cooldown
            if (_parryCooldownTimer > 0)
            {
                _parryCooldownTimer -= Time.deltaTime;
            }
            
            // Update dodge double tap timers
            for (int i = 0; i < _doubleTapTimers.Length; i++)
            {
                if (_doubleTapTimers[i] > 0)
                {
                    _doubleTapTimers[i] -= Time.deltaTime;
                    
                    if (_doubleTapTimers[i] <= 0)
                    {
                        _doubleTapCounts[i] = 0;
                    }
                }
            }
        }
        
        // Update stamina regeneration
        private void UpdateStamina()
        {
            // Start regenerating stamina after delay
            if (_staminaRegenTimer > 0)
            {
                _staminaRegenTimer -= Time.deltaTime;
            }
            else if (_currentStamina < maxStamina)
            {
                _currentStamina = Mathf.Min(_currentStamina + (staminaRegenRate * Time.deltaTime), maxStamina);
                UpdateStaminaBar();
            }
        }
        
        // Update the stamina UI
        private void UpdateStaminaBar()
        {
            // Implementation depends on UI system
            if (OnStaminaChanged != null)
            {
                OnStaminaChanged.Invoke(_currentStamina / maxStamina);
            }
        }
        
        // Consume stamina and reset regen timer
        public void ConsumeStamina(float amount)
        {
            _currentStamina = Mathf.Max(0, _currentStamina - amount);
            _staminaRegenTimer = staminaRegenDelay;
            UpdateStaminaBar();
        }
        
        // Restore stamina
        public void RestoreStamina(float amount)
        {
            _currentStamina = Mathf.Min(_currentStamina + amount, maxStamina);
            UpdateStaminaBar();
        }
        
        // Set max stamina with appropriate scaling of current stamina
        public void SetMaxStamina(float newMax)
        {
            float ratio = _currentStamina / maxStamina;
            maxStamina = newMax;
            _currentStamina = maxStamina * ratio;
            UpdateStaminaBar();
        }
        
        // Check for double tap dodge
        private void CheckDoubleTap(float horizontal, float vertical)
        {
            // Maps input to direction indices: 0=right, 1=left, 2=up, 3=down
            int dirIndex = -1;
            
            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
            {
                dirIndex = (horizontal > 0) ? 0 : 1;
            }
            else if (Mathf.Abs(vertical) > 0.1f)
            {
                dirIndex = (vertical > 0) ? 2 : 3;
            }
            
            // If we have a valid direction
            if (dirIndex >= 0)
            {
                // First tap or timer still active
                if (_doubleTapCounts[dirIndex] == 0 || _doubleTapTimers[dirIndex] > 0)
                {
                    _doubleTapCounts[dirIndex]++;
                    _doubleTapTimers[dirIndex] = doubleTapWindow;
                    
                    // If second tap within window
                    if (_doubleTapCounts[dirIndex] >= 2)
                    {
                        // Reset for next double tap
                        _doubleTapCounts[dirIndex] = 0;
                        _doubleTapTimers[dirIndex] = 0;
                        
                        // Try to dodge in the direction
                        Vector2 dodgeDir = Vector2.zero;
                        switch (dirIndex)
                        {
                            case 0: dodgeDir = Vector2.right; break;
                            case 1: dodgeDir = Vector2.left; break;
                            case 2: dodgeDir = Vector2.up; break;
                            case 3: dodgeDir = Vector2.down; break;
                        }
                        
                        TryDodge(dodgeDir);
                    }
                }
                // For other directions, reset their counters
                else for (int i = 0; i < _doubleTapCounts.Length; i++)
                {
                    if (i != dirIndex)
                    {
                        _doubleTapCounts[i] = 0;
                        _doubleTapTimers[i] = 0;
                    }
                }
            }
        }
        
        // Try to perform a dodge roll
        public void TryDodge(Vector2? direction = null)
        {
            if (!CanDodge) return;
            
            // Use provided direction or current input direction
            Vector2 dodgeDirection = direction ?? _moveInput;
            
            // If no direction provided and no current input, use last move direction
            if (dodgeDirection.magnitude < 0.1f)
            {
                dodgeDirection = _lastMoveDirection;
            }
            
            // Call the combat system to perform dodge
            if (CombatSystem.Instance != null)
            {
                CombatSystem.Instance.PerformDodge(this, dodgeDirection);
            }
        }
        
        // Start the dodge movement
        public void StartDodge(Vector2 direction, DodgeParameters parameters)
        {
            // Cancel any existing dodge
            if (_dodgeCoroutine != null)
            {
                StopCoroutine(_dodgeCoroutine);
            }
            
            // Start new dodge coroutine
            _dodgeCoroutine = StartCoroutine(DodgeCoroutine(direction, parameters));
            
            // Set cooldown
            _dodgeCooldownTimer = parameters.Cooldown;
        }
        
        // Dodge coroutine
        private IEnumerator DodgeCoroutine(Vector2 direction, DodgeParameters parameters)
        {
            float timer = 0f;
            Vector3 dodgeDir = new Vector3(direction.x, 0, direction.y).normalized;
            Vector3 startPos = transform.position;
            
            // Set dodge animation
            if (_animator != null)
            {
                _animator.SetTrigger("Dodge");
            }
            
            // Apply invincibility frames
            SetInvulnerable(true);
            
            // Check for obstacles if need be
            float totalDistance = parameters.Distance;
            if (dodgeObstacleLayers != 0)
            {
                // Cast ahead to check for obstacles
                if (Physics.SphereCast(transform.position, 0.5f, dodgeDir, out RaycastHit hit, totalDistance, dodgeObstacleLayers))
                {
                    // Adjust distance to stop before hitting the obstacle
                    totalDistance = hit.distance - 0.1f;
                    
                    // Check for Capricorn zodiac sign for zero stamina cost over obstacles
                    if (HasZodiacSignil("Capricorn"))
                    {
                        // Refund stamina cost - dodge becomes free
                        RestoreStamina(parameters.StaminaCost * capricornObstacleCostReduction);
                    }
                }
            }
            
            // Move along the dodge
            while (timer < parameters.Duration)
            {
                float t = timer / parameters.Duration;
                float speedFactor = parameters.SpeedCurve.Evaluate(t);
                
                // Move position
                Vector3 targetPos = startPos + (dodgeDir * totalDistance * speedFactor);
                
                // Use character controller if available
                if (characterController != null)
                {
                    Vector3 movement = (targetPos - transform.position);
                    characterController.Move(movement);
                }
                else
                {
                    transform.position = targetPos;
                }
                
                // When i-frames should end
                if (t >= parameters.InvincibilityFrames / parameters.Duration)
                {
                    SetInvulnerable(false);
                }
                
                timer += Time.deltaTime;
                yield return null;
            }
            
            // Ensure final position is correct
            Vector3 finalPos = startPos + (dodgeDir * totalDistance);
            
            // Use character controller if available
            if (characterController != null)
            {
                Vector3 movement = (finalPos - transform.position);
                characterController.Move(movement);
            }
            else
            {
                transform.position = finalPos;
            }
            
            // Ensure invulnerability is turned off
            SetInvulnerable(false);
            
            // Clear dodge coroutine reference
            _dodgeCoroutine = null;
        }
        
        // Try to perform a parry
        public void TryParry()
        {
            if (!CanParry) return;
            
            // Call the combat system to attempt parry
            if (CombatSystem.Instance != null)
            {
                CombatSystem.Instance.AttemptParry(this);
            }
        }
        
        // Start parry state
        public void StartParry(ParryParameters parameters)
        {
            // Cancel any existing parry
            if (_parryCoroutine != null)
            {
                StopCoroutine(_parryCoroutine);
            }
            
            // Start new parry coroutine
            _parryCoroutine = StartCoroutine(ParryCoroutine(parameters));
            
            // Set cooldown
            _parryCooldownTimer = parameters.ResetTime;
        }
        
        // Parry coroutine
        private IEnumerator ParryCoroutine(ParryParameters parameters)
        {
            // Start parry
            _isParryActive = true;
            
            // Set parry animation
            if (_animator != null)
            {
                _animator.SetTrigger("Parry");
            }
            
            // Wait for parry window duration
            yield return new WaitForSeconds(parameters.WindowDuration);
            
            // End parry window
            _isParryActive = false;
            
            // If no successful parry was triggered
            if (_parryCoroutine == this)
            {
                CombatSystem.Instance?.HandleParryResult(this, false);
            }
            
            // Clear parry coroutine reference
            _parryCoroutine = null;
        }
        
        // Check if an incoming attack can be parried based on angle
        public bool CanParryAttack(Vector3 attackDirection)
        {
            if (!_isParryActive) return false;
            
            if (requireDirectionalParry)
            {
                // Get player's forward direction
                Vector3 playerForward = transform.forward;
                
                // Calculate angle between attack and player's forward
                float angle = Vector3.Angle(-attackDirection, playerForward);
                
                // Check if within parry angle threshold
                return angle <= parryAngleThreshold;
            }
            
            // If directional parry not required, any attack is parryable during active window
            return true;
        }
        
        // Set invulnerability state
        public void SetInvulnerable(bool invulnerable)
        {
            _isInvulnerable = invulnerable;
            
            // Toggle invincibility effect if available
            if (invincibilityEffectPrefab != null)
            {
                // Implementation depends on the effect type
            }
        }
        
        // Modify move speed (used for slows, speed buffs, etc.)
        public void ModifySpeed(float amount)
        {
            moveSpeed += amount;
            
            // Ensure speed doesn't go below minimum
            if (moveSpeed < 1f)
            {
                moveSpeed = 1f;
            }
        }
    }
}