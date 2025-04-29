using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
    [CreateAssetMenu(fileName = "NewPlayerClass", menuName = "Codex/PlayerClass")]
    public class PlayerClass : ScriptableObject
    {
        [Header("Basic Info")]
        public string ClassName;
        public string Description;
        public Color ClassColor = Color.white;
        
        [Header("Starting Stats")]
        public float BaseHealth = 100.0f;
        public float BaseMana = 100.0f;
        public float BaseSpeed = 5.0f;
        public float BaseDefense = 0.0f;
        
        [Header("Health & Mana")]
        public float HealthRegen = 0.5f;
        public float ManaRegen = 5.0f;
        
        [Header("Combat")]
        public string StartingWeaponId;
        public List<string> StartingSpells = new List<string>();
        
        [Header("Unique Abilities")]
        public string PassiveAbility;
        public string UltimateAbility;
        public float UltimateCooldown = 60.0f;
        
        [Header("Visual")]
        public Sprite ClassIcon;
        public RuntimeAnimatorController AnimatorController;
        
        // Define character stat growth rates
        [Serializable]
        public class StatGrowth
        {
            public float HealthPerLevel = 10.0f;
            public float ManaPerLevel = 5.0f;
            public float DamagePerLevel = 2.0f;
            public float DefensePerLevel = 1.0f;
        }
        
        public StatGrowth Growth;
        
        // Special ability descriptors
        [Serializable]
        public class AbilityDescriptor
        {
            public string Name;
            public string Description;
            public Sprite Icon;
            public float Cooldown;
            public float ManaCost;
        }
        
        public List<AbilityDescriptor> Abilities = new List<AbilityDescriptor>();
        
        // Calculate stats for a given level
        public float GetMaxHealthForLevel(int level)
        {
            return BaseHealth + (Growth.HealthPerLevel * (level - 1));
        }
        
        public float GetMaxManaForLevel(int level)
        {
            return BaseMana + (Growth.ManaPerLevel * (level - 1));
        }
        
        public float GetDamageModifierForLevel(int level)
        {
            return Growth.DamagePerLevel * (level - 1);
        }
        
        public float GetDefenseForLevel(int level)
        {
            return BaseDefense + (Growth.DefensePerLevel * (level - 1));
        }
        
        // Apply class effects to a player
        public void ApplyClassEffects(Core.PlayerController player, int level = 1)
        {
            if (player == null) return;
            
            // Apply base stats
            player.SetMaxHealth(GetMaxHealthForLevel(level));
            player.SetMaxMana(GetMaxManaForLevel(level));
            player.SetMoveSpeed(BaseSpeed);
            player.SetHealthRegen(HealthRegen);
            player.SetManaRegen(ManaRegen);
            
            // Give starting weapon
            if (!string.IsNullOrEmpty(StartingWeaponId))
            {
                player.EquipWeapon(StartingWeaponId);
            }
            
            // Add starting spells
            foreach (var spell in StartingSpells)
            {
                player.AddAbility(spell);
            }
            
            // Apply passive ability
            if (!string.IsNullOrEmpty(PassiveAbility))
            {
                player.ApplyPassiveAbility(PassiveAbility);
            }
            
            // Set animator controller if available
            if (AnimatorController != null)
            {
                Animator animator = player.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.runtimeAnimatorController = AnimatorController;
                }
            }
            
            Debug.Log($"Applied {ClassName} class effects to player");
        }
    }
}