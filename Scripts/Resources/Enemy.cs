using Godot;
using System;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources
{
    [GlobalClass]
    public partial class Enemy : Resource
    {
        public enum EnemyType { Basic, Elite, Boss, Minion }
        public enum AttackPattern { Melee, Ranged, AOE, Summoner, Support }
        public enum MovementType { Stationary, Chasing, Patrolling, Teleporting, Flanking }
        
        [Export] public string EnemyName { get; set; }
        [Export] public string Description { get; set; }
        [Export] public EnemyType Type { get; set; } = EnemyType.Basic;
        [Export] public AttackPattern Pattern { get; set; } = AttackPattern.Melee;
        [Export] public MovementType Movement { get; set; } = MovementType.Chasing;
        
        // Base stats
        [Export] public float BaseHealth { get; set; } = 100.0f;
        [Export] public float BaseDamage { get; set; } = 10.0f;
        [Export] public float MoveSpeed { get; set; } = 150.0f;
        [Export] public float AttackSpeed { get; set; } = 1.0f; // Attacks per second
        [Export] public float DetectionRange { get; set; } = 10.0f;
        [Export] public float AttackRange { get; set; } = 1.5f;
        
        // Visual and audio
        [Export] public string SpritePath { get; set; } = "";
        [Export] public string DeathSound { get; set; } = "";
        [Export] public string AttackSound { get; set; } = "";
        [Export] public string AlertSound { get; set; } = "";
        
        // Combat properties
        [Export] public bool CanTakeCover { get; set; } = false;
        [Export] public bool HasArmor { get; set; } = false;
        [Export] public float ArmorValue { get; set; } = 0.0f; // Damage reduction percentage
        [Export] public bool IsRanged { get; set; } = false;
        [Export] public string ProjectileScene { get; set; } = "";
        [Export] public float ProjectileSpeed { get; set; } = 10.0f;
        
        // Special abilities
        [Export] public bool HasSpecialAttack { get; set; } = false;
        [Export] public string SpecialAttackName { get; set; } = "";
        [Export] public float SpecialAttackCooldown { get; set; } = 10.0f;
        [Export] public float SpecialAttackDamage { get; set; } = 20.0f;
        [Export] public float SpecialAttackRange { get; set; } = 5.0f;
        
        // Drop rates
        [Export] public float MotesDropAmount { get; set; } = 10.0f;
        [Export] public float WeaponDropChance { get; set; } = 0.05f;
        [Export] public float TarotDropChance { get; set; } = 0.02f;
        [Export] public float MutagenDropChance { get; set; } = 0.01f;
        
        // AI behavior parameters
        [Export] public float AggressionLevel { get; set; } = 0.5f; // 0 = passive, 1 = very aggressive
        [Export] public float FleeHealthThreshold { get; set; } = 0.2f; // Flee when health below this percentage
        [Export] public float GroupingBehavior { get; set; } = 0.0f; // 0 = solitary, 1 = pack mentality
        
        // Level scaling parameters
        [Export] public float HealthScalingPerLevel { get; set; } = 0.1f; // Health increases by this % per level
        [Export] public float DamageScalingPerLevel { get; set; } = 0.1f; // Damage increases by this % per level
        
        // Get scaled health and damage based on level
        public float GetScaledHealth(int level)
        {
            return BaseHealth * (1 + (level - 1) * HealthScalingPerLevel);
        }
        
        public float GetScaledDamage(int level)
        {
            return BaseDamage * (1 + (level - 1) * DamageScalingPerLevel);
        }
        
        // Get enemy "threat level" - a composite score of its combat capabilities
        public float GetThreatLevel()
        {
            float threatScore = BaseHealth / 50.0f + BaseDamage / 5.0f + AttackSpeed + MoveSpeed / 50.0f;
            
            if (HasSpecialAttack) threatScore += SpecialAttackDamage / 10.0f;
            if (HasArmor) threatScore += ArmorValue * 5.0f;
            if (IsRanged) threatScore += AttackRange / 3.0f;
            
            switch (Type)
            {
                case EnemyType.Elite: threatScore *= 1.5f; break;
                case EnemyType.Boss: threatScore *= 3.0f; break;
                case EnemyType.Minion: threatScore *= 0.7f; break;
            }
            
            return threatScore;
        }
        
        // Serialization methods for JSON export/import
        public Dictionary<string, Variant> Serialize()
        {
            var data = new Dictionary<string, Variant>();
            data["enemyName"] = EnemyName;
            data["description"] = Description;
            data["type"] = (int)Type;
            data["pattern"] = (int)Pattern;
            data["movement"] = (int)Movement;
            
            data["baseHealth"] = BaseHealth;
            data["baseDamage"] = BaseDamage;
            data["moveSpeed"] = MoveSpeed;
            data["attackSpeed"] = AttackSpeed;
            data["detectionRange"] = DetectionRange;
            data["attackRange"] = AttackRange;
            
            data["spritePath"] = SpritePath;
            data["deathSound"] = DeathSound;
            data["attackSound"] = AttackSound;
            data["alertSound"] = AlertSound;
            
            data["canTakeCover"] = CanTakeCover;
            data["hasArmor"] = HasArmor;
            data["armorValue"] = ArmorValue;
            data["isRanged"] = IsRanged;
            data["projectileScene"] = ProjectileScene;
            data["projectileSpeed"] = ProjectileSpeed;
            
            data["hasSpecialAttack"] = HasSpecialAttack;
            data["specialAttackName"] = SpecialAttackName;
            data["specialAttackCooldown"] = SpecialAttackCooldown;
            data["specialAttackDamage"] = SpecialAttackDamage;
            data["specialAttackRange"] = SpecialAttackRange;
            
            data["motesDropAmount"] = MotesDropAmount;
            data["weaponDropChance"] = WeaponDropChance;
            data["tarotDropChance"] = TarotDropChance;
            data["mutagenDropChance"] = MutagenDropChance;
            
            data["aggressionLevel"] = AggressionLevel;
            data["fleeHealthThreshold"] = FleeHealthThreshold;
            data["groupingBehavior"] = GroupingBehavior;
            
            data["healthScalingPerLevel"] = HealthScalingPerLevel;
            data["damageScalingPerLevel"] = DamageScalingPerLevel;
            
            return data;
        }
        
        public void Deserialize(Dictionary<string, Variant> data)
        {
            if (data.TryGetValue("enemyName", out var enemyName))
                EnemyName = enemyName.AsString();
                
            if (data.TryGetValue("description", out var description))
                Description = description.AsString();
                
            if (data.TryGetValue("type", out var type))
                Type = (EnemyType)type.AsInt32();
                
            if (data.TryGetValue("pattern", out var pattern))
                Pattern = (AttackPattern)pattern.AsInt32();
                
            if (data.TryGetValue("movement", out var movement))
                Movement = (MovementType)movement.AsInt32();
                
            if (data.TryGetValue("baseHealth", out var baseHealth))
                BaseHealth = (float)baseHealth.AsDouble();
                
            if (data.TryGetValue("baseDamage", out var baseDamage))
                BaseDamage = (float)baseDamage.AsDouble();
                
            if (data.TryGetValue("moveSpeed", out var moveSpeed))
                MoveSpeed = (float)moveSpeed.AsDouble();
                
            if (data.TryGetValue("attackSpeed", out var attackSpeed))
                AttackSpeed = (float)attackSpeed.AsDouble();
                
            if (data.TryGetValue("detectionRange", out var detectionRange))
                DetectionRange = (float)detectionRange.AsDouble();
                
            if (data.TryGetValue("attackRange", out var attackRange))
                AttackRange = (float)attackRange.AsDouble();
                
            if (data.TryGetValue("spritePath", out var spritePath))
                SpritePath = spritePath.AsString();
                
            if (data.TryGetValue("deathSound", out var deathSound))
                DeathSound = deathSound.AsString();
                
            if (data.TryGetValue("attackSound", out var attackSound))
                AttackSound = attackSound.AsString();
                
            if (data.TryGetValue("alertSound", out var alertSound))
                AlertSound = alertSound.AsString();
                
            if (data.TryGetValue("canTakeCover", out var canTakeCover))
                CanTakeCover = canTakeCover.AsBool();
                
            if (data.TryGetValue("hasArmor", out var hasArmor))
                HasArmor = hasArmor.AsBool();
                
            if (data.TryGetValue("armorValue", out var armorValue))
                ArmorValue = (float)armorValue.AsDouble();
                
            if (data.TryGetValue("isRanged", out var isRanged))
                IsRanged = isRanged.AsBool();
                
            if (data.TryGetValue("projectileScene", out var projectileScene))
                ProjectileScene = projectileScene.AsString();
                
            if (data.TryGetValue("projectileSpeed", out var projectileSpeed))
                ProjectileSpeed = (float)projectileSpeed.AsDouble();
                
            if (data.TryGetValue("hasSpecialAttack", out var hasSpecialAttack))
                HasSpecialAttack = hasSpecialAttack.AsBool();
                
            if (data.TryGetValue("specialAttackName", out var specialAttackName))
                SpecialAttackName = specialAttackName.AsString();
                
            if (data.TryGetValue("specialAttackCooldown", out var specialAttackCooldown))
                SpecialAttackCooldown = (float)specialAttackCooldown.AsDouble();
                
            if (data.TryGetValue("specialAttackDamage", out var specialAttackDamage))
                SpecialAttackDamage = (float)specialAttackDamage.AsDouble();
                
            if (data.TryGetValue("specialAttackRange", out var specialAttackRange))
                SpecialAttackRange = (float)specialAttackRange.AsDouble();
                
            if (data.TryGetValue("motesDropAmount", out var motesDropAmount))
                MotesDropAmount = (float)motesDropAmount.AsDouble();
                
            if (data.TryGetValue("weaponDropChance", out var weaponDropChance))
                WeaponDropChance = (float)weaponDropChance.AsDouble();
                
            if (data.TryGetValue("tarotDropChance", out var tarotDropChance))
                TarotDropChance = (float)tarotDropChance.AsDouble();
                
            if (data.TryGetValue("mutagenDropChance", out var mutagenDropChance))
                MutagenDropChance = (float)mutagenDropChance.AsDouble();
                
            if (data.TryGetValue("aggressionLevel", out var aggressionLevel))
                AggressionLevel = (float)aggressionLevel.AsDouble();
                
            if (data.TryGetValue("fleeHealthThreshold", out var fleeHealthThreshold))
                FleeHealthThreshold = (float)fleeHealthThreshold.AsDouble();
                
            if (data.TryGetValue("groupingBehavior", out var groupingBehavior))
                GroupingBehavior = (float)groupingBehavior.AsDouble();
                
            if (data.TryGetValue("healthScalingPerLevel", out var healthScalingPerLevel))
                HealthScalingPerLevel = (float)healthScalingPerLevel.AsDouble();
                
            if (data.TryGetValue("damageScalingPerLevel", out var damageScalingPerLevel))
                DamageScalingPerLevel = (float)damageScalingPerLevel.AsDouble();
        }
    }
}
