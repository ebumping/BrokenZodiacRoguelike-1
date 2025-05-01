# TAROT CARDS & SPELLS SYSTEM DOCUMENTATION

## TAROT CARD SYSTEM

### Overview
The Tarot Card System provides a progression mechanism through collectible cards that grant various abilities, modifiers, and effects. Cards are divided into Major and Minor Arcana, each with unique properties and effects that can synergize with other game systems.

### Card Categories

#### Major Arcana (22 Cards)
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

#### Minor Arcana (56 Cards)
Stackable passive enhancements divided into four suits, each corresponding to an elemental keyword.

##### Wands (Fire)
Focus on damage over time and area effects.
- **Ace of Wands**: Adds 5% fire damage to all attacks.
- **Two-Ten of Wands**: Escalating fire effects (burn duration, damage, area).
- **Page/Knight/Queen/King of Wands**: Special fire abilities (fireballs, fire shields, etc.)

##### Cups (Water)
Focus on healing and defensive abilities.
- **Ace of Cups**: Adds 5% lifesteal to all attacks.
- **Two-Ten of Cups**: Escalating healing effects (regeneration rate, heal amount, shield).
- **Page/Knight/Queen/King of Cups**: Special water abilities (healing waves, water shields, etc.)

##### Swords (Air)
Focus on critical damage and attack speed.
- **Ace of Swords**: Adds 5% chance to cause bleeding on hit.
- **Two-Ten of Swords**: Escalating bleeding effects (duration, damage, spread).
- **Page/Knight/Queen/King of Swords**: Special air abilities (whirlwinds, floating, etc.)

##### Pentacles (Earth)
Focus on resource generation and durability.
- **Ace of Pentacles**: Increases currency gain by 5%.
- **Two-Ten of Pentacles**: Escalating gold effects (drop rate, bonus amounts, conversions).
- **Page/Knight/Queen/King of Pentacles**: Special earth abilities (stone armor, gold generation, etc.)

### Card Combinations and Synergies
Collecting 3+ cards of the same suit or value unlocks combo effects:

- **Three of a Kind (Same Value)**: Enhances the basic effect (e.g., three 5s double their individual effects).
- **Flush (Same Suit)**: Activates the elemental mastery effect (e.g., all Wands give Fire Mastery).
- **Straight (Sequential Values)**: Provides progressive effects that chain together.
- **Full House (Three + Two)**: Combines major effects of both card groups.
- **Four of a Kind**: Provides significant boost to the card effect plus an activated ability.
- **Royal Flush**: Ultimate combo that transforms gameplay with a zodiac-specific super ability.

### Technical Implementation
The Tarot Card system is implemented using ScriptableObjects for card definitions and a manager class that handles the player's hand, deck, and combination detection.

```csharp
// Card definition ScriptableObject
[CreateAssetMenu(fileName = "New Tarot Card", menuName = "Codex/Tarot/Card")]
public class TarotCardDefinition : ScriptableObject
{
    public string cardName;
    public CardType cardType; // Major or Minor
    public int cardValue;
    public TarotSuit suit; // For Minor Arcana
    public MajorArcana arcanaType; // For Major Arcana
    public Sprite cardArtwork;
    public string description;
    public CardEffect[] effects;
}

// Card manager that handles the player's hand
public class TarotCardManager : MonoBehaviour
{
    public List<TarotCardDefinition> availableCards = new List<TarotCardDefinition>();
    public List<TarotCardDefinition> playerHand = new List<TarotCardDefinition>();
    public List<TarotCardDefinition> discardPile = new List<TarotCardDefinition>();
    
    // Core methods
    public void AddCardToHand(TarotCardDefinition card);
    public void DiscardCard(TarotCardDefinition card);
    public void ShuffleDeck();
    public TarotCardDefinition DrawCard();
    
    // Combination detection
    public void CheckForCardCombinations();
    private List<CardCombination> FindCombinations();
    private void ApplyCombinationEffects(CardCombination combination);
}
```

## SPELL SYSTEM

### Overview
The Spell System provides players with elemental abilities that can be combined, upgraded, and modified based on zodiac sign and sanity level. Spells range from direct damage attacks to utility abilities and summons.

### Spell Elements
- **Fire**: Damage over time, area effects.
- **Water**: Healing, shields, crowd control.
- **Earth**: Barriers, traps, resource generation.
- **Air**: Movement speed, projectile manipulation.
- **Void**: Sanity manipulation, reality distortion (unlocks at low sanity).
- **Cosmic**: Ultimate abilities that combine elements (unlocks at critical sanity).

### Spell Categories

#### Attack Spells
Direct damage spells with various elements and effects.
- **FireBolt**: Basic fire projectile with burn effect.
- **IceSpear**: Water projectile that slows enemies.
- **StoneShards**: Earth projectile that knocks back enemies.
- **WindSlash**: Air projectile with increased velocity.
- **VoidBeam**: Void projectile that drains sanity from enemies.
- **CosmicNova**: Cosmic explosion that affects all elements.

#### Utility Spells
Support abilities that provide various benefits.
- **FlameShield**: Fire barrier that damages enemies on contact.
- **HealingWave**: Water spell that restores health to player and allies.
- **EarthWall**: Earth barrier that blocks enemy projectiles.
- **WindDash**: Air movement ability for quick repositioning.
- **VoidShroud**: Void concealment that makes player temporarily invisible.
- **CosmicSight**: Cosmic ability that reveals all enemies and secrets.

#### Summoning Spells
Creates temporary allies or environmental effects.
- **FireElemental**: Summons a fire entity that attacks enemies.
- **WaterSprite**: Summons a water entity that heals allies.
- **EarthGolem**: Summons an earth entity that tanks damage.
- **AirWisp**: Summons an air entity that increases movement speed.
- **VoidTentacle**: Summons a void entity that grabs and holds enemies.
- **CosmicAvatar**: Summons a cosmic entity that combines all elemental powers.

### Spell Modifications
Spells can be modified based on zodiac sign, sanity level, and tarot card combinations:

- **Zodiac Modifications**: Each zodiac sign provides unique spell alterations (e.g., Aries enhances fire spells).
- **Sanity Effects**: Low sanity increases spell power but adds chaotic effects.
- **Tarot Enhancements**: Tarot card combinations can boost specific spell elements or effects.

### Technical Implementation
The Spell system is implemented using ScriptableObjects for spell definitions and component-based behavior for in-game effects:

```csharp
// Spell definition ScriptableObject
[CreateAssetMenu(fileName = "New Spell", menuName = "Codex/Spells/Spell")]
public class SpellDefinition : ScriptableObject
{
    public string spellName;
    public SpellElement element;
    public SpellCategory category;
    public float manaCost;
    public float cooldown;
    public Sprite icon;
    public GameObject visualEffectPrefab;
    public AudioClip castSound;
    public SpellEffectDefinition[] effects;
}

// Spell manager that handles the player's equipped spells
public class PlayerSpellManager : MonoBehaviour
{
    public List<SpellDefinition> availableSpells = new List<SpellDefinition>();
    public SpellDefinition[] equippedSpells = new SpellDefinition[3]; // 3 equipped spell slots
    public int currentSpellIndex = 0;
    
    public float currentMana;
    public float maxMana;
    public float manaRegenRate;
    
    // Core methods
    public void EquipSpell(SpellDefinition spell, int slotIndex);
    public void CycleActiveSpell(bool forward);
    public void CastCurrentSpell();
    
    // Mana management
    public void UseMana(float amount);
    public void RegenerateMana();
    
    // Modification systems
    public void ApplyZodiacModifications(SpellDefinition spell);
    public void ApplySanityEffects(SpellDefinition spell);
    public void ApplyTarotEnhancements(SpellDefinition spell);
}
```

## SPELL CRAFTING SYSTEM

### Overview
The Spell Crafting System allows players to combine spell components and reagents to create custom spells with unique effects. Each zodiac sign has a natural resonance with specific spell combinations, providing powerful synergies when properly aligned.

### Crafting Components

#### Spell Cores
Base components that determine the primary function (projectile, area effect, buff, debuff).
- **Fire Core**: Creates damage-focused spells with burn effects.
- **Water Core**: Creates healing or crowd control spells.
- **Earth Core**: Creates defensive or terrain manipulation spells.
- **Air Core**: Creates movement or projectile manipulation spells.
- **Void Core**: Creates sanity manipulation or reality distortion spells (requires low sanity).
- **Cosmic Core**: Creates hybrid spells that combine multiple elements (requires critical sanity).

#### Catalysts
Secondary components that modify the spell's behavior.
- **Amplifier**: Increases spell power at the cost of increased mana consumption.
- **Stabilizer**: Reduces mana cost but also reduces power.
- **Accelerator**: Reduces cooldown but decreases duration.
- **Expander**: Increases area of effect but reduces intensity.
- **Focuser**: Increases precision and critical chance but reduces area effects.
- **Chaos Shard**: Unpredictable effects that change with sanity level.

#### Essences
Elemental materials that add specific effects to spells.
- **Phoenix Feather**: Adds burning aftereffects to spells.
- **Abyssal Pearl**: Adds slowing or freezing effects to spells.
- **Terravore Root**: Adds knockback or stun effects to spells.
- **Zephyr Crystal**: Adds velocity or chain effects to spells.
- **Void Fragment**: Adds sanity drain or reality distortion effects.
- **Cosmic Dust**: Adds unpredictable but powerful effects based on current zodiac alignment.

### Zodiac Resonance
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

### Technical Implementation
The Spell Crafting system is implemented using a component-based approach for mixing and matching different spell elements:

```csharp
// Spell crafting manager
public class SpellCraftingManager : MonoBehaviour
{
    public List<SpellCore> availableCores = new List<SpellCore>();
    public List<SpellCatalyst> availableCatalysts = new List<SpellCatalyst>();
    public List<SpellEssence> availableEssences = new List<SpellEssence>();
    
    private ZodiacSign playerZodiacSign;
    private float playerSanity;
    
    // Core crafting method
    public SpellDefinition CraftSpell(SpellCore core, SpellCatalyst catalyst, SpellEssence essence)
    {
        // Create base spell from core
        SpellDefinition newSpell = CreateBaseSpell(core);
        
        // Apply catalyst modifications
        ApplyCatalystEffects(newSpell, catalyst);
        
        // Apply essence effects
        ApplyEssenceEffects(newSpell, essence);
        
        // Calculate and apply zodiac resonance
        float resonance = CalculateZodiacResonance(core, catalyst, essence, playerZodiacSign);
        ApplyResonanceEffects(newSpell, resonance);
        
        // Finalize spell
        return FinalizeSpell(newSpell);
    }
    
    // Helper methods
    private SpellDefinition CreateBaseSpell(SpellCore core);
    private void ApplyCatalystEffects(SpellDefinition spell, SpellCatalyst catalyst);
    private void ApplyEssenceEffects(SpellDefinition spell, SpellEssence essence);
    private float CalculateZodiacResonance(SpellCore core, SpellCatalyst catalyst, SpellEssence essence, ZodiacSign sign);
    private void ApplyResonanceEffects(SpellDefinition spell, float resonance);
    private SpellDefinition FinalizeSpell(SpellDefinition spell);
}
```

## INTEGRATION BETWEEN TAROT & SPELL SYSTEMS

### Tarot-Spell Synergies
Tarot cards can directly affect spell behavior and effectiveness:

- **The Magician + Any Spell**: Doubles the effect power of the next cast.
- **Wands Suit + Fire Spells**: Enhanced fire damage and burn duration.
- **Cups Suit + Water Spells**: Enhanced healing and shield effects.
- **Swords Suit + Air Spells**: Enhanced critical chance and attack speed.
- **Pentacles Suit + Earth Spells**: Enhanced durability and resource generation.

### Spell-Crafting-Tarot Integration
Tarot cards can unlock special crafting components or enhance resonances:

- **The High Priestess**: Reveals rare spell components in the current level.
- **Four of a Kind (Any Suit)**: Temporarily enhances resonance with that suit's element.
- **The Hermit**: Allows crafting while in combat by slowing time briefly.
- **Justice**: Balances all negative catalyst effects, reducing downsides.

### Technical Implementation of Integration
The integration is handled through event systems and shared managers:

```csharp
public class SystemIntegrationManager : MonoBehaviour
{
    public TarotCardManager tarotManager;
    public PlayerSpellManager spellManager;
    public SpellCraftingManager craftingManager;
    
    // Event subscription
    private void OnEnable()
    {
        tarotManager.OnCardAdded += HandleNewTarotCard;
        tarotManager.OnCombinationFormed += HandleTarotCombination;
        spellManager.OnSpellCast += HandleSpellCast;
    }
    
    // Integration handlers
    private void HandleNewTarotCard(TarotCardDefinition card)
    {
        // Apply tarot card effects to spell system
        foreach (var spell in spellManager.equippedSpells)
        {
            if (spell != null)
            {
                ApplyTarotEffectToSpell(card, spell);
            }
        }
        
        // Unlock crafting components if applicable
        if (ShouldUnlockCraftingComponent(card))
        {
            UnlockCraftingComponent(card);
        }
    }
    
    private void HandleTarotCombination(CardCombination combination)
    {
        // Apply combination effects to spell system
        ApplyCombinationEffectsToSpells(combination);
        
        // Enhance resonances if applicable
        if (ShouldEnhanceResonance(combination))
        {
            EnhanceZodiacResonance(combination);
        }
    }
    
    private void HandleSpellCast(SpellDefinition spell)
    {
        // Check for tarot effects that trigger on spell cast
        CheckForTarotTriggerEffects(spell);
    }
    
    // Helper methods
    private void ApplyTarotEffectToSpell(TarotCardDefinition card, SpellDefinition spell);
    private bool ShouldUnlockCraftingComponent(TarotCardDefinition card);
    private void UnlockCraftingComponent(TarotCardDefinition card);
    private void ApplyCombinationEffectsToSpells(CardCombination combination);
    private bool ShouldEnhanceResonance(CardCombination combination);
    private void EnhanceZodiacResonance(CardCombination combination);
    private void CheckForTarotTriggerEffects(SpellDefinition spell);
}
```

## CONCLUSION

The Tarot Card and Spell Systems form a deeply integrated part of the gameplay experience in "Codex of the Broken Zodiac." These systems complement the core sanity and zodiac mechanics, providing players with tools to customize their playstyle and adapt to the challenges they face in the procedurally generated environments. The synergies between these systems create emergent gameplay opportunities and strategic depth, rewarding experimentation and discovery.