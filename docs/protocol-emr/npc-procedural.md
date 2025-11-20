# NPC Framework and Procedural Generation Strategy

## Overview

This document defines the NPC (Non-Player Character) framework and procedural generation systems for Protocol EMR. These systems work together to create dynamic, reactive environments with intelligent NPCs that respond contextually to player actions while maintaining narrative coherence through procedurally varied spaces.

## Table of Contents

1. [NPC Framework](#npc-framework)
2. [Procedural Generation Strategy](#procedural-generation-strategy)
3. [Technical Systems Required](#technical-systems-required)
4. [Integration Points](#integration-points)
5. [Prototype Deliverables](#prototype-deliverables)
6. [Technical Implementation Guide](#technical-implementation-guide)
7. [QA and Testing Checklist](#qa-and-testing-checklist)

## NPC Framework

### Spawning Rules

The NPC spawning system dynamically manages when and where NPCs appear in the game world based on multiple factors including player progression, narrative state, and environmental context.

#### Spawn Triggers

| Trigger Type | Description | Priority | Example Use Case |
|--------------|-------------|----------|------------------|
| **Mission-Based** | NPCs spawn when specific mission state is reached | High | Security guards appear after alarm triggered |
| **Time-Based** | NPCs spawn at specific game time intervals | Medium | Patrol cycles, shift changes |
| **Zone-Based** | NPCs spawn when player enters specific zones | High | Ambush scenarios, area guardians |
| **Event-Driven** | NPCs spawn in response to world events | High | Reinforcements during combat |
| **Proximity** | NPCs spawn near player based on distance thresholds | Medium | Dynamic population density |
| **Random** | NPCs spawn with probability checks at intervals | Low | Ambient population, background characters |

#### Spawn Locations

```javascript
// Spawn volume configuration
SpawnVolume {
  volumeId: string,
  zoneType: enum [Safe, Neutral, Hostile, Restricted],
  maxNPCs: int,
  respawnCooldown: float, // seconds
  spawnWeights: {
    friendly: float,
    neutral: float,
    hostile: float
  },
  blockedByPlayerProximity: boolean,
  minDistanceFromPlayer: float // meters
}
```

**Spawn Location Types**:
- **Spawn Volumes**: 3D bounding volumes with density parameters
- **Spawn Points**: Precise locations for scripted encounters
- **Nav Mesh Regions**: Random spawn anywhere within navigable areas
- **Off-Camera Spawn**: Dynamically spawn NPCs outside player view cone

#### Spawn Rules and Constraints

1. **Line of Sight Check**: Never spawn NPCs within direct player line of sight
2. **Proximity Buffer**: Maintain minimum distance (default: 10 meters) from player
3. **Capacity Limits**: Maximum simultaneous NPCs per zone (default: 20)
4. **Performance Budgeting**: Dynamic spawn throttling based on frame rate
5. **Narrative Consistency**: Check mission state before spawning certain NPC types
6. **Cooldown Periods**: Prevent rapid re-spawning in same location (default: 60 seconds)

#### Despawn Conditions

- Player exits zone beyond despawn radius (default: 50 meters)
- NPC reaches designated safe extraction point
- Mission state changes invalidate NPC presence
- Performance optimization: despawn low-priority distant NPCs
- Time-based: NPCs on patrol routes despawn after completing cycle

### Behavior Trees

NPCs use hierarchical behavior trees to make decisions and respond to environmental stimuli. The behavior system distinguishes between friendly and hostile NPCs with shared foundational nodes.

#### Core Behavior Tree Structure

```
Root (Selector)
├── Check Mission Override (Decorator)
├── Respond To Immediate Threats (Sequence)
├── Execute Current Task (Selector)
│   ├── Combat Behavior
│   ├── Patrol Behavior
│   ├── Dialogue Behavior
│   ├── Investigation Behavior
│   └── Idle Behavior
└── Default Fallback (Idle)
```

#### Friendly NPC Behaviors

**Behavior Priorities** (highest to lowest):
1. **Mission Critical Interaction**: High-priority dialogue or assistance
2. **Player Assistance**: Respond to player requests, provide hints
3. **Contextual Reactions**: React to player achievements, failures, or discoveries
4. **Social Interaction**: Greet player, casual conversation
5. **Routine Activities**: Patrol routes, maintenance tasks, ambient animations

**Friendly Behavior Tree Example**:
```
FriendlyNPC (Selector)
├── Mission Critical Dialogue (Decorator: CheckMissionState)
│   └── Sequence
│       ├── Approach Player
│       ├── Play Dialogue
│       └── Update Mission State
├── Respond To Player Interaction (Decorator: PlayerInteracted)
│   └── Sequence
│       ├── Face Player
│       ├── Trigger Contextual Dialogue
│       └── Provide Assistance/Information
├── Patrol Route (Decorator: HasPatrolRoute)
│   └── Sequence
│       ├── Navigate To Next Waypoint
│       ├── Wait At Waypoint
│       └── Play Ambient Animation
└── Idle Behavior
    └── Sequence
        ├── Play Idle Animation (Variations)
        └── Occasional Look Around
```

**Friendly NPC Parameters**:
- **Greeting Radius**: 5 meters (triggers greeting behavior)
- **Dialogue Timeout**: 30 seconds (returns to routine after inactivity)
- **Assistance Priority**: High (interrupts patrol for player help)
- **Aggro Potential**: None (never hostile to player)

#### Hostile NPC Behaviors

**Behavior Priorities** (highest to lowest):
1. **Combat Engagement**: Attack detected threats
2. **Investigation**: Investigate suspicious sounds, events, or locations
3. **Alert Reinforcement**: Call for backup when threatened
4. **Patrol/Guard**: Maintain security of designated zones
5. **Idle/Ambient**: Stand guard, ambient animations

**Hostile Behavior Tree Example**:
```
HostileNPC (Selector)
├── Combat Mode (Decorator: ThreatDetected)
│   └── Sequence
│       ├── Alert Nearby NPCs
│       ├── Move To Combat Position (Cover, Flank)
│       ├── Attack Target (Ranged or Melee)
│       └── Evaluate Threat (Re-assess combat state)
├── Investigate Suspicious Event (Decorator: SuspicionLevel > Threshold)
│   └── Sequence
│       ├── Navigate To Last Known Threat Position
│       ├── Search Area (Widening Circle)
│       ├── Call Out (Audio Cue)
│       └── Return To Patrol (If nothing found)
├── Alerted State (Decorator: HeardNoise || SawPlayer)
│   └── Sequence
│       ├── Face Direction Of Interest
│       ├── Play Alert Animation
│       ├── Increase Awareness Level
│       └── Transition To Investigation
├── Patrol Route (Decorator: HasPatrolRoute && NotAlerted)
│   └── Sequence
│       ├── Navigate To Next Waypoint
│       ├── Look Around (Check Perception)
│       └── Continue Patrol
└── Idle Guard
    └── Sequence
        ├── Play Guard Idle Animation
        └── Periodic Perception Check
```

**Hostile NPC Parameters**:
- **Detection Range**: 15-30 meters (varies by NPC type and difficulty)
- **Combat Range**: 5-20 meters (melee vs ranged)
- **Alert Propagation Radius**: 25 meters (nearby NPCs alerted)
- **Suspicion Threshold**: 0-100 scale (triggers investigation at 50)
- **Search Duration**: 20-40 seconds (before returning to patrol)
- **Combat Aggression**: Low/Medium/High (affects decision-making)

#### Neutral NPC Behaviors

**Neutral NPCs** can transition between friendly and hostile states based on player actions:

```
NeutralNPC (Selector)
├── Evaluate Player Reputation (Decorator: CheckReputation)
│   └── Switch Behavior Tree (Friendly or Hostile)
├── Flee From Danger (Decorator: HealthLow || CombatNearby)
│   └── Navigate To Safe Zone
├── React To Player Actions (Decorator: PlayerActionDetected)
│   └── Adjust Reputation
└── Ambient Activity (Default)
```

### Perception System

The perception system governs how NPCs detect and respond to stimuli in the game world, including the player, sounds, and environmental changes.

#### Perception Components

| Component | Description | Update Frequency | Cost |
|-----------|-------------|------------------|------|
| **Vision** | Line-of-sight detection within view cone | 10-15 Hz (per NPC) | High |
| **Hearing** | Detection of sounds within audible radius | 5-10 Hz (per NPC) | Medium |
| **Touch** | Immediate detection of physical contact | Event-driven | Low |
| **Memory** | Recall of previously detected threats/entities | 1 Hz (per NPC) | Low |

#### Vision System

**Field of View (FOV) Configuration**:
```javascript
VisionConfig {
  viewDistance: float, // Max detection distance (meters)
  viewAngle: float, // Field of view angle (degrees)
  peripheralAngle: float, // Reduced detection angle (degrees)
  loseTargetTime: float, // Duration before losing tracked target (seconds)
  occlusionCheckInterval: float // Frequency of line-of-sight checks (Hz)
}
```

**Detection States**:
1. **Unaware**: No target detected (default state)
2. **Noticed**: Target in peripheral vision (partial detection)
3. **Seen**: Target in clear view cone (full detection)
4. **Tracked**: Active targeting of previously seen entity
5. **Lost**: Target lost from view (memory-based search)

**Vision Parameters by NPC Type**:

| NPC Type | View Distance | View Angle | Peripheral | Lose Target Time |
|----------|---------------|------------|------------|------------------|
| **Guard (Alert)** | 30m | 90° | 150° | 10s |
| **Guard (Relaxed)** | 20m | 70° | 120° | 5s |
| **Civilian** | 15m | 90° | 180° | 2s |
| **Elite Enemy** | 40m | 100° | 160° | 15s |

**Line of Sight Optimization**:
- Raycast with layer masks (ignore non-occluders)
- Staggered update intervals (not all NPCs check simultaneously)
- Distance-based LOD: reduce check frequency for distant NPCs
- View frustum culling: disable vision checks for off-screen NPCs

#### Hearing System

**Sound Propagation Model**:
```javascript
SoundStimulus {
  sourceLocation: Vector3,
  intensity: float, // 0-1 (loudness)
  radius: float, // Max audible distance
  soundType: enum [Footstep, Gunshot, Dialogue, Environment],
  priority: int // Higher priority interrupts lower
}
```

**Hearing Ranges by Sound Type**:

| Sound Type | Base Radius | Intensity Multiplier | Alert Level |
|------------|-------------|---------------------|-------------|
| **Footstep (Walk)** | 5m | 1.0x | Low |
| **Footstep (Run)** | 10m | 2.0x | Medium |
| **Gunshot** | 50m | 5.0x | High |
| **Explosion** | 100m | 10.0x | Critical |
| **Door/Object** | 8m | 1.5x | Low-Medium |
| **Dialogue** | 12m | 1.2x | Low |

**Hearing Behavior**:
- NPCs turn toward sound source location
- Suspicion level increases based on sound intensity and type
- Multiple sounds in quick succession compound suspicion
- Hearing checks occur continuously (event-driven system)

#### Memory System

NPCs retain short-term memory of detected stimuli to create intelligent search patterns:

```javascript
PerceptionMemory {
  lastKnownPlayerPosition: Vector3,
  timeSinceLastSeen: float,
  suspicionLevel: float, // 0-100
  suspicionDecayRate: float, // Per second
  searchRadius: float, // Expands over time
  investigationPoints: Array<Vector3> // Locations to check
}
```

**Memory Decay**:
- Suspicion decreases over time when no new stimuli detected
- Decay rate: 5-10 points per second (configurable)
- Full reset after 30-60 seconds of no activity
- Elite NPCs have slower decay rates (more persistent)

### Dialogue System Integration

NPCs interface with the dialogue system to deliver contextual conversations and information to the player.

#### Dialogue Triggers

| Trigger Type | Description | Example |
|--------------|-------------|---------|
| **Proximity** | Player enters dialogue radius | NPC greets player within 3 meters |
| **Interaction** | Player explicitly interacts with NPC | Press 'E' to talk |
| **Mission** | Dialogue triggered by mission state | NPC offers quest upon approach |
| **Event** | World event triggers dialogue | NPC reacts to explosion nearby |
| **Timed** | Dialogue occurs after time threshold | NPC speaks after 10s of player presence |

#### Dialogue Flow Architecture

```
DialogueNode {
  nodeId: string,
  speakerId: string, // NPC identifier
  dialogueText: string,
  voiceOverClip: AudioClip, // Optional VO
  displayDuration: float, // Auto-continue after duration (optional)
  choices: Array<DialogueChoice>, // Player response options
  conditions: Array<Condition>, // Requirements to show this node
  consequences: Array<Action> // Effects of reaching this node
}

DialogueChoice {
  choiceText: string,
  nextNodeId: string,
  requirements: Array<Condition>, // Show only if conditions met
  effects: Array<Action> // Immediate effects of choosing this option
}
```

#### Contextual Dialogue

NPCs select dialogue lines dynamically based on current context:

**Context Variables**:
- **Mission State**: Current active missions and progress
- **Player Reputation**: Relationship level with NPC or faction
- **Recent Events**: Player actions in past 5 minutes
- **Time of Day**: Morning, afternoon, evening, night
- **Location**: Current zone or environment type
- **Player Equipment**: Visible weapons, tools, or items
- **NPC State**: Health, mood, awareness level

**Example Contextual Dialogue Selection**:
```python
def select_dialogue_line(npc, player, context):
    if context.mission_active("rescue_operation"):
        return npc.get_dialogue("mission_rescue_active")
    elif context.player_reputation < 0:
        return npc.get_dialogue("hostile_greeting")
    elif context.recent_event("player_helped_civilian"):
        return npc.get_dialogue("grateful_thanks")
    elif context.time_of_day == "night":
        return npc.get_dialogue("night_greeting")
    else:
        return npc.get_dialogue("default_greeting")
```

### Contextual Reactions to Player Actions

NPCs dynamically respond to player behavior with appropriate animations, dialogue, and state changes.

#### Reaction Categories

| Player Action | NPC Reaction Type | Behavioral Response | Audio/Visual Feedback |
|---------------|-------------------|---------------------|----------------------|
| **Draw Weapon** | Alert/Defensive | Raise hands, back away, or draw weapon | "Whoa! Easy there!" |
| **Sprint Near NPC** | Startled | Flinch, step aside | Gasp sound, eye tracking |
| **Damage Nearby Object** | Suspicious | Investigate, increase suspicion | "What was that?!" |
| **Help Civilian** | Positive | Thank player, increase reputation | "Thank you so much!" |
| **Complete Quest** | Celebratory | Congratulate, offer reward | Applause, happy animation |
| **Stealth Approach** | Unaware (if successful) | Continue routine | None (if undetected) |
| **Failed Stealth** | Alert | Turn, investigate, call out | "Who's there?!" |
| **Attack Hostile** | Combat | Engage combat behavior | Battle cries, alert nearby |

#### Reaction Parameters

```javascript
ReactionConfig {
  triggerRadius: float, // Distance at which reaction occurs
  reactionDelay: float, // Seconds before reacting (0.1-0.5s for realism)
  reactionDuration: float, // How long reaction animation/state lasts
  interruptPriority: int, // Can interrupt current behavior?
  propagateToNearby: boolean, // Affect nearby NPCs?
  consequenceType: enum [None, ReputationChange, MissionTrigger, Combat]
}
```

#### Emotional State System

NPCs track emotional states that influence reactions:

**Emotion States**: Calm, Alert, Fearful, Angry, Grateful, Suspicious

```javascript
EmotionState {
  currentEmotion: enum,
  intensity: float, // 0-1
  transitionSpeed: float, // How quickly emotions change
  baselineEmotion: enum // Default state to return to
}
```

Emotional states modify:
- Dialogue tone and line selection
- Animation speed and intensity
- Decision-making thresholds (flee vs fight)
- Perception sensitivity (fearful NPCs more easily startled)

### Integration with Mission Triggers and Narrator Events

NPCs serve as critical integration points between gameplay, missions, and narrative systems.

#### Mission Integration Patterns

**1. Mission Giver NPCs**:
```javascript
MissionGiverNPC {
  availableMissions: Array<MissionId>,
  prerequisiteChecks: Array<Condition>,
  missionStartDialogue: DialogueTree,
  missionCompleteDialogue: DialogueTree,
  rewardDistribution: Function
}
```

**2. Objective Target NPCs**:
- NPCs that player must reach, rescue, or protect
- State tracking: Alive, Dead, Rescued, Hostile, Converted
- Mission updates on NPC state changes

**3. Dynamic Mission Events**:
```javascript
// NPC spawns as part of mission progression
OnMissionStateChanged("security_breach") {
  SpawnNPC({
    type: "SecurityGuard",
    count: 4,
    behavior: "Hostile",
    location: GetSpawnVolume("main_entrance")
  });
  
  NarratorEvent("Security forces inbound!");
}
```

#### Narrator Event Hooks

NPCs trigger narrator commentary based on their state and actions:

| NPC Event | Narrator Hook | Example Narration |
|-----------|---------------|-------------------|
| NPC Spotted Player | `NPC_DETECTED_PLAYER` | "You've been spotted. Move carefully." |
| NPC Killed | `NPC_ELIMINATED` | "Target neutralized. Remain vigilant." |
| NPC Dialogue Started | `NPC_DIALOGUE_BEGIN` | "This character may have information." |
| NPC Alerted | `NPC_ALERT_TRIGGERED` | "They're searching for you now." |
| NPC Reinforcement | `NPC_BACKUP_CALLED` | "More enemies inbound!" |
| NPC Rescued | `NPC_RESCUED` | "Civilian secured. Well done." |

**Event Broadcasting System**:
```python
class NPCEventBroadcaster:
    def on_npc_state_change(self, npc, old_state, new_state):
        # Broadcast to mission system
        mission_system.notify_npc_event(npc.id, new_state)
        
        # Trigger narrator commentary
        narrator.queue_contextual_line(
            event_type=f"NPC_{new_state}",
            context={"npc_type": npc.type, "location": npc.position}
        )
        
        # Update UI indicators
        ui_system.update_npc_indicator(npc.id, new_state)
```

## Procedural Generation Strategy

The procedural generation system creates varied, replayable environments while maintaining design quality and narrative coherence.

### Room Layout Generation

#### Generation Philosophy

**Seed-Based Determinism**: All procedural generation uses seed values for:
- **Reproducibility**: Same seed generates identical layouts
- **Debugging**: Ability to reproduce specific configurations
- **Sharing**: Players can share interesting seeds
- **Quality Control**: Seed library of tested, quality layouts

#### Layout Generation Algorithm

**Phase 1: Space Partitioning**
```python
def generate_room_layout(seed, min_rooms, max_rooms, total_area):
    random.seed(seed)
    
    # Binary Space Partitioning (BSP)
    root_node = BSPNode(bounds=total_area)
    split_recursive(root_node, min_room_size=10, max_depth=4)
    
    # Convert BSP leaves to rooms
    rooms = []
    for leaf in root_node.get_leaves():
        room = create_room_from_leaf(leaf)
        rooms.append(room)
    
    return rooms
```

**Phase 2: Room Sizing and Spacing**
- Minimum room size: 4x4 meters (functional space)
- Maximum room size: 20x20 meters (performance/design)
- Hallway width: 2-3 meters (allow NPC navigation)
- Clearance between rooms: 1-2 meters (wall thickness)

**Phase 3: Connectivity and Pathfinding**
```python
def create_room_connections(rooms):
    # Use Delaunay triangulation for natural connections
    graph = delaunay_triangulation(room_centers)
    
    # Create minimum spanning tree (ensure all rooms connected)
    mst = minimum_spanning_tree(graph)
    
    # Add additional connections for loops (avoid linear paths)
    add_random_edges(mst, probability=0.3)
    
    # Generate hallways/doors at connection points
    for edge in mst.edges:
        create_hallway(edge.room_a, edge.room_b)
```

#### Room Templates and Archetypes

Pre-designed room templates ensure quality while allowing variation:

| Room Archetype | Description | Size Range | Features |
|----------------|-------------|------------|----------|
| **Hub** | Central connection point | 10x10 - 15x15m | Multiple exits, open layout |
| **Corridor** | Linear connection | 2x8 - 3x20m | Long hallway, few branches |
| **Chamber** | Large specialized room | 15x15 - 20x20m | Quest objectives, bosses |
| **Storage** | Small side room | 4x4 - 6x8m | Loot, supplies, dead ends |
| **Junction** | Multi-path intersection | 6x6 - 8x8m | 3-4 exits, navigation choice |
| **Secure** | Protected area | 8x8 - 12x12m | Locked doors, hostile NPCs |

**Template Variation Parameters**:
```javascript
RoomTemplate {
  archetype: enum,
  baseLayout: GeometryData,
  variationParams: {
    wallMaterial: Array<Material>, // Random selection
    floorPattern: Array<Pattern>,
    propDensity: float, // 0-1 (sparse to dense)
    lightingScheme: enum [Bright, Dim, Dramatic, Minimal],
    clutterLevel: float // Environmental detail density
  },
  spawnPoints: Array<SpawnPoint>,
  furnitureSlots: Array<FurnitureSlot>
}
```

#### Randomization Techniques

**Geometric Variation**:
- Room dimensions: ±20% variance from template base size
- Wall angles: Slight rotations (±5 degrees) for organic feel
- Irregular shapes: Occasional L-shaped or T-shaped rooms
- Height variation: Ceiling heights between 3-6 meters

**Aesthetic Variation**:
- Material randomization: Select from theme-appropriate texture sets
- Prop placement: Furniture and clutter positioned procedurally
- Lighting variation: Color temperature, intensity, and placement
- Damage/wear: Procedural weathering and environmental storytelling

### Puzzle Placement and Difficulty Scaling

#### Puzzle Integration Philosophy

Puzzles are integrated into procedurally generated spaces in ways that:
- Feel naturally placed (not arbitrary)
- Scale difficulty based on progression
- Offer multiple solution paths when appropriate
- Provide consistent reward-to-effort ratios

#### Puzzle Types and Placement Rules

| Puzzle Type | Placement Location | Prerequisites | Difficulty Factors |
|-------------|-------------------|---------------|-------------------|
| **Keycard/Lock** | Secure room entrances | Key item in earlier room | Number of false keys, distance to key |
| **Terminal Hacking** | Chambers, secure rooms | None (skill-based) | Password length, time limit, hints |
| **Environmental** | Junction or chamber | Observation, item use | Obscurity of solution, steps required |
| **Logic/Pattern** | Hub or chamber | Clues scattered in area | Pattern complexity, clue clarity |
| **Timed Challenge** | Corridors, chambers | Alertness, skill | Time pressure, consequences of failure |
| **Stealth Bypass** | Any room with NPCs | Stealth capability | NPC count, patrol patterns, detection ranges |

#### Difficulty Scaling System

```javascript
PuzzleDifficultyConfig {
  baseLevel: int, // 1-10 scale
  playerProgression: float, // 0-1 (percentage through game)
  recentFailures: int, // Adaptive difficulty adjustment
  difficultyModifiers: {
    timePressure: float, // 1.0 = normal, 1.5 = tight timer
    informationClarity: float, // 1.0 = clear, 0.5 = obscure
    mechanicalDifficulty: float, // Execution challenge
    cognitiveComplexity: float // Problem-solving challenge
  }
}
```

**Dynamic Difficulty Adjustment**:
- Track player performance on previous puzzles
- Reduce difficulty after multiple failures (increase hints, extend timers)
- Increase difficulty after consistent successes
- Provide optional hard mode puzzles for bonus rewards

#### Procedural Puzzle Generation Example

**Terminal Hacking Puzzle**:
```python
def generate_hacking_puzzle(difficulty_level, seed):
    random.seed(seed)
    
    # Scale parameters based on difficulty
    password_length = 4 + difficulty_level // 2
    fake_passwords = 3 + difficulty_level
    time_limit = 60 - (difficulty_level * 3)
    
    # Generate password
    real_password = generate_password(password_length)
    
    # Generate fake passwords (similar to real)
    fakes = []
    for i in range(fake_passwords):
        fake = generate_similar_password(real_password, similarity=0.6)
        fakes.append(fake)
    
    # Add hints based on difficulty (easier = more hints)
    hint_count = max(1, 4 - difficulty_level // 3)
    hints = generate_hints(real_password, count=hint_count)
    
    return HackingPuzzle(
        password=real_password,
        decoys=fakes,
        hints=hints,
        time_limit=time_limit
    )
```

### Dynamic Event Spawning

Dynamic events inject variety and unpredictability into gameplay through runtime-generated encounters and scenarios.

#### Event Categories

| Event Type | Frequency | Trigger | Duration | Consequences |
|------------|-----------|---------|----------|--------------|
| **Ambush** | Rare | Room entry | 30-90s | Combat, NPC spawns |
| **Resource Drop** | Uncommon | Random timer | Instant | Loot spawned |
| **NPC Patrol** | Common | Time-based | Ongoing | NPCs on patrol route |
| **Environmental Hazard** | Uncommon | Room state change | 10-60s | Damage zones, obstacles |
| **Narrative Vignette** | Rare | Specific room types | 30-120s | Dialogue, story content |
| **Audio Cue** | Common | Player proximity | 3-10s | Tension, hints |

#### Event Spawning Logic

```python
class DynamicEventSpawner:
    def __init__(self, seed):
        self.rng = random.Random(seed)
        self.event_cooldowns = {}
        self.active_events = []
    
    def update(self, delta_time, player_position, game_state):
        # Check for event spawning conditions
        for event_type in EVENT_TYPES:
            if self.can_spawn_event(event_type, game_state):
                if self.rng.random() < event_type.spawn_probability:
                    self.spawn_event(event_type, player_position)
    
    def can_spawn_event(self, event_type, game_state):
        # Check cooldown
        if event_type.id in self.event_cooldowns:
            if time.time() - self.event_cooldowns[event_type.id] < event_type.cooldown:
                return False
        
        # Check prerequisites (mission state, room type, etc.)
        return event_type.check_prerequisites(game_state)
    
    def spawn_event(self, event_type, location):
        event = event_type.instantiate(location, self.rng)
        self.active_events.append(event)
        self.event_cooldowns[event_type.id] = time.time()
        
        # Notify narrator system
        narrator.on_dynamic_event(event)
```

#### Event Weighting and Probability

Events are weighted based on context to maintain appropriate pacing:

```javascript
EventWeightingContext {
  recentCombat: boolean, // Recently in combat? (reduce combat event chance)
  playerHealth: float, // Low health? (increase resource drop chance)
  timeSinceLastEvent: float, // Long time? (increase event chance)
  roomType: enum, // Certain events only in certain rooms
  missionState: string, // Mission can override weights
  difficulty: int // Higher difficulty = more frequent events
}
```

**Example Weighting**:
- **Base Event Chance**: 10% per room entered
- **Modifiers**:
  - +5% if 5+ minutes since last event
  - -10% if in combat in last 60 seconds
  - +8% if player health < 30%
  - -5% if narrative dialogue playing
  - +3% per difficulty level above Normal

### Object Distribution

Procedural placement of interactive objects, props, furniture, and loot throughout generated spaces.

#### Object Categories

| Category | Description | Placement Rules | Density |
|----------|-------------|-----------------|---------|
| **Furniture** | Desks, tables, chairs, cabinets | Room type-specific, grid-aligned | Medium |
| **Clutter** | Small props, decorative items | Scattered, random rotation | High |
| **Interactables** | Terminals, switches, doors | Functional locations, accessible | Low |
| **Loot Containers** | Boxes, safes, lockers | Room corners, against walls | Low-Medium |
| **Cover Objects** | Walls, crates, barriers | Strategic combat positions | Medium |
| **Lighting** | Lamps, fixtures, ambient sources | Ceiling/wall mounts, functional | Medium |

#### Procedural Placement Algorithm

```python
def place_objects_in_room(room, seed, object_density):
    random.seed(seed)
    
    # Phase 1: Place required functional objects
    place_doors(room)
    place_interactables(room) # Terminals, objectives
    
    # Phase 2: Place major furniture
    furniture_layout = generate_furniture_layout(room.type)
    for furniture in furniture_layout:
        position = find_valid_position(room, furniture.footprint)
        if position:
            place_object(furniture, position, random_rotation=True)
    
    # Phase 3: Place loot containers
    loot_count = calculate_loot_count(room.size, difficulty)
    for i in range(loot_count):
        container = select_loot_container(room.theme)
        position = find_corner_or_wall_position(room)
        place_object(container, position)
    
    # Phase 4: Place clutter and detail objects
    clutter_count = int(room.area * object_density)
    for i in range(clutter_count):
        clutter = select_random_clutter(room.theme)
        position = find_surface_or_floor_position(room)
        place_object(clutter, position, random_rotation=True)
    
    # Phase 5: Place lighting
    lighting_layout = calculate_lighting_positions(room)
    for light_pos in lighting_layout:
        light = select_light_fixture(room.lighting_scheme)
        place_light(light, light_pos)
```

#### Placement Constraints

**Collision Avoidance**:
- Maintain minimum spacing between objects (0.5 meters)
- Ensure player and NPC navigation paths remain clear
- Respect NavMesh walkable areas

**Thematic Consistency**:
- Select objects from theme-appropriate pools (office, industrial, residential)
- Match materials and color schemes within room
- Maintain visual coherence (avoid stylistic clash)

**Functional Requirements**:
- Interactables must be player-reachable
- Loot containers visible but not always obvious
- Cover objects provide gameplay advantage
- Lighting provides adequate visibility

#### Loot and Reward Distribution

```javascript
LootDistributionConfig {
  roomType: enum,
  difficulty: int,
  progressionLevel: float, // Player's game progression
  lootTables: {
    common: Array<Item>, // 60% chance
    uncommon: Array<Item>, // 30% chance
    rare: Array<Item>, // 9% chance
    legendary: Array<Item> // 1% chance
  },
  guaranteedLoot: boolean, // Boss rooms, mission rewards
  minimumValue: int, // Total loot value floor
  maximumValue: int // Total loot value ceiling
}
```

### Environmental Variation

#### Lighting Variation

**Lighting Schemes**:
- **Bright**: Fully lit, minimal shadows (safe zones, hubs)
- **Dim**: Reduced ambient light, strategic pools of light (exploration areas)
- **Dramatic**: High contrast, strong directional lights (narrative moments)
- **Minimal**: Sparse lighting, heavy shadows (stealth sections, horror)
- **Colored**: Tinted lighting for thematic emphasis (red alert, blue tech areas)

**Procedural Lighting Parameters**:
```javascript
LightingConfig {
  ambientIntensity: float, // 0-1 (global illumination level)
  pointLightCount: int, // Number of dynamic point lights
  pointLightIntensity: float, // Brightness of each point light
  shadowQuality: enum [None, Low, Medium, High],
  colorTemperature: float, // Kelvin (warm 2700K - cool 6500K)
  flickerProbability: float, // Chance of light flickering
  lightShaftIntensity: float // Volumetric light intensity
}
```

**Dynamic Lighting Changes**:
- Lights can be shot out or destroyed by player
- Emergency lighting activates when main lights fail
- Flickering increases in damaged/dangerous areas
- Mission events can trigger lighting state changes

#### Atmospheric Variation

**Atmosphere Components**:
- **Fog/Haze**: Density, color, height-based falloff
- **Particle Effects**: Dust motes, steam, smoke, sparks
- **Ambient Audio**: Background sound layers, reverb characteristics
- **Post-Processing**: Color grading, bloom, vignetting

**Room Atmosphere Profiles**:

| Profile | Fog Density | Particles | Audio Reverb | Color Grade | Mood |
|---------|-------------|-----------|--------------|-------------|------|
| **Clean** | None | Minimal dust | Dry (0.5s) | Neutral | Professional, safe |
| **Industrial** | Light | Steam, sparks | Medium (1.5s) | Desaturated | Working environment |
| **Abandoned** | Medium | Heavy dust | Long (3.0s) | Cool tint | Neglected, eerie |
| **Damaged** | Heavy | Smoke, debris | Long (2.5s) | Warm tint | Dangerous, chaotic |
| **Sterile** | None | None | Very dry (0.3s) | Blue-green | Clinical, technological |

**Procedural Atmosphere Generation**:
```python
def generate_atmosphere(room, seed, mission_state):
    random.seed(seed)
    
    # Select base profile from room type
    base_profile = ATMOSPHERE_PROFILES[room.type]
    
    # Apply variation
    profile = base_profile.copy()
    profile.fog_density *= random.uniform(0.7, 1.3)
    profile.particle_count *= random.uniform(0.5, 1.5)
    
    # Apply mission state modifiers
    if mission_state.alert_level == "HIGH":
        profile.color_grade = "RED_ALERT"
        profile.particle_effects.append("ALARM_FLASHING")
    
    return profile
```

## Technical Systems Required

### Navigation Mesh (NavMesh)

The NavMesh system defines where NPCs can move and pathfind within procedurally generated spaces.

#### NavMesh Generation

**Runtime NavMesh Baking**:
```python
def generate_navmesh(room_layout, seed):
    # Define navmesh generation parameters
    config = NavMeshConfig(
        agent_radius=0.5,  # NPC collision radius
        agent_height=2.0,  # NPC height
        max_slope=45,      # Maximum walkable slope (degrees)
        step_height=0.4    # Maximum step height
    )
    
    # Generate collision geometry from room layout
    collision_mesh = create_collision_mesh(room_layout)
    
    # Bake navmesh (Recast/Detour algorithm)
    navmesh = bake_navmesh(collision_mesh, config)
    
    # Add off-mesh connections (jumps, drops, teleports)
    add_offmesh_links(navmesh, room_layout.special_connections)
    
    return navmesh
```

**NavMesh Optimization**:
- Simplify mesh (reduce triangle count while preserving accuracy)
- Merge co-planar regions
- Remove inaccessible islands
- Cache baked NavMeshes for reused room templates

#### Pathfinding Integration

**A* Pathfinding with NavMesh**:
```javascript
Path findPath(NavMesh mesh, Vector3 start, Vector3 end) {
  // Find nearest point on navmesh to start/end
  NavPoint startPoint = mesh.getNearest(start);
  NavPoint endPoint = mesh.getNearest(end);
  
  // A* algorithm
  Path path = aStar(startPoint, endPoint, mesh);
  
  // Smooth path (reduce waypoints, curve corners)
  path = smoothPath(path);
  
  // Add height offsets if needed
  path = adjustForAgentHeight(path, agentHeight=2.0);
  
  return path;
}
```

**Dynamic NavMesh Updates**:
- Add temporary obstacles when objects are moved/destroyed
- Recalculate affected regions only (not full rebake)
- Handle blocked paths with dynamic rerouting

#### NavMesh Queries

Common queries NPCs perform:
- `getNearest(position)`: Find nearest point on NavMesh
- `findPath(start, end)`: Calculate path between two points
- `getRandomPoint(radius)`: Get random reachable location within radius
- `raycast(start, direction)`: Check line-of-sight along NavMesh
- `isReachable(point)`: Determine if point is on connected NavMesh

### Spawn Volumes and Zones

#### Spawn Volume Definition

```javascript
SpawnVolume {
  volumeId: string,
  bounds: AABB, // Axis-Aligned Bounding Box
  spawnType: enum [Point, Area, NavMesh],
  npcTypes: Array<NPCType>, // Which NPCs can spawn here
  maxActiveNPCs: int,
  respawnTime: float, // Seconds
  spawnWeights: Map<NPCType, float>,
  conditions: Array<Condition>, // When volume is active
  priority: int // Higher priority volumes activate first
}
```

**Volume Types**:
- **Point**: Precise spawn location (scripted encounters)
- **Area**: Random position within 3D volume
- **NavMesh Region**: Random valid NavMesh point within area

#### Zone Management

**Zone Configuration**:
```javascript
Zone {
  zoneId: string,
  rooms: Array<Room>,
  zoneType: enum [Safe, Combat, Stealth, Exploration, Boss],
  difficulty: int,
  spawnVolumes: Array<SpawnVolume>,
  ambientAudio: AudioClip,
  lightingProfile: LightingConfig,
  narratorContext: string // For contextual narration
}
```

**Zone State Tracking**:
- Active NPC count per zone
- Player presence/absence
- Objective completion status
- Alert level (calm, investigating, combat)
- Environmental state (powered, damaged, locked)

### Runtime Variation Parameters

#### Seed Management System

```python
class SeedManager:
    def __init__(self, master_seed):
        self.master_seed = master_seed
        self.subseed_cache = {}
    
    def get_subseed(self, category, identifier):
        """Generate consistent subseed from master seed"""
        key = f"{category}_{identifier}"
        if key not in self.subseed_cache:
            self.subseed_cache[key] = hash(f"{self.master_seed}_{key}")
        return self.subseed_cache[key]
    
    def get_room_seed(self, room_id):
        return self.get_subseed("room", room_id)
    
    def get_npc_seed(self, npc_id):
        return self.get_subseed("npc", npc_id)
    
    def get_loot_seed(self, container_id):
        return self.get_subseed("loot", container_id)
```

#### Variation Parameter Categories

| Category | Parameters | Affected Systems | Persistence |
|----------|------------|------------------|-------------|
| **Layout** | Room sizes, connections, archetypes | Procedural generation | Per-session |
| **Population** | NPC types, counts, patrol routes | Spawning system | Per-session |
| **Loot** | Item types, quantities, rarities | Loot distribution | Per-container |
| **Environment** | Lighting, atmosphere, materials | Visual variation | Per-room |
| **Events** | Event types, timings, probabilities | Dynamic events | Runtime |
| **Difficulty** | Enemy stats, puzzle complexity, resource scarcity | Balancing | Per-session |

#### Parameter Variation Ranges

```javascript
VariationConfig {
  // Layout variation
  roomSizeVariance: 0.2, // ±20% from template
  connectionDensity: [0.6, 0.9], // Min-max connections per room
  
  // Population variation
  npcCountVariance: 0.3, // ±30% from base count
  hostileRatio: [0.4, 0.7], // Percentage of hostile NPCs
  
  // Loot variation
  lootDensity: [0.5, 1.5], // Multiplier for loot quantity
  rareDropChance: [0.05, 0.15], // 5-15% rare item chance
  
  // Environmental variation
  lightingIntensity: [0.6, 1.2], // Brightness range
  fogDensity: [0.0, 0.5], // Fog thickness range
  
  // Event variation
  eventFrequency: [0.8, 1.2], // Event spawn rate multiplier
  
  // Difficulty scaling
  enemyHealthMultiplier: [0.8, 1.5],
  enemyDamageMultiplier: [0.7, 1.3],
  puzzleTimeMultiplier: [0.7, 1.3]
}
```

### State Tracking for Persistent World Changes

#### World State Manager

```python
class WorldStateManager:
    def __init__(self):
        self.state = {
            "rooms": {},
            "npcs": {},
            "objects": {},
            "events": {},
            "missions": {}
        }
    
    def track_room_state(self, room_id, state_data):
        """Track room-specific changes"""
        self.state["rooms"][room_id] = {
            "visited": True,
            "cleared": state_data.get("cleared", False),
            "objects_destroyed": state_data.get("destroyed_objects", []),
            "lights_disabled": state_data.get("disabled_lights", []),
            "doors_state": state_data.get("doors", {})
        }
    
    def track_npc_state(self, npc_id, state):
        """Track NPC life cycle"""
        self.state["npcs"][npc_id] = {
            "status": state, # "alive", "dead", "fled", "rescued"
            "last_position": npc.position,
            "awareness_level": npc.awareness,
            "relationship": npc.player_relationship
        }
    
    def track_object_state(self, object_id, state):
        """Track interactive object states"""
        self.state["objects"][object_id] = {
            "interacted": True,
            "state": state, # "open", "closed", "locked", "destroyed"
            "loot_taken": state.get("looted", False)
        }
    
    def get_room_state(self, room_id):
        return self.state["rooms"].get(room_id, {})
    
    def save_state(self, filename):
        """Persist state to disk"""
        with open(filename, 'w') as f:
            json.dump(self.state, f)
    
    def load_state(self, filename):
        """Load state from disk"""
        with open(filename, 'r') as f:
            self.state = json.load(f)
```

#### Persistent Change Types

| Change Type | Persists Across | Example | Technical Implementation |
|-------------|-----------------|---------|--------------------------|
| **Room Cleared** | Session | Enemies defeated in room | Flag in room state |
| **Object Destroyed** | Session | Broken crate, shot-out light | Remove from scene, add debris |
| **Door State** | Session | Locked/unlocked/opened doors | Store door state dictionary |
| **Loot Taken** | Session | Container emptied | Mark container as looted |
| **NPC Killed** | Session | Dead enemy doesn't respawn | NPC state = "dead", no respawn |
| **Mission Progress** | Permanent (save) | Quest completed | Mission system database |
| **Reputation Changes** | Permanent (save) | NPC relationships | Player profile |

#### Save System Integration

```javascript
SaveGameData {
  version: string,
  timestamp: DateTime,
  player: {
    position: Vector3,
    health: float,
    inventory: Array<Item>,
    stats: PlayerStats
  },
  worldState: {
    seed: int, // For recreation
    modifiedRooms: Map<RoomId, RoomState>,
    npcStates: Map<NPCId, NPCState>,
    objectStates: Map<ObjectId, ObjectState>
  },
  missions: {
    active: Array<MissionId>,
    completed: Array<MissionId>,
    progress: Map<MissionId, MissionProgress>
  },
  narrative: {
    eventsTriggered: Array<EventId>,
    dialoguesHeard: Array<DialogueId>,
    choicesMade: Array<ChoiceId>
  }
}
```

### NPC AI Architecture

#### Behavior Tree System

**Behavior Tree Node Types**:

| Node Type | Description | Example Use |
|-----------|-------------|-------------|
| **Selector** | Execute children until one succeeds (OR logic) | Try combat, if fails try flee |
| **Sequence** | Execute children until one fails (AND logic) | Check ammo AND fire weapon |
| **Parallel** | Execute multiple children simultaneously | Patrol AND watch for threats |
| **Decorator** | Modify child behavior (conditions, loops) | Only if health > 50% |
| **Action** | Leaf node that performs action | Navigate to point |
| **Condition** | Check state without action | Is player visible? |

**Example Behavior Tree Implementation**:
```python
class BehaviorTree:
    def __init__(self, root_node):
        self.root = root_node
        self.blackboard = {} # Shared data storage
    
    def tick(self, delta_time, npc):
        """Execute behavior tree for one frame"""
        self.blackboard["npc"] = npc
        self.blackboard["delta_time"] = delta_time
        return self.root.execute(self.blackboard)

class SelectorNode:
    def __init__(self, children):
        self.children = children
    
    def execute(self, blackboard):
        for child in self.children:
            result = child.execute(blackboard)
            if result == SUCCESS:
                return SUCCESS
        return FAILURE

class ActionNode:
    def __init__(self, action_function):
        self.action = action_function
    
    def execute(self, blackboard):
        return self.action(blackboard)
```

#### State Machine System

For simpler NPCs or complementary to behavior trees:

```javascript
StateMachine {
  currentState: State,
  states: Map<StateName, State>,
  transitions: Array<Transition>
}

State {
  stateName: string,
  onEnter: Function, // Called when entering state
  onUpdate: Function, // Called each frame while in state
  onExit: Function // Called when leaving state
}

Transition {
  fromState: StateName,
  toState: StateName,
  condition: Function // Returns true when transition should occur
}
```

**Example State Machine for Guard NPC**:
```
States: Idle → Patrol → Alert → Investigate → Combat → Dead
Transitions:
  Idle → Patrol: After 5 seconds of idle
  Patrol → Alert: Heard suspicious sound
  Alert → Investigate: Sound location identified
  Investigate → Combat: Player spotted
  Investigate → Patrol: Nothing found after 30s
  Combat → Dead: Health <= 0
  Any → Combat: Taking damage
```

#### AI Communication System

NPCs can communicate with each other to coordinate behavior:

```python
class AICommunicationSystem:
    def __init__(self):
        self.message_queue = []
    
    def send_message(self, sender, recipients, message_type, data):
        """Send AI message to other NPCs"""
        for recipient in recipients:
            message = AIMessage(
                sender=sender.id,
                recipient=recipient.id,
                type=message_type,
                data=data,
                timestamp=time.time()
            )
            self.message_queue.append(message)
    
    def process_messages(self):
        """Deliver messages to recipients"""
        for message in self.message_queue:
            recipient = get_npc_by_id(message.recipient)
            if recipient:
                recipient.on_message_received(message)
        self.message_queue.clear()

# Message types
AI_MESSAGE_TYPES = {
    "ENEMY_SPOTTED": "Alert nearby allies to player location",
    "CALL_FOR_BACKUP": "Request reinforcements",
    "AREA_CLEAR": "All clear signal after search",
    "TAKING_FIRE": "Under attack, need assistance",
    "TARGET_LOST": "Lost sight of player",
    "HOLD_POSITION": "Coordinate defensive positions"
}
```

## Integration Points

### NPC Behaviors Tied to Missions

#### Mission-Driven NPC Behavior

```javascript
MissionNPCBinding {
  missionId: string,
  npcId: string,
  behaviorOverrides: {
    spawningRule: SpawnRule, // When/where NPC spawns for mission
    dialogueTree: DialogueTree, // Mission-specific dialogue
    objectiveType: enum [Escort, Eliminate, Interact, Protect],
    completionTrigger: Function, // What completes the objective
    failureTrigger: Function // What fails the objective
  }
}
```

**Example Mission-NPC Integration**:
```python
# Mission: Rescue Hostage
mission_config = {
    "mission_id": "rescue_civilian_01",
    "npcs": [
        {
            "npc_id": "hostage_01",
            "spawn_location": "secure_room_03",
            "initial_state": "Captured",
            "behavior": "Friendly",
            "dialogue_tree": "hostage_rescue_dialogue",
            "objective": {
                "type": "Escort",
                "destination": "extraction_point_alpha",
                "on_success": complete_mission,
                "on_failure_conditions": [
                    {"trigger": "npc_died", "action": fail_mission},
                    {"trigger": "player_abandoned", "action": fail_mission}
                ]
            }
        },
        {
            "npc_id": "guard_patrol_01",
            "spawn_location": "hallway_02",
            "initial_state": "Patrol",
            "behavior": "Hostile",
            "patrol_route": "route_alpha",
            "objective": {
                "type": "Obstacle",
                "on_alert": trigger_reinforcements
            }
        }
    ]
}
```

#### Dynamic Mission State Updates

```python
def on_npc_state_changed(npc_id, old_state, new_state):
    # Check if this NPC is part of active missions
    for mission in active_missions:
        if npc_id in mission.tracked_npcs:
            mission.on_npc_state_change(npc_id, new_state)
            
            # Update mission objectives
            if new_state == "Dead" and mission.requires_npc_alive(npc_id):
                mission.fail_objective("keep_npc_alive")
            elif new_state == "Rescued" and mission.requires_rescue(npc_id):
                mission.complete_objective("rescue_target")
```

### Narrator Cues Triggering NPC Events

#### Narrator-Driven Spawning

```python
def on_narrator_event(event_type, context):
    """Narrator events can trigger NPC spawns"""
    
    if event_type == "STORY_REVEAL_BETRAYAL":
        # Spawn hostile NPCs as part of narrative
        spawn_npc_wave(
            npc_type="SecurityGuard",
            count=3,
            location=context["player_location"],
            behavior="Hostile",
            dialogue_line="Stop right there!"
        )
        
    elif event_type == "STORY_ALLY_ARRIVES":
        # Spawn friendly NPC to help player
        spawn_npc(
            npc_type="Ally",
            location=context["arrival_point"],
            behavior="Friendly",
            dialogue_tree="ally_introduction",
            temporary=False # Persists after spawn
        )
```

#### Synchronized Narration and NPC Actions

```javascript
NarratorNPCSync {
  narratorLineId: string,
  syncedActions: [
    {
      timing: float, // Seconds into narrator line
      action: {
        type: "SpawnNPC",
        npcType: "SecurityGuard",
        location: "doorway_entrance",
        animation: "kick_door_open"
      }
    },
    {
      timing: 2.5,
      action: {
        type: "NPCDialogue",
        npcId: "guard_captain_01",
        line: "Hands where I can see them!"
      }
    }
  ]
}
```

### Player Choices Affecting NPC Spawning/Behavior

#### Choice-Consequence System

```javascript
PlayerChoice {
  choiceId: string,
  choiceText: string,
  consequences: {
    immediate: Array<Action>,
    delayed: Array<DelayedAction>,
    npcBehaviorChanges: Array<NPCBehaviorChange>
  }
}

NPCBehaviorChange {
  affectedNPCs: Array<NPCId>, // Specific NPCs or tags
  changeType: enum [Spawn, Despawn, BehaviorSwitch, DialogueChange, HostilityChange],
  changeData: Object
}
```

**Example Choice-Driven NPC Changes**:
```python
# Choice: "Help the civilian" vs "Ignore and continue"
choice_help_civilian = {
    "choice_id": "encounter_01_help",
    "choice_text": "Help the civilian",
    "consequences": {
        "immediate": [
            {"type": "spawn_npc", "npc": "grateful_civilian", "location": "nearby"},
            {"type": "increase_reputation", "faction": "civilians", "amount": 10}
        ],
        "delayed": [
            {"delay": 300, "type": "spawn_npc_reinforcement", "npc": "civilian_ally", "location": "future_encounter"}
        ],
        "npc_behavior_changes": [
            {
                "affected_npcs": ["tag:civilian"],
                "change_type": "HostilityChange",
                "new_hostility": "Friendly"
            }
        ]
    }
}

choice_ignore = {
    "choice_id": "encounter_01_ignore",
    "choice_text": "Ignore and continue",
    "consequences": {
        "immediate": [
            {"type": "decrease_reputation", "faction": "civilians", "amount": 5}
        ],
        "npc_behavior_changes": [
            {
                "affected_npcs": ["tag:civilian"],
                "change_type": "DialogueChange",
                "new_dialogue": "distrustful_civilian_dialogue"
            }
        ]
    }
}
```

#### Reputation System Integration

```javascript
ReputationSystem {
  factions: Map<FactionName, ReputationLevel>, // -100 to +100
  
  onReputationChange(faction, old_level, new_level) {
    // Update NPC behaviors based on reputation
    let threshold_crossed = getThresholdCrossed(old_level, new_level);
    
    if (threshold_crossed == "HOSTILE") {
      // NPCs of this faction become hostile
      setNPCFactionHostility(faction, true);
    } else if (threshold_crossed == "FRIENDLY") {
      // NPCs become friendly, offer assistance
      setNPCFactionHostility(faction, false);
      unlockFactionDialogues(faction);
    }
  }
}
```

### Procedural Generation Seeding and Variation

#### Seed-Based NPC Variation

```python
def generate_npc_patrol_route(room_layout, seed, npc_id):
    """Generate consistent patrol route from seed"""
    rng = random.Random(f"{seed}_{npc_id}_patrol")
    
    # Select patrol waypoints from room's nav mesh
    waypoint_count = rng.randint(3, 6)
    waypoints = []
    
    for i in range(waypoint_count):
        waypoint = room_layout.navmesh.get_random_point(rng)
        waypoints.append(waypoint)
    
    # Determine patrol type
    patrol_type = rng.choice(["Loop", "PingPong", "Random"])
    
    return PatrolRoute(
        waypoints=waypoints,
        type=patrol_type,
        wait_time=rng.uniform(2.0, 5.0)
    )
```

#### Procedural Event and NPC Coordination

```python
def generate_room_population(room, seed, difficulty):
    """Populate room with NPCs based on seed and difficulty"""
    rng = random.Random(seed)
    
    # Calculate NPC count based on room size and difficulty
    base_npc_count = int(room.area / 20)  # 1 NPC per 20 sq meters
    difficulty_multiplier = 1 + (difficulty * 0.2)
    npc_count = int(base_npc_count * difficulty_multiplier)
    
    # Select NPC types from weighted pool
    npc_types = select_npc_types(room.type, npc_count, rng)
    
    # Generate spawn locations
    spawn_locations = []
    for i in range(npc_count):
        location = room.get_valid_spawn_point(rng)
        spawn_locations.append(location)
    
    # Create NPC configurations
    npcs = []
    for i, (npc_type, location) in enumerate(zip(npc_types, spawn_locations)):
        npc_config = {
            "type": npc_type,
            "location": location,
            "patrol_route": generate_npc_patrol_route(room, seed, f"npc_{i}"),
            "equipment": select_equipment(npc_type, difficulty, rng),
            "behavior_variant": rng.choice(BEHAVIOR_VARIANTS[npc_type])
        }
        npcs.append(npc_config)
    
    return npcs
```

#### Variation Without Chaos

**Controlled Randomness Principles**:
1. **Bounded Ranges**: Variation within acceptable min/max ranges
2. **Quality Constraints**: Generated content must meet quality standards
3. **Playtesting Seeds**: Curated seed library of known-good configurations
4. **Fallback Generation**: If generation fails quality check, regenerate or use fallback
5. **Designer Override**: Manual room templates override procedural generation when needed

```python
def generate_with_quality_check(generation_func, seed, max_attempts=10):
    """Generate content with quality validation"""
    for attempt in range(max_attempts):
        result = generation_func(seed + attempt)
        
        if quality_check(result):
            return result
    
    # Fallback to handcrafted default
    return get_fallback_content()

def quality_check(generated_content):
    """Validate generated content meets standards"""
    checks = [
        check_all_areas_reachable(generated_content),
        check_npc_count_reasonable(generated_content),
        check_no_soft_locks(generated_content),
        check_difficulty_appropriate(generated_content)
    ]
    return all(checks)
```

## Prototype Deliverables

### Core Requirements

#### Dynamically Spawning NPCs

**Minimum Viable Implementation**:
- [ ] **2 distinct NPC types implemented**:
  - 1 Friendly NPC (passive, can dialogue)
  - 1 Hostile NPC (active, combat behavior)
- [ ] **Spawn system functional**:
  - NPCs spawn at designated spawn points/volumes
  - Spawn triggers working (proximity, mission state, time-based)
  - Despawn when player leaves area
- [ ] **Basic behavior trees operational**:
  - Friendly: Idle, dialogue interaction, patrol
  - Hostile: Patrol, detection, combat, investigation
- [ ] **Perception system working**:
  - Vision cone detection (FOV-based)
  - Line-of-sight checks functional
  - Hearing system (responds to loud sounds)

#### Procedurally Varied Room Layouts

**Minimum Viable Implementation**:
- [ ] **Room generation functional**:
  - 3-5 room templates defined
  - Rooms connect with hallways/doors
  - Seed-based reproducibility
  - No unreachable areas or soft locks
- [ ] **NavMesh generation**:
  - Runtime NavMesh baking
  - NPCs can navigate all rooms
  - Pathfinding working
- [ ] **Environmental variation**:
  - Lighting schemes vary per room (2-3 distinct schemes)
  - Object placement procedural (furniture, clutter)
  - Material/texture variation applied
- [ ] **Quality assurance**:
  - Playable start to finish
  - No critical bugs in generation
  - Acceptable performance (30+ FPS)

#### Contextual NPC Reactions

**Minimum Viable Implementation**:
- [ ] **Reaction system functional**:
  - NPCs react to player proximity (greeting, alert)
  - NPCs react to player drawing weapon (defensive, hostile)
  - NPCs react to nearby sounds (turn toward, investigate)
- [ ] **Dialogue system integrated**:
  - At least 3 dialogue interactions per NPC type
  - Contextual dialogue (varies based on mission state or reputation)
  - Dialogue affects NPC behavior (e.g., provide hint, become hostile)
- [ ] **Narrator integration**:
  - Narrator comments on NPC encounters
  - Narrator triggered by NPC state changes (spotted, eliminated)

### Acceptance Criteria

#### Demonstrable Features

**Scenario 1: NPC Detection and Response**
```
GIVEN: Player enters room with patrolling hostile NPC
WHEN: Player enters NPC's line of sight
THEN: 
  - NPC stops patrol
  - NPC turns toward player
  - NPC plays alert animation/sound
  - NPC transitions to combat behavior
  - Narrator provides contextual commentary
```

**Scenario 2: Procedural Room Variation**
```
GIVEN: Player starts new game with different seed
WHEN: Player explores environment
THEN:
  - Room layouts differ from previous seed
  - NPC spawn locations vary
  - Object placement is different
  - Lighting schemes vary
  - Playthrough feels unique while maintaining quality
```

**Scenario 3: Contextual Dialogue**
```
GIVEN: Player approaches friendly NPC
WHEN: Player has completed related mission objective
THEN:
  - NPC acknowledges mission completion in dialogue
  - NPC offers new mission or reward
  - Dialogue options reflect current game state
```

#### Performance Targets

| Metric | Target | Method |
|--------|--------|--------|
| **Frame Rate** | 30+ FPS | Profiler during gameplay |
| **NavMesh Bake Time** | < 2 seconds per room | Runtime measurement |
| **NPC Count** | 20 simultaneous NPCs | Stress test scenario |
| **Pathfinding Update** | 10-15 Hz per NPC | Performance profiler |
| **Room Generation** | < 5 seconds for full level | Timer during generation |

#### Quality Standards

- **No Soft Locks**: All areas reachable, no progression blockers
- **Stable AI**: NPCs don't get stuck, exhibit reasonable behavior
- **Clean Visuals**: Procedural generation doesn't create visual glitches
- **Consistent Audio**: NPC dialogue and sound effects play correctly
- **Narrative Coherence**: Narrator and NPC interactions feel intentional

### Testing Scenarios

#### Test Case 1: Basic NPC Interaction
1. Start game with default seed
2. Navigate to first room with friendly NPC
3. Approach NPC (trigger greeting)
4. Interact with NPC (dialogue)
5. Verify NPC provides information or mission
6. Move away (NPC returns to idle)

**Expected Result**: Smooth interaction flow, no errors, appropriate animations and audio

#### Test Case 2: Hostile NPC Combat
1. Enter room with hostile NPC
2. Stay out of detection range (verify patrol behavior)
3. Make noise (verify NPC investigates)
4. Enter line of sight (verify NPC enters combat)
5. Move to cover (verify NPC attempts to flank)
6. Eliminate NPC (verify death behavior and narrator comment)

**Expected Result**: Realistic combat AI, responsive perception, appropriate difficulty

#### Test Case 3: Procedural Variation
1. Note current seed value
2. Play through level, noting layout
3. Restart with different seed
4. Verify significant layout differences
5. Restart with original seed
6. Verify layout matches first playthrough

**Expected Result**: Deterministic seed-based generation, meaningful variation

#### Test Case 4: Mission-NPC Integration
1. Accept mission requiring NPC interaction
2. Navigate to mission location
3. Verify NPC spawns as expected
4. Complete mission objective (rescue, eliminate, etc.)
5. Verify mission updates correctly
6. Verify narrator acknowledges mission progress

**Expected Result**: Tight integration between NPCs and mission system

## Technical Implementation Guide

### Recommended Technology Stack

| Component | Recommended Tools | Alternatives |
|-----------|------------------|--------------|
| **Game Engine** | Unreal Engine 5, Unity | Godot, Custom Engine |
| **Behavior Trees** | Unreal Behavior Tree, Unity ML-Agents | Custom implementation |
| **NavMesh** | Recast/Detour, Engine built-in | A* Pathfinding Project |
| **Procedural Generation** | PCG Framework (UE5), Custom | WaveFunctionCollapse, Dungeon Architect |
| **Dialogue** | Yarn Spinner, Ink | articy:draft, Custom |
| **State Management** | Scriptable Objects (Unity), Data Assets (Unreal) | Custom save system |

### Code Architecture Patterns

#### Entity-Component-System (ECS) for NPCs

```csharp
// NPC Entity composed of components
public class NPCEntity : Entity {
    public PerceptionComponent Perception;
    public BehaviorComponent Behavior;
    public MovementComponent Movement;
    public DialogueComponent Dialogue;
    public CombatComponent Combat;
    
    public override void Update(float deltaTime) {
        // Update all components
        Perception.Update(deltaTime);
        Behavior.Update(deltaTime);
        Movement.Update(deltaTime);
    }
}

// Perception Component (reusable)
public class PerceptionComponent : Component {
    public float ViewDistance;
    public float ViewAngle;
    public float HearingRadius;
    
    public List<Entity> DetectedEntities { get; private set; }
    
    public override void Update(float deltaTime) {
        DetectedEntities.Clear();
        
        // Vision check
        DetectVisualTargets();
        
        // Hearing check
        DetectAuditoryTargets();
    }
}
```

#### Manager Pattern for Systems

```csharp
// Singleton managers for global systems
public class NPCManager : Singleton<NPCManager> {
    private List<NPCEntity> activeNPCs = new List<NPCEntity>();
    
    public NPCEntity SpawnNPC(NPCType type, Vector3 position) {
        NPCEntity npc = Instantiate(type.Prefab, position);
        activeNPCs.Add(npc);
        return npc;
    }
    
    public void Update() {
        // Update all NPCs (consider spatial partitioning for optimization)
        foreach (var npc in activeNPCs) {
            npc.Update(Time.deltaTime);
        }
    }
    
    public void DespawnNPC(NPCEntity npc) {
        activeNPCs.Remove(npc);
        Destroy(npc.gameObject);
    }
}

public class ProceduralGenerationManager : Singleton<ProceduralGenerationManager> {
    public Level GenerateLevel(int seed) {
        // Generation logic
    }
}
```

### Performance Optimization Guidelines

#### NPC Update Optimization

1. **Level of Detail (LOD) for AI**:
   - Near player: Full AI update (10-15 Hz)
   - Medium distance: Reduced update rate (5 Hz)
   - Far distance: Minimal update or freeze (1 Hz)

2. **Spatial Partitioning**:
   - Use spatial hash grid or octree
   - Only update NPCs in active zones
   - Cull off-screen NPC processing

3. **Async Pathfinding**:
   - Pathfinding requests processed on worker threads
   - Results delivered asynchronously
   - Batch multiple requests per frame

4. **Perception Staggering**:
   - Not all NPCs check perception same frame
   - Stagger updates across frames
   - Example: NPC ID % frame_count determines update frame

#### Procedural Generation Optimization

1. **Caching**:
   - Cache generated NavMeshes for reused room templates
   - Cache room layouts for common seeds
   - Precompute expensive calculations

2. **Streaming**:
   - Generate rooms as player approaches
   - Unload distant rooms
   - Asynchronous generation on background thread

3. **Simplification**:
   - Use simpler geometry for NavMesh than visuals
   - Reduce polygon count on procedural objects
   - LOD system for generated content

## QA and Testing Checklist

### Functional Testing

#### NPC Systems
- [ ] NPCs spawn at correct locations
- [ ] NPCs follow patrol routes without getting stuck
- [ ] NPCs detect player within specified ranges
- [ ] NPCs react to player actions appropriately
- [ ] NPCs engage in combat correctly
- [ ] NPCs use dialogue system properly
- [ ] NPCs respond to mission state changes
- [ ] NPCs communicate with each other (alert propagation)
- [ ] NPCs die/despawn correctly

#### Procedural Generation
- [ ] Rooms generate without errors
- [ ] All rooms are accessible (no isolated areas)
- [ ] Room connections (doors/hallways) work correctly
- [ ] NavMesh covers all walkable areas
- [ ] Object placement doesn't block navigation
- [ ] Lighting generates correctly
- [ ] Seed-based generation is deterministic
- [ ] Different seeds produce different layouts
- [ ] Generation completes within acceptable time

#### Integration
- [ ] Mission system correctly spawns/tracks NPCs
- [ ] Narrator system triggers on NPC events
- [ ] Dialogue system integrates with NPC behaviors
- [ ] Player choices affect NPC spawning/behavior
- [ ] State persistence works (NPCs don't respawn after death)
- [ ] Save/load preserves NPC and world state

### Performance Testing
- [ ] Frame rate maintains 30+ FPS with max NPCs
- [ ] No memory leaks during extended play sessions
- [ ] NavMesh generation completes within 2 seconds
- [ ] Pathfinding updates at acceptable frequency
- [ ] Procedural generation doesn't cause frame hitches
- [ ] Large NPC counts don't crash game

### Edge Case Testing
- [ ] Player exploits (getting outside map bounds)
- [ ] NPCs handle blocked paths gracefully
- [ ] NPCs handle destroyed objects in patrol route
- [ ] System handles invalid seeds gracefully
- [ ] System handles extreme difficulty values
- [ ] Concurrent NPC events don't cause conflicts

### User Experience Testing
- [ ] NPC behaviors feel realistic and intelligent
- [ ] Procedural variation feels meaningful
- [ ] Difficulty curve feels appropriate
- [ ] Narrator integration feels seamless
- [ ] Dialogue feels contextual and relevant
- [ ] No frustrating soft locks or dead ends

### Bug Tracking Template

```markdown
## Bug Report: [Short Description]

**Severity**: Critical / High / Medium / Low
**Category**: NPC Behavior / Procedural Generation / Integration / Performance

**Description**:
[Detailed description of the bug]

**Steps to Reproduce**:
1. 
2. 
3. 

**Expected Behavior**:
[What should happen]

**Actual Behavior**:
[What actually happens]

**Environment**:
- Seed Value: [if applicable]
- NPC Type: [if applicable]
- Room Type: [if applicable]
- Mission State: [if applicable]

**Screenshots/Videos**:
[Attach if available]

**Workaround**:
[If known]
```

---

## Appendix: Reference Diagrams

### NPC Behavior Flow

```
[Player Action] → [Perception System] → [Behavior Tree] → [Action Execution]
       ↓                    ↓                   ↓                  ↓
  (Draw Weapon)      (Vision/Hearing)      (Combat Node)      (Attack Player)
       ↓                    ↓                   ↓                  ↓
  (Proximity)          (Detection)         (Investigate)     (Navigate To Sound)
       ↓                    ↓                   ↓                  ↓
  (Dialogue)            (No Threat)          (Idle/Patrol)    (Continue Routine)
```

### Procedural Generation Pipeline

```
[Master Seed] → [Room Layout Gen] → [Object Placement] → [NPC Population] → [Lighting/Atmosphere]
      ↓                  ↓                    ↓                   ↓                    ↓
 (Subseed A)        (BSP Algorithm)     (Furniture/Props)   (Spawn Points)      (Light Configs)
      ↓                  ↓                    ↓                   ↓                    ↓
 (Subseed B)        (Room Templates)     (Loot Containers)  (Behavior Trees)    (Fog/Particles)
      ↓                  ↓                    ↓                   ↓                    ↓
  (Deterministic)    (NavMesh Bake)      (Theme-Consistent)  (Patrol Routes)    (Mood Setting)
```

### Mission-NPC Integration Flow

```
[Mission Start] → [Check Prerequisites] → [Spawn Mission NPCs] → [Track Objectives]
                          ↓                         ↓                      ↓
                   (Player Reputation)      (Quest Giver NPC)      (NPC State Monitoring)
                          ↓                         ↓                      ↓
                   (Mission State)          (Target NPC)           (Objective Updates)
                          ↓                         ↓                      ↓
              (Environmental Conditions)    (Guard NPCs)           (Mission Complete/Fail)
```

## Seed Manager Integration

### Overview

The SeedManager provides centralized deterministic seeding for all procedural generation systems in Protocol EMR. This ensures that NPC populations, world generation, and story beats can be reproduced exactly by using the same seed value.

### Core Features

#### Deterministic Seeding
- **Master Seed**: Single integer that controls all procedural generation
- **Scope-based Seeds**: Separate sub-seeds for different systems (NPCs, chunks, audio, story, etc.)
- **Offset Management**: Track advancement of each scope for reproducible sequences

#### Well-Known Scopes
```csharp
public const string SCOPE_CHUNKS = "chunks";      // World geometry generation
public const string SCOPE_ENCOUNTERS = "encounters"; // Combat encounters placement
public const string SCOPE_AUDIO = "audio";         // Procedural audio generation
public const string SCOPE_NPCS = "npcs";           // NPC spawning and behavior
public const string SCOPE_STORY = "story";         // Story beat generation
public const string SCOPE_LOOT = "loot";           // Loot and item placement
public const string SCOPE_ENVIRONMENT = "environment"; // Environmental effects
```

#### API Usage Examples

```csharp
// Get deterministic random values
int randomNPCCount = SeedManager.Instance.GetRandomInt(SeedManager.SCOPE_NPCS, 1, 5);
Vector3 randomPosition = new Vector3(
    SeedManager.Instance.GetRandomFloat(SeedManager.SCOPE_NPCS, 0) * 100f,
    0f,
    SeedManager.Instance.GetRandomFloat(SeedManager.SCOPE_NPCS, 1) * 100f
);

// Select random NPC type deterministically
NPCType[] availableTypes = { NPCType.Guard, NPCType.Patrol, NPCType.Elite };
NPCType selectedType = SeedManager.Instance.GetRandomItem(availableTypes, SeedManager.SCOPE_NPCS, 2);

// Advance scope offset for next spawn
SeedManager.Instance.AdvanceScopeOffset(SeedManager.SCOPE_NPCS, 1);
```

### Settings Integration

The SeedManager integrates with the existing SettingsManager to allow players to:
- **Enable/Disable Custom Seeds**: Use auto-generated seeds or specify manually
- **Set Custom Seed Values**: Enter specific seeds for reproducible runs
- **Persistent Settings**: Seed preferences are saved with other game settings

```csharp
// Settings integration
SettingsManager.Instance.SetUseProceduralSeed(true);
SettingsManager.Instance.SetProceduralSeed(12345);

// GameManager applies settings on startup
if (SettingsManager.Instance.UseProceduralSeed())
{
    SeedManager.Instance.SetSeed(SettingsManager.Instance.GetProceduralSeed());
}
```

### Save/Load Integration

The SeedManager provides two methods for state persistence:

#### ObjectStateManager Integration
```csharp
// Automatic integration with existing save system
ObjectStateManager.Instance.RecordProceduralState();
// Saves current seed and all scope offsets to world_state.json
```

#### ProceduralStateStore (Lightweight)
```csharp
// Dedicated procedural state storage
ProceduralStateStore.Instance.SaveProceduralState();
// Saves to procedural_state.json
```

### Debug and QA Features

#### In-Game Seed Display
The PerformanceMonitor overlay now displays:
- Current active seed value
- F8 key to copy seed to clipboard
- Real-time seed visibility for QA testing

#### Console Commands
```csharp
// Copy current seed to clipboard
SeedManager.Instance.CopySeedToClipboard();

// Generate new random seed
SeedManager.Instance.GenerateNewSeed();

// Set specific seed
SeedManager.Instance.SetSeed(12345);

// Get debug information
Debug.Log(SeedManager.Instance.GetDebugInfo());
```

### Determinism Guarantees

The SeedManager ensures:
1. **Same Seed = Same World**: Identical seeds produce identical NPC placement, positions, and behaviors
2. **Scope Isolation**: Different scopes (NPCs vs chunks) don't interfere with each other
3. **Offset Consistency**: Advancing scope offsets produces predictable sequences
4. **Save/Load Fidelity**: Saved games restore exact same procedural state

### Testing and Validation

#### Unit Tests
The `SeedManagerTests.cs` validates:
- Seed initialization and setting
- Scope-based seed generation
- Deterministic behavior across reloads
- Save/load persistence
- Random value generation ranges

#### QA Workflow
1. **Load Test Scene**: Start game with test seed (e.g., 12345)
2. **Document NPC Layout**: Note NPC positions, types, and behaviors
3. **Restart with Same Seed**: Exit and restart with identical seed
4. **Verify Identical Layout**: Confirm exact reproduction of NPC placement
5. **Test Save/Load**: Save game, reload, verify state preservation

### Performance Considerations

- **Memory Usage**: <1MB for complete seed state
- **CPU Overhead**: <0.1ms per random number generation
- **Save Time**: <5ms to persist seed state
- **Load Time**: <5ms to restore seed state

---

**Document Version**: 1.1  
**Last Updated**: 2024  
**Author**: Protocol EMR Development Team  
**Status**: Active Development
