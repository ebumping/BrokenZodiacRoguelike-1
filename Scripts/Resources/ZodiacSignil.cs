using Godot;
using System;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources
{
    [GlobalClass]
    public partial class ZodiacSignil : Resource
    {
        [Export] public string SignName { get; set; }
        [Export] public string Description { get; set; }
        [Export] public string Symbol { get; set; } // Unicode symbol or icon reference
        
        [Export] public string Element { get; set; } // Fire, Earth, Air, Water
        [Export] public string Modality { get; set; } // Cardinal, Fixed, Mutable
        
        [Export] public string PrimaryEffect { get; set; }
        [Export] public string SecondaryEffect { get; set; }
        
        [Export] public float PrimaryEffectValue { get; set; }
        [Export] public float SecondaryEffectValue { get; set; }
        
        [Export] public string IconPath { get; set; }
        [Export(PropertyHint.ColorNoAlpha)] public Color SignColor { get; set; } = Colors.White;
        
        // Combat effects
        [Export] public float HealthModifier { get; set; } = 0.0f;
        [Export] public float SpeedModifier { get; set; } = 0.0f;
        [Export] public float DamageModifier { get; set; } = 0.0f;
        [Export] public float CooldownModifier { get; set; } = 0.0f;
        [Export] public float CriticalModifier { get; set; } = 0.0f;
        
        [Export] public string SpecialAbility { get; set; } = "";
        [Export] public bool HasOnKillEffect { get; set; } = false;
        [Export] public bool HasOnHitEffect { get; set; } = false;
        [Export] public bool HasOnDodgeEffect { get; set; } = false;
        
        // Serialization methods for JSON export/import
        public Dictionary<string, Variant> Serialize()
        {
            var data = new Dictionary<string, Variant>();
            data["signName"] = SignName;
            data["description"] = Description;
            data["symbol"] = Symbol;
            data["element"] = Element;
            data["modality"] = Modality;
            data["primaryEffect"] = PrimaryEffect;
            data["secondaryEffect"] = SecondaryEffect;
            data["primaryEffectValue"] = PrimaryEffectValue;
            data["secondaryEffectValue"] = SecondaryEffectValue;
            data["iconPath"] = IconPath;
            data["signColor"] = SignColor;
            data["healthModifier"] = HealthModifier;
            data["speedModifier"] = SpeedModifier;
            data["damageModifier"] = DamageModifier;
            data["cooldownModifier"] = CooldownModifier;
            data["criticalModifier"] = CriticalModifier;
            data["specialAbility"] = SpecialAbility;
            data["hasOnKillEffect"] = HasOnKillEffect;
            data["hasOnHitEffect"] = HasOnHitEffect;
            data["hasOnDodgeEffect"] = HasOnDodgeEffect;
            
            return data;
        }
        
        public void Deserialize(Dictionary<string, Variant> data)
        {
            if (data.TryGetValue("signName", out var signName))
                SignName = signName.AsString();
                
            if (data.TryGetValue("description", out var description))
                Description = description.AsString();
                
            if (data.TryGetValue("symbol", out var symbol))
                Symbol = symbol.AsString();
                
            if (data.TryGetValue("element", out var element))
                Element = element.AsString();
                
            if (data.TryGetValue("modality", out var modality))
                Modality = modality.AsString();
                
            if (data.TryGetValue("primaryEffect", out var primaryEffect))
                PrimaryEffect = primaryEffect.AsString();
                
            if (data.TryGetValue("secondaryEffect", out var secondaryEffect))
                SecondaryEffect = secondaryEffect.AsString();
                
            if (data.TryGetValue("primaryEffectValue", out var primaryEffectValue))
                PrimaryEffectValue = (float)primaryEffectValue.AsDouble();
                
            if (data.TryGetValue("secondaryEffectValue", out var secondaryEffectValue))
                SecondaryEffectValue = (float)secondaryEffectValue.AsDouble();
                
            if (data.TryGetValue("iconPath", out var iconPath))
                IconPath = iconPath.AsString();
                
            if (data.TryGetValue("signColor", out var signColor))
                SignColor = signColor.AsColor();
                
            if (data.TryGetValue("healthModifier", out var healthModifier))
                HealthModifier = (float)healthModifier.AsDouble();
                
            if (data.TryGetValue("speedModifier", out var speedModifier))
                SpeedModifier = (float)speedModifier.AsDouble();
                
            if (data.TryGetValue("damageModifier", out var damageModifier))
                DamageModifier = (float)damageModifier.AsDouble();
                
            if (data.TryGetValue("cooldownModifier", out var cooldownModifier))
                CooldownModifier = (float)cooldownModifier.AsDouble();
                
            if (data.TryGetValue("criticalModifier", out var criticalModifier))
                CriticalModifier = (float)criticalModifier.AsDouble();
                
            if (data.TryGetValue("specialAbility", out var specialAbility))
                SpecialAbility = specialAbility.AsString();
                
            if (data.TryGetValue("hasOnKillEffect", out var hasOnKillEffect))
                HasOnKillEffect = hasOnKillEffect.AsBool();
                
            if (data.TryGetValue("hasOnHitEffect", out var hasOnHitEffect))
                HasOnHitEffect = hasOnHitEffect.AsBool();
                
            if (data.TryGetValue("hasOnDodgeEffect", out var hasOnDodgeEffect))
                HasOnDodgeEffect = hasOnDodgeEffect.AsBool();
        }
    }
}
