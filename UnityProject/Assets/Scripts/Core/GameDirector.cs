using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CodexOfTheBrokenZodiac.UI;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    public class GameDirector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MainMenuController mainMenu;
        [SerializeField] private GameObject mainMenuObject;
        [SerializeField] private HUDController hud;
        [SerializeField] private GameObject worldContainer;
        [SerializeField] private GameObject[] systemPrefabs;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject screenFadeOverlay;
        
        [Header("Level Transition")]
        [SerializeField] private float fadeInTime = 1.0f;
        [SerializeField] private float fadeOutTime = 1.0f;
        
        // Internal state
        private bool _isTransitioning = false;
        private CanvasGroup _fadeCanvasGroup;
        private List<GameObject> _instancedSystemPrefabs = new List<GameObject>();
        
        private void Awake()
        {
            // Get references
            _fadeCanvasGroup = screenFadeOverlay.GetComponent<CanvasGroup>();
            
            // Initialize fade overlay
            if (_fadeCanvasGroup != null)
            {
                _fadeCanvasGroup.alpha = 1.0f;
            }
        }
        
        private void Start()
        {
            // Subscribe to events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
            
            // Initially hide gameplay UI
            if (hud != null)
            {
                hud.gameObject.SetActive(false);
            }
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
            
            // Start with a fade in to the main menu
            StartCoroutine(FadeIn());
            
            // Instantiate system prefabs
            InstantiateSystemPrefabs();
        }
        
        private void Update()
        {
            // Handle pause input
            if (Input.GetKeyDown(KeyCode.Escape) && !_isTransitioning)
            {
                if (GameManager.Instance != null)
                {
                    if (GameManager.Instance.CurrentState == GameState.Playing)
                    {
                        PauseGame();
                    }
                    else if (GameManager.Instance.CurrentState == GameState.Paused)
                    {
                        ResumeGame();
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
            
            // Clean up system prefabs if we're being destroyed
            foreach (var prefab in _instancedSystemPrefabs)
            {
                Destroy(prefab);
            }
        }
        
        private void InstantiateSystemPrefabs()
        {
            GameObject prefabInstance;
            for (int i = 0; i < systemPrefabs.Length; i++)
            {
                prefabInstance = Instantiate(systemPrefabs[i]);
                _instancedSystemPrefabs.Add(prefabInstance);
            }
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            Debug.Log($"Game state changed to {newState}");
            
            switch (newState)
            {
                case GameState.MainMenu:
                    HandleMainMenuState();
                    break;
                    
                case GameState.Playing:
                    HandlePlayingState();
                    break;
                    
                case GameState.Paused:
                    HandlePausedState();
                    break;
                    
                case GameState.GameOver:
                    HandleGameOverState();
                    break;
                    
                case GameState.Victory:
                    HandleVictoryState();
                    break;
                    
                case GameState.Connecting:
                    HandleConnectingState();
                    break;
            }
        }
        
        private void HandleMainMenuState()
        {
            // Show main menu, hide everything else
            mainMenuObject.SetActive(true);
            
            if (hud != null)
            {
                hud.gameObject.SetActive(false);
            }
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
            
            // Clear the world container
            foreach (Transform child in worldContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        private void HandlePlayingState()
        {
            // Hide main menu, show HUD, hide other UI
            mainMenuObject.SetActive(false);
            
            if (hud != null)
            {
                hud.gameObject.SetActive(true);
            }
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
        }
        
        private void HandlePausedState()
        {
            // Show pause menu
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);
            }
        }
        
        private void HandleGameOverState()
        {
            // Show game over panel
            if (gameOverPanel != null)
            {
                // Update stats displayed on game over screen
                if (GameManager.Instance != null)
                {
                    UpdateGameOverStats();
                }
                
                gameOverPanel.SetActive(true);
            }
        }
        
        private void HandleVictoryState()
        {
            // Show victory panel
            if (victoryPanel != null)
            {
                // Update stats displayed on victory screen
                if (GameManager.Instance != null)
                {
                    UpdateVictoryStats();
                }
                
                victoryPanel.SetActive(true);
            }
        }
        
        private void HandleConnectingState()
        {
            // Usually handled by network connection UI
            // Just ensure other UI is hidden
            mainMenuObject.SetActive(false);
            
            if (hud != null)
            {
                hud.gameObject.SetActive(false);
            }
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
        }
        
        private void UpdateGameOverStats()
        {
            // Find and update text elements in the game over panel with stats
            // This would be implemented based on your UI structure
        }
        
        private void UpdateVictoryStats()
        {
            // Find and update text elements in the victory panel with stats
            // This would be implemented based on your UI structure
        }
        
        public void StartNewGame(bool isHost, string playerName, string playerClass)
        {
            // Start fade out
            StartCoroutine(FadeOutAndStartGame(isHost, playerName, playerClass));
        }
        
        private IEnumerator FadeOutAndStartGame(bool isHost, string playerName, string playerClass)
        {
            _isTransitioning = true;
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Initialize the game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame(isHost);
            }
            
            // Fade back in
            yield return StartCoroutine(FadeIn());
            
            _isTransitioning = false;
        }
        
        private IEnumerator FadeIn()
        {
            if (_fadeCanvasGroup != null)
            {
                // Fade from black to clear
                float t = 0f;
                while (t < fadeInTime)
                {
                    t += Time.deltaTime;
                    _fadeCanvasGroup.alpha = Mathf.Clamp01(1f - (t / fadeInTime));
                    yield return null;
                }
                
                _fadeCanvasGroup.alpha = 0f;
                screenFadeOverlay.SetActive(false);
            }
        }
        
        private IEnumerator FadeOut()
        {
            if (_fadeCanvasGroup != null)
            {
                screenFadeOverlay.SetActive(true);
                
                // Fade from clear to black
                float t = 0f;
                while (t < fadeOutTime)
                {
                    t += Time.deltaTime;
                    _fadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeOutTime);
                    yield return null;
                }
                
                _fadeCanvasGroup.alpha = 1f;
            }
        }
        
        public void LoadNextLevel()
        {
            StartCoroutine(TransitionToNextLevel());
        }
        
        private IEnumerator TransitionToNextLevel()
        {
            _isTransitioning = true;
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Generate next level
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceToNextLevel();
            }
            
            // Fade back in
            yield return StartCoroutine(FadeIn());
            
            _isTransitioning = false;
        }
        
        // Button handlers for UI
        public void PauseGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }
        
        public void ResumeGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }
        
        public void ReturnToMainMenu()
        {
            if (GameManager.Instance != null)
            {
                StartCoroutine(FadeOutAndReturnToMainMenu());
            }
        }
        
        private IEnumerator FadeOutAndReturnToMainMenu()
        {
            _isTransitioning = true;
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Return to main menu
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMainMenu();
            }
            
            // Fade back in
            yield return StartCoroutine(FadeIn());
            
            _isTransitioning = false;
        }
        
        public void QuitGame()
        {
            Application.Quit();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}