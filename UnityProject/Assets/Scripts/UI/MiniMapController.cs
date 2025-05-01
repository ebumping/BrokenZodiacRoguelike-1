using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.UI
{
    public class MiniMapController : MonoBehaviour
    {
        // References
        [Header("References")]
        [SerializeField] private RawImage miniMapRenderTexture;
        [SerializeField] private RectTransform miniMapContainer;
        [SerializeField] private GameObject roomIconPrefab;
        [SerializeField] private GameObject playerIconPrefab;
        [SerializeField] private GameObject bossIconPrefab;
        [SerializeField] private GameObject itemIconPrefab;
        [SerializeField] private GameObject doorIconPrefab;
        [SerializeField] private Camera miniMapCamera;
        
        // Settings
        [Header("Settings")]
        [SerializeField] private Color visitedRoomColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        [SerializeField] private Color currentRoomColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        [SerializeField] private Color unexploredRoomColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color bossRoomColor = new Color(1.0f, 0.2f, 0.2f, 1.0f);
        [SerializeField] private float mapScale = 10f;
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 2.0f;
        [SerializeField] private float panSpeed = 10f;
        [SerializeField] private float distortionIntensity = 0.05f;
        [SerializeField] private float distortionSpeed = 0.5f;
        
        // Runtime variables
        private float _currentZoom = 1.0f;
        private Vector2 _panOffset = Vector2.zero;
        private bool _isExpanded = false;
        private Dictionary<string, GameObject> _roomIcons = new Dictionary<string, GameObject>();
        private Dictionary<int, GameObject> _playerIcons = new Dictionary<int, GameObject>();
        private Dictionary<string, GameObject> _itemIcons = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> _doorIcons = new Dictionary<string, GameObject>();
        private List<LineRenderer> _connectionLines = new List<LineRenderer>();
        private Material _distortionMaterial;
        private Material _blurMaterial;
        private RenderTexture _mapRenderTexture;
        private RenderTexture _distortionRenderTexture;
        private float _distortionTimer = 0f;
        
        // Properties
        private ProcGenManager ProcGen => ProcGenManager.Instance;
        private GameManager GameManager => GameManager.Instance;
        
        private void Awake()
        {
            if (miniMapCamera == null)
            {
                // Create minimap camera if not assigned
                GameObject cameraObj = new GameObject("MiniMapCamera");
                miniMapCamera = cameraObj.AddComponent<Camera>();
                miniMapCamera.orthographic = true;
                miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
                miniMapCamera.backgroundColor = new Color(0, 0, 0, 0);
                miniMapCamera.cullingMask = LayerMask.GetMask("MiniMap");
                cameraObj.transform.SetParent(transform);
                cameraObj.transform.localPosition = new Vector3(0, 0, -10);
            }
            
            // Create render textures
            _mapRenderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            _mapRenderTexture.filterMode = FilterMode.Bilinear;
            _distortionRenderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            _distortionRenderTexture.filterMode = FilterMode.Bilinear;
            
            // Create distortion materials
            _distortionMaterial = new Material(Shader.Find("Custom/CosmicDistortion"));
            if (_distortionMaterial == null)
            {
                // Fallback if custom shader not found
                _distortionMaterial = new Material(Shader.Find("Standard"));
                Debug.LogWarning("Cosmic Distortion shader not found. Using fallback shader.");
            }
            
            _blurMaterial = new Material(Shader.Find("Custom/CosmicBlur"));
            if (_blurMaterial == null)
            {
                // Fallback if custom shader not found
                _blurMaterial = new Material(Shader.Find("Standard"));
                Debug.LogWarning("Cosmic Blur shader not found. Using fallback shader.");
            }
            
            // Set up the minimap render texture
            miniMapCamera.targetTexture = _mapRenderTexture;
            miniMapRenderTexture.texture = _distortionRenderTexture;
        }
        
        private void OnEnable()
        {
            // Subscribe to events
            if (ProcGen != null)
            {
                ProcGen.OnLevelGenerated += RefreshMiniMap;
                ProcGen.OnRoomDiscovered += AddRoom;
                ProcGen.OnRoomVisited += UpdateRoomVisibility;
            }
            
            if (GameManager != null)
            {
                GameManager.OnPlayerSpawned += AddPlayer;
                GameManager.OnPlayerDespawned += RemovePlayer;
                GameManager.OnItemSpawned += AddItem;
                GameManager.OnItemCollected += RemoveItem;
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            if (ProcGen != null)
            {
                ProcGen.OnLevelGenerated -= RefreshMiniMap;
                ProcGen.OnRoomDiscovered -= AddRoom;
                ProcGen.OnRoomVisited -= UpdateRoomVisibility;
            }
            
            if (GameManager != null)
            {
                GameManager.OnPlayerSpawned -= AddPlayer;
                GameManager.OnPlayerDespawned -= RemovePlayer;
                GameManager.OnItemSpawned -= AddItem;
                GameManager.OnItemCollected -= RemoveItem;
            }
        }
        
        private void Update()
        {
            UpdateMiniMapCamera();
            UpdatePlayerPositions();
            UpdateCosmicDistortionEffect();
            HandleInput();
        }
        
        private void LateUpdate()
        {
            // Apply distortion effect
            ApplyDistortionEffect();
        }
        
        private void OnDestroy()
        {
            // Clean up render textures
            if (_mapRenderTexture != null)
                _mapRenderTexture.Release();
                
            if (_distortionRenderTexture != null)
                _distortionRenderTexture.Release();
        }
        
        #region Map Initialization and Refresh
        
        // Completely rebuild the minimap
        public void RefreshMiniMap()
        {
            ClearMiniMap();
            
            if (ProcGen == null) return;
            
            // Add all discovered rooms
            var rooms = ProcGen.GetAllRooms();
            foreach (var room in rooms)
            {
                if (room.IsDiscovered)
                    AddRoom(room);
            }
            
            // Add connections between rooms
            AddRoomConnections();
            
            // Add items, players, etc.
            RefreshDynamicElements();
        }
        
        // Clear all icons and connections
        private void ClearMiniMap()
        {
            // Clear room icons
            foreach (var icon in _roomIcons.Values)
            {
                Destroy(icon);
            }
            _roomIcons.Clear();
            
            // Clear player icons
            foreach (var icon in _playerIcons.Values)
            {
                Destroy(icon);
            }
            _playerIcons.Clear();
            
            // Clear item icons
            foreach (var icon in _itemIcons.Values)
            {
                Destroy(icon);
            }
            _itemIcons.Clear();
            
            // Clear door icons
            foreach (var icon in _doorIcons.Values)
            {
                Destroy(icon);
            }
            _doorIcons.Clear();
            
            // Clear connection lines
            foreach (var line in _connectionLines)
            {
                Destroy(line.gameObject);
            }
            _connectionLines.Clear();
        }
        
        // Refresh players, items, etc.
        private void RefreshDynamicElements()
        {
            // Add all players
            var players = GameManager?.GetAllPlayers();
            if (players != null)
            {
                foreach (var player in players)
                {
                    AddPlayer(player);
                }
            }
            
            // Add all items
            var items = GameManager?.GetAllItems();
            if (items != null)
            {
                foreach (var item in items)
                {
                    AddItem(item);
                }
            }
        }
        
        #endregion
        
        #region Room Management
        
        // Add a room to the minimap
        public void AddRoom(Room room)
        {
            if (room == null || _roomIcons.ContainsKey(room.RoomId)) return;
            
            // Create room icon
            GameObject roomIcon = Instantiate(roomIconPrefab, miniMapContainer);
            roomIcon.name = $"Room_{room.RoomId}"; 
            
            // Set position based on room coordinates
            Vector2 roomPosition = new Vector2(room.GridPosition.x, room.GridPosition.y) * mapScale;
            roomIcon.transform.localPosition = roomPosition;
            
            // Set icon color based on room state
            Image roomImage = roomIcon.GetComponent<Image>();
            if (roomImage != null)
            {
                if (room.IsBossRoom)
                    roomImage.color = bossRoomColor;
                else if (room.IsVisited)
                    roomImage.color = visitedRoomColor;
                else if (room.IsDiscovered && !room.IsVisited)
                    roomImage.color = unexploredRoomColor;
                else
                    roomImage.color = new Color(0, 0, 0, 0); // Invisible
            }
            
            // Add to tracking dictionary
            _roomIcons[room.RoomId] = roomIcon;
            
            // If this is a boss room, add special icon
            if (room.IsBossRoom)
            {
                GameObject bossIcon = Instantiate(bossIconPrefab, roomIcon.transform);
                bossIcon.transform.localPosition = Vector3.zero;
            }
            
            // Add door icons for each connection
            AddDoorIcons(room);
        }
        
        // Add door icons for a room's connections
        private void AddDoorIcons(Room room)
        {
            if (room == null) return;
            
            // Only add doors for visited or adjacent to visited rooms
            if (!room.IsVisited && !room.IsAdjacentToVisited) return;
            
            // Add door icons for each connection
            foreach (var connection in room.Connections)
            {
                string doorId = $"{room.RoomId}_{connection.TargetRoomId}";
                
                // Skip if this door already exists
                if (_doorIcons.ContainsKey(doorId)) continue;
                
                // Get direction to the other room
                Vector2 direction = connection.Direction;
                
                // Create door icon
                GameObject doorIcon = Instantiate(doorIconPrefab, _roomIcons[room.RoomId].transform);
                doorIcon.name = doorId;
                
                // Position the door on the edge of the room in the direction of the connection
                doorIcon.transform.localPosition = direction * 0.5f;
                
                // Rotate the door icon to face the correct direction
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                doorIcon.transform.localRotation = Quaternion.Euler(0, 0, angle);
                
                // Add to tracking dictionary
                _doorIcons[doorId] = doorIcon;
                
                // Set visibility based on connection state
                doorIcon.SetActive(connection.IsDiscovered);
            }
        }
        
        // Update room visibility (called when a room is visited)
        public void UpdateRoomVisibility(Room room)
        {
            if (room == null || !_roomIcons.ContainsKey(room.RoomId)) return;
            
            GameObject roomIcon = _roomIcons[room.RoomId];
            Image roomImage = roomIcon.GetComponent<Image>();
            
            if (roomImage != null)
            {
                // Update colors based on room state
                if (room.IsBossRoom)
                    roomImage.color = bossRoomColor;
                else if (room.IsCurrentRoom)
                    roomImage.color = currentRoomColor;
                else if (room.IsVisited)
                    roomImage.color = visitedRoomColor;
                else if (room.IsDiscovered)
                    roomImage.color = unexploredRoomColor;
                else
                    roomImage.color = new Color(0, 0, 0, 0); // Invisible
            }
            
            // Update the visibility of any doors in this room
            foreach (var connection in room.Connections)
            {
                string doorId = $"{room.RoomId}_{connection.TargetRoomId}";
                if (_doorIcons.ContainsKey(doorId))
                {
                    _doorIcons[doorId].SetActive(connection.IsDiscovered);
                }
            }
            
            // When a room is visited, add all adjacent rooms to the minimap
            if (room.IsVisited)
            {
                foreach (var connection in room.Connections)
                {
                    Room adjacentRoom = ProcGen.GetRoomById(connection.TargetRoomId);
                    if (adjacentRoom != null && adjacentRoom.IsDiscovered && !_roomIcons.ContainsKey(adjacentRoom.RoomId))
                    {
                        AddRoom(adjacentRoom);
                    }
                }
            }
        }
        
        // Add connections between rooms
        private void AddRoomConnections()
        {
            if (ProcGen == null) return;
            
            // Clear existing connections
            foreach (var line in _connectionLines)
            {
                Destroy(line.gameObject);
            }
            _connectionLines.Clear();
            
            // Get all rooms
            var rooms = ProcGen.GetAllRooms();
            
            // Process each room's connections
            foreach (var room in rooms)
            {
                if (!room.IsDiscovered) continue;
                
                foreach (var connection in room.Connections)
                {
                    if (!connection.IsDiscovered) continue;
                    
                    Room targetRoom = ProcGen.GetRoomById(connection.TargetRoomId);
                    if (targetRoom != null && targetRoom.IsDiscovered)
                    {
                        // Create connection line
                        GameObject lineObj = new GameObject($"Connection_{room.RoomId}_{targetRoom.RoomId}");
                        lineObj.transform.SetParent(miniMapContainer);
                        lineObj.layer = LayerMask.NameToLayer("MiniMap");
                        
                        LineRenderer line = lineObj.AddComponent<LineRenderer>();
                        line.positionCount = 2;
                        line.startWidth = 2f;
                        line.endWidth = 2f;
                        line.material = new Material(Shader.Find("Sprites/Default"));
                        line.startColor = visitedRoomColor;
                        line.endColor = visitedRoomColor;
                        
                        // Set line positions
                        Vector3 startPos = _roomIcons[room.RoomId].transform.position;
                        Vector3 endPos = _roomIcons[targetRoom.RoomId].transform.position;
                        startPos.z = 1; // Behind room icons
                        endPos.z = 1;    
                        line.SetPosition(0, startPos);
                        line.SetPosition(1, endPos);
                        
                        // Add to tracking list
                        _connectionLines.Add(line);
                    }
                }
            }
        }
        
        #endregion
        
        #region Dynamic Elements (Players, Items)
        
        // Add a player icon to the minimap
        public void AddPlayer(PlayerController player)
        {
            if (player == null) return;
            
            // Create player icon
            GameObject playerIcon = Instantiate(playerIconPrefab, miniMapContainer);
            playerIcon.name = $"Player_{player.NetworkId}";
            
            // Track position
            Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.z) / mapScale;
            playerIcon.transform.localPosition = playerPosition;
            
            // Customize icon based on player class
            PlayerClass playerClass = player.PlayerClass;
            if (playerClass != null)
            {
                Image iconImage = playerIcon.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.color = playerClass.ClassColor;
                }
            }
            
            // Add to tracking dictionary
            _playerIcons[player.NetworkId] = playerIcon;
        }
        
        // Remove a player icon
        public void RemovePlayer(PlayerController player)
        {
            if (player == null) return;
            
            int playerId = player.NetworkId;
            if (_playerIcons.ContainsKey(playerId))
            {
                Destroy(_playerIcons[playerId]);
                _playerIcons.Remove(playerId);
            }
        }
        
        // Update all player positions
        private void UpdatePlayerPositions()
        {
            if (GameManager == null) return;
            
            var players = GameManager.GetAllPlayers();
            if (players == null) return;
            
            foreach (var player in players)
            {
                if (player != null && _playerIcons.ContainsKey(player.NetworkId))
                {
                    // Update icon position
                    GameObject icon = _playerIcons[player.NetworkId];
                    Vector2 scaledPosition = new Vector2(player.transform.position.x, player.transform.position.z) / mapScale;
                    icon.transform.localPosition = Vector3.Lerp(icon.transform.localPosition, scaledPosition, Time.deltaTime * 5f);
                }
            }
        }
        
        // Add an item icon
        public void AddItem(PickupItem item)
        {
            if (item == null) return;
            
            // Only add items in discovered rooms
            Room itemRoom = ProcGen.GetRoomAtPosition(item.transform.position);
            if (itemRoom == null || !itemRoom.IsDiscovered) return;
            
            // Create item icon
            GameObject itemIcon = Instantiate(itemIconPrefab, miniMapContainer);
            itemIcon.name = $"Item_{item.ItemId}";
            
            // Position icon
            Vector2 itemPosition = new Vector2(item.transform.position.x, item.transform.position.z) / mapScale;
            itemIcon.transform.localPosition = itemPosition;
            
            // Customize icon based on item type
            Image iconImage = itemIcon.GetComponent<Image>();
            if (iconImage != null && item.ItemType != null)
            {
                // Set color based on item rarity/type
                switch (item.ItemRarity)
                {
                    case ItemRarity.Common:
                        iconImage.color = Color.white;
                        break;
                    case ItemRarity.Uncommon:
                        iconImage.color = Color.green;
                        break;
                    case ItemRarity.Rare:
                        iconImage.color = Color.blue;
                        break;
                    case ItemRarity.Epic:
                        iconImage.color = new Color(0.5f, 0, 0.5f); // Purple
                        break;
                    case ItemRarity.Legendary:
                        iconImage.color = Color.yellow;
                        break;
                }
            }
            
            // Add to tracking dictionary
            _itemIcons[item.ItemId] = itemIcon;
        }
        
        // Remove an item icon
        public void RemoveItem(PickupItem item)
        {
            if (item == null) return;
            
            if (_itemIcons.ContainsKey(item.ItemId))
            {
                Destroy(_itemIcons[item.ItemId]);
                _itemIcons.Remove(item.ItemId);
            }
        }
        
        #endregion
        
        #region Camera Control and Input
        
        // Update the minimap camera position and zoom
        private void UpdateMiniMapCamera()
        {
            // Follow the player's current room in non-expanded mode
            if (!_isExpanded && GameManager != null && GameManager.LocalPlayer != null)
            {
                Room currentRoom = ProcGen.GetRoomAtPosition(GameManager.LocalPlayer.transform.position);
                if (currentRoom != null && _roomIcons.ContainsKey(currentRoom.RoomId))
                {
                    Vector3 roomPos = _roomIcons[currentRoom.RoomId].transform.position;
                    roomPos.z = -10; // Camera z position
                    
                    // Smoothly move camera to follow the player's room
                    miniMapCamera.transform.position = Vector3.Lerp(
                        miniMapCamera.transform.position,
                        roomPos + new Vector3(_panOffset.x, _panOffset.y, 0),
                        Time.deltaTime * 3f);
                }
            }
            
            // Apply zoom
            miniMapCamera.orthographicSize = 10f * (1f / _currentZoom);
        }
        
        // Handle user input for map navigation
        private void HandleInput()
        {
            // Check for map expansion toggle
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleExpandedMap();
            }
            
            // Only allow zooming and panning in expanded mode
            if (_isExpanded)
            {
                // Zooming
                float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
                if (scrollDelta != 0)
                {
                    _currentZoom = Mathf.Clamp(_currentZoom + scrollDelta * zoomSpeed, minZoom, maxZoom);
                }
                
                // Panning with WASD or arrow keys
                Vector2 panInput = Vector2.zero;
                
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    panInput.y += 1;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                    panInput.y -= 1;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    panInput.x -= 1;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    panInput.x += 1;
                
                if (panInput != Vector2.zero)
                {
                    _panOffset += panInput * panSpeed * Time.deltaTime * (1f / _currentZoom);
                }
            }
        }
        
        // Toggle between compact and expanded map
        public void ToggleExpandedMap()
        {
            _isExpanded = !_isExpanded;
            
            // Reset pan and zoom when collapsing
            if (!_isExpanded)
            {
                _currentZoom = 1.0f;
                _panOffset = Vector2.zero;
            }
            
            // Resize and reposition map
            if (_isExpanded)
            {
                // Expand to fullscreen
                miniMapContainer.anchorMin = new Vector2(0.1f, 0.1f);
                miniMapContainer.anchorMax = new Vector2(0.9f, 0.9f);
                miniMapContainer.anchoredPosition = Vector2.zero;
                miniMapContainer.sizeDelta = Vector2.zero;
                
                // Increase distortion during expanded mode
                distortionIntensity = 0.1f;
            }
            else
            {
                // Return to corner minimap
                miniMapContainer.anchorMin = new Vector2(0.02f, 0.75f);
                miniMapContainer.anchorMax = new Vector2(0.25f, 0.98f);
                miniMapContainer.anchoredPosition = Vector2.zero;
                miniMapContainer.sizeDelta = Vector2.zero;
                
                // Reduce distortion in mini mode
                distortionIntensity = 0.05f;
            }
        }
        
        #endregion
        
        #region Cosmic Distortion Effect
        
        // Update the distortion effect parameters
        private void UpdateCosmicDistortionEffect()
        {
            // Increment timer for animation
            _distortionTimer += Time.deltaTime * distortionSpeed;
            
            // Update distortion parameters
            if (_distortionMaterial != null)
            {
                _distortionMaterial.SetFloat("_Time", _distortionTimer);
                _distortionMaterial.SetFloat("_Intensity", distortionIntensity);
                
                // Modulate the distortion based on nearby cosmic entities
                float cosmicInfluence = CalculateCosmicInfluence();
                _distortionMaterial.SetFloat("_CosmicInfluence", cosmicInfluence);
            }
        }
        
        // Apply the distortion effect by rendering to the final texture
        private void ApplyDistortionEffect()
        {
            // Render the map to the first render texture
            miniMapCamera.Render();
            
            // Apply distortion by blitting between render textures
            Graphics.Blit(_mapRenderTexture, _distortionRenderTexture, _distortionMaterial);
        }
        
        // Calculate the cosmic influence based on game state
        private float CalculateCosmicInfluence()
        {
            float influence = 0f;
            
            // Increase influence based on number of discovered cosmic entities
            if (GameManager != null)
            {
                // For example, more influence when near a boss or a rift
                int cosmicEntities = GameManager.CosmicEntityCount;
                influence += cosmicEntities * 0.1f;
                
                // More influence at low health
                if (GameManager.LocalPlayer != null)
                {
                    float healthPercent = GameManager.LocalPlayer.CurrentHealth / GameManager.LocalPlayer.MaxHealth;
                    if (healthPercent < 0.3f)
                        influence += (1f - healthPercent) * 0.5f;
                }
                
                // Increase when in a corrupted room
                Room currentRoom = null;
                if (GameManager.LocalPlayer != null)
                {
                    currentRoom = ProcGen.GetRoomAtPosition(GameManager.LocalPlayer.transform.position);
                }
                
                if (currentRoom != null && currentRoom.IsCorrupted)
                {
                    influence += 0.3f;
                }
                
                // Increase based on corruption level in the level
                influence += ProcGen.CorruptionLevel * 0.2f;
            }
            
            // Ensure it stays within reasonable limits
            return Mathf.Clamp(influence, 0f, 1f);
        }
        
        #endregion
    }
}