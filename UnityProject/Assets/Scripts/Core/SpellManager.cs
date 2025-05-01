using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    public class SpellManager : MonoBehaviour
    {
        // Singleton instance
        public static SpellManager Instance { get; private set; }
        
        [Header("Global Spell Settings")]
        [SerializeField] private float globalCooldownTime = 0.5f;
        [SerializeField] private float spellRangeMultiplier = 1.0f;
        [SerializeField] private float spellDamageMultiplier = 1.0f;
        [SerializeField] private float spellHealingMultiplier = 1.0f;
        [SerializeField] private float spellDurationMultiplier = 1.0f;
        [SerializeField] private float spellAreaMultiplier = 1.0f;
        [SerializeField] private float spellManaCostMultiplier = 1.0f;
        
        [Header("References")]
        [SerializeField] private List<Spell> availableSpells = new List<Spell>();
        [SerializeField] private GameObject spellImpactVFXParent;
        
        // Player-specific spell data
        private Dictionary<int, PlayerSpellData> _playerSpellData = new Dictionary<int, PlayerSpellData>();
        
        // Cooldown tracking
        private Dictionary<int, float> _globalCooldowns = new Dictionary<int, float>();
        
        // Currently active spells in the world
        private List<ActiveSpellInstance> _activeSpells = new List<ActiveSpellInstance>();
        
        private void Awake()
        {
            // Setup singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Create impact VFX parent if it doesn't exist
            if (spellImpactVFXParent == null)
            {
                spellImpactVFXParent = new GameObject("SpellImpactVFX");
                DontDestroyOnLoad(spellImpactVFXParent);
            }
        }
        
        private void Update()
        {
            // Update cooldowns
            List<int> playerIds = new List<int>(_globalCooldowns.Keys);
            foreach (int playerId in playerIds)
            {
                if (_globalCooldowns[playerId] > 0)
                {
                    _globalCooldowns[playerId] -= Time.deltaTime;
                }
                
                if (_playerSpellData.ContainsKey(playerId))
                {
                    _playerSpellData[playerId].UpdateCooldowns(Time.deltaTime);
                }
            }
            
            // Update active spells
            for (int i = _activeSpells.Count - 1; i >= 0; i--)
            {
                _activeSpells[i].UpdateSpell(Time.deltaTime);
                
                if (_activeSpells[i].IsFinished)
                {
                    _activeSpells[i].CleanUp();
                    _activeSpells.RemoveAt(i);
                }
            }
        }
        
        // Register a player with the spell system
        public void RegisterPlayer(int playerId, PlayerController playerController)
        {
            if (!_playerSpellData.ContainsKey(playerId))
            {
                _playerSpellData[playerId] = new PlayerSpellData(playerId, playerController);
                _globalCooldowns[playerId] = 0f;
            }
        }
        
        // Unregister a player
        public void UnregisterPlayer(int playerId)
        {
            if (_playerSpellData.ContainsKey(playerId))
            {
                _playerSpellData.Remove(playerId);
                _globalCooldowns.Remove(playerId);
            }
        }
        
        // Add a spell to a player's spellbook
        public void AddSpellToPlayer(int playerId, Spell spell, int level = 1)
        {
            if (_playerSpellData.ContainsKey(playerId))
            {
                _playerSpellData[playerId].AddSpell(spell, level);
                Debug.Log($"Added spell {spell.SpellName} (Lvl {level}) to player {playerId}");
            }
        }
        
        // Remove a spell from a player's spellbook
        public void RemoveSpellFromPlayer(int playerId, Spell spell)
        {
            if (_playerSpellData.ContainsKey(playerId))
            {
                _playerSpellData[playerId].RemoveSpell(spell);
            }
        }
        
        // Get all spells for a specific player
        public List<PlayerSpellInfo> GetPlayerSpells(int playerId)
        {
            if (_playerSpellData.ContainsKey(playerId))
            {
                return _playerSpellData[playerId].GetSpells();
            }
            return new List<PlayerSpellInfo>();
        }
        
        // Check if a spell is on cooldown
        public float GetSpellCooldown(int playerId, Spell spell)
        {
            if (_playerSpellData.ContainsKey(playerId))
            {
                return _playerSpellData[playerId].GetCooldown(spell);
            }
            return 0f;
        }
        
        // Cast a spell
        public bool CastSpell(int playerId, Spell spell, Vector3 targetPosition, GameObject targetObject = null)
        {
            // Check if the player exists
            if (!_playerSpellData.ContainsKey(playerId))
            {
                Debug.LogWarning($"Player {playerId} not registered with SpellManager");
                return false;
            }
            
            PlayerSpellData playerData = _playerSpellData[playerId];
            PlayerController caster = playerData.PlayerController;
            
            if (caster == null)
            {
                Debug.LogWarning($"Player controller for {playerId} is null");
                return false;
            }
            
            // Check if spell is known to the player
            if (!playerData.HasSpell(spell))
            {
                Debug.LogWarning($"Player {playerId} doesn't know spell {spell.SpellName}");
                return false;
            }
            
            // Get spell level for this player
            int spellLevel = playerData.GetSpellLevel(spell);
            
            // Check cooldown
            float remainingCooldown = playerData.GetCooldown(spell);
            if (remainingCooldown > 0f)
            {
                Debug.LogWarning($"Spell {spell.SpellName} is on cooldown for {remainingCooldown:F1} seconds");
                return false;
            }
            
            // Check global cooldown
            if (_globalCooldowns[playerId] > 0f)
            {
                Debug.LogWarning($"Player {playerId} is on global cooldown for {_globalCooldowns[playerId]:F1} seconds");
                return false;
            }
            
            // Check mana cost
            float manaCost = spell.GetManaCost(spellLevel, spellManaCostMultiplier);
            if (caster.CurrentMana < manaCost)
            {
                Debug.LogWarning($"Not enough mana to cast {spell.SpellName}. Required: {manaCost}, Have: {caster.CurrentMana}");
                return false;
            }
            
            // All checks passed, let's cast the spell!
            
            // Consume mana
            caster.ConsumeMana(manaCost);
            
            // Set cooldown
            playerData.SetCooldown(spell, spell.Cooldown);
            
            // Set global cooldown
            _globalCooldowns[playerId] = globalCooldownTime;
            
            // Create spell instance
            InstantiateSpell(spell, caster, targetPosition, targetObject, spellLevel);
            
            // Play cast animation on the player
            caster.PlayCastAnimation();
            
            // Play cast sound
            if (spell.CastSound != null)
            {
                AudioSource.PlayClipAtPoint(spell.CastSound, caster.transform.position);
            }
            
            Debug.Log($"Player {playerId} cast {spell.SpellName} (Lvl {spellLevel})");
            return true;
        }
        
        // Instantiate the actual spell in the world
        private void InstantiateSpell(Spell spell, PlayerController caster, Vector3 targetPosition, GameObject targetObject, int spellLevel)
        {
            if (spell.SpellPrefab == null)
            {
                Debug.LogWarning($"Spell {spell.SpellName} has no prefab");
                return;
            }
            
            // Calculate spawn position
            Transform spawnTransform = caster.ProjectileSpawnPoint != null ? 
                caster.ProjectileSpawnPoint : caster.transform;
            
            // Instantiate the spell prefab
            GameObject spellObject = Instantiate(spell.SpellPrefab, 
                spawnTransform.position, 
                Quaternion.identity);
            
            // Get or add spell behavior component based on spell type
            SpellBehavior spellBehavior = spellObject.GetComponent<SpellBehavior>();
            if (spellBehavior == null)
            {
                // Add appropriate behavior based on spell type
                switch (spell.Type)
                {
                    case SpellType.Projectile:
                        spellBehavior = spellObject.AddComponent<ProjectileSpellBehavior>();
                        break;
                    case SpellType.Beam:
                        spellBehavior = spellObject.AddComponent<BeamSpellBehavior>();
                        break;
                    case SpellType.Area:
                        spellBehavior = spellObject.AddComponent<AreaSpellBehavior>();
                        break;
                    // Add other spell types as needed
                    default:
                        spellBehavior = spellObject.AddComponent<DefaultSpellBehavior>();
                        break;
                }
            }
            
            // Setup the spell behavior
            spellBehavior.Initialize(spell, caster, targetPosition, targetObject, spellLevel, 
                new SpellModifiers
                {
                    DamageMultiplier = spellDamageMultiplier,
                    HealingMultiplier = spellHealingMultiplier,
                    DurationMultiplier = spellDurationMultiplier,
                    AreaMultiplier = spellAreaMultiplier,
                    RangeMultiplier = spellRangeMultiplier
                });
            
            // Add to active spells list
            _activeSpells.Add(new ActiveSpellInstance
            {
                SpellObject = spellObject,
                SpellBehavior = spellBehavior,
                Caster = caster,
                Spell = spell,
                Level = spellLevel,
                StartTime = Time.time
            });
        }
        
        // Create a visual effect at the impact location
        public void CreateImpactEffect(Spell spell, Vector3 position, Quaternion rotation)
        {
            if (spell.ImpactEffectPrefab != null)
            {
                GameObject impactEffect = Instantiate(spell.ImpactEffectPrefab, 
                    position, 
                    rotation, 
                    spellImpactVFXParent.transform);
                
                // Automatically destroy after a time
                Destroy(impactEffect, 5f);
                
                // Play impact sound if available
                if (spell.ImpactSound != null)
                {
                    AudioSource.PlayClipAtPoint(spell.ImpactSound, position);
                }
            }
        }
        
        // Apply a spell effect to a target
        public void ApplySpellEffect(SpellEffect effect, GameObject target)
        {
            if (target == null) return;
            
            // Check if it's an enemy
            EnemyController enemy = target.GetComponent<EnemyController>();
            if (enemy != null && effect.Damage > 0)
            {
                enemy.TakeDamage(effect.Damage);
                
                // Apply status effects
                foreach (var statusEffect in effect.StatusEffects)
                {
                    ApplyStatusEffectToEnemy(enemy, statusEffect);
                }
            }
            
            // Check if it's a player
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                // If it's a harmful spell from an enemy
                if (effect.Caster != player && effect.Damage > 0)
                {
                    player.TakeDamage(effect.Damage);
                }
                
                // If it's a healing spell
                if (effect.Healing > 0)
                {
                    player.RestoreHealth(effect.Healing);
                }
                
                // Apply status effects based on friendly/enemy
                foreach (var statusEffect in effect.StatusEffects)
                {
                    if (statusEffect.IsBeneficial)
                    {
                        // Apply buffs only to allies
                        if (effect.Caster == player || effect.IsFriendly)
                        {
                            ApplyStatusEffectToPlayer(player, statusEffect);
                        }
                    }
                    else
                    {
                        // Apply debuffs only to enemies
                        if (effect.Caster != player && !effect.IsFriendly)
                        {
                            ApplyStatusEffectToPlayer(player, statusEffect);
                        }
                    }
                }
            }
        }
        
        // Apply a status effect to an enemy
        private void ApplyStatusEffectToEnemy(EnemyController enemy, StatusEffect statusEffect)
        {
            // Check if the enemy already has a status effect manager
            StatusEffectManager effectManager = enemy.GetComponent<StatusEffectManager>();
            if (effectManager == null)
            {
                effectManager = enemy.gameObject.AddComponent<StatusEffectManager>();
            }
            
            effectManager.ApplyStatusEffect(statusEffect);
        }
        
        // Apply a status effect to a player
        private void ApplyStatusEffectToPlayer(PlayerController player, StatusEffect statusEffect)
        {
            // Check if the player already has a status effect manager
            StatusEffectManager effectManager = player.GetComponent<StatusEffectManager>();
            if (effectManager == null)
            {
                effectManager = player.gameObject.AddComponent<StatusEffectManager>();
            }
            
            effectManager.ApplyStatusEffect(statusEffect);
        }
        
        // Find a spell by name
        public Spell GetSpellByName(string spellName)
        {
            foreach (var spell in availableSpells)
            {
                if (spell.SpellName == spellName)
                {
                    return spell;
                }
            }
            
            Debug.LogWarning($"Spell with name {spellName} not found");
            return null;
        }
        
        // Modify global spell settings
        public void ModifySpellDamage(float modifier)
        {
            spellDamageMultiplier *= modifier;
        }
        
        public void ModifySpellHealing(float modifier)
        {
            spellHealingMultiplier *= modifier;
        }
        
        public void ModifySpellDuration(float modifier)
        {
            spellDurationMultiplier *= modifier;
        }
        
        public void ModifySpellArea(float modifier)
        {
            spellAreaMultiplier *= modifier;
        }
        
        public void ModifySpellRange(float modifier)
        {
            spellRangeMultiplier *= modifier;
        }
        
        public void ModifySpellManaCost(float modifier)
        {
            spellManaCostMultiplier *= modifier;
        }
    }
    
    // Class to track a player's spells and their cooldowns
    public class PlayerSpellData
    {
        public int PlayerId { get; private set; }
        public PlayerController PlayerController { get; private set; }
        
        private Dictionary<Spell, PlayerSpellInfo> _spells = new Dictionary<Spell, PlayerSpellInfo>();
        
        public PlayerSpellData(int playerId, PlayerController controller)
        {
            PlayerId = playerId;
            PlayerController = controller;
        }
        
        public void AddSpell(Spell spell, int level)
        {
            if (_spells.ContainsKey(spell))
            {
                _spells[spell].Level = level;
            }
            else
            {
                _spells[spell] = new PlayerSpellInfo
                {
                    Spell = spell,
                    Level = level,
                    Cooldown = 0f
                };
            }
        }
        
        public void RemoveSpell(Spell spell)
        {
            if (_spells.ContainsKey(spell))
            {
                _spells.Remove(spell);
            }
        }
        
        public bool HasSpell(Spell spell)
        {
            return _spells.ContainsKey(spell);
        }
        
        public int GetSpellLevel(Spell spell)
        {
            if (_spells.ContainsKey(spell))
            {
                return _spells[spell].Level;
            }
            return 0;
        }
        
        public float GetCooldown(Spell spell)
        {
            if (_spells.ContainsKey(spell))
            {
                return _spells[spell].Cooldown;
            }
            return 0f;
        }
        
        public void SetCooldown(Spell spell, float cooldown)
        {
            if (_spells.ContainsKey(spell))
            {
                _spells[spell].Cooldown = cooldown;
            }
        }
        
        public void UpdateCooldowns(float deltaTime)
        {
            foreach (var spellInfo in _spells.Values)
            {
                if (spellInfo.Cooldown > 0)
                {
                    spellInfo.Cooldown -= deltaTime;
                    if (spellInfo.Cooldown < 0)
                    {
                        spellInfo.Cooldown = 0;
                    }
                }
            }
        }
        
        public List<PlayerSpellInfo> GetSpells()
        {
            return new List<PlayerSpellInfo>(_spells.Values);
        }
    }
    
    // Information about a player's spell
    [System.Serializable]
    public class PlayerSpellInfo
    {
        public Spell Spell;
        public int Level;
        public float Cooldown;
    }
    
    // Class to track active spell instances in the world
    public class ActiveSpellInstance
    {
        public GameObject SpellObject;
        public SpellBehavior SpellBehavior;
        public PlayerController Caster;
        public Spell Spell;
        public int Level;
        public float StartTime;
        
        public bool IsFinished => SpellBehavior.IsFinished;
        
        public void UpdateSpell(float deltaTime)
        {
            if (SpellBehavior != null)
            {
                SpellBehavior.UpdateSpell(deltaTime);
            }
        }
        
        public void CleanUp()
        {
            if (SpellObject != null)
            {
                GameObject.Destroy(SpellObject);
            }
        }
    }
    
    // Modifiers for spell effects
    public struct SpellModifiers
    {
        public float DamageMultiplier;
        public float HealingMultiplier;
        public float DurationMultiplier;
        public float AreaMultiplier;
        public float RangeMultiplier;
    }
    
    // Data structure for spell effects when they hit targets
    public struct SpellEffect
    {
        public PlayerController Caster;
        public Spell SourceSpell;
        public float Damage;
        public float Healing;
        public bool IsFriendly;
        public List<StatusEffect> StatusEffects;
    }
}