using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
    // This class provides static methods to create the Major Arcana cards
    // In a real project, you would create these as individual ScriptableObject assets
    public static class MajorArcanaCards
    {
        // Create all 22 Major Arcana cards (0-21)
        public static List<TarotCard> CreateAllMajorArcana()
        {
            List<TarotCard> cards = new List<TarotCard>()
            {
                CreateFool(),
                CreateMagician(),
                CreateHighPriestess(),
                CreateEmpress(),
                CreateEmperor(),
                CreateHierophant(),
                CreateLovers(),
                CreateChariot(),
                CreateStrength(),
                CreateHermit(),
                CreateWheelOfFortune(),
                CreateJustice(),
                CreateHangedMan(),
                CreateDeath(),
                CreateTemperance(),
                CreateDevil(),
                CreateTower(),
                CreateStar(),
                CreateMoon(),
                CreateSun(),
                CreateJudgment(),
                CreateWorld()
            };
            
            return cards;
        }
        
        // 0 - The Fool
        public static TarotCard CreateFool()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Fool";
            card.MajorArcanaNumber = "0";
            card.Type = TarotCard.CardType.Major;
            card.Description = "The beginning of a journey. Grants increased dodge distance and a chance to avoid damage.";
            card.PrimaryEffect = "Dodge Distance";
            card.SecondaryEffect = "Damage Avoidance";
            card.PrimaryEffectValue = 1.5f;
            card.SecondaryEffectValue = 0.15f; // 15% chance to avoid damage
            card.CardColor = new Color(0.8f, 0.9f, 0.4f);
            card.SpeedModifier = 1.0f;
            card.IsPassive = true;
            return card;
        }
        
        // I - The Magician
        public static TarotCard CreateMagician()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Magician";
            card.MajorArcanaNumber = "I";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Master of the elements. Increases spell damage and reduces spell cooldowns.";
            card.PrimaryEffect = "Spell Damage";
            card.SecondaryEffect = "Cooldown Reduction";
            card.PrimaryEffectValue = 1.25f; // 25% more spell damage
            card.SecondaryEffectValue = -0.2f; // 20% cooldown reduction
            card.CardColor = new Color(0.8f, 0.2f, 0.2f); // Red
            card.CooldownModifier = -0.2f;
            card.IsPassive = true;
            return card;
        }
        
        // II - The High Priestess
        public static TarotCard CreateHighPriestess()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The High Priestess";
            card.MajorArcanaNumber = "II";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Keeper of hidden knowledge. Increases mana pool and grants mana on room clear.";
            card.PrimaryEffect = "Max Mana";
            card.SecondaryEffect = "Mana Restore";
            card.PrimaryEffectValue = 25f; // +25 max mana
            card.SecondaryEffectValue = 10f; // +10 mana on room clear
            card.CardColor = new Color(0.4f, 0.4f, 0.9f); // Blue
            card.HasOnRoomClearEffect = true;
            card.IsPassive = true;
            return card;
        }
        
        // III - The Empress
        public static TarotCard CreateEmpress()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Empress";
            card.MajorArcanaNumber = "III";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Source of nurturing energy. Increases health regeneration and maximum health.";
            card.PrimaryEffect = "Health Regen";
            card.SecondaryEffect = "Max Health";
            card.PrimaryEffectValue = 0.5f; // +0.5 health regen per second
            card.SecondaryEffectValue = 15f; // +15 max health
            card.CardColor = new Color(0.2f, 0.8f, 0.3f); // Green
            card.HealthModifier = 15f;
            card.IsPassive = true;
            return card;
        }
        
        // IV - The Emperor
        public static TarotCard CreateEmperor()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Emperor";
            card.MajorArcanaNumber = "IV";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Symbol of authority. Increases damage and critical hit chance with weapons.";
            card.PrimaryEffect = "Weapon Damage";
            card.SecondaryEffect = "Critical Chance";
            card.PrimaryEffectValue = 2f; // +2 damage
            card.SecondaryEffectValue = 0.1f; // +10% crit chance
            card.CardColor = new Color(0.7f, 0.1f, 0.1f); // Dark red
            card.DamageModifier = 2f;
            card.CriticalModifier = 0.1f;
            card.IsPassive = true;
            return card;
        }
        
        // V - The Hierophant
        public static TarotCard CreateHierophant()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Hierophant";
            card.MajorArcanaNumber = "V";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Spiritual teacher. Adds a protective shield ability that absorbs damage.";
            card.PrimaryEffect = "Shield Ability";
            card.SecondaryEffect = "Shield Strength";
            card.PrimaryEffectValue = 1f; // Shield ability
            card.SecondaryEffectValue = 25f; // Shield absorbs 25 damage
            card.CardColor = new Color(0.9f, 0.9f, 0.6f); // Light gold
            card.ActiveAbility = "DivineSanctuary";
            card.IsPassive = false;
            card.IsSingleUse = false;
            return card;
        }
        
        // VI - The Lovers
        public static TarotCard CreateLovers()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Lovers";
            card.MajorArcanaNumber = "VI";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Union of opposites. Combines the effects of your two most recent tarot cards.";
            card.PrimaryEffect = "Synergy";
            card.SecondaryEffect = "Amplification";
            card.PrimaryEffectValue = 1f; // Synergy effect
            card.SecondaryEffectValue = 0.25f; // 25% amplification
            card.CardColor = new Color(0.9f, 0.5f, 0.8f); // Pink/Purple
            card.ComboEffect = "Doubles the effects of adjacent cards";
            card.ComboThreshold = 2;
            card.IsPassive = true;
            return card;
        }
        
        // VII - The Chariot
        public static TarotCard CreateChariot()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Chariot";
            card.MajorArcanaNumber = "VII";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Unstoppable movement. Greatly increases movement speed and adds a dash attack ability.";
            card.PrimaryEffect = "Movement Speed";
            card.SecondaryEffect = "Dash Attack";
            card.PrimaryEffectValue = 1.5f; // 50% movement speed increase
            card.SecondaryEffectValue = 15f; // Dash attack deals 15 damage
            card.CardColor = new Color(0.8f, 0.8f, 0.8f); // Silver
            card.SpeedModifier = 1.5f;
            card.ActiveAbility = "ChariotDash";
            card.IsPassive = false;
            card.IsSingleUse = false;
            return card;
        }
        
        // VIII - Strength
        public static TarotCard CreateStrength()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "Strength";
            card.MajorArcanaNumber = "VIII";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Inner courage. Significantly increases weapon damage. Killing enemies has a chance to restore health.";
            card.PrimaryEffect = "Weapon Damage";
            card.SecondaryEffect = "Health on Kill";
            card.PrimaryEffectValue = 3f; // +3 damage
            card.SecondaryEffectValue = 2f; // +2 health on kill (chance-based)
            card.CardColor = new Color(0.8f, 0.5f, 0.2f); // Orange
            card.DamageModifier = 3f;
            card.HasOnKillEffect = true;
            card.IsPassive = true;
            return card;
        }
        
        // IX - The Hermit
        public static TarotCard CreateHermit()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Hermit";
            card.MajorArcanaNumber = "IX";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Solitary wisdom. Reveals all rooms on the map and increases critical damage.";
            card.PrimaryEffect = "Map Reveal";
            card.SecondaryEffect = "Critical Damage";
            card.PrimaryEffectValue = 1f; // Map reveal
            card.SecondaryEffectValue = 0.5f; // +50% critical damage
            card.CardColor = new Color(0.4f, 0.4f, 0.4f); // Gray
            card.CriticalModifier = 0.2f;
            card.IsPassive = true;
            return card;
        }
        
        // X - Wheel of Fortune
        public static TarotCard CreateWheelOfFortune()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "Wheel of Fortune";
            card.MajorArcanaNumber = "X";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Random chance. Your shots have a chance to trigger random magical effects.";
            card.PrimaryEffect = "Random Magic";
            card.SecondaryEffect = "Chaos Chance";
            card.PrimaryEffectValue = 1f; // Enable random effects
            card.SecondaryEffectValue = 0.2f; // 20% chance to trigger
            card.CardColor = new Color(0.9f, 0.7f, 0.2f); // Gold
            card.HasOnHitEffect = true;
            card.ComboEffect = "Increases random effect chance";
            card.ComboThreshold = 3;
            card.IsPassive = true;
            return card;
        }
        
        // XI - Justice
        public static TarotCard CreateJustice()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "Justice";
            card.MajorArcanaNumber = "XI";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Balance and equity. Evenly increases all your stats. Reflects a portion of damage back to enemies.";
            card.PrimaryEffect = "Balanced Growth";
            card.SecondaryEffect = "Damage Reflection";
            card.PrimaryEffectValue = 1f; // +1 to all stats
            card.SecondaryEffectValue = 0.15f; // 15% damage reflection
            card.CardColor = new Color(0.9f, 0.9f, 0.9f); // White
            card.HealthModifier = 10f;
            card.DamageModifier = 1f;
            card.SpeedModifier = 0.2f;
            card.CooldownModifier = -0.1f;
            card.CriticalModifier = 0.05f;
            card.HasOnHitEffect = true;
            card.IsPassive = true;
            return card;
        }
        
        // XII - The Hanged Man
        public static TarotCard CreateHangedMan()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Hanged Man";
            card.MajorArcanaNumber = "XII";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Sacrifice and insight. Taking damage increases your damage output for a short time.";
            card.PrimaryEffect = "Damage Bonus";
            card.SecondaryEffect = "Sacrifice";
            card.PrimaryEffectValue = 0.5f; // +50% damage after taking damage
            card.SecondaryEffectValue = 5f; // Lasts 5 seconds
            card.CardColor = new Color(0.5f, 0.0f, 0.5f); // Purple
            card.HasOnHitEffect = true;
            card.IsPassive = true;
            return card;
        }
        
        // XIII - Death
        public static TarotCard CreateDeath()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "Death";
            card.MajorArcanaNumber = "XIII";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Transformation. Your attacks have a chance to instantly kill low-health enemies. Gain mana on kill.";
            card.PrimaryEffect = "Execution";
            card.SecondaryEffect = "Mana on Kill";
            card.PrimaryEffectValue = 0.2f; // 20% execution chance for enemies below 15% health
            card.SecondaryEffectValue = 5f; // +5 mana on kill
            card.CardColor = new Color(0.0f, 0.0f, 0.0f); // Black
            card.HasOnHitEffect = true;
            card.HasOnKillEffect = true;
            card.IsPassive = true;
            return card;
        }
        
        // XIV - Temperance
        public static TarotCard CreateTemperance()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "Temperance";
            card.MajorArcanaNumber = "XIV";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Balanced forces. Your health and mana regenerate faster when both are below 50%.";
            card.PrimaryEffect = "Enhanced Regen";
            card.SecondaryEffect = "Balance";
            card.PrimaryEffectValue = 1.0f; // Double regen when below 50%
            card.SecondaryEffectValue = 0.5f; // Below 50% threshold
            card.CardColor = new Color(0.5f, 0.6f, 0.9f); // Light blue
            card.IsPassive = true;
            return card;
        }
        
        // XV - The Devil
        public static TarotCard CreateDevil()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Devil";
            card.MajorArcanaNumber = "XV";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Temptation and power. Greatly increases damage but reduces max health by 25%.";
            card.PrimaryEffect = "Massive Damage";
            card.SecondaryEffect = "Health Penalty";
            card.PrimaryEffectValue = 5f; // +5 damage
            card.SecondaryEffectValue = -0.25f; // -25% max health
            card.CardColor = new Color(0.5f, 0.0f, 0.0f); // Dark red
            card.DamageModifier = 5f;
            card.HealthModifier = -25f; // Negative modifier
            card.IsPassive = true;
            return card;
        }
        
        // XVI - The Tower
        public static TarotCard CreateTower()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Tower";
            card.MajorArcanaNumber = "XVI";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Sudden change. Adds an ability to call down lightning that chains between enemies.";
            card.PrimaryEffect = "Lightning Strike";
            card.SecondaryEffect = "Chain Lightning";
            card.PrimaryEffectValue = 25f; // 25 damage per strike
            card.SecondaryEffectValue = 3f; // Chains to 3 enemies
            card.CardColor = new Color(0.9f, 0.9f, 0.1f); // Yellow
            card.ActiveAbility = "LightningStrike";
            card.IsPassive = false;
            card.IsSingleUse = false;
            return card;
        }
        
        // XVII - The Star
        public static TarotCard CreateStar()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Star";
            card.MajorArcanaNumber = "XVII";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Hope and inspiration. Increases health and mana regeneration. Your attacks leave healing stars.";
            card.PrimaryEffect = "Enhanced Regen";
            card.SecondaryEffect = "Healing Stars";
            card.PrimaryEffectValue = 0.8f; // +0.8 health regen
            card.SecondaryEffectValue = 1f; // Stars heal 1 health
            card.CardColor = new Color(0.4f, 0.8f, 0.9f); // Light cyan
            card.HasOnHitEffect = true;
            card.IsPassive = true;
            return card;
        }
        
        // XVIII - The Moon
        public static TarotCard CreateMoon()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Moon";
            card.MajorArcanaNumber = "XVIII";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Illusion and mystery. Your attacks have a chance to confuse enemies, making them attack each other.";
            card.PrimaryEffect = "Confusion";
            card.SecondaryEffect = "Chaos Duration";
            card.PrimaryEffectValue = 0.25f; // 25% chance to confuse
            card.SecondaryEffectValue = 5f; // Confusion lasts 5 seconds
            card.CardColor = new Color(0.5f, 0.5f, 0.9f); // Purple-blue
            card.HasOnHitEffect = true;
            card.IsPassive = true;
            return card;
        }
        
        // XIX - The Sun
        public static TarotCard CreateSun()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The Sun";
            card.MajorArcanaNumber = "XIX";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Vitality and enlightenment. Your attacks burn enemies. Adds a solar flare ability.";
            card.PrimaryEffect = "Burning Attacks";
            card.SecondaryEffect = "Solar Flare";
            card.PrimaryEffectValue = 2f; // 2 damage per tick of burning
            card.SecondaryEffectValue = 35f; // Solar flare deals 35 damage
            card.CardColor = new Color(1.0f, 0.8f, 0.0f); // Bright yellow/orange
            card.HasOnHitEffect = true;
            card.ActiveAbility = "SolarFlare";
            card.IsPassive = false;
            card.IsSingleUse = false;
            return card;
        }
        
        // XX - Judgment
        public static TarotCard CreateJudgment()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "Judgment";
            card.MajorArcanaNumber = "XX";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Rebirth and absolution. When you would die, survive with 1 health once per level.";
            card.PrimaryEffect = "Second Chance";
            card.SecondaryEffect = "Invulnerability";
            card.PrimaryEffectValue = 1f; // One second chance
            card.SecondaryEffectValue = 3f; // 3 seconds of invulnerability after revival
            card.CardColor = new Color(0.9f, 0.7f, 0.9f); // Light purple
            card.IsPassive = true;
            card.IsSingleUse = true; // Resets per level
            return card;
        }
        
        // XXI - The World
        public static TarotCard CreateWorld()
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.CardName = "The World";
            card.MajorArcanaNumber = "XXI";
            card.Type = TarotCard.CardType.Major;
            card.Description = "Completion and fulfillment. Enhances all other tarot card effects by 25%. Revives you once with full health.";
            card.PrimaryEffect = "Enhanced Cards";
            card.SecondaryEffect = "Full Revival";
            card.PrimaryEffectValue = 0.25f; // 25% enhancement to other cards
            card.SecondaryEffectValue = 1f; // One full revival
            card.CardColor = new Color(0.2f, 0.7f, 0.5f); // Turquoise
            card.ComboEffect = "Enhances all other card effects";
            card.ComboThreshold = 5; // Works with at least 5 other cards
            card.IsPassive = true;
            card.IsSingleUse = true; // One revival per run
            return card;
        }
    }
}