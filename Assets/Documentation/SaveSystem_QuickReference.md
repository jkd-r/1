# Save/Load System - Quick Reference

## Quick Start

### 1. Create a Profile (First Time)
```csharp
ProfileData profile = ProfileManager.Instance.CreateProfile(
    "PlayerName",      // Profile name (1-20 chars)
    avatarIcon: 0,     // Avatar icon (0-11)
    difficulty: "Normal" // Difficulty (Easy/Normal/Hard/Nightmare)
);

ProfileManager.Instance.SwitchToProfile(profile.profileId);
```

### 2. Manual Save
```csharp
// Save to specific slot
SaveGameManager.Instance.SaveGame(
    SaveSlotType.ManualSave,  // Type
    slotIndex: 3,              // Slot number (0-9)
    customName: "Before Boss"  // Optional name
);

// Or quick save (F5)
SaveGameManager.Instance.QuickSave();
```

### 3. Load Game
```csharp
// Get available saves
List<SaveSlotMetadata> saves = SaveGameManager.Instance.GetSaveSlots();

// Load specific save
SaveGameManager.Instance.LoadGame(saveId, profileId);

// Or quick load (F9)
SaveGameManager.Instance.QuickLoad();
```

### 4. Auto-Save Triggers
```csharp
// Trigger auto-save on events
SaveTriggerSystem.Instance.OnMissionStarted("mission_01");
SaveTriggerSystem.Instance.OnMissionCompleted("mission_01");
SaveTriggerSystem.Instance.OnPuzzleSolved("puzzle_01");
SaveTriggerSystem.Instance.OnCombatStarted();
SaveTriggerSystem.Instance.OnDialogueCompleted("npc_01");
SaveTriggerSystem.Instance.OnSecretDiscovered("secret_01");

// Create checkpoint
SaveTriggerSystem.Instance.CreateCheckpoint("Before Boss Fight");
```

### 5. Track Statistics
```csharp
StatisticsTracker.Instance.IncrementMissionsCompleted();
StatisticsTracker.Instance.IncrementNPCsEncountered();
StatisticsTracker.Instance.IncrementCollectiblesFound();
StatisticsTracker.Instance.IncrementDeaths();
StatisticsTracker.Instance.IncrementPuzzlesSolved();
StatisticsTracker.Instance.IncrementSecretsFound();

// Get formatted playtime
string playtime = StatisticsTracker.Instance.GetFormattedPlaytime();
```

## Common Patterns

### Save Before Major Event
```csharp
void OnPlayerEntersBossRoom()
{
    SaveTriggerSystem.Instance.CreateCheckpoint("Boss Fight Start");
}
```

### Save After Achievement
```csharp
void OnPuzzleCompleted(string puzzleId)
{
    StatisticsTracker.Instance.IncrementPuzzlesSolved();
    SaveTriggerSystem.Instance.OnPuzzleSolved(puzzleId);
}
```

### Handle Save Completion
```csharp
void Start()
{
    SaveGameManager.Instance.OnSaveCompleted += OnSaveCompleted;
    SaveGameManager.Instance.OnSaveFailed += OnSaveFailed;
}

void OnSaveCompleted(SaveData saveData)
{
    Debug.Log($"Game saved: {saveData.locationName}");
    ShowNotification("Game saved successfully");
}

void OnSaveFailed(string error)
{
    Debug.LogError($"Save failed: {error}");
    ShowNotification("Failed to save game");
}
```

### Profile Switching
```csharp
// Get all profiles
List<ProfileData> profiles = ProfileManager.Instance.AllProfiles;

// Get recent profiles
List<ProfileData> recent = ProfileManager.Instance.GetRecentProfiles();

// Switch profile
ProfileManager.Instance.SwitchToProfile(profileId);

// Delete profile (with confirmation!)
ProfileManager.Instance.DeleteProfile(profileId);
```

## Hotkeys

| Key | Action | Description |
|-----|--------|-------------|
| F5 | Quick Save | Saves to quick-save slot |
| F6 | Checkpoint | Creates manual checkpoint |
| F9 | Quick Load | Loads from quick-save (requires confirmation) |

## Event Subscriptions

```csharp
// Profile events
ProfileManager.Instance.OnProfileCreated += OnProfileCreated;
ProfileManager.Instance.OnProfileSwitched += OnProfileSwitched;
ProfileManager.Instance.OnProfileDeleted += OnProfileDeleted;
ProfileManager.Instance.OnProfileUpdated += OnProfileUpdated;

// Save events
SaveGameManager.Instance.OnSaveCompleted += OnSaveCompleted;
SaveGameManager.Instance.OnLoadCompleted += OnLoadCompleted;
SaveGameManager.Instance.OnSaveFailed += OnSaveFailed;
SaveGameManager.Instance.OnLoadFailed += OnLoadFailed;
SaveGameManager.Instance.OnSaveProgress += OnSaveProgress;
SaveGameManager.Instance.OnLoadProgress += OnLoadProgress;

// Auto-save events
SaveTriggerSystem.Instance.OnAutoSaveTriggered += OnAutoSaveTriggered;

// Statistics events
StatisticsTracker.Instance.OnStatisticChanged += OnStatisticChanged;
```

## Save Data Structure

### Accessing Save Data
```csharp
SaveData currentSave = SaveGameManager.Instance.CurrentSaveData;

// Player state
Vector3 position = currentSave.playerState.position.ToVector3();
float health = currentSave.playerState.health;
List<InventoryItem> items = currentSave.playerState.inventory.items;

// World state
string currentScene = currentSave.worldState.currentScene;
List<DoorState> doors = currentSave.worldState.doorStates;
List<string> collectibles = currentSave.worldState.collectiblesFound;

// Mission state
List<ActiveMission> missions = currentSave.missionState.activeMissions;
List<string> completed = currentSave.missionState.completedMissions;

// NPC state
List<EncounteredNPC> npcs = currentSave.npcState.encounteredNPCs;
float hostility = currentSave.npcState.globalHostilityLevel;
```

### Modifying Save Data (Custom Systems)
```csharp
void CaptureMissionData(SaveData saveData)
{
    // Add active missions
    foreach (Mission mission in activeMissions)
    {
        ActiveMission missionData = new ActiveMission
        {
            missionId = mission.id,
            startTimestamp = mission.startTime.ToString("o")
        };
        
        saveData.missionState.activeMissions.Add(missionData);
    }
    
    // Add completed missions
    saveData.missionState.completedMissions.AddRange(completedMissionIds);
}
```

## Error Handling

### Check for Active Profile
```csharp
if (ProfileManager.Instance?.CurrentProfile == null)
{
    Debug.LogWarning("No active profile");
    return;
}
```

### Check for Save in Progress
```csharp
if (SaveGameManager.Instance.IsSaving)
{
    Debug.LogWarning("Save already in progress");
    return;
}
```

### Handle Corrupted Saves
```csharp
SaveGameManager.Instance.OnLoadFailed += (error) =>
{
    if (error.Contains("corrupted"))
    {
        // Try loading from backup
        ShowDialog("Save file corrupted. Attempting to restore from backup...");
    }
};
```

## Performance Tips

1. **Auto-Save Frequency**: Default 5 minutes is balanced. Shorter intervals increase disk I/O.
2. **Async Operations**: Save/load uses coroutines - doesn't block main thread.
3. **File Size**: Keep save data minimal. Remove debug data before save.
4. **Backups**: 3 backups per save is sufficient. More increases disk usage.
5. **Statistics**: Statistics tracker updates in Update() - very lightweight.

## Debugging

### Enable Debug Logging
```csharp
// Already enabled by default in all managers
// Check console for save/load operations
```

### View Save Files
```plaintext
Location: [UserDocuments]/ProtocolEMR/
- Profiles/*.json (human-readable)
- Saves/*.sav (human-readable JSON)
- Backups/*.sav (human-readable JSON)
```

### Inspect Save File
```csharp
string savePath = Path.Combine(
    Application.persistentDataPath, 
    "Saves", 
    "filename.sav"
);
string json = File.ReadAllText(savePath);
SaveData data = JsonUtility.FromJson<SaveData>(json);
```

### Test Corruption Recovery
```csharp
// Manually corrupt a save file
string savePath = GetSavePath(saveId, profileId);
File.WriteAllText(savePath, "corrupted data");

// Attempt to load - should restore from backup
SaveGameManager.Instance.LoadGame(saveId, profileId);
```

## Common Issues

### "No active profile" Warning
**Solution**: Create profile before saving
```csharp
ProfileManager.Instance.CreateProfile("Player1");
ProfileManager.Instance.SwitchToProfile(profileId);
```

### Save Not Persisting
**Solution**: Verify Application.persistentDataPath is writable
```csharp
Debug.Log($"Save location: {Application.persistentDataPath}");
// Check directory exists and has write permissions
```

### Auto-Save Not Triggering
**Solution**: Ensure SaveTriggerSystem is initialized
```csharp
if (SaveTriggerSystem.Instance == null)
{
    Debug.LogError("SaveTriggerSystem not initialized");
}
```

### Load Fails with "Checksum Mismatch"
**Solution**: Save file corrupted - restore from backup
```csharp
// Automatic restoration is attempted
// If all backups fail, repair is attempted
```

## Best Practices

1. ✅ **Always create profile first** before saving
2. ✅ **Use auto-save triggers** for important events
3. ✅ **Subscribe to save events** for UI feedback
4. ✅ **Track statistics** for player engagement metrics
5. ✅ **Create checkpoints** before major events
6. ✅ **Test corruption recovery** during development
7. ✅ **Don't save every frame** - use event-based triggers
8. ✅ **Validate data** before adding to save structures
9. ✅ **Use async operations** to avoid blocking gameplay
10. ✅ **Provide clear feedback** to player on save/load

## API Summary

### ProfileManager
- `CreateProfile(name, avatar, difficulty)` - Create new profile
- `SwitchToProfile(profileId)` - Switch active profile
- `DeleteProfile(profileId)` - Delete profile and saves
- `GetRecentProfiles()` - Get last 3 profiles
- `UpdateProfileStats(stats)` - Update profile statistics

### SaveGameManager
- `SaveGame(slotType, slotIndex, customName)` - Save game
- `LoadGame(saveId, profileId)` - Load game
- `QuickSave()` - Quick save (F5)
- `QuickLoad()` - Quick load (F9)
- `AutoSave()` - Trigger auto-save
- `CreateCheckpoint(name)` - Create checkpoint
- `GetSaveSlots(slotTypeFilter)` - Get available saves
- `DeleteSave(saveId, profileId)` - Delete specific save

### StatisticsTracker
- `IncrementMissionsCompleted()` - +1 mission
- `IncrementNPCsEncountered()` - +1 NPC
- `IncrementDeaths()` - +1 death
- `IncrementCollectiblesFound()` - +1 collectible
- `IncrementPuzzlesSolved()` - +1 puzzle
- `IncrementSecretsFound()` - +1 secret
- `GetFormattedPlaytime()` - Get HH:MM:SS playtime

### SaveTriggerSystem
- `OnMissionStarted(missionId)` - Trigger save on mission start
- `OnMissionCompleted(missionId)` - Trigger save on completion
- `OnPuzzleSolved(puzzleId)` - Trigger save on puzzle
- `OnCombatStarted()` - Trigger save on combat
- `CreateCheckpoint(name)` - Create named checkpoint
- `ForceAutoSave(reason)` - Force immediate save

## Integration Examples

### With Mission System
```csharp
public class MissionManager : MonoBehaviour
{
    void OnMissionStart(string missionId)
    {
        SaveTriggerSystem.Instance.OnMissionStarted(missionId);
    }
    
    void OnMissionComplete(string missionId)
    {
        StatisticsTracker.Instance.IncrementMissionsCompleted();
        SaveTriggerSystem.Instance.OnMissionCompleted(missionId);
    }
}
```

### With NPC System
```csharp
public class NPCController : MonoBehaviour
{
    void OnPlayerDetected()
    {
        StatisticsTracker.Instance.IncrementNPCsEncountered();
    }
}
```

### With Collectible System
```csharp
public class Collectible : MonoBehaviour
{
    void OnCollected()
    {
        StatisticsTracker.Instance.IncrementCollectiblesFound();
        SaveTriggerSystem.Instance.OnCollectibleFound(collectibleId);
    }
}
```
