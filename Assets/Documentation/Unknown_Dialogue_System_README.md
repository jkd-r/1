# Unknown Dialogue System - Comprehensive Guide

## Overview

The Unknown Dialogue System is Protocol EMR's mysterious communication framework that allows an enigmatic entity to guide, comment, and react to player actions without breaking immersion. The system presents the entity as "Unknown" rather than an AI or narrator, maintaining the game's mysterious atmosphere.

## Core Philosophy

- **Mysterious, Not Mechanical**: Messages feel like they come from an intelligent, observant entity
- **Immersive Communication**: No "system" language or obvious AI references
- **Context-Aware**: Messages adapt to player actions, difficulty, and game progression
- **Non-Intrusive**: Communication enhances rather than interrupts gameplay

## System Architecture

### Components

1. **UnknownDialogueManager** - Central orchestrator for message selection and triggering
2. **UnknownMessageDatabase** - ScriptableObject containing all messages
3. **UnknownPhoneUI** - Phone-style chat interface
4. **UnknownHUDOverlay** - Glitch-effect screen overlay
5. **UnknownDialogueTriggers** - Static helper class for triggering messages
6. **DynamicEventOrchestrator** *(Sprint 9+)* - Procedural scheduler that feeds bespoke triggers into the dialogue system
7. **WorldStateBlackboard** *(Sprint 9+)* - Lightweight world-state cache shared between procedural systems and the narrator

### Message Flow

```
Game Event → Trigger Call → UnknownDialogueManager
    ↓
Message Selection (context-aware, weighted)
    ↓
Display via Phone UI and/or HUD Overlay
    ↓
History Tracking & Cooldown Management
```

### Dynamic Event Integration (Sprint 9+)

| Component | Responsibility | Dialogue Touchpoints |
|-----------|----------------|----------------------|
| **DynamicEventOrchestrator** | Scans chunk-level world state, schedules ambient/combat/puzzle encounters deterministically via SeedManager | Emits `TriggerDynamicEventStarted`, `TriggerDynamicEventMilestone`, `TriggerDynamicEventResolved/Failed` with contextual payloads |
| **WorldStateBlackboard** | Tracks chunk occupancy, threat levels, player style deltas, mission flags | Supplies `UnknownMessageEvent` context data (chunk id, threat, player ratios, tags) |
| **ProceduralEventProfile** | ScriptableObject defining spawn rules, cooldowns, dialogue tags | Declares which Unknown triggers fire on start/end/milestones |
| **PerformanceMonitor** | Surfaced debug metrics for QA (F1) | Shows active dynamic events + F9 toggle hint |

**Trigger Set (new):**
- `DynamicEventAmbient` – low-threat anomalies, environmental beats
- `DynamicEventCombat` – waves/ambushes sourced from NPCManager
- `DynamicEventPuzzle` – emergent logic/pattern encounters
- `DynamicEventStarted` / `DynamicEventResolved` / `DynamicEventFailed`
- `DynamicEventMilestone` – progress markers or batched notifications

**Context Hydration:** Every orchestrator callback injects:
- `eventId`, `eventType`, `chunkId`, `threatLevel`
- Player style ratios (stealth/combat/exploration/puzzle)
- Mission flags + custom dialogue tags from the active profile
- Timing metadata (`duration`, `progress`, `isStart`)

**Spam Guardrails:**
- Dialogue triggers are queued and processed at most once per frame
- Unknown global cooldown still applies, plus per-event cooldowns from the orchestrator
- `TriggerDynamicEventBatch()` can collapse multiple same-frame callbacks into the highest-priority message (Combat > Puzzle > Ambient)

**QA Workflow:**
1. Press **F1** to open the Performance Monitor overlay → verify “Dynamic Events” block.
2. Press **F9** to toggle the orchestrator debug HUD and visualize active/upcoming events.
3. Trigger ambient/combat/puzzle encounters (or use SeedManager deterministic scopes) and confirm matching Unknown messages with chunk/threat context in the phone UI.

## Message Database

### Categories

- **Combat**: Battle-related messages
- **Puzzle**: Logic and problem-solving
- **Exploration**: Discovery and navigation
- **Mission**: Objectives and goals
- **Narrative**: Story and lore
- **Warning**: Danger alerts
- **Encouragement**: Positive reinforcement
- **Commentary**: Observations on player actions

### Triggers

The system supports 35+ trigger types including:

- Player combat actions (hit, damage, dodge)
- Puzzle interactions (encountered, failed, solved)
- Exploration events (area discovered, secret found)
- Mission updates (start, milestone, complete)
- Story progression (plot points, procedural story)
- Player behavior (stealth, aggression, reading)
- **Dynamic events** *(Sprint 9+)*: ambient, combat, puzzle, started, resolved, failed, milestone

### Message Properties

Each message includes:
- **Text**: The actual message content (<200 characters)
- **Category**: Message classification
- **Trigger**: Specific event that can trigger it
- **Difficulty Range**: Min/max difficulty levels (0-3)
- **Game Stage**: Early (0), Mid (1), or Late (2) game
- **Display Mode**: Phone, HUD overlay, or both
- **Display Delay**: Time before message appears (0-3s)
- **Display Duration**: How long message stays visible (0-10s)
- **Cooldown**: Time before message can repeat (0-300s)
- **Can Repeat**: Whether message is one-time or repeatable
- **Selection Weight**: Priority in selection algorithm (0.1-10)

## Message Selection Algorithm

The system uses intelligent selection with multiple filters:

1. **Eligibility Filter**: Match trigger, difficulty, game stage
2. **Personality Filter**: Adapt to Unknown's communication style
3. **Recency Filter**: Avoid repeating recent messages
4. **Player Style Filter**: Consider player behavior patterns
5. **Weighted Random**: Select from remaining candidates

### Selection Performance

- Message selection: <1ms per trigger
- Eligibility filtering: O(n) where n = total messages
- History checking: O(log n) with optimized lookups

## Communication Channels

### Phone Chat Interface

The phone UI provides:
- Scrollable message history
- Typing animations (0.05s per character)
- Timestamp display
- Unread message badge
- Message received sound effects
- Toggle with 'C' key (from Input System)

**Features:**
- Non-blocking: Can be opened/closed anytime
- Persistent: Full conversation history saved
- Accessible: Supports text scaling from settings

### HUD Glitch Overlay

The HUD overlay displays:
- Screen-edge messages (non-intrusive positioning)
- Glitch visual effects (chromatic aberration, distortion)
- Fade in/out animations
- Brief static sound effects
- Automatic dismissal after duration

**Effects:**
- Glitch intensity: Configurable (0-1)
- Screen shake: Subtle text displacement
- Color overlay: Cyan/blue digital aesthetic
- Accessibility: Reduced intensity option

## Personalization System

### Unknown Personality Modes

1. **Verbose** (0)
   - More frequent messages
   - Longer, explanatory text
   - Helpful and descriptive

2. **Balanced** (1) - Default
   - Moderate frequency
   - Concise, cryptic messages
   - Mix of helpful and mysterious

3. **Cryptic** (2)
   - Rare messages
   - Very short text (<50 chars)
   - Maximum mystery

### Adaptive Messaging

The system tracks player behavior:
- **Stealth Ratio**: Percentage of stealth actions
- **Aggression Ratio**: Percentage of combat actions
- **Exploration Ratio**: Percentage of discovery actions

Messages adapt based on these ratios:
- High stealth → More stealth-themed messages
- High aggression → More combat-themed messages
- High exploration → More discovery-themed messages

## Game Progression

### Game Stages

Messages vary by progression:

**Stage 0 (Early Game)**
- More guidance
- Explanatory tone
- Introduces mechanics
- Builds mystery

**Stage 1 (Mid Game)**
- Less direct guidance
- Assumes player knowledge
- Deepens narrative
- More cryptic

**Stage 2 (Late Game)**
- Minimal guidance
- Reveals deeper truths
- Maximum mystery
- Story resolution hints

### Difficulty Scaling

Messages adapt to difficulty:
- **Easy (0)**: More frequent, helpful hints
- **Normal (1)**: Balanced guidance
- **Hard (2)**: Rare, cryptic hints
- **Extreme (3)**: Minimal assistance

## Integration Points

### Combat System Integration

```csharp
// When player hits NPC
UnknownDialogueTriggers.TriggerPlayerHitNPC(npcObject, damageAmount);

// When player takes damage
UnknownDialogueTriggers.TriggerPlayerTookDamage(attacker, damage, currentHP, maxHP);

// When NPC is defeated
UnknownDialogueTriggers.TriggerNPCDefeated(npcObject);

// When player dodges
UnknownDialogueTriggers.TriggerDodgeSuccessful();
```

### Exploration Integration

```csharp
// When entering new area
UnknownDialogueTriggers.TriggerNewAreaDiscovered("Area Name");

// When finding secret
UnknownDialogueTriggers.TriggerSecretFound(secretObject);

// When finding item
UnknownDialogueTriggers.TriggerItemFound(itemObject);
```

### Mission System Integration

```csharp
// When starting mission
UnknownDialogueTriggers.TriggerMissionStart("Mission Name");

// When reaching milestone
UnknownDialogueTriggers.TriggerMissionMilestone("Milestone Name");

// When completing mission
UnknownDialogueTriggers.TriggerMissionComplete("Mission Name");
```

### NPC AI Integration

```csharp
// In NPCPerception.cs or NPCController.cs
if (playerDetected)
{
    UnknownDialogueTriggers.TriggerNPCEncountered(npcGameObject);
}

// When NPC becomes alert
if (alertLevel >= AlertThreshold.High)
{
    UnknownDialogueTriggers.TriggerDangerDetected(npcGameObject);
}
```

## Settings Integration

### Gameplay Settings

The system integrates with SettingsManager:

```csharp
// Get hint frequency (0-1)
float frequency = SettingsManager.Instance.GetUnknownHintFrequency();

// Set hint frequency
SettingsManager.Instance.SetUnknownHintFrequency(0.75f);

// Get personality (0=Verbose, 1=Balanced, 2=Cryptic)
int personality = SettingsManager.Instance.GetUnknownPersonality();

// Enable/disable messages
SettingsManager.Instance.SetUnknownMessagesEnabled(true);
```

### Accessibility Features

- **Subtitle Support**: All messages include text
- **Text Scaling**: Uses UI scale from accessibility settings
- **Glitch Intensity**: Reduced for motion sensitivity
- **Audio Control**: Message sounds respect volume settings
- **Color Options**: High contrast mode compatible

## Performance Targets

### Achieved Metrics

- Message selection: **<1ms** per trigger ✓
- UI update: **<2ms** when new message arrives ✓
- Phone UI render: **<3ms** per frame ✓
- Text rendering: **<2ms** (with effects) ✓
- Memory: Message database **<2MB** ✓
- No frame stuttering from message triggers ✓

### Optimization Techniques

1. **Efficient Filtering**: Early-exit conditions
2. **Cached Components**: UI elements cached on Awake
3. **Pooling**: Message UI elements reused
4. **Lazy Evaluation**: Messages selected only when needed
5. **Coroutine Management**: Proper cleanup of display coroutines

## Message Content Guidelines

### Writing Style

**DO:**
- Use professional, cryptic tone
- Keep messages <200 characters
- Use proper grammar and punctuation
- Make messages contextually relevant
- Maintain mysterious atmosphere
- Vary vocabulary across messages

**DON'T:**
- Use system-like language ("Error", "Loading", etc.)
- Break immersion with game terms
- Be too obvious or direct
- Use casual internet slang
- Reference the entity as "AI" or "narrator"
- Overuse exclamation points

### Example Messages

**Good:**
- "Proceed with caution."
- "Your survival instincts are commendable."
- "That was... unexpected."
- "Few discover this place."
- "The pieces align."

**Bad:**
- "Error: Puzzle not solved yet." (Too system-like)
- "Welcome to the game!" (Breaks immersion)
- "I am an AI helping you." (Too direct)
- "LOL nice dodge" (Too casual)

## Testing & Demo

### Demo Controller

Use `UnknownDialogueDemoController` for testing:

**Keyboard Shortcuts:**
- F6: Toggle demo panel
- 1: Test combat messages
- 2: Test puzzle messages
- 3: Test exploration messages
- 4: Test mission messages
- 5: Test narrative messages

### Testing Checklist

- [ ] Messages trigger correctly for each event type
- [ ] Phone UI opens/closes with 'C' key
- [ ] HUD overlay displays with glitch effects
- [ ] Typing animation works correctly
- [ ] Message history persists
- [ ] Cooldown system prevents spam
- [ ] Personality modes affect selection
- [ ] Hint frequency slider works
- [ ] Messages adapt to difficulty
- [ ] Messages adapt to game stage
- [ ] Performance targets met (60 FPS maintained)
- [ ] Audio plays correctly
- [ ] Accessibility settings respected

## Creating the Message Database

### Editor Menu

Use the Unity menu to create the database:

```
Protocol EMR > Dialogue > Create Message Database
```

This generates:
- `Assets/Resources/Dialogue/UnknownMessageDatabase.asset`
- Pre-populated with 90+ messages
- Organized by category and trigger
- Ready for customization

### Adding New Messages

1. Open the database asset in Unity Inspector
2. Expand the "Messages" list
3. Add new message element
4. Fill in properties:
   - Text: Your message content
   - Category: Choose appropriate category
   - Trigger: Select trigger event
   - Difficulty: Set min/max range
   - Game Stage: 0 (early), 1 (mid), 2 (late)
   - Display settings: Mode, delay, duration
   - Cooldown: Seconds before repeat

### Message Variants

For variety, create 3-5 variants per trigger:
- Same trigger, different text
- Vary by difficulty or game stage
- Different selection weights for rarity

## Future Enhancements

### Planned Features

- **Voice-Over Support**: Audio playback for messages
- **Voice Processing**: Synthetic/distorted effect
- **Player Response System**: YES/NO dialogue choices
- **Message Templates**: Dynamic text insertion (player name, location)
- **Procedural Message Generation**: AI-generated contextual messages
- **Emotion System**: Unknown's emotional state affects tone
- **Relationship Meter**: Player actions affect Unknown's attitude
- **Secret Messages**: Hidden messages for specific actions

### Integration Roadmap

- **Sprint 9**: Procedural story integration
- **Sprint 10**: Performance optimization
- **Future**: Full voice acting implementation

## Troubleshooting

### Common Issues

**Messages not appearing:**
- Check hint frequency setting (may be set to 0%)
- Verify UnknownDialogueManager exists in scene
- Ensure message database is loaded in Resources/Dialogue
- Check if messages are on cooldown

**Phone UI not opening:**
- Verify InputManager is set up correctly
- Check if 'C' key is remapped
- Ensure UnknownPhoneUI component exists
- Check if phone panel GameObject is active

**HUD overlay not showing:**
- Verify UnknownHUDOverlay component exists
- Check display mode (may be set to Phone only)
- Ensure Canvas is properly configured
- Check if glitch effects are disabled

**Performance issues:**
- Reduce max history size in database
- Disable glitch effects in accessibility
- Check for excessive message triggers
- Profile with Unity Profiler

## API Reference

### UnknownDialogueManager

```csharp
// Singleton instance
UnknownDialogueManager.Instance

// Properties
float HintFrequency { get; set; }
int GameStage { get; set; }
int DifficultyLevel { get; set; }
UnknownPersonality Personality { get; set; }

// Methods
void TriggerMessage(MessageTrigger trigger, GameObject source = null)
void TriggerMessage(UnknownMessageEvent messageEvent)
List<MessageHistory> GetMessageHistory()
void ClearMessageHistory()
void SetHintFrequency(float frequency)
PlayerStyleProfile GetPlayerStyleProfile()

// Events
event Action<UnknownMessage> OnMessageSelected
event Action<UnknownMessage, MessageDisplayMode> OnMessageDisplay
```

### UnknownDialogueTriggers

Static helper class with trigger methods:

```csharp
// Combat
TriggerPlayerHitNPC(GameObject npc, float damage)
TriggerPlayerTookDamage(GameObject attacker, float damage, float currentHP, float maxHP)
TriggerNPCDefeated(GameObject npc)
TriggerDodgeSuccessful()
TriggerCombatStarted(GameObject enemy)

// Puzzle
TriggerPuzzleEncountered(GameObject puzzle)
TriggerPuzzleAttemptFailed(GameObject puzzle)
TriggerPuzzleSolved(GameObject puzzle, bool perfect = false)
TriggerPlayerStuck()

// Exploration
TriggerNewAreaDiscovered(string areaName)
TriggerSecretFound(GameObject secret)
TriggerNPCEncountered(GameObject npc)
TriggerItemFound(GameObject item)
TriggerDangerDetected(GameObject danger)

// Mission
TriggerMissionStart(string missionName)
TriggerMissionMilestone(string milestoneName)
TriggerMissionComplete(string missionName)
TriggerObjectiveFailed(string objectiveName)
TriggerNewMissionAvailable(string missionName)

// Narrative
TriggerPlotPointReached(string plotPoint)
TriggerProceduralStoryMilestone(string milestone)
TriggerMajorEventOccurred(string eventName)
TriggerSecretDiscovered(string secretName)

// Player Behavior
TriggerStealthApproach()
TriggerAggressiveApproach()
TriggerDocumentRead(string documentName)

// Special
TriggerGameStart()
TriggerPlayerDeath()
TriggerCustomMessage(string context)
```

## Credits

**System Design**: Protocol EMR Development Team
**Implementation**: Sprint 8 - NPC & Communication Phase
**Message Writing**: Curated for immersive mysterious atmosphere
**Integration**: Compatible with all previous sprint systems

## Version History

**v1.0.0** - Sprint 8 Initial Release
- Complete Unknown dialogue system
- 90+ pre-written messages
- Phone and HUD display modes
- Personalization and adaptation
- Full integration with existing systems
- Performance targets achieved

---

For questions or issues, refer to the main Protocol EMR documentation or the Sprint 8 summary document.
