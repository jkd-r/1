# Challenge Modes & Hardcore Play

## Table of Contents

1. [Overview](#overview)
2. [Hardcore Mode](#hardcore-mode)
3. [Speedrun Mode](#speedrun-mode)
4. [Iron Mode (Survival)](#iron-mode-survival)
5. [Puzzle-Only Mode](#puzzle-only-mode)
6. [Nightmare Difficulty](#nightmare-difficulty)
7. [Custom Difficulty Settings](#custom-difficulty-settings)
8. [Leaderboard Integration](#leaderboard-integration)
9. [Achievements & Badges](#achievements--badges)
10. [Technical Requirements](#technical-requirements)
11. [Implementation Guide](#implementation-guide)
12. [Prototype Deliverables](#prototype-deliverables)
13. [QA Checklist](#qa-checklist)

## Overview

Challenge modes provide optional gameplay variants that enhance Protocol EMR's replayability and cater to different player preferences. Each mode modifies core gameplay mechanics while maintaining the atmospheric, narrative-driven experience of the base game.

### Design Philosophy

- **Player Choice**: All challenge modes are optional and can be selected at game start
- **Progressive Difficulty**: Modes stack logically (Hardcore + Nightmare, etc.)
- **Fair Challenge**: Difficulty increases through meaningful constraints, not arbitrary punishment
- **Replay Value**: Each mode offers a distinct experience worth multiple playthroughs
- **Accessibility**: Core game remains accessible while challenges cater to experienced players

## Hardcore Mode

### Core Features

| Feature | Description | Implementation |
|---------|-------------|----------------|
| **Permadeath** | One mistake ends the run, restart from beginning | Auto-delete save file on death, force new game |
| **Limited Saves** | No quicksave, only checkpoint saves | Disable manual save, enable only auto-checkpoints |
| **Increased Difficulty** | Enemies deal 1.5x damage, have 1.3x health | Modify enemy stat multipliers via DifficultyManager |
| **Resource Scarcity** | 50% fewer health items, 40% less ammo | Adjust spawn rates in ProceduralGenerator |
| **UI Indicator** | Red skull icon permanently displayed | Add HardcoreModeHUD component to player UI |
| **Stats Tracking** | Playtime, missions completed, death cause | HardcoreStatsManager with persistent storage |

### Permadeath System

```csharp
public class HardcoreMode : MonoBehaviour
{
    [SerializeField] private bool permadeathEnabled = true;
    [SerializeField] private bool allowQuickSaves = false;
    [SerializeField] private float enemyDamageMultiplier = 1.5f;
    [SerializeField] private float enemyHealthMultiplier = 1.3f;
    [SerializeField] private float resourceScarcity = 0.5f;
    
    public void OnPlayerDeath()
    {
        if (permadeathEnabled)
        {
            HardcoreStatsManager.RecordDeath();
            SaveManager.DeleteHardcoreSave();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
```

### Hardcore Statistics

| Stat | Tracking Method | Display Location |
|------|----------------|------------------|
| Total Playtime | Time.time accumulation | Main Menu → Stats |
| Missions Completed | Event counter | Death screen summary |
| Enemies Defeated | Kill counter | End screen stats |
| Rooms Explored | Area tracker | Progress percentage |
| Death Cause | Last damage source | Death screen |
| Best Run | High score system | Leaderboards |

## Speedrun Mode

### Core Features

| Feature | Description | Technical Details |
|---------|-------------|-------------------|
| **In-Game Timer** | Prominent display showing elapsed time | HUD overlay with millisecond precision |
| **Global Leaderboards** | Online ranking of completion times | REST API integration, local caching |
| **Category Support** | Any%, 100%, Puzzle-only divisions | Separate leaderboard tables |
| **Seed Sharing** | Copy map seed for same-layout races | String-based seed generation |
| **Replay Recording** | Optional ghost/replay functionality | Input recording and playback system |
| **Time Validation** | Anti-cheat protection for submitted times | Server-side validation |

### Speedrun Categories

| Category | Requirements | Typical Completion |
|----------|--------------|-------------------|
| **Any%** | Complete main story, ignore optional content | 20-45 minutes |
| **100%** | All missions, collectibles, puzzles solved | 45-90 minutes |
| **Puzzle-Only** | Complete all puzzles, skip combat | 15-30 minutes |
| **Hardcore%** | Any% rules + permadeath active | 25-50 minutes |

### Timer Implementation

```csharp
public class SpeedrunTimer : MonoBehaviour
{
    private float startTime;
    private bool isRunning;
    private bool isPaused;
    
    [SerializeField] private TextMeshProUGUI timerDisplay;
    [SerializeField] private GameObject timerPanel;
    
    public void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
        timerPanel.SetActive(true);
    }
    
    public void StopTimer()
    {
        isRunning = false;
        float finalTime = Time.time - startTime;
        LeaderboardManager.SubmitTime(finalTime, currentCategory);
    }
    
    private void Update()
    {
        if (isRunning && !isPaused)
        {
            float elapsed = Time.time - startTime;
            timerDisplay.text = FormatTime(elapsed);
        }
    }
    
    private string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return $"{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
    }
}
```

## Iron Mode (Survival)

### Core Features

| Feature | Description | Implementation |
|---------|-------------|----------------|
| **Limited Resources** | Ammo, health, tools break permanently | Durability system, limited spawn pools |
| **No Crafting** | Items cannot be created or repaired | Disable crafting systems entirely |
| **Environmental Hazards** | Increasing danger over time | Progressive hazard intensity system |
| **Wave-Based Enemies** | Timed enemy encounters | WaveManager with increasing difficulty |
| **Progressive Difficulty** | Challenge scales with survival time | Dynamic difficulty adjustment |

### Resource Management

| Resource | Starting Amount | Max Capacity | Break Chance |
|----------|----------------|--------------|--------------|
| **Health Kits** | 2 | 5 | N/A (consumable) |
| **Ammo Clips** | 3 | 10 | N/A (consumable) |
| **Flashlight** | 1 | 1 | 15% per use |
| **Multi-Tool** | 1 | 1 | 10% per interaction |
| **Lockpicks** | 2 | 5 | 20% per lock |

### Wave System

```csharp
public class IronWaveManager : MonoBehaviour
{
    [SerializeField] private float baseWaveInterval = 120f; // 2 minutes
    [SerializeField] private float difficultyIncreaseRate = 0.15f;
    [SerializeField] private int maxWaves = 20;
    
    private int currentWave = 0;
    private float nextWaveTime;
    
    public void Update()
    {
        if (Time.time >= nextWaveTime && currentWave < maxWaves)
        {
            SpawnWave();
            currentWave++;
            nextWaveTime = Time.time + CalculateWaveInterval();
        }
    }
    
    private float CalculateWaveInterval()
    {
        return baseWaveInterval * (1f - (currentWave * difficultyIncreaseRate));
    }
    
    private void SpawnWave()
    {
        int enemyCount = Mathf.RoundToInt(3f + (currentWave * 1.5f));
        // Spawn enemies with increased stats
    }
}
```

## Puzzle-Only Mode

### Core Features

| Feature | Description | Implementation |
|---------|-------------|----------------|
| **No Combat** | All enemy encounters disabled | Enemy spawning disabled, AI aggression set to 0 |
| **Focus on Puzzles** | Logic puzzles remain fully functional | Puzzle systems unchanged |
| **Peaceful Exploration** | Safe environment for story focus | Remove all hostile entities |
| **Enhanced Hints** | "Unknown" provides more guidance | Increase hint frequency and detail |
| **Story Emphasis** | Focus on narrative and lore | Enable all story triggers, reduce time pressure |

### Mode Modifications

| System | Normal Mode | Puzzle-Only Mode |
|--------|-------------|------------------|
| **Enemy Spawning** | Enabled based on difficulty | Completely disabled |
| **Health System** | Damage from enemies | Environmental damage only |
| **Narrator Hints** | Standard frequency | 2x frequency, more detailed |
| **Time Pressure** | Mission deadlines | Relaxed or removed |
| **Resource Scarcity** | Limited supplies | Abundant resources |

## Nightmare Difficulty

### Core Features

| Feature | Description | Multiplier |
|---------|-------------|------------|
| **Complex Layouts** | More intricate procedural generation | 1.5x room complexity |
| **Smarter Enemies** | Enhanced AI tactics and coordination | 2.0x AI intelligence |
| **Resource Scarcity** | Severely limited supplies | 0.3x normal spawn rate |
| **Fast Reactions** | Enemies detect and engage quicker | 0.5x reaction time |
| **Reduced HUD** | Minimal guidance and indicators | 50% less UI help |

### Nightmare AI Enhancements

| AI Behavior | Normal | Nightmare |
|-------------|--------|-----------|
| **Vision Range** | 15 units | 25 units |
| **Hearing Radius** | 10 units | 18 units |
| **Reaction Time** | 0.5 seconds | 0.2 seconds |
| **Coordination** | Individual | Group tactics |
| **Patrol Patterns** | Fixed routes | Adaptive routes |
| **Memory Duration** | 30 seconds | 120 seconds |

## Custom Difficulty Settings

### Granular Controls

| Setting | Range | Default | Effect |
|---------|-------|---------|--------|
| **Enemy Damage** | 0.5x - 3.0x | 1.0x | Damage dealt to player |
| **Player Health** | 50 - 200 HP | 100 HP | Maximum health pool |
| **Resource Scarcity** | 0.2x - 2.0x | 1.0x | Item spawn frequency |
| **Enemy Accuracy** | 0.3x - 1.5x | 1.0x | AI hit chance |
| **Hint Frequency** | 0.1x - 3.0x | 1.0x | Narrator help frequency |
| **Time Pressure** | Relaxed - Intense | Normal | Mission deadline strictness |
| **Puzzle Complexity** | Simple - Expert | Normal | Logic puzzle difficulty |

### Preset System

```csharp
[System.Serializable]
public class DifficultyPreset
{
    public string presetName;
    public float enemyDamageMultiplier = 1.0f;
    public float playerHealthMultiplier = 1.0f;
    public float resourceScarcity = 1.0f;
    public float enemyAccuracy = 1.0f;
    public float hintFrequency = 1.0f;
    public float timePressure = 1.0f;
    public float puzzleComplexity = 1.0f;
    
    public void ApplyPreset()
    {
        DifficultyManager.SetEnemyDamage(enemyDamageMultiplier);
        DifficultyManager.SetPlayerHealth(playerHealthMultiplier);
        DifficultyManager.SetResourceScarcity(resourceScarcity);
        // Apply other settings...
    }
}
```

### Built-in Presets

| Preset | Target Audience | Key Features |
|--------|----------------|--------------|
| **Story Mode** | Casual players | Low difficulty, high hints |
| **Balanced** | General audience | Default game experience |
| **Challenge** | Experienced players | Increased difficulty, normal hints |
| **Hardcore** | Expert players | High difficulty, minimal help |
| **Custom** | Personal preference | User-defined settings |

## Leaderboard Integration

### Leaderboard Structure

| Category | Sorting | Requirements | Validation |
|----------|---------|--------------|------------|
| **Any% Speedrun** | Fastest time | Complete main story | Seed verification |
| **100% Speedrun** | Fastest time | All objectives complete | Progress validation |
| **Hardcore Survival** | Longest time | Iron mode rules | Resource tracking |
| **Puzzle Master** | Fastest time | Puzzle-only mode | Solution verification |
| **Nightmare Clear** | Fastest time | Beat on Nightmare | Difficulty confirmation |

### Anti-Cheat Measures

| Protection | Method | Implementation |
|------------|--------|----------------|
| **Time Validation** | Server-side verification | Compare client/server timestamps |
| **Seed Verification** | Map generation consistency | Validate procedural seed |
| **Progress Tracking** | Objective completion | Verify required milestones |
| **Resource Validation** | Item acquisition tracking | Check for impossible stats |
| **Behavior Analysis** | Anomaly detection | Flag suspicious patterns |

### Leaderboard API

```csharp
public class LeaderboardManager : MonoBehaviour
{
    private const string API_BASE_URL = "https://api.protocolemr.com/leaderboards";
    
    public async Task SubmitScore(LeaderboardEntry entry)
    {
        string json = JsonUtility.ToJson(entry);
        
        using (UnityWebRequest request = new UnityWebRequest(API_BASE_URL + "/submit", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Score submitted successfully");
            }
            else
            {
                Debug.LogError("Score submission failed: " + request.error);
            }
        }
    }
}
```

## Achievements & Badges

### Achievement System

| Achievement | Requirements | Badge Type | Unlock Condition |
|-------------|--------------|------------|------------------|
| **Hardcore Survivor** | Complete game in Hardcore mode | Gold | Finish story with permadeath |
| **Speed Demon** | Sub-30 minute speedrun | Silver | Any% under 30 minutes |
| **Perfect Run** | Complete game without taking damage | Platinum | Zero damage from start to finish |
| **Collector** | Find all collectibles | Bronze | 100% items discovered |
| **Puzzle Master** | Solve all puzzles without hints | Gold | Zero hints used |
| **Iron Will** | Survive 10 waves in Iron mode | Silver | Wave 10 completion |
| **Nightmare Conqueror** | Beat game on Nightmare | Gold | Story completion |
| **Speedrunner** | Top 100 leaderboard position | Silver | Rank achievement |
| **Explorer** | Discover all secret areas | Bronze | 100% map coverage |
| **Story Devotee** | Complete in Puzzle-Only mode | Bronze | Story completion |

### Achievement Tracking

```csharp
public class AchievementManager : MonoBehaviour
{
    [System.Serializable]
    public class Achievement
    {
        public string achievementId;
        public string displayName;
        public string description;
        public BadgeType badgeType;
        public bool isUnlocked;
        public DateTime unlockDate;
    }
    
    public Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();
    
    public void UnlockAchievement(string achievementId)
    {
        if (achievements.ContainsKey(achievementId) && !achievements[achievementId].isUnlocked)
        {
            achievements[achievementId].isUnlocked = true;
            achievements[achievementId].unlockDate = DateTime.Now;
            
            // Show notification
            UIManager.ShowAchievementUnlocked(achievements[achievementId]);
            
            // Save progress
            SaveAchievements();
            
            // Check for meta-achievements
            CheckMetaAchievements();
        }
    }
}
```

## Technical Requirements

### Core Systems

| System | Requirements | Priority |
|--------|--------------|----------|
| **Difficulty Manager** | Centralized difficulty scaling | High |
| **Save System Extensions** | Hardcore mode save handling | High |
| **Timer System** | Precision timing for speedruns | High |
| **Leaderboard Backend** | Optional online integration | Medium |
| **Achievement Tracking** | Local achievement system | Medium |
| **UI Extensions** | Mode-specific interfaces | High |

### Performance Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Mode Switching** | < 2 seconds | Load time testing |
| **Timer Precision** | ±10ms | Frame timing analysis |
| **Leaderboard Updates** | < 5 seconds | Network performance |
| **Achievement Unlock** | < 1 second | UI responsiveness |
| **Save File Size** | < 1MB | Storage optimization |

### Memory Management

| Component | Memory Budget | Optimization |
|-----------|---------------|--------------|
| **Leaderboard Cache** | 5MB | Periodic cleanup |
| **Achievement Data** | 500KB | JSON compression |
| **Timer Data** | 100KB | Minimal storage |
| **Mode Settings** | 200KB | ScriptableObjects |

## Implementation Guide

### Phase 1: Core Infrastructure (Week 1-2)

1. **Difficulty Manager Implementation**
   - Create centralized difficulty scaling system
   - Implement multiplier-based stat modification
   - Add preset configuration system

2. **Save System Extensions**
   - Add Hardcore mode save deletion
   - Implement checkpoint-only saving
   - Create stats tracking system

3. **Timer System**
   - Build precision timer component
   - Add HUD integration
   - Implement time formatting

### Phase 2: Mode Implementation (Week 3-4)

1. **Hardcore Mode**
   - Implement permadeath mechanics
   - Add UI indicators
   - Create stats tracking

2. **Speedrun Mode**
   - Complete timer integration
   - Add category support
   - Implement seed sharing

3. **Iron Mode**
   - Build resource management system
   - Create wave spawning mechanics
   - Add progressive difficulty

### Phase 3: Advanced Features (Week 5-6)

1. **Custom Difficulty**
   - Implement granular settings UI
   - Add preset management
   - Create save/load functionality

2. **Achievement System**
   - Build achievement tracking
   - Add unlock notifications
   - Implement progress checking

3. **Leaderboard Integration**
   - Create local leaderboard system
   - Add online API integration (optional)
   - Implement validation system

## Prototype Deliverables

### Minimum Viable Product

| Feature | Status | Acceptance Criteria |
|---------|--------|---------------------|
| **Hardcore Mode** | Required | Permadeath works, saves deleted on death |
| **Speedrun Mode** | Required | Timer displays, times recorded |
| **Basic UI** | Required | Mode selection, indicators functional |
| **Save System** | Required | Hardcore saves handled correctly |

### Enhanced Features

| Feature | Status | Acceptance Criteria |
|---------|--------|---------------------|
| **Iron Mode** | Desired | Resource depletion, wave system working |
| **Custom Difficulty** | Desired | Sliders functional, presets saveable |
| **Achievements** | Desired | Basic tracking, unlock notifications |
| **Local Leaderboards** | Desired | Times sorted, categories working |

### Stretch Goals

| Feature | Status | Acceptance Criteria |
|---------|--------|---------------------|
| **Online Leaderboards** | Optional | API integration, validation working |
| **Replay System** | Optional | Input recording, playback functional |
| **Nightmare Difficulty** | Optional | Enhanced AI, complex generation |
| **Puzzle-Only Mode** | Optional | Combat disabled, hints enhanced |

## QA Checklist

### Hardcore Mode Testing

- [ ] Permadeath triggers correctly on player death
- [ ] Save files are deleted immediately upon death
- [ ] Player returns to main menu after death
- [ ] Hardcore stats are tracked accurately
- [ ] UI indicator displays correctly
- [ ] Enemy difficulty multipliers apply properly
- [ ] Resource scarcity functions as specified
- [ ] Checkpoint saves work but quicksaves are disabled

### Speedrun Mode Testing

- [ ] Timer starts at correct moment
- [ ] Timer displays with proper precision
- [ ] Timer stops on completion
- [ ] Time formatting is correct (MM:SS.mmm)
- [ ] Categories are properly tracked
- [ ] Seed sharing generates consistent layouts
- [ ] Local leaderboards sort correctly
- [ ] Speedrun stats persist between sessions

### Cross-Mode Testing

- [ ] Modes can be combined (Hardcore + Nightmare)
- [ ] Settings don't conflict between modes
- [ ] Mode selection UI is clear and functional
- [ ] Settings persist correctly in save files
- [ ] Performance impact is minimal
- [ ] Memory usage stays within budget
- [ ] All modes maintain game stability
- [ ] Difficulty scaling feels balanced

### Achievement System Testing

- [ ] Achievements unlock at correct conditions
- [ ] Unlock notifications display properly
- [ ] Achievement progress saves correctly
- [ ] Meta-achievements trigger appropriately
- [ ] Badge icons display correctly
- [ ] Achievement list updates in real-time
- [ ] Stats tracking is accurate
- [ ] No false positives or missed unlocks

### Performance Testing

- [ ] Mode switching completes within 2 seconds
- [ ] Timer maintains precision during gameplay
- [ ] FPS impact is minimal (<5% drop)
- [ ] Memory usage stays within allocated budgets
- [ ] Save/load times are acceptable
- [ ] Leaderboard updates complete quickly
- [ ] No memory leaks detected
- [ ] Performance is consistent across modes

### Accessibility Testing

- [ ] Mode options are clearly explained
- [ ] Custom difficulty sliders are intuitive
- [ ] UI elements are readable for all modes
- [ ] Colorblind-friendly indicators
- [ ] Text scaling works properly
- [ ] Controller support maintained
- [ ] Audio cues complement visual indicators
- [ ] Difficulty feels fair and balanced

### Integration Testing

- [ ] Challenge modes work with existing systems
- [ ] NPC AI responds correctly to difficulty changes
- [ ] Procedural generation respects mode settings
- [ ] Audio systems adapt to mode requirements
- [ ] Mission system integrates properly
- [ ] Save system handles all mode variants
- [ ] UI systems display mode-specific elements
- [ ] Event system passes mode context correctly

---

**Document Version**: 1.0  
**Last Updated**: 2024-11-19  
**Next Review**: 2024-11-26  
**Status**: Draft - Ready for Implementation