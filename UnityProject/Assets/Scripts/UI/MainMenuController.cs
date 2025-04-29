using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Input References")]
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TMP_Dropdown classDropdown;
        [SerializeField] private TextMeshProUGUI classDescriptionLabel;
        
        [Header("Join Game Dialog")]
        [SerializeField] private GameObject joinGameDialog;
        [SerializeField] private TMP_InputField ipAddressInput;
        [SerializeField] private TMP_InputField portInput;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button joinCancelButton;
        
        [Header("Settings Dialog")]
        [SerializeField] private GameObject settingsDialog;
        [SerializeField] private Button settingsSaveButton;
        [SerializeField] private Button settingsCancelButton;
        
        [Header("Loading Panel")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI loadingLabel;
        
        [Header("Error Dialog")]
        [SerializeField] private GameObject errorDialog;
        [SerializeField] private TextMeshProUGUI errorLabel;
        
        [Header("Audio")]
        [SerializeField] private AudioSource buttonSound;
        [SerializeField] private AudioSource menuMusic;
        
        [Header("Version Label")]
        [SerializeField] private TextMeshProUGUI versionLabel;
        
        // Class data
        private Dictionary<string, ClassInfo> classes = new Dictionary<string, ClassInfo>();
        
        // Player settings
        private PlayerSettings playerSettings = new PlayerSettings();
        
        private void Awake()
        {
            // Initialize class data
            InitializeClassData();
        }
        
        private void Start()
        {
            // Set version label
            versionLabel.text = "v0.1.0 Alpha";
            
            // Connect button events
            hostButton.onClick.AddListener(OnHostButtonClicked);
            joinButton.onClick.AddListener(OnJoinButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            quitButton.onClick.AddListener(OnQuitButtonClicked);
            
            connectButton.onClick.AddListener(OnConnectButtonClicked);
            joinCancelButton.onClick.AddListener(OnJoinCancelButtonClicked);
            
            settingsSaveButton.onClick.AddListener(OnSettingsSaveButtonClicked);
            settingsCancelButton.onClick.AddListener(OnSettingsCancelButtonClicked);
            
            // Connect dropdown event
            classDropdown.onValueChanged.AddListener(OnClassSelected);
            
            // Populate class dropdown
            PopulateClassDropdown();
            
            // Load player settings
            LoadPlayerSettings();
            
            // Start menu music
            if (menuMusic != null)
            {
                menuMusic.Play();
            }
            
            // Hide dialogs and loading panel
            joinGameDialog.SetActive(false);
            settingsDialog.SetActive(false);
            loadingPanel.SetActive(false);
            errorDialog.SetActive(false);
        }
        
        private void InitializeClassData()
        {
            // Add character classes with their descriptions and colors
            classes.Add("OccultDetective", new ClassInfo
            {
                description = "Volition-driven detective with marksman rifle, rally aura, and interrupt shot.",
                color = new Color(0.2f, 0.4f, 0.8f) // Light Blue
            });
            
            classes.Add("ApostateMedium", new ClassInfo
            {
                description = "Inland Empire-based medium with shotgun, astral projection blink, and spirit shield.",
                color = new Color(0.6f, 0.2f, 0.8f) // Light Purple
            });
            
            classes.Add("IrredeemableDebtor", new ClassInfo
            {
                description = "Authority-driven debtor with gatling cannon, fear shout, and berserk swap-HP-for-damage.",
                color = new Color(0.8f, 0.2f, 0.2f) // Dark Red
            });
            
            classes.Add("ReclusiveArchivist", new ClassInfo
            {
                description = "Empathy-centered archivist with beam staff, slow-time field, and combo tracker.",
                color = new Color(0.2f, 0.8f, 0.4f) // Light Green
            });
            
            classes.Add("SeditiousOrator", new ClassInfo
            {
                description = "Suggestion-based orator with dual pistols, charm grenade, and critical trick-shot.",
                color = new Color(0.8f, 0.8f, 0.2f) // Yellow
            });
            
            classes.Add("PhantomConstable", new ClassInfo
            {
                description = "Esprit de Corps-based constable with lawgiver revolver, badge-ward totem, and revive tether.",
                color = new Color(0.7f, 0.7f, 0.7f) // Light Gray
            });
        }
        
        private void PopulateClassDropdown()
        {
            classDropdown.ClearOptions();
            List<string> options = new List<string>();
            
            foreach (string className in classes.Keys)
            {
                // Format class name for display (convert CamelCase to "Camel Case")
                string displayName = "";
                for (int i = 0; i < className.Length; i++)
                {
                    if (i > 0 && char.IsUpper(className[i]) && !char.IsUpper(className[i-1]))
                    {
                        displayName += " ";
                    }
                    displayName += className[i];
                }
                
                options.Add(displayName);
            }
            
            classDropdown.AddOptions(options);
            
            // Select first by default
            if (classDropdown.options.Count > 0)
            {
                classDropdown.value = 0;
                UpdateClassDescription(0);
            }
        }
        
        private void UpdateClassDescription(int index)
        {
            string[] classKeys = new string[classes.Keys.Count];
            classes.Keys.CopyTo(classKeys, 0);
            
            if (index >= 0 && index < classKeys.Length)
            {
                string className = classKeys[index];
                ClassInfo info = classes[className];
                
                classDescriptionLabel.text = info.description;
                classDescriptionLabel.color = info.color;
            }
        }
        
        private void LoadPlayerSettings()
        {
            // Load player name if saved
            string savedName = PlayerPrefs.GetString("PlayerName", "");
            if (string.IsNullOrEmpty(savedName))
            {
                savedName = "Player" + Random.Range(0, 1000).ToString();
            }
            playerNameInput.text = savedName;
            
            // Load class selection if saved
            int savedClass = PlayerPrefs.GetInt("PlayerClassIndex", 0);
            if (savedClass < classDropdown.options.Count)
            {
                classDropdown.value = savedClass;
                UpdateClassDescription(savedClass);
            }
        }
        
        private void SavePlayerSettings()
        {
            PlayerPrefs.SetString("PlayerName", playerNameInput.text);
            PlayerPrefs.SetInt("PlayerClassIndex", classDropdown.value);
            PlayerPrefs.Save();
        }
        
        private void OnHostButtonClicked()
        {
            PlayButtonSound();
            
            string playerName = playerNameInput.text;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Host" + Random.Range(0, 1000).ToString();
                playerNameInput.text = playerName;
            }
            
            string[] classKeys = new string[classes.Keys.Count];
            classes.Keys.CopyTo(classKeys, 0);
            string playerClass = classKeys[classDropdown.value];
            
            SavePlayerSettings();
            
            // Show loading panel
            loadingLabel.text = "Starting Game...";
            loadingPanel.SetActive(true);
            
            // Use coroutine to delay so UI can update
            StartCoroutine(DelayedStartGame(true, playerName, playerClass));
        }
        
        private IEnumerator DelayedStartGame(bool isHost, string playerName, string playerClass)
        {
            yield return new WaitForSeconds(0.5f);
            
            // Start new game via game manager
            GameManager.Instance.StartNewGame(isHost);
            
            // Add local player via network manager
            NetworkManager.Instance.StartHosting();
        }
        
        private void OnJoinButtonClicked()
        {
            PlayButtonSound();
            joinGameDialog.SetActive(true);
        }
        
        private void OnSettingsButtonClicked()
        {
            PlayButtonSound();
            settingsDialog.SetActive(true);
        }
        
        private void OnQuitButtonClicked()
        {
            PlayButtonSound();
            Application.Quit();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        
        private void OnConnectButtonClicked()
        {
            PlayButtonSound();
            
            string playerName = playerNameInput.text;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Player" + Random.Range(0, 1000).ToString();
                playerNameInput.text = playerName;
            }
            
            string[] classKeys = new string[classes.Keys.Count];
            classes.Keys.CopyTo(classKeys, 0);
            string playerClass = classKeys[classDropdown.value];
            
            string address = ipAddressInput.text;
            int port = int.Parse(portInput.text);
            
            SavePlayerSettings();
            
            // Hide dialog
            joinGameDialog.SetActive(false);
            
            // Show loading panel
            loadingLabel.text = "Connecting...";
            loadingPanel.SetActive(true);
            
            // Connect to server
            NetworkManager.Instance.ConnectToServer(address, port, playerName, playerClass);
        }
        
        private void OnJoinCancelButtonClicked()
        {
            PlayButtonSound();
            joinGameDialog.SetActive(false);
        }
        
        private void OnSettingsSaveButtonClicked()
        {
            PlayButtonSound();
            
            // Save settings logic would go here
            
            settingsDialog.SetActive(false);
        }
        
        private void OnSettingsCancelButtonClicked()
        {
            PlayButtonSound();
            settingsDialog.SetActive(false);
        }
        
        private void OnClassSelected(int index)
        {
            PlayButtonSound();
            UpdateClassDescription(index);
        }
        
        private void PlayButtonSound()
        {
            if (buttonSound != null)
            {
                buttonSound.Play();
            }
        }
        
        public void ShowConnectionError(string errorMessage)
        {
            // Hide loading panel
            loadingPanel.SetActive(false);
            
            // Show error dialog
            errorLabel.text = errorMessage;
            errorDialog.SetActive(true);
            
            // Auto-hide after 5 seconds
            StartCoroutine(AutoHideError());
        }
        
        private IEnumerator AutoHideError()
        {
            yield return new WaitForSeconds(5.0f);
            errorDialog.SetActive(false);
        }
        
        // Class to hold info about character classes
        private class ClassInfo
        {
            public string description;
            public Color color;
        }
        
        // Class to hold player settings
        private class PlayerSettings
        {
            public string playerName;
            public int classIndex;
        }
    }
}