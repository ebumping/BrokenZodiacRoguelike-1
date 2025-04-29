# Codex of the Broken Zodiac - Unity Version

## Overview
This is a Unity adaptation of the "Codex of the Broken Zodiac" game, originally built with Godot. It preserves the core gameplay mechanics, procedural generation, and the Lovecraftian card-based progression system from the original.

## Game Description
An isometric twin-stick roguelike shooter with procedural generation, Lovecraftian themes, and card-based progression built with Unity.

## Main Features
- **Isometric Twin-stick Combat**: Fast-paced combat with fluid movement and aiming
- **Procedural Generation**: Dynamically generated levels with different room types
- **Lovecraftian Themes**: Dark atmosphere with eldritch horrors and mysteries
- **Card-based Progression**: Tarot cards that modify player abilities and stats
- **Multiple Character Classes**: Each with unique abilities and playstyles
- **Multiplayer Support**: Cooperative gameplay option

## Project Structure

### Core Systems
- `GameManager.cs`: Central manager for game state and run statistics
- `ProcGenManager.cs`: Handles procedural level generation
- `PlayerController.cs`: Player input, movement, combat, and abilities
- `EnemyController.cs`: AI behavior and combat for enemies
- `NetworkManager.cs`: Multiplayer connectivity and synchronization
- `ResourceManager.cs`: Asset loading and caching
- `GameDirector.cs`: Overall game flow controller

### Resource Classes
- `TarotCard.cs`: Tarot card system with various effects
- `Weapon.cs`: Weapon properties and behavior
- `PlayerClass.cs`: Character class definitions
- `ZodiacSignil.cs`: Zodiac-themed passive effects
- `Room.cs`: Room data structures for procedural generation

### UI Components
- `MainMenuController.cs`: Main menu interface
- `HUDController.cs`: In-game UI and information display
- `CardSelectorController.cs`: Card selection interface

## Required Unity Packages
- TextMeshPro: For UI text rendering
- Cinemachine: For camera control (recommended)
- Unity Input System: For better input handling (optional)
- Addressables: For resource management (optional)

## Setup Instructions
1. Create a new Unity project (2021.3 LTS or newer recommended)
2. Import the necessary packages
3. Copy the Scripts folder structure into your project
4. Create the basic scenes:
   - MainMenu
   - GameScene
5. Set up the core prefabs:
   - Player
   - Room templates
   - UI components
6. Configure the GameManager, NetworkManager, ProcGenManager, and ResourceManager as singletons

## Conversion Notes
This Unity version maintains gameplay parity with the Godot original while adapting to Unity's architecture and component system. Some implementation details were adjusted to better fit Unity's workflows:

- **Signal System**: Godot's signals were replaced with C# events
- **Scene Structure**: Unity's prefab system is used instead of Godot's scene system
- **Node Hierarchy**: Adapted to Unity's GameObject and Component pattern
- **Resource Loading**: Uses Unity's Resources and ScriptableObject system

## Next Steps
- Implement visual assets and animations
- Set up shader effects and particle systems
- Flesh out the room templates and enemy variations
- Implement save/load system
- Add sound effects and music

## Credits
Original Godot implementation created for the "Codex of the Broken Zodiac" project.
Unity adaptation preserves core gameplay and systems while following Unity best practices.