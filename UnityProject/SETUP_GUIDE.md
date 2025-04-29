# Unity Project Setup Guide for Codex of the Broken Zodiac

## Prerequisites
- Unity 2021.3 LTS or newer
- Basic understanding of Unity's interface and workflows

## Initial Project Setup

1. **Create New Unity Project**
   - Open Unity Hub
   - Click "New Project"
   - Select the "3D" template (or "3D with Extras" if available)
   - Name the project "Codex of the Broken Zodiac"
   - Choose a location and click "Create Project"

2. **Organize Folder Structure**
   The scripts in this repository are organized as follows, and you should maintain this structure in your Unity project:
   ```
   Assets/
     Scripts/
       Core/           # Core game systems
       Resources/      # Game resource definitions
       UI/             # UI-related scripts
       AI/             # Enemy and NPC AI
     Prefabs/          # Game object prefabs
     Scenes/           # Unity scenes
     Sprites/          # 2D graphics
     Materials/        # Materials and shaders
     Animations/       # Animation clips and controllers
     Audio/            # Sound effects and music
     Resources/        # Assets loaded at runtime
   ```

3. **Install Required Packages**
   - Open Window > Package Manager
   - Install the following packages:
     - TextMeshPro (Essential for UI)
     - Cinemachine (Camera control)
     - 2D Sprite (for sprite handling)
     - 2D Tilemap Editor (for level design, if needed)

## Core Systems Setup

1. **Create Manager Scene**
   - Create a new scene called "ManagerScene"
   - This will hold the persistent game managers
   - Add an empty GameObject called "Managers"
   - Add the following as children:
     - GameManager
     - NetworkManager
     - ResourceManager
     - ProcGenManager
   - Add the corresponding scripts to each object
   - Set this scene to load first in Build Settings

2. **Create Main Menu Scene**
   - Create a new scene called "MainMenu"
   - Create a Canvas object for the UI
   - Add UI elements according to the MainMenuController script
   - Add a GameDirector object and attach the GameDirector script

3. **Create Game Scene**
   - Create a new scene called "GameScene"
   - Add a WorldContainer empty GameObject
   - Add a Canvas for the HUD and implement according to HUDController
   - Add UI elements for pause menu, game over, etc.

## Player and Enemy Setup

1. **Create Player Prefab**
   - Create a new empty GameObject named "Player"
   - Add a Sprite Renderer component
   - Add a Rigidbody2D (set to Kinematic)
   - Add a Collider2D (CapsuleCollider2D recommended)
   - Add the PlayerController script
   - Add a child object for the weapon pivot point
   - Configure references in the Inspector
   - Save as a prefab in the Prefabs folder

2. **Create Enemy Prefabs**
   - Create a base enemy GameObject
   - Add Sprite Renderer, Rigidbody2D, and Collider2D
   - Add the EnemyController script
   - Configure base properties
   - Create variants (basic enemy, ranged enemy, boss, etc.)
   - Save as prefabs in the Prefabs folder

## Room System Setup

1. **Create Room Templates**
   - Create prefabs for different room types:
     - Starting room
     - Standard rooms (various sizes)
     - Special rooms (shrine, tarot, treasure)
     - Boss room
   - Each room should have:
     - Visual elements (floor, walls, decorations)
     - Spawn points for enemies
     - Spawn points for loot
     - Door locations
     - Collision boundaries

2. **Set Up Room Controller**
   - Add the RoomController script to each room template
   - Configure references to spawn points

## Resource Creation

1. **Create Scriptable Objects**
   - Right-click in the Project window > Create > Codex > PlayerClass
   - Create class definitions for each character class
   - Similarly create Weapons, TarotCards, and ZodiacSignils

2. **Set Up Resource Manager**
   - Drag the created scriptable objects into the ResourceManager references in the Inspector

## Testing

1. **Test Core Systems**
   - Enter Play mode in the MainMenu scene
   - Check that the main menu loads and buttons work
   - Test starting a new game
   - Verify that procedural generation works

2. **Test Gameplay**
   - Verify player movement and combat
   - Test enemy AI
   - Check room transitions
   - Test card selection and effects

## Build and Deployment

1. **Configure Build Settings**
   - Add all scenes to the build
   - Set proper scene order (ManagerScene first)
   - Configure player settings

2. **Build the Project**
   - Choose target platform
   - Build and run

## Common Issues and Solutions

- **Scene reference problems**: Make sure all prefabs and scenes are properly connected in the inspector
- **Null reference exceptions**: Check inspector references and initialization order
- **Movement feels wrong**: Adjust physics settings and movement parameters
- **Room generation issues**: Debug the ProcGenManager and verify room connections

## Script Adaptation Notes

The provided scripts are set up for Unity's component system, but you might need to make adjustments:

1. **MonoBehaviour Lifecycle**: Scripts use Unity's lifecycle methods (Awake, Start, Update)
2. **Prefab References**: Make sure to set up proper references in the Inspector
3. **Events**: Scripts use C# events rather than Godot signals
4. **Resource Loading**: Uses Unity's ScriptableObject system

Follow this guide to create a functioning Unity version of the game that maintains the original's design and gameplay elements.