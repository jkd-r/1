# NPC AI System - Sprint 7

## Overview

This comprehensive NPC AI system provides advanced artificial intelligence for non-player characters with procedural behaviors, state machines, perception systems, and group coordination. The system is designed to create intelligent, varied, and challenging NPCs that can patrol, chase, flee, hide, attack, and coordinate with each other.

## System Architecture

### Core Components

1. **NPCController** - Main AI controller that orchestrates all AI systems
2. **BehaviorTree** - Hierarchical state machine for complex AI behaviors
3. **NPCPerception** - Vision and hearing system for environmental awareness
4. **NPCNavigation** - NavMesh-based pathfinding and movement
5. **NPCCombat** - Combat mechanics including attacks, damage, and dodging
6. **NPCAnimationController** - Animation state management and blending
7. **NPCManager** - Global system for NPC coordination and alerts
8. **NPCSpawner** - Procedural NPC generation and zone-based spawning
9. **DifficultyManager** - Dynamic difficulty scaling system

### NPC Types

- **Guard**: Balanced combatants with good perception and tactics
- **Scientist**: High intelligence, low aggression, prioritizes fleeing
- **Civilian**: Moderate stats, tends to flee from danger
- **Beast**: High aggression and health, low intelligence, aggressive pursuit

## Key Features

### Behavior Tree System

The behavior tree uses a hierarchical structure of nodes:
- **Selector Nodes**: Execute children until one succeeds
- **Sequence Nodes**: Execute children until one fails
- **Condition Nodes**: Check game states (CanSeePlayer, HealthLow, etc.)
- **Action Nodes**: Perform behaviors (ChaseAndAttack, Flee, Patrol, etc.)

Example Guard Behavior Tree:
```
Root Selector:
├─ Sequence: Is Stunned? → Play Stun Animation
├─ Sequence: Is Dead? → Play Death Animation
├─ Selector: Combat Behaviors
│  ├─ Sequence: Can See Player AND In Range → Chase & Attack
│  └─ Sequence: Heard Player → Investigate Sound
└─ Selector: Movement Behaviors
   ├─ Sequence: Health < 25% → Flee to Safezone
   └─ Patrol Routine
```

### Perception System

- **Cone of Vision**: Configurable field of view (120° default) and range
- **Line of Sight**: Raycast-based vision blocked by obstacles
- **Hearing**: Detects footsteps, gunshots, and other sounds
- **Memory**: Remembers player position for 10 seconds after losing sight
- **Alertness**: Gradual awareness that increases with stimuli

### Navigation & Pathfinding

- **NavMesh Integration**: Uses Unity's built-in NavMesh system
- **Intelligent Pathfinding**: Recalculates paths every 0.5 seconds during chase
- **Stuck Detection**: Automatically handles stuck situations with recovery
- **Cover Seeking**: Finds and utilizes environmental cover
- **Patrol System**: Automatic patrol point generation and navigation

### Combat System

- **Attack Variety**: Multiple attack animations with procedural selection
- **Damage System**: Configurable damage values with difficulty scaling
- **Dodging**: Intelligence-based dodge chance with cooldowns
- **Knockback**: Physics-based knockback forces
- **Stamina**: NPCs have limited stamina for sprinting
- **Health States**: Different behaviors based on health thresholds

### Group Behavior

- **Alert Propagation**: NPCs alert nearby allies to threats
- **Coordinated Attacks**: Groups attempt to flank and surround players
- **Morale System**: Group morale affects behavior and cohesion
- **Leadership**: Intelligent NPCs lead groups with tactical decisions
- **Retreat Coordination**: Groups can coordinate organized retreats

### Procedural Generation

- **Parameter Variation**: Each NPC has procedurally varied stats within type
- **Seed-Based Generation**: Same seed produces identical NPC variations
- **Zone-Based Spawning**: Different areas have specific NPC populations
- **Dynamic Difficulty**: Stats scale with global difficulty settings

## Performance Targets

- **Per-NPC AI Tick**: <1ms per frame
- **Behavior Tree Evaluation**: <2ms per NPC
- **Pathfinding**: <1ms per NPC per 0.5 seconds
- **Perception Raycast**: <0.5ms per NPC per frame
- **Memory Usage**: <500KB per NPC
- **Maximum Active NPCs**: 20+ simultaneous at 60 FPS

## Usage Guide

### Basic Setup

1. Add the **NPCController** component to any GameObject
2. Ensure the GameObject has:
   - NavMeshAgent component
   - Animator component
   - Rigidbody component
3. Configure NPC type and parameters in the inspector
4. Set up patrol points or enable auto-generation

### Spawning NPCs

```csharp
// Create NPC prefab
GameObject npc = NPCPrefabConfiguration.CreateNPCPrefab(NPCType.Guard);

// Position NPC
npc.transform.position = spawnPosition;

// Register with manager
NPCController controller = npc.GetComponent<NPCController>();
NPCManager.Instance.RegisterNPC(controller);
```

### Zone-Based Spawning

```csharp
// Create spawn zone
NPCSpawnZone zone = new NPCSpawnZone
{
    zoneName = "Guard Patrol Area",
    allowedNPCTypes = new NPCType[] { NPCType.Guard },
    minNPCs = 2,
    maxNPCs = 4,
    spawnOnLevelLoad = true
};

// Add to spawner
NPCSpawner spawner = FindObjectOfType<NPCSpawner>();
spawner.SpawnZone(zone);
```

### Creating Alerts

```csharp
// Create global alert
Vector3 alertPosition = player.transform.position;
NPCManager.Instance.CreateGlobalAlert(
    alertPosition, 
    player.gameObject, 
    100f, 
    AlertType.AttackDetected
);
```

### Difficulty Scaling

```csharp
// Set difficulty
DifficultyManager.Instance.SetDifficulty(DifficultyLevel.Hard);

// Apply difficulty to NPC parameters
NPCParameters scaledParams = DifficultyManager.Instance.ApplyDifficultyToNPC(
    baseParameters, 
    NPCType.Guard
);
```

## Configuration

### NPC Parameters

```csharp
public class NPCParameters
{
    // Movement Speeds (m/s)
    public float walkSpeed = 4.0f;
    public float runSpeed = 8.0f;
    public float sprintSpeed = 12.0f;
    
    // Health & Combat
    public float maxHealth = 50.0f;
    public float damagePerHit = 15.0f;
    public float attackFrequency = 1.0f;
    
    // Perception
    public float perceptionRange = 15.0f;
    public float fieldOfView = 120.0f;
    public float hearingRange = 10.0f;
    public float reactionTime = 0.5f;
    
    // Behavior
    public float aggression = 70.0f;      // 0-100%
    public float intelligence = 60.0f;     // 0-100%
    public float morale = 80.0f;          // 0-100%
}
```

### Difficulty Settings

Each difficulty level has specific multipliers:
- **Easy**: 0.6x health, 0.5x damage, 0.7x perception
- **Normal**: 1.0x all stats (baseline)
- **Hard**: 1.5x health, 1.25x damage, improved tactics
- **Nightmare**: 2.0x health, 1.5x damage, coordinated groups

## Demo System

The included **NPCAIDemoController** provides comprehensive testing:

### Controls
- **F2**: Toggle demo UI
- **F3**: Spawn random NPC
- **F4**: Clear all NPCs
- **F5**: Trigger global alert
- **1-4**: Run test scenarios

### Test Scenarios
1. **Patrol Test**: Tests NPC patrol behaviors
2. **Combat Test**: Tests combat and attack behaviors
3. **Flee Test**: Tests flee and hide behaviors
4. **Group Test**: Tests group coordination and tactics

## Integration Points

### With Previous Sprints
- **Sprint 2**: Uses animation system for NPC locomotion
- **Sprint 3**: Integrates with combat system for damage/reactions
- **Sprint 4**: Uses NavMesh pathfinding from movement system
- **Sprint 6**: Will integrate with save/load system for NPC state

### Future Sprints
- **Sprint 8**: Will integrate with Unknown system for behavior modification
- **Sprint 9**: Will use procedural generation for level-specific NPC placement
- **Sprint 10**: Will enhance with advanced AI behaviors and learning

## Best Practices

### Performance Optimization
1. Limit active NPCs to maintain 60 FPS
2. Use behavior tree update intervals (0.1s default)
3. Optimize perception raycasts with layer masks
4. Pool NPC objects instead of frequent instantiation

### Design Guidelines
1. Vary NPC types within areas for interesting encounters
2. Use procedural generation for replayability
3. Balance difficulty scaling for player progression
4. Implement proper audio feedback for NPC behaviors

### Debugging
1. Enable debug gizmos to visualize perception cones
2. Use demo controller for testing specific behaviors
3. Monitor performance metrics with built-in profiling
4. Check console logs for state changes and alerts

## Troubleshooting

### Common Issues

**NPCs not moving:**
- Check NavMesh is baked correctly
- Verify NavMeshAgent is enabled and configured
- Ensure spawn positions are on NavMesh

**NPCs not seeing player:**
- Verify layer masks in perception system
- Check field of view and range settings
- Ensure player has "Player" tag

**Performance issues:**
- Reduce maximum NPC count
- Increase behavior tree update intervals
- Optimize perception raycast frequency

**Group behavior not working:**
- Ensure NPCManager is present in scene
- Check communication range settings
- Verify NPCs are registered with manager

## Future Enhancements

### Planned Features
- Learning AI that adapts to player behavior
- Advanced tactical formations
- Environmental interaction and object usage
- Voice lines and contextual dialogue
- Advanced stealth detection mechanics
- Dynamic faction systems

### Performance Improvements
- GPU-accelerated perception calculations
- LOD system for AI complexity
- Multithreaded behavior tree evaluation
- Optimized memory management

## Credits

This NPC AI system was developed for Protocol EMR as part of Sprint 7: NPC & Communication Phase. It provides a foundation for intelligent, engaging NPC interactions that enhance gameplay through procedural variety and emergent behaviors.