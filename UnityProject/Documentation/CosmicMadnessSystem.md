# Cosmic Madness Skill Tree Documentation

## Overview

The Cosmic Madness Skill Tree is an advanced progression system that unlocks unique abilities triggered by low sanity. It offers players powerful abilities with zodiac-specific variations, creating a dynamic and personalized gameplay experience based on the player's chosen zodiac sign.

## Core Components

### 1. CosmicMadnessAbility.cs

The base class for all cosmic madness abilities:

- Defines common properties and methods for all abilities
- Handles activation conditions based on sanity levels
- Provides zodiac-specific variations of abilities
- Manages cooldowns and resource costs

### 2. CosmicMadnessManager.cs

Central controller for the entire system:

- Tracks unlocked and active abilities for all players
- Manages madness points for unlocking new abilities
- Handles automatic activation of abilities at low sanity
- Provides events for UI and gameplay integration

### 3. CosmicMadnessUI.cs

User interface for the skill tree:

- Displays available, unlocked, and active abilities
- Shows connections between prerequisite abilities
- Provides detailed descriptions with zodiac-specific information
- Allows players to unlock new abilities with madness points

## Ability Categories

The system organizes abilities into distinct categories that reflect different aspects of cosmic madness:

1. **Transformation** - Physical changes to the player character
2. **Perception** - Changes to how the player perceives the world
3. **Manifestation** - Creating things from nothing
4. **Destruction** - Enhanced destructive capabilities
5. **Manipulation** - Control over the environment or enemies
6. **Transcendence** - Abilities beyond physical limitations

## Implemented Abilities

### 1. Eldritch Vision (Perception)

Allows players to see hidden enemies, items, and passages:

- **Base Effect**: Reveals hidden objects with colored outlines
- **Gemini**: +50% vision range for all detections
- **Sagittarius**: +30% vision range for all detections
- **Pisces**: +100% enemy detection range
- **Virgo**: +50% hidden item detection range
- **Libra**: Temporarily reveals hidden objects for a duration

### 2. Void Whisper (Manifestation/Destruction)

Channels the void to gain knowledge or attack enemies:

- **Base Effect**: Channel the void to either reveal map information or damage enemies
- **Cancer**: +50% defense during channeling
- **Scorpio**: +30% damage on void attacks
- **Leo**: +20% chance for knowledge instead of attacks
- **Capricorn**: -30% channel time
- **Aquarius**: +50% effect radius

### 3. Eldritch Transformation (Transformation)

Transforms the player into an eldritch form with enhanced abilities:

- **Base Effect**: Increased stats and visual transformation
- **Zodiac-specific forms**: Each zodiac has unique transformation with special abilities
  - Flying abilities (Gemini, Sagittarius, Aquarius)
  - Phase-through ability (Pisces, Cancer, Scorpio)
  - Teleportation (Gemini, Sagittarius)
  - Damage aura (Leo, Aries, Scorpio)
  - Minion summoning (Cancer, Gemini, Virgo)

### 4. Cosmic Ascension (Transcendence)

Ultimate ability that grants vastly increased power and zodiac-specific enhancements:

- **Base Effect**: Massive stat boosts and visual effects
- **Enhancement Types**:
  - **Destruction Beam** (Aries, Leo, Scorpio): Powerful continuous beam weapon
  - **Time Manipulation** (Capricorn, Aquarius, Pisces): Slow time around the player
  - **Minion Summoning** (Cancer, Gemini, Virgo): Summon powerful cosmic minions
  - **Elemental Mastery** (Taurus, Libra, Sagittarius): Control over elements (fire, ice, lightning, void, cosmic)

## Progression System

### Madness Points

Players earn Cosmic Madness points through:

- **Low Sanity**: Points accumulate while at low sanity levels
- **Boss Kills**: Defeating bosses grants substantial points
- **Special Events**: Discovering eldritch artifacts or locations

### Unlocking Abilities

Abilities have requirements to unlock:

- **Madness Points**: Each ability costs points to unlock
- **Prerequisites**: Some abilities require others to be unlocked first
- **Minimum Sanity State**: Some abilities require reaching certain sanity states
- **Zodiac Compatibility**: Some abilities are only available to specific zodiac signs

## Activation Mechanics

### Manual Activation

Some abilities can be manually triggered with keyboard shortcuts:

- Z key for Cosmic Ascension
- X key for Eldritch Transformation
- V key for Void Whisper

### Automatic Activation

Abilities can activate automatically when:

- Player sanity drops below ability-specific thresholds
- Specific in-game events occur
- During certain encounters or locations

## Integration with Other Systems

### Sanity System

- Abilities are triggered by low sanity thresholds
- Using abilities can further drain sanity
- Some abilities temporarily increase sanity

### Combat System

- Many abilities enhance combat capabilities
- Abilities can change damage types and effects
- Some abilities provide defensive capabilities

### Progression System

- Abilities unlock new gameplay options
- Advanced abilities require significant investment
- End-game abilities dramatically change gameplay

## Implementation Example

### Unlocking an Ability

```csharp
// In player controller or UI handler
void UnlockCosmicAbility()
{
    // Get the ability to unlock
    CosmicMadnessAbility ability = abilityDatabase.GetAbility("EldrithVision");
    
    // Try to unlock it for the local player
    int playerId = NetworkManager.LocalPlayerId;
    bool success = CosmicMadnessManager.Instance.UnlockAbility(playerId, ability);
    
    if (success)
    {
        // Handle successful unlock
        GameUI.ShowNotification("You've unlocked Eldrith Vision!");
    }
}
```

### Checking for Active Abilities

```csharp
// In enemy controller
void CheckIfVisible()
{
    // Get local player
    PlayerController player = GameManager.Instance.LocalPlayer;
    int playerId = player.NetworkId;
    
    // Check if player has Eldritch Vision active
    bool canSeeHidden = CosmicMadnessManager.Instance.IsAbilityActive(
        playerId, abilityDatabase.GetAbility("EldrithVision"));
    
    // Set visibility based on ability
    if (isHidden && !canSeeHidden)
    {
        SetVisible(false);
    }
    else
    {
        SetVisible(true);
    }
}
```

## User Interface

The Cosmic Madness skill tree UI provides a visual representation of available abilities:

- Abilities are organized by category in columns
- Lines connect prerequisite abilities
- Colors indicate different states (available, locked, active)
- Detailed tooltips show zodiac-specific information
- Point costs and requirements are clearly displayed

## Sound and Visual Effects

The system includes extensive audiovisual feedback:

- Unique sounds for ability activation and deactivation
- Visual effects for different ability types
- Post-processing effects for perception changes
- Material and mesh changes for transformations
- Particle systems for cosmic manifestations

## Customization for Modding

The system is designed to be extensible:

- New abilities can be added by creating ScriptableObject assets
- Zodiac effects can be modified in the Inspector
- Visual and audio effects can be customized
- Balance parameters are exposed for easy tuning

## Known Limitations

1. Transformation effects currently limited to material/mesh swaps (future: skeleton-based transformations)
2. Minion AI is basic and will be enhanced in future updates
3. Networking synchronization requires additional work for multiplayer
4. Some visual effects require URP shaders to be fully realized

## Future Enhancements

1. More ability types with deeper gameplay integration
2. Advanced transformation system with blendshapes and morphing
3. Quest-based ability unlocks tied to storyline
4. Expanded zodiac synergies between multiple abilities
5. Enemy-specific reactions to cosmic abilities