using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
    [CreateAssetMenu(fileName = "NewTarotCard", menuName = "Codex/TarotCard")]
    public class TarotCard : ScriptableObject
    {
        public enum CardType { Major, Minor }
        public enum CardSuit { None, Wands, Cups, Swords, Pentacles }
        
        [Header("Card Identity")]
        public string CardName;
        [TextArea(3, 5)]
        public string Description;
        public CardType Type = CardType.Major;
        public CardSuit Suit = CardSuit.None;
        public int Value = 0; // For minor arcana (1-14)
        public string MajorArcanaNumber = ""; // 0-21 for major arcana in Roman numerals
        
        [Header("Card Effects")]
        public string PrimaryEffect;
        public string SecondaryEffect;
        public float PrimaryEffectValue;
        public float SecondaryEffectValue;
        public bool IsSingleUse = false;
        public bool IsPassive = true;
        
        [Header("Visuals")]
        public Sprite CardIcon;
        public Color CardColor = Color.white;
        
        [Header("Combat Modifiers")]
        public float HealthModifier = 0.0f;
        public float DamageModifier = 0.0f;
        public float SpeedModifier = 0.0f;
        public float CooldownModifier = 0.0f;
        public float CriticalModifier = 0.0f;
        
        [Header("Special Effects")]
        public string ActiveAbility = "";
        public bool HasOnKillEffect = false;
        public bool HasOnHitEffect = false;
        public bool HasOnRoomClearEffect = false;
        
        [Header("Combo System")]
        public string ComboEffect = "";
        public int ComboThreshold = 3;
        
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
        
        // Apply card effects to a player
        public void ApplyCardEffects(Core.PlayerController player)
        {
            if (player == null) return;
            
            // Apply stat modifiers
            player.ModifyMaxHealth(HealthModifier);
            player.ModifyDamage(DamageModifier);
            player.ModifySpeed(SpeedModifier);
            player.ModifyCooldown(CooldownModifier);
            player.ModifyCriticalChance(CriticalModifier);
            
            // Register special effects
            if (HasOnKillEffect)
            {
                player.RegisterOnKillEffect(this);
            }
            
            if (HasOnHitEffect)
            {
                player.RegisterOnHitEffect(this);
            }
            
            if (HasOnRoomClearEffect)
            {
                player.RegisterOnRoomClearEffect(this);
            }
            
            // Add active ability if specified
            if (!string.IsNullOrEmpty(ActiveAbility))
            {
                player.AddAbility(ActiveAbility);
            }
            
            Debug.Log($"Applied {GetFormattedTitle()} effects to player");
        }
        
        // Remove card effects from a player (when card is discarded)
        public void RemoveCardEffects(Core.PlayerController player)
        {
            if (player == null) return;
            
            // Remove stat modifiers
            player.ModifyMaxHealth(-HealthModifier);
            player.ModifyDamage(-DamageModifier);
            player.ModifySpeed(-SpeedModifier);
            player.ModifyCooldown(-CooldownModifier);
            player.ModifyCriticalChance(-CriticalModifier);
            
            // Unregister special effects
            if (HasOnKillEffect)
            {
                player.UnregisterOnKillEffect(this);
            }
            
            if (HasOnHitEffect)
            {
                player.UnregisterOnHitEffect(this);
            }
            
            if (HasOnRoomClearEffect)
            {
                player.UnregisterOnRoomClearEffect(this);
            }
            
            // Remove active ability if specified
            if (!string.IsNullOrEmpty(ActiveAbility))
            {
                player.RemoveAbility(ActiveAbility);
            }
            
            Debug.Log($"Removed {GetFormattedTitle()} effects from player");
        }
        
        // For JSON serialization/deserialization
        [Serializable]
        public class TarotCardData
        {
            public string cardName;
            public string description;
            public int type;
            public int suit;
            public int value;
            public string majorArcanaNumber;
            public string primaryEffect;
            public string secondaryEffect;
            public float primaryEffectValue;
            public float secondaryEffectValue;
            public bool isSingleUse;
            public bool isPassive;
            public string iconPath;
            public Color cardColor;
            public float healthModifier;
            public float damageModifier;
            public float speedModifier;
            public float cooldownModifier;
            public float criticalModifier;
            public string activeAbility;
            public bool hasOnKillEffect;
            public bool hasOnHitEffect;
            public bool hasOnRoomClearEffect;
            public string comboEffect;
            public int comboThreshold;
        }
        
        public TarotCardData Serialize()
        {
            var data = new TarotCardData
            {
                cardName = CardName,
                description = Description,
                type = (int)Type,
                suit = (int)Suit,
                value = Value,
                majorArcanaNumber = MajorArcanaNumber,
                primaryEffect = PrimaryEffect,
                secondaryEffect = SecondaryEffect,
                primaryEffectValue = PrimaryEffectValue,
                secondaryEffectValue = SecondaryEffectValue,
                isSingleUse = IsSingleUse,
                isPassive = IsPassive,
                iconPath = CardIcon != null ? CardIcon.name : "",
                cardColor = CardColor,
                healthModifier = HealthModifier,
                damageModifier = DamageModifier,
                speedModifier = SpeedModifier,
                cooldownModifier = CooldownModifier,
                criticalModifier = CriticalModifier,
                activeAbility = ActiveAbility,
                hasOnKillEffect = HasOnKillEffect,
                hasOnHitEffect = HasOnHitEffect,
                hasOnRoomClearEffect = HasOnRoomClearEffect,
                comboEffect = ComboEffect,
                comboThreshold = ComboThreshold
            };
            
            return data;
        }
        
        public void Deserialize(TarotCardData data)
        {
            CardName = data.cardName;
            Description = data.description;
            Type = (CardType)data.type;
            Suit = (CardSuit)data.suit;
            Value = data.value;
            MajorArcanaNumber = data.majorArcanaNumber;
            PrimaryEffect = data.primaryEffect;
            SecondaryEffect = data.secondaryEffect;
            PrimaryEffectValue = data.primaryEffectValue;
            SecondaryEffectValue = data.secondaryEffectValue;
            IsSingleUse = data.isSingleUse;
            IsPassive = data.isPassive;
            CardColor = data.cardColor;
            HealthModifier = data.healthModifier;
            DamageModifier = data.damageModifier;
            SpeedModifier = data.speedModifier;
            CooldownModifier = data.cooldownModifier;
            CriticalModifier = data.criticalModifier;
            ActiveAbility = data.activeAbility;
            HasOnKillEffect = data.hasOnKillEffect;
            HasOnHitEffect = data.hasOnHitEffect;
            HasOnRoomClearEffect = data.hasOnRoomClearEffect;
            ComboEffect = data.comboEffect;
            ComboThreshold = data.comboThreshold;
            
            // Load icon sprite if path is provided
            if (!string.IsNullOrEmpty(data.iconPath))
            {
                CardIcon = Resources.Load<Sprite>(data.iconPath);
            }
        }
    }
}