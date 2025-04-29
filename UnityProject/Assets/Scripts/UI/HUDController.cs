using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodexOfTheBrokenZodiac.Core;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Health & Mana")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider manaBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI manaText;
        
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerClassText;
        [SerializeField] private TextMeshProUGUI playerLevelText;
        
        [Header("Weapons & Abilities")]
        [SerializeField] private Image weaponIcon;
        [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private Image[] spellIcons;
        [SerializeField] private Image[] cooldownOverlays;
        
        [Header("Resources")]
        [SerializeField] private TextMeshProUGUI motesCountText;
        
        [Header("Mini Map")]
        [SerializeField] private RawImage miniMap;
        [SerializeField] private RectTransform playerMarker;
        
        [Header("Level Info")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI roomText;
        [SerializeField] private TextMeshProUGUI enemiesDefeatedText;
        
        [Header("Card Display")]
        [SerializeField] private GameObject cardDisplayPanel;
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private GameObject cardPrefab;
        
        [Header("Boss Health Bar")]
        [SerializeField] private GameObject bossHealthBarPanel;
        [SerializeField] private Slider bossHealthBar;
        [SerializeField] private TextMeshProUGUI bossNameText;
        
        // References
        private PlayerController _player;
        private GameManager _gameManager;
        private List<GameObject> _cardObjects = new List<GameObject>();
        
        private void Start()
        {
            // Get references
            _gameManager = GameManager.Instance;
            
            // Register events
            if (_gameManager != null)
            {
                _gameManager.OnGameStateChanged += OnGameStateChanged;
                _gameManager.OnMotesCollected += UpdateMotesCount;
            }
            
            // Hide boss health bar initially
            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(false);
            }
            
            // Hide card display initially
            if (cardDisplayPanel != null)
            {
                cardDisplayPanel.SetActive(false);
            }
            
            // Refresh UI
            UpdateUI();
        }
        
        private void Update()
        {
            // Check for card display toggle
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleCardDisplay();
            }
        }
        
        public void SetPlayer(PlayerController player)
        {
            // Unsubscribe from previous player events
            if (_player != null)
            {
                _player.OnHealthChanged -= UpdateHealthBar;
                _player.OnManaChanged -= UpdateManaBar;
                _player.OnWeaponChanged -= UpdateWeaponDisplay;
                _player.OnSpellChanged -= UpdateSpellDisplay;
            }
            
            _player = player;
            
            // Subscribe to new player events
            if (_player != null)
            {
                _player.OnHealthChanged += UpdateHealthBar;
                _player.OnManaChanged += UpdateManaBar;
                _player.OnWeaponChanged += UpdateWeaponDisplay;
                _player.OnSpellChanged += UpdateSpellDisplay;
                
                // Update player info
                if (playerNameText != null)
                {
                    playerNameText.text = _player.PlayerName;
                }
                
                if (playerClassText != null && _player.PlayerClass != null)
                {
                    playerClassText.text = _player.PlayerClass.ClassName;
                }
                
                // Update initial values
                UpdateHealthBar(_player.CurrentHealth, _player.MaxHealth);
                UpdateManaBar(_player.CurrentMana, _player.MaxMana);
                
                // Update weapon display
                if (_player.EquippedWeapon != null)
                {
                    UpdateWeaponDisplay(_player.EquippedWeapon);
                }
            }
        }
        
        private void UpdateHealthBar(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.value = Mathf.Clamp01(current / max);
            }
            
            if (healthText != null)
            {
                healthText.text = $"{Mathf.Ceil(current)} / {Mathf.Ceil(max)}";
            }
        }
        
        private void UpdateManaBar(float current, float max)
        {
            if (manaBar != null)
            {
                manaBar.value = Mathf.Clamp01(current / max);
            }
            
            if (manaText != null)
            {
                manaText.text = $"{Mathf.Ceil(current)} / {Mathf.Ceil(max)}";
            }
        }
        
        private void UpdateWeaponDisplay(Weapon weapon)
        {
            if (weapon == null) return;
            
            if (weaponIcon != null && weapon.WeaponSprite != null)
            {
                weaponIcon.sprite = weapon.WeaponSprite;
                weaponIcon.enabled = true;
            }
            
            if (weaponNameText != null)
            {
                weaponNameText.text = weapon.WeaponName;
            }
            
            // If this weapon uses ammo, we'd update that here
            if (ammoText != null)
            {
                ammoText.gameObject.SetActive(false); // Hide ammo counter for now
            }
        }
        
        private void UpdateSpellDisplay(string spellName)
        {
            // Find spell in player's spell list and update UI
            // This would typically reference a more complex spell system
            Debug.Log($"Current spell: {spellName}");
        }
        
        private void UpdateMotesCount(int amount)
        {
            if (_gameManager != null && motesCountText != null)
            {
                motesCountText.text = _gameManager.MotesCollected.ToString();
            }
        }
        
        public void UpdateLevelInfo(int level, int room)
        {
            if (levelText != null)
            {
                levelText.text = $"Level: {level}";
            }
            
            if (roomText != null)
            {
                roomText.text = $"Room: {room}";
            }
            
            if (_gameManager != null && enemiesDefeatedText != null)
            {
                enemiesDefeatedText.text = $"Enemies: {_gameManager.EnemiesDefeated}";
            }
        }
        
        public void ShowBossHealthBar(string bossName, float maxHealth)
        {
            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(true);
                
                if (bossNameText != null)
                {
                    bossNameText.text = bossName;
                }
                
                if (bossHealthBar != null)
                {
                    bossHealthBar.value = 1f;
                }
            }
        }
        
        public void UpdateBossHealth(float current, float max)
        {
            if (bossHealthBar != null)
            {
                bossHealthBar.value = Mathf.Clamp01(current / max);
            }
        }
        
        public void HideBossHealthBar()
        {
            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(false);
            }
        }
        
        private void ToggleCardDisplay()
        {
            if (cardDisplayPanel != null)
            {
                bool isActive = !cardDisplayPanel.activeSelf;
                cardDisplayPanel.SetActive(isActive);
                
                if (isActive)
                {
                    RefreshCardDisplay();
                }
            }
        }
        
        private void RefreshCardDisplay()
        {
            // Clear existing cards
            foreach (var cardObject in _cardObjects)
            {
                Destroy(cardObject);
            }
            _cardObjects.Clear();
            
            if (_player == null || cardContainer == null || cardPrefab == null) return;
            
            // Get cards from player
            List<TarotCard> playerCards = _player.TarotCards;
            
            // Create card UI elements
            foreach (var card in playerCards)
            {
                GameObject cardObject = Instantiate(cardPrefab, cardContainer);
                CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
                
                if (cardDisplay != null)
                {
                    cardDisplay.SetCard(card);
                }
                
                _cardObjects.Add(cardObject);
            }
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            // Update UI based on game state
            gameObject.SetActive(newState == GameState.Playing);
        }
        
        private void UpdateUI()
        {
            // Update all UI elements with current values
            if (_gameManager != null)
            {
                UpdateLevelInfo(_gameManager.CurrentLevel, 0); // Room ID would come from ProcGen
                UpdateMotesCount(0); // Just to update the UI with current value
            }
        }
    }
    
    // Simple class to display a tarot card in the UI
    public class CardDisplay : MonoBehaviour
    {
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        
        private TarotCard _card;
        
        public void SetCard(TarotCard card)
        {
            _card = card;
            
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
    }
}