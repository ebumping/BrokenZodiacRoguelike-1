# Sanity System Documentation

## Overview

The Sanity System is a core gameplay mechanic that affects how players perceive and interact with the game world based on their current mental state. As sanity decreases, the game world becomes increasingly distorted and dangerous, with hallucinations and visual effects making it harder to distinguish between reality and illusion.

## Core Components

### 1. SanitySystem.cs

The central manager that tracks player sanity levels and coordinates all sanity-related effects:

- Manages sanity values for all players
- Defines sanity thresholds and states
- Triggers hallucinations and effects
- Handles sanity loss/gain events
- Controls post-processing visual effects

### 2. SanityUI.cs

Responsible for displaying the player's current sanity state via the UI:

- Shows sanity meter and current state
- Animates UI elements based on sanity level
- Provides visual feedback during hallucinations
- Displays warning indicators at low sanity

### 3. SanityEnemyAppearance.cs

Alters how enemies appear based on player sanity:

- Changes enemy meshes and materials
- Modifies animation speed and behavior
- Adds visual effects to enemies
- Creates glitching animations at low sanity

### 4. SanityMiniMapEffect.cs

Distorts the mini-map based on sanity level:

- Shakes room icons
- Distorts connections between rooms
- Changes colors of rooms and paths
- Adds hallucinated rooms and connections

## Sanity States

The system defines five distinct sanity states, each with increasing severity:

1. **Normal (70-100%)**: Default state, no effects
2. **Wary (50-70%)**: Subtle audio cues, slight unease
3. **Disturbed (30-50%)**: Visual and audio distortions begin
4. **Unstable (10-30%)**: Hallucinations, strong audio/visual effects
5. **Broken (0-10%)**: Extreme hallucinations, gameplay altering effects

## Sanity Loss Triggers

The following events can cause sanity loss:

- Enemy encounters (more for bosses)
- Taking damage
- Being at low health
- Ally deaths
- Dark areas
- Corrupted rooms

## Sanity Gain Triggers

Players can regain sanity through:

- Killing enemies (more for bosses)
- Healing
- Clearing rooms
- Using consumables
- Natural regeneration after delay

## Hallucination Types

The system can generate several types of hallucinations:

1. **Visual Distortion**: Screen effects, color changes
2. **Audio Hallucination**: Whispers, sounds, voices
3. **Environment Change**: Objects appearing/disappearing
4. **False Enemy**: Enemies that aren't really there
5. **False Attack**: Fake damage/attack indicators

## Integration with Other Systems

### GameManager Integration

- Registers players with sanity system
- Handles enemy/player events
- Controls camera shake effects

### Player Integration

- Subscribes to damage/health events
- Tracks current sanity state
- Applies effects on state change

### ProcGen Integration

- Tracks room corruption status
- Handles room lighting data
- Provides spatial information for effects

## Implementation Example

### Adding Sanity Loss on Custom Event

```csharp
// Example: Lose sanity when entering a ritual area
public void OnEnterRitualArea(PlayerController player)
{
    if (player == null) return;
    
    int playerId = player.NetworkId;
    float sanityLoss = 15f; // Large amount for dramatic effect
    
    // Apply sanity loss via the sanity system
    SanitySystem.Instance?.ModifyPlayerSanity(playerId, -sanityLoss);
    
    // Maybe trigger a forced hallucination for dramatic effect
    SanitySystem.Instance?.TriggerHallucination(playerId, true);
}
```

### Adding Custom Hallucination Response

```csharp
// Subscribe to hallucination events
SanitySystem.Instance.OnPlayerHallucination += OnHallucination;

// Handle hallucination events
private void OnHallucination(int playerId, SanityHallucination type)
{
    // Only respond to local player
    if (playerId != NetworkManager.LocalPlayerId) return;
    
    // Custom response to false attacks
    if (type == SanityHallucination.FalseAttack)
    {
        // Play a heartbeat sound
        AudioManager.Instance?.PlaySound("heartbeat");
        
        // Make the player flinch
        PlayerAnimator.SetTrigger("Flinch");
    }
}
```

## Best Practices

1. **Balance** - Carefully tune sanity loss/gain rates to create tension without frustration

2. **Feedback** - Always provide clear visual and audio feedback when sanity changes

3. **Recovery** - Ensure players have meaningful ways to recover sanity

4. **Progression** - Scale sanity effects with game progression and difficulty

5. **Performance** - Use object pooling for hallucinations to minimize garbage collection

6. **Consistency** - Maintain a consistent visual language for sanity effects

## Known Limitations

1. Hallucinated enemies cannot deal actual damage

2. Some visual effects require the Universal Render Pipeline (URP)

3. Audio effects are temporarily limited by available sound assets

4. Network synchronization of hallucinations is limited to avoid excessive bandwidth usage

## Planned Enhancements

1. More varied and dynamic hallucinations

2. Class-specific sanity effects and resistance

3. Tarot card interactions with the sanity system

4. Environmental storytelling through sanity visions

5. Expanded audio hallucination system