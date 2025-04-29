using Godot;
using System;
using System.Collections.Generic;

namespace CodexOfTheBrokenZodiac.Resources
{
    [GlobalClass]
    public partial class TarotCard : Resource
    {
        public enum CardType { Major, Minor }
        public enum CardSuit { None, Wands, Cups, Swords, Pentacles }
        
        [Export] public string CardName { get; set; }
        [Export] public string Description { get; set; }
        [Export] public CardType Type { get; set; } = CardType.Major;
        [Export] public CardSuit Suit { get; set; } = CardSuit.None;
        [Export] public int Value { get; set; } = 0; // For minor arcana (1-14)
        
        [Export] public string MajorArcanaNumber { get; set; } = ""; // 0-21 for major arcana in Roman numerals
        
        [Export] public string PrimaryEffect { get; set; }
        [Export] public string SecondaryEffect { get; set; }
        
        [Export] public float PrimaryEffectValue { get; set; }
        [Export] public float SecondaryEffectValue { get; set; }
        
        [Export] public bool IsSingleUse { get; set; } = false;
        [Export] public bool IsPassive { get; set; } = true;
        
        [Export] public string IconPath { get; set; }
        [Export(PropertyHint.ColorNoAlpha)] public Color CardColor { get; set; } = Colors.White;
        
        // Combat effects
        [Export] public float HealthModifier { get; set; } = 0.0f;
        [Export] public float DamageModifier { get; set; } = 0.0f;
        [Export] public float SpeedModifier { get; set; } = 0.0f;
        [Export] public float CooldownModifier { get; set; } = 0.0f;
        [Export] public float CriticalModifier { get; set; } = 0.0f;
        
        [Export] public string ActiveAbility { get; set; } = "";
        [Export] public bool HasOnKillEffect { get; set; } = false;
        [Export] public bool HasOnHitEffect { get; set; } = false;
        [Export] public bool HasOnRoomClearEffect { get; set; } = false;
        
        // Combo effects for multiple cards of same type
        [Export] public string ComboEffect { get; set; } = "";
        [Export] public int ComboThreshold { get; set; } = 3;
        
        // Returns formatted card title (e.g., "XV - The Devil" or "Ace of Wands")
        public string GetFormattedTitle()
        {
            if (Type == CardType.Major && !string.IsNullOrEmpty(MajorArcanaNumber))
            {
                return $"{MajorArcanaNumber} - {CardName}";
            }
            else if (Type == CardType.Minor)
            {
                string valueName = GetValueName();
                return $"{valueName} of {Suit}";
            }
            
            return CardName;
        }
        
        private string GetValueName()
        {
            switch (Value)
            {
                case 1: return "Ace";
                case 2: return "Two";
                case 3: return "Three";
                case 4: return "Four";
                case 5: return "Five";
                case 6: return "Six";
                case 7: return "Seven";
                case 8: return "Eight";
                case 9: return "Nine";
                case 10: return "Ten";
                case 11: return "Page";
                case 12: return "Knight";
                case 13: return "Queen";
                case 14: return "King";
                default: return Value.ToString();
            }
        }
        
        // Get element based on suit
        public string GetElement()
        {
            switch (Suit)
            {
                case CardSuit.Wands: return "Fire";
                case CardSuit.Cups: return "Water";
                case CardSuit.Swords: return "Air";
                case CardSuit.Pentacles: return "Earth";
                default: return "None";
            }
        }
        
        // Serialization methods for JSON export/import
        public Dictionary<string, Variant> Serialize()
        {
            var data = new Dictionary<string, Variant>();
            data["cardName"] = CardName;
            data["description"] = Description;
            data["type"] = (int)Type;
            data["suit"] = (int)Suit;
            data["value"] = Value;
            data["majorArcanaNumber"] = MajorArcanaNumber;
            data["primaryEffect"] = PrimaryEffect;
            data["secondaryEffect"] = SecondaryEffect;
            data["primaryEffectValue"] = PrimaryEffectValue;
            data["secondaryEffectValue"] = SecondaryEffectValue;
            data["isSingleUse"] = IsSingleUse;
            data["isPassive"] = IsPassive;
            data["iconPath"] = IconPath;
            data["cardColor"] = CardColor;
            data["healthModifier"] = HealthModifier;
            data["damageModifier"] = DamageModifier;
            data["speedModifier"] = SpeedModifier;
            data["cooldownModifier"] = CooldownModifier;
            data["criticalModifier"] = CriticalModifier;
            data["activeAbility"] = ActiveAbility;
            data["hasOnKillEffect"] = HasOnKillEffect;
            data["hasOnHitEffect"] = HasOnHitEffect;
            data["hasOnRoomClearEffect"] = HasOnRoomClearEffect;
            data["comboEffect"] = ComboEffect;
            data["comboThreshold"] = ComboThreshold;
            
            return data;
        }
        
        public void Deserialize(Dictionary<string, Variant> data)
        {
            if (data.TryGetValue("cardName", out var cardName))
                CardName = cardName.AsString();
                
            if (data.TryGetValue("description", out var description))
                Description = description.AsString();
                
            if (data.TryGetValue("type", out var type))
                Type = (CardType)type.AsInt32();
                
            if (data.TryGetValue("suit", out var suit))
                Suit = (CardSuit)suit.AsInt32();
                
            if (data.TryGetValue("value", out var value))
                Value = value.AsInt32();
                
            if (data.TryGetValue("majorArcanaNumber", out var majorArcanaNumber))
                MajorArcanaNumber = majorArcanaNumber.AsString();
                
            if (data.TryGetValue("primaryEffect", out var primaryEffect))
                PrimaryEffect = primaryEffect.AsString();
                
            if (data.TryGetValue("secondaryEffect", out var secondaryEffect))
                SecondaryEffect = secondaryEffect.AsString();
                
            if (data.TryGetValue("primaryEffectValue", out var primaryEffectValue))
                PrimaryEffectValue = (float)primaryEffectValue.AsDouble();
                
            if (data.TryGetValue("secondaryEffectValue", out var secondaryEffectValue))
                SecondaryEffectValue = (float)secondaryEffectValue.AsDouble();
                
            if (data.TryGetValue("isSingleUse", out var isSingleUse))
                IsSingleUse = isSingleUse.AsBool();
                
            if (data.TryGetValue("isPassive", out var isPassive))
                IsPassive = isPassive.AsBool();
                
            if (data.TryGetValue("iconPath", out var iconPath))
                IconPath = iconPath.AsString();
                
            if (data.TryGetValue("cardColor", out var cardColor))
                CardColor = cardColor.AsColor();
                
            if (data.TryGetValue("healthModifier", out var healthModifier))
                HealthModifier = (float)healthModifier.AsDouble();
                
            if (data.TryGetValue("damageModifier", out var damageModifier))
                DamageModifier = (float)damageModifier.AsDouble();
                
            if (data.TryGetValue("speedModifier", out var speedModifier))
                SpeedModifier = (float)speedModifier.AsDouble();
                
            if (data.TryGetValue("cooldownModifier", out var cooldownModifier))
                CooldownModifier = (float)cooldownModifier.AsDouble();
                
            if (data.TryGetValue("criticalModifier", out var criticalModifier))
                CriticalModifier = (float)criticalModifier.AsDouble();
                
            if (data.TryGetValue("activeAbility", out var activeAbility))
                ActiveAbility = activeAbility.AsString();
                
            if (data.TryGetValue("hasOnKillEffect", out var hasOnKillEffect))
                HasOnKillEffect = hasOnKillEffect.AsBool();
                
            if (data.TryGetValue("hasOnHitEffect", out var hasOnHitEffect))
                HasOnHitEffect = hasOnHitEffect.AsBool();
                
            if (data.TryGetValue("hasOnRoomClearEffect", out var hasOnRoomClearEffect))
                HasOnRoomClearEffect = hasOnRoomClearEffect.AsBool();
                
            if (data.TryGetValue("comboEffect", out var comboEffect))
                ComboEffect = comboEffect.AsString();
                
            if (data.TryGetValue("comboThreshold", out var comboThreshold))
                ComboThreshold = comboThreshold.AsInt32();
        }
    }
}
