using Godot;
using System;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources
{
    [GlobalClass]
    public partial class PlayerClass : Resource
    {
        [Export] public string ClassName { get; set; }
        [Export] public string Description { get; set; }
        [Export] public string PsycheTheme { get; set; }
        
        [Export] public float BaseHealth { get; set; } = 100.0f;
        [Export] public float BaseMana { get; set; } = 100.0f;
        [Export] public float MoveSpeed { get; set; } = 200.0f;
        
        [Export] public string DefaultWeapon { get; set; } = "";
        [Export] public string[] Spells { get; set; } = Array.Empty<string>();
        
        [Export] public string CoreAbility { get; set; } = "";
        [Export] public string PassiveAbility { get; set; } = "";
        [Export] public string SpecialAbility { get; set; } = "";
        
        [Export] public string SpritePath { get; set; } = "";
        [Export] public string IconPath { get; set; } = "";
        
        [Export(PropertyHint.ColorNoAlpha)] public Color ClassColor { get; set; } = Colors.White;
        
        // Talent tree nodes (simplified for initial implementation)
        [Export] public string[] TalentNodes { get; set; } = Array.Empty<string>();
        
        // Serialization methods for JSON export/import
        public Dictionary<string, Variant> Serialize()
        {
            var data = new Dictionary<string, Variant>();
            data["className"] = ClassName;
            data["description"] = Description;
            data["psycheTheme"] = PsycheTheme;
            data["baseHealth"] = BaseHealth;
            data["baseMana"] = BaseMana;
            data["moveSpeed"] = MoveSpeed;
            data["defaultWeapon"] = DefaultWeapon;
            data["spells"] = new Godot.Collections.Array<string>(Spells);
            data["coreAbility"] = CoreAbility;
            data["passiveAbility"] = PassiveAbility;
            data["specialAbility"] = SpecialAbility;
            data["spritePath"] = SpritePath;
            data["iconPath"] = IconPath;
            data["classColor"] = ClassColor;
            data["talentNodes"] = new Godot.Collections.Array<string>(TalentNodes);
            
            return data;
        }
        
        public void Deserialize(Dictionary<string, Variant> data)
        {
            if (data.TryGetValue("className", out var className))
                ClassName = className.AsString();
                
            if (data.TryGetValue("description", out var description))
                Description = description.AsString();
                
            if (data.TryGetValue("psycheTheme", out var psycheTheme))
                PsycheTheme = psycheTheme.AsString();
                
            if (data.TryGetValue("baseHealth", out var baseHealth))
                BaseHealth = (float)baseHealth.AsDouble();
                
            if (data.TryGetValue("baseMana", out var baseMana))
                BaseMana = (float)baseMana.AsDouble();
                
            if (data.TryGetValue("moveSpeed", out var moveSpeed))
                MoveSpeed = (float)moveSpeed.AsDouble();
                
            if (data.TryGetValue("defaultWeapon", out var defaultWeapon))
                DefaultWeapon = defaultWeapon.AsString();
                
            if (data.TryGetValue("spells", out var spells))
            {
                var spellArray = spells.AsStringArray();
                Spells = new string[spellArray.Length];
                for (int i = 0; i < spellArray.Length; i++)
                {
                    Spells[i] = spellArray[i];
                }
            }
            
            if (data.TryGetValue("coreAbility", out var coreAbility))
                CoreAbility = coreAbility.AsString();
                
            if (data.TryGetValue("passiveAbility", out var passiveAbility))
                PassiveAbility = passiveAbility.AsString();
                
            if (data.TryGetValue("specialAbility", out var specialAbility))
                SpecialAbility = specialAbility.AsString();
                
            if (data.TryGetValue("spritePath", out var spritePath))
                SpritePath = spritePath.AsString();
                
            if (data.TryGetValue("iconPath", out var iconPath))
                IconPath = iconPath.AsString();
                
            if (data.TryGetValue("classColor", out var classColor))
                ClassColor = classColor.AsColor();
                
            if (data.TryGetValue("talentNodes", out var talentNodes))
            {
                var talentArray = talentNodes.AsStringArray();
                TalentNodes = new string[talentArray.Length];
                for (int i = 0; i < talentArray.Length; i++)
                {
                    TalentNodes[i] = talentArray[i];
                }
            }
        }
    }
}
