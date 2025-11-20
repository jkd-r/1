# AI Narrator and Missions System

## Overview

The AI Narrator and Missions system provides dynamic, context-aware storytelling and quest management for the Protocol EMR game. This system combines event-driven narration with a flexible mission pipeline to create immersive, reactive gameplay experiences.

## AI Narrator Architecture

### Messaging Channels

The AI Narrator communicates with players through multiple channels to ensure comprehensive coverage and immersion:

#### HUD (Heads-Up Display)
- **Purpose**: Real-time contextual hints and environmental narration
- **Format**: Short, concise text overlays (1-2 sentences max)
- **Trigger**: Player proximity to points of interest, environmental changes
- **Examples**:
  - "The terminal flickers with an unread message"
  - "Footprints suggest recent activity here"
  - "The air grows colder as you descend"

#### Phone Chat Interface
- **Purpose**: Detailed mission updates, character dialogue, and story progression
- **Format**: Conversational messages with character avatars and timestamps
- **Trigger**: Mission milestones, player actions, scheduled events
- **Features**:
  - Character-specific avatars and typing indicators
  - Message history and search functionality
  - Attachment support (images, audio clips, documents)

#### Voice Over (VO)
- **Purpose**: Immersive narration and critical story moments
- **Format**: Pre-recorded or synthesized voice lines
- **Trigger**: Major story beats, dramatic moments, tutorial segments
- **Implementation**:
  - Priority-based audio mixing
  - Localization support
  - Dynamic volume adjustment based on game state

### Context Hooks

The AI Narrator monitors various game state changes to provide relevant commentary:

#### Player Action Hooks
```javascript
// Player movement and interaction hooks
PLAYER_ENTERED_ZONE(zoneId, previousZone)
PLAYER_INTERACTED_WITH(objectId, interactionType)
PLAYER_USED_ITEM(itemId, targetId)
PLAYER_TOOK_DAMAGE(damageAmount, source)
PLAYER_HEALTH_CHANGED(currentHealth, maxHealth)
```

#### Environmental Hooks
```javascript
// World state monitoring
TIME_OF_DAY_CHANGED(hour, minute)
WEATHER_CHANGED(newWeather, previousWeather)
AREA_STATUS_CHANGED(areaId, newStatus)
OBJECTIVE_COMPLETED(objectiveId)
ENEMY_SPAWNED(enemyType, location)
```

#### Narrative Hooks
```javascript
// Story progression tracking
STORY_BEAT_REACHED(beatId, context)
CHOICE_MADE(choiceId, consequences)
RELATIONSHIP_CHANGED(characterId, newLevel)
SKILL_UNLOCKED(skillId, playerLevel)
```

### Event-Driven Guidance System

The narrator uses a sophisticated event processing pipeline:

#### Event Prioritization
1. **Critical Events** (Immediate response required)
   - Player death/revive scenarios
   - Major story revelations
   - Time-sensitive mission failures

2. **High Priority** (Response within 30 seconds)
   - Objective completions
   - Character introductions
   - Environmental hazards

3. **Medium Priority** (Response within 2 minutes)
   - Exploration hints
   - Lore discoveries
   - Optional content availability

4. **Low Priority** (Background narration)
   - Ambient commentary
   - Recurring observations
   - Flavor text

#### Context Analysis Engine
```python
class ContextAnalyzer:
    def analyze_event(self, event):
        context = {
            'player_state': self.get_player_context(),
            'environment': self.get_environment_context(),
            'narrative_state': self.get_story_context(),
            'urgency': self.calculate_urgency(event),
            'relevance': self.calculate_relevance(event)
        }
        return self.generate_response(context)
```

### Dynamic Encounter System

The AI Narrator can trigger spontaneous encounters based on player behavior and world state:

#### Encounter Triggers
- **Behavioral**: Player patterns (e.g., stealth vs. aggressive approaches)
- **Temporal**: Time-based events (e.g., night patrols, rush hour)
- **Conditional**: Skill checks, inventory requirements
- **Random**: Weighted chance encounters with cooldowns

#### Encounter Types
1. **Combat Encounters**
   - Ambush scenarios
   - Reinforcement waves
   - Boss introductions

2. **Social Encounters**
   - NPC conversations
   - Merchant opportunities
   - Information brokers

3. **Environmental Encounters**
   - Natural disasters
   - Structural failures
   - Weather phenomena

4. **Narrative Encounters**
   - Flashback sequences
   - Dream sequences
   - Memory fragments

### Dynamic Event Orchestrator (Sprint 9+)

The **DynamicEventOrchestrator** links the procedural world simulation with the AI narrator:

1. **WorldStateBlackboard** aggregates chunk-level metrics (position, threat level, mission flags, player style deltas).
2. **ProceduralEventProfile** assets declare spawn rules, cooldown windows, Unknown dialogue tags, and success/failure behaviors.
3. **SeedManager** ensures deterministic selection by using a dedicated `dynamic_events` scope per chunk.
4. **NPCManager/DifficultyManager** feed threat + encounter data, allowing combat waves to scale automatically.
5. **UnknownDialogueTriggers** receive orchestrator callbacks (`DynamicEventAmbient/Combat/Puzzle`, `DynamicEventStarted`, `DynamicEventResolved/Failed`, `DynamicEventMilestone`) with hydrated context for bespoke responses.

**Performance + QA Notes**
- Scheduling budget: <1ms/frame (tracked via `enablePerformanceLogging`).
- Press **F1** to open the Performance Monitor, confirm active event counts, then **F9** to toggle the orchestrator HUD overlay.
- QA can force ambient/combat/puzzle events via seed replay, then watch the Unknown phone UI for matching dialogue beats (chunk id, threat, player style deltas appear in debug logs/contexts).

## Mission/Quest Pipeline

### Mission Structure

#### Mission Templates
Missions are built from standardized templates that ensure consistency while allowing for customization:

```json
{
  "mission_template": {
    "id": "mission_001",
    "title": "Investigate the Anomaly",
    "description": "Strange readings have been detected in the research wing.",
    "type": "investigation",
    "difficulty": "medium",
    "estimated_duration": "30-45 minutes",
    "prerequisites": ["tutorial_complete", "level_5"],
    "objectives": [
      {
        "id": "obj_001",
        "title": "Reach the Research Wing",
        "description": "Navigate to the eastern research facility.",
        "type": "travel",
        "target": "research_wing_entrance",
        "optional": false,
        "hidden": false
      }
    ],
    "branches": [
      {
        "condition": "player_has_science_skill",
        "path": "science_approach",
        "weight": 1.0
      },
      {
        "condition": "player_has_combat_skill",
        "path": "combat_approach", 
        "weight": 0.8
      }
    ]
  }
}
```

#### Objective Types
1. **Travel Objectives**
   - Reach specific locations
   - Follow paths/routes
   - Escape areas

2. **Collection Objectives**
   - Gather items
   - Scan objects
   - Harvest resources

3. **Interaction Objectives**
   - Talk to NPCs
   - Use terminals
   - Activate devices

4. **Combat Objectives**
   - Eliminate targets
   - Survive waves
   - Protect assets

5. **Stealth Objectives**
   - Avoid detection
   - Hack systems
   - Retrieve items unseen

### Branching Logic System

#### Decision Points
Missions branch at key decision points based on:
- **Player Choices**: Dialogue options, action selections
- **Skill Checks**: Success/failure of ability tests
- **Inventory**: Available items and equipment
- **Reputation**: Standing with factions
- **Time**: Time-sensitive decisions

```javascript
class BranchingLogic {
  evaluateBranch(mission, playerContext) {
    const availableBranches = mission.branches.filter(branch => {
      return this.evaluateCondition(branch.condition, playerContext);
    });
    
    return this.selectWeightedBranch(availableBranches);
  }
  
  evaluateCondition(condition, context) {
    // Complex condition evaluation logic
    switch(condition.type) {
      case 'skill_check':
        return context.skills[condition.skill] >= condition.threshold;
      case 'item_check':
        return context.inventory.includes(condition.item);
      case 'reputation_check':
        return context.reputation[condition.faction] >= condition.level;
      default:
        return false;
    }
  }
}
```

#### Consequence Tracking
Every branch tracks consequences that affect:
- **Mission Outcomes**: Success, partial success, failure
- **Character Development**: Skill gains, reputation changes
- **Story Progression**: Unlocking new content, closing paths
- **World State**: Environmental changes, NPC attitudes

### Phone Notification System

#### Notification Types
1. **Mission Updates**
   - New objectives added
   - Objectives completed
   - Mission failures/successes

2. **Character Messages**
   - Story dialogue
   - Hints and tips
   - Emotional reactions

3. **System Alerts**
   - Time warnings
   - Critical events
   - Technical issues

#### Notification Flow
```python
class NotificationManager:
    def __init__(self):
        self.pending_notifications = []
        self.delivered_notifications = []
        self.priority_queue = PriorityQueue()
    
    def create_notification(self, notification_type, content, urgency):
        notification = {
            'id': self.generate_id(),
            'type': notification_type,
            'content': content,
            'urgency': urgency,
            'timestamp': time.time(),
            'delivery_method': self.determine_delivery_method(urgency)
        }
        
        self.priority_queue.put(notification)
    
    def determine_delivery_method(self, urgency):
        if urgency == 'critical':
            return ['phone', 'hud', 'vo']
        elif urgency == 'high':
            return ['phone', 'hud']
        else:
            return ['phone']
```

## Data Formats

### Scriptable Objects (Unity C#)

```csharp
[CreateAssetMenu(fileName = "NewMission", menuName = "Game/Mission")]
public class MissionData : ScriptableObject
{
    [Header("Mission Info")]
    public string missionId;
    public string title;
    [TextArea(3, 5)]
    public string description;
    public MissionType type;
    public Difficulty difficulty;
    
    [Header("Objectives")]
    public List<ObjectiveData> objectives;
    
    [Header("Branches")]
    public List<MissionBranch> branches;
    
    [Header("Rewards")]
    public List<RewardData> rewards;
    
    [Header("Narrative")]
    public List<NarrativeTrigger> narrativeTriggers;
}

[CreateAssetMenu(fileName = "NewObjective", menuName = "Game/Objective")]
public class ObjectiveData : ScriptableObject
{
    public string objectiveId;
    public string title;
    [TextArea(2, 4)]
    public string description;
    public ObjectiveType type;
    public bool isOptional;
    public bool isHidden;
    public string targetId;
    public int requiredAmount;
    public List<string> completionTriggers;
}
```

### JSON Configuration Files

```json
{
  "narrator_config": {
    "response_timing": {
      "critical_events": 0.5,
      "high_priority": 2.0,
      "medium_priority": 10.0,
      "low_priority": 30.0
    },
    "messaging_channels": {
      "hud": {
        "max_length": 100,
        "duration": 5.0,
        "position": "top_center"
      },
      "phone": {
        "max_message_length": 500,
        "typing_speed": 0.05,
        "auto_advance": true
      },
      "voice": {
        "volume_range": [0.3, 1.0],
        "fade_duration": 1.0,
        "priority_cutoff": 0.7
      }
    }
  }
}
```

### Dialogue Trees (JSON)

```json
{
  "dialogue_tree": {
    "node_id": "intro_conversation",
    "speaker": "ai_narrator",
    "lines": [
      {
        "text": "Welcome to Protocol EMR. I'm your guide through this facility.",
        "voice_line": "intro_001.wav",
        "duration": 3.2
      }
    ],
    "responses": [
      {
        "text": "What is this place?",
        "next_node": "explain_facility",
        "condition": null
      },
      {
        "text": "Who are you?",
        "next_node": "explain_narrator", 
        "condition": null
      }
    ]
  }
}
```

## State Tracking

### Player State Management

```typescript
interface PlayerState {
  // Core stats
  health: number;
  maxHealth: number;
  energy: number;
  level: number;
  experience: number;
  
  // Skills and abilities
  skills: Record<string, number>;
  unlockedAbilities: string[];
  
  // Inventory
  inventory: InventoryItem[];
  equipment: EquipmentSlot[];
  
  // Mission progress
  activeMissions: string[];
  completedMissions: string[];
  failedMissions: string[];
  
  // Relationships
  reputation: Record<string, number>;
  knownCharacters: string[];
  
  // World knowledge
  discoveredLocations: string[];
  unlockedLore: string[];
  completedTutorials: string[];
}
```

### Mission State Tracking

```python
class MissionState:
    def __init__(self, mission_id):
        self.mission_id = mission_id
        self.status = "inactive"  # inactive, active, completed, failed
        self.current_objectives = []
        self.completed_objectives = []
        self.failed_objectives = []
        self.branch_taken = None
        self.start_time = None
        self.completion_time = None
        self.player_choices = []
        self.consequences_applied = []
    
    def update_objective(self, objective_id, new_status):
        if objective_id in self.current_objectives:
            self.current_objectives.remove(objective_id)
            
        if new_status == "completed":
            self.completed_objectives.append(objective_id)
        elif new_status == "failed":
            self.failed_objectives.append(objective_id)
    
    def apply_consequence(self, consequence):
        self.consequences_applied.append(consequence)
        # Apply consequence to player state
```

## Tooling for Writers and Designers

### Mission Editor Tool

#### Features
- **Visual Mission Builder**: Drag-and-drop interface for creating mission flows
- **Branch Management**: Visual branching logic with condition builders
- **Dialogue Editor**: Integrated dialogue tree editor with voice line management
- **Preview Mode**: Test missions in a sandbox environment
- **Localization Support**: Multi-language content management

#### Interface Components
```
┌─────────────────────────────────────────────────────────────┐
│ Mission Editor: Investigate the Anomaly                     │
├─────────────────────────────────────────────────────────────┤
│ [General] [Objectives] [Branches] [Dialogue] [Rewards]      │
├─────────────────────────────────────────────────────────────┤
│ Title: Investigate the Anomaly                              │
│ Description: Strange readings detected...                   │
│ Type: [Investigation ▼] Difficulty: [Medium ▼]             │
│                                                             │
│ ┌─ Objectives ────────────────────────────────────────────┐ │
│ │ ✓ Reach Research Wing                                   │ │
│ │ ○ Scan Anomaly Source                                   │ │
│ │ ○ Report Findings                                       │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─ Branch Visualizer ─────────────────────────────────────┐ │
│ │ [Start] → [Choice: Approach Method]                     │ │
│ │           ├─ [Science Path] → [Lab Analysis] → [End]    │ │
│ │           └─ [Combat Path] → [Force Entry] → [End]      │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Narrative Scripting Tool

#### Script Format
```yaml
narrative_script:
  scene: facility_intro
  triggers:
    - event: player_entered_zone
      zone_id: facility_entrance
      condition: first_time
  
  sequences:
    - id: welcome_sequence
      type: phone_message
      sender: ai_narrator
      delay: 2.0
      messages:
        - text: "Welcome to the EMR Research Facility."
          voice_line: "welcome_001.wav"
        - text: "I'll be your guide. Let's get you oriented."
          voice_line: "welcome_002.wav"
    
    - id: environment_hint
      type: hud_message
      delay: 5.0
      message: "The main terminal is active. Approach it to begin."
      duration: 4.0
```

### Testing and Validation Tools

#### Mission Validator
- **Syntax Checking**: Validates JSON/YAML mission files
- **Logic Verification**: Checks for unreachable objectives, circular dependencies
- **Balance Analysis**: Estimates mission difficulty and duration
- **Localization Testing**: Ensures all text has translations

#### Narrative Flow Tester
- **Path Analysis**: Traces all possible narrative paths
- **Condition Testing**: Validates branching logic
- **Timing Simulation**: Tests response delays and pacing
- **Integration Testing**: Verifies narrator triggers work correctly

## Sample Mission Flows

### Mission: "The Missing Researcher"

#### Setup
```json
{
  "mission": {
    "id": "missing_researcher_001",
    "title": "The Missing Researcher",
    "type": "investigation",
    "difficulty": "easy",
    "estimated_duration": "20 minutes",
    "setup": {
      "starting_location": "research_lab_entrance",
      "key_characters": ["dr_sarah_chen", "security_guard_mike"],
      "required_items": ["keycard_research"]
    }
  }
}
```

#### Flow Diagram
```
Start: Player enters Research Lab
  ↓
HUD: "Security logs show Dr. Chen hasn't checked out."
  ↓
Phone Message from AI: "Dr. Chen is missing. Her last known location was Lab 3."
  ↓
Objective Added: "Find Dr. Chen's workstation"
  ↓
Player reaches Lab 3
  ↓
Branch Point: Computer is password protected
  ├─ Science Skill Check: Hack computer
  │   ↓
  │   Success: Find research notes about anomaly
  │   ↓
  │   New Objective: "Investigate anomaly in Sub-Level B"
  │   ↓
  │   Complete Mission
  └─ Combat/Alternative: Find password in nearby office
      ↓
      Success: Access computer, find location clue
      ↓
      New Objective: "Search Sub-Level B"
      ↓
      Complete Mission
```

#### Sample Narrator Scripts

**Initial Setup**
```
[HUD - 3 seconds after entering lab]
"The air recycling system is working overtime. Something's stressing the systems."

[Phone Message - 5 seconds later]
Sender: AI Guide
"I've detected unusual energy patterns. Dr. Chen was investigating this area before she disappeared."

[Voice Over - When finding first clue]
"Researcher Log 47: The readings are off the charts. This shouldn't be possible..."
```

**Branching Narration**
```
[Science Path Success]
[Phone Message]
Sender: AI Guide
"Excellent work with the terminal! Dr. Chen's notes mention an containment failure in Sub-Level B. That must be where she went."

[Alternative Path Success]
[Phone Message]
Sender: AI Guide  
"Good thinking checking the office. The sticky note on the monitor has the password. Dr. Chen was definitely onto something big."
```

### Mission: "Corporate Espionage"

#### Complex Branching Example
```javascript
const missionFlow = {
  start: {
    trigger: "player_receives_anonymous_message",
    narrator_response: {
      phone: "Unknown Sender: They're hiding something in the server room. Prove it.",
      hud: "New secure message received."
    }
  },
  
  branches: {
    stealth_approach: {
      condition: "player_has_hacking_skill >= 3",
      objectives: [
        "Access security terminal",
        "Disable cameras", 
        "Download server logs"
      ],
      narration: {
        success: "Clean extraction. No one will know you were here.",
        failure: "Alarms triggered! Security is converging on your position."
      }
    },
    
    social_approach: {
      condition: "player_reputation_it >= 5",
      objectives: [
        "Meet IT contact in cafeteria",
        "Obtain access codes",
        "Enter server room legally"
      ],
      narration: {
        success: "Your contact came through. Sometimes who you know matters more than what you know.",
        failure: "The contact sold you out! Security is waiting for you."
      }
    },
    
    combat_approach: {
      condition: "default_branch",
      objectives: [
        "Find security keycard",
        "Fight through guards",
        "Force server access"
      ],
      narration: {
        success: "Brute force works, but the cleanup will be massive.",
        failure: "You're overwhelmed! Fall back and regroup."
      }
    }
  }
};
```

## Implementation Considerations

### Performance Optimization
- **Event Batching**: Process narrator events in batches to reduce overhead
- **Context Caching**: Cache frequently accessed context data
- **Priority Queues**: Use efficient priority queues for message ordering
- **Asset Streaming**: Stream voice lines and other large assets on demand

### Scalability
- **Modular Design**: Separate narrator components for independent scaling
- **Event Archiving**: Archive old events to maintain performance
- **Dynamic Loading**: Load mission content dynamically based on player progress
- **State Compression**: Compress state data for network transmission

### Accessibility
- **Text-to-Speech**: Optional TTS for all narrator messages
- **Visual Indicators**: Visual alternatives to audio cues
- **Adjustable Pacing**: User-configurable message timing
- **Language Support**: Full localization pipeline

### Testing Strategy
- **Unit Tests**: Test individual narrator components
- **Integration Tests**: Verify narrator-game integration
- **User Testing**: Gather feedback on narration quality and timing
- **Load Testing**: Test performance with complex mission scenarios

## Conclusion

The AI Narrator and Missions system provides a comprehensive framework for dynamic, engaging storytelling in Protocol EMR. By combining event-driven narration with flexible mission pipelines, the system creates responsive, immersive experiences that adapt to player choices and actions.

The modular architecture allows for easy expansion and modification, while the comprehensive tooling suite enables writers and designers to create compelling content without requiring deep technical knowledge. The result is a system that can evolve with the game while maintaining consistent, high-quality narrative experiences.

---

*This document serves as the technical specification and design guide for the AI Narrator and Missions system. Implementation should follow the outlined architecture and data formats to ensure consistency and maintainability.*