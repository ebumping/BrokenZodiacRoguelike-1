using System.Collections.Generic;
using UnityEngine;

namespace CodexOfTheBrokenZodiac.Resources
{
    // This ScriptableObject holds all tarot cards in the game
    [CreateAssetMenu(fileName = "TarotCardDatabase", menuName = "Codex/TarotCardDatabase")]
    public class TarotCardDatabase : ScriptableObject
    {
        [SerializeField] private List<TarotCard> majorArcana = new List<TarotCard>();
        [SerializeField] private List<TarotCard> wandsCards = new List<TarotCard>();
        [SerializeField] private List<TarotCard> cupsCards = new List<TarotCard>();
        [SerializeField] private List<TarotCard> swordsCards = new List<TarotCard>();
        [SerializeField] private List<TarotCard> pentaclesCards = new List<TarotCard>();
        
        // Combined list of all cards for easy access
        private List<TarotCard> _allCards = null;
        
        // Get all available cards
        public List<TarotCard> GetAllCards()
        {
            if (_allCards == null)
            {
                // Initialize the combined list if it hasn't been already
                _allCards = new List<TarotCard>();
                _allCards.AddRange(majorArcana);
                _allCards.AddRange(wandsCards);
                _allCards.AddRange(cupsCards);
                _allCards.AddRange(swordsCards);
                _allCards.AddRange(pentaclesCards);
            }
            
            return _allCards;
        }
        
        // Get cards of a specific suit
        public List<TarotCard> GetCardsBySuit(TarotCard.CardSuit suit)
        {
            switch (suit)
            {
                case TarotCard.CardSuit.None:
                    return majorArcana;
                case TarotCard.CardSuit.Wands:
                    return wandsCards;
                case TarotCard.CardSuit.Cups:
                    return cupsCards;
                case TarotCard.CardSuit.Swords:
                    return swordsCards;
                case TarotCard.CardSuit.Pentacles:
                    return pentaclesCards;
                default:
                    return new List<TarotCard>(); 
            }
        }
        
        // Get a specific Major Arcana card by its number (0-21)
        public TarotCard GetMajorArcanaByNumber(int number)
        {
            if (number < 0 || number >= majorArcana.Count)
                return null;
                
            return majorArcana[number];
        }
        
        // Get a specific Minor Arcana card by suit and value (1-14)
        public TarotCard GetMinorArcana(TarotCard.CardSuit suit, int value)
        {
            if (value < 1 || value > 14)
                return null;
                
            List<TarotCard> suitCards = GetCardsBySuit(suit);
            
            // Find the card with the matching value
            foreach (var card in suitCards)
            {
                if (card.Value == value)
                    return card;
            }
            
            return null;
        }
        
        // Get a random card (useful for roguelike elements)
        public TarotCard GetRandomCard()
        {
            List<TarotCard> allCards = GetAllCards();
            if (allCards.Count == 0)
                return null;
                
            int randomIndex = Random.Range(0, allCards.Count);
            return allCards[randomIndex];
        }
        
        // Get a selection of random cards (for card choice events)
        public List<TarotCard> GetRandomCardSelection(int count, bool allowDuplicates = false)
        {
            List<TarotCard> result = new List<TarotCard>();
            List<TarotCard> availableCards = new List<TarotCard>(GetAllCards());
            
            if (availableCards.Count == 0)
                return result;
                
            // Ensure we don't try to get more cards than exist in the deck
            count = Mathf.Min(count, allowDuplicates ? 100 : availableCards.Count);
            
            for (int i = 0; i < count; i++)
            {
                if (availableCards.Count == 0)
                    break;
                    
                int randomIndex = Random.Range(0, availableCards.Count);
                result.Add(availableCards[randomIndex]);
                
                if (!allowDuplicates)
                    availableCards.RemoveAt(randomIndex);
            }
            
            return result;
        }
        
        // Get cards that have synergy with existing cards (for advanced card choice logic)
        public List<TarotCard> GetSynergyCards(List<TarotCard> existingCards, int count)
        {
            // If no existing cards, just return random ones
            if (existingCards == null || existingCards.Count == 0)
                return GetRandomCardSelection(count);
                
            List<TarotCard> result = new List<TarotCard>();
            List<TarotCard> allCards = GetAllCards();
            Dictionary<TarotCard, float> cardScores = new Dictionary<TarotCard, float>();
            
            // Score each card based on synergy with existing cards
            foreach (var card in allCards)
            {
                if (existingCards.Contains(card))
                    continue; // Skip cards the player already has
                    
                float synergyScore = CalculateSynergyScore(card, existingCards);
                cardScores[card] = synergyScore;
            }
            
            // Sort cards by synergy score
            List<KeyValuePair<TarotCard, float>> sortedCards = new List<KeyValuePair<TarotCard, float>>(cardScores);
            sortedCards.Sort((x, y) => y.Value.CompareTo(x.Value)); // Descending order
            
            // Take the top cards but add some randomness
            int poolSize = Mathf.Min(sortedCards.Count, count * 3); // Create a pool of good candidates
            List<TarotCard> candidatePool = new List<TarotCard>();
            
            for (int i = 0; i < poolSize; i++)
            {
                candidatePool.Add(sortedCards[i].Key);
            }
            
            // Now select randomly from this better pool
            while (result.Count < count && candidatePool.Count > 0)
            {
                int randomIndex = Random.Range(0, candidatePool.Count);
                result.Add(candidatePool[randomIndex]);
                candidatePool.RemoveAt(randomIndex);
            }
            
            return result;
        }
        
        // Calculate how well a card synergizes with existing cards
        private float CalculateSynergyScore(TarotCard card, List<TarotCard> existingCards)
        {
            float score = 1.0f; // Base score
            
            // Check for suit synergies
            int suitCount = existingCards.FindAll(c => c.Suit == card.Suit).Count;
            if (suitCount > 0)
            {
                // Reward having cards of the same suit
                score += suitCount * 0.5f;
            }
            
            // Check for archetype synergies
            bool hasHealthCards = existingCards.Exists(c => c.HealthModifier > 0);
            bool hasDamageCards = existingCards.Exists(c => c.DamageModifier > 0);
            bool hasSpeedCards = existingCards.Exists(c => c.SpeedModifier > 0);
            bool hasCooldownCards = existingCards.Exists(c => c.CooldownModifier < 0); // Lower cooldown is better
            
            // Reward complementary effects
            if (hasHealthCards && card.HealthModifier > 0) score += 1.0f;
            if (hasDamageCards && card.DamageModifier > 0) score += 1.0f;
            if (hasSpeedCards && card.SpeedModifier > 0) score += 1.0f;
            if (hasCooldownCards && card.CooldownModifier < 0) score += 1.0f;
            
            // Reward specific combos (examples)
            if (card.HasOnKillEffect && existingCards.Exists(c => c.DamageModifier > 2.0f))
                score += 1.5f; // On-kill effects are good with high damage
                
            if (card.HasOnHitEffect && existingCards.Exists(c => c.CooldownModifier < -0.2f))
                score += 1.5f; // On-hit effects are good with fast attack speed
                
            // Add some randomness
            score += Random.Range(0f, 0.5f);
            
            return score;
        }
    }
}