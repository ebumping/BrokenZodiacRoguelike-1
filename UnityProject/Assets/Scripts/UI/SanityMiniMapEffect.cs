using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.UI
{
    // This component adds sanity-based distortion effects to the mini-map
    [RequireComponent(typeof(MiniMapController))]
    public class SanityMiniMapEffect : MonoBehaviour
    {
        [Header("Distortion Settings")]
        [SerializeField] private float maxRoomShakeIntensity = 5f;
        [SerializeField] private float maxConnectionDistortion = 10f;
        [SerializeField] private float minDistortionSanity = 0.4f; // Start distorting below this threshold
        [SerializeField] private float hallucination_RoomChance = 0.1f;
        [SerializeField] private float hallucination_ConnectionChance = 0.2f;
        [SerializeField] private float hallucinatedRoomDuration = 10f;
        
        [Header("Visual Effects")]
        [SerializeField] private Material normalMapMaterial;
        [SerializeField] private Material distortedMapMaterial;
        [SerializeField] private Color normalRoomColor = Color.white;
        [SerializeField] private Color[] corruptedRoomColors; // Colors for corrupted rooms at low sanity
        [SerializeField] private Sprite[] hallucinatedRoomIcons; // Special room icons that appear as hallucinations
        
        // Runtime variables
        private MiniMapController _miniMapController;
        private float _distortionFactor = 0f;
        private SanityState _currentSanityState = SanityState.Normal;
        private System.Collections.Generic.List<GameObject> _hallucinatedRooms = new System.Collections.Generic.List<GameObject>();
        private System.Collections.Generic.List<LineRenderer> _distortedConnections = new System.Collections.Generic.List<LineRenderer>();
        
        private void Awake()
        {
            _miniMapController = GetComponent<MiniMapController>();
        }
        
        private void OnEnable()
        {
            // Subscribe to events
            if (SanitySystem.Instance != null)
            {
                SanitySystem.Instance.OnPlayerSanityChanged += OnSanityChanged;
                SanitySystem.Instance.OnPlayerSanityStateChanged += OnSanityStateChanged;
                SanitySystem.Instance.OnPlayerHallucination += OnHallucination;
                
                // Initialize with current sanity if available
                int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
                if (localPlayerId != 0)
                {
                    float sanity = SanitySystem.Instance.GetPlayerSanityPercentage(localPlayerId);
                    SanityState state = SanitySystem.Instance.GetPlayerSanityState(localPlayerId);
                    UpdateDistortion(sanity, state);
                }
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            if (SanitySystem.Instance != null)
            {
                SanitySystem.Instance.OnPlayerSanityChanged -= OnSanityChanged;
                SanitySystem.Instance.OnPlayerSanityStateChanged -= OnSanityStateChanged;
                SanitySystem.Instance.OnPlayerHallucination -= OnHallucination;
            }
            
            // Clean up hallucinations
            ClearHallucinations();
        }
        
        private void Update()
        {
            // Apply continuous distortion effects
            if (_distortionFactor > 0)
            {
                ApplyRoomShake();
                ApplyConnectionDistortion();
            }
        }
        
        // Handle sanity changed event
        private void OnSanityChanged(int playerId, float currentSanity, float maxSanity)
        {
            // Only update for local player
            int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
            if (playerId != localPlayerId)
                return;
                
            UpdateDistortion(currentSanity / maxSanity, _currentSanityState);
        }
        
        // Handle sanity state changed event
        private void OnSanityStateChanged(int playerId, SanityState oldState, SanityState newState)
        {
            // Only update for local player
            int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
            if (playerId != localPlayerId)
                return;
                
            _currentSanityState = newState;
            
            // Get current sanity percentage
            float sanityPercentage = SanitySystem.Instance.GetPlayerSanityPercentage(localPlayerId);
            UpdateDistortion(sanityPercentage, newState);
            
            // Apply material change if state changed significantly
            if (oldState < SanityState.Disturbed && newState >= SanityState.Disturbed)
            {
                ApplyDistortedMaterial();
            }
            else if (oldState >= SanityState.Disturbed && newState < SanityState.Disturbed)
            {
                ApplyNormalMaterial();
            }
        }
        
        // Handle hallucination event
        private void OnHallucination(int playerId, SanityHallucination hallucinationType)
        {
            // Only update for local player
            int localPlayerId = GameManager.Instance?.LocalPlayer?.NetworkId ?? 0;
            if (playerId != localPlayerId)
                return;
                
            // Add map-related hallucinations
            if (hallucinationType == SanityHallucination.EnvironmentChange)
            {
                AddHallucinatedRoom();
            }
        }
        
        // Update distortion based on sanity
        private void UpdateDistortion(float sanityPercentage, SanityState state)
        {
            // Calculate distortion factor (0-1)
            if (sanityPercentage > minDistortionSanity)
            {
                _distortionFactor = 0f;
            }
            else
            {
                // Map [0, minDistortionSanity] to [1, 0]
                _distortionFactor = 1f - (sanityPercentage / minDistortionSanity);
            }
            
            // Apply color changes based on state
            if (state >= SanityState.Disturbed)
            {
                ApplyRoomColorDistortion(state);
            }
            else
            {
                ResetRoomColors();
            }
            
            // Check for random hallucinated rooms
            if (state >= SanityState.Unstable && Random.value < _distortionFactor * 0.1f)
            {
                AddHallucinatedRoom();
            }
        }
        
        // Apply room shaking effect
        private void ApplyRoomShake()
        {
            if (_miniMapController == null) return;
            
            // Get all room icons
            var roomIcons = _miniMapController.GetRoomIcons();
            if (roomIcons == null || roomIcons.Count == 0) return;
            
            // Apply random offsets based on distortion factor
            foreach (var icon in roomIcons.Values)
            {
                if (icon == null) continue;
                
                // Check if this room should be shaking
                Room room = icon.GetComponent<RoomIconData>()?.Room;
                if (room == null) continue;
                
                // Corrupted rooms shake more
                float roomFactor = room.IsCorrupted ? _distortionFactor * 1.5f : _distortionFactor;
                
                // Unexplored rooms shake less
                if (!room.IsVisited)
                    roomFactor *= 0.5f;
                
                // Apply random shake
                if (roomFactor > 0)
                {
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f),
                        0
                    ) * maxRoomShakeIntensity * roomFactor;
                    
                    // Apply with lerp for smoother movement
                    icon.transform.localPosition = Vector3.Lerp(
                        icon.transform.localPosition,
                        icon.transform.localPosition + randomOffset,
                        Time.deltaTime * 5f);
                }
            }
        }
        
        // Apply distortion to room connections
        private void ApplyConnectionDistortion()
        {
            if (_miniMapController == null) return;
            
            // Get all connections
            var connections = _miniMapController.GetRoomConnections();
            if (connections == null || connections.Count == 0) return;
            
            // Apply distortion to each connection
            foreach (var connection in connections)
            {
                if (connection == null) continue;
                
                // Only distort if we have enough control points
                LineRenderer line = connection.GetComponent<LineRenderer>();
                if (line == null || line.positionCount < 2) continue;
                
                // Get original positions
                Vector3 start = line.GetPosition(0);
                Vector3 end = line.GetPosition(line.positionCount - 1);
                
                // Apply distortion
                if (_distortionFactor > 0 && line.positionCount == 2)
                {
                    // Upgrade to more control points for distortion
                    line.positionCount = 4;
                    
                    // Store in distorted list if not already there
                    if (!_distortedConnections.Contains(line))
                    {
                        _distortedConnections.Add(line);
                    }
                }
                
                if (line.positionCount >= 4)
                {
                    // Calculate control points
                    Vector3 direction = end - start;
                    Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0).normalized;
                    
                    // Apply noise to control points
                    float noise1 = Mathf.PerlinNoise(Time.time * 0.5f, start.x * 0.1f);
                    float noise2 = Mathf.PerlinNoise(end.x * 0.1f, Time.time * 0.5f);
                    
                    Vector3 control1 = Vector3.Lerp(start, end, 0.33f) + 
                                    perpendicular * (noise1 * 2f - 1f) * maxConnectionDistortion * _distortionFactor;
                    
                    Vector3 control2 = Vector3.Lerp(start, end, 0.66f) + 
                                    perpendicular * (noise2 * 2f - 1f) * maxConnectionDistortion * _distortionFactor;
                    
                    // Set positions
                    line.SetPosition(0, start);
                    line.SetPosition(1, control1);
                    line.SetPosition(2, control2);
                    line.SetPosition(3, end);
                }
            }
        }
        
        // Apply color distortion to rooms
        private void ApplyRoomColorDistortion(SanityState state)
        {
            if (_miniMapController == null) return;
            
            // Get all room icons
            var roomIcons = _miniMapController.GetRoomIcons();
            if (roomIcons == null || roomIcons.Count == 0) return;
            
            // Apply color changes based on state
            foreach (var icon in roomIcons.Values)
            {
                if (icon == null) continue;
                
                Image image = icon.GetComponent<Image>();
                if (image == null) continue;
                
                // Get room data
                Room room = icon.GetComponent<RoomIconData>()?.Room;
                if (room == null) continue;
                
                // Corrupted rooms get special colors
                if (room.IsCorrupted && corruptedRoomColors != null && corruptedRoomColors.Length > 0)
                {
                    // Pick a color based on sanity state
                    int colorIndex = Mathf.Min((int)state - (int)SanityState.Disturbed, corruptedRoomColors.Length - 1);
                    if (colorIndex >= 0)
                    {
                        image.color = corruptedRoomColors[colorIndex];
                    }
                }
                // For non-corrupted rooms, add slight color shifts at very low sanity
                else if (state >= SanityState.Unstable)
                {
                    // Slightly alter color based on distortion factor
                    float hueShift = Mathf.Sin(Time.time * 0.5f + icon.transform.position.x * 0.1f) * 0.1f * _distortionFactor;
                    
                    Color.RGBToHSV(image.color, out float h, out float s, out float v);
                    h = (h + hueShift) % 1f;
                    image.color = Color.HSVToRGB(h, s, v);
                }
            }
        }
        
        // Reset room colors to normal
        private void ResetRoomColors()
        {
            if (_miniMapController == null) return;
            
            // Delegate to minimap controller to reset colors
            _miniMapController.ResetRoomColors();
        }
        
        // Apply distorted material to the minimap
        private void ApplyDistortedMaterial()
        {
            if (_miniMapController == null || distortedMapMaterial == null) return;
            
            _miniMapController.SetMaterial(distortedMapMaterial);
        }
        
        // Apply normal material to the minimap
        private void ApplyNormalMaterial()
        {
            if (_miniMapController == null || normalMapMaterial == null) return;
            
            _miniMapController.SetMaterial(normalMapMaterial);
            
            // Reset connection distortions
            ResetConnectionDistortions();
        }
        
        // Reset connection distortions
        private void ResetConnectionDistortions()
        {
            foreach (var connection in _distortedConnections)
            {
                if (connection == null) continue;
                
                // Get original endpoints
                if (connection.positionCount >= 2)
                {
                    Vector3 start = connection.GetPosition(0);
                    Vector3 end = connection.GetPosition(connection.positionCount - 1);
                    
                    // Reset to straight line
                    connection.positionCount = 2;
                    connection.SetPosition(0, start);
                    connection.SetPosition(1, end);
                }
            }
            
            _distortedConnections.Clear();
        }
        
        // Add a hallucinated room to the minimap
        private void AddHallucinatedRoom()
        {
            if (_miniMapController == null || 
                hallucinatedRoomIcons == null || 
                hallucinatedRoomIcons.Length == 0) return;
            
            // Randomly decide whether to add a false room or connection
            bool addRoom = Random.value < hallucination_RoomChance;
            bool addConnection = Random.value < hallucination_ConnectionChance;
            
            if (addRoom)
            {
                // Create a hallucinated room
                Vector2 randomPos = new Vector2(
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f)
                ) * 10f;
                
                // Create room icon
                GameObject roomIcon = _miniMapController.CreateRoomIcon();
                if (roomIcon == null) return;
                
                // Position it
                roomIcon.transform.localPosition = randomPos;
                
                // Set a random hallucinated icon
                Image image = roomIcon.GetComponent<Image>();
                if (image != null)
                {
                    int iconIndex = Random.Range(0, hallucinatedRoomIcons.Length);
                    image.sprite = hallucinatedRoomIcons[iconIndex];
                    
                    // Make it somewhat transparent
                    Color color = image.color;
                    color.a = 0.7f;
                    image.color = color;
                }
                
                // Add to tracking list
                _hallucinatedRooms.Add(roomIcon);
                
                // Schedule removal
                StartCoroutine(RemoveHallucinatedRoomAfterDelay(roomIcon, hallucinatedRoomDuration));
            }
            
            if (addConnection)
            {
                // Create a hallucinated connection between random rooms
                var roomIcons = _miniMapController.GetRoomIcons();
                if (roomIcons == null || roomIcons.Count < 2) return;
                
                // Get two random room icons
                int roomCount = roomIcons.Count;
                int room1Index = Random.Range(0, roomCount);
                int room2Index = (room1Index + Random.Range(1, roomCount)) % roomCount;
                
                GameObject[] icons = new GameObject[roomIcons.Count];
                roomIcons.Values.CopyTo(icons, 0);
                
                GameObject room1Icon = icons[room1Index];
                GameObject room2Icon = icons[room2Index];
                
                if (room1Icon == null || room2Icon == null) return;
                
                // Create a connection line
                GameObject lineObj = _miniMapController.CreateConnectionLine();
                if (lineObj == null) return;
                
                LineRenderer line = lineObj.GetComponent<LineRenderer>();
                if (line == null) return;
                
                // Set up the line
                line.positionCount = 4; // Curved line
                
                // Get positions
                Vector3 start = room1Icon.transform.position;
                Vector3 end = room2Icon.transform.position;
                
                // Calculate control points for a curve
                Vector3 direction = end - start;
                Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0).normalized;
                
                float control1Offset = Random.Range(-5f, 5f);
                float control2Offset = Random.Range(-5f, 5f);
                
                Vector3 control1 = Vector3.Lerp(start, end, 0.33f) + perpendicular * control1Offset;
                Vector3 control2 = Vector3.Lerp(start, end, 0.66f) + perpendicular * control2Offset;
                
                // Set positions
                line.SetPosition(0, start);
                line.SetPosition(1, control1);
                line.SetPosition(2, control2);
                line.SetPosition(3, end);
                
                // Set color to something eerie
                line.startColor = new Color(0.5f, 0, 0.5f, 0.7f);
                line.endColor = new Color(0.5f, 0, 0.5f, 0.7f);
                
                // Add to tracking list
                _distortedConnections.Add(line);
                
                // Schedule removal
                StartCoroutine(RemoveHallucinatedConnectionAfterDelay(lineObj, hallucinatedRoomDuration));
            }
        }
        
        // Remove hallucinated room after delay
        private IEnumerator RemoveHallucinatedRoomAfterDelay(GameObject room, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Fade out
            Image image = room.GetComponent<Image>();
            if (image != null)
            {
                float fadeDuration = 1f;
                float time = 0f;
                Color startColor = image.color;
                
                while (time < fadeDuration)
                {
                    time += Time.deltaTime;
                    Color color = startColor;
                    color.a = Mathf.Lerp(startColor.a, 0f, time / fadeDuration);
                    image.color = color;
                    yield return null;
                }
            }
            
            // Remove from tracking list
            _hallucinatedRooms.Remove(room);
            
            // Destroy the room icon
            Destroy(room);
        }
        
        // Remove hallucinated connection after delay
        private IEnumerator RemoveHallucinatedConnectionAfterDelay(GameObject connection, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Fade out
            LineRenderer line = connection.GetComponent<LineRenderer>();
            if (line != null)
            {
                float fadeDuration = 1f;
                float time = 0f;
                Color startColor = line.startColor;
                
                while (time < fadeDuration)
                {
                    time += Time.deltaTime;
                    Color color = startColor;
                    color.a = Mathf.Lerp(startColor.a, 0f, time / fadeDuration);
                    line.startColor = color;
                    line.endColor = color;
                    yield return null;
                }
            }
            
            // Remove from tracking list
            if (line != null)
            {
                _distortedConnections.Remove(line);
            }
            
            // Destroy the connection
            Destroy(connection);
        }
        
        // Clear all hallucinations
        private void ClearHallucinations()
        {
            // Clear rooms
            foreach (var room in _hallucinatedRooms)
            {
                if (room != null)
                {
                    Destroy(room);
                }
            }
            _hallucinatedRooms.Clear();
            
            // Reset connections
            ResetConnectionDistortions();
        }
    }
}