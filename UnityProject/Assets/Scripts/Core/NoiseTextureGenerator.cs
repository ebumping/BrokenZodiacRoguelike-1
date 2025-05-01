using UnityEngine;

namespace CodexOfTheBrokenZodiac.Core
{
    // This class generates a procedural noise texture for use with the cosmic distortion shader
    public class NoiseTextureGenerator : MonoBehaviour
    {
        // Noise texture settings
        [Header("Texture Settings")]
        [SerializeField] private int textureSize = 512;
        [SerializeField] private TextureFormat textureFormat = TextureFormat.RGBA32;
        [SerializeField] private bool useMipMaps = false;
        [SerializeField] private FilterMode filterMode = FilterMode.Bilinear;
        
        // Noise settings
        [Header("Noise Settings")]
        [SerializeField] private int seed = 42;
        [SerializeField] private float scale = 10f;
        [SerializeField] private int octaves = 5;
        [SerializeField] private float persistence = 0.5f;
        [SerializeField] private float lacunarity = 2f;
        [SerializeField] private Vector2 offset = Vector2.zero;
        
        // Output reference
        [Header("Output")]
        [SerializeField] private bool autoGenerate = true;
        [SerializeField] private bool regenerateInEditor = false;
        [SerializeField] private Material targetMaterial;
        [SerializeField] private string noiseTextureName = "_NoiseTex";
        
        // Generated texture
        private Texture2D _noiseTexture;
        
        private void Awake()
        {
            if (autoGenerate)
            {
                GenerateNoiseTexture();
            }
        }
        
        private void OnValidate()
        {
            if (regenerateInEditor && Application.isEditor && !Application.isPlaying)
            {
                GenerateNoiseTexture();
            }
        }
        
        // Generate a new noise texture
        public Texture2D GenerateNoiseTexture()
        {
            // Create new texture
            _noiseTexture = new Texture2D(textureSize, textureSize, textureFormat, useMipMaps);
            _noiseTexture.filterMode = filterMode;
            
            // Set pixel values using Perlin noise
            Color[] colorMap = new Color[textureSize * textureSize];
            
            // Initialize random number generator with seed
            System.Random prng = new System.Random(seed);
            
            // Generate octave offsets
            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }
            
            // Calculate noise values
            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;
            
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;
                    
                    // Calculate noise value for each octave
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x / (float)textureSize * scale * frequency) + octaveOffsets[i].x;
                        float sampleY = (y / (float)textureSize * scale * frequency) + octaveOffsets[i].y;
                        
                        // Use coherent noise function (Perlin or Simplex)
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                        noiseHeight += perlinValue * amplitude;
                        
                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }
                    
                    // Keep track of min/max values for normalization
                    if (noiseHeight > maxNoiseHeight)
                        maxNoiseHeight = noiseHeight;
                    if (noiseHeight < minNoiseHeight)
                        minNoiseHeight = noiseHeight;
                    
                    // Store the noise value
                    colorMap[y * textureSize + x] = new Color(noiseHeight, noiseHeight, noiseHeight, 1f);
                }
            }
            
            // Normalize the noise values
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    int index = y * textureSize + x;
                    
                    // Normalize from 0 to 1
                    colorMap[index].r = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, colorMap[index].r);
                    colorMap[index].g = colorMap[index].r;
                    colorMap[index].b = colorMap[index].r;
                }
            }
            
            // Apply the color map to the texture
            _noiseTexture.SetPixels(colorMap);
            _noiseTexture.Apply();
            
            // Assign to the target material if specified
            if (targetMaterial != null)
            {
                targetMaterial.SetTexture(noiseTextureName, _noiseTexture);
            }
            
            return _noiseTexture;
        }
        
        // Generate a variant of the noise texture with different parameters
        public Texture2D GenerateVariant(int newSeed, float newScale)
        {
            int originalSeed = seed;
            float originalScale = scale;
            
            // Temporarily change parameters
            seed = newSeed;
            scale = newScale;
            
            // Generate with new parameters
            Texture2D variant = GenerateNoiseTexture();
            
            // Restore original parameters
            seed = originalSeed;
            scale = originalScale;
            
            return variant;
        }
        
        // Get the current noise texture
        public Texture2D GetNoiseTexture()
        {
            if (_noiseTexture == null && autoGenerate)
            {
                GenerateNoiseTexture();
            }
            
            return _noiseTexture;
        }
        
        // Assign the noise texture to a material
        public void AssignToMaterial(Material material, string propertyName = "_NoiseTex")
        {
            if (_noiseTexture == null && autoGenerate)
            {
                GenerateNoiseTexture();
            }
            
            if (material != null && _noiseTexture != null)
            {
                material.SetTexture(propertyName, _noiseTexture);
            }
        }
    }
}