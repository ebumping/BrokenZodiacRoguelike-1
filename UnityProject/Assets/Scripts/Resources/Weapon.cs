using System;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Codex/Weapon")]
    public class Weapon : ScriptableObject
    {
        [Header("Basic Info")]
        public string WeaponName;
        public string Description;
        
        [Header("Combat Stats")]
        public float Damage = 10.0f;
        public float FireRate = 0.5f; // Time between shots
        public float ProjectileSpeed = 20.0f;
        public float CriticalChance = 0.05f;
        public float CriticalMultiplier = 2.0f;
        public float Range = 15.0f;
        
        [Header("References")]
        public GameObject ProjectilePrefab;
        public Sprite WeaponSprite;
        public AudioClip FireSound;
        public AudioClip ReloadSound;
        
        [Header("Special")]
        public bool HasSpecialAbility = false;
        public string SpecialAbilityName;
        public float SpecialCooldown = 10.0f;
        
        [Header("Effects")]
        public GameObject MuzzleFlashPrefab;
        public GameObject ImpactEffectPrefab;
        
        // Base modifiers
        private float _damageModifier = 0.0f;
        private float _fireRateModifier = 0.0f;
        private float _criticalChanceModifier = 0.0f;
        
        // Get the current total damage (base + modifier)
        public float GetDamage()
        {
            return Damage + _damageModifier;
        }
        
        // Get the current fire rate (base + modifier)
        public float GetFireRate()
        {
            return Mathf.Max(0.05f, FireRate + _fireRateModifier);
        }
        
        // Get current critical chance (base + modifier)
        public float GetCriticalChance()
        {
            return Mathf.Clamp(CriticalChance + _criticalChanceModifier, 0f, 1f);
        }
        
        // Modifier methods
        public void ModifyDamage(float amount)
        {
            _damageModifier += amount;
            Debug.Log($"{WeaponName} damage modified by {amount}. New damage: {GetDamage()}");
        }
        
        public void ModifyFireRate(float amount) // Negative values make firing faster
        {
            _fireRateModifier += amount;
            Debug.Log($"{WeaponName} fire rate modified by {amount}. New rate: {GetFireRate()}");
        }
        
        public void ModifyCriticalChance(float amount)
        {
            _criticalChanceModifier += amount;
            Debug.Log($"{WeaponName} critical chance modified by {amount}. New chance: {GetCriticalChance()}");
        }
        
        // Reset modifiers
        public void ResetModifiers()
        {
            _damageModifier = 0.0f;
            _fireRateModifier = 0.0f;
            _criticalChanceModifier = 0.0f;
        }
        
        // Try to get a critical hit
        public bool RollForCritical()
        {
            return UnityEngine.Random.value < GetCriticalChance();
        }
        
        // Calculate damage with critical
        public float CalculateDamage()
        {
            if (RollForCritical())
            {
                return GetDamage() * CriticalMultiplier;
            }
            return GetDamage();
        }
    }
}