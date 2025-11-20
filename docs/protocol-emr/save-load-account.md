# Save/Load & Account System Protocol

This document defines the comprehensive save/load architecture, account management, player progression tracking, and data persistence systems for Protocol EMR. These systems ensure reliable game state management, multiple profile support, and robust data integrity throughout the player experience.

For integration with procedural story generation systems including seed-based determinism and state preservation of procedurally generated content, see **[Procedural Story Generation](./procedural-story-generation.md)**.

## Table of Contents

1. [Account/Profile System](#accountprofile-system)
2. [Save Game Architecture](#save-game-architecture)
3. [Player Progression Tracking](#player-progression-tracking)
4. [Save Triggers](#save-triggers)
5. [Load Game System](#load-game-system)
6. [Cloud Sync & Backup](#cloud-sync--backup)
7. [Data Persistence](#data-persistence)
8. [Game State Management](#game-state-management)
9. [New Game Handling](#new-game-handling)
10. [Data Security](#data-security)
11. [UI/UX for Save/Load](#uiux-for-saveload)
12. [Performance Considerations](#performance-considerations)
13. [Technical Requirements](#technical-requirements)
14. [Integration Points](#integration-points)
15. [Prototype Deliverables](#prototype-deliverables)
16. [QA and Testing Checklist](#qa-and-testing-checklist)

## Account/Profile System

### Overview

The Profile System allows multiple players to maintain separate progression states on a single installation. Profiles are locally stored and managed through an intuitive menu interface, ensuring clear separation between different play sessions and player experiences.

### Profile Data Structure

```javascript
// Profile schema
Profile {
  profileId: string (UUID),
  profileName: string (max 20 characters),
  creationDate: timestamp (ISO 8601),
  lastPlayedDate: timestamp (ISO 8601),
  totalPlaytime: float (seconds),
  currentSaveSlotId: string (UUID, last active save),
  
  // Profile settings
  avatarIcon: enum [Icon01, Icon02, ..., Icon12],
  difficulty: enum [Easy, Normal, Hard, Nightmare],
  
  // Progress summary (highest across all saves)
  highestMissionCompleted: int,
  totalMissionsCompleted: int,
  totalAchievements: int,
  
  // Statistics (cumulative across all saves in profile)
  cumulativeStats: {
    totalPlaytime: float,
    totalDeaths: int,
    totalNPCsEncountered: int,
    totalCollectiblesFound: int,
    totalCombatEncounters: int,
    totalPuzzlesSolved: int
  },
  
  // Metadata
  version: string (save system version),
  checksum: string (integrity validation)
}
```

### Profile Management Features

#### Profile Creation
- **Initial Launch**: Prompt player to create first profile on game startup
- **Profile Name**: Text input with validation (1-20 characters, alphanumeric + spaces)
- **Avatar Selection**: 12 preset icons to choose from (expandable)
- **Difficulty Selection**: Choose initial difficulty (can be changed later per save)

#### Profile Switching
- **Access**: Available from main menu before gameplay starts
- **Profile List**: Displays all profiles sorted by last played date
- **Quick-Load**: Option to automatically load last played save from selected profile
- **No In-Game Switching**: Profile switching only allowed from main menu (not during gameplay)

#### Profile Display Information

| Field | Description | Display Location |
|-------|-------------|------------------|
| Profile Name | Player-chosen name | Profile selector, main menu header |
| Avatar Icon | Visual identifier | Profile selector, pause menu |
| Last Played | Time since last session | Profile selector |
| Total Playtime | Cumulative hours:minutes | Profile selector, statistics screen |
| Progress Indicator | Mission completion percentage | Profile selector |
| Current Save Info | Latest save timestamp + location | Profile selector (subtitle) |

#### Profile Deletion
- **Access**: Profile management menu
- **Confirmation Dialog**: Two-step confirmation to prevent accidental deletion
  - Step 1: "Are you sure you want to delete [ProfileName]? This will delete all associated save files."
  - Step 2: Type profile name to confirm (for extra protection)
- **Cascade Delete**: Automatically deletes all associated save files
- **No Recovery**: Once deleted, profile cannot be restored (unless backup exists)

#### Recent Profiles Quick-Load
- **Main Menu Quick Access**: Show 3 most recent profiles with "Continue" button
- **One-Click Resume**: Automatically loads most recent save from that profile
- **Fallback**: If no valid save exists, prompt to start new game

### Profile File Structure

```
[UserDocuments]/ProtocolEMR/Profiles/
├── profile_[UUID].json
├── profile_[UUID].json
└── profile_[UUID].json
```

### Profile Icon/Avatar Customization

#### Available Icons (Prototype)
- 12 preset icons representing different character archetypes
- Icon categories: Tactical, Investigator, Survivor, Engineer, Specialist

#### Future Expansion
- Unlockable icons based on achievements
- Custom icon upload (validated for appropriate content)
- Animated avatar frames

## Save Game Architecture

### Overview

The save system uses a multi-slot approach with automatic, manual, and checkpoint saves to ensure player progress is never lost. Save files use JSON format with optional encryption for security and easy debugging during development.

### Save File Format

#### Option A: JSON (Recommended for Prototype)
- **Advantages**: Human-readable, easy debugging, version control friendly
- **Format**: Pretty-printed JSON with clear structure
- **Compression**: Optional GZIP compression to reduce file size
- **Encryption**: Optional AES-256 encryption layer for security

#### Option B: Binary with Serialization
- **Advantages**: Faster I/O, smaller file size, harder to manually edit
- **Format**: Binary serialized data with custom headers
- **Use Case**: Production builds with performance priority

**Prototype Recommendation**: Use JSON for development, transition to binary for production if needed.

### Save Slot Management

#### Slot Types

| Slot Type | Quantity | Description | Update Frequency | Player Control |
|-----------|----------|-------------|------------------|----------------|
| **Auto-Save** | 3 rotating | Automatic saves at checkpoints | Every 5-10 minutes | Read-only (no manual save) |
| **Manual Save** | 10 slots | Player-triggered saves | On demand | Full control |
| **Checkpoint** | 5 rotating | Pre-major event saves | Before key events | Automatic, read-only |
| **Quick-Save** | 1 slot | Hotkey fast save | On hotkey press (F5) | Full control |

#### Slot Rotation
- **Auto-Save Rotation**: Keep 3 most recent auto-saves, delete oldest when creating new
- **Checkpoint Rotation**: Keep 5 most recent checkpoint saves
- **Manual Saves**: Player manages manually, can delete any slot
- **Quick-Save Overwrite**: Quick-save always overwrites previous quick-save

### Save Data Structure

```javascript
// Complete save file schema
SaveFile {
  // Metadata
  saveId: string (UUID),
  profileId: string (UUID, foreign key to profile),
  saveSlotType: enum [AutoSave, ManualSave, Checkpoint, QuickSave],
  slotIndex: int (0-9 for manual, 0-2 for auto-save, etc.),
  
  timestamp: timestamp (ISO 8601),
  saveVersion: string (format: "1.2.3"),
  gameVersion: string,
  
  // Save preview metadata
  locationName: string (player's current location),
  currentMissionName: string,
  playtimeSeconds: float,
  screenshotPath: string (optional thumbnail path),
  
  // Player state
  playerState: {
    position: {x: float, y: float, z: float},
    rotation: {pitch: float, yaw: float, roll: float},
    health: float (0-100),
    stamina: float (0-100),
    currentWeapon: string (item ID),
    equippedItems: [string] (array of item IDs),
    
    inventory: {
      items: [
        {
          itemId: string,
          quantity: int,
          metadata: object (item-specific data)
        }
      ],
      capacity: int,
      currency: int
    },
    
    statistics: {
      totalPlaytime: float,
      missionsCompleted: int,
      npcsEncountered: int,
      combatEncounters: int,
      deaths: int,
      collectiblesFound: int,
      puzzlesSolved: int,
      stealthKills: int,
      alertsTriggered: int
    }
  },
  
  // Mission state
  missionState: {
    activeMissions: [
      {
        missionId: string,
        objectives: [
          {
            objectiveId: string,
            status: enum [Active, Completed, Failed],
            progress: float (0-1)
          }
        ],
        startTimestamp: timestamp
      }
    ],
    completedMissions: [string] (array of mission IDs),
    availableMissions: [string],
    failedMissions: [string],
    missionChoices: {
      // Key-value pairs of choice IDs and selections
      "mission_01_choice_finale": "option_spare",
      "mission_03_choice_alliance": "option_trust"
    }
  },
  
  // World state
  worldState: {
    currentScene: string (scene ID),
    seed: int (procedural generation seed for current level),
    
    // Persistent world changes
    objectStates: [
      {
        objectId: string (unique per instance),
        state: enum [Default, Destroyed, Collected, Unlocked, Activated],
        customData: object (object-specific state)
      }
    ],
    
    // Door and lock states
    doorStates: [
      {
        doorId: string,
        isLocked: boolean,
        isOpen: boolean,
        keyRequired: string (item ID or null)
      }
    ],
    
    // Collectible state
    collectiblesFound: [string] (array of collectible IDs),
    
    // Environmental state
    environmentState: {
      timeOfDay: float (0-24),
      lightingState: enum [Normal, Alert, Emergency],
      hazardsActive: [string] (array of hazard IDs)
    }
  },
  
  // NPC state
  npcState: {
    encounteredNPCs: [
      {
        npcId: string,
        status: enum [Alive, Dead, Fled, Neutral, Hostile, Friendly],
        lastKnownPosition: {x: float, y: float, z: float},
        relationshipLevel: int (-100 to 100),
        dialogueFlags: [string] (completed dialogue tree nodes),
        questGiverState: enum [Available, Active, Completed, Failed]
      }
    ],
    globalHostilityLevel: float (0-1),
    alertStates: {
      currentAlertLevel: enum [None, Suspicious, Alerted, Combat],
      alertTimer: float
    }
  },
  
  // Unlocked content
  unlockedContent: {
    cosmetics: [string] (unlocked cosmetic item IDs),
    alternativePaths: [string] (discovered secret routes),
    loreEntries: [string] (discovered lore/codex entries),
    abilities: [string] (unlocked player abilities)
  },
  
  // Settings snapshot (for session restoration)
  sessionSettings: {
    difficulty: enum [Easy, Normal, Hard, Nightmare],
    activeHints: boolean,
    subtitlesEnabled: boolean
  },
  
  // Integrity and validation
  checksum: string (SHA-256 hash of save data),
  compressed: boolean,
  encrypted: boolean
}
```

### Save File Versioning

#### Version Format
- **Semantic Versioning**: `MAJOR.MINOR.PATCH` (e.g., "1.0.0")
- **Major**: Breaking changes requiring migration
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes, fully compatible

#### Version Compatibility Handling

```javascript
// Version compatibility matrix
CompatibilityCheck {
  currentVersion: string,
  saveVersion: string,
  
  isCompatible(): boolean {
    let current = parseVersion(currentVersion);
    let save = parseVersion(saveVersion);
    
    // Major version must match
    if (current.major != save.major) return false;
    
    // Minor version can be higher (forward compatible)
    if (current.minor < save.minor) return false;
    
    return true;
  },
  
  requiresMigration(): boolean {
    // If minor version differs, may need migration
    let current = parseVersion(currentVersion);
    let save = parseVersion(saveVersion);
    return current.minor > save.minor;
  }
}
```

#### Migration System
- **Auto-Migration**: Automatically upgrade old save files to new format
- **Migration Log**: Record migration steps for debugging
- **Backup Before Migration**: Create backup of original save before migrating
- **Rollback**: Allow reverting to previous game version if migration fails

### Corruption Protection and Recovery

#### Corruption Detection
1. **Checksum Validation**: SHA-256 hash verification on load
2. **Schema Validation**: JSON schema validation against expected structure
3. **Logical Validation**: Check for impossible values (negative health, invalid positions)

#### Recovery Strategies

| Issue | Detection Method | Recovery Action |
|-------|------------------|-----------------|
| **Checksum Mismatch** | SHA-256 comparison | Load from backup, warn player |
| **Invalid JSON** | JSON parser exception | Attempt repair, load backup if failed |
| **Missing Required Fields** | Schema validation | Use default values, flag warning |
| **Corrupted Player State** | Logical validation | Reset player to safe state (spawn point) |
| **All Saves Corrupted** | Multi-file check | Offer New Game with recovered statistics |

#### Automatic Backup Creation
- **Backup Timing**: Create backup before overwriting save file
- **Backup Retention**: Keep last 3 backups per slot
- **Backup Location**: `[UserDocuments]/ProtocolEMR/Backups/`

## Player Progression Tracking

### Overview

Player progression tracking encompasses mission completion, world exploration, combat statistics, and collectibles. This data informs both save/load states and feeds into achievement systems and analytics.

### Mission and Objective Completion

```javascript
MissionProgress {
  missionId: string,
  missionName: string,
  status: enum [NotStarted, Active, Completed, Failed],
  
  objectives: [
    {
      objectiveId: string,
      description: string,
      status: enum [Active, Completed, Failed, Optional],
      progress: float (0-1), // Percentage completion
      isOptional: boolean
    }
  ],
  
  completionTimestamp: timestamp (null if not completed),
  attempts: int (number of times mission was attempted),
  bestTime: float (seconds, for timed missions),
  
  // Choice tracking
  choices: {
    "choice_id": "selected_option_id"
  }
}
```

### Player Inventory State

```javascript
InventoryState {
  items: [
    {
      itemId: string,
      itemType: enum [Weapon, Tool, Consumable, Collectible, KeyItem],
      quantity: int,
      isEquipped: boolean,
      
      // Item-specific metadata
      metadata: {
        durability: float (0-1, for weapons/tools),
        ammo: int (for weapons),
        customization: object (attachments, upgrades)
      }
    }
  ],
  
  capacity: int (max items),
  weight: float (current encumbrance),
  currency: int (in-game money/credits)
}
```

### World State Tracking

```javascript
WorldState {
  // Spatial exploration
  locationsDiscovered: [string] (location IDs),
  roomsExplored: [string] (room IDs),
  secretsFound: [string] (secret area IDs),
  
  // Puzzle state
  puzzlesSolved: [
    {
      puzzleId: string,
      solvedTimestamp: timestamp,
      attempts: int,
      solutionTime: float (seconds)
    }
  ],
  
  // NPC interactions
  npcsEncountered: [
    {
      npcId: string,
      firstEncounterTimestamp: timestamp,
      interactionCount: int,
      relationship: int (-100 to 100),
      status: enum [Alive, Dead, Fled, Unknown]
    }
  ],
  
  // Events triggered
  worldEvents: [
    {
      eventId: string,
      timestamp: timestamp,
      playerTriggered: boolean
    }
  ]
}
```

### Player Statistics

#### Core Statistics

| Statistic | Type | Description | Tracking Scope |
|-----------|------|-------------|----------------|
| **Total Playtime** | float (seconds) | Cumulative time played | Per profile |
| **Missions Completed** | int | Number of main missions completed | Per save |
| **Optional Objectives** | int | Bonus objectives completed | Per save |
| **NPCs Encountered** | int | Unique NPCs met | Per save |
| **Combat Encounters** | int | Number of combat situations | Per save |
| **Stealth Kills** | int | Silent takedowns | Per save |
| **Alerts Triggered** | int | Times player was detected | Per save |
| **Deaths** | int | Player death count | Per save |
| **Collectibles Found** | int / int (found/total) | Collectible completion | Per save |
| **Puzzles Solved** | int | Puzzles completed | Per save |
| **Distance Traveled** | float (meters) | Total distance walked/run | Per save |
| **Doors Unlocked** | int | Unique doors opened | Per save |

#### Advanced Statistics (Optional)

```javascript
AdvancedStats {
  // Combat metrics
  combat: {
    totalDamageDealt: float,
    totalDamageTaken: float,
    accuracyPercentage: float (0-100),
    favoriteWeapon: string (most used weapon ID),
    longestCombatStreak: int
  },
  
  // Exploration metrics
  exploration: {
    mapCompletionPercentage: float (0-100),
    fastestMissionCompletion: {
      missionId: string,
      timeSeconds: float
    },
    mostVisitedLocation: string
  },
  
  // Social metrics
  social: {
    dialoguesCompleted: int,
    npcsHelped: int,
    npcsBetrayed: int,
    reputationLevel: int (-100 to 100)
  }
}
```

### Unlocked Content

```javascript
UnlockedContent {
  // Cosmetics
  cosmetics: [
    {
      cosmeticId: string,
      cosmeticType: enum [Skin, Outfit, Weapon, Accessory],
      unlockedTimestamp: timestamp,
      unlockMethod: enum [Achievement, Mission, Purchase, Secret]
    }
  ],
  
  // Abilities and upgrades
  abilities: [
    {
      abilityId: string,
      tier: int (1-3),
      unlockedTimestamp: timestamp
    }
  ],
  
  // Alternative paths
  alternativePaths: [
    {
      pathId: string,
      discoveredTimestamp: timestamp,
      timesUsed: int
    }
  ],
  
  // Lore and codex entries
  loreEntries: [
    {
      entryId: string,
      category: enum [Character, Location, Event, Technology],
      unlockedTimestamp: timestamp,
      hasBeenRead: boolean
    }
  ]
}
```

### Choice Tracking for Branching Storylines

```javascript
StoryChoices {
  // Major story decisions
  majorChoices: [
    {
      choiceId: string,
      choiceDescription: string,
      selectedOption: string,
      timestamp: timestamp,
      affectedNPCs: [string] (NPC IDs),
      affectedMissions: [string] (mission IDs),
      consequences: [string] (description of outcomes)
    }
  ],
  
  // Minor dialogue choices
  dialogueChoices: {
    "dialogue_node_id": "selected_option_id"
  },
  
  // Branching path tracking
  currentStoryBranch: string (branch ID),
  availableBranches: [string],
  lockedBranches: [string] (branches no longer accessible)
}
```

## Save Triggers

### Overview

The save system activates through multiple trigger types, balancing automatic safety nets with player control. This ensures progress is preserved while respecting player agency.

### Auto-Save System

#### Auto-Save Triggers

| Trigger Event | Priority | Cooldown | Description |
|--------------|----------|----------|-------------|
| **Mission Checkpoint** | High | 30s | After completing major mission objectives |
| **Time Interval** | Medium | 5-10 min | Periodic timed saves during exploration |
| **Zone Transition** | Medium | 60s | When entering new major areas |
| **Before Combat** | High | 120s | Just before enemy encounters (if detected) |
| **After Puzzle** | Medium | 30s | After successfully solving puzzle |
| **Collectible Found** | Low | 120s | After collecting important items |
| **Dialogue End** | Low | 60s | After important NPC conversations |

#### Auto-Save Configuration

```javascript
AutoSaveConfig {
  enabled: boolean (default: true),
  
  timingConfig: {
    minIntervalSeconds: 300, // 5 minutes minimum
    maxIntervalSeconds: 600, // 10 minutes maximum
    globalCooldown: 30 // Minimum time between any auto-saves
  },
  
  triggerWeights: {
    missionCheckpoint: 100,
    timeInterval: 50,
    zoneTransition: 70,
    beforeCombat: 90,
    afterPuzzle: 60,
    collectibleFound: 30,
    dialogueEnd: 40
  },
  
  // Safety rules
  disableDuringCombat: boolean (default: true),
  disableDuringCutscene: boolean (default: true),
  disableWhileMoving: boolean (default: false)
}
```

#### Auto-Save Behavior
- **Non-Blocking**: Save operation runs asynchronously
- **Visual Feedback**: Brief icon in corner of screen (1-2 seconds)
- **Sound Feedback**: Subtle confirmation sound (optional)
- **Rotation**: Maintains 3 most recent auto-saves

### Manual Save System

#### Manual Save Access
- **Pause Menu**: Dedicated "Save Game" option
- **Safe Zones Only**: Can only manually save in designated safe areas (optional restriction)
- **Slot Selection**: Player chooses which slot to save to (1-10)
- **Overwrite Confirmation**: Prompt if overwriting existing save

#### Manual Save UI Flow
1. Player opens pause menu
2. Selects "Save Game"
3. System checks if saving is allowed (not in combat, not in cutscene)
4. Displays save slot selection screen
5. Player selects slot
6. If slot occupied, confirm overwrite
7. Save executes with progress indicator
8. Confirmation message displayed

### Quick-Save System

#### Quick-Save Hotkey
- **Default Binding**: F5 (PC), L1+R1 (console)
- **Rebindable**: Configurable in settings
- **Instant**: No menu interaction required
- **Single Slot**: Always overwrites previous quick-save

#### Quick-Save Behavior
- **Conditions**: Same restrictions as manual save
- **Feedback**: Brief "Quick Save" text overlay + icon
- **Duration**: 1-2 seconds feedback display
- **Sound**: Distinct quick-save confirmation sound

### Checkpoint Save System

#### Checkpoint Triggers

| Checkpoint Type | Description | Example Scenario |
|----------------|-------------|------------------|
| **Pre-Combat** | Before major boss fights or difficult encounters | Entering boss arena |
| **Pre-Decision** | Before major story choice points | Mission finale dialogue |
| **Pre-Challenge** | Before difficult puzzle or platforming section | Complex environmental puzzle |
| **Pre-Danger** | Before high-risk areas | Entering hazardous zone |
| **Story Beat** | After significant narrative moments | Major plot reveal cutscene |

#### Checkpoint Configuration
- **Automatic**: Player has no control over checkpoint saves
- **Retention**: Keep 5 most recent checkpoints
- **Loading**: Can load from checkpoint list in Load Game menu
- **Indicator**: Visual marker in game world where checkpoint was created

### Save Confirmation UI Feedback

#### Visual Feedback
- **Icon**: Animated save icon (spinning disk/cloud symbol)
- **Location**: Top-right corner of screen
- **Duration**: 2-3 seconds
- **Animation**: Fade in, hold, fade out

#### Text Feedback
- **Auto-Save**: "Auto-Saving..."
- **Manual Save**: "Game Saved to Slot [X]"
- **Quick-Save**: "Quick-Saved"
- **Checkpoint**: "Checkpoint Reached"

#### Audio Feedback
- **Success**: Brief positive confirmation tone (200-300ms)
- **Failure**: Negative/error tone with error message
- **Volume**: -18dB relative to SFX bus (subtle, non-intrusive)

### Save Restrictions

#### Blocked Save Conditions
- During active combat
- During cutscenes or scripted sequences
- While player is dead/dying
- While loading another area
- During tutorial sequences (optional)
- While paused (for auto-save only)

#### Error Messages

| Condition | Error Message |
|-----------|---------------|
| **Combat Active** | "Cannot save during combat" |
| **Cutscene Playing** | "Cannot save during cutscene" |
| **Area Loading** | "Cannot save while loading" |
| **Insufficient Space** | "Insufficient disk space to save" |
| **Write Protected** | "Cannot write to save location. Check permissions." |

## Load Game System

### Overview

The load system provides comprehensive access to all save slots with detailed preview information, allowing players to easily identify and restore desired game states.

### Load Menu Interface

#### Main Load Screen Components

```javascript
LoadMenuLayout {
  // Header
  header: {
    profileName: string,
    profileIcon: image,
    totalPlaytime: string (formatted "XXh YYm")
  },
  
  // Filters and sorting
  filterBar: {
    sortBy: enum [Date, Playtime, Mission, SlotType],
    sortOrder: enum [Ascending, Descending],
    filterByType: enum [All, Manual, AutoSave, Checkpoint, QuickSave]
  },
  
  // Save slot list
  saveSlotList: [
    {
      slotIndex: int,
      slotType: enum [Manual, AutoSave, Checkpoint, QuickSave],
      
      // Preview info
      previewData: {
        thumbnail: image (optional screenshot),
        locationName: string,
        missionName: string,
        timestamp: string (formatted),
        playtime: string (formatted),
        saveVersion: string,
        
        // Quick stats
        quickStats: {
          missionProgress: string ("12/20 missions"),
          characterLevel: int,
          deaths: int
        }
      }
    }
  ],
  
  // Actions
  actions: {
    loadButton: button,
    deleteButton: button,
    backButton: button
  }
}
```

#### Save Preview Information

Each save slot displays:

| Field | Description | Format |
|-------|-------------|--------|
| **Slot Type Icon** | Visual indicator of save type | Icon (A, M, C, Q) |
| **Thumbnail** | Optional screenshot from save point | 16:9 image (320x180px) |
| **Location Name** | Player's current location | Text string |
| **Mission Name** | Active mission | Text string |
| **Timestamp** | When save was created | "MMM DD, YYYY - HH:MM AM/PM" |
| **Playtime** | Total playtime in this save | "XXh YYm" |
| **Mission Progress** | Missions completed vs total | "12/20" |
| **Character Level** | Player level or progression tier | "Level 5" |
| **Version** | Save file version | "v1.2.3" (small text) |

### Load Process

#### Load Flow
1. Player selects "Load Game" from main menu or pause menu
2. System scans save directory for valid save files
3. Validates save file integrity (checksum verification)
4. Displays save slots with preview information
5. Player selects desired save
6. Confirmation prompt: "Load [SaveName]? Unsaved progress will be lost."
7. If confirmed, initiate load process
8. Display loading screen with progress indicator
9. Restore game state from save file
10. Transition to gameplay at saved location

#### Load Progress Indicator

```javascript
LoadProgressStages {
  stages: [
    {stage: "Validating save file...", progress: 0.1},
    {stage: "Loading world state...", progress: 0.3},
    {stage: "Loading player data...", progress: 0.5},
    {stage: "Loading mission state...", progress: 0.7},
    {stage: "Initializing NPCs...", progress: 0.85},
    {stage: "Finalizing...", progress: 0.95}
  ]
}
```

### Resume Interrupted Session Detection

#### Session Interruption Scenarios
- Game crash
- Power loss
- Forced process termination
- Operating system shutdown

#### Detection Mechanism

```javascript
SessionRecovery {
  // On game start
  checkForInterruptedSession(): boolean {
    let sessionMarker = readFile("session_active.lock");
    if (sessionMarker exists && sessionMarker.timestamp < 5 minutes ago) {
      // Session was interrupted
      return true;
    }
    return false;
  },
  
  // On game launch
  handleInterruptedSession() {
    if (checkForInterruptedSession()) {
      showDialog({
        title: "Interrupted Session Detected",
        message: "The game did not close properly last time. Would you like to restore the last auto-save?",
        options: ["Restore", "Continue Normally"],
        onRestore: () => loadLatestAutoSave(),
        onContinue: () => clearSessionMarker()
      });
    }
  }
}
```

#### Session Marker File
- **Location**: `[UserDocuments]/ProtocolEMR/session_active.lock`
- **Created**: When game starts
- **Deleted**: When game exits normally
- **Contents**: Timestamp, profile ID, last save ID

### Delete Save Functionality

#### Delete Save Flow
1. Player selects save slot
2. Presses "Delete" button
3. Confirmation dialog appears:
   - **Message**: "Delete this save? This action cannot be undone."
   - **Options**: "Confirm" | "Cancel"
4. If confirmed, delete save file
5. Show confirmation: "Save deleted"
6. Refresh save list

#### Delete Save Protection
- **No Undo**: Deletion is permanent (unless backup exists)
- **Backup Preserved**: Backup files are not deleted (manual cleanup only)
- **Active Save Warning**: If deleting currently loaded save, warn player
- **Last Save Warning**: If deleting only save in profile, extra confirmation required

### Load Failure Handling

#### Failure Scenarios and Recovery

| Failure Type | Detection | Recovery Action |
|--------------|-----------|-----------------|
| **Corrupted File** | Checksum mismatch | Attempt backup load, offer alternative save |
| **Missing File** | File not found | Remove from list, suggest other saves |
| **Version Mismatch** | Version check | Attempt migration, warn of incompatibility |
| **Insufficient Memory** | Memory allocation error | Clear cache, retry with minimal loading |
| **Invalid Data** | Schema validation | Load partial data, reset invalid sections |

#### Error Messages

```javascript
LoadErrorMessages {
  corrupted: "Save file is corrupted. Attempting to load backup...",
  missing: "Save file could not be found. It may have been deleted.",
  versionMismatch: "This save was created with a different game version. Migration may be required.",
  insufficientMemory: "Insufficient memory to load save. Close other applications and try again.",
  invalidData: "Save file contains invalid data. Some progress may be lost.",
  unknownError: "An unknown error occurred while loading. Please try another save."
}
```

#### Fallback Strategy
1. Try loading requested save
2. If fails, try loading most recent backup of that save
3. If fails, offer list of alternative valid saves
4. If no valid saves, offer to start new game with option to recover statistics

## Cloud Sync & Backup

### Overview

Cloud sync provides optional save file synchronization across devices and platforms, while local backup ensures data redundancy for recovery. This system is designed for future expansion but includes local backup implementation in prototype.

### Local Save Backup

#### Backup Strategy

```javascript
BackupConfig {
  enabled: boolean (default: true),
  
  backupTiming: {
    createBeforeOverwrite: boolean (default: true),
    createOnManualSave: boolean (default: true),
    createOnAutoSave: boolean (default: false), // Too frequent
    createBeforeMigration: boolean (default: true)
  },
  
  retention: {
    maxBackupsPerSlot: 3,
    maxTotalBackups: 50,
    autoCleanupOldBackups: boolean (default: true),
    minBackupAgeForCleanup: 604800 // 7 days in seconds
  },
  
  storage: {
    backupLocation: "[UserDocuments]/ProtocolEMR/Backups/",
    compressionEnabled: boolean (default: true),
    encryptionEnabled: boolean (default: false)
  }
}
```

#### Backup File Naming Convention

```
backup_[saveId]_[timestamp].json.gz
Example: backup_a3b4c5d6_20240115_143022.json.gz
```

#### Backup Creation Process
1. Check if backup is needed (based on trigger)
2. Read current save file
3. Compress with GZIP (if enabled)
4. Generate checksum
5. Write to backup folder with timestamp
6. Verify backup integrity
7. Check backup count and cleanup old backups if needed

#### Backup Restoration
- **Manual Restore**: From advanced settings menu
- **Automatic Restore**: On load failure, offer to restore from backup
- **Backup Browser**: UI to view and restore available backups

### Backup Folder Management

#### Folder Structure

```
[UserDocuments]/ProtocolEMR/
├── Profiles/
│   └── profile_[UUID].json
├── Saves/
│   ├── save_[UUID].json
│   ├── save_[UUID].json
│   └── ...
├── Backups/
│   ├── backup_[saveId]_[timestamp].json.gz
│   ├── backup_[saveId]_[timestamp].json.gz
│   └── ...
└── SessionData/
    └── session_active.lock
```

#### Cleanup Policy
- **Automatic Cleanup**: Remove backups older than 7 days if count exceeds limit
- **Manual Cleanup**: Player can delete backups from settings menu
- **Protected Backups**: Option to mark backups as protected (won't auto-delete)

#### Storage Space Management
- **Space Warning**: Alert player if save/backup folder exceeds 500MB
- **Storage Report**: Display total size of saves and backups in settings
- **Bulk Delete**: Option to delete all backups at once

### Cloud Backup Compatibility (Future Implementation)

#### Planned Cloud Platforms
- **Steam Cloud**: For Steam releases
- **Epic Games Cloud Saves**: For Epic Games Store releases
- **Xbox Cloud Gaming**: For Xbox platform
- **PlayStation Plus Cloud Storage**: For PlayStation platform

#### Cloud Sync Architecture (Future)

```javascript
CloudSyncConfig {
  enabled: boolean (default: false), // Disabled in prototype
  
  platforms: {
    steam: {
      enabled: boolean,
      autoSync: boolean,
      syncOnLaunch: boolean,
      syncOnExit: boolean
    },
    epic: {
      enabled: boolean,
      autoSync: boolean,
      syncOnLaunch: boolean,
      syncOnExit: boolean
    }
  },
  
  syncBehavior: {
    uploadOnSave: boolean (default: false), // Upload after each save
    uploadOnExit: boolean (default: true), // Upload on game exit
    downloadOnLaunch: boolean (default: true), // Download on game start
    conflictResolution: enum [UseLocal, UseCloud, AskPlayer]
  }
}
```

#### Sync Conflict Resolution

**Conflict Scenario**: Local save and cloud save differ, unclear which is newer.

**Resolution Options**:
1. **Use Local**: Keep local save, overwrite cloud
2. **Use Cloud**: Download cloud save, overwrite local
3. **Keep Both**: Create separate save slots for both versions
4. **Compare**: Show preview of both saves, let player choose

**Conflict Dialog**:
```
Cloud Sync Conflict Detected

Your local save and cloud save differ. Which would you like to keep?

Local Save:                    Cloud Save:
- Last Played: 2 hours ago     - Last Played: 5 hours ago
- Location: Laboratory B       - Location: Control Room
- Mission: Escape Protocol     - Mission: Investigate Signal
- Playtime: 12h 34m            - Playtime: 11h 58m

[Use Local]  [Use Cloud]  [Keep Both]  [Show Details]
```

#### Cloud Sync Status Indicators
- **Syncing**: Animated cloud icon
- **Synced**: Static cloud icon with checkmark
- **Sync Failed**: Cloud icon with warning symbol
- **Offline**: Cloud icon grayed out

## Data Persistence

### Overview

Data persistence ensures that player settings, preferences, and persistent statistics are maintained across sessions and save files, providing continuity in the player experience.

### Save Location

#### Platform-Specific Paths

| Platform | Save Location |
|----------|---------------|
| **Windows** | `%USERPROFILE%\Documents\ProtocolEMR\` |
| **macOS** | `~/Documents/ProtocolEMR/` |
| **Linux** | `~/.local/share/ProtocolEMR/` |
| **Steam** | `[SteamUserData]/[AppID]/remote/` (cloud saves) |

#### Fallback Locations
- **Primary**: User documents folder
- **Fallback 1**: Application data folder
- **Fallback 2**: Game installation directory (read-only, not recommended)

### Persistent Player Preferences

#### Settings that Persist Across Sessions

```javascript
PersistentSettings {
  // Video settings
  video: {
    resolution: {width: int, height: int},
    fullscreen: boolean,
    vsync: boolean,
    frameRateLimit: int,
    graphicsQuality: enum [Low, Medium, High, Ultra],
    fov: int (60-120)
  },
  
  // Audio settings
  audio: {
    masterVolume: float (0-1),
    sfxVolume: float (0-1),
    musicVolume: float (0-1),
    voiceVolume: float (0-1),
    subtitlesEnabled: boolean,
    audioLanguage: string (ISO 639-1 code)
  },
  
  // Gameplay settings
  gameplay: {
    difficulty: enum [Easy, Normal, Hard, Nightmare],
    hintsEnabled: boolean,
    autoSaveEnabled: boolean,
    controlScheme: enum [Default, Alternative, Custom],
    mouseSensitivity: float (0-1),
    invertYAxis: boolean
  },
  
  // Accessibility settings
  accessibility: {
    colorblindMode: enum [None, Protanopia, Deuteranopia, Tritanopia],
    textSize: enum [Small, Medium, Large, ExtraLarge],
    screenShakeEnabled: boolean,
    motionBlurEnabled: boolean
  },
  
  // UI preferences
  ui: {
    language: string (ISO 639-1 code),
    hudOpacity: float (0-1),
    crosshairStyle: enum [Default, Dot, Cross, Circle],
    showDamageNumbers: boolean,
    showObjectiveMarkers: boolean
  }
}
```

#### Settings File Location
- **File**: `[SaveLocation]/settings.json`
- **Format**: JSON
- **Encryption**: None (settings are not sensitive)

### Persistent Game Statistics

#### Cumulative Statistics (Across All Saves)

```javascript
PersistentStatistics {
  profileId: string (UUID),
  
  // Lifetime statistics
  lifetime: {
    totalPlaytimeSeconds: float,
    totalGamesStarted: int,
    totalGamesCompleted: int,
    totalDeaths: int,
    
    // Aggregate counts
    totalMissionsCompleted: int,
    totalNPCsEncountered: int,
    totalCombatEncounters: int,
    totalCollectiblesFound: int,
    totalPuzzlesSolved: int,
    totalSecretsFound: int,
    
    // Extreme records
    longestPlaySession: float (seconds),
    fastestMissionCompletion: {
      missionId: string,
      timeSeconds: float
    },
    highestCombatStreak: int
  },
  
  // First-time flags
  firstTime: {
    hasCompletedTutorial: boolean,
    hasSeenIntro: boolean,
    hasAccessedSettings: boolean,
    hasUsedManualSave: boolean
  },
  
  // Achievements (unlocked across all saves)
  achievements: [
    {
      achievementId: string,
      unlockedTimestamp: timestamp,
      saveIdWhereUnlocked: string (UUID)
    }
  ]
}
```

#### Statistics File Location
- **File**: `[SaveLocation]/Profiles/profile_[UUID]_stats.json`
- **Update Frequency**: On save creation, on game exit
- **Backup**: Included in backup rotation

### Session State Restoration

#### Session State Data

```javascript
SessionState {
  // Last session info
  lastSessionProfileId: string (UUID),
  lastSessionSaveId: string (UUID),
  lastSessionTimestamp: timestamp,
  lastSessionDuration: float (seconds),
  
  // Session recovery data
  unsavedProgressExists: boolean,
  lastKnownPosition: {x: float, y: float, z: float},
  lastKnownScene: string,
  lastKnownMission: string,
  
  // Session preferences (temporary)
  sessionVolume: float (volume during last session),
  sessionWindowState: {
    position: {x: int, y: int},
    size: {width: int, height: int},
    monitor: int
  }
}
```

#### Session Recovery After Crash

1. **Detect Crash**: Check for `session_active.lock` file on launch
2. **Load Session State**: Read last known position and state
3. **Offer Recovery**: 
   - Dialog: "Game did not close properly. Restore last auto-save?"
   - Options: "Restore" | "Start Normally"
4. **If Restore**:
   - Load most recent auto-save
   - Restore player to last known position
   - Flag as recovered session (for analytics)
5. **If Start Normally**:
   - Clear session marker
   - Proceed to main menu

#### Crash Report Generation (Optional)

```javascript
CrashReport {
  crashTimestamp: timestamp,
  sessionDuration: float,
  lastKnownState: {
    scene: string,
    position: {x: float, y: float, z: float},
    activeMission: string,
    lastAction: string
  },
  systemInfo: {
    os: string,
    cpuModel: string,
    ramGB: int,
    gpuModel: string
  },
  logFile: string (path to log file)
}
```

## Game State Management

### Overview

Game state management encompasses the serialization and persistence of all dynamic world elements, ensuring that player actions and world changes are accurately saved and restored.

### World State Serialization

#### Serializable World Elements

```javascript
WorldStateSerialization {
  // Static objects with dynamic states
  interactableObjects: [
    {
      objectId: string (unique per instance),
      objectType: enum [Door, Container, Switch, Terminal, Collectible],
      state: enum [Default, Opened, Closed, Destroyed, Collected, Activated],
      
      // Transform (if movable)
      transform: {
        position: {x: float, y: float, z: float},
        rotation: {pitch: float, yaw: float, roll: float},
        scale: {x: float, y: float, z: float}
      },
      
      // Type-specific data
      typeData: object {
        // Door
        isLocked: boolean,
        keyRequired: string (item ID),
        // Container
        inventory: [itemId],
        // Terminal
        accessLevel: int,
        hasBeenHacked: boolean
      }
    }
  ],
  
  // Collected items
  collectedItems: [string] (array of collected object IDs),
  
  // Destroyed objects
  destroyedObjects: [string] (array of destroyed object IDs),
  
  // Dynamic objects (spawned during gameplay)
  dynamicObjects: [
    {
      objectId: string,
      prefabName: string,
      transform: {position, rotation, scale},
      state: object (object-specific state data)
    }
  ]
}
```

#### Object State Tracking

**Persistent Object Identification**:
- Each interactable object has unique ID: `[sceneId]_[objectType]_[instanceId]`
- Example: `lab_alpha_door_003`, `warehouse_b_container_012`

**State Change Tracking**:
```javascript
// When object state changes
onObjectStateChange(objectId, newState) {
  worldState.objectStates[objectId] = {
    state: newState,
    timestamp: getCurrentTimestamp(),
    changedByPlayer: true
  };
  
  markDirty(worldState); // Flag for next save
}
```

### NPC State Tracking

#### NPC Persistent Data

```javascript
NPCState {
  npcId: string (unique per NPC instance),
  npcType: string (prefab/template ID),
  
  // Status
  status: enum [Alive, Dead, Fled, Unconscious, Despawned],
  lastKnownPosition: {x: float, y: float, z: float},
  lastKnownScene: string,
  
  // Relationship and behavior
  hostilityState: enum [Friendly, Neutral, Suspicious, Hostile],
  relationshipLevel: int (-100 to 100),
  playerDetected: boolean,
  alertLevel: enum [None, Suspicious, Alert, Combat],
  
  // Dialogue state
  dialogueProgress: {
    completedDialogues: [string] (dialogue node IDs),
    currentDialogueTree: string (null if not in dialogue),
    relationshipModifiers: {
      "dialogue_01": +10,
      "dialogue_05": -5
    }
  },
  
  // Quest state
  questGiverState: {
    hasQuest: boolean,
    questId: string (null if no quest),
    questStatus: enum [Available, Given, Completed, Failed]
  },
  
  // Combat state
  combatState: {
    currentHealth: float,
    isInCombat: boolean,
    lastAttackTimestamp: timestamp,
    damageDealt: float,
    damageTaken: float
  },
  
  // Memory (what NPC remembers about player)
  memory: {
    lastSeenTimestamp: timestamp,
    lastSeenPosition: {x: float, y: float, z: float},
    playerActions: [
      {
        action: enum [Attack, Help, Stealth, Dialogue],
        timestamp: timestamp,
        relationshipImpact: int
      }
    ]
  }
}
```

#### NPC Spawning on Load

```javascript
// When loading save
onLoadNPCState(npcState) {
  // If NPC is dead, don't spawn
  if (npcState.status === "Dead") {
    return;
  }
  
  // If NPC has fled, may respawn in different location
  if (npcState.status === "Fled") {
    spawnNPCInSafeZone(npcState.npcId, npcState.npcType);
    return;
  }
  
  // Spawn NPC at last known position
  let npc = spawnNPC(npcState.npcType, npcState.lastKnownPosition);
  
  // Restore state
  npc.health = npcState.combatState.currentHealth;
  npc.hostilityState = npcState.hostilityState;
  npc.relationshipLevel = npcState.relationshipLevel;
  npc.dialogueProgress = npcState.dialogueProgress;
  
  // Restore memory
  npc.memory = npcState.memory;
}
```

### Dynamic World Changes Persistence

#### Procedurally Generated Room State

```javascript
ProceduralRoomState {
  roomId: string,
  generationSeed: int (seed used to generate room),
  
  // Room layout
  layout: {
    roomType: enum [Corridor, Chamber, Junction, Arena],
    dimensions: {width: float, length: float, height: float},
    entrancePositions: [{x, y, z}],
    exitPositions: [{x, y, z}]
  },
  
  // Persistent changes
  modifications: [
    {
      type: enum [ObjectDestroyed, ObjectMoved, ObjectAdded],
      objectId: string,
      changeTimestamp: timestamp,
      newState: object
    }
  ],
  
  // Room state flags
  stateFlags: {
    hasBeenVisited: boolean,
    hasBeenFullyExplored: boolean,
    puzzlesSolved: [string],
    secretsFound: [string],
    enemiesDefeated: int,
    enemiesRemaining: int
  }
}
```

**Persistence Strategy**:
- **Seed-Based Generation**: Use saved seed to regenerate identical rooms
- **Delta Tracking**: Only save changes from procedural baseline
- **State Flags**: Track completion and exploration status

### Mission State Persistence

```javascript
MissionStateManagement {
  // Active missions
  activeMissions: [
    {
      missionId: string,
      missionName: string,
      startTimestamp: timestamp,
      
      // Objectives
      objectives: [
        {
          objectiveId: string,
          status: enum [Active, Completed, Failed, Cancelled],
          progress: float (0-1),
          
          // Objective-specific data
          targetId: string (NPC ID, location ID, etc.),
          requiredCount: int,
          currentCount: int
        }
      ],
      
      // Mission-specific state
      missionData: {
        customVariables: object (mission-specific flags and values)
      }
    }
  ],
  
  // Mission history
  completedMissions: [
    {
      missionId: string,
      completionTimestamp: timestamp,
      completionTime: float (seconds),
      choicesMade: {
        "choice_id": "selected_option"
      },
      rewardsClaimed: boolean
    }
  ],
  
  // Failed missions
  failedMissions: [
    {
      missionId: string,
      failureTimestamp: timestamp,
      failureReason: string
    }
  ],
  
  // Available missions (not yet started)
  availableMissions: [string] (mission IDs)
}
```

### Environment State Persistence

#### Environmental Variables

```javascript
EnvironmentState {
  // Lighting state
  lighting: {
    currentState: enum [Normal, Alert, Emergency, Blackout],
    timeOfDay: float (0-24, for games with day/night cycle),
    customLightStates: {
      "light_id": boolean (on/off)
    }
  },
  
  // Atmospheric conditions
  atmosphere: {
    fogDensity: float (0-1),
    particleEffects: [
      {
        effectId: string,
        isActive: boolean,
        position: {x, y, z}
      }
    ]
  },
  
  // Hazards
  hazards: [
    {
      hazardId: string,
      hazardType: enum [Fire, Gas, Radiation, Electrical],
      isActive: boolean,
      position: {x, y, z},
      radius: float,
      damagePerSecond: float
    }
  ],
  
  // Dynamic obstacles
  obstacles: [
    {
      obstacleId: string,
      isBlocking: boolean,
      canBeDestroyed: boolean,
      health: float
    }
  ]
}
```

## New Game Handling

### Overview

New game handling manages the initialization of fresh game states while providing options for carrying over progression or starting completely fresh. This includes New Game+ functionality for replayability.

### New Game Flow

#### Initial New Game Process
1. Player selects "New Game" from main menu
2. If profile already exists with saves, confirm: "Start new game? Current saves will not be affected."
3. Select difficulty level
4. Optional: Character customization (if applicable)
5. Display opening cinematic/intro sequence
6. Initialize default game state
7. Start at first mission/tutorial

### New Game Initialization

```javascript
NewGameState {
  // Initialize player
  playerState: {
    position: DEFAULT_SPAWN_POSITION,
    health: 100,
    stamina: 100,
    inventory: {
      items: [], // Empty inventory
      capacity: DEFAULT_INVENTORY_SIZE,
      currency: 0
    },
    statistics: {
      totalPlaytime: 0,
      // All stats set to 0
    }
  },
  
  // Initialize missions
  missionState: {
    activeMissions: [],
    completedMissions: [],
    availableMissions: [FIRST_MISSION_ID],
    missionChoices: {}
  },
  
  // Initialize world
  worldState: {
    currentScene: STARTING_SCENE_ID,
    seed: generateNewSeed(), // Random seed for procedural generation
    objectStates: [],
    doorStates: [],
    collectiblesFound: []
  },
  
  // Initialize NPCs
  npcState: {
    encounteredNPCs: [],
    globalHostilityLevel: 0,
    alertStates: {currentAlertLevel: "None"}
  },
  
  // Session settings
  sessionSettings: {
    difficulty: selectedDifficulty,
    activeHints: true,
    subtitlesEnabled: true
  }
}
```

### New Game+ System

#### New Game+ Overview
New Game+ allows players to replay the game with certain progression elements carried over, providing additional challenge and replayability.

#### Carried Over Elements

```javascript
NewGamePlusCarryOver {
  // Carried over
  carryOver: {
    playerStatistics: true, // Cumulative stats
    unlockedCosmetics: true,
    unlockedAbilities: true,
    achievements: true,
    loreEntries: true,
    bestTimes: true, // For speedrun tracking
    
    // Optional carry-over (player choice)
    inventory: boolean (default: false),
    currency: boolean (default: false),
    equipment: boolean (default: false)
  },
  
  // Reset elements
  reset: {
    missionProgress: true, // Start from beginning
    worldState: true, // Fresh world
    npcRelationships: true,
    storylineChoices: true,
    collectibles: true // Can recollect
  },
  
  // New Game+ bonuses
  bonuses: {
    difficultyModifier: +1, // Enemies slightly harder
    experienceMultiplier: 1.5, // Faster progression
    additionalDialogueOptions: true, // Based on previous playthrough
    secretsRevealed: true // Show hints for secrets found in previous playthrough
  }
}
```

#### New Game+ Activation

**Unlock Condition**: Complete the main story at least once

**Activation Flow**:
1. Player completes main story
2. "New Game+ Now Available" message displayed
3. From main menu, "New Game+" option appears
4. Select "New Game+"
5. Choose carry-over options:
   - "Carry over inventory? [Yes/No]"
   - "Carry over currency? [Yes/No]"
   - "Carry over equipment? [Yes/No]"
6. Select difficulty (minimum: previous difficulty level)
7. Confirm and start New Game+

#### New Game+ Save Marking

```javascript
SaveFileMetadata {
  isNewGamePlus: boolean,
  newGamePlusIteration: int (1, 2, 3, etc.),
  carryOverSettings: {
    inventory: boolean,
    currency: boolean,
    equipment: boolean
  }
}
```

### Difficulty Persistence

#### Difficulty Settings

| Difficulty | Enemy Health | Enemy Damage | Resource Availability | Hint System | Save Restrictions |
|------------|--------------|--------------|----------------------|-------------|-------------------|
| **Easy** | 75% | 50% | Abundant | Full hints | No restrictions |
| **Normal** | 100% | 100% | Normal | Basic hints | No restrictions |
| **Hard** | 150% | 150% | Scarce | Minimal hints | No manual save in combat areas |
| **Nightmare** | 200% | 200% | Very scarce | No hints | Checkpoint saves only |

#### Difficulty Change Handling
- **Per Save**: Difficulty is saved per save file, not per profile
- **In-Game Change**: Player can change difficulty from pause menu
- **Change Restrictions**:
  - Can lower difficulty at any time
  - Can only raise difficulty from main menu (not during active gameplay)
  - Changing difficulty creates new checkpoint save
- **Achievement Impact**: Some achievements require specific difficulties

### Settings Preservation for New Game

#### Preserved Settings
When starting a new game, carry over player preferences:
- Video settings
- Audio settings
- Control bindings
- Accessibility options
- UI preferences

#### Fresh Settings
Reset gameplay-specific settings to defaults:
- Difficulty (player selects for new game)
- Hint system (default: enabled)
- Subtitles (default: enabled)

### Profile Creation Dialog on First Launch

#### First Launch Flow

```javascript
FirstLaunchFlow {
  onFirstLaunch() {
    // Detect first launch
    if (!profilesExist()) {
      showWelcomeScreen();
      showProfileCreationDialog();
    } else {
      showMainMenu();
    }
  }
}
```

#### Welcome Screen
- **Title**: "Welcome to Protocol EMR"
- **Message**: Brief introduction to the game
- **Button**: "Create Profile"

#### Profile Creation Dialog

```
Create Your Profile

Profile Name: [________________]
              (Max 20 characters)

Select Avatar:
[Icon1] [Icon2] [Icon3] [Icon4]
[Icon5] [Icon6] [Icon7] [Icon8]
[Icon9] [Icon10] [Icon11] [Icon12]

[Cancel]  [Create Profile]
```

## Data Security

### Overview

Data security ensures save file integrity, prevents unauthorized modification, and protects player progression from cheating and data corruption.

### Save File Encryption

#### Encryption Strategy (Optional)

**Prototype**: Encryption disabled (for easier debugging)
**Production**: Basic encryption enabled

```javascript
EncryptionConfig {
  enabled: boolean (default: false in prototype),
  
  algorithm: "AES-256-CBC",
  
  // Key derivation
  keyDerivation: {
    method: "PBKDF2",
    iterations: 10000,
    saltLength: 32 // bytes
  },
  
  // Per-file encryption
  perFileSalt: boolean (default: true),
  
  // Performance
  encryptAsync: boolean (default: true) // Non-blocking encryption
}
```

#### Encryption Process

```javascript
encryptSaveFile(saveData) {
  // 1. Serialize to JSON
  let jsonString = JSON.stringify(saveData);
  
  // 2. Generate salt
  let salt = generateRandomSalt(32);
  
  // 3. Derive encryption key
  let key = deriveKey(MASTER_KEY, salt, 10000);
  
  // 4. Encrypt data
  let iv = generateRandomIV(16);
  let encrypted = aes256Encrypt(jsonString, key, iv);
  
  // 5. Combine salt + iv + encrypted data
  let finalData = concatenate(salt, iv, encrypted);
  
  return finalData;
}

decryptSaveFile(encryptedData) {
  // 1. Extract components
  let salt = encryptedData.slice(0, 32);
  let iv = encryptedData.slice(32, 48);
  let encrypted = encryptedData.slice(48);
  
  // 2. Derive key
  let key = deriveKey(MASTER_KEY, salt, 10000);
  
  // 3. Decrypt
  let jsonString = aes256Decrypt(encrypted, key, iv);
  
  // 4. Parse JSON
  let saveData = JSON.parse(jsonString);
  
  return saveData;
}
```

### Save File Validation and Integrity Checks

#### Checksum Generation

```javascript
generateChecksum(saveData) {
  // Create canonical JSON string (sorted keys, no whitespace)
  let canonicalJSON = JSON.stringify(saveData, Object.keys(saveData).sort());
  
  // Generate SHA-256 hash
  let checksum = sha256(canonicalJSON);
  
  return checksum;
}

validateChecksum(saveData, expectedChecksum) {
  let actualChecksum = generateChecksum(saveData);
  return actualChecksum === expectedChecksum;
}
```

#### Integrity Check on Load

```javascript
loadSaveFileWithValidation(filePath) {
  // 1. Read file
  let fileData = readFile(filePath);
  
  // 2. Decrypt if encrypted
  if (isEncrypted(fileData)) {
    fileData = decryptSaveFile(fileData);
  }
  
  // 3. Parse JSON
  let saveData = JSON.parse(fileData);
  
  // 4. Validate checksum
  let storedChecksum = saveData.checksum;
  delete saveData.checksum; // Remove checksum before validating
  
  if (!validateChecksum(saveData, storedChecksum)) {
    throw new IntegrityError("Save file checksum mismatch");
  }
  
  // 5. Validate schema
  if (!validateSchema(saveData)) {
    throw new ValidationError("Save file schema invalid");
  }
  
  // 6. Logical validation
  if (!validateLogic(saveData)) {
    throw new ValidationError("Save file contains invalid values");
  }
  
  return saveData;
}
```

### Protection Against Manual Editing and Cheating

#### Anti-Cheat Measures

| Measure | Description | Effectiveness |
|---------|-------------|---------------|
| **Encryption** | AES-256 encryption makes manual editing very difficult | High |
| **Checksum** | Detects any modifications to save file | High |
| **Schema Validation** | Ensures save structure is valid | Medium |
| **Logical Validation** | Checks for impossible values | Medium |
| **Obfuscation** | Obfuscate field names in JSON | Low |
| **Timestamp Validation** | Verify playtime vs. real-time elapsed | Medium |

#### Logical Validation Examples

```javascript
validateLogicalConsistency(saveData) {
  let errors = [];
  
  // Health must be 0-100
  if (saveData.playerState.health < 0 || saveData.playerState.health > 100) {
    errors.push("Invalid health value");
  }
  
  // Playtime must be reasonable
  let daysSinceCreation = (Date.now() - saveData.timestamp) / (1000 * 60 * 60 * 24);
  let maxPossiblePlaytime = daysSinceCreation * 24 * 3600; // All time played
  if (saveData.playerState.statistics.totalPlaytime > maxPossiblePlaytime) {
    errors.push("Playtime exceeds possible time elapsed");
  }
  
  // Mission consistency
  if (saveData.missionState.completedMissions.length > TOTAL_MISSIONS) {
    errors.push("More missions completed than exist");
  }
  
  // Inventory capacity
  if (saveData.playerState.inventory.items.length > saveData.playerState.inventory.capacity) {
    errors.push("Inventory exceeds capacity");
  }
  
  return errors.length === 0;
}
```

### Private Folder Permissions

#### Folder Security Settings

**Windows**:
```javascript
// Set folder to user-only access
setFolderPermissions("[UserDocuments]/ProtocolEMR/", {
  owner: CURRENT_USER,
  permissions: "ReadWrite",
  inheritance: false
});
```

**macOS/Linux**:
```bash
# Set folder permissions to 700 (owner only)
chmod 700 ~/Documents/ProtocolEMR/
```

#### Security Recommendations
- **User-Specific**: Save folders should be in user-specific directories
- **No Network Shares**: Discourage saving to network drives (performance and security)
- **Antivirus Exclusion**: Recommend excluding save folder from real-time antivirus scanning (performance)
- **Read-Only Protection**: Optionally mark completed saves as read-only

## UI/UX for Save/Load

### Overview

The save/load UI provides intuitive, informative interfaces for managing save files with clear visual feedback and minimal friction.

### Save/Load Screen Layout

#### Main Save Screen

```
┌─────────────────────────────────────────────────────────────┐
│  SAVE GAME                                    Profile: Alex  │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Sort: [Date ▼]  Filter: [All Saves ▼]  [Search...]  │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ [A] AUTO-SAVE                    │ Laboratory B       │ │
│  │ ┌──────────┐                     │ Mission: Escape    │ │
│  │ │[Thumb]   │  Jan 15, 2024       │ Playtime: 12h 34m  │ │
│  │ │          │  11:23 PM           │ Progress: 60%      │ │
│  │ └──────────┘                     │ [Load] [Delete]    │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ [M] MANUAL SAVE - Slot 1         │ Control Room       │ │
│  │ ┌──────────┐                     │ Mission: Investig..│ │
│  │ │[Thumb]   │  Jan 15, 2024       │ Playtime: 11h 58m  │ │
│  │ │          │  10:45 PM           │ Progress: 58%      │ │
│  │ └──────────┘                     │ [Load] [Delete]    │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                               │
│  [More saves...]                                             │
│                                                               │
│                                         [Back to Main Menu] │
└─────────────────────────────────────────────────────────────┘
```

### Save Slot Display Components

#### Slot Type Icons

| Type | Icon | Color | Description |
|------|------|-------|-------------|
| **Auto-Save** | [A] | Blue | Automatic checkpoint save |
| **Manual Save** | [M] | Green | Player-triggered manual save |
| **Checkpoint** | [C] | Yellow | Pre-event checkpoint |
| **Quick-Save** | [Q] | Purple | F5 quick-save |

#### Thumbnail Display (Optional)

- **Size**: 160x90 pixels (16:9 aspect ratio)
- **Generation**: Screenshot captured at moment of save
- **Fallback**: Default placeholder image if screenshot unavailable
- **Performance**: Thumbnails generated asynchronously, displayed when ready

#### Save Slot Information Layout

```
┌──────────────────────────────────────────────┐
│ [TYPE] SAVE NAME            │ Location Name  │
│ ┌──────────┐                │ Mission: Name  │
│ │ Thumb    │  Timestamp     │ Playtime: XXh  │
│ │          │  Date          │ Progress: XX%  │
│ └──────────┘                │ [Action Btns]  │
└──────────────────────────────────────────────┘
```

### Confirmation Dialogs

#### Save Overwrite Confirmation

```
┌────────────────────────────────────────┐
│         Overwrite Save File?           │
├────────────────────────────────────────┤
│                                        │
│  This will replace:                    │
│                                        │
│  Manual Save - Slot 3                  │
│  Location: Control Room                │
│  Mission: Investigate Signal           │
│  Playtime: 8h 23m                      │
│  Saved: Jan 14, 2024 - 9:15 PM        │
│                                        │
│                                        │
│         [Cancel]  [Overwrite]          │
└────────────────────────────────────────┘
```

#### Delete Save Confirmation

```
┌────────────────────────────────────────┐
│           Delete Save File?            │
├────────────────────────────────────────┤
│                                        │
│  This action cannot be undone.         │
│                                        │
│  Manual Save - Slot 5                  │
│  Location: Laboratory B                │
│  Mission: Escape Protocol              │
│  Playtime: 12h 34m                     │
│  Saved: Jan 15, 2024 - 11:23 PM       │
│                                        │
│                                        │
│         [Cancel]  [Delete]             │
└────────────────────────────────────────┘
```

#### Load Save Confirmation (When In-Game)

```
┌────────────────────────────────────────┐
│              Load Save File?           │
├────────────────────────────────────────┤
│                                        │
│  Loading will lose any unsaved         │
│  progress since your last save.        │
│                                        │
│  Load:                                 │
│  Auto-Save                             │
│  Laboratory B - Escape Protocol        │
│  Playtime: 12h 34m                     │
│  Saved: Jan 15, 2024 - 11:23 PM       │
│                                        │
│                                        │
│         [Cancel]  [Load]               │
└────────────────────────────────────────┘
```

### Clear Save Slot Status Indicators

#### Visual Status Indicators

```javascript
SaveSlotVisualStatus {
  slotType: {
    AutoSave: {
      icon: "⏱",
      backgroundColor: "#1E3A8A", // Blue
      borderColor: "#3B82F6"
    },
    ManualSave: {
      icon: "💾",
      backgroundColor: "#166534", // Green
      borderColor: "#22C55E"
    },
    Checkpoint: {
      icon: "📍",
      backgroundColor: "#854D0E", // Yellow
      borderColor: "#EAB308"
    },
    QuickSave: {
      icon: "⚡",
      backgroundColor: "#6B21A8", // Purple
      borderColor: "#A855F7"
    }
  },
  
  // Additional status badges
  badges: {
    NewGamePlus: {
      text: "NG+",
      color: "#DC2626" // Red
    },
    Corrupted: {
      text: "⚠ Corrupted",
      color: "#DC2626"
    },
    OldVersion: {
      text: "⬆ Upgrade Available",
      color: "#F59E0B" // Orange
    }
  }
}
```

### Sort and Filter Options

#### Sort Options

| Option | Sort Order |
|--------|------------|
| **Date (Newest First)** | Sort by timestamp descending |
| **Date (Oldest First)** | Sort by timestamp ascending |
| **Playtime (Highest First)** | Sort by playtime descending |
| **Playtime (Lowest First)** | Sort by playtime ascending |
| **Mission Progress** | Sort by mission completion percentage |
| **Slot Type** | Group by auto/manual/checkpoint/quick |

#### Filter Options

| Filter | Description |
|--------|-------------|
| **All Saves** | Show all save types |
| **Manual Saves Only** | Show only manual saves |
| **Auto-Saves Only** | Show only auto-saves |
| **Checkpoints Only** | Show only checkpoint saves |
| **Quick-Saves Only** | Show only quick-saves |
| **New Game+** | Show only NG+ saves |

### Search and Filter Functionality

#### Search Feature

```javascript
SearchConfig {
  searchFields: [
    "locationName",
    "missionName",
    "slotType",
    "timestamp"
  ],
  
  searchBehavior: {
    caseSensitive: false,
    fuzzyMatching: true,
    minCharacters: 2 // Start searching after 2 characters
  },
  
  displayResults: {
    highlightMatches: boolean (default: true),
    showMatchCount: boolean (default: true)
  }
}
```

**Search UI**:
```
Search: [laboratory________] 🔍
        3 saves found
```

### Visual Feedback During Operations

#### Save Operation Feedback

**Saving Animation**:
```
┌────────────────────────┐
│     Saving Game...     │
│                        │
│   [████████░░░░] 67%   │
│                        │
│  Writing player data   │
└────────────────────────┘
```

**Save Success**:
```
┌────────────────────────┐
│    Save Successful!    │
│          ✓             │
│   Saved to Slot 3      │
└────────────────────────┘
(Auto-closes after 2 seconds)
```

**Save Failed**:
```
┌────────────────────────┐
│      Save Failed       │
│          ✗             │
│  Insufficient space    │
│                        │
│      [Try Again]       │
└────────────────────────┘
```

#### Load Operation Feedback

**Loading Screen**:
```
┌─────────────────────────────────────┐
│         LOADING                     │
│                                     │
│   [████████████████████░] 95%       │
│                                     │
│   Initializing NPCs...              │
│                                     │
│   Protocol EMR                      │
└─────────────────────────────────────┘
```

**Progress Stages**:
- Validating save file... (10%)
- Loading world state... (30%)
- Loading player data... (50%)
- Loading mission state... (70%)
- Initializing NPCs... (85%)
- Finalizing... (95%)
- Ready (100%)

## Performance Considerations

### Overview

Performance optimization ensures save/load operations have minimal impact on gameplay, with non-blocking asynchronous operations and efficient serialization.

### Non-Blocking Save Operations

#### Asynchronous Save Architecture

```javascript
async function saveGameAsync(saveSlot) {
  // 1. Capture game state (fast, synchronous)
  let gameState = captureGameState(); // ~1-5ms
  
  // 2. Perform heavy operations asynchronously (non-blocking)
  await performAsyncSaveOperations(gameState, saveSlot);
  
  // 3. Show confirmation
  showSaveConfirmation();
}

async function performAsyncSaveOperations(gameState, saveSlot) {
  // All heavy operations run on background thread
  
  // Serialize to JSON (5-20ms)
  let jsonString = await serializeAsync(gameState);
  
  // Generate checksum (5-10ms)
  let checksum = await generateChecksumAsync(jsonString);
  gameState.checksum = checksum;
  
  // Compress (10-50ms)
  if (COMPRESSION_ENABLED) {
    jsonString = await compressAsync(jsonString);
  }
  
  // Encrypt (10-50ms)
  if (ENCRYPTION_ENABLED) {
    jsonString = await encryptAsync(jsonString);
  }
  
  // Write to disk (10-100ms depending on disk speed)
  await writeFileAsync(getSaveFilePath(saveSlot), jsonString);
  
  // Create backup asynchronously
  if (BACKUP_ENABLED) {
    await createBackupAsync(saveSlot);
  }
}
```

#### Thread Management

```javascript
ThreadPoolConfig {
  // Dedicated save/load thread pool
  workerThreads: 2, // 2 threads for save/load operations
  
  // Priority levels
  priorities: {
    save: "Medium",
    load: "High", // Load has priority over save
    backup: "Low"
  },
  
  // Queue management
  maxQueueSize: 10,
  queueBehavior: "DropOldest" // If queue full, drop oldest pending save
}
```

### Loading Screen with Progress Bar

#### Progress Tracking

```javascript
LoadProgressTracker {
  stages: [
    {name: "Validating", weight: 0.05},
    {name: "Reading file", weight: 0.10},
    {name: "Decompressing", weight: 0.10},
    {name: "Decrypting", weight: 0.10},
    {name: "Parsing", weight: 0.10},
    {name: "Loading world", weight: 0.25},
    {name: "Loading entities", weight: 0.20},
    {name: "Initializing", weight: 0.10}
  ],
  
  calculateProgress(): float {
    let totalProgress = 0;
    for (stage in completedStages) {
      totalProgress += stage.weight;
    }
    return totalProgress;
  }
}
```

#### Loading Screen UI

```javascript
LoadingScreenConfig {
  // Visual elements
  elements: {
    backgroundImage: "loading_background.png",
    progressBar: {
      width: 400,
      height: 20,
      fillColor: "#3B82F6",
      backgroundColor: "#1E293B",
      borderColor: "#64748B"
    },
    loadingText: {
      font: "Arial",
      size: 18,
      color: "#E2E8F0",
      position: "Below progress bar"
    },
    tipsText: {
      font: "Arial",
      size: 14,
      color: "#94A3B8",
      position: "Bottom center",
      rotationInterval: 5000 // ms, rotate tips every 5 seconds
    }
  },
  
  // Loading tips
  tips: [
    "Use stealth to avoid unnecessary combat encounters.",
    "Explore thoroughly to find hidden collectibles.",
    "Listen carefully to the narrator for important clues.",
    "Save often to avoid losing progress.",
    "Check your mission log for objective updates."
  ]
}
```

### Minimal Frame Rate Impact During Saves

#### Performance Targets

| Operation | Target Time | Max Acceptable | Frame Impact |
|-----------|-------------|----------------|--------------|
| **Capture State** | < 5ms | 10ms | ~0 frames (@60 FPS) |
| **Save (Async)** | < 100ms | 200ms | 0 frames (non-blocking) |
| **Load** | < 2s | 5s | Full load screen |
| **Quick-Save** | < 50ms | 100ms | ~0-1 frames |

#### Frame Rate Protection

```javascript
SaveOperationFrameProtection {
  // Limit synchronous operations to prevent frame drops
  maxSyncTimePerFrame: 5, // ms
  
  // If operation takes longer, split across multiple frames
  yieldToFrameRendering: boolean (default: true),
  
  // Frame rate monitoring
  monitorFrameRate: boolean (default: true),
  targetFrameRate: 60,
  
  // If frame rate drops below threshold, pause non-critical operations
  frameRateThreshold: 30,
  pauseBackgroundOperations: boolean (default: true)
}
```

### Save/Load Time Optimization

#### Optimization Strategies

**1. Differential Saves (Advanced)**
- Only save changed data since last save
- Maintain base save + delta files
- Reconstruction on load

**2. Save Data Compression**
- GZIP compression for JSON data
- Typical compression ratio: 60-80% size reduction
- Trade-off: 10-20ms compression time

**3. Lazy Loading**
- Load critical data first (player, current scene)
- Load non-critical data asynchronously (distant NPCs, world state)
- Player can start playing faster

**4. Pre-allocation**
- Pre-allocate memory buffers for serialization
- Reduces garbage collection during save operations

**5. Disk I/O Optimization**
- Use buffered I/O for file operations
- Batch multiple small writes into single operation
- Prefer SSD over HDD (notify player if saving to slow drive)

#### Performance Monitoring

```javascript
PerformanceMonitoring {
  // Track save/load times
  metrics: {
    lastSaveTime: float (ms),
    averageSaveTime: float (ms),
    lastLoadTime: float (ms),
    averageLoadTime: float (ms),
    
    // Detailed breakdown
    saveBreakdown: {
      captureState: float (ms),
      serialize: float (ms),
      compress: float (ms),
      encrypt: float (ms),
      writeFile: float (ms)
    },
    
    loadBreakdown: {
      readFile: float (ms),
      decrypt: float (ms),
      decompress: float (ms),
      parse: float (ms),
      restoreState: float (ms)
    }
  },
  
  // Automatic optimization
  adaptiveOptimization: {
    // If saves are too slow, disable compression
    disableCompressionIfSlow: boolean,
    slowThreshold: 200, // ms
    
    // If disk is slow, increase save interval
    increaseSaveInterval: boolean,
    diskSlowThreshold: 100 // MB/s
  }
}
```

## Technical Requirements

### Overview

This section defines the specific technical implementation requirements, including serialization systems, file I/O patterns, and acceptance criteria.

### Serialization System

#### Option A: Newtonsoft JSON.NET (Recommended for Unity/C#)

**Installation**:
```bash
# Via NuGet
Install-Package Newtonsoft.Json

# Via Unity Package Manager
# Add to Packages/manifest.json:
"com.unity.nuget.newtonsoft-json": "3.2.1"
```

**Usage Example**:
```csharp
using Newtonsoft.Json;
using System.IO;

public class SaveManager {
    public void SaveGame(SaveFile saveData, string filePath) {
        // Serialize with formatting
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        
        // Write to file
        File.WriteAllText(filePath, json);
    }
    
    public SaveFile LoadGame(string filePath) {
        // Read file
        string json = File.ReadAllText(filePath);
        
        // Deserialize
        SaveFile saveData = JsonConvert.DeserializeObject<SaveFile>(json);
        
        return saveData;
    }
}
```

**Custom Serialization Settings**:
```csharp
var settings = new JsonSerializerSettings {
    // Format for readability
    Formatting = Formatting.Indented,
    
    // Handle circular references
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    
    // Include type information for polymorphic types
    TypeNameHandling = TypeNameHandling.Auto,
    
    // Ignore null values (smaller file size)
    NullValueHandling = NullValueHandling.Ignore,
    
    // Custom converters
    Converters = {
        new Vector3Converter(),
        new QuaternionConverter()
    }
};

string json = JsonConvert.SerializeObject(saveData, settings);
```

#### Option B: Unity JsonUtility (Unity-Specific)

**Usage Example**:
```csharp
using UnityEngine;
using System.IO;

public class SaveManager {
    public void SaveGame(SaveFile saveData, string filePath) {
        // Serialize (pretty print)
        string json = JsonUtility.ToJson(saveData, true);
        
        // Write to file
        File.WriteAllText(filePath, json);
    }
    
    public SaveFile LoadGame(string filePath) {
        // Read file
        string json = File.ReadAllText(filePath);
        
        // Deserialize
        SaveFile saveData = JsonUtility.FromJson<SaveFile>(json);
        
        return saveData;
    }
}
```

**Limitations**:
- Cannot serialize dictionaries directly (requires wrapper class)
- Cannot serialize properties (only public fields)
- No circular reference handling

#### Option C: Binary Serialization (Production)

**Usage Example**:
```csharp
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager {
    public void SaveGame(SaveFile saveData, string filePath) {
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(filePath, FileMode.Create)) {
            formatter.Serialize(stream, saveData);
        }
    }
    
    public SaveFile LoadGame(string filePath) {
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(filePath, FileMode.Open)) {
            return (SaveFile)formatter.Deserialize(stream);
        }
    }
}
```

### Encryption Implementation (Optional)

#### AES-256 Encryption Example

```csharp
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class EncryptionHelper {
    private static readonly byte[] MASTER_KEY = Encoding.UTF8.GetBytes("YourSecure32ByteMasterKeyHere!!");
    
    public static string Encrypt(string plainText) {
        using (Aes aes = Aes.Create()) {
            aes.Key = MASTER_KEY;
            aes.GenerateIV();
            
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            
            using (MemoryStream msEncrypt = new MemoryStream()) {
                // Prepend IV to encrypted data
                msEncrypt.Write(aes.IV, 0, aes.IV.Length);
                
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
                        swEncrypt.Write(plainText);
                    }
                }
                
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }
    
    public static string Decrypt(string cipherText) {
        byte[] buffer = Convert.FromBase64String(cipherText);
        
        using (Aes aes = Aes.Create()) {
            aes.Key = MASTER_KEY;
            
            // Extract IV from beginning of buffer
            byte[] iv = new byte[aes.IV.Length];
            Array.Copy(buffer, 0, iv, 0, iv.Length);
            aes.IV = iv;
            
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            
            using (MemoryStream msDecrypt = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length)) {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}
```

### File I/O Patterns

#### Asynchronous File I/O

```csharp
using System.IO;
using System.Threading.Tasks;

public class AsyncFileIO {
    public static async Task WriteFileAsync(string filePath, string content) {
        byte[] encodedText = Encoding.UTF8.GetBytes(content);
        
        using (FileStream sourceStream = new FileStream(filePath,
            FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true)) {
            await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
        }
    }
    
    public static async Task<string> ReadFileAsync(string filePath) {
        using (FileStream sourceStream = new FileStream(filePath,
            FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true)) {
            
            byte[] buffer = new byte[sourceStream.Length];
            await sourceStream.ReadAsync(buffer, 0, (int)sourceStream.Length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
```

#### Safe File Writing (Atomic Operations)

```csharp
public class SafeFileWriter {
    public static void WriteSafe(string filePath, string content) {
        string tempPath = filePath + ".tmp";
        string backupPath = filePath + ".bak";
        
        try {
            // 1. Write to temporary file
            File.WriteAllText(tempPath, content);
            
            // 2. Verify temporary file
            if (!File.Exists(tempPath)) {
                throw new IOException("Failed to write temporary file");
            }
            
            // 3. Backup existing file (if exists)
            if (File.Exists(filePath)) {
                File.Copy(filePath, backupPath, overwrite: true);
            }
            
            // 4. Replace original with temp (atomic on most filesystems)
            File.Move(tempPath, filePath, overwrite: true);
            
            // 5. Delete backup if successful
            if (File.Exists(backupPath)) {
                File.Delete(backupPath);
            }
        }
        catch (Exception ex) {
            // Restore from backup if write failed
            if (File.Exists(backupPath)) {
                File.Copy(backupPath, filePath, overwrite: true);
            }
            throw new IOException("Failed to write save file", ex);
        }
        finally {
            // Cleanup
            if (File.Exists(tempPath)) {
                File.Delete(tempPath);
            }
        }
    }
}
```

### Directory Management

```csharp
using System;
using System.IO;

public class SaveDirectoryManager {
    private static string GetSaveDirectory() {
        string userDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string saveDir = Path.Combine(userDocuments, "ProtocolEMR", "Saves");
        
        // Create directory if it doesn't exist
        if (!Directory.Exists(saveDir)) {
            Directory.CreateDirectory(saveDir);
        }
        
        return saveDir;
    }
    
    private static string GetBackupDirectory() {
        string userDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string backupDir = Path.Combine(userDocuments, "ProtocolEMR", "Backups");
        
        if (!Directory.Exists(backupDir)) {
            Directory.CreateDirectory(backupDir);
        }
        
        return backupDir;
    }
    
    public static string GetSaveFilePath(string saveId) {
        return Path.Combine(GetSaveDirectory(), $"save_{saveId}.json");
    }
    
    public static string GetBackupFilePath(string saveId, DateTime timestamp) {
        string filename = $"backup_{saveId}_{timestamp:yyyyMMdd_HHmmss}.json.gz";
        return Path.Combine(GetBackupDirectory(), filename);
    }
}
```

### Compression Implementation

```csharp
using System.IO;
using System.IO.Compression;
using System.Text;

public class CompressionHelper {
    public static byte[] Compress(string text) {
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        
        using (MemoryStream ms = new MemoryStream()) {
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true)) {
                zip.Write(buffer, 0, buffer.Length);
            }
            return ms.ToArray();
        }
    }
    
    public static string Decompress(byte[] compressedData) {
        using (MemoryStream ms = new MemoryStream(compressedData)) {
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress)) {
                using (MemoryStream output = new MemoryStream()) {
                    zip.CopyTo(output);
                    return Encoding.UTF8.GetString(output.ToArray());
                }
            }
        }
    }
}
```

## Integration Points

### Integration with NPC System

**Reference**: See `npc-procedural.md` for detailed NPC framework

#### NPC State Persistence
- **Save**: Capture NPC status (alive/dead), position, hostility, dialogue progress
- **Load**: Respawn NPCs at saved positions with correct states
- **Relationship Tracking**: Save NPC relationship levels for persistent interactions

#### Integration Example
```csharp
// On save
saveData.npcState = NPCManager.GetAllNPCStates();

// On load
NPCManager.RestoreNPCStates(saveData.npcState);
```

### Integration with Mission System

**Reference**: See `ai-narrator-and-missions.md` for mission architecture

#### Mission State Persistence
- **Active Missions**: Save current objectives and progress
- **Mission Choices**: Track player decisions for branching narratives
- **Narrator State**: Save narrator event triggers and dialogue states

#### Integration Example
```csharp
// On save
saveData.missionState = MissionManager.GetMissionState();
saveData.narratorState = NarratorSystem.GetNarratorState();

// On load
MissionManager.RestoreMissionState(saveData.missionState);
NarratorSystem.RestoreNarratorState(saveData.narratorState);
```

### Integration with Procedural Generation

**Reference**: See `npc-procedural.md` for procedural generation strategy

#### Seed-Based Persistence
- **Save Generation Seed**: Store seed for each procedurally generated room
- **Room Modifications**: Save delta changes from procedural baseline
- **Regeneration on Load**: Use saved seed to regenerate identical rooms

#### Integration Example
```csharp
// On save
saveData.worldState.seed = ProceduralGenerator.GetCurrentSeed();
saveData.worldState.roomStates = ProceduralGenerator.GetRoomStates();

// On load
ProceduralGenerator.SetSeed(saveData.worldState.seed);
ProceduralGenerator.RestoreRoomStates(saveData.worldState.roomStates);
```

### Integration with Player Inventory

#### Inventory Serialization
- **Items**: Save item IDs, quantities, and metadata
- **Equipment**: Save equipped items and weapon customizations
- **Capacity**: Save current inventory capacity and encumbrance

### Integration with Settings System

#### Settings Persistence
- **Separate File**: Store settings in separate file from save data
- **Carry Over**: Settings persist across profiles and saves
- **Session Settings**: Some settings (difficulty) are per-save

## Prototype Deliverables

### Phase 1: Core Save/Load System (Week 1-2)

#### Deliverables
1. **Basic Save System**
   - Manual save functionality
   - JSON serialization
   - Single save slot
   - Player state capture (position, health, inventory)

2. **Basic Load System**
   - Load menu with save preview
   - Save file validation
   - Player state restoration

3. **Save File Structure**
   - Complete save data schema defined
   - JSON format implementation
   - Checksum generation

#### Acceptance Criteria
- ✓ Player can manually save game from pause menu
- ✓ Player can load saved game from main menu
- ✓ Player state (position, health, inventory) restored correctly
- ✓ Save file is human-readable JSON
- ✓ Save file includes checksum for integrity validation

### Phase 2: Multi-Slot & Auto-Save (Week 3)

#### Deliverables
1. **Multiple Save Slots**
   - 10 manual save slots
   - Save slot selection UI
   - Overwrite confirmation

2. **Auto-Save System**
   - Auto-save at mission checkpoints
   - Time-based auto-save (every 5 minutes)
   - 3 rotating auto-save slots

3. **Save Triggers**
   - Mission checkpoint triggers
   - Zone transition triggers
   - Time interval triggers

#### Acceptance Criteria
- ✓ Player can save to 10 different manual slots
- ✓ Game auto-saves at checkpoints and time intervals
- ✓ Auto-saves rotate, keeping 3 most recent
- ✓ Player receives visual feedback when auto-saving
- ✓ Player cannot overwrite auto-save slots manually

### Phase 3: Profile System (Week 4)

#### Deliverables
1. **Profile Management**
   - Profile creation UI
   - Profile selection UI
   - Profile deletion with confirmation
   - Multiple profiles support (unlimited)

2. **Profile Data**
   - Profile name and avatar
   - Profile creation date and playtime
   - Profile statistics tracking
   - Profile-specific save organization

3. **First Launch Experience**
   - Welcome screen
   - Profile creation prompt
   - Tutorial integration

#### Acceptance Criteria
- ✓ Player can create multiple profiles
- ✓ Player can switch between profiles from main menu
- ✓ Each profile has separate save files
- ✓ Profile deletion removes all associated saves
- ✓ First launch prompts profile creation

### Phase 4: Advanced Features (Week 5-6)

#### Deliverables
1. **Checkpoint System**
   - Pre-combat checkpoints
   - Pre-decision checkpoints
   - 5 rotating checkpoint slots

2. **Quick-Save**
   - Hotkey (F5) quick-save
   - Single quick-save slot
   - Quick-save feedback

3. **World State Persistence**
   - Object state tracking (doors, containers, collectibles)
   - NPC state tracking (status, position, relationship)
   - Mission state tracking (active, completed, choices)

4. **Backup System**
   - Automatic backup creation
   - 3 backups per save slot
   - Backup restoration UI

#### Acceptance Criteria
- ✓ Checkpoint saves created before major events
- ✓ Player can quick-save with hotkey (F5)
- ✓ World state (doors unlocked, items collected) persists correctly
- ✓ NPC states (alive/dead, hostile/friendly) persist correctly
- ✓ Mission progress and choices persist correctly
- ✓ Backups created automatically before overwriting saves
- ✓ Player can restore from backups in case of corruption

### Phase 5: Polish & Optimization (Week 7)

#### Deliverables
1. **Performance Optimization**
   - Asynchronous save operations
   - Loading screen with progress bar
   - Save/load time under performance targets

2. **Error Handling**
   - Corruption detection and recovery
   - Disk space checks
   - Load failure handling

3. **UI Polish**
   - Save thumbnails (optional screenshots)
   - Sort and filter options
   - Search functionality
   - Confirmation dialogs

4. **Session Recovery**
   - Crash detection on launch
   - Auto-save restoration offer
   - Session marker cleanup

#### Acceptance Criteria
- ✓ Save operations complete in under 100ms (non-blocking)
- ✓ Load operations complete in under 5 seconds
- ✓ No frame drops during save operations
- ✓ Corrupted saves detected and backups offered
- ✓ Insufficient disk space handled gracefully
- ✓ Crash recovery offers to restore last auto-save
- ✓ UI is responsive and intuitive
- ✓ All confirmation dialogs function correctly

## QA and Testing Checklist

### Functional Testing

#### Save System

| Test Case | Steps | Expected Result | Pass/Fail |
|-----------|-------|-----------------|-----------|
| **Manual Save** | Open pause menu → Select "Save Game" → Choose slot → Confirm | Save created successfully, confirmation shown | [ ] |
| **Auto-Save** | Play for 5+ minutes | Auto-save triggers, visual feedback shown | [ ] |
| **Quick-Save** | Press F5 during gameplay | Quick-save created, feedback shown | [ ] |
| **Checkpoint Save** | Trigger checkpoint event | Checkpoint save created automatically | [ ] |
| **Overwrite Save** | Save to occupied slot → Confirm overwrite | Save overwritten, confirmation shown | [ ] |
| **Cancel Save** | Begin save → Cancel before completion | Save cancelled, no file created/modified | [ ] |
| **Save During Combat** | Attempt manual save during combat | Save blocked, error message shown | [ ] |
| **Save During Cutscene** | Attempt save during cutscene | Save blocked, error message shown | [ ] |

#### Load System

| Test Case | Steps | Expected Result | Pass/Fail |
|-----------|-------|-----------------|-----------|
| **Load Save** | Main menu → Load Game → Select save → Confirm | Game loads, player at saved location | [ ] |
| **Load From In-Game** | Pause menu → Load Game → Select save → Confirm | Warning shown, game loads on confirmation | [ ] |
| **Load Corrupted Save** | Attempt to load corrupted file | Error detected, backup offered | [ ] |
| **Load Old Version Save** | Load save from older game version | Migration attempted, warning if incompatible | [ ] |
| **Cancel Load** | Begin load → Cancel before completion | Load cancelled, return to menu | [ ] |
| **Delete Save** | Select save → Delete → Confirm | Save deleted, list refreshed | [ ] |

#### Profile System

| Test Case | Steps | Expected Result | Pass/Fail |
|-----------|-------|-----------------|-----------|
| **Create Profile** | Main menu → Create Profile → Enter name → Select avatar | Profile created, becomes active | [ ] |
| **Switch Profile** | Main menu → Profile selector → Select different profile | Profile switched, correct saves shown | [ ] |
| **Delete Profile** | Profile manager → Select profile → Delete → Confirm | Profile and saves deleted | [ ] |
| **Profile Isolation** | Create save in Profile A → Switch to Profile B | Profile B does not show Profile A's saves | [ ] |
| **First Launch** | Launch game with no profiles | Welcome screen shown, profile creation prompted | [ ] |

### Data Integrity Testing

| Test Case | Steps | Expected Result | Pass/Fail |
|-----------|-------|-----------------|-----------|
| **Checksum Validation** | Manually edit save file → Attempt load | Corruption detected, error shown | [ ] |
| **Complete State Restoration** | Save → Modify world → Load | World restored to exact saved state | [ ] |
| **Player State** | Save → Change health/inventory → Load | Player health/inventory restored correctly | [ ] |
| **NPC State** | Save → Kill NPC → Load | NPC remains dead on load | [ ] |
| **Mission State** | Save → Complete objective → Load | Objective reverts to incomplete | [ ] |
| **World State** | Save → Open door → Collect item → Load | Door closed, item not collected on load | [ ] |
| **Seed Consistency** | Save in procedural room → Load | Room regenerates identically | [ ] |

### Performance Testing

| Test Case | Steps | Expected Result | Pass/Fail |
|-----------|-------|-----------------|-----------|
| **Save Time** | Trigger save → Measure time | Save completes in < 100ms (async) | [ ] |
| **Load Time** | Load save → Measure time | Load completes in < 5 seconds | [ ] |
| **Frame Rate Impact** | Monitor FPS during save | No frame drops (< 1 frame impact) | [ ] |
| **Memory Usage** | Monitor RAM during save/load | No memory leaks, usage within bounds | [ ] |
| **Disk Usage** | Check save file sizes | Save files reasonable size (< 5MB each) | [ ] |
| **Multiple Saves** | Create 50+ saves | No performance degradation | [ ] |

### Error Handling Testing

| Test Case | Steps | Expected Result | Pass/Fail |
|-----------|-------|-----------------|-----------|
| **Insufficient Disk Space** | Fill disk → Attempt save | Error message shown, save cancelled | [ ] |
| **Write-Protected Directory** | Make save dir read-only → Attempt save | Error message shown, alternative offered | [ ] |
| **Missing Save File** | Delete save file → Attempt load | Error message, file removed from list | [ ] |
| **Corrupted Save** | Corrupt save file → Attempt load | Corruption detected, backup offered | [ ] |
| **Power Loss Simulation** | Force quit during save | Session recovery triggers on next launch | [ ] |
| **Crash Recovery** | Force quit game → Relaunch | Interrupted session detected, auto-save offered | [ ] |

### Edge Case Testing

| Test Case | Steps | Expected Result | Pass/Fail |
|-----------|-------|-----------------|-----------|
| **Rapid Saves** | Trigger 10 saves rapidly | All saves queued and executed correctly | [ ] |
| **Save During Load** | Begin load → Attempt save | Save blocked or queued | [ ] |
| **Load During Save** | Begin save → Attempt load | Operation serialized correctly | [ ] |
| **Maximum Save Slots** | Create 10 manual + 3 auto + 5 checkpoint saves | All saves managed correctly | [ ] |
| **Very Long Playtime** | Load save with 100+ hours playtime | Playtime displays correctly, no overflow | [ ] |
| **Special Characters in Profile Name** | Create profile with special chars (émojis, ñ) | Name saved and displayed correctly | [ ] |

### Platform-Specific Testing

| Test Case | Platform | Expected Result | Pass/Fail |
|-----------|----------|-----------------|-----------|
| **Windows Save Location** | Windows | Saves to %USERPROFILE%\Documents\ProtocolEMR\ | [ ] |
| **macOS Save Location** | macOS | Saves to ~/Documents/ProtocolEMR/ | [ ] |
| **Linux Save Location** | Linux | Saves to ~/.local/share/ProtocolEMR/ | [ ] |
| **Path with Spaces** | All | Handles user paths with spaces correctly | [ ] |
| **Non-English Username** | All | Handles non-ASCII usernames correctly | [ ] |

### UI/UX Testing

| Test Case | Steps | Expected Result | Pass/Fail |
|-----------|-------|-----------------|-----------|
| **Visual Feedback** | Trigger save | Icon and text feedback shown for 2 seconds | [ ] |
| **Audio Feedback** | Trigger save | Confirmation sound plays | [ ] |
| **Progress Bar** | Load save | Progress bar updates smoothly | [ ] |
| **Sort Saves** | Use sort dropdown | Saves sort correctly by selected criteria | [ ] |
| **Filter Saves** | Use filter dropdown | Only filtered save types shown | [ ] |
| **Search Saves** | Enter search term | Matching saves highlighted | [ ] |
| **Confirmation Dialogs** | Attempt delete/overwrite | Clear confirmation dialog shown | [ ] |

---

## Summary

This document provides a comprehensive protocol for implementing a robust save/load and account management system for Protocol EMR. The system prioritizes:

1. **Data Integrity**: Checksums, validation, and backup systems ensure player progress is never lost
2. **User Experience**: Multiple profiles, clear UI, and intuitive workflows make save management effortless
3. **Performance**: Asynchronous operations and optimization ensure zero gameplay impact
4. **Flexibility**: Multiple save types (auto, manual, checkpoint, quick) provide player control
5. **Recovery**: Crash detection, corruption handling, and backup restoration protect against data loss
6. **Scalability**: Versioning and migration systems support long-term game updates

The phased implementation plan ensures core functionality is delivered early, with advanced features added incrementally. Comprehensive QA testing verifies system reliability across all scenarios.

---

**Document Version**: 1.0.0  
**Last Updated**: January 2024  
**Related Documents**:
- `npc-procedural.md` - NPC state persistence integration
- `ai-narrator-and-missions.md` - Mission state integration
- `audio-assets.md` - Audio feedback specifications
