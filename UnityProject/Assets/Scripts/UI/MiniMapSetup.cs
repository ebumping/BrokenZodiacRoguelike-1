using UnityEngine;
using UnityEngine.UI;
using CodexOfTheBrokenZodiac.Core;

namespace CodexOfTheBrokenZodiac.UI
{
    // This class helps set up the mini-map prefab
    [ExecuteInEditMode]
    public class MiniMapSetup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MiniMapController miniMapController;
        [SerializeField] private RawImage miniMapImage;
        [SerializeField] private RectTransform miniMapContainer;
        [SerializeField] private NoiseTextureGenerator noiseGenerator;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject roomIconPrefab;
        [SerializeField] private GameObject playerIconPrefab;
        [SerializeField] private GameObject bossIconPrefab;
        [SerializeField] private GameObject itemIconPrefab;
        [SerializeField] private GameObject doorIconPrefab;
        
        [Header("Materials")]
        [SerializeField] private Material distortionMaterial;
        [SerializeField] private Material blurMaterial;
        
        [Header("UI Elements")]
        [SerializeField] private Button expandButton;
        [SerializeField] private Button collapseButton;
        [SerializeField] private Button zoomInButton;
        [SerializeField] private Button zoomOutButton;
        
        [Header("Actions")]
        [SerializeField] private bool setUpInEditor = false;
        
        private void OnValidate()
        {
            if (setUpInEditor && Application.isEditor && !Application.isPlaying)
            {
                setUpInEditor = false;
                SetUpMiniMap();
            }
        }
        
        private void Start()
        {
            // Set up the UI elements
            if (expandButton != null)
            {
                expandButton.onClick.AddListener(ExpandMap);
            }
            
            if (collapseButton != null)
            {
                collapseButton.onClick.AddListener(CollapseMap);
            }
            
            if (zoomInButton != null)
            {
                zoomInButton.onClick.AddListener(ZoomIn);
            }
            
            if (zoomOutButton != null)
            {
                zoomOutButton.onClick.AddListener(ZoomOut);
            }
            
            // Set up the mini-map if in play mode
            if (Application.isPlaying)
            {
                SetUpMiniMap();
            }
        }
        
        // Set up the mini-map components
        public void SetUpMiniMap()
        {
            if (miniMapController == null)
            {
                miniMapController = GetComponent<MiniMapController>();
                if (miniMapController == null)
                {
                    miniMapController = gameObject.AddComponent<MiniMapController>();
                }
            }
            
            // Set references in the controller
            if (miniMapController != null)
            {
                // Use reflection to set private serialized fields
                SetPrivateField(miniMapController, "miniMapRenderTexture", miniMapImage);
                SetPrivateField(miniMapController, "miniMapContainer", miniMapContainer);
                SetPrivateField(miniMapController, "roomIconPrefab", roomIconPrefab);
                SetPrivateField(miniMapController, "playerIconPrefab", playerIconPrefab);
                SetPrivateField(miniMapController, "bossIconPrefab", bossIconPrefab);
                SetPrivateField(miniMapController, "itemIconPrefab", itemIconPrefab);
                SetPrivateField(miniMapController, "doorIconPrefab", doorIconPrefab);
            }
            
            // Set up noise texture generator
            if (noiseGenerator == null)
            {
                noiseGenerator = gameObject.GetComponent<NoiseTextureGenerator>();
                if (noiseGenerator == null)
                {
                    noiseGenerator = gameObject.AddComponent<NoiseTextureGenerator>();
                }
            }
            
            // Set up materials if not already initialized
            if (distortionMaterial == null)
            {
                distortionMaterial = new Material(Shader.Find("Custom/CosmicDistortion"));
            }
            
            if (blurMaterial == null)
            {
                blurMaterial = new Material(Shader.Find("Custom/CosmicBlur"));
            }
            
            // Assign the noise texture to materials
            if (noiseGenerator != null)
            {
                noiseGenerator.AssignToMaterial(distortionMaterial, "_NoiseTex");
                noiseGenerator.AssignToMaterial(blurMaterial, "_NoiseTex");
            }
            
            // Set materials in the controller
            SetPrivateField(miniMapController, "_distortionMaterial", distortionMaterial);
            SetPrivateField(miniMapController, "_blurMaterial", blurMaterial);
            
            Debug.Log("Mini-map setup completed");
        }
        
        // Helper to set private serialized fields using reflection
        private void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;
            
            var field = target.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"Field '{fieldName}' not found in {target.GetType().Name}");
            }
        }
        
        // UI Button callbacks
        private void ExpandMap()
        {
            if (miniMapController != null && !IsMapExpanded())
            {
                miniMapController.ToggleExpandedMap();
            }
        }
        
        private void CollapseMap()
        {
            if (miniMapController != null && IsMapExpanded())
            {
                miniMapController.ToggleExpandedMap();
            }
        }
        
        private void ZoomIn()
        {
            // This would be handled by the controller's input system
            // Here just for UI button support if needed
        }
        
        private void ZoomOut()
        {
            // This would be handled by the controller's input system
            // Here just for UI button support if needed
        }
        
        // Check if map is expanded using reflection
        private bool IsMapExpanded()
        {
            if (miniMapController == null) return false;
            
            var field = miniMapController.GetType().GetField("_isExpanded", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (field != null)
            {
                return (bool)field.GetValue(miniMapController);
            }
            
            return false;
        }
    }
}