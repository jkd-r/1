# Tutorial and Onboarding System

## Overview

The Tutorial and Onboarding system introduces new players to Protocol EMR's core mechanics through a carefully paced, immersive experience that minimizes friction while establishing atmosphere and narrative context. The system balances mandatory learning with optional skip paths, adaptive difficulty-based guidance, and comprehensive accessibility support to serve diverse player skill levels and preferences.

## Table of Contents

1. [First Launch Flow](#first-launch-flow)
2. [Interactive Tutorial](#interactive-tutorial)
3. [Tutorial Features](#tutorial-features)
4. [Onboarding Cinematic](#onboarding-cinematic)
5. [Difficulty Introduction](#difficulty-introduction)
6. [Controls Help System](#controls-help-system)
7. [Adaptive Tutorial System](#adaptive-tutorial-system)
8. [New Game+ Onboarding](#new-game-onboarding)
9. [Accessibility in Tutorial](#accessibility-in-tutorial)
10. [Technical Requirements](#technical-requirements)
11. [Implementation Details](#implementation-details)
12. [QA and Acceptance Criteria](#qa-and-acceptance-criteria)

---

## First Launch Flow

The first launch flow is the critical path that establishes player identity, difficulty context, and narrative tone before actual gameplay begins.

### Flow Sequence

| Stage | Duration | Component | Action |
|-------|----------|-----------|--------|
| 1 | 3-5s | Splash Screen | Display logo with ambient audio |
| 2 | 45-90s | Profile Creation | Name entry, difficulty selection |
| 3 | 120-180s | Story Intro Cinematic | Narrative context setting |
| 4 | 10-15s | Tutorial Entry Point | Optional skip or proceed |
| 5 | Variable | Tutorial Missions | Interactive learning (can skip) |
| 6 | 10-20s | Tutorial Completion | Briefing before main game |

### Splash Screen

**Purpose**: Establish visual identity and allow loading of core systems

**Implementation**:
- Display Protocol EMR logo (centered, 16:9 aspect)
- Subtle ambient audio: low-frequency hum suggesting technological presence
- Subtle fade-in over 2 seconds
- Hold for 3-5 seconds before transition
- "Press any key to continue" (optional, can auto-advance)

**Technical Details**:
- Allow skipping with any input for accessibility
- Preload next scene assets during splash duration
- Play introductory audio bed (44.1kHz, stereo, 5-10 seconds)

### Profile Creation Screen

**Purpose**: Establish player identity and gameplay difficulty

**Layout**:
```
┌─────────────────────────────────────┐
│    Welcome to Protocol EMR          │
│                                     │
│  Enter Your Name:                   │
│  [________________] (max 20 chars)  │
│                                     │
│  Select Difficulty:                 │
│  ○ Easy                             │
│  ○ Normal (Recommended)             │
│  ○ Hard                             │
│  ○ Nightmare                        │
│                                     │
│  [Cancel]  [Start Game]             │
└─────────────────────────────────────┘
```

**Features**:
- Name field with alphanumeric support
- Difficulty preview tooltips (hover for description)
- Default selection: Normal
- Cancel returns to main menu
- Validation prevents empty names

**Saved Data**:
- Store player name for UI display
- Store difficulty selection (affects AI behavior, resource spawns, hint frequency)
- Create new save slot automatically

### Story Intro Cinematic

**Purpose**: Establish atmosphere, introduce the facility concept, and create emotional investment

**Visual Style**:
- Stylized, high-contrast sci-fi aesthetic
- Heavy emphasis on environmental storytelling
- Minimal dialogue (max 2-3 lines total)

**Story Beats** (120-180 seconds total):

1. **Black screen** (5s): Audio only
   - Sound design: Muffled alarms, distant mechanical sounds
   - Establishing disorientation and confinement

2. **Fade to white** (10s):
   - Visual: POV looking up at sterile facility ceiling
   - Camera trembling slightly (feeling of waking/disorientation)
   - HUD elements flickering on like neural interface initialization

3. **First-person view establishment** (45s):
   - Slow pan across high-tech facility interior
   - Observe: Clean lab environment, advanced technology, locked doors
   - Establish visual language: clean/functional aesthetic
   - Audio: Electrical hum, occasional beep of inactive systems

4. **Discovery moment** (20s):
   - Player reaches window/monitor showing red emergency alerts
   - Visual realization: something is wrong
   - Audio: Emergency system activating, doors sealing
   - Brief text overlay: "EMERGENCY LOCKDOWN ACTIVATED"

5. **Introduction to "Unknown"** (30s):
   - Phone buzzes/notification sound
   - Message appears: "You're awake. There's not much time."
   - Establish first contact with the AI narrator
   - Create sense of urgency without panic

6. **Atmospheric closure** (15s):
   - Pull back to show full player perspective in confined room
   - Establish sense of isolation and confinement
   - Audio builds to ambient tension
   - Final message: "Let me help you get out of here."

**Cinematics Skip Option**:
- Player can skip with ESC key (displayed on screen)
- Shows 5-second countdown before auto-advance
- Skipping goes directly to first playable moment in tutorial

## Interactive Tutorial

The interactive tutorial teaches all core mechanics through hands-on experience with progressive complexity and optional skip at any point.

### Tutorial Structure

#### Movement Tutorial (2-3 minutes)

**Objective**: Learn WASD movement and exploration

**Setup**:
- Player in confined starting room
- Clear path ahead
- Visual markers (floor lights) indicating forward path

**Progression**:
1. **Introduction**
   - HUD hint appears: "Use WASD to move forward"
   - Arrow indicator pointing to WASD keys
   - Highlight outline around movement path

2. **Forward movement**
   - Player must move 10 meters forward
   - Positive feedback: "Good" / movement audio cue
   - Next hint reveals: "Hold Shift to sprint"

3. **Sprint capability**
   - Optional sprint tutorial
   - Encourages using Shift for faster movement
   - Completion check after 15 meters of sprinting

4. **Strafing introduction**
   - Hint: "Use A and D to strafe left/right"
   - Room narrows slightly, requiring lateral movement
   - Confidence building: Movement feels smooth and responsive

5. **Completion**
   - "Movement tutorial complete - well done"
   - Move forward to next learning area

**Unknown AI Commentary**:
- "You're moving pretty well for someone who just woke up."
- "These corridors are designed for efficient navigation."

#### Look and Camera Control Tutorial (2-3 minutes)

**Objective**: Learn mouse look and camera awareness

**Setup**:
- Player in medium-sized room with interactive elements at different heights
- Positioned facing a wall with notable features

**Progression**:
1. **Introduction**
   - Hint: "Move your mouse to look around"
   - Encourage full 360° view
   - Visual indicator showing camera angle

2. **Vertical look**
   - Hint: "Look up (move mouse forward)"
   - Highlight element on ceiling worth observing
   - Learning: Awareness of vertical space

3. **Horizontal scan**
   - Multiple objects scattered around room
   - Hint: "Scan the room for interactive elements"
   - Builds pattern recognition

4. **Finding interactable objects**
   - "Notice the glowing indicators on certain objects"
   - Objects begin highlighting when looked at directly
   - Creates foundation for interaction system

5. **Completion**
   - "Camera control mastered"
   - Smooth camera movement without restrictions

**Unknown AI Commentary**:
- "Your neural interface is calibrating your visual input."
- "Everything you see is important - look carefully."

#### Interaction Tutorial (2-3 minutes)

**Objective**: Learn object interaction mechanics

**Setup**:
- Room with 3-4 interactive objects of increasing complexity
- Objects clearly marked with highlights
- Close proximity for safe learning

**Progression**:
1. **Introduction to interaction**
   - Hint: "Press E to interact with highlighted objects"
   - Highlight first object: simple panel/button
   - Visual reticle shows interaction target

2. **First interaction**
   - Player presses E near highlighted object
   - Feedback: Light animation, satisfying sound, progress notification
   - Example: "Panel activated - good"

3. **Object pickup**
   - New object appears: a simple item (e.g., access card, battery)
   - Hint: "Press E to pick this up"
   - Item should be small, clearly valuable
   - Inventory confirmation: "Item acquired: [Name]"

4. **Complex interaction**
   - Terminal or device with multiple interaction points
   - Hint: "Some objects require multiple interactions"
   - Discovery: This is how puzzles/systems work

5. **Completion**
   - "Interaction tutorial complete"
   - Player now confident with E-key mechanics

**Unknown AI Commentary**:
- "That device looks important. The facility runs on these systems."
- "You're learning quickly."

#### Inventory Tutorial (1-2 minutes)

**Objective**: Learn inventory management

**Setup**:
- Player has 3-4 items from previous interactions
- Clean inventory interface visible when opened

**Progression**:
1. **Opening inventory**
   - Hint: "Press I to open your inventory"
   - Inventory UI slides onto screen
   - Shows all collected items with icons and names

2. **Item examination**
   - Hint: "Click items to examine them"
   - Each item shows description/stats
   - Learning: Some items are key items, others are consumables

3. **Item categories**
   - UI shows items organized by type: Tools, Key Items, Consumables
   - Hint: "Stay organized - inventory management matters"
   - Demonstrates inventory limitation (max 20 slots initially)

4. **Equipment slots**
   - Optional: Show quick-access slots for commonly used items
   - Can equip items for faster access (if applicable)

5. **Completion**
   - "Inventory mastered"
   - Inventory stays accessible throughout game

**Unknown AI Commentary**:
- "That inventory interface is synced with your neural implant."
- "You'll collect many things - organize wisely."

#### Phone/Communication Tutorial (2-3 minutes)

**Objective**: Learn phone interface and communication with Unknown

**Setup**:
- Player has completed movement, look, interaction tutorials
- Phone notification sound plays
- Message from "Unknown" appears

**Progression**:
1. **Phone access**
   - Hint: "Press C to open your phone"
   - Phone UI appears showing conversation with Unknown
   - Previous messages visible (from cinematic)

2. **Message reading**
   - Hint: "Read the latest message"
   - Message contains a hint or mission-relevant information
   - Learn: Phone is primary communication channel

3. **Replying (optional)**
   - If dialogue options available: "Select a response"
   - Otherwise: "Your message was received"
   - Feedback: Response appears in conversation

4. **Message history**
   - Hint: "Scroll up to read previous messages"
   - Learn: Full conversation thread is accessible
   - Builds sense of ongoing story

5. **Completion**
   - "Communication link established"
   - Phone accessible anytime (except cutscenes)

**Unknown AI Commentary** (via phone):
- "This channel will be our lifeline. Check it frequently."
- "I'm here to help, but I'm still learning about this situation myself."

#### Combat Tutorial (3-5 minutes)

**Objective**: Learn combat mechanics safely

**Setup**:
- Safe training area with clearly marked safety zone
- One passive training dummy or NPC (non-hostile)
- Minimal resource pressure

**Progression**:
1. **Combat stance**
   - Hint: "Approach the training dummy carefully"
   - Explain: No pressure here, this is practice
   - Visual: Combat readiness indicators

2. **Basic attack**
   - Hint: "Press Left Mouse Button to attack"
   - Attack the dummy (melee or ranged depending on game)
   - Feedback: Obvious damage indication, sound effect
   - Learning: Attacks have cooldowns/stamina usage

3. **Damage feedback**
   - Visual: Health bar for dummy decreases
   - Audio: Impact sound, satisfying feedback
   - Hint: "Target health is decreasing - keep attacking"

4. **Defensive mechanics**
   - Hint: "Press Space to dodge"
   - Teach evasion (if applicable)
   - Or: "Press F to block incoming damage"
   - Learn: Defense is equally important as offense

5. **Resource management**
   - Hint: "Your stamina is limited - pace your attacks"
   - Demonstrate: Stamina meter depletes, attacks become slower
   - Learn: Combat requires tactical thinking

6. **Completion**
   - Dummy defeated or tutorial ends
   - "Combat basics learned - you can handle yourself"
   - Clear that actual combat will be more challenging

**Unknown AI Commentary**:
- "Your implant includes basic combat protocols."
- "You won't always win through fighting. Remember that."

#### Puzzle Tutorial (2-4 minutes)

**Objective**: Learn puzzle-solving approach

**Setup**:
- Simple environmental puzzle (3-step solution)
- All required tools available
- Clear visual cues for solution

**Progression**:
1. **Puzzle introduction**
   - Hint: "This device appears to require three components"
   - Introduce: Observation-based puzzle solving
   - Visual: Highlight puzzle components

2. **Finding first component**
   - Hint: "Look around - components might be hidden"
   - Nearby component clearly visible when searched
   - Interaction: Pick up first component

3. **First piece placement**
   - Hint: "Insert the first component"
   - Obvious placement location
   - Feedback: Component slots in place with satisfying sound
   - Progress indication: "1 of 3 components installed"

4. **Second and third components**
   - Hints become less specific: "Find the remaining components"
   - Encourage independent exploration
   - Components visible but require observation

5. **Final solution**
   - All components placed
   - Device activates: "System online"
   - Reward: Access granted, next area opens
   - Learning: Puzzles have logical, discoverable solutions

6. **Completion**
   - "Puzzle-solving tutorial complete"
   - Confidence building: "You solved it on your own"

**Unknown AI Commentary**:
- "The facility's puzzles follow certain logic patterns."
- "Observe, analyze, and try different approaches. Problem-solving is key to escape."

### Tutorial Feature Integration

All tutorials include:

- **Contextual Hints**: Appear automatically at optimal teaching moments
- **Visual Highlights**: Interactive objects glow and show interaction keys
- **Objective Markers**: Current objective displayed clearly
- **Progress Indication**: Clear feedback on completion status
- **Skip Options**: "Skip Tutorial Step" button always visible
- **Difficulty Alignment**: Hints and difficulty scale with selected difficulty
- **Positive Reinforcement**: Encouragement messages for completed actions

---

## Tutorial Features

### Contextual Hints System

Hints appear automatically based on player behavior and time:

#### Hint Triggering

| Trigger Type | Timing | Example |
|--------------|--------|---------|
| **Action-based** | On player action initiation | Movement starts → "Use WASD to move" |
| **Idle timeout** | After 30-45s without progress | Standing still near puzzle → "Try examining nearby objects" |
| **Location-based** | Player enters hint zone | Approaches terminal → "Interactive devices have E prompt" |
| **Tutorial milestone** | Each tutorial stage reaches target | Movement tutorial complete → "Next: Learn to look around" |
| **Difficulty-based** | Scales with selected difficulty | Easy: More hints; Nightmare: Fewer hints |

#### Hint Content Guidelines

- **Clear**: One idea per hint (max 2 sentences)
- **Actionable**: Always include what to do
- **Non-spoiler**: Never reveal puzzle solutions
- **Encouraging**: Use positive, supportive language
- **Progressive**: Later hints assume previous learning

### Interactive Object Highlighting

#### Visual Highlighting System

**Default state**:
- Interactive objects have subtle glow (soft outline)
- Pulsing slightly to draw attention without distraction

**On look-at (player targeting)**:
- Glow intensifies
- Interaction key appears (E, F, etc.)
- Object name/type displayed in reticle
- Description appears: "Locked Door", "Access Panel", "Item: Battery"

**On hover (mouse near object)**:
- Highlight color changes to indicate interaction type
- Green: Usable/collectible
- Blue: Readable/information
- Red: Dangerous/locked
- Yellow: Special interaction

#### Highlighting Customization

Players can toggle highlighting via settings:
- **Full**: All interactive objects highlighted
- **Essential**: Only critical path objects highlighted
- **Minimal**: Only objects being directly looked at
- **None**: No highlighting (advanced/hardcore mode)

### Objective Markers

#### Objective Display

**HUD Objective Panel**:
```
Current Objective:
✓ Learn to move (Complete)
→ Practice sprinting (In Progress)
  Collect the access card
  Reach the corridor exit
```

**Features**:
- Current objective highlighted in brighter color
- Previous objectives shown as completed (with checkmark)
- Upcoming objectives visible (for context)
- Optional: Mini-map marker showing direction to objective

#### Objective Management

- **Auto-tracking**: Objectives update automatically
- **Manual toggle**: Player can collapse/expand objectives with hotkey (Tab)
- **Difficulty scaling**: Hard/Nightmare modes show fewer hints in objectives
- **Context hints**: Objectives include helpful tips when selected

### Unknown AI Guidance

"Unknown" (the AI narrator) provides tutorial guidance through:

1. **Phone messages**: Tutorial hints arrive via in-game phone
2. **HUD text**: Critical instructions appear on-screen
3. **Voice-over**: Important moments get audio narration
4. **Ambient commentary**: Personality-driven observations about player actions

#### Unknown's Tutorial Personality

- **Supportive**: Encouraging tone ("You're doing great")
- **Informative**: Explains game systems clearly
- **Mysterious**: Hints at larger narrative mystery
- **Practical**: Focuses on survival and problem-solving
- **Growth-oriented**: Celebrates player learning progression

### Step-by-Step Skip Functionality

Players can skip individual tutorial steps at any time:

**Skip Interface**:
```
┌─────────────────────┐
│ [Skip This Step]    │
│ [Skip All Tutorial] │
└─────────────────────┘
```

**Behavior**:
- Skip This Step: Move to next tutorial lesson immediately
- Skip All Tutorial: Enter main game with mini briefing
- Skipped content remains accessible via Help menu

### Skipped Content Recovery

If player skips tutorials, they can still access learning:

- **In-Game Help**: Always-accessible tutorial videos/text
- **Pause Menu**: "Tutorial" submenu with all lessons
- **Unknown Tips**: Unknown offers hints for skipped mechanics when player struggles
- **No Penalty**: Skipping doesn't affect difficulty or progression

---

## Onboarding Cinematic

The onboarding cinematic establishes atmosphere, narrative framing, and emotional context before tutorials begin.

### Cinematic Objectives

- Establish the facility as a character
- Create urgency without panic
- Introduce player as test subject (without revealing everything)
- Create mystery about what happened
- Frame Unknown as potentially trustworthy guide
- Establish visual and audio language for game

### Cinematic Story

#### Act 1: Awakening (30 seconds)

**Visual Language**: Clinical, high-tech, disorienting

- Black fade
- Slowly brighten: White lights flickering on
- POV: Looking upward at sterile ceiling
- Camera trembles slightly—suggestion of confusion/waking
- Ambient sounds: Distant machinery, electrical hum
- Text overlay: "Consciousness reboot protocol initiated"

**Narrative Goal**: Establish waking in unknown environment

#### Act 2: First Awareness (40 seconds)

**Visual Language**: Gradually expanding perspective

- POV moves forward, looking around facility interior
- Observe: Clean lab aesthetic, advanced technology, inactive displays
- Focus on visual contrast: Pristine design vs. obvious emergency state
- Empty corridors—isolation emphasized
- Environmental storytelling: Scattered papers, knocked-over equipment, emergency exits marked
- Audio: Electrical systems humming, occasional beep of warning systems
- Text overlay: "Neural interface synchronization: 45%"

**Narrative Goal**: Player realizes they're in advanced facility, something is wrong

#### Act 3: Discovery (30 seconds)

**Visual Language**: The moment of realization

- POV reaches window or large display screen
- Screen shows: Red emergency alerts, facility schematic with zones marked "SEALED"
- Realization moment: The facility is in emergency lockdown
- Close-up: Security protocols displayed—"ALL EXITS SEALED"
- Emergency lighting flashes red once—dramatic punctuation
- Audio: System alarm tone (not panicked, just authoritative)

**Narrative Goal**: Player understands immediate danger—they're trapped

#### Act 4: First Contact (40 seconds)

**Visual Language**: Introduction to Unknown

- Phone notification vibrates/chimes
- Message notification appears: "New Message"
- Phone screen shows conversation opening
- Message from "Unknown": "You're awake. There's not much time."
- Another message: "The facility is in emergency protocol. I'm trying to help."
- Message: "Your neural implant is now synchronized. We can communicate."
- Sense of connection: Someone/something knows they're here

**Narrative Goal**: Establish Unknown as mysterious guide; create partnership feeling

#### Act 5: Atmosphere and Tension (20 seconds)

**Visual Language**: Building dread

- Camera pulls back to reveal full room: Small, confined, one exit
- Isolation is the central visual concept
- Ambient audio builds: Lower frequency tones suggesting danger
- Unknown's final message: "There's something else here. Something that shouldn't be."
- Sound cue: Distant noise, suggest something moving in facility
- Brief flash of red warning light

**Narrative Goal**: Establish that escape won't be simple; introduce danger

#### Act 6: Transition to Tutorial (20 seconds)

**Visual Language**: From cinematic to gameplay

- Fade from cinematic camera to first-person gameplay
- UI elements fade in: Health, objective marker, interaction hints
- Player now in control
- First interactive element highlighted
- Unknown's message: "Get out of this room. That's the first step."

**Narrative Goal**: Smooth transition from story to agency; player now in control

### Cinematic Technical Specifications

| Aspect | Specification | Notes |
|--------|---------------|-------|
| **Total Duration** | 120-180 seconds | Skippable after 5s intro |
| **Resolution** | 1920x1080 (16:9) | Supports 2560x1440 |
| **Frame Rate** | 60 FPS | Match game runtime |
| **Audio** | Stereo, 48kHz | Dynamic mixing with SFX |
| **Skip Key** | ESC (with 5s timer) | Shows "Press ESC to skip" |
| **Text Readability** | 36-48pt font | High contrast white on black |
| **Color Grading** | Cool tones, desaturated | Sci-fi clinical aesthetic |

### Cinematic Production

**Video Approach**:
- Real-time rendered in-engine cinematics (not pre-recorded)
- Dynamic camera movements using animation tracks
- Lighting changes scripted and automated
- Particle effects for environmental storytelling
- High-quality rendering pipeline for polish

**Alternative Approach** (if real-time problematic):
- High-quality pre-rendered video (1920x1080, H.264)
- Stored as streamable asset
- Requires 200-300MB storage per resolution

---

## Difficulty Introduction

Each difficulty level has distinct onboarding characteristics affecting hints, resource availability, and guidance frequency.

### Difficulty Tier Comparison

| Aspect | Easy | Normal | Hard | Nightmare |
|--------|------|--------|------|-----------|
| **Hint Frequency** | Every 30s if idle | Every 45s if idle | Every 90s if idle | Manual only |
| **Highlighted Objects** | All interactive | Key path + nearby | Only direct targets | None (unlockable) |
| **Objective Details** | Specific steps | General direction | Vague goals | Self-discovered |
| **Combat Difficulty** | Very forgiving | Balanced | Challenging | Punishing |
| **Resource Abundance** | Abundant | Moderate | Scarce | Very scarce |
| **Unknown Tone** | Supportive/Cheerful | Professional | Terse | Silent/Hostile |
| **Tutorial Skip** | Encouraged | Optional | Not suggested | Assumed |
| **Death Consequence** | Instant respawn | Last checkpoint | Checkpoint lost progress | Permanent death |

### Easy Mode Onboarding

**Philosophy**: Remove all obstacles to learning

**Features**:
- All interactive objects have bright, obvious highlights
- Objective step-by-step guidance: "Do X, then do Y"
- Unknown sends frequent encouraging messages
- Combat training: Enemies won't fight back during tutorial
- Unlimited time for puzzle solving
- Frequent positive feedback: "Great job!", "Well done!"
- Tutorial can't be failed—always completion guaranteed

**Unknown's Voice**:
- Warm, encouraging, mentor-like
- Explains everything clearly
- Never lets player get lost
- Available for questions (via phone/hints)

**Example Hint Flow** (Easy):
1. Player looks lost → Unknown sends: "Try examining that glowing object"
2. After 30s still idle → Unknown: "Press E to interact"
3. After 60s still idle → Unknown: "That's the access panel. It opens the door."

### Normal Mode Onboarding

**Philosophy**: Balance learning with challenge

**Features**:
- Interactive objects highlighted, but less obviously
- Objective guidance: General direction, some ambiguity
- Unknown sends helpful but not constant guidance
- Combat training: Enemies basic but combative
- Puzzle hints available but require asking (via Unknown)
- Time pressure minimal but present
- Mix of positive feedback and neutral tone

**Unknown's Voice**:
- Professional, efficient, occasional personality
- Answers questions but doesn't over-explain
- Respectful of player's problem-solving attempts
- Occasional hints about larger story

**Example Hint Flow** (Normal):
1. Player looks lost → Optional: "Hints available" indicator
2. After 60s idle → Unknown: "There might be answers nearby"
3. After 90s idle → Unknown: "Check your interactive surroundings"

### Hard Mode Onboarding

**Philosophy**: Expect player competence

**Features**:
- Only essential objects highlighted in tutorial
- Objectives vague: "Escape the room" (not "Go through door")
- Unknown sends rare guidance
- Combat training: Enemies strategic, real threat
- Puzzle solving: Minimal hints, expect experimentation
- Tutorial skipping not discouraged but assumed completion
- Neutral feedback: No excessive praise

**Unknown's Voice**:
- Terse, business-like, minimal personality
- Only communicates if critical
- Expects player to figure out systems
- Occasional sardonic commentary

**Example Hint Flow** (Hard):
1. Player idle → No automatic hints
2. After 120s idle → Optional: "Need guidance?" prompt appears
3. Player must request help actively via Unknown (phone message)

### Nightmare Mode Onboarding

**Philosophy**: Expect expert players

**Features**:
- No object highlighting (can be toggled on via settings)
- Objectives: Player must deduce goals from environment
- Unknown: Silent by default, sends no hints
- Combat training: Dangerous, real combat scenario
- Puzzle solving: No guidance, full experimentation
- Assumes tutorial completion; offers optional brief recap
- Minimal feedback; consequences for mistakes
- Tutorial can be skipped entirely

**Unknown's Voice**:
- Does not communicate during tutorial
- Only critical alerts during gameplay
- Potentially hostile or indifferent tone
- Assumes player knows what they're doing

**Example Hint Flow** (Nightmare):
1. Player idle → Nothing happens
2. After 180s idle → Still nothing (unless in mortal danger)
3. Player must discover systems independently

### Difficulty Transitions

If player switches difficulty mid-game:

- **Easy → Normal/Hard/Nightmare**: Hints reduce gradually over next 10 minutes
- **Hard/Nightmare → Easy/Normal**: Hints increase but not retroactively
- **Cinematic replay**: Available from main menu (difficulty doesn't affect cinematic)

---

## Controls Help System

### Help Menu Overview

The controls help system is always accessible and context-aware:

**Access**:
- Pause menu → "Controls" option
- In-game hotkey: F1
- Never blocks gameplay access

**Content**:
- Current keybindings
- Controller layout
- Contextual actions
- Rebinding interface

### Keybinding Display

#### Keyboard Layout

```
    [Tab]  [Objective] [Help]  [Phone]
            Menu       ↓       ↓
           WASD        I       C
         Move    Open Inv  Open Phone
    [Shift]     [E]           [Q]
    Sprint    Interact      Equip/Use

[Space]       [Left Click]    [Right Click]
Dodge/Jump    Attack/Use       Look/Aim

[Esc] Pause   [F1] Help   [M] Map    [V] Voice
```

#### Display Features

- **Customizable**: Show current key or show all available commands
- **Visual clarity**: Keys shown as button icons
- **Color coding**: Different action types have different colors
  - Movement: Blue
  - Interaction: Green
  - Combat: Red
  - UI: Yellow
  - Utility: Gray

### Controller Layout Display

```
┌─────────────────────────────────────┐
│        XBOX / PLAYSTATION LAYOUT    │
│                                     │
│              [Y/Δ]                  │
│              Equip                  │
│   [X/□]              [B/○]          │
│   Previous      Next Item           │
│              [A/×]                  │
│            Interact                 │
│                                     │
│  [LB/L1]           [RB/R1]          │
│  Attack            Dodge            │
│                                     │
│  [LStick]          [RStick]         │
│  Move              Look             │
│                                     │
│  [Start]           [Back/Select]    │
│  Pause             Menu             │
└─────────────────────────────────────┘
```

### Contextual Actions

Help system displays relevant actions based on current gameplay state:

**In Room with Puzzle**:
- E: Examine
- Left Click: Interact
- I: Open Inventory (to manage items)

**In Combat**:
- Left Click: Attack
- Right Click: Aim/Block
- Space: Dodge
- Q: Use equipped item

**In Conversation**:
- Number Keys: Select dialogue option
- Space: Advance dialogue
- Right Click: Skip to end

**In Menu**:
- Arrow Keys: Navigate
- Enter: Select
- Esc: Back

### Keybinding Customization

Players can rebind keys in settings:

**Interface**:
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
KEYBINDING CUSTOMIZATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Movement
  Forward:    [W]       [Change]
  Left:       [A]       [Change]
  Right:      [D]       [Change]
  Sprint:     [Shift]   [Change]

Interaction
  Interact:   [E]       [Change]
  Examine:    [Q]       [Change]

Combat
  Attack:     [M1]      [Change]
  Block:      [M2]      [Change]
  Dodge:      [Space]   [Change]

[Reset to Default] [Save] [Cancel]
```

**Features**:
- Conflict detection: Warns if rebinding creates duplicate key
- Preset layouts: Option to load common layouts (AZERTY, Dvorak, etc.)
- Save multiple profiles
- Controller support: Separate controller keybindings

### Help Content Accessibility

Help system includes:

1. **Videos**: Short clips demonstrating each action
2. **Descriptions**: Text explanation of what each key does
3. **Examples**: "E to interact with this control panel"
4. **Search**: Find actions by name or key
5. **Favorites**: Mark frequently-used actions for quick access

---

## Adaptive Tutorial System

The adaptive system monitors player behavior and modifies guidance dynamically.

### Struggle Detection

The system monitors for signs of player difficulty:

#### Struggle Signals

| Signal | Detection | Response |
|--------|-----------|----------|
| **Multiple Deaths** | Player dies 2+ times in sequence | Offer additional hints, reduce enemy difficulty |
| **Time on Puzzle** | Player working on puzzle >5 min without progress | "Need a hint?" prompt, specific guidance options |
| **Idle Duration** | Player standing still >2 minutes | Unknown checks in: "Everything okay?" |
| **Repeated Failures** | Same action tried 5+ times unsuccessfully | Suggest alternative approach |
| **Backtracking** | Revisiting same area 3+ times in short period | Objective clarification offered |
| **Avoidance** | Player avoiding obvious objective area | Gentle guidance toward correct path |

#### Response Escalation

1. **Subtle**: Highlight relevant object, mention in objectives
2. **Direct**: Unknown sends specific hint
3. **Detailed**: Unknown explains system in detail
4. **Assisted**: Option to have Unknown guide through step-by-step
5. **Skip**: Option to skip this tutorial section

### Skip Detection and Response

If player skips tutorials, system adapts:

#### Skipped Tutorial Tracking

- Record which tutorials were skipped
- Monitor how player performs without that knowledge
- Trigger automatic tips when skipped mechanics become relevant

#### In-Game Tip System

When player encounters situation requiring skipped knowledge:

**Example 1: Skipped Combat Tutorial**
- Player encounters enemy without combat experience
- Unknown sends: "You'll need to fight. Left click to attack, right click to block."
- Marker appears showing combat UI: "Stamina depletes with attacks"
- Optional training combat with pause available

**Example 2: Skipped Puzzle Tutorial**
- Player approaches puzzle without tutorial experience
- Unknown sends: "This looks like a puzzle. Examine all components carefully."
- Hint system more aggressive: Offers hints more frequently
- Can request step-by-step guidance

**Example 3: Skipped Inventory Tutorial**
- Player picks up first item without tutorial
- Inventory opens automatically with guidance
- Unknown explains: "That's your inventory. Press I to manage items."
- Quick tutorial before continuing

### Contextual Unknown Assistance

Unknown learns what the player needs and adapts communication:

#### Behavioral Analysis

- **Problem solver**: Fewer hints, let player figure out answers
- **Needs guidance**: More frequent tips, step-by-step help
- **Visual learner**: Emphasize highlights and markers
- **Exploratory**: Reward exploration, less prescriptive guidance

#### Difficulty Recalibration

If game is too hard:
- Unknown suggests: "Try easy mode or take a break"
- Reduce enemy health/damage mid-fight
- Increase hints and guidance frequency

If game is too easy:
- Unknown suggests: "Want to try normal/hard mode?"
- Reduce available hints
- Increase enemy challenge

### Tool Suggestion System

Unknown can suggest tool usage contextually:

**Example Scenarios**:
- "That firewall looks complicated. The decryption tool might help."
- "You're running low on health. Find a med kit or rest."
- "The path ahead is blocked. The cutting tool could help."

**Implementation**:
- Only suggest tools player has acquired
- Never suggest solution directly
- Optional (player can dismiss)
- Respects difficulty mode (Nightmare: no suggestions)

---

## New Game+ Onboarding

Players starting New Game+ (with progression from previous playthrough) receive modified onboarding.

### New Game+ Flow

#### Quick Profile Review

```
┌──────────────────────────────────┐
│ NEW GAME+ - RETURNING PLAYER     │
│                                  │
│ Previous Progress:               │
│ ✓ Story Completion: 85%          │
│ ✓ Puzzles Solved: 127/135        │
│ ✓ Playtime: 8 hours 34 minutes   │
│                                  │
│ Select Difficulty:               │
│ ○ Easy                           │
│ ○ Normal                         │
│ ○ Hard (Previous was Normal)     │
│ ○ Nightmare                      │
│ ⊗ Increase Difficulty Unlocked   │
│                                  │
│ New Game+ Carry-Over             │
│ ✓ Keep Abilities: 8/8            │
│ ✓ Keep Mastery Knowledge         │
│ ⊗ Reset Inventory (Fresh start)  │
│                                  │
│ [Back] [Start New Game+]         │
└──────────────────────────────────┘
```

### Skip Onboarding Cinematic

Players in New Game+ can opt to skip:

- Splash screen: Auto-skip (can view in extras)
- Profile creation: Player name/difficulty only
- Story cinematic: Skippable (can rewatch in extras)
- Tutorial missions: Entirely skippable
- First area: Auto-populated with context

### Quick Difficulty Briefing

Instead of full tutorials, show brief difficulty adjustment info:

**Example (Easy → Hard)**:
```
Upgrading to Hard Mode

Changes from Normal:
✗ Fewer hints and guidance
✗ Less forgiving combat
✗ Scarcer resources
✓ More engaging puzzle challenges
✓ Better story pacing

You can still access tutorials from the Help menu.
Ready to begin?
```

### Carry-Over Mechanics

#### Selective Carry-Over Options

| Element | Easy | Normal | Hard | Nightmare |
|---------|------|--------|------|-----------|
| **Skills** | All | All | All | All |
| **Knowledge** | Full | Full | Partial | Minimal |
| **Items** | Keep key items | Reset | Reset | Reset |
| **Abilities** | Keep all | Keep all | Keep most | Keep essential |
| **Story State** | Continues | Restarts with memory | Restarts clean | Fresh slate |

**Player choices**:
```
Keep Previous Experience?
□ Yes - I want to remember what I learned
□ No - Give me a fresh challenge
□ Custom - Let me choose individual items
```

### Progression Stats Display

After skipping cinematic and tutorial:

```
┌──────────────────────────────────┐
│ READY TO BEGIN NEW GAME+         │
│                                  │
│ Previous Playthrough Stats:      │
│ • Completion Time: 8:34          │
│ • Areas Explored: 12/15          │
│ • Puzzles Solved: 127/135        │
│ • Combat Encounters: 23/28       │
│ • Choices Made: 14 unique        │
│                                  │
│ This Run's Challenge: Hard Mode  │
│ • Difficulty Modifier: +35%      │
│ • Enemy Adaptivity: On           │
│ • Fresh Encounters: Enabled      │
│                                  │
│ Begin?                           │
└──────────────────────────────────┘
```

### Veteran-Specific Guidance

Unknown's communication changes for returning players:

**Tone Shift**:
- More casual/familiar greeting
- References to previous playthrough
- Assumes familiarity with systems
- Offers challenge commentary
- Acknowledges difficulty increase

**Example Dialogue**:
- "Welcome back. Ready for a real challenge?"
- "I remember you - you figured out [previous puzzle] nicely."
- "This difficulty will test everything you learned."
- "Good to see you again. Let's get started."

---

## Accessibility in Tutorial

Comprehensive accessibility support ensures all players can experience the tutorial effectively.

### Text and Font Accessibility

#### Text Size Scaling

```
Display Options → Text Size
┌────────────────────────────────┐
│ Hint Text Size:                │
│ Small  [==] Default [==] Large │
│ 14pt        24pt        48pt   │
│                                │
│ Menu Text Size:                │
│ Small  [====] Default [=] Huge │
│ 12pt        18pt        36pt   │
│                                │
│ Objective Text Size:           │
│ Small  [===] Default [==] Large│
│ 16pt        24pt        40pt   │
└────────────────────────────────┘
```

**Implementation**:
- Minimum 18pt for tutorial text
- Supports up to 48pt for accessibility
- Respects OS font scale settings
- Dynamic layout adjustment for larger text

### Color and Visual Accessibility

#### Colorblind Mode

```
Colorblind Accessibility
┌──────────────────────────────┐
│ Enable Colorblind Mode:      │
│ ○ Off (default)              │
│ ○ Deuteranopia (red-blind)   │
│ ○ Protanopia (green-blind)   │
│ ○ Tritanopia (blue-yellow)   │
│ ○ Achromatopsia (monochrome) │
└──────────────────────────────┘
```

**Color Coding Alternatives**:
- Interactive objects: Highlights + symbols/icons (not just colors)
- Health/status: Use patterns, icons, text in addition to colors
- Objectives: Icons + text, never rely on color alone
- Tutorial tips: Visual icons + text (checkmark, arrow, X, etc.)

#### High Contrast Mode

- Increase contrast between UI elements and background
- Use bold outlines instead of subtle gradients
- Remove drop shadows that reduce readability
- Ensure text has minimum 7:1 contrast ratio

### Subtitles and Text-Based Guidance

#### Subtitle Options

```
Accessibility → Subtitles
┌────────────────────────────┐
│ ✓ Enable Subtitles         │
│ Unknown Voice Over [CC]    │
│ Background Sounds [CC]     │
│ Sound Effects [>]          │
│                            │
│ Subtitle Size:             │
│ Small [=] Default [==] Big │
│                            │
│ Background Opacity:        │
│ Transparent [...] Opaque   │
│ [Preview]                  │
└────────────────────────────┘
```

**Implementation**:
- All voice guidance gets subtitles
- Sound effect descriptions for deaf players
- Speaker identification (Unknown, etc.)
- Timings sync with audio

#### Text-Only Mode

For deaf players, all guidance available as text:
- Hint system: Text descriptions of audio cues
- Unknown guidance: Always available in text form
- Cinematic: Full narration and descriptions
- Tutorial: Visual demonstrations with text explanations

### Motor/Input Accessibility

#### Alternative Input Methods

- **Voice commands**: Available for players with mobility issues
  - "Skip tutorial"
  - "Accept hint"
  - "Open inventory"

- **Single-button mode**: For severe motor limitations
  - One button cycles through options
  - Enter key selects current option
  - Slow but fully accessible

- **Eye-tracking support**: Optional (requires hardware)
  - Look at UI elements to select
  - Dwell time for confirmation
  - Works with tutorial highlighting

#### Remappable Controls

- All controls fully remappable
- Supports alternative input devices (one-handed, etc.)
- Profiles for different accessibility needs
- Import/export control schemes

### Cognitive Accessibility

#### Reduced Motion

```
Accessibility → Motion
┌────────────────────────────┐
│ ✓ Reduce Motion Effects    │
│                            │
│ Effects:                   │
│ • Parallax scrolling: Off  │
│ • Screen transitions: Fade │
│ • Flashing UI: Disabled    │
│ • Screen shake: Disabled   │
│ • Motion blur: Disabled    │
│                            │
│ [Preview Changes]          │
└────────────────────────────┘
```

**Implementation**:
- Remove dizzying camera movements
- Simpler UI animations
- No flashing/strobing (epilepsy safety)
- No motion blur during intense sequences

#### Simplified UI Option

```
Accessibility → Complexity
┌────────────────────────────┐
│ Interface Complexity:      │
│ ○ Full (default)          │
│ ○ Simplified             │
│ ○ Minimal                │
│                           │
│ Simplified removes:       │
│ • Advanced stats          │
│ • Secondary information  │
│ • Decorative elements    │
│                           │
│ Focuses on:              │
│ • Primary objective      │
│ • Essential controls     │
│ • Core information       │
│                           │
│ [Preview]                │
└────────────────────────────┘
```

#### Dyslexia-Friendly Font

```
Accessibility → Font
┌────────────────────────────┐
│ UI Font:                   │
│ ○ Standard (Arial)         │
│ ○ Dyslexia-Friendly        │
│                            │
│ Line Spacing:              │
│ Default [===] Increased    │
│                            │
│ Letter Spacing:            │
│ Default [==] Increased     │
│                            │
│ [Preview Sample Text]      │
└────────────────────────────┘
```

### Time-Pressure Accessibility

Tutorial removes all time pressure:

- **No time limits** on any tutorial actions
- **No countdown timers** during learning
- **Pause anytime** (except cinematics, which can be skipped)
- **Revisit anytime**: Tutorial sections can be replayed

### Difficulty and Accessibility Interaction

Accessibility options stack with difficulty settings:

| Setting | Easy | Normal | Hard | Nightmare |
|---------|------|--------|------|-----------|
| **Subtitles** | Yes (always) | Optional | Optional | Optional |
| **Text Size** | 24pt default | 18pt default | 18pt default | 14pt default |
| **Highlight** | Full | Partial | Minimal | None |
| **Time Pressure** | None | None | None | Minimal |
| **Motor Assist** | Auto-aim | Manual | Manual | Off |
| **Hints** | Very frequent | Frequent | Rare | None |

---

## Technical Requirements

### Tutorial Scripting System

#### Event-Driven Architecture

The tutorial system uses an event-driven architecture for flexibility and scalability:

```csharp
public interface ITutorialEvent
{
    string EventId { get; }
    void Trigger(TutorialContext context);
    void Resolve(TutorialContext context);
}

public class TutorialEventManager : MonoBehaviour
{
    private Dictionary<string, List<ITutorialEvent>> eventListeners;
    
    public void RegisterEvent(string eventId, ITutorialEvent listener)
    {
        if (!eventListeners.ContainsKey(eventId))
            eventListeners[eventId] = new List<ITutorialEvent>();
        eventListeners[eventId].Add(listener);
    }
    
    public void FireEvent(string eventId, TutorialContext context)
    {
        if (eventListeners.ContainsKey(eventId))
        {
            foreach (var listener in eventListeners[eventId])
                listener.Trigger(context);
        }
    }
}
```

#### Tutorial State Machine

```csharp
public enum TutorialState
{
    NotStarted,
    Cinematic,
    MovementTutorial,
    LookTutorial,
    InteractionTutorial,
    InventoryTutorial,
    PhoneTutorial,
    CombatTutorial,
    PuzzleTutorial,
    Complete,
    Skipped
}

public class TutorialStateMachine
{
    private TutorialState currentState;
    
    public void TransitionTo(TutorialState newState)
    {
        OnExitState(currentState);
        currentState = newState;
        OnEnterState(currentState);
    }
    
    private void OnEnterState(TutorialState state)
    {
        // Load state-specific content
        // Show appropriate hints
        // Set up input handling
    }
}
```

### Hint Manager System

#### Hint Storage and Retrieval

```csharp
[System.Serializable]
public class TutorialHint
{
    public string id;
    public string content;
    public string audioClip;  // For voice guidance
    public float displayDuration;
    public int priority;  // 1=critical, 5=low
    public string[] triggerEvents;
    public int idleTimeBeforeTrigger;
}

public class HintManager : MonoBehaviour
{
    private Dictionary<string, TutorialHint> hints;
    private Queue<string> hintQueue;
    
    public void ShowHint(string hintId)
    {
        if (hints.ContainsKey(hintId))
        {
            var hint = hints[hintId];
            DisplayHintUI(hint);
            if (hint.audioClip != null)
                PlayHintAudio(hint.audioClip);
        }
    }
}
```

#### Dynamic Hint Selection

Hints are selected based on:
- Current tutorial state
- Player difficulty level
- Previous hints shown (avoid repetition)
- Player behavior (idle time, repeated failures)
- Time elapsed in current task

### Cinematic Player System

#### Cinematic Sequencing

```csharp
public class CinematicPlayer : MonoBehaviour
{
    private Timeline cinematicTimeline;
    private float skipableAfterTime = 5f;
    private bool canSkip = false;
    
    public void PlayCinematic(string cinematicId)
    {
        cinematicTimeline = cinematicDatabase.GetCinematic(cinematicId);
        cinematicTimeline.Play();
        StartCoroutine(EnableSkipAfterDelay());
    }
    
    private IEnumerator EnableSkipAfterDelay()
    {
        yield return new WaitForSeconds(skipableAfterTime);
        canSkip = true;
        ShowSkipPrompt();
    }
    
    public void SkipCinematic()
    {
        if (canSkip)
        {
            cinematicTimeline.Stop();
            OnCinematicComplete();
        }
    }
}
```

#### Subtitle Synchronization

Subtitles sync with cinematic playback:
- Timeline markers trigger subtitle display
- Subtitles hide/show based on dialogue/narration
- Support for multiple languages
- Respect user subtitle settings

### Objective Tracking System

```csharp
public class ObjectiveTracker : MonoBehaviour
{
    private List<Objective> activeObjectives;
    
    public void AddObjective(Objective objective)
    {
        activeObjectives.Add(objective);
        UpdateObjectiveUI();
    }
    
    public void CompleteObjective(string objectiveId)
    {
        var objective = activeObjectives.Find(o => o.id == objectiveId);
        if (objective != null)
        {
            objective.isComplete = true;
            UpdateObjectiveUI();
            TriggerObjectiveCompletionFeedback();
        }
    }
}
```

### Interactive Object Highlighting

```csharp
public class InteractiveObjectHighlight : MonoBehaviour
{
    private Material highlightMaterial;
    private float pulseIntensity = 1.5f;
    private bool isHighlighted = false;
    
    public void EnableHighlight(HighlightType type)
    {
        isHighlighted = true;
        StartCoroutine(PulseEffect());
        UpdateHighlightColor(type);
    }
    
    private IEnumerator PulseEffect()
    {
        while (isHighlighted)
        {
            float value = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f;
            highlightMaterial.SetFloat("_EmissionStrength", value * pulseIntensity);
            yield return null;
        }
    }
}
```

### Difficulty Scaling System

```csharp
public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard,
    Nightmare
}

public class DifficultyScaling : MonoBehaviour
{
    public float GetHintFrequencyMultiplier(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => 1.5f,      // 150% frequency
            DifficultyLevel.Normal => 1.0f,    // 100% frequency
            DifficultyLevel.Hard => 0.5f,      // 50% frequency
            DifficultyLevel.Nightmare => 0.0f, // No hints
            _ => 1.0f
        };
    }
    
    public bool ShouldHighlightObjects(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => true,
            DifficultyLevel.Normal => true,
            DifficultyLevel.Hard => false,
            DifficultyLevel.Nightmare => false,
            _ => true
        };
    }
}
```

### Accessibility Configuration System

```csharp
[System.Serializable]
public class AccessibilitySettings
{
    public bool subtitlesEnabled = true;
    public float textSize = 1.0f;  // 1.0 = default
    public ColorblindMode colorblindMode = ColorblindMode.None;
    public bool highContrast = false;
    public bool reduceMotion = false;
    public bool dyslexiaFriendlyFont = false;
    
    public void ApplySettings()
    {
        // Apply to all UI elements
        // Update visual settings
        // Notify systems of changes
    }
}
```

---

## Implementation Details

### Phase 1: Core Structure

1. **Tutorial State Manager**: Implements state machine for tutorial progression
2. **Event System**: Set up event-driven architecture for tutorial events
3. **Hint Manager**: Create hint storage and display system
4. **Basic UI**: Tutorial objective panel, hint display box

### Phase 2: Tutorial Content

1. **Movement Tutorial**: Implementation with WASD detection
2. **Look Tutorial**: Mouse look tracking and highlights
3. **Interaction Tutorial**: E-key interaction with objects
4. **Inventory Tutorial**: Inventory UI integration

### Phase 3: Advanced Features

1. **Adaptive Hints**: Struggle detection and dynamic hint adjustment
2. **Cinematic System**: Timeline-based cinematic playback
3. **Unknown AI Integration**: Phone messages and HUD narration
4. **Skip System**: Allow skipping individual steps or entire tutorial

### Phase 4: Polish and Accessibility

1. **Accessibility Options**: Subtitles, text size, colorblind modes
2. **Difficulty Integration**: Adjust tutorial for all difficulty levels
3. **Audio Implementation**: Voice-over, ambient audio, sound effects
4. **Performance Optimization**: Ensure smooth tutorial experience

### Data Storage

Tutorial progress stored in player save file:

```json
{
  "tutorialState": "CombatTutorial",
  "completedSteps": [
    "movement",
    "look",
    "interaction",
    "inventory",
    "phone"
  ],
  "skippedSteps": [],
  "hintsShown": {
    "movement_1": true,
    "look_1": true,
    "interaction_1": true
  },
  "cinematicsWatched": [
    "opening_cinematic"
  ],
  "difficultySelected": "Normal",
  "accessibility": {
    "subtitles": true,
    "textSize": 1.2,
    "colorblindMode": "none"
  }
}
```

---

## QA and Acceptance Criteria

### Tutorial Flow Testing

#### Acceptance Criteria: First Launch

**GIVEN** a player launches the game for the first time  
**WHEN** they progress through splash screen, profile creation, and cinematic  
**THEN** they should:
- See splash screen for 3-5 seconds minimum
- Create profile with valid name
- Select difficulty (default: Normal)
- Watch 120-180 second cinematic
- Enter tutorial without friction

**GIVEN** the cinematic is playing  
**WHEN** 5 seconds have elapsed  
**THEN** ESC key should be available to skip to next phase

**GIVEN** player skips cinematic  
**WHEN** skip key is pressed  
**THEN** immediate transition to first tutorial area should occur (no fade/loading)

#### Acceptance Criteria: Interactive Tutorials

**GIVEN** player completes movement tutorial  
**WHEN** they move 10+ meters forward  
**THEN** tutorial should:
- Show completion feedback
- Transition to next tutorial smoothly
- Display next objective clearly

**GIVEN** player is in any tutorial section  
**WHEN** 45 seconds elapse without progress  
**THEN** appropriate hint should display automatically (scaled by difficulty)

**GIVEN** player clicks "Skip This Step"  
**WHEN** confirmation is shown  
**THEN** tutorial should:
- Skip to next section immediately
- Record skipped section
- Mark section as accessible from Help menu

**GIVEN** player completes all tutorial sections  
**WHEN** final section ends  
**THEN** transition to main game should occur with briefing about difficulty

### Difficulty and Adaptation Testing

#### Acceptance Criteria: Easy Mode

**GIVEN** player selects Easy difficulty  
**WHEN** tutorial begins  
**THEN** should observe:
- All interactive objects highlighted brightly
- Hints appear every 30 seconds if idle
- Combat training: Enemies don't attack back
- Objective step-by-step guidance
- Frequent positive reinforcement messages

#### Acceptance Criteria: Nightmare Mode

**GIVEN** player selects Nightmare difficulty  
**WHEN** tutorial begins  
**THEN** should observe:
- No object highlighting
- No automatic hints
- Combat training: Real combat danger
- Vague objectives: "Escape the room" (not specific steps)
- Unknown provides no guidance unless critical

#### Acceptance Criteria: Struggle Detection

**GIVEN** player dies 3 times in succession  
**WHEN** third death occurs  
**THEN** system should:
- Offer hint system activation
- Reduce enemy difficulty by 20%
- Increase hint frequency
- Unknown offers: "Having trouble? I can help."

**GIVEN** player idle for 2+ minutes on puzzle  
**WHEN** timeout expires  
**THEN** system should:
- Unknown asks: "Need a hint?"
- Optional hint system becomes available
- If not claimed within 60s, Unknown gives specific hint

### Accessibility Testing

#### Acceptance Criteria: Subtitles

**GIVEN** subtitles are enabled  
**WHEN** Unknown provides voice guidance  
**THEN** should see:
- Text of voice line displayed
- Timing synchronized with audio
- Speaker identification ("Unknown:")
- Readable font size (minimum 18pt)
- High contrast against background

**GIVEN** cinematic plays with dialogue  
**WHEN** subtitles enabled  
**THEN** all dialogue should have synchronized subtitles

#### Acceptance Criteria: Text Scaling

**GIVEN** player sets text size to 150%  
**WHEN** tutorial displays hints  
**THEN** should observe:
- All text proportionally larger
- UI layout adjusts to accommodate
- No text cutoff or overlap
- Readable even at maximum size (48pt)

#### Acceptance Criteria: Colorblind Mode

**GIVEN** colorblind mode is enabled (any type)  
**WHEN** tutorial displays highlights or UI elements  
**THEN** should see:
- Color replaced with patterns/icons
- Information still readable
- No loss of functionality
- Deuteranopia mode tested specifically

#### Acceptance Criteria: Reduced Motion

**GIVEN** reduce motion is enabled  
**WHEN** tutorial cinematic plays  
**THEN** should observe:
- No camera shake or rapid movement
- Transitions are simple fades
- No flashing or strobing
- Smooth paced action

### Performance Testing

#### Acceptance Criteria: Load Times

**GIVEN** tutorial begins from game start  
**WHEN** player progresses through tutorial  
**THEN** should observe:
- Tutorial scene load: Under 10 seconds
- Transitions between sections: Under 2 seconds
- No frame rate dips below 30 FPS

**GIVEN** tutorial cinematic plays  
**WHEN** cinematic completes  
**THEN** memory should be:
- Game running: Under 4GB RAM
- No memory leaks during extended tutorials
- Cinematic assets properly unloaded after playback

#### Acceptance Criteria: Input Responsiveness

**GIVEN** player presses WASD to move  
**WHEN** input is registered  
**THEN** player should:
- Begin moving within 16ms (1 frame at 60FPS)
- Movement feel smooth and responsive
- No lag or input buffering

### New Game+ Testing

#### Acceptance Criteria: Skip Options

**GIVEN** player starts New Game+  
**WHEN** profile review shows  
**THEN** should have option to:
- Skip cinematic entirely
- Skip tutorial entirely
- Choose difficulty increase

**GIVEN** player chooses skip tutorial  
**WHEN** skip confirmed  
**THEN** should:
- Brief difficulty info shown (2-3 seconds)
- Jump to main game in first area
- Previous knowledge retained (abilities, etc.)

#### Acceptance Criteria: Progression Stats

**GIVEN** New Game+ briefing shows previous stats  
**WHEN** player reviews stats  
**THEN** should display:
- Previous completion time (accurate)
- Number of puzzles solved
- Difficulty completed on
- New Game+ difficulty option
- Estimated time for this run

### Localization Testing

**GIVEN** tutorial text is displayed  
**WHEN** game language is changed  
**THEN** should show:
- All hints in selected language
- Subtitles in selected language
- Voice-over dubbed/localized (if available)
- UI properly formatted for text length

### Edge Case Testing

#### Acceptance Criteria: Tutorial Interruptions

**GIVEN** player pauses during tutorial  
**WHEN** resume is selected  
**THEN** should:
- Resume at exact same point
- Maintain tutorial progress
- Not lose any state

**GIVEN** player alt-tabs or minimizes during tutorial  
**WHEN** application regains focus  
**THEN** should:
- Resume cleanly without issues
- Maintain all tutorial progress
- Audio syncs properly with video (if cinematic)

#### Acceptance Criteria: Tutorial Cleanup

**GIVEN** tutorial is complete  
**WHEN** player enters main game  
**THEN** should:
- Tutorial UI elements removed
- Tutorial mode disabled
- No lingering hint system interference
- Full game systems accessible

---

## Related Documentation

- [AI Narrator and Missions System](/docs/protocol-emr/ai-narrator-and-missions.md)
- [Core Systems](/docs/protocol-emr/core-systems.md)
- [Cinematics and Transitions](/docs/protocol-emr/cinematics-transitions.md)
- [Save/Load and Account Management](/docs/protocol-emr/save-load-account.md)

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024 | Initial comprehensive tutorial documentation |

