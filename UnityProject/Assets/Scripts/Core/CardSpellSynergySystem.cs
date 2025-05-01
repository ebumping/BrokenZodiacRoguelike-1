using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    // This class manages the synergies between tarot cards and spells
    public class CardSpellSynergySystem : MonoBehaviour
    {
        // Singleton instance
        public static CardSpellSynergySystem Instance { get; private set; }
        
        // Database of all possible synergies
        private Dictionary<string, Dictionary<string, SynergyEffect>> _synergies = new Dictionary<string, Dictionary<string, SynergyEffect>>();
        
        // Active synergies per player
        private Dictionary<int, List<ActiveSynergy>> _playerSynergies = new Dictionary<int, List<ActiveSynergy>>();
        
        private void Awake()
        {
            // Setup singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSynergies();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        // Initialize all possible synergies between cards and spells
        private void InitializeSynergies()
        {
            // Get all cards and spells
            List<TarotCard> allCards = new List<TarotCard>();
            List<Spell> allSpells = new List<Spell>();
            
            // In a real implementation, these would be loaded from the databases
            // For this example, we'll define some hardcoded synergies
            
            // Major Arcana Synergies (examples)
            
            // The Magician + Fire spells
            AddSynergy("The Magician", "Fireball", new SynergyEffect
            {
                Description = "The Magician enhances fire magic, increasing damage and explosion radius.",
                DamageMultiplier = 1.5f,
                AreaMultiplier = 1.3f
            });
            
            // The High Priestess + Arcane spells
            AddSynergy("The High Priestess", "Arcane Shield", new SynergyEffect
            {
                Description = "The High Priestess strengthens arcane barriers, extending their duration.",
                DurationMultiplier = 1.5f,
                EffectPowerMultiplier = 1.3f
            });
            
            // The Empress + Healing spells
            AddSynergy("The Empress", "Healing Wave", new SynergyEffect
            {
                Description = "The Empress enhances healing effects, increasing potency and adding regeneration.",
                HealingMultiplier = 1.5f,
                AdditionalEffect = StatusEffectType.Regeneration
            });
            
            // The Emperor + Damage spells
            AddSynergy("The Emperor", "Lightning Bolt", new SynergyEffect
            {
                Description = "The Emperor enhances destructive magic, increasing damage and adding a stun effect.",
                DamageMultiplier = 1.3f,
                AdditionalEffect = StatusEffectType.Stun
            });
            
            // The Tower + AoE spells
            AddSynergy("The Tower", "Poison Nova", new SynergyEffect
            {
                Description = "The Tower amplifies area effects, increasing their radius and damage.",
                DamageMultiplier = 1.2f,
                AreaMultiplier = 1.5f
            });
            
            // The Moon + Mind spells
            AddSynergy("The Moon", "Mind Blast", new SynergyEffect
            {
                Description = "The Moon enhances mind-affecting magic, extending duration and adding confusion.",
                DurationMultiplier = 1.5f,
                AdditionalEffect = StatusEffectType.Confusion
            });
            
            // The World + All spells
            foreach (var spellName in new string[] { "Fireball", "Ice Spike", "Lightning Bolt", "Healing Wave", "Arcane Shield" })
            {
                AddSynergy("The World", spellName, new SynergyEffect
                {
                    Description = "The World enhances all magic, reducing mana cost and cooldown.",
                    ManaCostMultiplier = 0.7f,
                    CooldownMultiplier = 0.8f
                });
            }
            
            // Minor Arcana Synergies (examples)
            
            // Wands (Fire) synergize with fire spells
            for (int i = 1; i <= 14; i++)
            {
                string cardName = GetMinorArcanaName("Wands", i);
                AddSynergy(cardName, "Fireball", new SynergyEffect
                {
                    Description = "Wands enhance fire magic.",
                    DamageMultiplier = 1.2f,
                    CooldownMultiplier = 0.9f
                });
            }
            
            // Cups (Water) synergize with healing spells
            for (int i = 1; i <= 14; i++)
            {
                string cardName = GetMinorArcanaName("Cups", i);
                AddSynergy(cardName, "Healing Wave", new SynergyEffect
                {
                    Description = "Cups enhance healing magic.",
                    HealingMultiplier = 1.3f,
                    DurationMultiplier = 1.2f
                });
            }
            
            // Swords (Air) synergize with lightning spells
            for (int i = 1; i <= 14; i++)
            {
                string cardName = GetMinorArcanaName("Swords", i);
                AddSynergy(cardName, "Lightning Bolt", new SynergyEffect
                {
                    Description = "Swords enhance lightning magic.",
                    DamageMultiplier = 1.2f,
                    ChainCountModifier = 2 // Extra chain targets
                });
            }
            
            // Pentacles (Earth) synergize with defense spells
            for (int i = 1; i <= 14; i++)
            {
                string cardName = GetMinorArcanaName("Pentacles", i);
                AddSynergy(cardName, "Arcane Shield", new SynergyEffect
                {
                    Description = "Pentacles enhance protective magic.",
                    EffectPowerMultiplier = 1.4f,
                    DurationMultiplier = 1.2f
                });
            }
            
            // Special card combinations (examples)
            
            // Three of any suit adds a combo effect
            foreach (string suit in new string[] { "Wands", "Cups", "Swords", "Pentacles" })
            {
                string cardName1 = GetMinorArcanaName(suit, 1); // Ace
                string cardName2 = GetMinorArcanaName(suit, 2); // Two
                string cardName3 = GetMinorArcanaName(suit, 3); // Three
                
                // Example spell to synergize with
                string spellName = suit == "Wands" ? "Fireball" :
                                   suit == "Cups" ? "Healing Wave" :
                                   suit == "Swords" ? "Lightning Bolt" :
                                   "Arcane Shield";
                
                // Add combo synergy
                AddComboSynergy(new string[] { cardName1, cardName2, cardName3 }, spellName, new SynergyEffect
                {
                    Description = $"Three {suit} cards create a powerful combo effect.",
                    DamageMultiplier = 1.5f,
                    HealingMultiplier = 1.5f,
                    DurationMultiplier = 1.5f,
                    AreaMultiplier = 1.3f,
                    ManaCostMultiplier = 0.7f
                });
            }
            
            // Major Arcana combos based on related cards
            AddComboSynergy(new string[] { "The Magician", "The High Priestess", "The Empress" }, "Time Slow", new SynergyEffect
            {
                Description = "The trinity of creation massively enhances time magic.",
                DurationMultiplier = 2.0f,
                AreaMultiplier = 1.5f,
                CooldownMultiplier = 0.6f,
                EffectPowerMultiplier = 1.5f
            });
            
            Debug.Log($"Initialized {_synergies.Count} card-spell synergies");
        }
        
        // Helper to get minor arcana card names
        private string GetMinorArcanaName(string suit, int value)
        {
            string valueName = "";
            switch (value)
            {
                case 1: valueName = "Ace"; break;
                case 2: valueName = "Two"; break;
                case 3: valueName = "Three"; break;
                case 4: valueName = "Four"; break;
                case 5: valueName = "Five"; break;
                case 6: valueName = "Six"; break;
                case 7: valueName = "Seven"; break;
                case 8: valueName = "Eight"; break;
                case 9: valueName = "Nine"; break;
                case 10: valueName = "Ten"; break;
                case 11: valueName = "Page"; break;
                case 12: valueName = "Knight"; break;
                case 13: valueName = "Queen"; break;
                case 14: valueName = "King"; break;
                default: valueName = value.ToString(); break;
            }
            
            return $"{valueName} of {suit}";
        }
        
        // Add a single card-spell synergy
        private void AddSynergy(string cardName, string spellName, SynergyEffect effect)
        {
            if (!_synergies.ContainsKey(cardName))
            {
                _synergies[cardName] = new Dictionary<string, SynergyEffect>();
            }
            
            _synergies[cardName][spellName] = effect;
        }
        
        // Add a combo synergy (multiple cards with one spell)
        private void AddComboSynergy(string[] cardNames, string spellName, SynergyEffect effect)
        {
            // Store this as a special combo
            string comboKey = $"COMBO:{string.Join("+", cardNames)}";
            AddSynergy(comboKey, spellName, effect);
        }
        
        // Register a player with the synergy system
        public void RegisterPlayer(int playerId)
        {
            if (!_playerSynergies.ContainsKey(playerId))
            {
                _playerSynergies[playerId] = new List<ActiveSynergy>();
            }
        }
        
        // Unregister a player
        public void UnregisterPlayer(int playerId)
        {
            if (_playerSynergies.ContainsKey(playerId))
            {
                _playerSynergies.Remove(playerId);
            }
        }
        
        // Update synergies when a player's cards change
        public void UpdatePlayerSynergies(int playerId, List<TarotCard> cards, List<string> spells)
        {
            if (!_playerSynergies.ContainsKey(playerId))
            {
                RegisterPlayer(playerId);
            }
            
            // Clear existing synergies
            _playerSynergies[playerId].Clear();
            
            // Check for single card synergies
            foreach (var card in cards)
            {
                foreach (var spellName in spells)
                {
                    CheckAndAddSynergy(playerId, card.CardName, spellName);
                }
            }
            
            // Check for combo synergies
            CheckForComboSynergies(playerId, cards, spells);
            
            // Apply synergy effects to the spells
            ApplySynergyEffects(playerId);
        }
        
        // Check for and add a specific card-spell synergy
        private void CheckAndAddSynergy(int playerId, string cardName, string spellName)
        {
            if (_synergies.ContainsKey(cardName) && _synergies[cardName].ContainsKey(spellName))
            {
                SynergyEffect effect = _synergies[cardName][spellName];
                
                _playerSynergies[playerId].Add(new ActiveSynergy
                {
                    CardName = cardName,
                    SpellName = spellName,
                    Effect = effect
                });
                
                Debug.Log($"Added synergy: {cardName} + {spellName}");
            }
        }
        
        // Check for combo synergies
        private void CheckForComboSynergies(int playerId, List<TarotCard> cards, List<string> spells)
        {
            // Get all card names
            List<string> cardNames = new List<string>();
            foreach (var card in cards)
            {
                cardNames.Add(card.CardName);
            }
            
            // Check each combo possibility (this is inefficient but works for example)
            foreach (var key in _synergies.Keys)
            {
                if (key.StartsWith("COMBO:"))
                {
                    // Parse the combo cards
                    string[] comboCards = key.Substring(6).Split('+');
                    bool comboValid = true;
                    
                    // Check if player has all required cards
                    foreach (var comboCard in comboCards)
                    {
                        if (!cardNames.Contains(comboCard))
                        {
                            comboValid = false;
                            break;
                        }
                    }
                    
                    if (comboValid)
                    {
                        // Check if player has any of the synergizing spells
                        foreach (var spellName in _synergies[key].Keys)
                        {
                            if (spells.Contains(spellName))
                            {
                                SynergyEffect effect = _synergies[key][spellName];
                                
                                _playerSynergies[playerId].Add(new ActiveSynergy
                                {
                                    CardName = key, // This stores the combo key
                                    SpellName = spellName,
                                    Effect = effect,
                                    IsCombo = true
                                });
                                
                                Debug.Log($"Added combo synergy: {key} + {spellName}");
                            }
                        }
                    }
                }
            }
        }
        
        // Apply all active synergy effects to player's spells
        private void ApplySynergyEffects(int playerId)
        {
            if (!_playerSynergies.ContainsKey(playerId) || SpellManager.Instance == null)
                return;
            
            // Group effects by spell
            Dictionary<string, List<SynergyEffect>> spellEffects = new Dictionary<string, List<SynergyEffect>>();
            
            foreach (var synergy in _playerSynergies[playerId])
            {
                if (!spellEffects.ContainsKey(synergy.SpellName))
                {
                    spellEffects[synergy.SpellName] = new List<SynergyEffect>();
                }
                
                spellEffects[synergy.SpellName].Add(synergy.Effect);
            }
            
            // Apply combined effects to each spell
            foreach (var spellName in spellEffects.Keys)
            {
                Spell spell = SpellManager.Instance.GetSpellByName(spellName);
                if (spell != null)
                {
                    // Combine all effects for this spell
                    CombinedSynergyEffect combinedEffect = CombineEffects(spellEffects[spellName]);
                    
                    // Apply to the spell via SpellManager
                    ApplyEffectToSpell(playerId, spell, combinedEffect);
                }
            }
        }
        
        // Combine multiple synergy effects into one
        private CombinedSynergyEffect CombineEffects(List<SynergyEffect> effects)
        {
            CombinedSynergyEffect combined = new CombinedSynergyEffect();
            
            // Initialize with default multipliers (1.0 = no change)
            combined.DamageMultiplier = 1.0f;
            combined.HealingMultiplier = 1.0f;
            combined.DurationMultiplier = 1.0f;
            combined.AreaMultiplier = 1.0f;
            combined.ManaCostMultiplier = 1.0f;
            combined.CooldownMultiplier = 1.0f;
            combined.EffectPowerMultiplier = 1.0f;
            
            // Combine all effects
            foreach (var effect in effects)
            {
                // Multiply the multipliers (this compounds the effects)
                if (effect.DamageMultiplier != 0)
                    combined.DamageMultiplier *= effect.DamageMultiplier;
                    
                if (effect.HealingMultiplier != 0)
                    combined.HealingMultiplier *= effect.HealingMultiplier;
                    
                if (effect.DurationMultiplier != 0)
                    combined.DurationMultiplier *= effect.DurationMultiplier;
                    
                if (effect.AreaMultiplier != 0)
                    combined.AreaMultiplier *= effect.AreaMultiplier;
                    
                if (effect.ManaCostMultiplier != 0)
                    combined.ManaCostMultiplier *= effect.ManaCostMultiplier;
                    
                if (effect.CooldownMultiplier != 0)
                    combined.CooldownMultiplier *= effect.CooldownMultiplier;
                    
                if (effect.EffectPowerMultiplier != 0)
                    combined.EffectPowerMultiplier *= effect.EffectPowerMultiplier;
                    
                // Add flat modifiers
                combined.ChainCountModifier += effect.ChainCountModifier;
                
                // Track additional effects
                if (effect.AdditionalEffect != StatusEffectType.Burn) // Using Burn as 'None' type
                    combined.AdditionalEffects.Add(effect.AdditionalEffect);
            }
            
            return combined;
        }
        
        // Apply a combined effect to a specific spell for a player
        private void ApplyEffectToSpell(int playerId, Spell spell, CombinedSynergyEffect effect)
        {
            // In a real implementation, this would modify the spell's behavior
            // for this specific player. Here we'll use a simplified approach.
            
            // We could add a custom component to the spell prefab to handle these modifiers
            // or track them in the SpellManager for each player-spell combination.
            
            // For this example, we'll just update the SpellManager's global modifiers
            // which isn't ideal but demonstrates the concept
            if (SpellManager.Instance != null)
            {
                // Apply spell-specific modifiers (this is simplified)
                // In a real game, these would be applied only to this specific spell for this player
                SpellManager.Instance.ModifySpellDamage(effect.DamageMultiplier);
                SpellManager.Instance.ModifySpellHealing(effect.HealingMultiplier);
                SpellManager.Instance.ModifySpellDuration(effect.DurationMultiplier);
                SpellManager.Instance.ModifySpellArea(effect.AreaMultiplier);
                SpellManager.Instance.ModifySpellManaCost(effect.ManaCostMultiplier);
                
                // Log the synergy effect
                Debug.Log($"Applied synergy effect to {spell.SpellName} for player {playerId}: " +
                          $"DMG:{effect.DamageMultiplier:F2} HEAL:{effect.HealingMultiplier:F2} " +
                          $"DUR:{effect.DurationMultiplier:F2} AREA:{effect.AreaMultiplier:F2} " +
                          $"MANA:{effect.ManaCostMultiplier:F2} COOL:{effect.CooldownMultiplier:F2}");
            }
        }
        
        // Get a description of all active synergies for a player
        public List<string> GetActiveSynergyDescriptions(int playerId)
        {
            List<string> descriptions = new List<string>();
            
            if (_playerSynergies.ContainsKey(playerId))
            {
                foreach (var synergy in _playerSynergies[playerId])
                {
                    string cardName = synergy.IsCombo ? synergy.CardName.Substring(6).Replace("+", " + ") : synergy.CardName;
                    
                    descriptions.Add($"{cardName} + {synergy.SpellName}: {synergy.Effect.Description}");
                }
            }
            
            return descriptions;
        }
    }
    
    // Synergy effect data
    [System.Serializable]
    public class SynergyEffect
    {
        public string Description;
        public float DamageMultiplier;
        public float HealingMultiplier;
        public float DurationMultiplier;
        public float AreaMultiplier;
        public float ManaCostMultiplier;
        public float CooldownMultiplier;
        public float EffectPowerMultiplier;
        public int ChainCountModifier;
        public StatusEffectType AdditionalEffect = StatusEffectType.Burn; // Using Burn as 'None' type
    }
    
    // Combined synergy effect (result of multiple effects)
    [System.Serializable]
    public class CombinedSynergyEffect
    {
        public float DamageMultiplier;
        public float HealingMultiplier;
        public float DurationMultiplier;
        public float AreaMultiplier;
        public float ManaCostMultiplier;
        public float CooldownMultiplier;
        public float EffectPowerMultiplier;
        public int ChainCountModifier;
        public List<StatusEffectType> AdditionalEffects = new List<StatusEffectType>();
    }
    
    // Active synergy for a player
    [System.Serializable]
    public class ActiveSynergy
    {
        public string CardName;
        public string SpellName;
        public SynergyEffect Effect;
        public bool IsCombo;
    }
}