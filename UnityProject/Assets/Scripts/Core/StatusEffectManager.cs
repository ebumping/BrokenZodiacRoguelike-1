using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    // Manages status effects on an entity (player or enemy)
    public class StatusEffectManager : MonoBehaviour
    {
        // List of active status effects
        private List<ActiveStatusEffect> _activeEffects = new List<ActiveStatusEffect>();
        
        // Cache references
        private PlayerController _playerController;
        private EnemyController _enemyController;
        
        // Visual feedback
        private Transform _effectsParent;
        private Dictionary<StatusEffectType, GameObject> _effectVisuals = new Dictionary<StatusEffectType, GameObject>();
        
        private void Awake()
        {
            // Cache references
            _playerController = GetComponent<PlayerController>();
            _enemyController = GetComponent<EnemyController>();
            
            // Create parent for visual effects
            _effectsParent = new GameObject("StatusEffects").transform;
            _effectsParent.SetParent(transform);
            _effectsParent.localPosition = Vector3.zero;
        }
        
        private void Update()
        {
            // Update all active effects
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                
                // Update duration
                effect.RemainingDuration -= Time.deltaTime;
                
                // Update tick timer for periodic effects
                if (effect.TickTimer > 0)
                {
                    effect.TickTimer -= Time.deltaTime;
                    
                    if (effect.TickTimer <= 0)
                    {
                        // Apply tick effect
                        ApplyEffectTick(effect);
                        
                        // Reset tick timer
                        effect.TickTimer = effect.StatusEffect.TickRate;
                    }
                }
                
                // Remove expired effects
                if (effect.RemainingDuration <= 0)
                {
                    RemoveStatusEffect(effect.StatusEffect.Type);
                    i--; // Adjust index since we removed an item
                }
            }
        }
        
        // Apply a new status effect
        public void ApplyStatusEffect(StatusEffect statusEffect)
        {
            // Check if we already have this effect type
            int existingIndex = _activeEffects.FindIndex(e => e.StatusEffect.Type == statusEffect.Type);
            
            if (existingIndex >= 0)
            {
                // Handle stacking or refreshing
                var existingEffect = _activeEffects[existingIndex];
                
                // Take the higher power and longer duration
                existingEffect.StatusEffect.Power = Mathf.Max(existingEffect.StatusEffect.Power, statusEffect.Power);
                existingEffect.RemainingDuration = Mathf.Max(existingEffect.RemainingDuration, statusEffect.Duration);
                
                // Reset tick timer
                existingEffect.TickTimer = existingEffect.StatusEffect.TickRate;
            }
            else
            {
                // Add new effect
                var newEffect = new ActiveStatusEffect
                {
                    StatusEffect = statusEffect,
                    RemainingDuration = statusEffect.Duration,
                    TickTimer = statusEffect.TickRate
                };
                
                _activeEffects.Add(newEffect);
                
                // Apply initial effect
                ApplyEffectInitial(newEffect);
                
                // Create visual
                CreateEffectVisual(statusEffect);
            }
        }
        
        // Remove a status effect
        public void RemoveStatusEffect(StatusEffectType type)
        {
            // Find the effect
            int index = _activeEffects.FindIndex(e => e.StatusEffect.Type == type);
            
            if (index >= 0)
            {
                var effect = _activeEffects[index];
                
                // Cleanup effect
                RemoveEffectVisual(type);
                RemoveEffectModifiers(effect);
                
                // Remove from list
                _activeEffects.RemoveAt(index);
            }
        }
        
        // Clear all status effects
        public void ClearAllEffects()
        {
            foreach (var effect in _activeEffects)
            {
                RemoveEffectVisual(effect.StatusEffect.Type);
                RemoveEffectModifiers(effect);
            }
            
            _activeEffects.Clear();
        }
        
        // Check if entity has a specific status effect
        public bool HasStatusEffect(StatusEffectType type)
        {
            return _activeEffects.Exists(e => e.StatusEffect.Type == type);
        }
        
        // Get the remaining duration of a status effect
        public float GetStatusEffectDuration(StatusEffectType type)
        {
            var effect = _activeEffects.Find(e => e.StatusEffect.Type == type);
            return effect != null ? effect.RemainingDuration : 0f;
        }
        
        #region Effect Application
        // Apply initial effect when first applied
        private void ApplyEffectInitial(ActiveStatusEffect effect)
        {
            // Apply different effects based on the type
            switch (effect.StatusEffect.Type)
            {
                case StatusEffectType.Stun:
                    ApplyStun(true);
                    break;
                    
                case StatusEffectType.Root:
                    ApplyRoot(true);
                    break;
                    
                case StatusEffectType.Slow:
                    ApplySlow(effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Haste:
                    ApplyHaste(effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Shield:
                    ApplyShield(effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Silence:
                    ApplySilence(true);
                    break;
                    
                case StatusEffectType.Weakness:
                    ApplyWeakness(effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Amplify:
                    ApplyAmplify(effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Fear:
                    ApplyFear(true);
                    break;
                    
                case StatusEffectType.Charm:
                    ApplyCharm(true);
                    break;
                    
                case StatusEffectType.Reflect:
                    ApplyReflect(effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Confusion:
                    ApplyConfusion(true);
                    break;
                    
                case StatusEffectType.Invulnerable:
                    ApplyInvulnerability(true);
                    break;
                    
                // Damage over time effects are handled by ticks
                case StatusEffectType.Burn:
                case StatusEffectType.Freeze:
                case StatusEffectType.Shock:
                case StatusEffectType.Poison:
                case StatusEffectType.Bleed:
                case StatusEffectType.Regeneration:
                case StatusEffectType.ManaFlow:
                    // These are applied in ticks
                    break;
            }
        }
        
        // Apply periodic tick effect
        private void ApplyEffectTick(ActiveStatusEffect effect)
        {
            // Apply different effects based on the type
            switch (effect.StatusEffect.Type)
            {
                case StatusEffectType.Burn:
                    ApplyDamageOverTime(effect.StatusEffect.Power, "fire");
                    break;
                    
                case StatusEffectType.Freeze:
                    ApplyDamageOverTime(effect.StatusEffect.Power, "ice");
                    // Freezing also slows
                    ApplySlow(0.5f); // 50% slow
                    break;
                    
                case StatusEffectType.Shock:
                    ApplyDamageOverTime(effect.StatusEffect.Power, "lightning");
                    // Shock has a chance to stun briefly
                    if (Random.value < 0.2f) // 20% chance
                    {
                        ApplyTempStun(0.5f); // 0.5 second stun
                    }
                    break;
                    
                case StatusEffectType.Poison:
                    ApplyDamageOverTime(effect.StatusEffect.Power, "poison");
                    break;
                    
                case StatusEffectType.Bleed:
                    ApplyDamageOverTime(effect.StatusEffect.Power, "physical");
                    break;
                    
                case StatusEffectType.Regeneration:
                    ApplyHealOverTime(effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.ManaFlow:
                    ApplyManaOverTime(effect.StatusEffect.Power);
                    break;
            }
        }
        
        // Remove effect modifiers when expired
        private void RemoveEffectModifiers(ActiveStatusEffect effect)
        {
            // Undo different effects based on the type
            switch (effect.StatusEffect.Type)
            {
                case StatusEffectType.Stun:
                    ApplyStun(false);
                    break;
                    
                case StatusEffectType.Root:
                    ApplyRoot(false);
                    break;
                    
                case StatusEffectType.Slow:
                    // Undo slow effect - add speed back
                    ApplySlow(-effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Haste:
                    // Undo haste effect - remove extra speed
                    ApplyHaste(-effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Silence:
                    ApplySilence(false);
                    break;
                    
                case StatusEffectType.Weakness:
                    // Undo weakness - add damage back
                    ApplyWeakness(-effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Amplify:
                    // Undo amplify - reduce damage taken
                    ApplyAmplify(-effect.StatusEffect.Power);
                    break;
                    
                case StatusEffectType.Fear:
                    ApplyFear(false);
                    break;
                    
                case StatusEffectType.Charm:
                    ApplyCharm(false);
                    break;
                    
                case StatusEffectType.Reflect:
                    ApplyReflect(0f); // Disable reflection
                    break;
                    
                case StatusEffectType.Confusion:
                    ApplyConfusion(false);
                    break;
                    
                case StatusEffectType.Invulnerable:
                    ApplyInvulnerability(false);
                    break;
            }
        }
        #endregion
        
        #region Visual Effects
        // Create visual effect for a status
        private void CreateEffectVisual(StatusEffect statusEffect)
        {
            // If there's already a visual, remove it first
            RemoveEffectVisual(statusEffect.Type);
            
            // Create visual based on prefab if available
            if (statusEffect.EffectPrefab != null)
            {
                GameObject visualObj = Instantiate(statusEffect.EffectPrefab, _effectsParent);
                visualObj.transform.localPosition = Vector3.zero;
                _effectVisuals[statusEffect.Type] = visualObj;
            }
            else
            {
                // Create a default visual effect
                GameObject visualObj = CreateDefaultVisual(statusEffect);
                _effectVisuals[statusEffect.Type] = visualObj;
            }
        }
        
        // Create a default visual for status effects without custom prefabs
        private GameObject CreateDefaultVisual(StatusEffect statusEffect)
        {
            GameObject visualObj = new GameObject(statusEffect.Type.ToString() + "Effect");
            visualObj.transform.SetParent(_effectsParent);
            visualObj.transform.localPosition = Vector3.zero;
            
            // Create particle system
            ParticleSystem particles = visualObj.AddComponent<ParticleSystem>();
            
            // Configure particles based on effect type
            var main = particles.main;
            main.startColor = statusEffect.EffectColor;
            main.startSize = 0.3f;
            main.startLifetime = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Add a different shape or behavior based on effect type
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            
            // Duration based on effect
            particles.Stop();
            main.duration = statusEffect.Duration;
            main.loop = true;
            particles.Play();
            
            return visualObj;
        }
        
        // Remove visual effect
        private void RemoveEffectVisual(StatusEffectType type)
        {
            if (_effectVisuals.TryGetValue(type, out GameObject visualObj))
            {
                Destroy(visualObj);
                _effectVisuals.Remove(type);
            }
        }
        #endregion
        
        #region Effect Implementations
        private void ApplyDamageOverTime(float amount, string damageType)
        {
            if (_playerController != null)
            {
                _playerController.TakeDamage(amount);
            }
            else if (_enemyController != null)
            {
                _enemyController.TakeDamage(amount);
            }
        }
        
        private void ApplyHealOverTime(float amount)
        {
            if (_playerController != null)
            {
                _playerController.RestoreHealth(amount);
            }
            // Enemies typically don't heal, but could be implemented here
        }
        
        private void ApplyManaOverTime(float amount)
        {
            if (_playerController != null)
            {
                _playerController.RestoreMana(amount);
            }
            // Enemies don't use mana in this implementation
        }
        
        private void ApplyStun(bool isStunned)
        {
            if (_enemyController != null)
            {
                _enemyController.Stun(isStunned ? 999f : 0f); // Use a large value and handle duration elsewhere
            }
            // Players typically aren't stunned the same way
        }
        
        private void ApplyTempStun(float duration)
        {
            if (_enemyController != null)
            {
                _enemyController.Stun(duration);
            }
        }
        
        private void ApplyRoot(bool isRooted)
        {
            if (_enemyController != null)
            {
                // Root prevents movement but not attacks
                _enemyController.SetRooted(isRooted);
            }
            else if (_playerController != null)
            {
                // For players, this would affect their movement code
                _playerController.SetRooted(isRooted);
            }
        }
        
        private void ApplySlow(float slowAmount)
        {
            if (_enemyController != null)
            {
                // Reduce enemy speed
                _enemyController.ModifySpeed(-slowAmount); // Negative because it's a slow
            }
            else if (_playerController != null)
            {
                // For players
                _playerController.ModifySpeed(-slowAmount);
            }
        }
        
        private void ApplyHaste(float hasteAmount)
        {
            if (_playerController != null)
            {
                // Increase player speed
                _playerController.ModifySpeed(hasteAmount);
                // Also could affect attack/cast speed
                _playerController.ModifyCooldown(-hasteAmount * 0.1f); // 10% of haste applies to cooldowns
            }
            // Enemies typically don't get haste, but could be implemented here
        }
        
        private void ApplyShield(float shieldAmount)
        {
            if (_playerController != null)
            {
                _playerController.ApplyShield(shieldAmount);
            }
            // Enemies typically don't get shields
        }
        
        private void ApplySilence(bool isSilenced)
        {
            if (_playerController != null)
            {
                _playerController.SetSilenced(isSilenced);
            }
            // Most enemies don't cast spells, but could be extended for those that do
        }
        
        private void ApplyWeakness(float weaknessAmount)
        {
            if (_enemyController != null)
            {
                // Reduce enemy damage output
                _enemyController.ModifyDamage(-weaknessAmount); // Negative because it's a debuff
            }
            else if (_playerController != null)
            {
                // For players
                _playerController.ModifyDamage(-weaknessAmount);
            }
        }
        
        private void ApplyAmplify(float amplifyAmount)
        {
            // This increases damage taken - would be tracked and applied when receiving damage
            if (_enemyController != null)
            {
                _enemyController.SetDamageAmplification(amplifyAmount);
            }
            else if (_playerController != null)
            {
                _playerController.SetDamageAmplification(amplifyAmount);
            }
        }
        
        private void ApplyFear(bool isFeared)
        {
            if (_enemyController != null)
            {
                // Makes enemies run away
                _enemyController.SetFeared(isFeared);
            }
            // Players typically aren't feared in the same way
        }
        
        private void ApplyCharm(bool isCharmed)
        {
            if (_enemyController != null)
            {
                // Makes enemy temporarily fight for player
                _enemyController.SetCharmed(isCharmed);
            }
            // Players typically aren't charmed
        }
        
        private void ApplyReflect(float reflectChance)
        {
            if (_playerController != null)
            {
                _playerController.SetReflectionChance(reflectChance);
            }
            // Enemies typically don't reflect
        }
        
        private void ApplyConfusion(bool isConfused)
        {
            if (_enemyController != null)
            {
                // Makes enemy attack random targets
                _enemyController.SetConfused(isConfused);
            }
            // Players typically aren't confused in the same way
        }
        
        private void ApplyInvulnerability(bool isInvulnerable)
        {
            if (_playerController != null)
            {
                _playerController.SetInvulnerable(isInvulnerable);
            }
            else if (_enemyController != null)
            {
                _enemyController.SetInvulnerable(isInvulnerable);
            }
        }
        #endregion
    }
    
    // Data structure for active status effects
    [System.Serializable]
    public class ActiveStatusEffect
    {
        public StatusEffect StatusEffect;
        public float RemainingDuration;
        public float TickTimer;
    }
    
    // Status effect definition
    [System.Serializable]
    public class StatusEffect
    {
        public StatusEffectType Type;
        public float Power;
        public float Duration;
        public float TickRate = 1f;
        public Color EffectColor = Color.white;
        public GameObject EffectPrefab;
        public bool IsBeneficial;
        public Spell SourceSpell;
        public PlayerController Caster;
    }
}