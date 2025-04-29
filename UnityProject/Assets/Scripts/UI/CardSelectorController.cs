using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodexOfTheBrokenZodiac.Core;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.UI
{
    public class CardSelectorController : MonoBehaviour
    {
        [Header("Card Display")]
        [SerializeField] private int numberOfCards = 3;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Header("Audio")]
        [SerializeField] private AudioSource selectSound;
        [SerializeField] private AudioSource confirmSound;
        
        // References
        private PlayerController _player;
        private List<CardSelectionItem> _cardItems = new List<CardSelectionItem>();
        private TarotCard _selectedCard;
        
        // Card options for selection
        private List<TarotCard> _cardOptions = new List<TarotCard>();
        
        private void Start()
        {
            // Connect button event
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmSelection);
                confirmButton.interactable = false; // Disable until a card is selected
            }
            
            // Hide the panel initially
            gameObject.SetActive(false);
        }
        
        public void ShowCardSelection(List<TarotCard> cards, PlayerController player, string title = "Select a Card")
        {
            _player = player;
            _cardOptions = cards;
            _selectedCard = null;
            
            // Clear any existing cards
            ClearCards();
            
            // Update title
            if (titleText != null)
            {
                titleText.text = title;
            }
            
            // Create card items
            for (int i = 0; i < Mathf.Min(numberOfCards, cards.Count); i++)
            {
                CreateCardItem(cards[i], i);
            }
            
            // Disable confirm button until a card is selected
            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }
            
            // Show the panel
            gameObject.SetActive(true);
            
            // Pause game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }
        
        private void CreateCardItem(TarotCard card, int index)
        {
            if (cardPrefab == null || cardContainer == null) return;
            
            GameObject cardObject = Instantiate(cardPrefab, cardContainer);
            CardSelectionItem cardItem = cardObject.GetComponent<CardSelectionItem>();
            
            if (cardItem != null)
            {
                cardItem.SetCard(card, index);
                cardItem.OnCardSelected += OnCardSelected;
                _cardItems.Add(cardItem);
            }
        }
        
        private void ClearCards()
        {
            // Unsubscribe and destroy existing card items
            foreach (var cardItem in _cardItems)
            {
                if (cardItem != null)
                {
                    cardItem.OnCardSelected -= OnCardSelected;
                    Destroy(cardItem.gameObject);
                }
            }
            
            _cardItems.Clear();
        }
        
        private void OnCardSelected(TarotCard card)
        {
            _selectedCard = card;
            
            // Play selection sound
            if (selectSound != null)
            {
                selectSound.Play();
            }
            
            // Update visual selection
            foreach (var cardItem in _cardItems)
            {
                cardItem.SetSelected(cardItem.Card == card);
            }
            
            // Enable confirm button
            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }
        }
        
        private void OnConfirmSelection()
        {
            if (_selectedCard == null || _player == null) return;
            
            // Play confirm sound
            if (confirmSound != null)
            {
                confirmSound.Play();
            }
            
            // Add the card to the player
            _player.AddTarotCard(_selectedCard);
            
            // Hide the panel
            gameObject.SetActive(false);
            
            // Resume game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }
    }
    
    // Component to handle individual card selection items
    public class CardSelectionItem : MonoBehaviour
    {
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private Button cardButton;
        
        // Events
        public delegate void CardSelectedHandler(TarotCard card);
        public event CardSelectedHandler OnCardSelected;
        
        // Properties
        public TarotCard Card { get; private set; }
        
        private void Start()
        {
            // Connect button
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(OnButtonClicked);
            }
            
            // Hide selection indicator initially
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(false);
            }
        }
        
        public void SetCard(TarotCard card, int index)
        {
            Card = card;
            
            if (cardNameText != null)
            {
                cardNameText.text = card.GetFormattedTitle();
            }
            
            if (cardDescriptionText != null)
            {
                cardDescriptionText.text = card.Description;
            }
            
            if (cardImage != null && card.CardIcon != null)
            {
                cardImage.sprite = card.CardIcon;
                cardImage.color = card.CardColor;
            }
        }
        
        public void SetSelected(bool selected)
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(selected);
            }
        }
        
        private void OnButtonClicked()
        {
            OnCardSelected?.Invoke(Card);
        }
    }
}