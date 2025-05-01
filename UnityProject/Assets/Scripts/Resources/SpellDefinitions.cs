using System.Collections.Generic;
using UnityEngine;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.Resources
{
    // This class provides static methods to create spells
    // In a real project, you would create these as individual ScriptableObject assets
    public static class SpellDefinitions
    {
        #region Offensive Spells
        // Fireball - Classic fire projectile
        public static Spell CreateFireball()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Fireball";
            spell.Description = "Launches a ball of fire that explodes on impact, dealing area damage.";
            spell.Type = SpellType.Projectile;
            spell.Element = SpellElement.Fire;
            spell.ManaCost = 15f;
            spell.Cooldown = 3f;
            spell.CastTime = 0.3f;
            spell.RequiresTarget = false;
            spell.Range = 15f;
            spell.BaseDamage = 20f;
            spell.EffectDuration = 0.5f; // Quick explosion
            spell.AreaOfEffect = 3f; // Explosion radius
            
            // Add burning status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Burn,
                Power = 5f, // 5 damage per tick
                Duration = 3f, // 3 seconds
                TickRate = 1f, // Every second
                EffectColor = new Color(1f, 0.5f, 0f, 0.7f) // Orange
            });
            
            spell.SpellTags.Add("Damage");
            spell.SpellTags.Add("AoE");
            spell.SpellTags.Add("Fire");
            
            return spell;
        }
        
        // Ice Spike - Piercing ice projectile
        public static Spell CreateIceSpike()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Ice Spike";
            spell.Description = "Fires a piercing spike of ice that damages and slows enemies.";
            spell.Type = SpellType.Projectile;
            spell.Element = SpellElement.Ice;
            spell.ManaCost = 12f;
            spell.Cooldown = 2f;
            spell.CastTime = 0.2f;
            spell.RequiresTarget = false;
            spell.Range = 12f;
            spell.BaseDamage = 15f;
            spell.EffectDuration = 0.5f;
            
            // Add slowing status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Slow,
                Power = 0.3f, // 30% slow
                Duration = 4f, // 4 seconds
                EffectColor = new Color(0.5f, 0.8f, 1f, 0.7f) // Light blue
            });
            
            spell.SpellTags.Add("Damage");
            spell.SpellTags.Add("Control");
            spell.SpellTags.Add("Ice");
            
            return spell;
        }
        
        // Lightning Bolt - Chain lightning
        public static Spell CreateLightningBolt()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Lightning Bolt";
            spell.Description = "Calls down a bolt of lightning that chains between nearby enemies.";
            spell.Type = SpellType.Chain;
            spell.Element = SpellElement.Lightning;
            spell.ManaCost = 25f;
            spell.Cooldown = 5f;
            spell.CastTime = 0.5f;
            spell.RequiresTarget = true;
            spell.Range = 10f;
            spell.BaseDamage = 30f;
            spell.EffectDuration = 1f;
            spell.AreaOfEffect = 6f; // Chain range
            
            // Add shock status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Shock,
                Power = 4f, // 4 damage per tick
                Duration = 2f, // 2 seconds
                TickRate = 0.5f, // Every half second
                EffectColor = new Color(0.8f, 0.8f, 1f, 0.7f) // Light purple
            });
            
            spell.SpellTags.Add("Damage");
            spell.SpellTags.Add("Chain");
            spell.SpellTags.Add("Lightning");
            
            return spell;
        }
        
        // Poison Nova - Area poison effect
        public static Spell CreatePoisonNova()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Poison Nova";
            spell.Description = "Releases a wave of poison that damages enemies over time.";
            spell.Type = SpellType.Area;
            spell.Element = SpellElement.Shadow;
            spell.ManaCost = 20f;
            spell.Cooldown = 8f;
            spell.CastTime = 0.4f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self-centered
            spell.BaseDamage = 10f; // Initial damage
            spell.EffectDuration = 0.5f; // Nova expansion
            spell.AreaOfEffect = 8f; // Nova radius
            
            // Add poison status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Poison,
                Power = 6f, // 6 damage per tick
                Duration = 5f, // 5 seconds
                TickRate = 1f, // Every second
                EffectColor = new Color(0.3f, 0.8f, 0.3f, 0.7f) // Green
            });
            
            spell.SpellTags.Add("Damage");
            spell.SpellTags.Add("AoE");
            spell.SpellTags.Add("Poison");
            
            return spell;
        }
        
        // Mind Blast - Single target with confusion
        public static Spell CreateMindBlast()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Mind Blast";
            spell.Description = "Assaults an enemy's mind, causing damage and confusion.";
            spell.Type = SpellType.Projectile;
            spell.Element = SpellElement.Mind;
            spell.ManaCost = 18f;
            spell.Cooldown = 6f;
            spell.CastTime = 0.3f;
            spell.RequiresTarget = true;
            spell.Range = 10f;
            spell.BaseDamage = 25f;
            spell.EffectDuration = 0.2f;
            
            // Add confusion status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Confusion,
                Power = 1f, // Full confusion
                Duration = 4f, // 4 seconds
                EffectColor = new Color(0.8f, 0.3f, 0.8f, 0.7f) // Purple
            });
            
            spell.SpellTags.Add("Damage");
            spell.SpellTags.Add("Control");
            spell.SpellTags.Add("Mind");
            
            return spell;
        }
        
        // Void Rift - Persistent damage zone
        public static Spell CreateVoidRift()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Void Rift";
            spell.Description = "Creates a rift to the void that continuously damages enemies within it.";
            spell.Type = SpellType.Area;
            spell.Element = SpellElement.Void;
            spell.ManaCost = 30f;
            spell.Cooldown = 12f;
            spell.CastTime = 0.8f;
            spell.RequiresTarget = true;
            spell.Range = 12f;
            spell.BaseDamage = 8f; // Per tick
            spell.EffectDuration = 8f; // Lasts 8 seconds
            spell.AreaOfEffect = 4f; // Rift radius
            
            // Add weakness status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Weakness,
                Power = 0.25f, // 25% damage reduction
                Duration = 3f, // 3 seconds
                EffectColor = new Color(0.1f, 0.1f, 0.3f, 0.7f) // Dark blue
            });
            
            spell.SpellTags.Add("Damage");
            spell.SpellTags.Add("Zone");
            spell.SpellTags.Add("Void");
            
            return spell;
        }
        #endregion
        
        #region Defensive Spells
        // Arcane Shield - Basic protection
        public static Spell CreateArcaneShield()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Arcane Shield";
            spell.Description = "Creates a protective barrier that absorbs damage.";
            spell.Type = SpellType.Shield;
            spell.Element = SpellElement.Arcane;
            spell.ManaCost = 20f;
            spell.Cooldown = 10f;
            spell.CastTime = 0.5f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self cast
            spell.EffectDuration = 8f; // Shield duration
            
            // Add shield status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Shield,
                Power = 40f, // Absorbs 40 damage
                Duration = 8f, // 8 seconds
                EffectColor = new Color(0.6f, 0.4f, 1f, 0.7f) // Purple
            });
            
            spell.SpellTags.Add("Defense");
            spell.SpellTags.Add("Shield");
            spell.SpellTags.Add("Arcane");
            
            return spell;
        }
        
        // Healing Wave - Area healing
        public static Spell CreateHealingWave()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Healing Wave";
            spell.Description = "Releases a wave of healing energy that restores health to you and nearby allies.";
            spell.Type = SpellType.Area;
            spell.Element = SpellElement.Light;
            spell.ManaCost = 25f;
            spell.Cooldown = 8f;
            spell.CastTime = 0.6f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self-centered
            spell.BaseHealing = 30f;
            spell.EffectDuration = 0.5f;
            spell.AreaOfEffect = 6f; // Healing radius
            
            // Add regeneration status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Regeneration,
                Power = 5f, // 5 health per tick
                Duration = 4f, // 4 seconds
                TickRate = 1f, // Every second
                EffectColor = new Color(0.7f, 1f, 0.7f, 0.7f) // Light green
            });
            
            spell.SpellTags.Add("Healing");
            spell.SpellTags.Add("AoE");
            spell.SpellTags.Add("Light");
            
            return spell;
        }
        
        // Mana Surge - Mana restoration
        public static Spell CreateManaSurge()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Mana Surge";
            spell.Description = "Instantly restores mana and increases mana regeneration for a short time.";
            spell.Type = SpellType.Buff;
            spell.Element = SpellElement.Arcane;
            spell.ManaCost = 10f; // Low cost
            spell.Cooldown = 15f; // Long cooldown
            spell.CastTime = 0.3f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self cast
            spell.EffectDuration = 0.1f;
            
            // Add mana regeneration status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.ManaFlow,
                Power = 8f, // 8 mana per tick
                Duration = 5f, // 5 seconds
                TickRate = 1f, // Every second
                EffectColor = new Color(0.4f, 0.6f, 1f, 0.7f) // Blue
            });
            
            spell.SpellTags.Add("Utility");
            spell.SpellTags.Add("Mana");
            spell.SpellTags.Add("Arcane");
            
            return spell;
        }
        
        // Stone Skin - Damage reduction
        public static Spell CreateStoneSkin()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Stone Skin";
            spell.Description = "Hardens your skin like stone, reducing damage taken.";
            spell.Type = SpellType.Buff;
            spell.Element = SpellElement.Earth;
            spell.ManaCost = 15f;
            spell.Cooldown = 12f;
            spell.CastTime = 0.4f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self cast
            spell.EffectDuration = 10f; // Buff duration
            
            // This would use a custom effect to reduce damage taken
            // For demonstration, we'll use an invulnerability effect with low power
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Invulnerable, // In a real implementation, this would be a DamageReduction type
                Power = 0.3f, // 30% damage reduction
                Duration = 10f, // 10 seconds
                EffectColor = new Color(0.6f, 0.5f, 0.3f, 0.7f) // Brown
            });
            
            spell.SpellTags.Add("Defense");
            spell.SpellTags.Add("Buff");
            spell.SpellTags.Add("Earth");
            
            return spell;
        }
        
        // Reflect Barrier - Reflects projectiles
        public static Spell CreateReflectBarrier()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Reflect Barrier";
            spell.Description = "Creates a barrier that has a chance to reflect enemy projectiles back at them.";
            spell.Type = SpellType.Shield;
            spell.Element = SpellElement.Light;
            spell.ManaCost = 22f;
            spell.Cooldown = 15f;
            spell.CastTime = 0.5f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self cast
            spell.EffectDuration = 6f; // Barrier duration
            
            // Add reflection status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Reflect,
                Power = 0.7f, // 70% reflection chance
                Duration = 6f, // 6 seconds
                EffectColor = new Color(1f, 0.9f, 0.5f, 0.7f) // Gold
            });
            
            spell.SpellTags.Add("Defense");
            spell.SpellTags.Add("Reflect");
            spell.SpellTags.Add("Light");
            
            return spell;
        }
        #endregion
        
        #region Utility Spells
        // Blink - Short range teleport
        public static Spell CreateBlink()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Blink";
            spell.Description = "Teleports a short distance in the direction you're facing.";
            spell.Type = SpellType.Utility;
            spell.Element = SpellElement.Arcane;
            spell.ManaCost = 15f;
            spell.Cooldown = 5f;
            spell.CastTime = 0.1f; // Very quick cast
            spell.RequiresTarget = false;
            spell.Range = 8f; // Teleport distance
            spell.EffectDuration = 0.2f;
            
            spell.SpellTags.Add("Movement");
            spell.SpellTags.Add("Escape");
            spell.SpellTags.Add("Arcane");
            
            return spell;
        }
        
        // Time Slow - Slows everything around you
        public static Spell CreateTimeSlow()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Time Slow";
            spell.Description = "Slows down time in an area around you, affecting enemies, projectiles, and effects.";
            spell.Type = SpellType.Area;
            spell.Element = SpellElement.Time;
            spell.ManaCost = 30f;
            spell.Cooldown = 20f;
            spell.CastTime = 0.5f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self-centered
            spell.EffectDuration = 5f; // Time slow duration
            spell.AreaOfEffect = 8f; // Slow radius
            
            // Add slow status effect (heavily slowed)
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Slow,
                Power = 0.7f, // 70% slow
                Duration = 5f, // 5 seconds
                EffectColor = new Color(0.5f, 1f, 0.8f, 0.7f) // Teal
            });
            
            spell.SpellTags.Add("Control");
            spell.SpellTags.Add("Time");
            spell.SpellTags.Add("AoE");
            
            return spell;
        }
        
        // Invisibility - Stealth
        public static Spell CreateInvisibility()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Invisibility";
            spell.Description = "Makes you invisible to enemies for a short time. Attacking breaks the effect.";
            spell.Type = SpellType.Buff;
            spell.Element = SpellElement.Shadow;
            spell.ManaCost = 25f;
            spell.Cooldown = 25f;
            spell.CastTime = 0.6f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self cast
            spell.EffectDuration = 10f; // Invisibility duration
            
            // This would use a custom effect for invisibility
            // For demonstration, we'll use no status effect here
            
            spell.SpellTags.Add("Stealth");
            spell.SpellTags.Add("Escape");
            spell.SpellTags.Add("Shadow");
            
            return spell;
        }
        
        // Magnetic Pull - Attracts items
        public static Spell CreateMagneticPull()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Magnetic Pull";
            spell.Description = "Creates a magnetic field that pulls all nearby items toward you.";
            spell.Type = SpellType.Utility;
            spell.Element = SpellElement.Earth;
            spell.ManaCost = 10f;
            spell.Cooldown = 15f;
            spell.CastTime = 0.3f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self-centered
            spell.EffectDuration = 5f; // Pull duration
            spell.AreaOfEffect = 15f; // Pull radius
            
            spell.SpellTags.Add("Utility");
            spell.SpellTags.Add("Item");
            spell.SpellTags.Add("Earth");
            
            return spell;
        }
        
        // Spirit Vision - Reveals hidden things
        public static Spell CreateSpiritVision()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Spirit Vision";
            spell.Description = "Allows you to see hidden treasures, secret doors, and invisible enemies.";
            spell.Type = SpellType.Buff;
            spell.Element = SpellElement.Mind;
            spell.ManaCost = 15f;
            spell.Cooldown = 30f;
            spell.CastTime = 0.4f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self cast
            spell.EffectDuration = 20f; // Vision duration
            
            spell.SpellTags.Add("Detection");
            spell.SpellTags.Add("Utility");
            spell.SpellTags.Add("Mind");
            
            return spell;
        }
        #endregion
        
        #region Class-Specific Spells
        // Detective Spells
        
        // Interrupt Shot - Occult Detective signature spell
        public static Spell CreateInterruptShot()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Interrupt Shot";
            spell.Description = "A precisely aimed shot that interrupts enemy attacks and briefly stuns them.";
            spell.Type = SpellType.Projectile;
            spell.Element = SpellElement.Light;
            spell.ManaCost = 20f;
            spell.Cooldown = 8f;
            spell.CastTime = 0.2f;
            spell.RequiresTarget = true;
            spell.Range = 15f;
            spell.BaseDamage = 15f;
            spell.EffectDuration = 0.2f;
            
            // Add stun status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Stun,
                Power = 1f,
                Duration = 2f, // 2 second stun
                EffectColor = new Color(1f, 0.9f, 0.3f, 0.7f) // Yellow
            });
            
            spell.SpellTags.Add("Control");
            spell.SpellTags.Add("Stun");
            spell.SpellTags.Add("Detective");
            
            return spell;
        }
        
        // Rally Aura - Occult Detective signature spell
        public static Spell CreateRallyAura()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Rally Aura";
            spell.Description = "Bolsters the resolve of you and nearby allies, increasing damage output.";
            spell.Type = SpellType.Area;
            spell.Element = SpellElement.Light;
            spell.ManaCost = 25f;
            spell.Cooldown = 15f;
            spell.CastTime = 0.5f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self-centered
            spell.EffectDuration = 10f; // Aura duration
            spell.AreaOfEffect = 6f; // Aura radius
            
            // This would need a custom status effect for the damage boost
            // For demonstration, we won't add a status effect here
            
            spell.SpellTags.Add("Buff");
            spell.SpellTags.Add("AoE");
            spell.SpellTags.Add("Detective");
            
            return spell;
        }
        
        // Astral Projection - Apostate Medium signature spell
        public static Spell CreateAstralProjection()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Astral Projection";
            spell.Description = "Projects your spirit forward, allowing you to scout ahead safely. Can be ended early to teleport to that location.";
            spell.Type = SpellType.Utility;
            spell.Element = SpellElement.Cosmic;
            spell.ManaCost = 30f;
            spell.Cooldown = 25f;
            spell.CastTime = 0.7f;
            spell.RequiresTarget = false;
            spell.Range = 20f; // Projection distance
            spell.EffectDuration = 10f; // Max projection time
            
            spell.SpellTags.Add("Scout");
            spell.SpellTags.Add("Teleport");
            spell.SpellTags.Add("Medium");
            
            return spell;
        }
        
        // Spirit Shield - Apostate Medium signature spell
        public static Spell CreateSpiritShield()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Spirit Shield";
            spell.Description = "Summons protective spirits that absorb damage and confuse enemies who attack you.";
            spell.Type = SpellType.Shield;
            spell.Element = SpellElement.Cosmic;
            spell.ManaCost = 25f;
            spell.Cooldown = 15f;
            spell.CastTime = 0.5f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self cast
            spell.EffectDuration = 12f; // Shield duration
            
            // Add shield status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Shield,
                Power = 50f, // Absorbs 50 damage
                Duration = 12f, // 12 seconds
                EffectColor = new Color(0.7f, 0.7f, 1f, 0.7f) // Light purple
            });
            
            // When shield is hit, it would apply confusion to the attacker
            // This would need custom implementation
            
            spell.SpellTags.Add("Defense");
            spell.SpellTags.Add("Spirit");
            spell.SpellTags.Add("Medium");
            
            return spell;
        }
        
        // Fear Shout - Irredeemable Debtor signature spell
        public static Spell CreateFearShout()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Fear Shout";
            spell.Description = "A terrifying shout that causes nearby enemies to flee in terror.";
            spell.Type = SpellType.Area;
            spell.Element = SpellElement.Mind;
            spell.ManaCost = 20f;
            spell.Cooldown = 12f;
            spell.CastTime = 0.3f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self-centered
            spell.BaseDamage = 10f; // Some damage
            spell.EffectDuration = 0.5f;
            spell.AreaOfEffect = 8f; // Fear radius
            
            // Add fear status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Fear,
                Power = 1f,
                Duration = 5f, // 5 seconds of fear
                EffectColor = new Color(0.8f, 0.2f, 0.2f, 0.7f) // Red
            });
            
            spell.SpellTags.Add("Control");
            spell.SpellTags.Add("Fear");
            spell.SpellTags.Add("Debtor");
            
            return spell;
        }
        
        // Berserker Rage - Irredeemable Debtor signature spell
        public static Spell CreateBerserkerRage()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Berserker Rage";
            spell.Description = "Sacrifice health to enter a rage state, greatly increasing damage output.";
            spell.Type = SpellType.Buff;
            spell.Element = SpellElement.Blood;
            spell.ManaCost = 15f;
            spell.Cooldown = 20f;
            spell.CastTime = 0.4f;
            spell.RequiresTarget = false;
            spell.Range = 0f; // Self cast
            spell.EffectDuration = 8f; // Rage duration
            
            // This would need a custom status effect for the damage boost and health sacrifice
            // For demonstration, we won't add a status effect here
            
            spell.SpellTags.Add("Buff");
            spell.SpellTags.Add("Blood");
            spell.SpellTags.Add("Debtor");
            
            return spell;
        }
        
        // Slow Time Field - Reclusive Archivist signature spell
        public static Spell CreateSlowTimeField()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Slow Time Field";
            spell.Description = "Creates a field where time flows slower, affecting enemy movement and attacks.";
            spell.Type = SpellType.Area;
            spell.Element = SpellElement.Time;
            spell.ManaCost = 35f;
            spell.Cooldown = 25f;
            spell.CastTime = 0.6f;
            spell.RequiresTarget = true;
            spell.Range = 12f;
            spell.EffectDuration = 8f; // Field duration
            spell.AreaOfEffect = 5f; // Field radius
            
            // Add slow status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Slow,
                Power = 0.5f, // 50% slow
                Duration = 8f, // 8 seconds
                EffectColor = new Color(0.4f, 0.8f, 0.8f, 0.7f) // Teal
            });
            
            spell.SpellTags.Add("Control");
            spell.SpellTags.Add("Time");
            spell.SpellTags.Add("Archivist");
            
            return spell;
        }
        
        // Knowledge Beam - Reclusive Archivist signature spell
        public static Spell CreateKnowledgeBeam()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Knowledge Beam";
            spell.Description = "Focuses centuries of arcane knowledge into a devastating beam that penetrates enemies.";
            spell.Type = SpellType.Beam;
            spell.Element = SpellElement.Arcane;
            spell.ManaCost = 5f; // Per second of channeling
            spell.Cooldown = 3f;
            spell.CastTime = 0.2f;
            spell.RequiresTarget = false;
            spell.Range = 15f; // Beam length
            spell.BaseDamage = 20f; // Damage per second
            spell.EffectDuration = 3f; // Max channel time
            
            spell.SpellTags.Add("Damage");
            spell.SpellTags.Add("Beam");
            spell.SpellTags.Add("Archivist");
            
            return spell;
        }
        
        // Charm Grenade - Seditious Orator signature spell
        public static Spell CreateCharmGrenade()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Charm Grenade";
            spell.Description = "Throws a grenade that releases a cloud of charming gas, temporarily turning enemies into allies.";
            spell.Type = SpellType.Projectile;
            spell.Element = SpellElement.Mind;
            spell.ManaCost = 30f;
            spell.Cooldown = 20f;
            spell.CastTime = 0.4f;
            spell.RequiresTarget = true;
            spell.Range = 10f;
            spell.EffectDuration = 0.5f; // Explosion time
            spell.AreaOfEffect = 4f; // Gas cloud radius
            
            // Add charm status effect
            spell.StatusEffects.Add(new StatusEffectData
            {
                Type = StatusEffectType.Charm,
                Power = 1f,
                Duration = 6f, // 6 seconds of charm
                EffectColor = new Color(1f, 0.5f, 0.8f, 0.7f) // Pink
            });
            
            spell.SpellTags.Add("Control");
            spell.SpellTags.Add("Charm");
            spell.SpellTags.Add("Orator");
            
            return spell;
        }
        
        // Critical Trick Shot - Seditious Orator signature spell
        public static Spell CreateCriticalTrickShot()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Critical Trick Shot";
            spell.Description = "A precisely aimed shot that is guaranteed to critically hit and can bounce to additional targets.";
            spell.Type = SpellType.Chain;
            spell.Element = SpellElement.Light;
            spell.ManaCost = 25f;
            spell.Cooldown = 15f;
            spell.CastTime = 0.3f;
            spell.RequiresTarget = true;
            spell.Range = 12f;
            spell.BaseDamage = 40f; // High base damage for the crit
            spell.EffectDuration = 0.5f;
            spell.AreaOfEffect = 8f; // Bounce range
            
            spell.SpellTags.Add("Damage");
            spell.SpellTags.Add("Critical");
            spell.SpellTags.Add("Orator");
            
            return spell;
        }
        
        // Badge Ward Totem - Phantom Constable signature spell
        public static Spell CreateBadgeWardTotem()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Badge Ward Totem";
            spell.Description = "Places a totem that creates a zone of order, reducing incoming damage and reflecting projectiles.";
            spell.Type = SpellType.Trap;
            spell.Element = SpellElement.Light;
            spell.ManaCost = 30f;
            spell.Cooldown = 25f;
            spell.CastTime = 0.8f;
            spell.RequiresTarget = true;
            spell.Range = 8f;
            spell.EffectDuration = 15f; // Totem duration
            spell.AreaOfEffect = 5f; // Ward radius
            
            // Would need custom implementation for the damage reduction zone
            // For demonstration, we won't add a status effect here
            
            spell.SpellTags.Add("Defense");
            spell.SpellTags.Add("Totem");
            spell.SpellTags.Add("Constable");
            
            return spell;
        }
        
        // Revive Tether - Phantom Constable signature spell
        public static Spell CreateReviveTether()
        {
            Spell spell = ScriptableObject.CreateInstance<Spell>();
            spell.SpellName = "Revive Tether";
            spell.Description = "Creates a tether that can instantly revive you or an ally upon death, then disappears.";
            spell.Type = SpellType.Buff;
            spell.Element = SpellElement.Light;
            spell.ManaCost = 40f;
            spell.Cooldown = 60f; // Very long cooldown
            spell.CastTime = 1.0f;
            spell.RequiresTarget = true; // Can target self or ally
            spell.Range = 10f;
            spell.EffectDuration = 30f; // Tether duration if not triggered
            
            // Would need custom implementation for the resurrection effect
            // For demonstration, we won't add a status effect here
            
            spell.SpellTags.Add("Revive");
            spell.SpellTags.Add("Support");
            spell.SpellTags.Add("Constable");
            
            return spell;
        }
        #endregion
        
        // Create all spells
        public static List<Spell> CreateAllSpells()
        {
            List<Spell> spells = new List<Spell>();
            
            // Offensive spells
            spells.Add(CreateFireball());
            spells.Add(CreateIceSpike());
            spells.Add(CreateLightningBolt());
            spells.Add(CreatePoisonNova());
            spells.Add(CreateMindBlast());
            spells.Add(CreateVoidRift());
            
            // Defensive spells
            spells.Add(CreateArcaneShield());
            spells.Add(CreateHealingWave());
            spells.Add(CreateManaSurge());
            spells.Add(CreateStoneSkin());
            spells.Add(CreateReflectBarrier());
            
            // Utility spells
            spells.Add(CreateBlink());
            spells.Add(CreateTimeSlow());
            spells.Add(CreateInvisibility());
            spells.Add(CreateMagneticPull());
            spells.Add(CreateSpiritVision());
            
            // Class-specific spells
            spells.Add(CreateInterruptShot());
            spells.Add(CreateRallyAura());
            spells.Add(CreateAstralProjection());
            spells.Add(CreateSpiritShield());
            spells.Add(CreateFearShout());
            spells.Add(CreateBerserkerRage());
            spells.Add(CreateSlowTimeField());
            spells.Add(CreateKnowledgeBeam());
            spells.Add(CreateCharmGrenade());
            spells.Add(CreateCriticalTrickShot());
            spells.Add(CreateBadgeWardTotem());
            spells.Add(CreateReviveTether());
            
            return spells;
        }
        
        // Create spells for a specific class
        public static List<Spell> CreateSpellsForClass(string className)
        {
            List<Spell> classSpells = new List<Spell>();
            
            // Add general spells that all classes can use
            classSpells.Add(CreateFireball());
            classSpells.Add(CreateIceSpike());
            classSpells.Add(CreateArcaneShield());
            classSpells.Add(CreateHealingWave());
            classSpells.Add(CreateBlink());
            
            // Add class-specific spells
            switch (className)
            {
                case "OccultDetective":
                    classSpells.Add(CreateInterruptShot());
                    classSpells.Add(CreateRallyAura());
                    classSpells.Add(CreateTimeSlow());
                    classSpells.Add(CreateReflectBarrier());
                    break;
                    
                case "ApostateMedium":
                    classSpells.Add(CreateAstralProjection());
                    classSpells.Add(CreateSpiritShield());
                    classSpells.Add(CreateSpiritVision());
                    classSpells.Add(CreatePoisonNova());
                    break;
                    
                case "IrredeemableDebtor":
                    classSpells.Add(CreateFearShout());
                    classSpells.Add(CreateBerserkerRage());
                    classSpells.Add(CreateVoidRift());
                    classSpells.Add(CreateLightningBolt());
                    break;
                    
                case "ReclusiveArchivist":
                    classSpells.Add(CreateSlowTimeField());
                    classSpells.Add(CreateKnowledgeBeam());
                    classSpells.Add(CreateMagneticPull());
                    classSpells.Add(CreateManaSurge());
                    break;
                    
                case "SeditiousOrator":
                    classSpells.Add(CreateCharmGrenade());
                    classSpells.Add(CreateCriticalTrickShot());
                    classSpells.Add(CreateMindBlast());
                    classSpells.Add(CreateInvisibility());
                    break;
                    
                case "PhantomConstable":
                    classSpells.Add(CreateBadgeWardTotem());
                    classSpells.Add(CreateReviveTether());
                    classSpells.Add(CreateStoneSkin());
                    classSpells.Add(CreateHealingWave());
                    break;
            }
            
            return classSpells;
        }
    }
}