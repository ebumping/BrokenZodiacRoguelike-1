# Zodiac-Specific Sanity Transformation Mechanics

## Overview

When a player's sanity drops below critical thresholds, they undergo a dramatic transformation based on their zodiac sign. These transformations alter appearance, grant unique abilities, and modify stats. Each zodiac sign has a distinct transformation that reflects its core themes and elemental associations.

## Core Components

### 1. ZodiacSanityTransformation.cs

Main controller that handles the transformation process:

- Monitors player sanity and triggers transformations at low thresholds
- Manages transformation duration and cooldowns
- Applies and removes stat modifiers
- Handles visual and gameplay effects
- Implements zodiac-specific special abilities

### 2. ZodiacTransformationData

Data structure defining properties of each transformation:

- Basic information (name, description, duration)
- Stat modifiers
- Visual effects (materials, meshes, auras)
- Special abilities (flight, phasing, etc.)
- Damage auras and other effects

### 3. ZodiacTransformationDefinitions

Scriptable object containing all zodiac transformation definitions:

- Factory methods for creating default transformations
- Detailed implementations for all 12 zodiac signs
- Balanced attributes for gameplay

## Transformation Process

### Triggering Conditions

- **Automatic**: Occurs when sanity drops below the `transformationSanityThreshold` (default 30%)
- **Cooldown**: A cooldown period prevents frequent transformations
- **Reversion**: Character reverts to normal when sanity rises above `transformationRegainThreshold` (default 40%)
- **Manual**: Can be triggered for testing with F2 key

### Transformation Effects

1. **Visual Changes**
   - Material and mesh replacement
   - Custom aura effects
   - Transformation particle effects

2. **Stat Modifications**
   - Movement speed changes
   - Damage modifiers
   - Defense/health adjustments
   - Resource and cooldown modifications

3. **Special Abilities**
   - Flying/floating
   - Phasing through walls
   - Damage immunities
   - Status effect immunities
   - Sanity drain protection
   - Damage auras

4. **Custom Mechanics**
   - Zodiac-specific behaviors
   - Special effects (fire trails, tremors, etc.)
   - Minion summoning

## Zodiac Transformations

### Aries: The Flame Ram

- **Theme**: Fire, aggression, speed
- **Appearance**: Body ignites with celestial fire
- **Stats**: +50% speed, +40% damage, -30% defense
- **Special**: Creates fire trails and explosive impacts
- **Aura**: Fiery damage aura burning nearby enemies

### Taurus: The Stone Bull

- **Theme**: Earth, endurance, stability
- **Appearance**: Skin hardens to stone
- **Stats**: -30% speed, +200% defense, +50% health, +20% damage
- **Special**: Footsteps cause tremors that stagger enemies
- **Immunity**: Status effect immunity

### Gemini: The Phantom Twin

- **Theme**: Air, duality, illusion
- **Appearance**: Spectral dual-state form
- **Stats**: +30% speed, -20% defense, +10% damage, +30% cooldown reduction
- **Special**: Creates mirror images that confuse enemies
- **Ability**: Can phase through walls

### Cancer: The Abyssal Crab

- **Theme**: Water, protection, regeneration
- **Appearance**: Hardened carapace of cosmic chitin
- **Stats**: -20% speed, +100% defense, +30% health
- **Special**: Regenerates when damaged
- **Ability**: Can burrow through walls, immune to sanity drain

### Leo: The Solar Lion

- **Theme**: Fire, light, power
- **Appearance**: Body radiates intense light
- **Stats**: +20% speed, +50% damage, +10% defense
- **Special**: Roar can stun enemies and shatter objects
- **Aura**: Solar damage aura that reveals secrets

### Virgo: The Cosmic Maiden

- **Theme**: Earth, purity, perfection
- **Appearance**: Pure celestial form
- **Stats**: +10% speed, +20% defense, +100% mana, +40% cooldown reduction
- **Special**: Creates healing energy, projectiles split and home
- **Immunity**: Status effect and sanity drain immunity

### Libra: The Void Balance

- **Theme**: Air, balance, harmony
- **Appearance**: Form splits between light and dark
- **Stats**: +0% speed (unchanged), +50% defense, +30% damage
- **Special**: Enemies that deal damage receive equal damage in return
- **Aura**: Balanced aura that reflects damage

### Scorpio: The Void Scorpion

- **Theme**: Water, venom, transformation
- **Appearance**: Chitinous form with venomous sting
- **Stats**: +10% speed, +40% defense, +30% damage
- **Special**: Attacks inflict cosmic poison that stacks and spreads
- **Aura**: Venomous aura that applies poison
- **Immunity**: Status effect immunity

### Sagittarius: The Cosmic Archer

- **Theme**: Fire, movement, precision
- **Appearance**: Swift and ethereal form
- **Stats**: +60% speed, -30% defense, +40% damage, +50% cooldown reduction
- **Special**: Attacks become piercing cosmic arrows that chain
- **Ability**: Can dash/fly through the air

### Capricorn: The Cosmic Goat

- **Theme**: Earth, climbing, persistence
- **Appearance**: Mountain goat with cosmic elements
- **Stats**: +30% speed, +70% defense, +20% damage, +30% health
- **Special**: Can scale any surface, attacks push enemies back
- **Duration**: Longest transformation duration (55 seconds)

### Aquarius: The Void Current

- **Theme**: Air, flow, innovation
- **Appearance**: Fluid current of cosmic energy
- **Stats**: +40% speed, -20% defense, +20% damage, +50% mana
- **Special**: Creates puddles of slowing cosmic energy
- **Ability**: Can flow through small spaces and phase through walls

### Pisces: The Abyssal Fish

- **Theme**: Water, fluidity, perception
- **Appearance**: Fluid, ethereal form
- **Stats**: +50% speed, +0% defense (unchanged), +20% damage, +100% mana
- **Special**: Can see through illusions and walls
- **Ability**: Can swim through air, immune to sanity drain
- **Aura**: Trail of cosmic water that damages enemies

## Implementation Details

### Helper Components

- **FlightComponent**: Enables flying/floating for certain transformations
- **PhaseComponent**: Allows phasing through walls and terrain
- **DamageAuraComponent**: Creates damage-dealing auras
- **MirrorImageBehavior**: Controls mirror image movement and behavior

### Special Effects

- **Fire trails**: Created by Aries transformation
- **Ground tremors**: Generated by Taurus transformation
- **Mirror images**: Spawned by Gemini transformation
- **Visual distortions**: Applied to player models during transformation

## Integration with Other Systems

### Sanity System

- Transformations are triggered by the sanity system
- Some transformations make the player immune to sanity drain
- Transformations can continue to drain sanity while active

### Combat System

- Transformations modify damage, defense, and attack behaviors
- Special attacks and abilities affect enemy interactions
- Some transformations reflect or absorb damage

### Movement System

- Flight and phase abilities modify movement mechanics
- Speed changes affect player control and navigation
- Special movement abilities like wall climbing are enabled

## Game Balance Considerations

- **Advantages vs. Disadvantages**: Each transformation has trade-offs (e.g., more damage but less defense)
- **Duration Balancing**: More powerful transformations have shorter durations
- **Cooldown Management**: Prevents transformation spamming
- **Sanity Economy**: Transformations can continue to drain sanity, creating resource management decisions

## Example Usage

```csharp
// To manually trigger a transformation (for debugging/testing)
ZodiacSanityTransformation transformation = player.GetComponent<ZodiacSanityTransformation>();
if (transformation != null)
{
    transformation.BeginTransformation(true); // force transformation
}

// To check if a player is transformed
bool isTransformed = transformation.IsTransformed;

// To get current transformation data
ZodiacTransformationData currentTransform = transformation.CurrentTransformation;
string transformName = currentTransform.TransformationName;
```

## Future Enhancements

1. **Visual Transformations**: More dramatic mesh replacements and VFX
2. **Sound Design**: Unique sound effects for each transformation
3. **Environment Interactions**: Transformations affecting the environment more deeply
4. **Player Progression**: Allowing players to unlock enhanced versions of their zodiac transformation
5. **Multiplayer Integration**: Visual indicators and networking for transformations