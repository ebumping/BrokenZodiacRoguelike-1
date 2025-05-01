using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    // Partial class with spell system integration for PlayerController
    public partial class PlayerController
    {
        // Public properties for spell system
        public Transform ProjectileSpawnPoint => _aimPivot;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public float CurrentMana => _currentMana;
        public float MaxMana => _manaMax;
        public float HealthRegen => _healthRegen;
        public float ManaRegen => _manaRegen;
        public int NetworkId => networkId;
        public string PlayerName { get; private set; } = "Player";
        public List<TarotCard> TarotCards => _tarotCards;
        
        // Status flags
        private bool _isRooted = false;
        private bool _isSilenced = false;
        private bool _isInvulnerable = false;
        private float _reflectionChance = 0f;
        private float _damageAmplification = 0f;
        private float _shieldAmount = 0f;
        private float _damageReductionMultiplier = 1.0f;
        
        // Spell system integration
        private List<Spell> _knownSpells = new List<Spell>();
        
        // Play the casting animation - called by SpellManager
        public void PlayCastAnimation()
        {
            if (_sprite != null)
            {
                _sprite.Play("cast");
            }
        }
        
        // Consume mana - called by SpellManager
        public void ConsumeMana(float amount)
        {
            _currentMana = Mathf.Max(0, _currentMana - amount);
            UpdateManaBar();
        }
        
        // Updated CastSpell method that integrates with SpellManager
        private void CastSpellWithManager()
        {
            if (_spells.Count == 0 || _currentSpellIndex >= _spells.Count) return;
            
            // Get current spell from SpellManager
            Spell currentSpell = SpellManager.Instance.GetSpellByName(_spells[_currentSpellIndex]);
            if (currentSpell == null)
            {
                UnityEngine.Debug.LogWarning($"Spell not found: {_spells[_currentSpellIndex]}");
                return;
            }
            
            // Check if player is silenced
            if (_isSilenced)
            {
                UnityEngine.Debug.Log("Cannot cast while silenced");
                return;
            }
            
            // Check cooldown
            float remainingCooldown = SpellManager.Instance.GetSpellCooldown(networkId, currentSpell);
            if (remainingCooldown > 0)
            {
                UnityEngine.Debug.Log($"Spell on cooldown for {remainingCooldown:F1} seconds");
                return;
            }
            
            // Calculate target position based on aim direction
            Vector3 targetPosition;
            GameObject targetObject = null;
            
            if (currentSpell.RequiresTarget)
            {
                // Try to find a target
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position, 
                    _aimDirection, 
                    currentSpell.Range, 
                    LayerMask.GetMask("Enemy", "Player", "Environment"));
                
                if (hit.collider != null)
                {
                    targetPosition = hit.point;
                    targetObject = hit.collider.gameObject;
                }
                else
                {
                    // No target found within range
                    targetPosition = transform.position + (Vector3)(_aimDirection * currentSpell.Range);
                }
            }
            else
            {
                // Non-targeted spell, just use aim direction
                targetPosition = transform.position + (Vector3)(_aimDirection * 5f);
            }
            
            // Cast the spell through SpellManager
            bool success = SpellManager.Instance.CastSpell(networkId, currentSpell, targetPosition, targetObject);
            
            if (success)
            {
                // Play sound
                PlaySound("cast");
                
                // Set local cooldown for animation
                _canCastSpell = false;
                _spellCooldownTimer = 0.5f; // Global cooldown
            }
        }
        
        // Initialize spell system
        public void InitializeSpellSystem()
        {
            // Register with SpellManager
            if (SpellManager.Instance != null)
            {
                SpellManager.Instance.RegisterPlayer(networkId, this);
                
                // Get starting spells based on class
                if (_playerClass != null)
                {
                    var starterSpells = SpellManager.Instance.GetSpellDatabase().GetStarterSpells(_playerClass.ClassName);
                    foreach (var spell in starterSpells)
                    {
                        AddSpell(spell.SpellName);
                        SpellManager.Instance.AddSpellToPlayer(networkId, spell);
                    }
                }
            }
        }
        
        // Clean up spell system
        public void CleanupSpellSystem()
        {
            // Unregister from SpellManager
            if (SpellManager.Instance != null)
            {
                SpellManager.Instance.UnregisterPlayer(networkId);
            }
        }
        
        // Status effect methods
        public void SetRooted(bool isRooted)
        {
            _isRooted = isRooted;
        }
        
        public void SetSilenced(bool isSilenced)
        {
            _isSilenced = isSilenced;
        }
        
        public void SetInvulnerable(bool isInvulnerable)
        {
            _isInvulnerable = isInvulnerable;
        }
        
        public void SetReflectionChance(float chance)
        {
            _reflectionChance = chance;
        }
        
        public void SetDamageAmplification(float amplification)
        {
            _damageAmplification = amplification;
        }
        
        public void ApplyShield(float amount)
        {
            _shieldAmount += amount;
        }
        
        // Override the base TakeDamage method to include status effects
        public new void TakeDamage(float amount)
        {
            // Check invulnerability
            if (_isInvulnerable)
                return;
            
            // Check for reflection
            if (UnityEngine.Random.value < _reflectionChance)
            {
                // Reflection logic would go here
                UnityEngine.Debug.Log("Reflected damage!");
                return;
            }
            
            // Apply damage amplification
            float actualDamage = amount;
            if (_damageAmplification > 0)
            {
                actualDamage *= (1f + _damageAmplification);
            }
            
            // Apply damage reduction
            actualDamage *= _damageReductionMultiplier;
            
            // Check for shield
            if (_shieldAmount > 0)
            {
                if (_shieldAmount >= actualDamage)
                {
                    _shieldAmount -= actualDamage;
                    // Visual effect for shield absorbing damage
                    return;
                }
                else
                {
                    actualDamage -= _shieldAmount;
                    _shieldAmount = 0;
                    // Visual effect for shield breaking
                }
            }
            
            // Apply actual damage to health
            _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);
            UpdateHealthBar();
            
            // Play hit animation
            if (_sprite != null)
            {
                _sprite.Play("hit");
            }
            
            // Play hit sound
            PlaySound("hit");
            
            // Check for death
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        // Stat modification methods
        public void SetMaxHealth(float value)
        {
            float originalMax = maxHealth;
            maxHealth = value;
            
            // Adjust current health proportionally
            if (originalMax > 0)
            {
                _currentHealth = ((_currentHealth / originalMax) * maxHealth);
            }
            
            UpdateHealthBar();
        }
        
        public void SetMaxMana(float value)
        {
            float originalMax = _manaMax;
            _manaMax = value;
            
            // Adjust current mana proportionally
            if (originalMax > 0)
            {
                _currentMana = ((_currentMana / originalMax) * _manaMax);
            }
            
            UpdateManaBar();
        }
        
        public void SetMoveSpeed(float value)
        {
            _moveSpeed = value;
        }
        
        public void SetHealthRegen(float value)
        {
            _healthRegen = value;
        }
        
        public void SetManaRegen(float value)
        {
            _manaRegen = value;
        }
        
        public void SetNetworkId(int id)
        {
            networkId = id;
        }
        
        public void SetPlayerName(string name)
        {
            PlayerName = name;
        }
        
        // Tarot Card methods
        public void AddTarotCard(TarotCard card)
        {
            if (card != null)
            {
                _tarotCards.Add(card);
                card.ApplyCardEffects(this);
                
                // Update synergies with the new card
                UpdateCardSpellSynergies();
            }
        }
        
        // Update synergies between cards and spells
        private void UpdateCardSpellSynergies()
        {
            if (CardSpellSynergySystem.Instance != null)
            {
                CardSpellSynergySystem.Instance.UpdatePlayerSynergies(networkId, _tarotCards, _spells);
            }
        }
        
        // Updated to use spell name strings
        public void AddAbility(string spellName)
        {
            if (!_spells.Contains(spellName))
            {
                _spells.Add(spellName);
                
                // If this is our first spell, select it
                if (_spells.Count == 1)
                {
                    _currentSpellIndex = 0;
                    OnSpellChanged?.Invoke(spellName);
                }
                
                // Also add to SpellManager if it's a valid spell
                Spell spell = SpellManager.Instance?.GetSpellByName(spellName);
                if (spell != null)
                {
                    SpellManager.Instance.AddSpellToPlayer(networkId, spell);
                }
                
                // Update synergies with the new spell
                UpdateCardSpellSynergies();
            }
        }
        
        // Remove a spell ability
        public void RemoveAbility(string spellName)
        {
            int index = _spells.IndexOf(spellName);
            if (index >= 0)
            {
                _spells.RemoveAt(index);
                
                // Update current spell index if needed
                if (_currentSpellIndex >= _spells.Count && _spells.Count > 0)
                {
                    _currentSpellIndex = 0;
                    OnSpellChanged?.Invoke(_spells[_currentSpellIndex]);
                }
                
                // Also remove from SpellManager
                Spell spell = SpellManager.Instance?.GetSpellByName(spellName);
                if (spell != null)
                {
                    SpellManager.Instance.RemoveSpellFromPlayer(networkId, spell);
                }
                
                // Update synergies without the removed spell
                UpdateCardSpellSynergies();
            }
        }
    }
}