using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Manages weapon functionality, attacks, and effects
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponData currentWeapon;
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private Transform projectileSpawnPoint;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        
        // Components
        private PlayerStats playerStats;
        private SanitySystem sanitySystem;
        
        // State
        private float attackCooldown = 0f;
        private bool isAttacking = false;
        
        // Weapon instances
        private GameObject currentWeaponInstance;
        
        private void Awake()
        {
            // Get required components
            playerStats = GetComponent<PlayerStats>();
            sanitySystem = GetComponent<SanitySystem>();
            
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            if (currentWeapon != null)
            {
                EquipWeapon(currentWeapon);
            }
        }
        
        private void Update()
        {
            // Update attack cooldown
            if (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime;
            }
        }
        
        /// <summary>
        /// Equip a new weapon
        /// </summary>
        public void EquipWeapon(WeaponData weapon)
        {
            // Destroy current weapon instance if exists
            if (currentWeaponInstance != null)
            {
                Destroy(currentWeaponInstance);
            }
            
            // Set new weapon data
            currentWeapon = weapon;
            
            // Create new weapon instance
            if (weaponHolder != null && weapon.weaponPrefab != null)
            {
                currentWeaponInstance = Instantiate(weapon.weaponPrefab, weaponHolder);
                currentWeaponInstance.transform.localPosition = Vector3.zero;
                currentWeaponInstance.transform.localRotation = Quaternion.identity;
            }
        }
        
        /// <summary>
        /// Perform an attack with the current weapon
        /// </summary>
        public void Attack()
        {
            if (attackCooldown <= 0 && currentWeapon != null)
            {
                // Start attack
                isAttacking = true;
                
                // Calculate attack speed with modifiers
                float attackSpeedModifier = 1f;
                if (playerStats != null)
                {
                    attackSpeedModifier = playerStats.GetAttackSpeedModifier();
                }
                
                // Set cooldown based on weapon attack speed and modifiers
                attackCooldown = currentWeapon.attackCooldown / attackSpeedModifier;
                
                // Handle different weapon types
                switch (currentWeapon.weaponType)
                {
                    case WeaponType.Melee:
                        PerformMeleeAttack();
                        break;
                        
                    case WeaponType.Ranged:
                        PerformRangedAttack();
                        break;
                        
                    case WeaponType.Magic:
                        PerformMagicAttack();
                        break;
                }
                
                // Play attack sound
                if (audioSource != null && currentWeapon.attackSound != null)
                {
                    audioSource.PlayOneShot(currentWeapon.attackSound);
                }
                
                // End attack after animation
                StartCoroutine(EndAttack());
            }
        }
        
        /// <summary>
        /// Perform a melee attack
        /// </summary>
        private void PerformMeleeAttack()
        {
            // Calculate attack range and angle
            float attackRange = currentWeapon.range;
            float attackAngle = currentWeapon.attackArc;
            
            // Calculate attack damage with modifiers
            float damage = currentWeapon.damage;
            if (playerStats != null)
            {
                damage *= playerStats.GetAttackPower() / 10f;
            }
            
            // Apply sanity effects to damage
            if (sanitySystem != null)
            {
                damage *= sanitySystem.GetDamageMultiplier();
            }
            
            // Get all colliders in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
            
            foreach (Collider hitCollider in hitColliders)
            {
                // Skip self
                if (hitCollider.transform == transform)
                    continue;
                
                // Check if in attack arc
                Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
                
                if (angleToTarget <= attackAngle * 0.5f)
                {
                    // Apply damage to target
                    HealthSystem targetHealth = hitCollider.GetComponent<HealthSystem>();
                    if (targetHealth != null)
                    {
                        // Calculate hit position
                        Vector3 hitPosition = hitCollider.ClosestPoint(transform.position);
                        
                        // Apply damage
                        targetHealth.ApplyDamage(damage, currentWeapon.damageType, hitPosition, gameObject);
                    }
                }
            }
            
            // Show attack effect
            if (currentWeapon.attackEffectPrefab != null)
            {
                GameObject effect = Instantiate(currentWeapon.attackEffectPrefab, transform.position, transform.rotation);
                Destroy(effect, 2f);
            }
        }
        
        /// <summary>
        /// Perform a ranged attack
        /// </summary>
        private void PerformRangedAttack()
        {
            if (projectileSpawnPoint == null)
            {
                projectileSpawnPoint = transform;
            }
            
            // Calculate attack damage with modifiers
            float damage = currentWeapon.damage;
            if (playerStats != null)
            {
                damage *= playerStats.GetAttackPower() / 10f;
            }
            
            // Apply sanity effects to damage
            if (sanitySystem != null)
            {
                damage *= sanitySystem.GetDamageMultiplier();
            }
            
            // Create projectile
            if (currentWeapon.projectilePrefab != null)
            {
                GameObject projectile = Instantiate(currentWeapon.projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                
                // Set up projectile
                ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                if (projectileController != null)
                {
                    projectileController.Initialize(damage, currentWeapon.projectileSpeed, currentWeapon.damageType, gameObject);
                }
            }
        }
        
        /// <summary>
        /// Perform a magic attack
        /// </summary>
        private void PerformMagicAttack()
        {
            // Magic attacks are similar to ranged but with different effects
            PerformRangedAttack();
            
            // Add additional magic effects
            if (currentWeapon.attackEffectPrefab != null)
            {
                GameObject effect = Instantiate(currentWeapon.attackEffectPrefab, transform.position, transform.rotation);
                Destroy(effect, 2f);
            }
        }
        
        /// <summary>
        /// End the attack after animation
        /// </summary>
        private IEnumerator EndAttack()
        {
            // Wait for attack animation
            yield return new WaitForSeconds(currentWeapon.attackDuration);
            
            isAttacking = false;
        }
        
        /// <summary>
        /// Get the current weapon data
        /// </summary>
        public WeaponData GetCurrentWeapon()
        {
            return currentWeapon;
        }
        
        /// <summary>
        /// Check if currently attacking
        /// </summary>
        public bool IsAttacking()
        {
            return isAttacking;
        }
    }
    
    /// <summary>
    /// Types of weapons
    /// </summary>
    public enum WeaponType
    {
        Melee,
        Ranged,
        Magic
    }
    
    /// <summary>
    /// Data definition for a weapon
    /// </summary>
    [System.Serializable]
    public class WeaponData
    {
        public string weaponName;
        public WeaponType weaponType;
        public GameObject weaponPrefab;
        public GameObject projectilePrefab;
        public GameObject attackEffectPrefab;
        public AudioClip attackSound;
        public float damage = 10f;
        public float attackCooldown = 1f;
        public float attackDuration = 0.5f;
        public float range = 2f;
        public float attackArc = 90f;
        public float projectileSpeed = 10f;
        public DamageType damageType = DamageType.Physical;
        public string description;
        public Sprite icon;
    }
}