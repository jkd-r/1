# Save/Load & Account System - Sprint 6

## Overview

The Save/Load & Account System provides comprehensive game state persistence with multiple profiles, save slots, corruption protection, and automatic backups. The system ensures player progress is never lost through robust error handling and backup mechanisms.

## Features

### Player Profile System
- **Multiple Profiles**: Support for up to 10 profiles per installation
- **Profile Metadata**: Name, avatar icon, difficulty, creation date, playtime
- **Cumulative Statistics**: Track total playtime, missions, NPCs encountered, deaths, etc.
- **Profile Switching**: Easy switching between profiles from main menu
- **Profile Deletion**: Safe deletion with confirmation dialogs

### Save Slot Architecture
- **Auto-Save**: 3 rotating auto-save slots (every 5 minutes + event-triggered)
- **Manual Saves**: 10 manual save slots per profile
- **Checkpoints**: 5 rotating checkpoint saves before major events
- **Quick-Save**: Single quick-save slot (F5 hotkey)

### Save Data Structure
Comprehensive JSON-based save format including:
- Player state (position, rotation, health, stamina, inventory)
- World state (scene, objects, doors, collectibles, environment)
- Mission state (active, completed, failed missions with objectives)
- NPC state (encountered NPCs, relationships, dialogue flags, alerts)
- Unlocked content (cosmetics, paths, lore, abilities)
- Session settings (difficulty, hints, subtitles)

### Data Integrity
- **SHA-256 Checksums**: Verify save file integrity on load
- **Automatic Backups**: Keep 3 rotating backups per save file
- **Corruption Detection**: Detect and recover from corrupted saves
- **Schema Validation**: Validate save data structure and logical consistency
- **Repair System**: Attempt to repair corrupted saves with default values

### Auto-Save Triggers
Automatic saves are created at:
- Every 5 minutes (configurable)
- Mission start/completion
- Puzzle solved
- Combat encounter start
- Dialogue completion
- Secret area discovered

### Performance
- **Save Time**: <2 seconds target
- **Load Time**: <3 seconds target
- **Async I/O**: Non-blocking file operations
- **File Size**: <5MB per save (JSON format)
- **Memory Usage**: <10MB during save/load operations

## Usage

### Creating a Profile

```csharp
using ProtocolEMR.Core.SaveSystem;

// Create a new profile
ProfileData profile = ProfileManager.Instance.CreateProfile(
    "PlayerName",
    avatarIcon: 0,
    difficulty: "Normal"
);

// Switch to the profile
ProfileManager.Instance.SwitchToProfile(profile.profileId);
```

### Manual Save

```csharp
// Save to manual save slot 3
SaveGameManager.Instance.SaveGame(
    SaveSlotType.ManualSave, 
    slotIndex: 3, 
    customName: "Before Boss Fight"
);
```

### Quick Save/Load

```csharp
// Quick save (or press F5)
SaveGameManager.Instance.QuickSave();

// Quick load (or press F9)
SaveGameManager.Instance.QuickLoad();
```

### Loading a Save

```csharp
// Get available save slots
List<SaveSlotMetadata> saves = SaveGameManager.Instance.GetSaveSlots();

// Load a specific save
SaveGameManager.Instance.LoadGame(saveId, profileId);
```

### Auto-Save Triggers

```csharp
// Trigger auto-save on mission start
SaveTriggerSystem.Instance.OnMissionStarted("mission_01");

// Trigger auto-save on puzzle solved
SaveTriggerSystem.Instance.OnPuzzleSolved("puzzle_room_01");

// Create checkpoint before major event
SaveTriggerSystem.Instance.CreateCheckpoint("Before Final Boss");
```

### Statistics Tracking

```csharp
// Track player statistics
StatisticsTracker.Instance.IncrementMissionsCompleted();
StatisticsTracker.Instance.IncrementCollectiblesFound();
StatisticsTracker.Instance.IncrementDeaths();

// Get formatted playtime
string playtime = StatisticsTracker.Instance.GetFormattedPlaytime();

// Get statistics summary
string summary = StatisticsTracker.Instance.GetStatisticsSummary();
```

### Event Handling

```csharp
// Subscribe to save events
SaveGameManager.Instance.OnSaveCompleted += OnSaveCompleted;
SaveGameManager.Instance.OnLoadCompleted += OnLoadCompleted;
SaveGameManager.Instance.OnSaveFailed += OnSaveFailed;

void OnSaveCompleted(SaveData saveData)
{
    Debug.Log($"Save completed: {saveData.locationName}");
}
```

## Hotkeys

- **F5**: Quick Save
- **F6**: Create Manual Checkpoint
- **F9**: Quick Load (requires confirmation)

## File Structure

```
[UserDocuments]/ProtocolEMR/
├── Profiles/
│   ├── profile_[UUID].json
│   ├── profile_[UUID].json
│   └── profile_[UUID].json
├── Saves/
│   ├── [SaveID]_[ProfileID]_[Timestamp].sav
│   ├── [SaveID]_[ProfileID]_[Timestamp].sav
│   └── ...
└── Backups/
    ├── [SaveID]_backup_[Timestamp].sav
    ├── [SaveID]_backup_[Timestamp].sav
    └── ...
```

## Save File Format

Save files are stored as human-readable JSON for development builds:

```json
{
  "saveId": "uuid",
  "profileId": "uuid",
  "saveSlotType": "ManualSave",
  "slotIndex": 0,
  "timestamp": "2025-01-20T12:00:00Z",
  "locationName": "Zone_A_Room_3",
  "playtimeSeconds": 3600,
  "playerState": {
    "position": {"x": 10.5, "y": 1.8, "z": -5.2},
    "health": 100,
    "stamina": 85,
    "inventory": {...}
  },
  "worldState": {...},
  "missionState": {...},
  "checksum": "sha256_hash"
}
```

## Integration with Other Systems

### Player Controller
- Saves player position, rotation, stamina
- Restores player state on load

### Health System
- Saves current health
- Restores health on load

### Mission System
- Saves active, completed, and failed missions
- Saves mission objectives and progress
- Tracks mission choices

### NPC System
- Saves encountered NPCs and their states
- Saves NPC relationships and dialogue flags
- Restores NPC positions and statuses

### Inventory System
- Saves complete inventory state
- Saves equipped items and weapons
- Restores inventory on load

### World State
- Saves door states (open/locked)
- Saves puzzle completion
- Saves collectible collection
- Saves world events and triggers

## Error Handling

### Corrupted Save Files
1. Checksum validation detects corruption
2. Attempts to load from most recent backup
3. If backups are corrupted, attempts to repair save data
4. As last resort, offers to start new game with recovered statistics

### Missing Save Files
- Gracefully handles missing files
- Provides clear error messages
- Offers alternative saves or new game

### Version Incompatibility
- Checks save version compatibility
- Automatically migrates old save formats
- Warns user of incompatible saves

## Testing Checklist

- [ ] Create profile with custom name and avatar
- [ ] Switch between multiple profiles
- [ ] Delete profile and verify all saves deleted
- [ ] Manual save to all 10 slots
- [ ] Load from each manual save slot
- [ ] Quick save (F5) and verify save created
- [ ] Quick load (F9) and verify load works
- [ ] Auto-save triggers after 5 minutes
- [ ] Auto-save triggers on mission start/complete
- [ ] Auto-save triggers on puzzle solved
- [ ] Checkpoint save before combat
- [ ] Corrupt save file manually and verify recovery
- [ ] Delete save file and verify backup restoration
- [ ] Load save with missing data and verify repair
- [ ] Statistics tracking increments correctly
- [ ] Playtime tracking accurate
- [ ] Save notifications appear
- [ ] Load progress bar displays
- [ ] Save file size <5MB
- [ ] Save time <2 seconds
- [ ] Load time <3 seconds
- [ ] No frame drops during auto-save

## Future Enhancements

- Cloud save synchronization (Steam Cloud, Epic Games Cloud)
- Screenshot thumbnails for save slots
- Compression for smaller file sizes
- Encryption for save file security
- Save file migration tools
- Import/export save files
- New Game+ mode with stat carry-over

## Troubleshooting

### Save Not Working
- Verify ProfileManager.Instance exists
- Check Application.persistentDataPath is writable
- Verify no other save operation in progress
- Check console for error messages

### Load Not Working
- Verify save file exists in Saves directory
- Check save file is not corrupted (checksum)
- Verify save version is compatible
- Check backup files if main save corrupted

### Auto-Save Not Triggering
- Verify SaveTriggerSystem.Instance exists
- Check auto-save enabled in SaveGameManager
- Verify active profile exists
- Check minTimeBetweenSaves not restricting saves

## API Reference

See inline documentation in source files:
- `ProfileManager.cs` - Profile management
- `SaveGameManager.cs` - Save/load operations
- `SaveData.cs` - Data structures
- `SaveFileValidator.cs` - Validation and checksums
- `BackupManager.cs` - Backup creation and restoration
- `StatisticsTracker.cs` - Statistics tracking
- `SaveTriggerSystem.cs` - Auto-save triggers
- `SaveLoadHotkeyHandler.cs` - Hotkey handling
- `SaveLoadUIManager.cs` - UI management
