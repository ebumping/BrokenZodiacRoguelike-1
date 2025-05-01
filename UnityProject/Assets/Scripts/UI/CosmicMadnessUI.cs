using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CodexOfTheBrokenZodiac.Core;
using CodexOfTheBrokenZodiac.Resources;
using TMPro;

namespace CodexOfTheBrokenZodiac.UI
{
    // This class handles the UI for the Cosmic Madness skill tree
    public class CosmicMadnessUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private Transform skillNodesContainer;
        [SerializeField] private GameObject skillNodePrefab;
        [SerializeField] private GameObject skillConnectionPrefab;
        [SerializeField] private GameObject categoryLabelPrefab;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityDescriptionText;
        [SerializeField] private Image abilityIconImage;
        [SerializeField] private Button unlockButton;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject unavailableOverlay;
        [SerializeField] private GameObject zodiacIncompatibleWarning;
        [SerializeField] private RectTransform connectionContainer;
        
        [Header("Positioning")]
        [SerializeField] private float nodeSpacingX = 150f;
        [SerializeField] private float nodeSpacingY = 120f;
        [SerializeField] private float categorySpacing = 300f;
        
        [Header("Visuals")]
        [SerializeField] private Color availableNodeColor = Color.white;
        [SerializeField] private Color unlockedNodeColor = Color.green;
        [SerializeField] private Color lockedNodeColor = Color.gray;
        [SerializeField] private Color activeNodeColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color zodiacLockedNodeColor = new Color(0.8f, 0f, 0.8f);
        [SerializeField] private Sprite defaultAbilityIcon;
        [SerializeField] private Color[] categoryColors; // Colors for different categories
        [SerializeField] private Sprite[] categoryBackgrounds; // Background images for different categories
        
        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.2f;
        
        // Runtime references
        private Dictionary<CosmicMadnessAbility, GameObject> _abilityNodes = new Dictionary<CosmicMadnessAbility, GameObject>();
        private Dictionary<string, List<GameObject>> _connectionsByKey = new Dictionary<string, List<GameObject>>();
        private CosmicMadnessAbility _selectedAbility = null;
        private int _localPlayerId;
        private PlayerController _localPlayer;
        private bool _isDirty = true;
        
        private void Start()
        {
            // Get local player
            _localPlayer = GameManager.Instance?.LocalPlayer;
            if (_localPlayer != null)
                _localPlayerId = _localPlayer.NetworkId;
                
            // Hide panel initially
            if (skillTreePanel != null)
                skillTreePanel.SetActive(false);
                
            // Register for events
            if (CosmicMadnessManager.Instance != null)
            {
                CosmicMadnessManager.Instance.OnAbilityUnlocked += OnAbilityUnlocked;
                CosmicMadnessManager.Instance.OnAbilityActivated += OnAbilityActivated;
                CosmicMadnessManager.Instance.OnAbilityDeactivated += OnAbilityDeactivated;
                CosmicMadnessManager.Instance.OnMadnessPointsChanged += OnMadnessPointsChanged;
            }
            
            // Setup UI components
            if (unlockButton != null)
                unlockButton.onClick.AddListener(OnUnlockButtonClicked);
                
            // Mark for refresh
            _isDirty = true;
        }
        
        private void OnDestroy()
        {
            // Unregister events
            if (CosmicMadnessManager.Instance != null)
            {
                CosmicMadnessManager.Instance.OnAbilityUnlocked -= OnAbilityUnlocked;
                CosmicMadnessManager.Instance.OnAbilityActivated -= OnAbilityActivated;
                CosmicMadnessManager.Instance.OnAbilityDeactivated -= OnAbilityDeactivated;
                CosmicMadnessManager.Instance.OnMadnessPointsChanged -= OnMadnessPointsChanged;
            }
            
            // Clean up UI components
            if (unlockButton != null)
                unlockButton.onClick.RemoveListener(OnUnlockButtonClicked);
        }
        
        private void Update()
        {
            // Check for input to toggle panel
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleSkillTreePanel();
            }
            
            // Refresh the UI if needed
            if (_isDirty && skillTreePanel.activeSelf)
            {
                RefreshSkillTree();
                _isDirty = false;
            }
            
            // Update node animations
            UpdateNodeAnimations();
        }
        
        // Toggle skill tree panel visibility
        public void ToggleSkillTreePanel()
        {
            if (skillTreePanel != null)
            {
                skillTreePanel.SetActive(!skillTreePanel.activeSelf);
                
                if (skillTreePanel.activeSelf)
                {
                    // Refresh when opened
                    RefreshSkillTree();
                }
                else
                {
                    // Clear selection when closed
                    SetSelectedAbility(null);
                }
            }
        }
        
        // Refresh the entire skill tree UI
        private void RefreshSkillTree()
        {
            // Update player reference if needed
            if (_localPlayer == null)
            {
                _localPlayer = GameManager.Instance?.LocalPlayer;
                if (_localPlayer != null)
                    _localPlayerId = _localPlayer.NetworkId;
            }
            
            // Clear existing nodes
            ClearSkillTree();
            
            if (CosmicMadnessManager.Instance == null || _localPlayer == null)
                return;
                
            // Update points display
            UpdatePointsDisplay();
            
            // Get available abilities
            var availableAbilities = CosmicMadnessManager.Instance.GetAvailableAbilities(_localPlayerId);
            
            // Group by category
            Dictionary<CosmicMadnessCategory, List<CosmicMadnessAbility>> abilitiesByCategory = 
                new Dictionary<CosmicMadnessCategory, List<CosmicMadnessAbility>>();
                
            foreach (var ability in availableAbilities)
            {
                if (!abilitiesByCategory.ContainsKey(ability.Category))
                    abilitiesByCategory[ability.Category] = new List<CosmicMadnessAbility>();
                    
                abilitiesByCategory[ability.Category].Add(ability);
            }
            
            // Create category columns
            float currentX = 0f;
            
            foreach (CosmicMadnessCategory category in System.Enum.GetValues(typeof(CosmicMadnessCategory)))
            {
                if (!abilitiesByCategory.ContainsKey(category))
                    continue;
                    
                // Create category label
                CreateCategoryLabel(category, new Vector2(currentX, 0));
                
                // Create nodes for this category
                var abilities = abilitiesByCategory[category];
                CreateNodesForCategory(abilities, category, currentX);
                
                // Move to next column
                currentX += categorySpacing;
            }
            
            // Create connections between nodes
            CreateNodeConnections();
            
            // No selection initially
            SetSelectedAbility(null);
        }
        
        // Clear all nodes from the skill tree
        private void ClearSkillTree()
        {
            if (skillNodesContainer == null)
                return;
                
            // Clear node references
            _abilityNodes.Clear();
            
            // Clear connections
            foreach (var connectionList in _connectionsByKey.Values)
            {
                foreach (var connection in connectionList)
                {
                    Destroy(connection);
                }
            }
            _connectionsByKey.Clear();
            
            // Destroy all children
            foreach (Transform child in skillNodesContainer)
            {
                Destroy(child.gameObject);
            }
            
            if (connectionContainer != null)
            {
                foreach (Transform child in connectionContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        // Create a category label
        private void CreateCategoryLabel(CosmicMadnessCategory category, Vector2 position)
        {
            if (categoryLabelPrefab == null || skillNodesContainer == null)
                return;
                
            GameObject labelObj = Instantiate(categoryLabelPrefab, skillNodesContainer);
            RectTransform rect = labelObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(position.x, 250f); // Position at top
            }
            
            // Set text
            TextMeshProUGUI text = labelObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = GetCategoryName(category);
                
                // Set color based on category
                int categoryIndex = (int)category;
                if (categoryColors != null && categoryIndex < categoryColors.Length)
                {
                    text.color = categoryColors[categoryIndex];
                }
            }
            
            // Set background
            Image bg = labelObj.GetComponent<Image>();
            if (bg != null)
            {
                int categoryIndex = (int)category;
                if (categoryBackgrounds != null && categoryIndex < categoryBackgrounds.Length)
                {
                    bg.sprite = categoryBackgrounds[categoryIndex];
                }
            }
        }
        
        // Create nodes for a category
        private void CreateNodesForCategory(List<CosmicMadnessAbility> abilities, CosmicMadnessCategory category, float startX)
        {
            if (skillNodePrefab == null || skillNodesContainer == null || abilities == null)
                return;
                
            // Sort abilities by prerequisites
            SortAbilitiesByDependencies(abilities);
            
            // Create nodes in a grid
            int nodeCount = abilities.Count;
            int maxNodesPerRow = 3; // Adjust as needed
            
            for (int i = 0; i < nodeCount; i++)
            {
                int row = i / maxNodesPerRow;
                int col = i % maxNodesPerRow;
                
                float posX = startX + (col - 1) * nodeSpacingX;
                float posY = 100f - row * nodeSpacingY;
                
                CreateAbilityNode(abilities[i], new Vector2(posX, posY));
            }
        }
        
        // Sort abilities so prerequisites come before the abilities that depend on them
        private void SortAbilitiesByDependencies(List<CosmicMadnessAbility> abilities)
        {
            abilities.Sort((a, b) => {
                // Check if b depends on a
                if (a.Prerequisites != null)
                {
                    foreach (var prereq in a.Prerequisites)
                    {
                        if (prereq == b)
                            return 1; // a comes after b
                    }
                }
                
                // Check if a depends on b
                if (b.Prerequisites != null)
                {
                    foreach (var prereq in b.Prerequisites)
                    {
                        if (prereq == a)
                            return -1; // a comes before b
                    }
                }
                
                // Default sorting by name
                return a.AbilityName.CompareTo(b.AbilityName);
            });
        }
        
        // Create a node for an ability
        private void CreateAbilityNode(CosmicMadnessAbility ability, Vector2 position)
        {
            if (ability == null || skillNodePrefab == null || skillNodesContainer == null)
                return;
                
            GameObject nodeObj = Instantiate(skillNodePrefab, skillNodesContainer);
            RectTransform rect = nodeObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = position;
            }
            
            // Store reference
            _abilityNodes[ability] = nodeObj;
            
            // Set node data
            SkillNodeData nodeData = nodeObj.GetComponent<SkillNodeData>();
            if (nodeData != null)
            {
                nodeData.SetAbility(ability);
            }
            
            // Set icon
            Image iconImage = nodeObj.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = ability.Icon ?? defaultAbilityIcon;
            }
            
            // Set color based on state
            UpdateNodeVisuals(ability, nodeObj);
            
            // Add click handler
            Button button = nodeObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => { SetSelectedAbility(ability); });
            }
        }
        
        // Create connections between nodes based on prerequisites
        private void CreateNodeConnections()
        {
            if (connectionContainer == null || skillConnectionPrefab == null)
                return;
                
            foreach (var entry in _abilityNodes)
            {
                CosmicMadnessAbility ability = entry.Key;
                GameObject nodeObj = entry.Value;
                
                if (ability.Prerequisites == null || ability.Prerequisites.Length == 0)
                    continue;
                    
                foreach (var prerequisite in ability.Prerequisites)
                {
                    if (prerequisite == null || !_abilityNodes.ContainsKey(prerequisite))
                        continue;
                        
                    GameObject prereqNodeObj = _abilityNodes[prerequisite];
                    
                    // Create connection line
                    CreateConnectionLine(prereqNodeObj, nodeObj, ability, prerequisite);
                }
            }
        }
        
        // Create a connection line between two nodes
        private void CreateConnectionLine(GameObject fromNode, GameObject toNode, CosmicMadnessAbility toAbility, CosmicMadnessAbility fromAbility)
        {
            if (fromNode == null || toNode == null || connectionContainer == null || skillConnectionPrefab == null)
                return;
                
            // Create connection object
            GameObject connectionObj = Instantiate(skillConnectionPrefab, connectionContainer);
            
            // Position line renderer
            RectTransform fromRect = fromNode.GetComponent<RectTransform>();
            RectTransform toRect = toNode.GetComponent<RectTransform>();
            
            if (fromRect == null || toRect == null)
                return;
                
            // Get positions
            Vector3 startPos = fromRect.position;
            Vector3 endPos = toRect.position;
            
            // Create unique key for this connection
            string connectionKey = $"{fromAbility.AbilityName}_{toAbility.AbilityName}";
            
            // Store connection
            if (!_connectionsByKey.ContainsKey(connectionKey))
                _connectionsByKey[connectionKey] = new List<GameObject>();
                
            _connectionsByKey[connectionKey].Add(connectionObj);
            
            // Set line positions
            LineRenderer line = connectionObj.GetComponent<LineRenderer>();
            if (line != null)
            {
                // Convert to local space
                Vector3[] positions = new Vector3[2];
                positions[0] = connectionContainer.InverseTransformPoint(startPos);
                positions[1] = connectionContainer.InverseTransformPoint(endPos);
                
                line.positionCount = 2;
                line.SetPositions(positions);
                
                // Set color based on unlock state
                bool fromUnlocked = CosmicMadnessManager.Instance?.IsAbilityUnlocked(_localPlayerId, fromAbility) ?? false;
                bool toUnlocked = CosmicMadnessManager.Instance?.IsAbilityUnlocked(_localPlayerId, toAbility) ?? false;
                
                if (fromUnlocked && toUnlocked)
                {
                    line.startColor = unlockedNodeColor;
                    line.endColor = unlockedNodeColor;
                }
                else if (fromUnlocked)
                {
                    line.startColor = unlockedNodeColor;
                    line.endColor = availableNodeColor;
                }
                else
                {
                    line.startColor = lockedNodeColor;
                    line.endColor = lockedNodeColor;
                }
            }
        }
        
        // Update all node animations
        private void UpdateNodeAnimations()
        {
            if (_localPlayer == null || CosmicMadnessManager.Instance == null)
                return;
                
            float time = Time.time * pulseSpeed;
            
            foreach (var entry in _abilityNodes)
            {
                CosmicMadnessAbility ability = entry.Key;
                GameObject nodeObj = entry.Value;
                
                if (ability == null || nodeObj == null)
                    continue;
                    
                // Active abilities should pulse
                bool isActive = CosmicMadnessManager.Instance.IsAbilityActive(_localPlayerId, ability);
                
                if (isActive)
                {
                    float pulse = 1f + Mathf.Sin(time) * pulseIntensity;
                    nodeObj.transform.localScale = new Vector3(pulse, pulse, 1f);
                }
                else
                {
                    nodeObj.transform.localScale = Vector3.one;
                }
            }
        }
        
        // Update node visuals based on state
        private void UpdateNodeVisuals(CosmicMadnessAbility ability, GameObject nodeObj)
        {
            if (ability == null || nodeObj == null || CosmicMadnessManager.Instance == null)
                return;
                
            Image iconImage = nodeObj.GetComponentInChildren<Image>();
            if (iconImage == null)
                return;
                
            bool isUnlocked = CosmicMadnessManager.Instance.IsAbilityUnlocked(_localPlayerId, ability);
            bool isActive = CosmicMadnessManager.Instance.IsAbilityActive(_localPlayerId, ability);
            bool prereqsMet = ability.ArePrerequisitesMet(CosmicMadnessManager.Instance);
            bool isAvailable = ability.IsAvailableTo(_localPlayer);
            
            // Check if zodiac locked
            bool isZodiacCompatible = true;
            if (ability.IsZodiacLocked)
            {
                isZodiacCompatible = false;
                ZodiacSign playerSign = _localPlayer.GetZodiacSign();
                
                if (ability.CompatibleZodiacSigns != null)
                {
                    foreach (var sign in ability.CompatibleZodiacSigns)
                    {
                        if (sign == playerSign)
                        {
                            isZodiacCompatible = true;
                            break;
                        }
                    }
                }
            }
            
            // Set color based on state
            if (isActive)
            {
                iconImage.color = activeNodeColor;
            }
            else if (isUnlocked)
            {
                iconImage.color = unlockedNodeColor;
            }
            else if (!isAvailable || !prereqsMet)
            {
                iconImage.color = lockedNodeColor;
            }
            else if (!isZodiacCompatible)
            {
                iconImage.color = zodiacLockedNodeColor;
            }
            else
            {
                iconImage.color = availableNodeColor;
            }
            
            // Add visual indicator for zodiac locked
            Transform zodiacLockIcon = nodeObj.transform.Find("ZodiacLockIcon");
            if (zodiacLockIcon != null)
            {
                zodiacLockIcon.gameObject.SetActive(ability.IsZodiacLocked);
            }
        }
        
        // Set the selected ability and update details panel
        private void SetSelectedAbility(CosmicMadnessAbility ability)
        {
            _selectedAbility = ability;
            
            if (ability == null)
            {
                // Hide details
                if (abilityNameText != null)
                    abilityNameText.text = "";
                    
                if (abilityDescriptionText != null)
                    abilityDescriptionText.text = "";
                    
                if (abilityIconImage != null)
                    abilityIconImage.gameObject.SetActive(false);
                    
                if (unlockButton != null)
                    unlockButton.gameObject.SetActive(false);
                    
                if (lockedOverlay != null)
                    lockedOverlay.SetActive(false);
                    
                if (unavailableOverlay != null)
                    unavailableOverlay.SetActive(false);
                    
                if (zodiacIncompatibleWarning != null)
                    zodiacIncompatibleWarning.SetActive(false);
                    
                return;
            }
            
            // Update details
            if (abilityNameText != null)
                abilityNameText.text = ability.AbilityName;
                
            if (abilityDescriptionText != null)
            {
                if (_localPlayer != null)
                {
                    ZodiacSign playerSign = _localPlayer.GetZodiacSign();
                    abilityDescriptionText.text = ability.GetDetailedDescription(playerSign);
                }
                else
                {
                    abilityDescriptionText.text = ability.Description;
                }
            }
            
            if (abilityIconImage != null)
            {
                abilityIconImage.gameObject.SetActive(true);
                abilityIconImage.sprite = ability.Icon ?? defaultAbilityIcon;
            }
            
            // Update button state
            UpdateUnlockButtonState();
        }
        
        // Update the unlock button state based on selected ability
        private void UpdateUnlockButtonState()
        {
            if (_selectedAbility == null || unlockButton == null || 
                lockedOverlay == null || unavailableOverlay == null || 
                CosmicMadnessManager.Instance == null || _localPlayer == null)
                return;
                
            bool isUnlocked = CosmicMadnessManager.Instance.IsAbilityUnlocked(_localPlayerId, _selectedAbility);
            bool prereqsMet = _selectedAbility.ArePrerequisitesMet(CosmicMadnessManager.Instance);
            bool isAvailable = _selectedAbility.IsAvailableTo(_localPlayer);
            float points = CosmicMadnessManager.Instance.GetAvailableMadnessPoints(_localPlayerId);
            bool enoughPoints = points >= _selectedAbility.RequiredPoints;
            
            // Check sanity requirement
            SanityState playerState = SanitySystem.Instance?.GetPlayerSanityState(_localPlayerId) ?? SanityState.Normal;
            bool meetsStatRequirement = (int)playerState >= (int)_selectedAbility.MinimumSanityState;
            
            // Check if zodiac compatible
            bool isZodiacCompatible = true;
            if (_selectedAbility.IsZodiacLocked)
            {
                isZodiacCompatible = false;
                ZodiacSign playerSign = _localPlayer.GetZodiacSign();
                
                if (_selectedAbility.CompatibleZodiacSigns != null)
                {
                    foreach (var sign in _selectedAbility.CompatibleZodiacSigns)
                    {
                        if (sign == playerSign)
                        {
                            isZodiacCompatible = true;
                            break;
                        }
                    }
                }
            }
            
            // Update UI elements
            unlockButton.gameObject.SetActive(!isUnlocked && prereqsMet && isAvailable && isZodiacCompatible && meetsStatRequirement);
            unlockButton.interactable = enoughPoints;
            
            if (unlockButton.gameObject.activeSelf)
            {
                TextMeshProUGUI buttonText = unlockButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"Unlock ({_selectedAbility.RequiredPoints} points)";
                }
            }
            
            // Update overlays
            lockedOverlay.SetActive(!isUnlocked && (!prereqsMet || !meetsStatRequirement) && isZodiacCompatible);
            unavailableOverlay.SetActive(!isUnlocked && !isAvailable && isZodiacCompatible);
            
            if (zodiacIncompatibleWarning != null)
                zodiacIncompatibleWarning.SetActive(!isZodiacCompatible);
                
            // Update locked overlay text
            TextMeshProUGUI lockedText = lockedOverlay.GetComponentInChildren<TextMeshProUGUI>();
            if (lockedText != null)
            {
                if (!prereqsMet)
                {
                    lockedText.text = "Unlock prerequisites first";
                }
                else if (!meetsStatRequirement)
                {
                    lockedText.text = $"Requires {_selectedAbility.MinimumSanityState} Sanity State";
                }
            }
        }
        
        // Handle unlock button click
        private void OnUnlockButtonClicked()
        {
            if (_selectedAbility == null || CosmicMadnessManager.Instance == null)
                return;
                
            // Attempt to unlock
            bool success = CosmicMadnessManager.Instance.UnlockAbility(_localPlayerId, _selectedAbility);
            
            if (success)
            {
                // Play unlock effect
                // Update UI
                UpdateUnlockButtonState();
                UpdatePointsDisplay();
            }
        }
        
        // Update points display
        private void UpdatePointsDisplay()
        {
            if (pointsText == null || CosmicMadnessManager.Instance == null)
                return;
                
            float points = CosmicMadnessManager.Instance.GetAvailableMadnessPoints(_localPlayerId);
            float maxPoints = CosmicMadnessManager.Instance.GetMaxMadnessPoints(_localPlayerId);
            
            pointsText.text = $"Cosmic Madness Points: {points:F1}/{maxPoints:F0}";
        }
        
        #region Event Handlers
        
        // Handle ability unlocked event
        private void OnAbilityUnlocked(int playerId, CosmicMadnessAbility ability)
        {
            if (playerId != _localPlayerId || ability == null)
                return;
                
            // Update node visuals
            if (_abilityNodes.ContainsKey(ability))
            {
                UpdateNodeVisuals(ability, _abilityNodes[ability]);
            }
            
            // Update connections
            RefreshConnections(ability);
            
            // Update details panel if this is the selected ability
            if (_selectedAbility == ability)
            {
                UpdateUnlockButtonState();
            }
            
            // Update points display
            UpdatePointsDisplay();
        }
        
        // Handle ability activated event
        private void OnAbilityActivated(int playerId, CosmicMadnessAbility ability)
        {
            if (playerId != _localPlayerId || ability == null)
                return;
                
            // Update node visuals
            if (_abilityNodes.ContainsKey(ability))
            {
                UpdateNodeVisuals(ability, _abilityNodes[ability]);
            }
        }
        
        // Handle ability deactivated event
        private void OnAbilityDeactivated(int playerId, CosmicMadnessAbility ability)
        {
            if (playerId != _localPlayerId || ability == null)
                return;
                
            // Update node visuals
            if (_abilityNodes.ContainsKey(ability))
            {
                UpdateNodeVisuals(ability, _abilityNodes[ability]);
            }
        }
        
        // Handle madness points changed event
        private void OnMadnessPointsChanged(int playerId, float current, float max)
        {
            if (playerId != _localPlayerId)
                return;
                
            // Update points display
            UpdatePointsDisplay();
            
            // Update unlock button state
            if (_selectedAbility != null)
            {
                UpdateUnlockButtonState();
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        // Refresh connections for an ability
        private void RefreshConnections(CosmicMadnessAbility ability)
        {
            if (ability == null)
                return;
                
            // Refresh connections where this ability is a prerequisite
            foreach (var entry in _abilityNodes)
            {
                CosmicMadnessAbility otherAbility = entry.Key;
                
                if (otherAbility.Prerequisites != null)
                {
                    foreach (var prereq in otherAbility.Prerequisites)
                    {
                        if (prereq == ability)
                        {
                            // This connection needs refreshing
                            string connectionKey = $"{ability.AbilityName}_{otherAbility.AbilityName}";
                            
                            if (_connectionsByKey.ContainsKey(connectionKey))
                            {
                                foreach (var connectionObj in _connectionsByKey[connectionKey])
                                {
                                    if (connectionObj == null)
                                        continue;
                                        
                                    LineRenderer line = connectionObj.GetComponent<LineRenderer>();
                                    if (line != null)
                                    {
                                        bool fromUnlocked = CosmicMadnessManager.Instance?.IsAbilityUnlocked(_localPlayerId, ability) ?? false;
                                        bool toUnlocked = CosmicMadnessManager.Instance?.IsAbilityUnlocked(_localPlayerId, otherAbility) ?? false;
                                        
                                        if (fromUnlocked && toUnlocked)
                                        {
                                            line.startColor = unlockedNodeColor;
                                            line.endColor = unlockedNodeColor;
                                        }
                                        else if (fromUnlocked)
                                        {
                                            line.startColor = unlockedNodeColor;
                                            line.endColor = availableNodeColor;
                                        }
                                        else
                                        {
                                            line.startColor = lockedNodeColor;
                                            line.endColor = lockedNodeColor;
                                        }
                                    }
                                }
                            }
                            
                            // Also update the node itself as it might now be available
                            if (_abilityNodes.ContainsKey(otherAbility))
                            {
                                UpdateNodeVisuals(otherAbility, _abilityNodes[otherAbility]);
                            }
                        }
                    }
                }
            }
        }
        
        // Get category name from enum
        private string GetCategoryName(CosmicMadnessCategory category)
        {
            switch (category)
            {
                case CosmicMadnessCategory.Transformation:
                    return "Transformation";
                case CosmicMadnessCategory.Perception:
                    return "Perception";
                case CosmicMadnessCategory.Manifestation:
                    return "Manifestation";
                case CosmicMadnessCategory.Destruction:
                    return "Destruction";
                case CosmicMadnessCategory.Manipulation:
                    return "Manipulation";
                case CosmicMadnessCategory.Transcendence:
                    return "Transcendence";
                default:
                    return "Unknown";
            }
        }
        
        #endregion
    }
    
    // Component to hold node data
    public class SkillNodeData : MonoBehaviour
    {
        private CosmicMadnessAbility _ability;
        
        public CosmicMadnessAbility Ability => _ability;
        
        public void SetAbility(CosmicMadnessAbility ability)
        {
            _ability = ability;
        }
    }
}