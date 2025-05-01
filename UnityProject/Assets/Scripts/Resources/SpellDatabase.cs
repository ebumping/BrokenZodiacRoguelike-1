using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.Resources
{
    [CreateAssetMenu(fileName = "SpellDatabase", menuName = "Codex/SpellDatabase")]
    public class SpellDatabase : ScriptableObject
    {
        // Categories of spells for organization
        [SerializeField] private List<Spell> offensiveSpells = new List<Spell>();
        [SerializeField] private List<Spell> defensiveSpells = new List<Spell>();
        [SerializeField] private List<Spell> utilitySpells = new List<Spell>();
        [SerializeField] private List<Spell> summoningSpells = new List<Spell>();
        [SerializeField] private List<Spell> transformationSpells = new List<Spell>();
        
        // Class-specific spell lists
        [SerializeField] private List<Spell> detectiveSpells = new List<Spell>();
        [SerializeField] private List<Spell> mediumSpells = new List<Spell>();
        [SerializeField] private List<Spell> debtorSpells = new List<Spell>();
        [SerializeField] private List<Spell> archivistSpells = new List<Spell>();
        [SerializeField] private List<Spell> oratorSpells = new List<Spell>();
        [SerializeField] private List<Spell> constableSpells = new List<Spell>();
        
        // Combined list of all spells for easy access
        private List<Spell> _allSpells = null;
        
        // Get all available spells
        public List<Spell> GetAllSpells()
        {
            if (_allSpells == null)
            {
                // Initialize the combined list if it hasn't been already
                _allSpells = new List<Spell>();
                _allSpells.AddRange(offensiveSpells);
                _allSpells.AddRange(defensiveSpells);
                _allSpells.AddRange(utilitySpells);
                _allSpells.AddRange(summoningSpells);
                _allSpells.AddRange(transformationSpells);
            }
            
            return _allSpells;
        }
        
        // Get spells for a specific class
        public List<Spell> GetSpellsByClass(string className)
        {
            switch (className)
            {
                case "OccultDetective":
                    return detectiveSpells;
                case "ApostateMedium":
                    return mediumSpells;
                case "IrredeemableDebtor":
                    return debtorSpells;
                case "ReclusiveArchivist":
                    return archivistSpells;
                case "SeditiousOrator":
                    return oratorSpells;
                case "PhantomConstable":
                    return constableSpells;
                default:
                    return new List<Spell>();
            }
        }
        
        // Get spells by element
        public List<Spell> GetSpellsByElement(SpellElement element)
        {
            List<Spell> result = new List<Spell>();
            foreach (var spell in GetAllSpells())
            {
                if (spell.Element == element)
                    result.Add(spell);
            }
            return result;
        }
        
        // Get spells by type
        public List<Spell> GetSpellsByType(SpellType type)
        {
            List<Spell> result = new List<Spell>();
            foreach (var spell in GetAllSpells())
            {
                if (spell.Type == type)
                    result.Add(spell);
            }
            return result;
        }
        
        // Find a spell by name
        public Spell GetSpellByName(string spellName)
        {
            foreach (var spell in GetAllSpells())
            {
                if (spell.SpellName == spellName)
                    return spell;
            }
            return null;
        }
        
        // Get starter spells for a player based on class
        public List<Spell> GetStarterSpells(string className)
        {
            List<Spell> classSpells = GetSpellsByClass(className);
            List<Spell> starterSpells = new List<Spell>();
            
            // Each class starts with 2-3 basic spells
            int count = Mathf.Min(3, classSpells.Count);
            for (int i = 0; i < count; i++)
            {
                starterSpells.Add(classSpells[i]);
            }
            
            return starterSpells;
        }
        
        // Get spells that can be unlocked at a particular player level
        public List<Spell> GetUnlockableSpells(string className, int playerLevel)
        {
            List<Spell> classSpells = GetSpellsByClass(className);
            List<Spell> unlockable = new List<Spell>();
            
            foreach (var spell in classSpells)
            {
                // Assuming spells have a level requirement in UnlockRequirement field
                // Format would be something like "Level:5"
                if (!string.IsNullOrEmpty(spell.UnlockRequirement) && 
                    spell.UnlockRequirement.StartsWith("Level:"))
                {
                    string levelStr = spell.UnlockRequirement.Substring(6);
                    if (int.TryParse(levelStr, out int requiredLevel) && playerLevel >= requiredLevel)
                    {
                        unlockable.Add(spell);
                    }
                }
            }
            
            return unlockable;
        }
        
        // Get random spells for loot or rewards
        public List<Spell> GetRandomSpells(int count, int maxLevel = 99)
        {
            List<Spell> availableSpells = new List<Spell>();
            
            // Filter by level requirement
            foreach (var spell in GetAllSpells())
            {
                if (!string.IsNullOrEmpty(spell.UnlockRequirement) && 
                    spell.UnlockRequirement.StartsWith("Level:"))
                {
                    string levelStr = spell.UnlockRequirement.Substring(6);
                    if (int.TryParse(levelStr, out int requiredLevel) && requiredLevel <= maxLevel)
                    {
                        availableSpells.Add(spell);
                    }
                }
                else
                {
                    availableSpells.Add(spell);
                }
            }
            
            // Select random spells
            List<Spell> result = new List<Spell>();
            for (int i = 0; i < count && availableSpells.Count > 0; i++)
            {
                int index = Random.Range(0, availableSpells.Count);
                result.Add(availableSpells[index]);
                availableSpells.RemoveAt(index); // Prevent duplicates
            }
            
            return result;
        }
    }
}