# Save/Load System - Integration Guide

## Overview

This guide explains how to integrate the save/load system with various game systems including inventory, missions, NPCs, combat, and world state management.

## Integration Architecture

```
GameManager
    ├── ProfileManager (manages profiles)
    ├── SaveGameManager (orchestrates save/load)
    │   ├── CapturePlayerState() → PlayerController, HealthSystem
    │   ├── CaptureWorldState() → WorldStateManager, DoorManager
    │   ├── CaptureMissionState() → MissionManager
    │   ├── CaptureNPCState() → NPCManager, NPCController
    │   └── CaptureInventoryState() → InventoryManager
    ├── StatisticsTracker (tracks player statistics)
    └── SaveTriggerSystem (event-based auto-saves)
```

## Player State Integration

### PlayerController Integration

The PlayerController integration is already implemented in SaveGameManager:

```csharp
// In SaveGameManager.CapturePlayerState()
PlayerController player = FindObjectOfType<PlayerController>();
if (player != null)
{
    saveData.playerState.position = new Vector3Serializable(player.transform.position);
    saveData.playerState.rotation = new Vector3Serializable(player.transform.eulerAngles);
    saveData.playerState.stamina = player.CurrentStamina;
}

// In SaveGameManager.ApplyPlayerState()
PlayerController player = FindObjectOfType<PlayerController>();
if (player != null)
{
    player.transform.position = playerState.position.ToVector3();
    player.transform.eulerAngles = playerState.rotation.ToVector3();
    // Stamina is managed internally by PlayerController
}
```

### HealthSystem Integration

```csharp
// In your HealthSystem class
public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public float CurrentHealth => currentHealth;

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            OnPlayerDeath();
        }
    }

    private void OnPlayerDeath()
    {
        // Track death statistic
        StatisticsTracker.Instance?.IncrementDeaths();
    }
}
```

Already integrated in SaveGameManager:
```csharp
// Capture
HealthSystem healthSystem = FindObjectOfType<HealthSystem>();
if (healthSystem != null)
{
    playerState.health = healthSystem.CurrentHealth;
}

// Restore
HealthSystem healthSystem = FindObjectOfType<HealthSystem>();
if (healthSystem != null)
{
    healthSystem.SetHealth(playerState.health);
}
```

## Inventory System Integration

### Inventory Manager Extension

Extend your InventoryManager to support save/load:

```csharp
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    private List<InventoryItem> items = new List<InventoryItem>();
    
    // Capture inventory state for saving
    public InventoryState CaptureInventoryState()
    {
        InventoryState state = new InventoryState
        {
            capacity = maxCapacity,
            currency = currentCurrency
        };
        
        foreach (var item in items)
        {
            InventoryItem itemData = new InventoryItem
            {
                itemId = item.id,
                quantity = item.quantity,
                metadata = new ItemMetadata
                {
                    durability = item.durability,
                    ammo = item.ammo,
                    customization = JsonUtility.ToJson(item.customization)
                }
            };
            state.items.Add(itemData);
        }
        
        return state;
    }
    
    // Restore inventory from loaded state
    public void ApplyInventoryState(InventoryState state)
    {
        items.Clear();
        currentCurrency = state.currency;
        maxCapacity = state.capacity;
        
        foreach (var itemData in state.items)
        {
            // Create and add item to inventory
            Item item = CreateItem(itemData.itemId);
            item.quantity = itemData.quantity;
            item.durability = itemData.metadata.durability;
            item.ammo = itemData.metadata.ammo;
            
            if (!string.IsNullOrEmpty(itemData.metadata.customization))
            {
                item.customization = JsonUtility.FromJson<ItemCustomization>(
                    itemData.metadata.customization
                );
            }
            
            items.Add(item);
        }
        
        OnInventoryChanged?.Invoke();
    }
    
    // Track collectible pickup
    public void AddItem(string itemId, int quantity = 1)
    {
        // Add to inventory...
        
        // Track statistic
        if (IsCollectible(itemId))
        {
            StatisticsTracker.Instance?.IncrementCollectiblesFound();
            SaveTriggerSystem.Instance?.OnCollectibleFound(itemId);
        }
    }
}
```

Update SaveGameManager:
```csharp
// In CapturePlayerState()
if (InventoryManager.Instance != null)
{
    playerState.inventory = InventoryManager.Instance.CaptureInventoryState();
}

// In ApplyPlayerState()
if (InventoryManager.Instance != null)
{
    InventoryManager.Instance.ApplyInventoryState(playerState.inventory);
}
```

## Mission System Integration

### Mission Manager Extension

```csharp
public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }
    
    private List<Mission> activeMissions = new List<Mission>();
    private List<string> completedMissions = new List<string>();
    private List<string> failedMissions = new List<string>();
    
    // Capture mission state for saving
    public MissionState CaptureMissionState()
    {
        MissionState state = new MissionState();
        
        // Active missions
        foreach (var mission in activeMissions)
        {
            ActiveMission missionData = new ActiveMission
            {
                missionId = mission.id,
                startTimestamp = mission.startTime.ToString("o")
            };
            
            foreach (var objective in mission.objectives)
            {
                MissionObjective objData = new MissionObjective
                {
                    objectiveId = objective.id,
                    status = objective.status.ToString(),
                    progress = objective.progress
                };
                missionData.objectives.Add(objData);
            }
            
            state.activeMissions.Add(missionData);
        }
        
        // Completed and failed missions
        state.completedMissions.AddRange(completedMissions);
        state.failedMissions.AddRange(failedMissions);
        
        // Mission choices
        state.missionChoices = new Dictionary<string, string>(missionChoices);
        
        return state;
    }
    
    // Restore mission state from loaded data
    public void ApplyMissionState(MissionState state)
    {
        // Clear current missions
        activeMissions.Clear();
        completedMissions.Clear();
        failedMissions.Clear();
        
        // Restore active missions
        foreach (var missionData in state.activeMissions)
        {
            Mission mission = CreateMission(missionData.missionId);
            mission.startTime = DateTime.Parse(missionData.startTimestamp);
            
            foreach (var objData in missionData.objectives)
            {
                var objective = mission.GetObjective(objData.objectiveId);
                if (objective != null)
                {
                    objective.status = Enum.Parse<ObjectiveStatus>(objData.status);
                    objective.progress = objData.progress;
                }
            }
            
            activeMissions.Add(mission);
        }
        
        // Restore completed/failed missions
        completedMissions.AddRange(state.completedMissions);
        failedMissions.AddRange(state.failedMissions);
        
        // Restore mission choices
        missionChoices = new Dictionary<string, string>(state.missionChoices);
        
        OnMissionsChanged?.Invoke();
    }
    
    // Start mission with auto-save
    public void StartMission(string missionId)
    {
        Mission mission = CreateMission(missionId);
        activeMissions.Add(mission);
        
        // Trigger auto-save
        SaveTriggerSystem.Instance?.OnMissionStarted(missionId);
    }
    
    // Complete mission with statistics tracking
    public void CompleteMission(string missionId)
    {
        activeMissions.RemoveAll(m => m.id == missionId);
        completedMissions.Add(missionId);
        
        // Track statistic and trigger auto-save
        StatisticsTracker.Instance?.IncrementMissionsCompleted();
        SaveTriggerSystem.Instance?.OnMissionCompleted(missionId);
    }
}
```

Update SaveGameManager:
```csharp
// In CaptureMissionState()
private void CaptureMissionState(MissionState missionState)
{
    if (MissionManager.Instance != null)
    {
        var capturedState = MissionManager.Instance.CaptureMissionState();
        missionState.activeMissions = capturedState.activeMissions;
        missionState.completedMissions = capturedState.completedMissions;
        missionState.failedMissions = capturedState.failedMissions;
        missionState.missionChoices = capturedState.missionChoices;
    }
}

// In ApplyMissionState()
private void ApplyMissionState(MissionState missionState)
{
    if (MissionManager.Instance != null)
    {
        MissionManager.Instance.ApplyMissionState(missionState);
    }
}
```

## NPC System Integration

### NPC Manager Extension

```csharp
public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }
    
    private List<NPCController> registeredNPCs = new List<NPCController>();
    
    // Capture NPC state for saving
    public NPCState CaptureNPCState()
    {
        NPCState state = new NPCState
        {
            globalHostilityLevel = globalHostility
        };
        
        state.alertStates = new AlertState
        {
            currentAlertLevel = currentAlert.ToString(),
            alertTimer = alertTimer
        };
        
        foreach (var npc in registeredNPCs)
        {
            if (npc.HasBeenEncountered)
            {
                EncounteredNPC npcData = new EncounteredNPC
                {
                    npcId = npc.NPCId,
                    status = npc.CurrentStatus.ToString(),
                    lastKnownPosition = new Vector3Serializable(npc.transform.position),
                    relationshipLevel = npc.RelationshipLevel,
                    dialogueFlags = new List<string>(npc.CompletedDialogues),
                    questGiverState = npc.QuestState.ToString()
                };
                
                state.encounteredNPCs.Add(npcData);
            }
        }
        
        return state;
    }
    
    // Restore NPC state from loaded data
    public void ApplyNPCState(NPCState state)
    {
        globalHostility = state.globalHostilityLevel;
        currentAlert = Enum.Parse<AlertLevel>(state.alertStates.currentAlertLevel);
        alertTimer = state.alertStates.alertTimer;
        
        // Restore individual NPC states
        foreach (var npcData in state.encounteredNPCs)
        {
            NPCController npc = FindNPCById(npcData.npcId);
            if (npc != null)
            {
                npc.SetStatus(Enum.Parse<NPCStatus>(npcData.status));
                npc.SetRelationshipLevel(npcData.relationshipLevel);
                npc.SetCompletedDialogues(npcData.dialogueFlags);
                npc.SetQuestState(Enum.Parse<QuestState>(npcData.questGiverState));
                
                // Restore position if NPC is mobile
                if (npc.IsMobile)
                {
                    npc.transform.position = npcData.lastKnownPosition.ToVector3();
                }
            }
        }
    }
    
    // Track NPC encounter
    public void OnNPCEncountered(NPCController npc)
    {
        if (!npc.HasBeenEncountered)
        {
            npc.MarkAsEncountered();
            StatisticsTracker.Instance?.IncrementNPCsEncountered();
        }
    }
}
```

Update SaveGameManager:
```csharp
// In CaptureNPCState()
private void CaptureNPCState(NPCState npcState)
{
    if (NPCManager.Instance != null)
    {
        var capturedState = NPCManager.Instance.CaptureNPCState();
        npcState.encounteredNPCs = capturedState.encounteredNPCs;
        npcState.globalHostilityLevel = capturedState.globalHostilityLevel;
        npcState.alertStates = capturedState.alertStates;
    }
}

// In ApplyNPCState()
private void ApplyNPCState(NPCState npcState)
{
    if (NPCManager.Instance != null)
    {
        NPCManager.Instance.ApplyNPCState(npcState);
    }
}
```

## World State Integration

### World State Manager

Create a WorldStateManager to track persistent world changes:

```csharp
public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }
    
    private Dictionary<string, ObjectState> objectStates = new Dictionary<string, ObjectState>();
    private Dictionary<string, DoorState> doorStates = new Dictionary<string, DoorState>();
    private HashSet<string> collectedItems = new HashSet<string>();
    private List<PuzzleState> solvedPuzzles = new List<PuzzleState>();
    
    public WorldState CaptureWorldState()
    {
        WorldState state = new WorldState
        {
            currentScene = SceneManager.GetActiveScene().name,
            seed = UnityEngine.Random.seed
        };
        
        // Object states
        foreach (var kvp in objectStates)
        {
            state.objectStates.Add(kvp.Value);
        }
        
        // Door states
        foreach (var kvp in doorStates)
        {
            state.doorStates.Add(kvp.Value);
        }
        
        // Collectibles
        state.collectiblesFound.AddRange(collectedItems);
        
        // Puzzles
        state.puzzlesSolved.AddRange(solvedPuzzles);
        
        return state;
    }
    
    public void ApplyWorldState(WorldState state)
    {
        // Set random seed for procedural content
        UnityEngine.Random.InitState(state.seed);
        
        // Restore object states
        objectStates.Clear();
        foreach (var objState in state.objectStates)
        {
            objectStates[objState.objectId] = objState;
            ApplyObjectState(objState);
        }
        
        // Restore door states
        doorStates.Clear();
        foreach (var doorState in state.doorStates)
        {
            doorStates[doorState.doorId] = doorState;
            ApplyDoorState(doorState);
        }
        
        // Restore collectibles
        collectedItems.Clear();
        collectedItems.UnionWith(state.collectiblesFound);
        
        // Restore puzzles
        solvedPuzzles.Clear();
        solvedPuzzles.AddRange(state.puzzlesSolved);
    }
    
    public void SetObjectState(string objectId, string state)
    {
        objectStates[objectId] = new ObjectState
        {
            objectId = objectId,
            state = state,
            customData = ""
        };
    }
    
    public void SetDoorState(string doorId, bool isOpen, bool isLocked)
    {
        doorStates[doorId] = new DoorState
        {
            doorId = doorId,
            isOpen = isOpen,
            isLocked = isLocked
        };
    }
    
    public void MarkPuzzleSolved(string puzzleId, int attempts, float time)
    {
        solvedPuzzles.Add(new PuzzleState
        {
            puzzleId = puzzleId,
            solvedTimestamp = DateTime.UtcNow.ToString("o"),
            attempts = attempts,
            solutionTime = time
        });
        
        StatisticsTracker.Instance?.IncrementPuzzlesSolved();
        SaveTriggerSystem.Instance?.OnPuzzleSolved(puzzleId);
    }
}
```

Update SaveGameManager:
```csharp
// In CaptureWorldState()
private void CaptureWorldState(WorldState worldState)
{
    worldState.currentScene = SceneManager.GetActiveScene().name;
    worldState.seed = UnityEngine.Random.seed;
    
    if (WorldStateManager.Instance != null)
    {
        var capturedState = WorldStateManager.Instance.CaptureWorldState();
        worldState.objectStates = capturedState.objectStates;
        worldState.doorStates = capturedState.doorStates;
        worldState.collectiblesFound = capturedState.collectiblesFound;
        worldState.puzzlesSolved = capturedState.puzzlesSolved;
    }
}

// In ApplyWorldState()
private void ApplyWorldState(WorldState worldState)
{
    UnityEngine.Random.InitState(worldState.seed);
    
    if (WorldStateManager.Instance != null)
    {
        WorldStateManager.Instance.ApplyWorldState(worldState);
    }
}
```

## Combat System Integration

```csharp
public class CombatManager : MonoBehaviour
{
    public void OnCombatStarted()
    {
        // Track combat encounter
        StatisticsTracker.Instance?.IncrementCombatEncounters();
        SaveTriggerSystem.Instance?.OnCombatStarted();
    }
    
    public void OnStealthKill()
    {
        StatisticsTracker.Instance?.IncrementStealthKills();
    }
}
```

## Dialogue System Integration

```csharp
public class DialogueManager : MonoBehaviour
{
    public void OnDialogueCompleted(string npcId)
    {
        SaveTriggerSystem.Instance?.OnDialogueCompleted(npcId);
    }
}
```

## Quick Integration Checklist

### For New Game Systems

When creating a new game system that needs save/load support:

1. ✅ **Create capture method** in your manager
   ```csharp
   public YourSystemState CaptureState() { ... }
   ```

2. ✅ **Create apply method** in your manager
   ```csharp
   public void ApplyState(YourSystemState state) { ... }
   ```

3. ✅ **Add state structure** to SaveData.cs
   ```csharp
   public YourSystemState yourSystemState;
   ```

4. ✅ **Call capture in SaveGameManager**
   ```csharp
   saveData.yourSystemState = YourManager.Instance.CaptureState();
   ```

5. ✅ **Call apply in SaveGameManager**
   ```csharp
   YourManager.Instance.ApplyState(saveData.yourSystemState);
   ```

6. ✅ **Add statistics tracking** where appropriate
   ```csharp
   StatisticsTracker.Instance?.IncrementYourStatistic();
   ```

7. ✅ **Add auto-save triggers** for important events
   ```csharp
   SaveTriggerSystem.Instance?.OnYourEventHappened(eventId);
   ```

## Testing Your Integration

```csharp
[Test]
public void TestSaveLoad()
{
    // Setup initial state
    YourManager.Instance.SetupTestData();
    
    // Capture state
    var stateBefore = YourManager.Instance.CaptureState();
    
    // Save
    SaveGameManager.Instance.SaveGame(SaveSlotType.ManualSave, 0);
    
    // Modify state
    YourManager.Instance.ModifyData();
    
    // Load
    SaveGameManager.Instance.LoadGame(saveId, profileId);
    
    // Verify state restored
    var stateAfter = YourManager.Instance.CaptureState();
    Assert.AreEqual(stateBefore, stateAfter);
}
```

## Performance Considerations

1. **Minimize saved data**: Only save what's necessary
2. **Use references**: Save IDs instead of full objects
3. **Batch operations**: Group multiple state changes
4. **Async operations**: Use coroutines for large data sets
5. **Validation**: Validate data before saving

## Common Patterns

### Pattern 1: Persistent Objects
```csharp
public class PersistentObject : MonoBehaviour
{
    [SerializeField] private string objectId;
    
    void Start()
    {
        if (WorldStateManager.Instance.IsObjectCollected(objectId))
        {
            gameObject.SetActive(false);
        }
    }
    
    public void OnCollected()
    {
        WorldStateManager.Instance.MarkObjectCollected(objectId);
        gameObject.SetActive(false);
    }
}
```

### Pattern 2: Progressive States
```csharp
public class ProgressiveArea : MonoBehaviour
{
    enum AreaState { Locked, Unlocked, Completed }
    
    void Start()
    {
        AreaState state = WorldStateManager.Instance.GetAreaState(areaId);
        ApplyAreaState(state);
    }
}
```

### Pattern 3: Conditional Content
```csharp
public class ConditionalSpawner : MonoBehaviour
{
    void Start()
    {
        if (WorldStateManager.Instance.HasCompletedMission("mission_01"))
        {
            SpawnEnemies();
        }
    }
}
```

## Troubleshooting

### Data Not Saving
- Verify manager instance exists
- Check capture method is called
- Verify data structure is serializable

### Data Not Loading
- Check apply method is called
- Verify manager exists before restore
- Check for null references

### Performance Issues
- Profile capture/apply methods
- Reduce saved data size
- Use async operations for large data

## Summary

The save/load system is designed to be extensible and easy to integrate with new game systems. Follow the patterns outlined in this guide to ensure consistent and reliable save/load behavior across all game systems.
