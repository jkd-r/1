# Animation System Quick Start Guide

## 5-Minute Setup

### 1. Player Animation Setup

Add these components to your Player GameObject:

```
Player
├── FirstPersonCamera (existing)
├── MovementController (existing)
├── CharacterAnimationController (NEW)
└── PlayerAnimations (NEW)
```

**Steps:**

1. Create an Animator component on Player if not present
2. Add `CharacterAnimationController` component
   - Set Animator reference
   - Configure animation names in AnimationSetup
3. Add `PlayerAnimations` component
   - Reference CharacterAnimationController
   - Reference MovementController
   - Reference FirstPersonCamera for head bob

### 2. NPC Animation Setup

Add to your NPC Prefab:

```
NPC
├── Animator (existing)
├── NPCController (existing)
├── CharacterAnimationController (NEW)
└── NPCAnimationSync (NEW)
```

**Steps:**

1. Add `CharacterAnimationController` component to NPC
2. Add `NPCAnimationSync` component
   - Reference NPCController
   - CharacterAnimationController auto-references or creates

### 3. Animation Controller Setup (Unity)

Create Animator Controller with these states:

**Base Layer:**
- `Locomotion.Idle`
- `Locomotion.Walk`
- `Locomotion.Run`
- `Locomotion.Sprint`
- `Locomotion.Jump`
- `Locomotion.Fall`
- `Locomotion.Land`
- `Locomotion.CrouchIdle`
- `Locomotion.CrouchWalk`

**Action Layer:**
- `Actions.Attack`
- `Actions.Damage`
- `Actions.Death`
- `Actions.Interact`

**Parameters:**
- Speed (float)
- VerticalSpeed (float)
- Direction (float)
- IsGrounded (bool)
- IsMoving (bool)
- IsCrouching (bool)
- InCombat (bool)
- IsSprinting (bool)

## Common Tasks

### Control Player Movement Animation

```csharp
PlayerAnimations playerAnims = GetComponent<PlayerAnimations>();
CharacterAnimationController animController = playerAnims.GetAnimationController();

// Set speed (automatically selects idle/walk/run/sprint)
animController.SetMovementSpeed(3.5f);

// Enable crouch
animController.SetCrouching(true);

// Jump
animController.PlayJumpAnimation();
```

### Control NPC Combat Animation

```csharp
NPCAnimationSync npcAnims = GetComponent<NPCAnimationSync>();

// Attack
npcAnims.TriggerAttackAnimation(0);

// Take damage
npcAnims.PlayDamageAnimation();

// Death
npcAnims.PlayDeathAnimation();
```

### Query Animation State

```csharp
CharacterAnimationController animController = GetComponent<CharacterAnimationController>();

// Check current state
if (animController.IsAnimationPlaying("Locomotion.Run")) {
    Debug.Log("Player is running");
}

// Get animation length
float attackLength = animController.GetAnimationLength("Actions.Attack");

// Check normalized time
float progress = animController.GetCurrentAnimationNormalizedTime();
if (progress > 0.5f) {
    // Halfway through animation
}
```

## Animation Blend Times

| Type | Duration | Usage |
|---|---|---|
| Locomotion | 0.15s | Idle ↔ Walk ↔ Run ↔ Sprint |
| Actions | 0.2s | Attack/Damage animations |
| State | 0.3s | General transitions |

## Performance Tips

1. **Cache references** to avoid repeated GetComponent calls
2. **Use animation length caching** - lengths are cached on first access
3. **Batch animation updates** - update once per frame, not per input
4. **Layer optimization** - use upper body layer for upper animations only
5. **Parameter pooling** - reuse animator parameters across characters

## Troubleshooting

### Animation Not Playing

✗ **Issue:** Animation state not transitioning
- ✅ Verify animation name matches Animator Controller
- ✅ Check Animator is properly assigned
- ✅ Verify animation exists in controller

### Jittery Movement

✗ **Issue:** Animations stuttering or jumping between states
- ✅ Increase blend time in AnimationBlendSettings
- ✅ Check framerate is stable (60+ FPS)
- ✅ Reduce update frequency if too high

### Animation Interruption

✗ **Issue:** Attack animation cut short by movement
- ✅ Enable queue in TransitionToState()
- ✅ Check state machine transitions allow interruption
- ✅ Verify animation priorities are set correctly

### NPC Animations Out of Sync

✗ **Issue:** NPC animations don't match AI state
- ✅ Verify NPCAnimationSync component is present
- ✅ Check AI state transitions are working
- ✅ Verify animation names are correct

## Example: Complete Player Setup

```csharp
public class PlayerSetup : MonoBehaviour
{
    private CharacterAnimationController animController;
    private PlayerAnimations playerAnims;
    private MovementController movementController;

    private void Start()
    {
        // Get components
        animController = GetComponent<CharacterAnimationController>();
        playerAnims = GetComponent<PlayerAnimations>();
        movementController = GetComponent<MovementController>();

        Debug.Log($"Animation System Ready");
        Debug.Log($"Current State: {animController.CurrentLocomotionState}");
    }

    private void Update()
    {
        // Movement speed updates automatically via PlayerAnimations
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animController.PlayJumpAnimation();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            animController.PlayInteractAnimation();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animController.PlayAttackAnimation();
        }
    }
}
```

## Example: Complete NPC Setup

```csharp
public class NPCSetup : MonoBehaviour
{
    private NPCAnimationSync npcAnims;
    private NPCController npcController;

    private void Start()
    {
        npcAnims = GetComponent<NPCAnimationSync>();
        npcController = GetComponent<NPCController>();

        Debug.Log($"NPC Animation Sync Ready");
    }

    private void Update()
    {
        // Animation sync happens automatically in NPCAnimationSync

        if (npcController.CurrentState == NPCState.Attack)
        {
            // Attack animation triggered automatically
        }

        if (npcController.CurrentState == NPCState.Dead)
        {
            // Death animation triggered automatically
        }
    }
}
```

## Architecture Diagram

```
PlayerAnimations (high-level)
    ↓
CharacterAnimationController (orchestration)
    ↓
AnimationStateManager (state machine)
    ↓
Animator (playback)
    ↓
Animation Clips (asset)
```

## Next Steps

1. Create Animator Controller with animation clips
2. Add components to Player and NPC prefabs
3. Test locomotion transitions (idle→walk→run→sprint)
4. Test combat animations (attack, damage, death)
5. Fine-tune blend times for your art style
6. Implement audio event callbacks from animations

## Resources

- **Animation Setup Guide:** See `Sprint2_Animation_System.md`
- **Movement System:** See `CharacterLocomotion.cs`
- **AI System:** See `NPCController.cs`
- **Combat System:** See `NPCCombat.cs`

---

Ready to animate! 🎬
