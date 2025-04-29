using Godot;
using System;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources
{
    [GlobalClass]
    public partial class Weapon : Resource
    {
        public enum WeaponType { Pistol, Rifle, Shotgun, Staff, Launcher, MeleeRelic }
        public enum RarityTier { Common, Uncommon, Rare, Epic, Mythic }
        
        [Export] public string WeaponName { get; set; }
        [Export] public string Description { get; set; }
        [Export] public WeaponType Type { get; set; } = WeaponType.Rifle;
        [Export] public RarityTier Rarity { get; set; } = RarityTier.Common;
        [Export] public string Epithet { get; set; } = ""; // Special name for mythic weapons
        
        // Basic weapon stats
        [Export] public float Damage { get; set; } = 10.0f;
        [Export] public float FireRate { get; set; } = 0.5f; // Seconds between shots
        [Export] public float ReloadTime { get; set; } = 1.5f;
        [Export] public int MagazineSize { get; set; } = 10;
        [Export] public float Range { get; set; } = 20.0f;
        [Export] public float Spread { get; set; } = 0.05f; // Angular spread in radians
        [Export] public float ProjectileSpeed { get; set; } = 20.0f;
        [Export] public int ProjectilesPerShot { get; set; } = 1; // For shotguns or special weapons
        [Export] public bool IsAutomatic { get; set; } = false;
        
        // Visuals and sound
        [Export] public string WeaponModel { get; set; } = ""; // Path to the weapon model/sprite
        [Export] public string ProjectileScene { get; set; } = "res://Scenes/Items/Projectile.tscn";
        [Export] public string MuzzleFlashScene { get; set; } = "";
        [Export] public string ShootSound { get; set; } = "";
        [Export] public string ReloadSound { get; set; } = "";
        [Export(PropertyHint.ColorNoAlpha)] public Color WeaponColor { get; set; } = Colors.White;
        
        // Affixes (prefixes and suffixes)
        [Export] public string[] Prefixes { get; set; } = Array.Empty<string>();
        [Export] public string[] Suffixes { get; set; } = Array.Empty<string>();
        
        // Affix effects
        [Export] public float DamageModifier { get; set; } = 0.0f;
        [Export] public float FireRateModifier { get; set; } = 0.0f;
        [Export] public float ReloadTimeModifier { get; set; } = 0.0f;
        [Export] public float CriticalChance { get; set; } = 0.05f;
        [Export] public float CriticalMultiplier { get; set; } = 1.5f;
        
        // Special effects
        [Export] public bool HasElementalEffect { get; set; } = false;
        [Export] public string ElementalType { get; set; } = ""; // Fire, Poison, Electric, etc.
        [Export] public float ElementalDamage { get; set; } = 0.0f;
        [Export] public float ElementalChance { get; set; } = 0.0f;
        
        [Export] public bool HasOnKillEffect { get; set; } = false;
        [Export] public bool HasOnHitEffect { get; set; } = false;
        [Export] public bool HasOnReloadEffect { get; set; } = false;
        [Export] public bool HasSpecialAbility { get; set; } = false;
        [Export] public string SpecialAbilityDescription { get; set; } = "";
        
        // Number of affix slots based on rarity
        public int GetAffixSlots()
        {
            switch (Rarity)
            {
                case RarityTier.Common: return 0;
                case RarityTier.Uncommon: return 1;
                case RarityTier.Rare: return 2;
                case RarityTier.Epic: return 2;
                case RarityTier.Mythic: return 3;
                default: return 0;
            }
        }
        
        // Get display name with affixes
        public string GetDisplayName()
        {
            if (Rarity == RarityTier.Mythic && !string.IsNullOrEmpty(Epithet))
            {
                return Epithet;
            }
            
            string name = WeaponName;
            
            if (Prefixes != null && Prefixes.Length > 0)
            {
                name = string.Join(" ", Prefixes) + " " + name;
            }
            
            if (Suffixes != null && Suffixes.Length > 0)
            {
                name = name + " of " + string.Join(" ", Suffixes);
            }
            
            return name;
        }
        
        // Calculated properties considering modifiers
        public float GetEffectiveDamage()
        {
            return Damage * (1 + DamageModifier);
        }
        
        public float GetEffectiveFireRate()
        {
            return FireRate * (1 + FireRateModifier);
        }
        
        public float GetEffectiveReloadTime()
        {
            return ReloadTime * (1 + ReloadTimeModifier);
        }
        
        // Serialization methods for JSON export/import
        public Dictionary<string, Variant> Serialize()
        {
            var data = new Dictionary<string, Variant>();
            data["weaponName"] = WeaponName;
            data["description"] = Description;
            data["type"] = (int)Type;
            data["rarity"] = (int)Rarity;
            data["epithet"] = Epithet;
            
            data["damage"] = Damage;
            data["fireRate"] = FireRate;
            data["reloadTime"] = ReloadTime;
            data["magazineSize"] = MagazineSize;
            data["range"] = Range;
            data["spread"] = Spread;
            data["projectileSpeed"] = ProjectileSpeed;
            data["projectilesPerShot"] = ProjectilesPerShot;
            data["isAutomatic"] = IsAutomatic;
            
            data["weaponModel"] = WeaponModel;
            data["projectileScene"] = ProjectileScene;
            data["muzzleFlashScene"] = MuzzleFlashScene;
            data["shootSound"] = ShootSound;
            data["reloadSound"] = ReloadSound;
            data["weaponColor"] = WeaponColor;
            
            data["prefixes"] = new Godot.Collections.Array<string>(Prefixes);
            data["suffixes"] = new Godot.Collections.Array<string>(Suffixes);
            
            data["damageModifier"] = DamageModifier;
            data["fireRateModifier"] = FireRateModifier;
            data["reloadTimeModifier"] = ReloadTimeModifier;
            data["criticalChance"] = CriticalChance;
            data["criticalMultiplier"] = CriticalMultiplier;
            
            data["hasElementalEffect"] = HasElementalEffect;
            data["elementalType"] = ElementalType;
            data["elementalDamage"] = ElementalDamage;
            data["elementalChance"] = ElementalChance;
            
            data["hasOnKillEffect"] = HasOnKillEffect;
            data["hasOnHitEffect"] = HasOnHitEffect;
            data["hasOnReloadEffect"] = HasOnReloadEffect;
            data["hasSpecialAbility"] = HasSpecialAbility;
            data["specialAbilityDescription"] = SpecialAbilityDescription;
            
            return data;
        }
        
        public void Deserialize(Dictionary<string, Variant> data)
        {
            if (data.TryGetValue("weaponName", out var weaponName))
                WeaponName = weaponName.AsString();
                
            if (data.TryGetValue("description", out var description))
                Description = description.AsString();
                
            if (data.TryGetValue("type", out var type))
                Type = (WeaponType)type.AsInt32();
                
            if (data.TryGetValue("rarity", out var rarity))
                Rarity = (RarityTier)rarity.AsInt32();
                
            if (data.TryGetValue("epithet", out var epithet))
                Epithet = epithet.AsString();
                
            if (data.TryGetValue("damage", out var damage))
                Damage = (float)damage.AsDouble();
                
            if (data.TryGetValue("fireRate", out var fireRate))
                FireRate = (float)fireRate.AsDouble();
                
            if (data.TryGetValue("reloadTime", out var reloadTime))
                ReloadTime = (float)reloadTime.AsDouble();
                
            if (data.TryGetValue("magazineSize", out var magazineSize))
                MagazineSize = magazineSize.AsInt32();
                
            if (data.TryGetValue("range", out var range))
                Range = (float)range.AsDouble();
                
            if (data.TryGetValue("spread", out var spread))
                Spread = (float)spread.AsDouble();
                
            if (data.TryGetValue("projectileSpeed", out var projectileSpeed))
                ProjectileSpeed = (float)projectileSpeed.AsDouble();
                
            if (data.TryGetValue("projectilesPerShot", out var projectilesPerShot))
                ProjectilesPerShot = projectilesPerShot.AsInt32();
                
            if (data.TryGetValue("isAutomatic", out var isAutomatic))
                IsAutomatic = isAutomatic.AsBool();
                
            if (data.TryGetValue("weaponModel", out var weaponModel))
                WeaponModel = weaponModel.AsString();
                
            if (data.TryGetValue("projectileScene", out var projectileScene))
                ProjectileScene = projectileScene.AsString();
                
            if (data.TryGetValue("muzzleFlashScene", out var muzzleFlashScene))
                MuzzleFlashScene = muzzleFlashScene.AsString();
                
            if (data.TryGetValue("shootSound", out var shootSound))
                ShootSound = shootSound.AsString();
                
            if (data.TryGetValue("reloadSound", out var reloadSound))
                ReloadSound = reloadSound.AsString();
                
            if (data.TryGetValue("weaponColor", out var weaponColor))
                WeaponColor = weaponColor.AsColor();
                
            if (data.TryGetValue("prefixes", out var prefixes))
            {
                var prefixArray = prefixes.AsStringArray();
                Prefixes = new string[prefixArray.Length];
                for (int i = 0; i < prefixArray.Length; i++)
                {
                    Prefixes[i] = prefixArray[i];
                }
            }
            
            if (data.TryGetValue("suffixes", out var suffixes))
            {
                var suffixArray = suffixes.AsStringArray();
                Suffixes = new string[suffixArray.Length];
                for (int i = 0; i < suffixArray.Length; i++)
                {
                    Suffixes[i] = suffixArray[i];
                }
            }
            
            if (data.TryGetValue("damageModifier", out var damageModifier))
                DamageModifier = (float)damageModifier.AsDouble();
                
            if (data.TryGetValue("fireRateModifier", out var fireRateModifier))
                FireRateModifier = (float)fireRateModifier.AsDouble();
                
            if (data.TryGetValue("reloadTimeModifier", out var reloadTimeModifier))
                ReloadTimeModifier = (float)reloadTimeModifier.AsDouble();
                
            if (data.TryGetValue("criticalChance", out var criticalChance))
                CriticalChance = (float)criticalChance.AsDouble();
                
            if (data.TryGetValue("criticalMultiplier", out var criticalMultiplier))
                CriticalMultiplier = (float)criticalMultiplier.AsDouble();
                
            if (data.TryGetValue("hasElementalEffect", out var hasElementalEffect))
                HasElementalEffect = hasElementalEffect.AsBool();
                
            if (data.TryGetValue("elementalType", out var elementalType))
                ElementalType = elementalType.AsString();
                
            if (data.TryGetValue("elementalDamage", out var elementalDamage))
                ElementalDamage = (float)elementalDamage.AsDouble();
                
            if (data.TryGetValue("elementalChance", out var elementalChance))
                ElementalChance = (float)elementalChance.AsDouble();
                
            if (data.TryGetValue("hasOnKillEffect", out var hasOnKillEffect))
                HasOnKillEffect = hasOnKillEffect.AsBool();
                
            if (data.TryGetValue("hasOnHitEffect", out var hasOnHitEffect))
                HasOnHitEffect = hasOnHitEffect.AsBool();
                
            if (data.TryGetValue("hasOnReloadEffect", out var hasOnReloadEffect))
                HasOnReloadEffect = hasOnReloadEffect.AsBool();
                
            if (data.TryGetValue("hasSpecialAbility", out var hasSpecialAbility))
                HasSpecialAbility = hasSpecialAbility.AsBool();
                
            if (data.TryGetValue("specialAbilityDescription", out var specialAbilityDescription))
                SpecialAbilityDescription = specialAbilityDescription.AsString();
        }
    }
}
