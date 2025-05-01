using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
    // This class provides static methods to create the Minor Arcana cards
    // In a real project, you would create these as individual ScriptableObject assets
    public static class MinorArcanaCards
    {
        // Create all Minor Arcana cards for a specific suit
        public static List<TarotCard> CreateMinorArcanaSuit(TarotCard.CardSuit suit)
        {
            List<TarotCard> cards = new List<TarotCard>();
            
            // Add Ace through King (1-14)
            for (int i = 1; i <= 14; i++)
            {
                cards.Add(CreateMinorArcanaCard(suit, i));
            }
            
            return cards;
        }
        
        // Create a specific Minor Arcana card
        public static TarotCard CreateMinorArcanaCard(TarotCard.CardSuit suit, int value)
        {
            TarotCard card = ScriptableObject.CreateInstance<TarotCard>();
            card.Type = TarotCard.CardType.Minor;
            card.Suit = suit;
            card.Value = value;
            
            // Set base properties based on suit
            switch (suit)
            {
                case TarotCard.CardSuit.Wands:
                    card.CardColor = new Color(0.9f, 0.4f, 0.1f); // Orange-red
                    ConfigureWandsCard(card, value);
                    break;
                    
                case TarotCard.CardSuit.Cups:
                    card.CardColor = new Color(0.1f, 0.4f, 0.9f); // Blue
                    ConfigureCupsCard(card, value);
                    break;
                    
                case TarotCard.CardSuit.Swords:
                    card.CardColor = new Color(0.7f, 0.7f, 0.8f); // Silver-gray
                    ConfigureSwordsCard(card, value);
                    break;
                    
                case TarotCard.CardSuit.Pentacles:
                    card.CardColor = new Color(0.1f, 0.6f, 0.1f); // Green
                    ConfigurePentaclesCard(card, value);
                    break;
            }
            
            return card;
        }
        
        #region Wands Configuration
        // Wands (Fire) - Associated with creativity, passion, energy, and action
        private static void ConfigureWandsCard(TarotCard card, int value)
        {
            // Set properties based on card value
            switch (value)
            {
                case 1: // Ace of Wands
                    card.CardName = "Ace of Wands";
                    card.Description = "Pure creative potential. Your attacks have a chance to create a small explosion.";
                    card.PrimaryEffect = "Explosive Attacks";
                    card.SecondaryEffect = "Explosion Radius";
                    card.PrimaryEffectValue = 0.15f; // 15% chance
                    card.SecondaryEffectValue = 2.0f; // 2 unit radius
                    card.HasOnHitEffect = true;
                    card.DamageModifier = 1.0f;
                    break;
                    
                case 2: // Two of Wands
                    card.CardName = "Two of Wands";
                    card.Description = "Future planning. Reveals the location of the boss room on each level.";
                    card.PrimaryEffect = "Boss Reveal";
                    card.SecondaryEffect = "Planning";
                    card.PrimaryEffectValue = 1.0f; // Reveals boss
                    card.SecondaryEffectValue = 1.0f; // Has planning effect
                    card.IsPassive = true;
                    break;
                    
                case 3: // Three of Wands
                    card.CardName = "Three of Wands";
                    card.Description = "Expansion. Increases projectile size and piercing capability.";
                    card.PrimaryEffect = "Projectile Size";
                    card.SecondaryEffect = "Piercing";
                    card.PrimaryEffectValue = 1.5f; // 50% larger projectiles
                    card.SecondaryEffectValue = 1.0f; // Can pierce 1 enemy
                    card.IsPassive = true;
                    break;
                    
                case 4: // Four of Wands
                    card.CardName = "Four of Wands";
                    card.Description = "Celebration. Clearing a room restores 5 health and 10 mana.";
                    card.PrimaryEffect = "Room Clear Healing";
                    card.SecondaryEffect = "Room Clear Mana";
                    card.PrimaryEffectValue = 5.0f; // 5 health
                    card.SecondaryEffectValue = 10.0f; // 10 mana
                    card.HasOnRoomClearEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 5: // Five of Wands
                    card.CardName = "Five of Wands";
                    card.Description = "Conflict. Your shots have a chance to confuse enemies, making them attack each other.";
                    card.PrimaryEffect = "Confusion";
                    card.SecondaryEffect = "Confusion Duration";
                    card.PrimaryEffectValue = 0.2f; // 20% chance
                    card.SecondaryEffectValue = 3.0f; // 3 seconds
                    card.HasOnHitEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 6: // Six of Wands
                    card.CardName = "Six of Wands";
                    card.Description = "Victory. Killing enemies has a 20% chance to drop additional motes.";
                    card.PrimaryEffect = "Extra Motes";
                    card.SecondaryEffect = "Drop Chance";
                    card.PrimaryEffectValue = 3.0f; // 3 extra motes
                    card.SecondaryEffectValue = 0.2f; // 20% chance
                    card.HasOnKillEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 7: // Seven of Wands
                    card.CardName = "Seven of Wands";
                    card.Description = "Defensiveness. Taking damage has a chance to trigger a fire nova that damages nearby enemies.";
                    card.PrimaryEffect = "Fire Nova";
                    card.SecondaryEffect = "Nova Damage";
                    card.PrimaryEffectValue = 0.3f; // 30% chance on hit
                    card.SecondaryEffectValue = 12.0f; // 12 damage
                    card.HasOnHitEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 8: // Eight of Wands
                    card.CardName = "Eight of Wands";
                    card.Description = "Swift action. Increases projectile speed and fire rate.";
                    card.PrimaryEffect = "Projectile Speed";
                    card.SecondaryEffect = "Fire Rate";
                    card.PrimaryEffectValue = 1.3f; // 30% faster projectiles
                    card.SecondaryEffectValue = -0.15f; // 15% faster fire rate
                    card.CooldownModifier = -0.15f;
                    card.IsPassive = true;
                    break;
                    
                case 9: // Nine of Wands
                    card.CardName = "Nine of Wands";
                    card.Description = "Resilience. Increases max health and provides a shield that regenerates between rooms.";
                    card.PrimaryEffect = "Max Health";
                    card.SecondaryEffect = "Shield";
                    card.PrimaryEffectValue = 10.0f; // +10 max health
                    card.SecondaryEffectValue = 15.0f; // 15 shield points
                    card.HealthModifier = 10.0f;
                    card.HasOnRoomClearEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 10: // Ten of Wands
                    card.CardName = "Ten of Wands";
                    card.Description = "Burden. Decreases movement speed but greatly increases damage.";
                    card.PrimaryEffect = "Damage Boost";
                    card.SecondaryEffect = "Speed Penalty";
                    card.PrimaryEffectValue = 4.0f; // +4 damage
                    card.SecondaryEffectValue = -0.2f; // -20% speed
                    card.DamageModifier = 4.0f;
                    card.SpeedModifier = -0.2f;
                    card.IsPassive = true;
                    break;
                    
                case 11: // Page of Wands
                    card.CardName = "Page of Wands";
                    card.Description = "Enthusiasm. Adds a fire dash ability that leaves a trail of flames.";
                    card.PrimaryEffect = "Fire Dash";
                    card.SecondaryEffect = "Flame Trail";
                    card.PrimaryEffectValue = 1.0f; // Dash ability
                    card.SecondaryEffectValue = 3.0f; // 3 damage per tick from flames
                    card.ActiveAbility = "FireDash";
                    card.IsPassive = false;
                    break;
                    
                case 12: // Knight of Wands
                    card.CardName = "Knight of Wands";
                    card.Description = "Action and adventure. Adds a charging attack that deals massive damage to the first enemy hit.";
                    card.PrimaryEffect = "Charge Attack";
                    card.SecondaryEffect = "Impact Damage";
                    card.PrimaryEffectValue = 1.0f; // Charge ability
                    card.SecondaryEffectValue = 3.0f; // 3x normal damage
                    card.ActiveAbility = "KnightCharge";
                    card.IsPassive = false;
                    break;
                    
                case 13: // Queen of Wands
                    card.CardName = "Queen of Wands";
                    card.Description = "Confidence and determination. Your attacks have a chance to charm enemies to fight for you.";
                    card.PrimaryEffect = "Charm";
                    card.SecondaryEffect = "Charm Duration";
                    card.PrimaryEffectValue = 0.1f; // 10% chance
                    card.SecondaryEffectValue = 10.0f; // 10 seconds duration
                    card.HasOnHitEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 14: // King of Wands
                    card.CardName = "King of Wands";
                    card.Description = "Leadership and vision. Adds a meteor shower ability that damages all enemies in the room.";
                    card.PrimaryEffect = "Meteor Shower";
                    card.SecondaryEffect = "Meteor Damage";
                    card.PrimaryEffectValue = 1.0f; // Ability activation
                    card.SecondaryEffectValue = 30.0f; // 30 damage per meteor
                    card.ActiveAbility = "MeteorShower";
                    card.IsPassive = false;
                    break;
            }
        }
        #endregion
        
        #region Cups Configuration
        // Cups (Water) - Associated with emotions, relationships, creativity, and intuition
        private static void ConfigureCupsCard(TarotCard card, int value)
        {
            // Set properties based on card value
            switch (value)
            {
                case 1: // Ace of Cups
                    card.CardName = "Ace of Cups";
                    card.Description = "Emotional awakening. Increases health regeneration and healing received.";
                    card.PrimaryEffect = "Health Regen";
                    card.SecondaryEffect = "Healing Bonus";
                    card.PrimaryEffectValue = 0.5f; // +0.5 health regen
                    card.SecondaryEffectValue = 0.25f; // +25% healing received
                    card.IsPassive = true;
                    break;
                    
                case 2: // Two of Cups
                    card.CardName = "Two of Cups";
                    card.Description = "Partnership. When playing co-op, you and your ally deal 20% more damage when near each other.";
                    card.PrimaryEffect = "Ally Damage";
                    card.SecondaryEffect = "Proximity Bonus";
                    card.PrimaryEffectValue = 0.2f; // +20% damage
                    card.SecondaryEffectValue = 5.0f; // 5 unit proximity
                    card.IsPassive = true;
                    break;
                    
                case 3: // Three of Cups
                    card.CardName = "Three of Cups";
                    card.Description = "Celebration. Completing a room has a chance to spawn a health orb.";
                    card.PrimaryEffect = "Health Orb";
                    card.SecondaryEffect = "Spawn Chance";
                    card.PrimaryEffectValue = 5.0f; // 5 health restored
                    card.SecondaryEffectValue = 0.3f; // 30% chance
                    card.HasOnRoomClearEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 4: // Four of Cups
                    card.CardName = "Four of Cups";
                    card.Description = "Contemplation. Reduces spell mana cost but slightly increases cooldowns.";
                    card.PrimaryEffect = "Mana Efficiency";
                    card.SecondaryEffect = "Cooldown Increase";
                    card.PrimaryEffectValue = -0.25f; // -25% mana cost
                    card.SecondaryEffectValue = 0.1f; // +10% cooldown
                    card.CooldownModifier = 0.1f;
                    card.IsPassive = true;
                    break;
                    
                case 5: // Five of Cups
                    card.CardName = "Five of Cups";
                    card.Description = "Disappointment. Taking fatal damage instead reduces you to 1 health and temporarily increases your damage.";
                    card.PrimaryEffect = "Death Prevention";
                    card.SecondaryEffect = "Vengeance";
                    card.PrimaryEffectValue = 1.0f; // Once per level
                    card.SecondaryEffectValue = 0.5f; // +50% damage for 10 seconds
                    card.IsPassive = true;
                    card.IsSingleUse = true; // Refreshes per level
                    break;
                    
                case 6: // Six of Cups
                    card.CardName = "Six of Cups";
                    card.Description = "Nostalgia. At the start of each level, gain a temporary copy of a random card from your previous run.";
                    card.PrimaryEffect = "Memory Card";
                    card.SecondaryEffect = "Duration";
                    card.PrimaryEffectValue = 1.0f; // One card
                    card.SecondaryEffectValue = 1.0f; // Lasts for one level
                    card.IsPassive = true;
                    break;
                    
                case 7: // Seven of Cups
                    card.CardName = "Seven of Cups";
                    card.Description = "Illusion. Your attacks have a chance to confuse enemies, making them see illusions and attack randomly.";
                    card.PrimaryEffect = "Confusion";
                    card.SecondaryEffect = "Illusion Duration";
                    card.PrimaryEffectValue = 0.15f; // 15% chance
                    card.SecondaryEffectValue = 4.0f; // 4 second duration
                    card.HasOnHitEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 8: // Eight of Cups
                    card.CardName = "Eight of Cups";
                    card.Description = "Abandonment. Gain increased movement speed and damage after clearing a room.";
                    card.PrimaryEffect = "Speed Boost";
                    card.SecondaryEffect = "Damage Boost";
                    card.PrimaryEffectValue = 0.2f; // +20% speed
                    card.SecondaryEffectValue = 2.0f; // +2 damage
                    card.HasOnRoomClearEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 9: // Nine of Cups
                    card.CardName = "Nine of Cups";
                    card.Description = "Wish fulfillment. Increases the quality of item drops and shop offerings.";
                    card.PrimaryEffect = "Better Loot";
                    card.SecondaryEffect = "Wish Chance";
                    card.PrimaryEffectValue = 1.0f; // Better loot
                    card.SecondaryEffectValue = 0.1f; // 10% chance for bonus item
                    card.IsPassive = true;
                    break;
                    
                case 10: // Ten of Cups
                    card.CardName = "Ten of Cups";
                    card.Description = "Harmony. All stats increase by 10%. In co-op, nearby allies also gain this bonus.";
                    card.PrimaryEffect = "Stat Bonus";
                    card.SecondaryEffect = "Ally Bonus";
                    card.PrimaryEffectValue = 0.1f; // +10% to all stats
                    card.SecondaryEffectValue = 0.1f; // Same bonus to allies
                    card.HealthModifier = 10.0f;
                    card.DamageModifier = 1.0f;
                    card.SpeedModifier = 0.1f;
                    card.CooldownModifier = -0.1f;
                    card.CriticalModifier = 0.1f;
                    card.IsPassive = true;
                    break;
                    
                case 11: // Page of Cups
                    card.CardName = "Page of Cups";
                    card.Description = "Intuition. Adds a water shield ability that absorbs damage and slows nearby enemies.";
                    card.PrimaryEffect = "Water Shield";
                    card.SecondaryEffect = "Slow Aura";
                    card.PrimaryEffectValue = 20.0f; // Absorbs 20 damage
                    card.SecondaryEffectValue = 0.3f; // 30% slow
                    card.ActiveAbility = "WaterShield";
                    card.IsPassive = false;
                    break;
                    
                case 12: // Knight of Cups
                    card.CardName = "Knight of Cups";
                    card.Description = "Romance. Adds a healing wave ability that restores health to you and nearby allies.";
                    card.PrimaryEffect = "Healing Wave";
                    card.SecondaryEffect = "Ally Healing";
                    card.PrimaryEffectValue = 20.0f; // Heals 20 health
                    card.SecondaryEffectValue = 15.0f; // Heals allies for 15
                    card.ActiveAbility = "HealingWave";
                    card.IsPassive = false;
                    break;
                    
                case 13: // Queen of Cups
                    card.CardName = "Queen of Cups";
                    card.Description = "Compassion. Your attacks have a chance to create healing orbs for you and your allies.";
                    card.PrimaryEffect = "Healing Orbs";
                    card.SecondaryEffect = "Orb Chance";
                    card.PrimaryEffectValue = 3.0f; // 3 health per orb
                    card.SecondaryEffectValue = 0.2f; // 20% chance
                    card.HasOnHitEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 14: // King of Cups
                    card.CardName = "King of Cups";
                    card.Description = "Emotional control. Adds a tidal wave ability that pushes enemies back and slows them.";
                    card.PrimaryEffect = "Tidal Wave";
                    card.SecondaryEffect = "Wave Force";
                    card.PrimaryEffectValue = 1.0f; // Ability activation
                    card.SecondaryEffectValue = 15.0f; // 15 force units
                    card.ActiveAbility = "TidalWave";
                    card.IsPassive = false;
                    break;
            }
        }
        #endregion
        
        #region Swords Configuration
        // Swords (Air) - Associated with intellect, challenges, conflict, and thought
        private static void ConfigureSwordsCard(TarotCard card, int value)
        {
            // Set properties based on card value
            switch (value)
            {
                case 1: // Ace of Swords
                    card.CardName = "Ace of Swords";
                    card.Description = "Mental clarity. Increases critical hit chance and reveals enemy weak points.";
                    card.PrimaryEffect = "Critical Chance";
                    card.SecondaryEffect = "Weak Point";
                    card.PrimaryEffectValue = 0.1f; // +10% crit chance
                    card.SecondaryEffectValue = 0.5f; // +50% crit damage
                    card.CriticalModifier = 0.1f;
                    card.IsPassive = true;
                    break;
                    
                case 2: // Two of Swords
                    card.CardName = "Two of Swords";
                    card.Description = "Difficult choice. After clearing a room, choose between health restoration or damage increase.";
                    card.PrimaryEffect = "Healing Choice";
                    card.SecondaryEffect = "Damage Choice";
                    card.PrimaryEffectValue = 10.0f; // 10 health
                    card.SecondaryEffectValue = 2.0f; // +2 damage
                    card.HasOnRoomClearEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 3: // Three of Swords
                    card.CardName = "Three of Swords";
                    card.Description = "Heartbreak. When your health drops below 30%, deal 50% more damage for 10 seconds.";
                    card.PrimaryEffect = "Pain Catalyst";
                    card.SecondaryEffect = "Vengeance Duration";
                    card.PrimaryEffectValue = 0.5f; // +50% damage
                    card.SecondaryEffectValue = 10.0f; // 10 second duration
                    card.IsPassive = true;
                    break;
                    
                case 4: // Four of Swords
                    card.CardName = "Four of Swords";
                    card.Description = "Rest and recovery. Standing still increases your health and mana regeneration.";
                    card.PrimaryEffect = "Rest Healing";
                    card.SecondaryEffect = "Rest Mana";
                    card.PrimaryEffectValue = 1.0f; // +1 health per second when still
                    card.SecondaryEffectValue = 5.0f; // +5 mana per second when still
                    card.IsPassive = true;
                    break;
                    
                case 5: // Five of Swords
                    card.CardName = "Five of Swords";
                    card.Description = "Conflict and defeat. Your attacks have a chance to weaken enemies, reducing their damage output.";
                    card.PrimaryEffect = "Weakness";
                    card.SecondaryEffect = "Weakness Duration";
                    card.PrimaryEffectValue = 0.25f; // -25% enemy damage
                    card.SecondaryEffectValue = 5.0f; // 5 second duration
                    card.HasOnHitEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 6: // Six of Swords
                    card.CardName = "Six of Swords";
                    card.Description = "Transition. Adds a teleport ability that allows you to quickly escape danger.";
                    card.PrimaryEffect = "Teleport";
                    card.SecondaryEffect = "Invulnerability";
                    card.PrimaryEffectValue = 10.0f; // 10 unit teleport
                    card.SecondaryEffectValue = 1.0f; // 1 second invulnerability
                    card.ActiveAbility = "Teleport";
                    card.IsPassive = false;
                    break;
                    
                case 7: // Seven of Swords
                    card.CardName = "Seven of Swords";
                    card.Description = "Deception. Your attacks have a chance to steal health from enemies.";
                    card.PrimaryEffect = "Life Steal";
                    card.SecondaryEffect = "Steal Chance";
                    card.PrimaryEffectValue = 0.2f; // 20% of damage dealt
                    card.SecondaryEffectValue = 0.25f; // 25% chance
                    card.HasOnHitEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 8: // Eight of Swords
                    card.CardName = "Eight of Swords";
                    card.Description = "Restriction. Enemy projectiles move 25% slower, and you take reduced damage from projectiles.";
                    card.PrimaryEffect = "Projectile Slow";
                    card.SecondaryEffect = "Projectile Defense";
                    card.PrimaryEffectValue = 0.25f; // 25% slower
                    card.SecondaryEffectValue = 0.3f; // 30% less damage
                    card.IsPassive = true;
                    break;
                    
                case 9: // Nine of Swords
                    card.CardName = "Nine of Swords";
                    card.Description = "Anxiety and nightmares. Your attacks have a chance to inflict fear, causing enemies to flee temporarily.";
                    card.PrimaryEffect = "Fear";
                    card.SecondaryEffect = "Fear Duration";
                    card.PrimaryEffectValue = 0.2f; // 20% chance
                    card.SecondaryEffectValue = 3.0f; // 3 second duration
                    card.HasOnHitEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 10: // Ten of Swords
                    card.CardName = "Ten of Swords";
                    card.Description = "Painful endings. Killing an enemy creates a vortex that damages nearby enemies.";
                    card.PrimaryEffect = "Death Vortex";
                    card.SecondaryEffect = "Vortex Damage";
                    card.PrimaryEffectValue = 3.0f; // 3 unit radius
                    card.SecondaryEffectValue = 15.0f; // 15 damage
                    card.HasOnKillEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 11: // Page of Swords
                    card.CardName = "Page of Swords";
                    card.Description = "Curiosity. Adds a whirlwind ability that deflects enemy projectiles back at them.";
                    card.PrimaryEffect = "Whirlwind";
                    card.SecondaryEffect = "Deflection";
                    card.PrimaryEffectValue = 1.0f; // Ability activation
                    card.SecondaryEffectValue = 3.0f; // 3 second duration
                    card.ActiveAbility = "Whirlwind";
                    card.IsPassive = false;
                    break;
                    
                case 12: // Knight of Swords
                    card.CardName = "Knight of Swords";
                    card.Description = "Swiftness. Adds a blade dash ability that allows you to rush through enemies, damaging all in your path.";
                    card.PrimaryEffect = "Blade Dash";
                    card.SecondaryEffect = "Dash Damage";
                    card.PrimaryEffectValue = 10.0f; // 10 unit dash
                    card.SecondaryEffectValue = 20.0f; // 20 damage
                    card.ActiveAbility = "BladeDash";
                    card.IsPassive = false;
                    break;
                    
                case 13: // Queen of Swords
                    card.CardName = "Queen of Swords";
                    card.Description = "Clear perception. Your critical hits apply a bleed effect that damages enemies over time.";
                    card.PrimaryEffect = "Bleed Effect";
                    card.SecondaryEffect = "Bleed Duration";
                    card.PrimaryEffectValue = 2.0f; // 2 damage per tick
                    card.SecondaryEffectValue = 5.0f; // 5 second duration
                    card.HasOnHitEffect = true;
                    card.CriticalModifier = 0.05f;
                    card.IsPassive = true;
                    break;
                    
                case 14: // King of Swords
                    card.CardName = "King of Swords";
                    card.Description = "Mental clarity. Adds a blade storm ability that summons whirling blades to attack all enemies.";
                    card.PrimaryEffect = "Blade Storm";
                    card.SecondaryEffect = "Storm Damage";
                    card.PrimaryEffectValue = 1.0f; // Ability activation
                    card.SecondaryEffectValue = 40.0f; // 40 total damage
                    card.ActiveAbility = "BladeStorm";
                    card.IsPassive = false;
                    break;
            }
        }
        #endregion
        
        #region Pentacles Configuration
        // Pentacles (Earth) - Associated with material world, finances, work, and physical body
        private static void ConfigurePentaclesCard(TarotCard card, int value)
        {
            // Set properties based on card value
            switch (value)
            {
                case 1: // Ace of Pentacles
                    card.CardName = "Ace of Pentacles";
                    card.Description = "New prosperity. Increases the number of motes collected from enemies and chests.";
                    card.PrimaryEffect = "Mote Bonus";
                    card.SecondaryEffect = "Lucky Find";
                    card.PrimaryEffectValue = 0.25f; // +25% motes
                    card.SecondaryEffectValue = 0.1f; // 10% chance for extra item
                    card.IsPassive = true;
                    break;
                    
                case 2: // Two of Pentacles
                    card.CardName = "Two of Pentacles";
                    card.Description = "Balance. When you take damage, gain a temporary damage boost.";
                    card.PrimaryEffect = "Balanced Response";
                    card.SecondaryEffect = "Response Duration";
                    card.PrimaryEffectValue = 0.2f; // +20% damage
                    card.SecondaryEffectValue = 5.0f; // 5 second duration
                    card.IsPassive = true;
                    break;
                    
                case 3: // Three of Pentacles
                    card.CardName = "Three of Pentacles";
                    card.Description = "Collaboration. In co-op play, you and nearby allies deal 15% more damage.";
                    card.PrimaryEffect = "Team Damage";
                    card.SecondaryEffect = "Proximity";
                    card.PrimaryEffectValue = 0.15f; // +15% damage
                    card.SecondaryEffectValue = 5.0f; // 5 unit radius
                    card.IsPassive = true;
                    break;
                    
                case 4: // Four of Pentacles
                    card.CardName = "Four of Pentacles";
                    card.Description = "Conservation. When below 50% health, you take 25% less damage.";
                    card.PrimaryEffect = "Damage Reduction";
                    card.SecondaryEffect = "Health Threshold";
                    card.PrimaryEffectValue = 0.25f; // 25% damage reduction
                    card.SecondaryEffectValue = 0.5f; // 50% health threshold
                    card.IsPassive = true;
                    break;
                    
                case 5: // Five of Pentacles
                    card.CardName = "Five of Pentacles";
                    card.Description = "Hardship. When below 30% health, your attack speed and movement speed increase.";
                    card.PrimaryEffect = "Desperate Speed";
                    card.SecondaryEffect = "Attack Haste";
                    card.PrimaryEffectValue = 0.3f; // +30% movement speed
                    card.SecondaryEffectValue = -0.2f; // 20% faster attacks
                    card.IsPassive = true;
                    break;
                    
                case 6: // Six of Pentacles
                    card.CardName = "Six of Pentacles";
                    card.Description = "Generosity. Killing enemies has a chance to drop health orbs that heal for 5 health.";
                    card.PrimaryEffect = "Health Drops";
                    card.SecondaryEffect = "Drop Chance";
                    card.PrimaryEffectValue = 5.0f; // 5 health
                    card.SecondaryEffectValue = 0.15f; // 15% chance
                    card.HasOnKillEffect = true;
                    card.IsPassive = true;
                    break;
                    
                case 7: // Seven of Pentacles
                    card.CardName = "Seven of Pentacles";
                    card.Description = "Patience. The longer you stay in a room, the more bonus damage you deal.";
                    card.PrimaryEffect = "Growing Strength";
                    card.SecondaryEffect = "Max Bonus";
                    card.PrimaryEffectValue = 0.05f; // +5% damage per 10 seconds
                    card.SecondaryEffectValue = 0.5f; // Up to +50% damage
                    card.IsPassive = true;
                    break;
                    
                case 8: // Eight of Pentacles
                    card.CardName = "Eight of Pentacles";
                    card.Description = "Craftsmanship. Increases the effects of all your other tarot cards by 10%.";
                    card.PrimaryEffect = "Card Synergy";
                    card.SecondaryEffect = "Enhancement";
                    card.PrimaryEffectValue = 0.1f; // +10% effect
                    card.SecondaryEffectValue = 1.0f; // Applied to all cards
                    card.ComboEffect = "Enhances all other card effects";
                    card.ComboThreshold = 1; // Works with at least 1 other card
                    card.IsPassive = true;
                    break;
                    
                case 9: // Nine of Pentacles
                    card.CardName = "Nine of Pentacles";
                    card.Description = "Self-sufficiency. Increases max health and reduces damage taken when standing still.";
                    card.PrimaryEffect = "Max Health";
                    card.SecondaryEffect = "Defense Stance";
                    card.PrimaryEffectValue = 15.0f; // +15 max health
                    card.SecondaryEffectValue = 0.3f; // 30% damage reduction when still
                    card.HealthModifier = 15.0f;
                    card.IsPassive = true;
                    break;
                    
                case 10: // Ten of Pentacles
                    card.CardName = "Ten of Pentacles";
                    card.Description = "Legacy. Increases the chance of finding rare items and tarot cards.";
                    card.PrimaryEffect = "Item Rarity";
                    card.SecondaryEffect = "Card Rarity";
                    card.PrimaryEffectValue = 0.2f; // +20% rare item chance
                    card.SecondaryEffectValue = 0.2f; // +20% rare card chance
                    card.IsPassive = true;
                    break;
                    
                case 11: // Page of Pentacles
                    card.CardName = "Page of Pentacles";
                    card.Description = "Opportunity. Adds an ability to create a pentacle that attracts nearby motes and items.";
                    card.PrimaryEffect = "Mote Magnet";
                    card.SecondaryEffect = "Attraction Range";
                    card.PrimaryEffectValue = 1.0f; // Ability activation
                    card.SecondaryEffectValue = 10.0f; // 10 unit range
                    card.ActiveAbility = "MoteMagnet";
                    card.IsPassive = false;
                    break;
                    
                case 12: // Knight of Pentacles
                    card.CardName = "Knight of Pentacles";
                    card.Description = "Reliability. Adds an earth shield ability that reduces damage taken and reflects projectiles.";
                    card.PrimaryEffect = "Earth Shield";
                    card.SecondaryEffect = "Reflection";
                    card.PrimaryEffectValue = 30.0f; // 30 damage absorption
                    card.SecondaryEffectValue = 0.5f; // 50% reflection chance
                    card.ActiveAbility = "EarthShield";
                    card.IsPassive = false;
                    break;
                    
                case 13: // Queen of Pentacles
                    card.CardName = "Queen of Pentacles";
                    card.Description = "Nurturing. Periodically creates healing plant nodes that restore health when touched.";
                    card.PrimaryEffect = "Healing Plants";
                    card.SecondaryEffect = "Plant Healing";
                    card.PrimaryEffectValue = 15.0f; // 15 second interval
                    card.SecondaryEffectValue = 5.0f; // 5 health per plant
                    card.IsPassive = true;
                    break;
                    
                case 14: // King of Pentacles
                    card.CardName = "King of Pentacles";
                    card.Description = "Wealth. Adds an earthquake ability that damages all enemies and has a chance to drop extra motes.";
                    card.PrimaryEffect = "Earthquake";
                    card.SecondaryEffect = "Mote Bonus";
                    card.PrimaryEffectValue = 25.0f; // 25 damage
                    card.SecondaryEffectValue = 20.0f; // 20 bonus motes
                    card.ActiveAbility = "Earthquake";
                    card.IsPassive = false;
                    break;
            }
        }
        #endregion
        
        // Create a complete set of Minor Arcana
        public static List<TarotCard> CreateAllMinorArcana()
        {
            List<TarotCard> allMinorArcana = new List<TarotCard>();
            
            // Add all four suits
            allMinorArcana.AddRange(CreateMinorArcanaSuit(TarotCard.CardSuit.Wands));
            allMinorArcana.AddRange(CreateMinorArcanaSuit(TarotCard.CardSuit.Cups));
            allMinorArcana.AddRange(CreateMinorArcanaSuit(TarotCard.CardSuit.Swords));
            allMinorArcana.AddRange(CreateMinorArcanaSuit(TarotCard.CardSuit.Pentacles));
            
            return allMinorArcana;
        }
    }
}