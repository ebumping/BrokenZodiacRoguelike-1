# Godot to Unity Conversion Guide

## Key Differences Between Implementations

This document outlines the major differences between the Godot and Unity implementations of "Codex of the Broken Zodiac" to help understand the conversion approach.

### Architecture & Structure

| Godot | Unity | Notes |
|-------|-------|-------|
| Node hierarchy | GameObject/Component system | Unity uses composition over inheritance |
| Scene system | Prefab system | Similar concepts, different workflows |
| `.tscn` files | Unity scenes (`.unity`) | Unity scenes work with prefabs |
| Signals | C# Events | Equivalent functionality, different syntax |
| GDScript/C# | C# only | Unity scripts are C# only |
| Resources | ScriptableObjects | Unity's data container equivalent |
| `.gd` scripts | `.cs` MonoBehaviour scripts | Different lifecycle methods |
| Autoloads | Singletons with DontDestroyOnLoad | Manager pattern in Unity |

### Physics & Input

| Godot | Unity | Notes |
|-------|-------|-------|
| `CharacterBody2D` | Rigidbody2D | Unity typically uses Rigidbody2D with kinematic option |
| Input actions | Input system | Similar with different implementation |
| `move_and_slide()` | `rb.velocity` | Different approaches to movement |
| `_physics_process()` | `FixedUpdate()` | Equivalent for physics calculations |
| Built-in collision layers | Layer-based collision matrix | Similar concept, different setup |

### UI System

| Godot | Unity | Notes |
|-------|-------|-------|
| Control nodes | Unity UI (Canvas, etc.) | Different coordinate systems |
| Theme resources | UI styles | Unity uses more individual settings |
| AnimationPlayer | Animation system | Similar concepts, different implementations |
| SignalBus | Events/UnityEvents | Communication between UI and logic |
| Control node tree | Canvas/RectTransform hierarchy | Different nesting logic |
| `connect()` for signals | AddListener for events | Different event registration |

### Networking

| Godot | Unity | Notes |
|-------|-------|-------|
| MultiplayerAPI | Unity Networking solution | Different approaches to replication |
| RPC calls | [Command]/[ClientRpc] | Different attribute system |
| NetworkedMultiplayerENet | NetworkManager | Different connection handling |
| Godot server/client | Unity host/client | Similar concepts, different APIs |

### Resource Management

| Godot | Unity | Notes |
|-------|-------|-------|
| ResourceLoader | Resources.Load | Different path conventions |
| `.tres` files | ScriptableObject assets | Unity assets are more editor-integrated |
| `preload()` | Direct references | Unity often uses serialized fields |
| Resource caching | AssetDatabase/Resources | Unity has multiple resource systems |

### Key Script Conversions

#### GameManager
- Godot: Autoload singleton with signals
- Unity: MonoBehaviour singleton with events
- Functions remain mostly identical

#### PlayerController
- Godot: Extends CharacterBody2D
- Unity: MonoBehaviour with Rigidbody2D component
- Input handling moved to separate methods
- Physics calculations adapted to Unity's system

#### ProcGenManager
- Godot: Autoload singleton for level generation
- Unity: MonoBehaviour singleton
- Room instantiation logic modified for Unity's prefab system

#### UI Controllers
- Godot: Control node extensions
- Unity: MonoBehaviour scripts attached to Canvas objects
- Signal connections replaced with event listeners

## Implementation Process

1. **Core Data Structures**: Maintain the same data models (RoomData, etc.)
2. **System Logic**: Keep algorithms identical (room generation, etc.)
3. **UI Flow**: Preserve user interface patterns
4. **Component Translation**: Convert Godot nodes to Unity components
5. **Events**: Change signal connections to C# events

## Unity-Specific Enhancements

- Improved editor workflow using custom inspectors
- More robust scene management through ScriptableObjects
- Built-in support for Unity animation and particle systems
- Integration with Unity's asset bundle system for content updates
- Potential for platform-specific optimizations

## Testing Approach

After conversion, verify that these key aspects match the Godot implementation:

1. **Room Generation**: Rooms should generate with the same patterns and connections
2. **Player Movement**: Controls should feel identical
3. **Combat**: Weapon behavior and enemy interactions should match
4. **Card Effects**: Tarot card effects should work the same way
5. **Progression**: Level advancement and difficulty scaling should be equivalent

## Conclusion

While the underlying technology is different, the gameplay experience and mechanics remain faithful to the original Godot implementation. The Unity version leverages Unity's component system, prefab workflow, and C# programming paradigms while maintaining the core game design and roguelike elements that define "Codex of the Broken Zodiac".