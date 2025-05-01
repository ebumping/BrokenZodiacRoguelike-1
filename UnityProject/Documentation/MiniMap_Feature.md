# Interactive Mini-Map with Cosmic Distortion Animations

## Overview

The mini-map system provides a dynamic, interactive map that displays the procedurally generated level layout with cosmic horror-themed distortion effects. This system is designed to enhance the Lovecraftian atmosphere of the game while providing practical navigation functionality.

## Features

- Real-time tracking of player positions
- Room visibility based on exploration status
- Interactive expansion and zoom controls
- Cosmic distortion effects that respond to gameplay events
- Support for multi-player with unique player icons
- Dynamic integration with procedural generation
- Animated room transitions and discoveries

## Components

### Core Classes

1. **MiniMapController**: The primary controller that manages the mini-map display, room icons, and cosmic distortion effects.
   - Handles user input for zooming and panning
   - Manages cosmic distortion animations
   - Tracks and displays room states

2. **NoiseTextureGenerator**: Generates procedural noise textures used by the cosmic distortion shader.
   - Supports multiple octaves of Perlin noise
   - Can create variations for different visual effects
   - Customizable parameters for noise characteristics

3. **MiniMapSetup**: Helper class for setting up the mini-map prefab in Unity.
   - Connects UI elements to the controller
   - Initializes material references
   - Sets up proper component relationships

### Shaders

1. **CosmicDistortion.shader**: The primary shader for creating eldritch distortion effects.
   - Tentacle-like distortions that respond to player proximity to cosmic entities
   - Void patches that appear and disappear
   - Subtle color aberration effects
   - Pulsing intensity based on game state

2. **CosmicBlur.shader**: A complementary shader that creates blurring effects around the edges.
   - Radial blur that intensifies during certain events
   - Edge glow effects for a more ethereal appearance
   - Chromatic aberration for otherworldly feel

## Integration

The mini-map integrates with several other game systems:

- **ProcGenManager**: Provides room layout data and notifies the mini-map of room discoveries
- **GameManager**: Provides player positions and game state information
- **PlayerController**: Players are tracked and displayed on the mini-map
- **Room**: Room data including type, visibility, connections, and corruption state

## Usage

### Basic Setup

1. Add the MiniMap prefab to your UI canvas
2. Ensure the required layers exist in your project ("UI" and "MiniMap")
3. Reference the MiniMapController in the GameManager
4. Make sure ProcGenManager events are properly connected to the MiniMapController

### Customizing Distortion Effects

The cosmic distortion effects can be customized through the NoiseTextureGenerator component:

```csharp
// Get the generator
NoiseTextureGenerator noiseGen = miniMapObject.GetComponent<NoiseTextureGenerator>();

// Create a new noise variant
Texture2D customNoise = noiseGen.GenerateVariant(seed: 42, scale: 15f);

// Apply to materials
noiseGen.AssignToMaterial(distortionMaterial, "_NoiseTex");
```

Intensity of the distortion effects can be controlled through the MiniMapController:

```csharp
// Increase distortion during boss fights
MiniMapController miniMap = FindObjectOfType<MiniMapController>();
ReflectionType.SetValue(miniMap, "_distortionIntensity", 0.2f);
```

### Responding to Game Events

The mini-map can respond to various game events to enhance the atmosphere:

1. **Boss Encounters**: Increase distortion during boss fights
2. **Corrupted Rooms**: Apply stronger effects in corrupted areas
3. **Low Health**: Intensify distortion when player health is low
4. **Ritual Events**: Special effects during key story moments

Example: Implementing a boss encounter effect

```csharp
// In your boss encounter script
private void OnBossEncounterStart()
{
    // Find MiniMapController
    MiniMapController miniMap = FindObjectOfType<MiniMapController>();
    
    // Get the _distortionMaterial field using reflection
    var matField = miniMap.GetType().GetField("_distortionMaterial", 
        System.Reflection.BindingFlags.NonPublic | 
        System.Reflection.BindingFlags.Instance);
    
    Material mat = (Material)matField.GetValue(miniMap);
    
    // Increase cosmic influence
    mat.SetFloat("_CosmicInfluence", 0.8f);
    mat.SetFloat("_TentacleAmount", 8f);
    mat.SetColor("_VoidColor", new Color(0.8f, 0, 0.2f));
    
    // Animate the effect
    StartCoroutine(PulsateDistortion(mat));
}

private IEnumerator PulsateDistortion(Material mat)
{
    float time = 0;
    float duration = 30f; // Boss encounter duration
    
    while (time < duration)
    {
        // Pulsate the effect
        float pulse = Mathf.Sin(time * 2f) * 0.2f + 0.8f;
        mat.SetFloat("_Intensity", pulse * 0.2f);
        
        time += Time.deltaTime;
        yield return null;
    }
    
    // Reset
    mat.SetFloat("_CosmicInfluence", 0.1f);
    mat.SetFloat("_TentacleAmount", 3f);
    mat.SetFloat("_Intensity", 0.1f);
}
```

## Customizing Mini-Map Appearance

The mini-map's appearance can be customized through the MiniMapController inspector:

- **Room Colors**: Customize the colors for different room types and states
- **Icon Prefabs**: Change the look of room icons, player markers, etc.
- **Container Layout**: Adjust the position and size of the mini-map on screen
- **Distortion Settings**: Fine-tune the cosmic distortion effects

## Performance Considerations

1. The cosmic distortion shaders can be performance-intensive on mobile devices. Consider:
   - Reducing distortion intensity on lower-end devices
   - Using fewer octaves in the NoiseTextureGenerator
   - Reducing the resolution of the render textures

2. For large levels, consider:
   - Implementing level-of-detail for the mini-map
   - Only showing rooms within a certain range of the player
   - Using simpler icons for distant rooms

## Known Limitations

1. The mini-map currently supports 2D levels with discrete room layouts only
2. Performance may degrade with extremely large levels (100+ rooms)
3. Custom shaders require Unity Shader Graph or custom HLSL knowledge to modify

## Future Enhancements

1. Support for multi-level dungeons with floor transitions
2. Objective markers and quest indicators
3. Enhanced cosmic events with unique visual distortions
4. Minimap fog-of-war effects for unexplored areas
5. Snapshot system to capture and compare room layouts