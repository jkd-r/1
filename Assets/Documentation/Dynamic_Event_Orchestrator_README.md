# Dynamic Event Orchestrator - System Documentation

## Overview

The **Dynamic Event Orchestrator** is a procedural event scheduling system that reacts to world state, spawns encounters, and feeds contextual triggers into the Unknown dialogue system. It bridges the gap between procedural generation and narrative delivery, ensuring a living, responsive game world.

## Core Components

### DynamicEventOrchestrator.cs

Central manager that:
- Scans world state every `eventCheckInterval` (default: 5 seconds)
- Schedules ambient/combat/puzzle events deterministically using SeedManager
- Integrates with DifficultyManager and NPCManager for scaled encounters
- Maintains <1ms per-frame performance budget
- Queues dialogue triggers to prevent spam

**Key Properties:**
- `enableOrchestration` - Master toggle for event scheduling
- `eventCheckInterval` - How often to evaluate new events (seconds)
- `minTimeBetweenEvents` - Global event cooldown (seconds)
- `maxSchedulingTimeMs` - Performance budget warning threshold

### WorldStateBlackboard.cs

Lightweight data store tracking:
- **Chunk State**: Current chunk ID, position, visit history
- **Threat Levels**: Per-chunk threat values (0-1), global average
- **Player Style**: Stealth/combat/exploration/puzzle ratios
- **Mission State**: Flags, current phase, game progress (0-1)

**API Examples:**
```csharp
// Update chunk
DynamicEventOrchestrator.Instance.SetChunk(chunkId, position);

// Set threat level
DynamicEventOrchestrator.Instance.SetThreatLevel(chunkId, 0.7f);

// Record player actions
DynamicEventOrchestrator.Instance.RecordPlayerAction(PlayerActionType.Combat);

// Mission flags
DynamicEventOrchestrator.Instance.SetMissionFlag("tutorial_complete", true);
DynamicEventOrchestrator.Instance.SetMissionPhase(2);
DynamicEventOrchestrator.Instance.SetGameProgress(0.45f);
```

### ProceduralEventProfile.cs

ScriptableObject configuration for event types:

**Event Properties:**
- `eventId` - Unique identifier
- `eventName` - Display name
- `eventType` - Ambient, Combat, Puzzle, Story, Discovery, Ambush
- `minDifficultyLevel` / `maxDifficultyLevel` - Difficulty range (0-3)
- `minGameProgress` / `maxGameProgress` - Progress range (0-1)
- `minThreatLevel` - Minimum chunk threat required
- `spawnWeight` - Selection probability weight

**Timing:**
- `eventCooldown` - Cooldown before event can repeat (seconds)
- `chunkEventCooldown` - Minimum time between ANY events in chunk (seconds)
- `eventDuration` - How long the event lasts (0 = indefinite)

**Mission Requirements:**
- `requiredMissionFlags` - Flags that must be set
- `forbiddenMissionFlags` - Flags that must NOT be set
- `requiredMissionPhase` - Specific phase required (-1 = any)

**Dialogue Integration:**
- `triggerDialogueOnStart` - Fire Unknown message when event starts
- `triggerDialogueOnEnd` - Fire Unknown message when event ends
- `dialogueTags` - Custom tags for context enrichment

**Event Parameters:**
- `npcSpawnCount` - Number of NPCs for combat events
- `puzzleComplexity` - Difficulty tier for puzzle events (1-5)
- `ambientIntensity` - Effect strength for ambient events (0-1)

**Repeatability:**
- `canRepeat` - Can event occur multiple times
- `maxOccurrences` - Maximum occurrences (-1 = unlimited)

## Integration with Unknown Dialogue

### New Trigger Types

Added to `UnknownMessageData.cs`:
- `DynamicEventAmbient`
- `DynamicEventCombat`
- `DynamicEventPuzzle`
- `DynamicEventStarted`
- `DynamicEventResolved`
- `DynamicEventMilestone`
- `DynamicEventFailed`

### New Trigger Methods

Added to `UnknownDialogueTriggers.cs`:

```csharp
// Called when event starts
UnknownDialogueTriggers.TriggerDynamicEventStarted(
    eventId: "event_001_12345",
    eventType: "Combat",
    chunkId: 5,
    threatLevel: 0.8f,
    additionalContext: new Dictionary<string, object> {
        { "stealthRatio", 0.3f },
        { "combatRatio", 0.7f }
    }
);

// Called when event completes
UnknownDialogueTriggers.TriggerDynamicEventResolved(
    eventId: "event_001_12345",
    success: true,
    duration: 45.5f,
    additionalContext: contextData
);

// Called on progress milestones
UnknownDialogueTriggers.TriggerDynamicEventMilestone(
    eventId: "event_001_12345",
    milestoneName: "Wave 2 Complete",
    progress: 66,
    additionalContext: contextData
);

// Batch multiple events (uses priority: Combat > Puzzle > Ambient)
UnknownDialogueTriggers.TriggerDynamicEventBatch(eventList);
```

### Context Hydration

Every orchestrator callback automatically injects:
- `eventId` - Unique event instance ID
- `eventType` - Type of event (Ambient/Combat/Puzzle/etc)
- `chunkId` - World chunk identifier
- `threatLevel` - Current chunk threat (0-1)
- `gameProgress` - Overall game completion (0-1)
- `stealthRatio` - Player stealth tendency (0-1)
- `combatRatio` - Player combat tendency (0-1)
- `explorationRatio` - Player exploration tendency (0-1)
- `puzzleRatio` - Player puzzle-solving tendency (0-1)
- Custom tags from `ProceduralEventProfile.dialogueTags`

### Spam Prevention

1. **Frame-Based Queue**: Dialogue triggers queued and processed once per frame
2. **Global Cooldown**: Unknown's `globalMessageCooldown` still applies
3. **Event Cooldowns**: Per-event and per-chunk cooldowns from profiles
4. **Batch Prioritization**: `TriggerDynamicEventBatch()` selects highest-priority message

## Deterministic Seeding

The orchestrator uses `SeedManager` with scope `"dynamic_events"`:

```csharp
// Deterministic event selection
int seed = SeedManager.Instance.GetSeed(SCOPE_EVENTS, chunkId);
var rng = new System.Random(seed);

// Weighted selection uses scope offset advancement
SeedManager.Instance.AdvanceScopeOffset(SCOPE_EVENTS, 1);
float randomValue = SeedManager.Instance.GetRandomFloat(SCOPE_EVENTS, 0);
```

Same seed = same event schedule per chunk.

## Performance Targets

- **Scheduling**: <1ms per frame
- **Event Selection**: <0.5ms per check
- **Dialogue Queuing**: <0.1ms per event
- **Memory**: <5MB for orchestrator + world state
- **Active Events**: Support 10+ concurrent events

## Debug Tools

### Performance Monitor (F1)
Shows active event count and F9 hint for detailed view.

### Orchestrator Debug HUD (F9)
Displays:
- Orchestration status (ON/OFF)
- Active event count
- Event cooldown count
- Pending dialogue queue size
- World state summary (chunk, threat, game progress)
- Active event details (name, type, time, progress)

### Demo Controller (F10)

`DynamicEventOrchestratorDemo.cs` provides:
- **1**: Spawn combat event (high threat)
- **2**: Spawn puzzle event
- **3**: Spawn ambient event
- **T**: Cycle threat level (0% → 25% → 50% → 75% → 100%)
- **C**: Change chunk
- **E**: Complete active event
- **F10**: Toggle demo UI

### Gizmos Visualization

When `visualizeUpcomingEvents` enabled:
- Red sphere: Combat event
- Blue sphere: Puzzle event
- Green sphere: Ambient event
- Yellow sphere: Other event types

## Creating Event Profiles

1. **Create Asset**: Right-click → Create → Protocol EMR → Procedural → Event Profile
2. **Configure Identity**:
   - Set unique `eventId`
   - Choose `eventType` (Ambient/Combat/Puzzle/etc)
   - Assign descriptive `eventName`
3. **Set Spawn Rules**:
   - Define difficulty range (0-3)
   - Set game progress window (0-1)
   - Set minimum threat level
   - Adjust spawn weight (higher = more likely)
4. **Configure Timing**:
   - Set event cooldown (how often it can repeat)
   - Set chunk cooldown (delay before other events in same chunk)
   - Set event duration (0 = manual completion)
5. **Define Requirements**:
   - Add required mission flags
   - Add forbidden mission flags
   - Set required mission phase (-1 = any)
6. **Dialogue Setup**:
   - Enable `triggerDialogueOnStart` / `triggerDialogueOnEnd`
   - Add custom `dialogueTags` for context
7. **Event Parameters**:
   - Combat: Set `npcSpawnCount`
   - Puzzle: Set `puzzleComplexity` (1-5)
   - Ambient: Set `ambientIntensity` (0-1)
8. **Place in Resources**:
   - Save to `Assets/Resources/Events/` folder for auto-loading

## Example Event Profiles

### Ambient Discovery Event
```
eventId: "ambient_discovery_001"
eventType: Ambient
minDifficultyLevel: 0
maxDifficultyLevel: 3
minGameProgress: 0.0
maxGameProgress: 1.0
minThreatLevel: 0.0
spawnWeight: 2.0
eventCooldown: 180s
chunkEventCooldown: 30s
eventDuration: 0 (indefinite)
triggerDialogueOnStart: true
dialogueTags: ["discovery", "exploration"]
ambientIntensity: 0.5
```

### Combat Wave Event
```
eventId: "combat_wave_001"
eventType: Combat
minDifficultyLevel: 1
maxDifficultyLevel: 3
minGameProgress: 0.2
maxGameProgress: 1.0
minThreatLevel: 0.6
spawnWeight: 1.5
eventCooldown: 300s
chunkEventCooldown: 60s
eventDuration: 120s
triggerDialogueOnStart: true
triggerDialogueOnEnd: true
dialogueTags: ["combat", "wave"]
npcSpawnCount: 5
requiredMissionFlags: ["combat_tutorial_complete"]
```

### Logic Puzzle Event
```
eventId: "puzzle_encryption_001"
eventType: Puzzle
minDifficultyLevel: 0
maxDifficultyLevel: 2
minGameProgress: 0.1
maxGameProgress: 0.7
minThreatLevel: 0.0
spawnWeight: 1.0
eventCooldown: 240s
chunkEventCooldown: 45s
eventDuration: 0 (player-controlled)
triggerDialogueOnStart: true
triggerDialogueOnEnd: true
dialogueTags: ["puzzle", "logic"]
puzzleComplexity: 3
canRepeat: true
maxOccurrences: 3
```

## QA Workflow

### Basic Acceptance Testing

1. **Launch Game**: Verify orchestrator initializes (check console)
2. **Open Performance Monitor** (F1): Confirm "Dynamic Events" section appears
3. **Toggle Orchestrator HUD** (F9): Verify world state displays correctly
4. **Change Chunks** (C or walk around): Check chunk ID updates
5. **Adjust Threat** (T): Cycle through threat levels, confirm UI updates
6. **Trigger Events**:
   - **Combat** (1): High threat → watch for combat dialogue
   - **Puzzle** (2): Medium threat → watch for puzzle dialogue
   - **Ambient** (3): Low threat → watch for ambient dialogue
7. **Verify Dialogue**: Open Unknown phone (F6 if integrated) → confirm messages match event types
8. **Check Context**: Enable dialogue logging → verify chunk ID, threat level, player style ratios
9. **Complete Events** (E): End active events → verify completion dialogue triggers
10. **Test Cooldowns**: Spam event spawns → confirm cooldowns prevent immediate re-spawn

### Deterministic Testing

1. **Note Current Seed** (F8 to copy)
2. **Trigger Event Sequence**: Record which events spawned in which chunks
3. **Restart Game**: Load same seed via Settings → Procedural Seed
4. **Replay Actions**: Navigate same chunks, adjust same threat levels
5. **Verify Match**: Events should spawn in same order at same chunks

### Spam Prevention Testing

1. **Force Multiple Events**: Spawn combat + puzzle + ambient in same frame
2. **Check Dialogue**: Only ONE Unknown message should appear (highest priority = combat)
3. **Monitor Frame Time**: Performance HUD should show no spikes
4. **Check Global Cooldown**: Next message shouldn't appear until global cooldown expires

## Known Limitations

1. **NPC Spawning**: Currently placeholder logic (requires full NPCManager integration)
2. **Puzzle Spawning**: Placeholder (requires puzzle system from future sprint)
3. **Ambient Effects**: Placeholder (requires audio/VFX systems)
4. **Save/Load**: Event state not yet persisted (future sprint)
5. **Multiplayer**: Not designed for multiplayer scenarios

## Future Enhancements (Planned)

- Full NPC spawn integration with faction/type configuration
- Puzzle prefab instantiation and configuration
- Audio/VFX ambient effect spawning
- Save/load event state persistence
- Event chain system (sequences, dependencies)
- Dynamic difficulty adjustment based on event outcomes
- Analytics tracking for event success/failure rates
- A/B testing support for event profiles

## Troubleshooting

**No Events Spawning:**
- Check `enableOrchestration` is true
- Verify event profiles exist in `Resources/Events/` or assigned in inspector
- Check difficulty/progress/threat requirements on profiles
- Confirm no cooldowns are active (check orchestrator HUD)

**Performance Issues:**
- Enable `enablePerformanceLogging` to measure scheduling time
- Reduce `eventCheckInterval` to check less frequently
- Lower number of active event profiles
- Optimize event selection filters

**Dialogue Not Triggering:**
- Verify UnknownDialogueManager is in scene
- Check `triggerDialogueOnStart` / `triggerDialogueOnEnd` enabled on profile
- Confirm message database has messages for new trigger types
- Check global dialogue cooldown hasn't blocked trigger

**Events Spawning Too Often:**
- Increase `eventCooldown` on profiles
- Increase `chunkEventCooldown` on profiles
- Increase `minTimeBetweenEvents` on orchestrator
- Lower spawn weights on profiles

---

**Version**: Sprint 9 Initial Release  
**Dependencies**: SeedManager, DifficultyManager, NPCManager, UnknownDialogueManager  
**Performance**: <1ms scheduling, <5MB memory  
**Integration Level**: Procedural + Dialogue + AI  

For questions or issues, refer to the main Protocol EMR documentation or Sprint 9 summary.
