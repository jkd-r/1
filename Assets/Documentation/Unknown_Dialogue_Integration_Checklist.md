# Unknown Dialogue System - Integration Checklist

Use this checklist to integrate the Unknown Dialogue System with your game systems.

## Setup Phase

### Database Creation
- [ ] Run `Protocol EMR > Dialogue > Create Message Database` menu
- [ ] Verify database created at `Assets/Resources/Dialogue/UnknownMessageDatabase.asset`
- [ ] Inspect database to review 90+ pre-written messages
- [ ] (Optional) Customize messages in Inspector

### Scene Setup
- [ ] Add `UnknownDialogueManager` to scene (or via GameManager prefab)
- [ ] Create Canvas if not exists
- [ ] Add `UnknownPhoneUI` component (under Canvas)
- [ ] Add `UnknownHUDOverlay` component (under Canvas)
- [ ] Configure UI references in Inspector
- [ ] (Optional) Add `UnknownDialogueDemoController` for testing

### Settings Integration
- [ ] Verify `SettingsManager` has Unknown settings (hint frequency, personality, enable)
- [ ] Test settings persistence (change settings, restart, verify saved)
- [ ] Add UI sliders/toggles for Unknown settings in settings menu

## Combat System Integration

### Player Combat
- [ ] When player hits NPC:
  ```csharp
  UnknownDialogueTriggers.TriggerPlayerHitNPC(npc, damage);
  ```
- [ ] When player takes damage:
  ```csharp
  UnknownDialogueTriggers.TriggerPlayerTookDamage(attacker, damage, currentHP, maxHP);
  ```
- [ ] When player dodges successfully:
  ```csharp
  UnknownDialogueTriggers.TriggerDodgeSuccessful();
  ```
- [ ] When combat starts:
  ```csharp
  UnknownDialogueTriggers.TriggerCombatStarted(enemy);
  ```

### NPC Combat
- [ ] When NPC is defeated:
  ```csharp
  UnknownDialogueTriggers.TriggerNPCDefeated(npc);
  ```
- [ ] When NPC detects player:
  ```csharp
  UnknownDialogueTriggers.TriggerNPCEncountered(npc);
  ```
- [ ] When danger nearby:
  ```csharp
  UnknownDialogueTriggers.TriggerDangerDetected(threat);
  ```

### NPC AI Integration (Sprint 7)
- [ ] Subscribe to `NPCManager.OnGlobalAlertTriggered` event
- [ ] Trigger messages based on alert type:
  - SoundDetected → TriggerDangerDetected
  - PlayerSighted → TriggerNPCEncountered
  - AttackDetected → TriggerCombatStarted

## Exploration System Integration

### Area Discovery
- [ ] In area trigger component:
  ```csharp
  UnknownDialogueTriggers.TriggerNewAreaDiscovered(areaName);
  ```
- [ ] Track which areas have been discovered to avoid repeats

### Secrets and Items
- [ ] When secret found:
  ```csharp
  UnknownDialogueTriggers.TriggerSecretFound(secretObject);
  ```
- [ ] When item picked up:
  ```csharp
  UnknownDialogueTriggers.TriggerItemFound(itemObject);
  ```
- [ ] Mark secrets/items as discovered to prevent duplicate messages

### Interaction System
- [ ] When interacting with important objects, trigger contextual messages
- [ ] Consider triggering `TriggerCustomMessage` for unique interactions

## Puzzle System Integration

### Puzzle Events
- [ ] When player approaches puzzle:
  ```csharp
  UnknownDialogueTriggers.TriggerPuzzleEncountered(puzzle);
  ```
- [ ] When puzzle attempt fails:
  ```csharp
  UnknownDialogueTriggers.TriggerPuzzleAttemptFailed(puzzle);
  ```
- [ ] When puzzle solved:
  ```csharp
  UnknownDialogueTriggers.TriggerPuzzleSolved(puzzle, isPerfect);
  ```
- [ ] When player stuck (no progress for X seconds):
  ```csharp
  UnknownDialogueTriggers.TriggerPlayerStuck();
  ```

### Puzzle Tracking
- [ ] Track puzzle attempts count
- [ ] Track time spent on puzzle
- [ ] Determine "perfect" solve criteria

## Mission System Integration

### Mission Lifecycle
- [ ] When mission starts:
  ```csharp
  UnknownDialogueTriggers.TriggerMissionStart(missionName);
  ```
- [ ] When milestone reached:
  ```csharp
  UnknownDialogueTriggers.TriggerMissionMilestone(milestoneName);
  ```
- [ ] When mission completes:
  ```csharp
  UnknownDialogueTriggers.TriggerMissionComplete(missionName);
  ```
- [ ] When objective fails:
  ```csharp
  UnknownDialogueTriggers.TriggerObjectiveFailed(objectiveName);
  ```
- [ ] When new mission available:
  ```csharp
  UnknownDialogueTriggers.TriggerNewMissionAvailable(missionName);
  ```

### Mission Context
- [ ] Track current mission objectives
- [ ] Track completed missions
- [ ] Store mission progress for context-aware messaging

## Narrative System Integration

### Story Progression
- [ ] When plot point reached:
  ```csharp
  UnknownDialogueTriggers.TriggerPlotPointReached(plotPointName);
  ```
- [ ] When procedural story milestone:
  ```csharp
  UnknownDialogueTriggers.TriggerProceduralStoryMilestone(milestoneName);
  ```
- [ ] When major event occurs:
  ```csharp
  UnknownDialogueTriggers.TriggerMajorEventOccurred(eventName);
  ```
- [ ] When secret discovered (lore):
  ```csharp
  UnknownDialogueTriggers.TriggerSecretDiscovered(secretName);
  ```

### Document Reading
- [ ] When player reads document:
  ```csharp
  UnknownDialogueTriggers.TriggerDocumentRead(documentName);
  ```
- [ ] Track which documents have been read

## Player Behavior Tracking

### Play Style Detection
- [ ] Track stealth kills vs direct combat
- [ ] When using stealth:
  ```csharp
  UnknownDialogueTriggers.TriggerStealthApproach();
  ```
- [ ] When using aggressive tactics:
  ```csharp
  UnknownDialogueTriggers.TriggerAggressiveApproach();
  ```

### System Integration
- [ ] Allow UnknownDialogueManager to access player style profile
- [ ] Update game stage (0/1/2) as game progresses
- [ ] Update difficulty level based on settings

## Game Lifecycle Events

### Game Start/End
- [ ] On game start (first frame or after loading):
  ```csharp
  UnknownDialogueTriggers.TriggerGameStart();
  ```
- [ ] On player death:
  ```csharp
  UnknownDialogueTriggers.TriggerPlayerDeath();
  ```

### Scene Transitions
- [ ] Clear appropriate message history on major scene changes
- [ ] Persist important message history across scenes

## UI Integration

### Phone Interface
- [ ] Bind phone toggle to 'C' key (already in InputManager)
- [ ] Test phone open/close functionality
- [ ] Verify message history displays correctly
- [ ] Test scrolling with many messages
- [ ] Verify unread badge updates correctly

### HUD Overlay
- [ ] Position overlay at screen edges (non-intrusive)
- [ ] Test glitch effects with different settings
- [ ] Verify fade in/out animations
- [ ] Test with different screen resolutions
- [ ] Verify accessibility settings affect glitch intensity

### Settings Menu
- [ ] Add "Unknown Entity" section to settings
- [ ] Add hint frequency slider (0-100%)
- [ ] Add personality dropdown (Verbose/Balanced/Cryptic)
- [ ] Add enable/disable toggle
- [ ] Bind to SettingsManager methods

## Testing Phase

### Functional Testing
- [ ] Test each trigger type (30+ triggers)
- [ ] Verify messages appear within 1 second
- [ ] Verify cooldown system prevents spam
- [ ] Verify recent message filter works
- [ ] Verify hint frequency affects message rate
- [ ] Verify personality mode affects message selection

### Integration Testing
- [ ] Test combat flow with messages
- [ ] Test exploration with messages
- [ ] Test puzzle solving with messages
- [ ] Test mission progression with messages
- [ ] Test NPC AI integration with messages

### Performance Testing
- [ ] Profile message selection performance (<1ms)
- [ ] Test with 100+ messages in database
- [ ] Test with frequent message triggers
- [ ] Verify 60 FPS maintained during messaging
- [ ] Check memory usage (<2MB for database)

### Accessibility Testing
- [ ] Test with motion blur disabled (reduced glitch effects)
- [ ] Test with high contrast mode
- [ ] Test with different UI scales
- [ ] Test with audio disabled
- [ ] Test with subtitles enabled

### Content Testing
- [ ] Review all messages for tone consistency
- [ ] Verify no "system-like" language
- [ ] Verify messages maintain mystery
- [ ] Verify messages are contextually appropriate
- [ ] Verify 3-5 variants exist per major trigger

## Polish Phase

### Message Database Refinement
- [ ] Add custom messages for unique game situations
- [ ] Balance message weights for natural variation
- [ ] Adjust cooldown times for optimal pacing
- [ ] Set appropriate difficulty levels for messages
- [ ] Assign correct game stages to messages

### Audio Enhancement
- [ ] Add message received sound effect
- [ ] Add typing sound loop
- [ ] Add glitch sound effects
- [ ] Add static/noise sounds for overlays
- [ ] Mix audio levels appropriately

### Visual Effects
- [ ] Fine-tune glitch intensity
- [ ] Adjust chromatic aberration amount
- [ ] Optimize fade timing
- [ ] Polish typing animation speed
- [ ] Test with different UI themes

## Documentation

### Code Documentation
- [ ] Add XML comments to custom trigger calls
- [ ] Document integration decisions
- [ ] Create integration example for your project
- [ ] Update architecture diagrams

### Player-Facing Documentation
- [ ] Update controls guide with phone key
- [ ] Update settings menu descriptions
- [ ] Create tooltip for Unknown settings
- [ ] Add Unknown entity to game lore

## Deployment Checklist

### Pre-Release
- [ ] All integration points tested
- [ ] Performance targets met
- [ ] No console errors
- [ ] Message database complete
- [ ] Settings persist correctly
- [ ] Phone UI functions correctly
- [ ] HUD overlay displays correctly

### Build Verification
- [ ] Test in standalone build (not just Editor)
- [ ] Verify Resources/Dialogue folder included
- [ ] Verify message database loads correctly
- [ ] Test settings persistence in build
- [ ] Profile performance in build

### Post-Release Monitoring
- [ ] Monitor for message spam issues
- [ ] Track which messages appear most frequently
- [ ] Collect player feedback on messaging
- [ ] Analyze message trigger patterns
- [ ] Plan content updates based on data

## Custom Integration Tasks

Use this section to track project-specific integration needs:

### Custom Triggers
- [ ] _____________________________________
- [ ] _____________________________________
- [ ] _____________________________________

### Custom Messages
- [ ] _____________________________________
- [ ] _____________________________________
- [ ] _____________________________________

### Custom Systems
- [ ] _____________________________________
- [ ] _____________________________________
- [ ] _____________________________________

## Notes

Use this space for integration notes and decisions:

```
[Your notes here]
```

---

**Checklist Version**: 1.0
**Sprint**: 8
**Last Updated**: Sprint 8 Completion
