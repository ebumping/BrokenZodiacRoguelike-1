# CODEX OF THE BROKEN ZODIAC - DESIGN BIBLE

## GAME OVERVIEW

"Codex of the Broken Zodiac" is an isometric twin-stick roguelike shooter that explores complex player transformation mechanics through innovative zodiac-based sanity systems. The game delivers a unique cosmic horror experience with dynamic, procedurally generated gameplay and a sophisticated horror event generation system.

### Core Concept
Players navigate procedurally-generated cosmic horror environments, managing their sanity while battling eldritch entities. As their sanity deteriorates, they undergo transformation based on their zodiac sign, gaining new abilities but risking complete loss of control. The game features a tarot card progression system and elemental spell mechanics that synergize with the cosmic horror theme.

## CORE SYSTEMS

### 1. SANITY SYSTEM

#### Overview
The Sanity System is the central gameplay mechanic that affects perception, triggers transformations, and unlocks abilities. The player's sanity diminishes when taking damage, witnessing cosmic horrors, or using forbidden abilities. As sanity decreases, the player experiences visual and auditory hallucinations, environmental distortions, and eventually undergoes zodiac-specific transformations.

#### Sanity States
- **Stable (100%-80%)**: Normal gameplay with minimal effects.
- **Unsettled (79%-60%)**: Minor visual distortions, occasional whispers.
- **Disturbed (59%-40%)**: Moderate visual effects, enemy appearance distortion, sporadic hallucinations.
- **Fractured (39%-20%)**: Severe visual warping, frequent hallucinations, map alterations, first-stage zodiac transformation.
- **Shattered (19%-1%)**: Extreme reality distortion, constant hallucinations, second-stage zodiac transformation.
- **Transcended (0%)**: Final zodiac transformation, temporary loss of player control, ultimate abilities unlocked.

#### Implementation Details
```csharp
public class SanitySystem : MonoBehaviour
{
    [Range(0f, 100f)] public float currentSanity = 100f;
    public SanityState currentState;
    private ZodiacSign playerZodiacSign;
    private SanityEffectsManager effectsManager;
    private ZodiacSanityTransformation transformationManager;
    
    // Methods for managing sanity changes
    public void DecreaseSanity(float amount);
    public void IncreaseSanity(float amount);
    public void UpdateSanityState();
    
    // Event triggers based on sanity levels
    private void TriggerSanityEffects();
    private void TriggerZodiacTransformation();
}
```

### 2. COSMIC MADNESS SKILL TREE

#### Overview
The Cosmic Madness Skill Tree allows players to unlock and upgrade cosmic abilities as they progress through sanity thresholds. These abilities are themed around the cosmic horror elements and provide powerful advantages at the cost of sanity.

#### Key Abilities
- **EldrithVision**: Reveals hidden enemies and secrets through walls.
- **VoidWhisper**: Manipulates enemy behavior through cosmic suggestion.
- **EldrithTransformation**: Temporarily assumes an eldritch form with enhanced abilities.
- **CosmicAscension**: Ultimate ability that transforms the player into a cosmic entity.

#### Implementation Details
```csharp
public class CosmicMadnessManager : MonoBehaviour
{
    private List<CosmicAbility> unlockedAbilities = new List<CosmicAbility>();
    private SanitySystem sanitySystem;
    
    // Methods for managing cosmic abilities
    public void UnlockAbility(CosmicAbilityType type);
    public void UpgradeAbility(CosmicAbilityType type);
    public void UseAbility(CosmicAbilityType type);
    
    // Ability unlocking based on sanity thresholds
    private void CheckSanityThresholds();
}
```

### 3. ZODIAC-SPECIFIC SANITY TRANSFORMATION MECHANICS

#### Overview
As players lose sanity, they undergo transformations based on their zodiac sign. Each transformation occurs in three stages, with each stage unlocking new abilities and altering gameplay mechanics.

#### Zodiac Transformations
1. **Aries**: FireRamTransformation (First Stage) → BurningChargeTransformation (Second Stage) → CosmicInfernoTransformation (Final Stage)
2. **Taurus**: StoneHideTransformation → MountainFormTransformation → EarthTitanTransformation
3. **Gemini**: MirrorSelfTransformation → DualityManifestationTransformation → CosmicTwinTransformation
4. **Cancer**: ShellGuardTransformation → TidalPullTransformation → AbyssalCarapaceTransformation
5. **Leo**: SolarManeTransformation → StarLightTransformation → CosmicLionTransformation
6. **Virgo**: PurificationAuraTransformation → HealingResonanceTransformation → PerfectedFormTransformation
7. **Libra**: BalanceShiftTransformation → KarmicJudgmentTransformation → CosmicEquilibriumTransformation
8. **Scorpio**: VenomStingerTransformation → ShadowInfiltrationTransformation → DeathAspectTransformation
9. **Sagittarius**: SwiftArrowTransformation → CosmicArcherTransformation → StarbornHunterTransformation
10. **Capricorn**: TimeWarpTransformation → RealityAnchorTransformation → CosmicArchitectTransformation
11. **Aquarius**: MindfloodTransformation → CollectiveConsciousnessTransformation → UniversalNoosphereTransformation
12. **Pisces**: DualCurrentTransformation → OceanicMindTransformation → CosmicSeaTransformation

#### Unique Transformation Effects
- **AriesFireTrail**: Leaves damaging fire trail during dashes.
- **TaurusGroundTremor**: Creates shockwaves on heavy attacks.
- **GeminiMirrorClone**: Spawns damaging mirror images when attacking.
- **CancerTidalShield**: Reflects projectiles with water shield.
- **LeoSolarFlare**: Blinds enemies with bursts of light.
- **VirgoHealingPulse**: Emits healing waves on perfect dodges.
- **LibraDamageReflection**: Balances damage between player and enemies.
- **ScorpioVenomCloud**: Creates toxic clouds that damage enemies over time.
- **SagittariusCosmicArrows**: Fires homing cosmic arrows on critical hits.
- **CapricornTimeDistortion**: Creates bubbles of accelerated or decelerated time, affecting allies and enemies differently.
- **AquariusCollectiveInsight**: Links the minds of nearby allies, sharing buffs, health regeneration, and spell effects.
- **PiscesRealityRipple**: Creates reality distortions that confuse enemies.

#### Implementation Details
```csharp
public class ZodiacSanityTransformation : MonoBehaviour
{
    private ZodiacSign playerZodiacSign;
    private int currentTransformationStage = 0;
    private SanitySystem sanitySystem;
    
    // Methods for handling transformations
    public void InitiateTransformation(int stage);
    public void RevertTransformation();
    public void ApplyTransformationEffects();
    
    // Zodiac-specific transformation methods
    private void ApplyAriesTransformation(int stage);
    private void ApplyTaurusTransformation(int stage);
    // ... methods for all zodiac signs
}
```

### 4. PROCEDURAL HORROR EVENT GENERATOR

#### Overview
The Procedural Horror Event Generator creates dynamic horror events based on the player's sanity level, zodiac sign, and environmental context. These events range from minor environmental effects to major reality distortions and enemy spawns.

#### Event Categories
- **Environmental Effects**: Flickering lights, blood pools, gravity fluctuations, whispering walls.
- **Zodiac-Specific Events**: Unique events triggered by zodiac sign (e.g., AriesBurningVisions, TaurusEarthquake).
- **Hallucination-Based**: False enemies, phantom NPCs, illusory pathways, distorted perceptions.

#### Implementation Details
```csharp
public class HorrorEventGenerator : MonoBehaviour
{
    private List<HorrorEventBase> activeEvents = new List<HorrorEventBase>();
    private SanitySystem sanitySystem;
    private ZodiacSign playerZodiacSign;
    
    // Methods for generating and managing horror events
    public void GenerateEvent();
    public void EndEvent(HorrorEventBase eventToEnd);
    
    // Event generation based on sanity and zodiac
    private HorrorEventBase SelectAppropriateEvent();
    private void ApplyEventEffects(HorrorEventBase selectedEvent);
}

// Base class for all horror events
public abstract class HorrorEventBase
{
    public string eventName;
    public float duration;
    public float intensityFactor;
    
    public abstract void InitiateEvent();
    public abstract void UpdateEvent();
    public abstract void EndEvent();
}

// Example of a specific horror event
public class FlickeringLightsEvent : HorrorEventBase
{
    private Light[] affectedLights;
    private float flickerSpeed;
    
    public override void InitiateEvent()
    {
        // Find lights in the environment and make them flicker
    }
    
    public override void UpdateEvent()
    {
        // Update flickering effect based on intensity and time
    }
    
    public override void EndEvent()
    {
        // Restore lights to normal
    }
}
```

### 5. TAROT CARD SYSTEM

#### Overview
The Tarot Card System provides a progression mechanism through collectible cards that grant various abilities, modifiers, and effects. Cards are divided into Major and Minor Arcana, each with unique properties and effects that can synergize with other game systems.

#### Card Categories

##### Major Arcana (22 Cards)
Powerful, single-use cards that provide significant run-lasting effects.

- **The Fool (0)**: Grants immunity to the next fatal blow.
- **The Magician (I)**: Doubles the effect of the next spell cast.
- **The High Priestess (II)**: Reveals all secrets in the current and next three levels.
- **The Empress (III)**: Increases health regeneration by 100% for the remainder of the run.
- **The Emperor (IV)**: Increases damage by 25% for the remainder of the run.
- **The Hierophant (V)**: Unlocks a random zodiac ability regardless of sanity state.
- **The Lovers (VI)**: Combines effects of two randomly selected Minor Arcana cards.
- **The Chariot (VII)**: Increases movement speed by 30% for the remainder of the run.
- **Justice (VIII)**: Balances all stats (health, damage, speed) to their average value plus 10%.
- **The Hermit (IX)**: Provides stealth capability for 30 seconds when below 30% health.
- **Wheel of Fortune (X)**: Randomizes all current stat bonuses with potentially higher values.
- **Strength (XI)**: Increases max health by 50 points for the remainder of the run.
- **The Hanged Man (XII)**: Inverts damage taken and dealt for 10 seconds when activated.
- **Death (XIII)**: Instantly defeats all enemies in the current room, but rerolls all future drops.
- **Temperance (XIV)**: Provides perfect balance between sanity loss and gain for 5 minutes.
- **The Devil (XV)**: Increases damage by 50% but disables all healing for the remainder of the run.
- **The Tower (XVI)**: Destroys the current room structure, revealing all secrets and passages.
- **The Star (XVII)**: Creates a permanent checkpoint that can be returned to once.
- **The Moon (XVIII)**: Enhances all sanity-based abilities but doubles sanity loss rate.
- **The Sun (XIX)**: Fills sanity to maximum and prevents loss for 5 minutes.
- **Judgment (XX)**: Resurrects with full health if defeated in the next 10 minutes.
- **The World (XXI)**: Reveals paths to all special rooms for the remainder of the run.

##### Minor Arcana (56 Cards)
Stackable passive enhancements divided into four suits, each corresponding to an elemental keyword.

- **Wands (Fire)**: Focus on damage over time and area effects.
  - Ace of Wands: Adds 5% fire damage to all attacks.
  - Two-Ten of Wands: Escalating fire effects (burn duration, damage, area).
  - Page/Knight/Queen/King of Wands: Special fire abilities (fireballs, fire shields, etc.)

- **Cups (Water)**: Focus on healing and defensive abilities.
  - Ace of Cups: Adds 5% lifesteal to all attacks.
  - Two-Ten of Cups: Escalating healing effects (regeneration rate, heal amount, shield).
  - Page/Knight/Queen/King of Cups: Special water abilities (healing waves, water shields, etc.)

- **Swords (Air)**: Focus on critical damage and attack speed.
  - Ace of Swords: Adds 5% chance to cause bleeding on hit.
  - Two-Ten of Swords: Escalating bleeding effects (duration, damage, spread).
  - Page/Knight/Queen/King of Swords: Special air abilities (whirlwinds, floating, etc.)

- **Pentacles (Earth)**: Focus on resource generation and durability.
  - Ace of Pentacles: Increases currency gain by 5%.
  - Two-Ten of Pentacles: Escalating gold effects (drop rate, bonus amounts, conversions).
  - Page/Knight/Queen/King of Pentacles: Special earth abilities (stone armor, gold generation, etc.)

#### Card Combinations and Synergies
Collecting 3+ cards of the same suit or value unlocks combo effects:

- **Three of a Kind (Same Value)**: Enhances the basic effect (e.g., three 5s double their individual effects).
- **Flush (Same Suit)**: Activates the elemental mastery effect (e.g., all Wands give Fire Mastery).
- **Straight (Sequential Values)**: Provides progressive effects that chain together.
- **Full House (Three + Two)**: Combines major effects of both card groups.
- **Four of a Kind**: Provides significant boost to the card effect plus an activated ability.
- **Royal Flush**: Ultimate combo that transforms gameplay with a zodiac-specific super ability.

#### Implementation Details
```csharp
public enum TarotSuit { Wands, Cups, Swords, Pentacles }
public enum TarotRank { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Page, Knight, Queen, King }
public enum MajorArcana { Fool, Magician, HighPriestess, Empress, Emperor, Hierophant, Lovers, Chariot, Justice, Hermit, WheelOfFortune, Strength, HangedMan, Death, Temperance, Devil, Tower, Star, Moon, Sun, Judgement, World }

public class TarotCard
{
    public bool isMajorArcana;
    public MajorArcana? majorArcanaType;
    public TarotSuit? suit;
    public TarotRank? rank;
    public string cardName;
    public string description;
    public Sprite cardArt;
    
    public void ApplyCardEffect(PlayerController player);
    public void RemoveCardEffect(PlayerController player);
}

public class TarotDeck : MonoBehaviour
{
    private List<TarotCard> availableCards = new List<TarotCard>();
    private List<TarotCard> playerHand = new List<TarotCard>();
    private List<TarotCard> discardedCards = new List<TarotCard>();
    
    public void InitializeDeck();
    public TarotCard DrawCard();
    public void AddCardToHand(TarotCard card);
    public void DiscardCard(TarotCard card);
    public void CheckForCombinations();
    public void ApplyCombinationEffects(List<TarotCard> combination, CombinationType type);
}
```

### 6. SPELL SYSTEM

#### Overview
The Spell System provides players with elemental abilities that can be combined, upgraded, and modified based on zodiac sign and sanity level. Spells range from direct damage attacks to utility abilities and summons.

#### Spell Elements
- **Fire**: Damage over time, area effects.
- **Water**: Healing, shields, crowd control.
- **Earth**: Barriers, traps, resource generation.
- **Air**: Movement speed, projectile manipulation.
- **Void**: Sanity manipulation, reality distortion (unlocks at low sanity).
- **Cosmic**: Ultimate abilities that combine elements (unlocks at critical sanity).

#### Spell Categories
- **Attack Spells**: Direct damage spells with various elements and effects.
  - FireBolt: Basic fire projectile with burn effect.
  - IceSpear: Water projectile that slows enemies.
  - StoneShards: Earth projectile that knocks back enemies.
  - WindSlash: Air projectile with increased velocity.
  - VoidBeam: Void projectile that drains sanity from enemies.
  - CosmicNova: Cosmic explosion that affects all elements.

- **Utility Spells**: Support abilities that provide various benefits.
  - FlameShield: Fire barrier that damages enemies on contact.
  - HealingWave: Water spell that restores health to player and allies.
  - EarthWall: Earth barrier that blocks enemy projectiles.
  - WindDash: Air movement ability for quick repositioning.
  - VoidShroud: Void concealment that makes player temporarily invisible.
  - CosmicSight: Cosmic ability that reveals all enemies and secrets.

- **Summoning Spells**: Creates temporary allies or environmental effects.
  - FireElemental: Summons a fire entity that attacks enemies.
  - WaterSprite: Summons a water entity that heals allies.
  - EarthGolem: Summons an earth entity that tanks damage.
  - AirWisp: Summons an air entity that increases movement speed.
  - VoidTentacle: Summons a void entity that grabs and holds enemies.
  - CosmicAvatar: Summons a cosmic entity that combines all elemental powers.

#### Spell Modifications
Spells can be modified based on zodiac sign, sanity level, and tarot card combinations:

- **Zodiac Modifications**: Each zodiac sign provides unique spell alterations (e.g., Aries enhances fire spells).
- **Sanity Effects**: Low sanity increases spell power but adds chaotic effects.
- **Tarot Enhancements**: Tarot card combinations can boost specific spell elements or effects.

#### Spell Casting System
Players can equip up to three spells at once, cycling through them with scroll wheel or dedicated buttons. Spells consume mana, which regenerates over time or through special actions.

#### Implementation Details
```csharp
public enum SpellElement { Fire, Water, Earth, Air, Void, Cosmic }
public enum SpellCategory { Attack, Utility, Summoning }

public abstract class Spell
{
    public string spellName;
    public SpellElement element;
    public SpellCategory category;
    public float manaCost;
    public float cooldown;
    public Sprite spellIcon;
    
    protected PlayerController caster;
    protected ZodiacSign casterZodiacSign;
    protected float sanityLevel;
    
    public abstract void CastSpell(Vector3 targetPosition);
    public abstract void ApplyZodiacModification();
    public abstract void ApplySanityEffects();
    public abstract void ApplyTarotEnhancements(List<TarotCard> activeCards);
}

public class SpellManager : MonoBehaviour
{
    private List<Spell> availableSpells = new List<Spell>();
    private List<Spell> equippedSpells = new List<Spell>();
    private int currentSpellIndex = 0;
    
    private float currentMana;
    private float maxMana;
    private float manaRegenRate;
    
    public void EquipSpell(Spell spell, int slotIndex);
    public void UnequipSpell(int slotIndex);
    public void CycleSpells(bool forward);
    public void CastCurrentSpell(Vector3 targetPosition);
    public void UpdateMana();
}

// Example of a concrete spell implementation
public class FireBolt : Spell
{
    public float baseDamage;
    public float burnDuration;
    public float burnDamagePerSecond;
    public GameObject projectilePrefab;
    
    public override void CastSpell(Vector3 targetPosition)
    {
        // Instantiate projectile and set properties
        // Apply modifications based on zodiac, sanity, and tarot cards
    }
    
    public override void ApplyZodiacModification()
    {
        if (casterZodiacSign == ZodiacSign.Aries)
        {
            baseDamage *= 1.5f;
            burnDuration *= 1.2f;
        }
        // Apply other zodiac modifications
    }
    
    public override void ApplySanityEffects()
    {
        if (sanityLevel < 50f)
        {
            baseDamage *= 1 + ((50f - sanityLevel) / 50f);
            // Add chaotic effects at very low sanity
        }
    }
    
    public override void ApplyTarotEnhancements(List<TarotCard> activeCards)
    {
        // Apply effects based on active tarot cards
    }
}
```

### 7. SPELL CRAFTING SYSTEM WITH ZODIAC RESONANCE

#### Overview
The Spell Crafting System allows players to combine spell components and reagents to create custom spells with unique effects. Each zodiac sign has a natural resonance with specific spell combinations, providing powerful synergies when properly aligned.

#### Crafting Components
- **Spell Cores**: Base components that determine the primary function (projectile, area effect, buff, debuff).
  - Fire Core: Creates damage-focused spells with burn effects.
  - Water Core: Creates healing or crowd control spells.
  - Earth Core: Creates defensive or terrain manipulation spells.
  - Air Core: Creates movement or projectile manipulation spells.
  - Void Core: Creates sanity manipulation or reality distortion spells (requires low sanity).
  - Cosmic Core: Creates hybrid spells that combine multiple elements (requires critical sanity).

- **Catalysts**: Secondary components that modify the spell's behavior.
  - Amplifier: Increases spell power at the cost of increased mana consumption.
  - Stabilizer: Reduces mana cost but also reduces power.
  - Accelerator: Reduces cooldown but decreases duration.
  - Expander: Increases area of effect but reduces intensity.
  - Focuser: Increases precision and critical chance but reduces area effects.
  - Chaos Shard: Unpredictable effects that change with sanity level.

- **Essences**: Elemental materials that add specific effects to spells.
  - Phoenix Feather: Adds burning aftereffects to spells.
  - Abyssal Pearl: Adds slowing or freezing effects to spells.
  - Terravore Root: Adds knockback or stun effects to spells.
  - Zephyr Crystal: Adds velocity or chain effects to spells.
  - Void Fragment: Adds sanity drain or reality distortion effects.
  - Cosmic Dust: Adds unpredictable but powerful effects based on current zodiac alignment.

#### Zodiac Resonance
Each zodiac sign resonates with specific spell combinations, providing enhanced effects:

- **Fire Signs (Aries, Leo, Sagittarius)**: 
  - Resonates strongly with Fire Core + Phoenix Feather combinations
  - Creates "Solar Flare" effects when properly aligned
  - Spell Example: Phoenix Blast - A fire projectile that splits into multiple flaming orbs on impact

- **Earth Signs (Taurus, Virgo, Capricorn)**: 
  - Resonates strongly with Earth Core + Terravore Root combinations
  - Creates "Tectonic Shift" effects when properly aligned
  - Spell Example: Stone Rampart - Creates a defensive wall that damages enemies who touch it

- **Air Signs (Gemini, Libra, Aquarius)**: 
  - Resonates strongly with Air Core + Zephyr Crystal combinations
  - Creates "Cyclone Vortex" effects when properly aligned
  - Spell Example: Thought Nexus - Links targets together, sharing damage and healing effects

- **Water Signs (Cancer, Scorpio, Pisces)**: 
  - Resonates strongly with Water Core + Abyssal Pearl combinations
  - Creates "Abyssal Current" effects when properly aligned
  - Spell Example: Mind Tide - Creates a wave that confuses enemies, making them attack each other

#### Crafting Process
1. Select a Spell Core (determines base function)
2. Add a Catalyst (modifies behavior)
3. Infuse with Essence (adds elemental effects)
4. Attune to Zodiac Sign (applies resonance bonuses)
5. Test the spell in a specially designated area

#### Zodiac Attunement Bonus Effects
- **Perfect Attunement**: When crafting a spell perfectly aligned with the player's zodiac sign, creates a "Celestial Spell" with dramatically enhanced effects.
- **Cross-Sign Crafting**: Crafting spells aligned with other zodiac signs is possible but provides reduced benefits.
- **Opposition Crafting**: Crafting spells aligned with opposing zodiac signs (opposite on the wheel) creates "Dissonant Spells" with unpredictable but potentially powerful effects.

#### Implementation Details
```csharp
public class SpellCraftingSystem : MonoBehaviour
{
    private List<SpellCore> availableCores = new List<SpellCore>();
    private List<SpellCatalyst> availableCatalysts = new List<SpellCatalyst>();
    private List<SpellEssence> availableEssences = new List<SpellEssence>();
    private ZodiacSign playerZodiacSign;
    private SanitySystem sanitySystem;
    
    public Spell CraftSpell(SpellCore core, SpellCatalyst catalyst, SpellEssence essence);
    public float CalculateZodiacResonance(Spell craftedSpell, ZodiacSign zodiacSign);
    public void ApplyZodiacAttunement(Spell spell, float resonanceValue);
    public void UnlockCraftingComponent(CraftingComponentType type, string componentId);
}

public abstract class SpellCore
{
    public string coreName;
    public SpellElement element;
    public SpellCategory baseCategory;
    public float basePower;
    public float baseCooldown;
    public float baseManaCost;
    
    public abstract SpellBehavior CreateBaseBehavior();
    public abstract void ApplyCatalystModification(SpellCatalyst catalyst);
    public abstract void ApplyEssenceEffect(SpellEssence essence);
}

public abstract class SpellCatalyst
{
    public string catalystName;
    public CatalystType type;
    public float powerModifier;
    public float cooldownModifier;
    public float manaCostModifier;
    public float durationModifier;
    public float areaModifier;
    
    public abstract void ApplyToCoreStats(SpellCore core);
    public abstract void ModifySpellBehavior(SpellBehavior behavior);
}

public abstract class SpellEssence
{
    public string essenceName;
    public SpellElement element;
    public EffectType effectType;
    public float effectPower;
    public float effectDuration;
    
    public abstract void ApplyElementalEffect(SpellBehavior behavior);
    public abstract float GetZodiacResonance(ZodiacSign zodiacSign);
}
```

### 8. DESTRUCTIBLE ENVIRONMENTS

#### Overview
The Destructible Environments system allows players to interact with and modify the game world, uncovering secrets, creating tactical advantages, and accessing hidden areas. The system focuses on secret walls, breakable objects, and environmental interactions that change based on player actions and sanity levels.

#### Destructible Elements

- **Secret Walls**: Hidden passages that can be revealed through specific actions.
  - Perception-based: Visible only at certain sanity thresholds.
  - Interact-based: Require specific actions (pushing, examining) to reveal.
  - Attack-based: Can be destroyed with sufficient damage or specific weapon types.
  - Zodiac-aligned: Respond only to players with specific zodiac signs or abilities.

- **Weakened Structures**: Damaged parts of the environment that can be destroyed.
  - Cracked Walls: Can be broken to create new passages or shortcuts.
  - Unstable Floors: Can collapse to reveal hidden areas below.
  - Decayed Supports: Can be destroyed to trigger environmental hazards or changes.

- **Reactive Elements**: Environmental features that respond to player actions or abilities.
  - Elemental Conduits: Channels that respond to specific spell elements.
  - Sanity Anchors: Objects that stabilize or destabilize reality based on player sanity.
  - Zodiac Seals: Special barriers or mechanisms attuned to specific zodiac signs.

#### Secret Room Types

- **Relic Chambers**: Hidden rooms containing powerful artifacts or tarot cards.
- **Sanctuary Spaces**: Safe areas that restore sanity and provide temporary buffs.
- **Eldritch Libraries**: Rooms with lore elements and potential skill enhancements.
- **Nightmare Pockets**: Dangerous areas with powerful enemies but valuable rewards.
- **Zodiac Shrines**: Special rooms attuned to specific zodiac signs, offering sign-specific upgrades.

#### Interaction Mechanics

- **Perception System**: The player's ability to detect secret walls changes based on sanity.
  - High Sanity: Secret walls appear as subtle visual cues (slight discoloration, air shimmer).
  - Medium Sanity: Some secrets become more visible (glowing runes, obvious cracks).
  - Low Sanity: Reality distortion may reveal hidden paths automatically but create false ones too.

- **Destructive Interactions**: Methods for breaking through destructible elements.
  - Weapon Damage: Different weapons have varying effectiveness against different materials.
  - Spell Effects: Elemental spells can interact with environments (fire burns wooden barriers, etc.).
  - Zodiac Abilities: Certain zodiac transformations grant special environment interaction capabilities.

- **Environmental Chain Reactions**: Destroying one element might trigger cascading effects.
  - Structural Collapse: Breaking support columns may cause ceiling collapse.
  - Flooding: Breaking water pipes might fill rooms, creating new paths or obstacles.
  - Fires: Igniting flammable materials can spread, revealing metal structures behind.

#### Implementation Details
```csharp
public class DestructibleEnvironment : MonoBehaviour
{
    public DestructibleType type;
    public EnvironmentMaterial material;
    public float durability;
    public ZodiacSign? requiredZodiacSign;
    public float sanityThresholdToReveal;
    public GameObject hiddenContent;
    public List<DestructibleEnvironment> chainReactionElements;
    
    private bool isRevealed;
    private bool isDestroyed;
    
    public void AttemptReveal(PlayerController player);
    public void AttemptDestroy(DamageInfo damageInfo);
    public void TriggerChainReaction();
    public bool CheckZodiacRequirement(ZodiacSign playerSign);
    public bool CheckSanityRequirement(float playerSanity);
}

public class SecretWall : DestructibleEnvironment
{
    public RevealMethod revealMethod;
    public InteractRequirement interactRequirement;
    public GameObject revealedPathway;
    public ParticleSystem revealEffect;
    
    public override void OnReveal();
    public override void OnDestroy();
}

public class EnvironmentManager : MonoBehaviour
{
    private List<DestructibleEnvironment> activeEnvironmentElements;
    private SanitySystem sanitySystem;
    private ZodiacSign playerZodiacSign;
    
    public void UpdateEnvironmentPerception(float playerSanity);
    public void CheckForZodiacInteractions();
    public void HandleElementDestruction(DestructibleEnvironment destroyedElement);
    public void RevealSecretRooms(SecretRoomType type, Vector3 playerPosition, float radius);
}
```

### 9. MULTIPLAYER SYSTEM

#### Overview
The Multiplayer System allows up to four players to join a session, each with a unique zodiac sign and class. Players can interact with each other's sanity states, share tarot effects, and combine spells for powerful synergies.

#### Multiplayer Features
- **Co-op Progression**: Shared progression through levels with individual and team objectives.
- **Sanity Influence**: Players can influence each other's sanity (positively or negatively).
- **Spell Combinations**: Multiple players can combine spells for enhanced effects.
- **Tarot Sharing**: Certain tarot cards can share effects with teammates.
- **Zodiac Synergies**: Complementary zodiac signs provide team bonuses.

#### Implementation Details
```csharp
public class MultiplayerManager : MonoBehaviour
{
    private List<PlayerController> connectedPlayers = new List<PlayerController>();
    private Dictionary<ZodiacSign, ZodiacSign> complementarySigns = new Dictionary<ZodiacSign, ZodiacSign>();
    
    public void InitializeMultiplayerSession();
    public void AddPlayer(PlayerController newPlayer);
    public void RemovePlayer(PlayerController player);
    public void CheckForZodiacSynergies();
    public void ApplyTeamEffects();
    public void CombinePlayerSpells(Spell spell1, Spell spell2, Vector3 targetPosition);
}
```

## ART ASSET REQUIREMENTS

### 1. CHARACTER ASSETS

#### Base Character Models (6 Classes)
- **Occult Detective**
  - Prompt: "Isometric pixel art character, occult detective with trenchcoat and revolver, cosmic horror game, muted color palette with purple accents, standing pose ready for twin-stick shooter controls"
  - Variations: Walking, Shooting, Dodging, Special Ability, Death

- **Apostate Medium**
  - Prompt: "Isometric pixel art character, mystical medium with spectral connections, flowing robes, eldritch symbols, cosmic horror game, muted color palette with blue accents, standing pose ready for twin-stick shooter controls"
  - Variations: Walking, Shooting, Dodging, Special Ability, Death

- **Irredeemable Debtor**
  - Prompt: "Isometric pixel art character, desperate debtor with chaingun weapon, tattered business attire, occult markings, cosmic horror game, muted color palette with red accents, standing pose ready for twin-stick shooter controls"
  - Variations: Walking, Shooting, Dodging, Special Ability, Death

- **Reclusive Archivist**
  - Prompt: "Isometric pixel art character, scholarly archivist with energy beam staff, surrounded by floating tomes, cosmic horror game, muted color palette with yellow accents, standing pose ready for twin-stick shooter controls"
  - Variations: Walking, Shooting, Dodging, Special Ability, Death

- **Seditious Orator**
  - Prompt: "Isometric pixel art character, charismatic speaker with dual pistols, formal attire with propaganda symbols, cosmic horror game, muted color palette with green accents, standing pose ready for twin-stick shooter controls"
  - Variations: Walking, Shooting, Dodging, Special Ability, Death

- **Phantom Constable**
  - Prompt: "Isometric pixel art character, spectral police officer with lawgiver revolver, ghostly uniform with badge, cosmic horror game, muted color palette with cyan accents, standing pose ready for twin-stick shooter controls"
  - Variations: Walking, Shooting, Dodging, Special Ability, Death

#### Zodiac Transformation Models (36 Total - 3 Stages for Each Sign)
- **Aries Transformations**
  - Stage 1: "Isometric pixel art character with small flaming horns and ember trail, aries zodiac transformation, cosmic horror game, muted color palette with orange fire accents"
  - Stage 2: "Isometric pixel art character with large burning ram horns and fiery body, aries zodiac transformation, cosmic horror game, muted color palette with intense orange and red fire effects"
  - Stage 3: "Isometric pixel art character fully transformed into cosmic fire ram entity, aries zodiac final transformation, cosmic horror game, muted color palette with supernatural flames and eldritch symbols"

- **Taurus Transformations**
  - Stage 1: "Isometric pixel art character with stone-like skin patches and small bull horns, taurus zodiac transformation, cosmic horror game, muted color palette with earth tones"
  - Stage 2: "Isometric pixel art character with full rocky hide and large bull horns, taurus zodiac transformation, cosmic horror game, muted color palette with pronounced earth and stone textures"
  - Stage 3: "Isometric pixel art character fully transformed into cosmic earth bull entity, taurus zodiac final transformation, cosmic horror game, muted color palette with crystalline growths and eldritch symbols"

- **Gemini Transformations**
  - Stage 1: "Isometric pixel art character with spectral twin shadow, gemini zodiac transformation, cosmic horror game, muted color palette with silver accents"
  - Stage 2: "Isometric pixel art character with manifest twin connected by cosmic energy, gemini zodiac transformation, cosmic horror game, muted color palette with mirror effects and duality symbols"
  - Stage 3: "Isometric pixel art character fully transformed with cosmic twin entity merged/separated, gemini zodiac final transformation, cosmic horror game, muted color palette with reality-splitting effects and eldritch symbols"

[Similar prompts for remaining zodiac signs - Cancer, Leo, Virgo, Libra, Scorpio, Sagittarius, Capricorn, Aquarius, Pisces]

### 2. ENVIRONMENT ASSETS

#### Procedural Room Tilesets
- **Laboratory Tileset**
  - Prompt: "Isometric pixel art tileset for cosmic horror laboratory, examination tables, specimen containers, eldritch machinery, broken equipment, muted color palette with green accents, seamless tiles for procedural generation"
  - Required Pieces: Floor, Walls, Corners, Doorways, Obstacles, Decoration

- **Library Tileset**
  - Prompt: "Isometric pixel art tileset for cosmic horror library, ancient bookshelves, forbidden tomes, reading tables, eldritch symbols, muted color palette with yellow accents, seamless tiles for procedural generation"
  - Required Pieces: Floor, Walls, Corners, Doorways, Obstacles, Decoration

- **Ritual Chamber Tileset**
  - Prompt: "Isometric pixel art tileset for cosmic horror ritual chamber, altars, summoning circles, candles, occult symbols, muted color palette with red accents, seamless tiles for procedural generation"
  - Required Pieces: Floor, Walls, Corners, Doorways, Obstacles, Decoration

- **Void Space Tileset**
  - Prompt: "Isometric pixel art tileset for cosmic horror void space, non-euclidean architecture, floating platforms, reality tears, cosmic energy flows, muted color palette with purple accents, seamless tiles for procedural generation"
  - Required Pieces: Floor, Walls, Corners, Doorways, Obstacles, Decoration

- **Desolate Wasteland Tileset**
  - Prompt: "Isometric pixel art tileset for cosmic horror wasteland, blighted ground, twisted vegetation, abandoned structures, cosmic corruption, muted color palette with brown accents, seamless tiles for procedural generation"
  - Required Pieces: Floor, Walls, Corners, Doorways, Obstacles, Decoration

#### Horror Event Visual Effects
- **Environmental Distortions**
  - Prompt: "Pixel art animation sequence of reality warping effect, walls bulging, floor shifting, cosmic horror style, muted colors with sudden flashes, seamless looping for game environment distortion"

- **Sanity Visualizations**
  - Prompt: "Pixel art UI elements showing sanity deterioration, eldritch symbols growing at screen edges, subtle eye motifs, cosmic horror whispers visualized, muted color palette with increasing intensity as sanity drops"

- **Zodiac Event Manifestations**
  - Prompt: "Pixel art animation sequence of zodiac [sign] cosmic event, [specific zodiac theme] emerging from reality tear, muted color palette with [zodiac-specific color] highlights, suitable for twin-stick shooter horror game"

### 3. ENEMY ASSETS

#### Basic Enemies (12 Types - One Per Zodiac)
- **Aries Cultist**
  - Prompt: "Isometric pixel art enemy, cultist with ram skull mask and fire rituals, aries-themed cosmic horror, muted color palette with orange accents, animation frames for movement and attack"

- **Taurus Abomination**
  - Prompt: "Isometric pixel art enemy, hulking bull-headed monstrosity with stone-like hide, taurus-themed cosmic horror, muted color palette with brown accents, animation frames for movement and attack"

[Similar prompts for remaining zodiac enemies]

#### Elite Enemies (6 Types - Aligned with Classes)
- **Corrupted Detective**
  - Prompt: "Isometric pixel art elite enemy, twisted detective figure with elongated limbs and void-infused revolver, cosmic horror transformation, muted color palette with dark purple corruption, animation frames for movement and special attacks"

[Similar prompts for remaining elite enemies]

#### Boss Enemies (4 Types)
- **The Astrologer**
  - Prompt: "Isometric pixel art boss enemy, massive floating astrologer with multiple arms holding zodiac artifacts, cosmic horror entity, muted color palette with celestial highlights, multiple animation frames for complex attack patterns"

- **The Broken Wheel**
  - Prompt: "Isometric pixel art boss enemy, giant zodiac wheel construct with corrupted symbols and mechanical-organic hybrid parts, cosmic horror entity, muted color palette with circuit-like energy flows, multiple animation frames for rotation and attacks"

- **The Constellation Beast**
  - Prompt: "Isometric pixel art boss enemy, amorphous entity composed of twisted star patterns and cosmic dust, forming and reforming into different zodiac constellations, cosmic horror entity, muted color palette with stellar highlights, multiple animation frames for transformation sequences"

- **The Eclipse**
  - Prompt: "Isometric pixel art final boss enemy, colossal cosmic entity manifesting as a sentient eclipse, corona of eldritch energy and gravitational distortions, ultimate cosmic horror entity, muted color palette with reality-tearing effects, multiple animation frames for cosmic-scale attacks"

### 4. ITEM ASSETS

#### Weapons (24 Total - 4 Per Class)
- **Occult Detective Weapons**
  - Prompt: "Isometric pixel art weapon set for occult detective character, 4 variations of marksman rifles with eldritch modifications, cosmic horror style, muted color palette with subtle glows, different sizes and effects"

[Similar prompts for remaining class weapons]

#### Tarot Cards (78 Total - 22 Major, 56 Minor)
- **Major Arcana**
  - Prompt: "Pixel art tarot card [card name], cosmic horror interpretation of traditional symbolism, muted color palette with [appropriate accent color], subtle eldritch elements, suitable as collectible game item"

- **Minor Arcana - Wands**
  - Prompt: "Pixel art minor arcana tarot card, [card name] of Wands, cosmic horror interpretation with fire elements, muted color palette with orange accents, eldritch symbols, suitable as collectible game item"

[Similar prompts for remaining suits]

#### Spell Icons (36 Total - 6 Elements with 6 Spells Each)
- **Fire Spell Icons**
  - Prompt: "Pixel art spell icon set, 6 fire element spells for cosmic horror game, variations of flame effects with eldritch modifications, muted color palette with orange and red highlights, distinct shapes for UI display"

[Similar prompts for remaining elements]

### 5. UI ASSETS

#### Main Interface Elements
- **Sanity Meter**
  - Prompt: "Pixel art UI element, sanity meter for cosmic horror game, gradually morphing from stable to corrupted appearance, muted color palette transitioning from blue to red, clear visual states for different sanity levels"

- **Zodiac Indicator**
  - Prompt: "Pixel art UI element, zodiac sign indicator with all 12 symbols arranged in wheel format, cosmic horror style, muted color palette with current sign highlighted, subtle animation for active state"

- **Tarot Hand Display**
  - Prompt: "Pixel art UI element, card hand display showing 5 tarot card slots, cosmic horror style, muted color palette with ornate frame, clear states for filled and empty slots"

- **Spell Selection Wheel**
  - Prompt: "Pixel art UI element, radial spell selection wheel with 6 slots, cosmic horror style, muted color palette with elemental color coding, distinct active/inactive states"

#### Menu Screens
- **Main Menu**
  - Prompt: "Pixel art game menu screen, cosmic horror twin-stick shooter title screen, zodiac wheel motif with broken/corrupted sections, muted color palette with subtle animations, title 'Codex of the Broken Zodiac' in eldritch font"

- **Character Selection**
  - Prompt: "Pixel art game menu screen, character selection interface with 6 character slots and zodiac sign selection, cosmic horror twin-stick shooter style, muted color palette with class-specific highlights, clear UI elements"

- **Options Menu**
  - Prompt: "Pixel art game menu screen, options interface with eldritch-styled sliders and buttons, cosmic horror twin-stick shooter style, muted color palette with subtle animated elements, functional yet thematic design"

## TECHNICAL IMPLEMENTATION GUIDELINES

### Unity Setup
- Use Unity 2022.3 LTS or newer
- Configure for 2D isometric rendering with URP
- Set up pixel-perfect camera system
- Implement proper lighting for horror atmosphere
- Configure input system for both keyboard+mouse and controller support

### Core Programming Architecture
- Implement entity component system for efficient object management
- Create modular systems with proper dependency injection
- Design event-driven communication between systems
- Implement save/load functionality with data persistence
- Ensure proper optimization for procedural generation

### Unity-Specific Implementation
- Use ScriptableObjects for data-driven design (tarot cards, spells, enemy types)
- Implement procedural generation with Unity's tile system and custom generators
- Use shader graph for sanity-based visual effects and distortions
- Implement post-processing for horror atmosphere (using URP Post Processing)
- Configure appropriate colliders and physics for isometric twin-stick mechanics

## CONCLUSION

"Codex of the Broken Zodiac" is a comprehensive Unity-based isometric twin-stick roguelike shooter with deep systems integration. The game's core innovations lie in its zodiac-based sanity transformation system, tarot card progression, and spell combinations, creating a unique cosmic horror experience that changes based on player choices and randomized elements.

The technical implementation leverages Unity's strengths in 2D rendering, procedural generation, and modular system design, allowing for efficient development and future expandability through additional zodiac abilities, tarot cards, and horror events.
