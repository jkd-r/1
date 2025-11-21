# Animation System Integration Guide

## Complete Integration Workflow

This guide provides step-by-step instructions for integrating the Sprint 2 Animation System into your game project.

## Part 1: Setup Animator Controller

### Creating the Animation Controller

1. **Create Animator Controller:**
   - Right-click in Project → Create → Animator Controller
   - Name it `CharacterAnimationController`
   - Assign to Player/NPC GameObject

2. **Create Animation States (Base Layer):**

   **Locomotion States:**
   - `Locomotion.Idle` - Idle pose
   - `Locomotion.Walk` - Walking animation
   - `Locomotion.Run` - Running animation
   - `Locomotion.Sprint` - Sprinting animation
   - `Locomotion.Jump` - Jump takeoff
   - `Locomotion.Fall` - Falling animation
   - `Locomotion.Land` - Landing animation
   - `Locomotion.CrouchIdle` - Crouch idle pose
   - `Locomotion.CrouchWalk` - Crouch walking

3. **Create Sub-layer for Actions:**

   **Action States:**
   - `Actions.Attack` - Attack animation
   - `Actions.Damage` - Damage reaction
   - `Actions.Death` - Death animation
   - `Actions.Interact` - Interaction animation

4. **Add Animator Parameters:**

   **Locomotion Parameters:**
   - Name: `Speed` | Type: Float | Default: 0
   - Name: `VerticalSpeed` | Type: Float | Default: 0
   - Name: `Direction` | Type: Float | Default: 0
   - Name: `IsGrounded` | Type: Bool | Default: true
   - Name: `IsMoving` | Type: Bool | Default: false
   - Name: `IsCrouching` | Type: Bool | Default: false

   **Combat Parameters:**
   - Name: `InCombat` | Type: Bool | Default: false
   - Name: `IsSprinting` | Type: Bool | Default: false
   - Name: `IsAttacking` | Type: Bool | Default: false
   - Name: `IsDead` | Type: Bool | Default: false
   - Name: `IsStunned` | Type: Bool | Default: false

5. **Setup State Transitions:**

   **Base Layer Transitions:**
   ```
   Idle → Walk: Speed > 0.5
   Walk → Run: Speed > 2.5
   Run → Sprint: Speed > 5.5
   Sprint → Run: Speed < 4.5
   Run → Walk: Speed < 1.5
   Walk → Idle: Speed < 0.1
   
   Any → Jump: Jump trigger
   Jump → Fall: VerticalSpeed < -1
   Fall → Land: IsGrounded = true
   Land → Idle: (after animation)
   
   Any → CrouchIdle: IsCrouching = true
   CrouchIdle → CrouchWalk: IsMoving = true
   CrouchWalk → CrouchIdle: IsMoving = false
   CrouchIdle/Walk → Walk/Idle: IsCrouching = false
   ```

   **Important Settings:**
   - Set transition duration to 0.15s (for locomotion)
   - Enable `Has Exit Time` for smooth transitions
   - Uncheck `Fixed Duration` for frame-based transitions

## Part 2: Player Setup

### Add Components to Player GameObject

```
Player
├── CharacterController (existing)
├── MovementController (existing)
├── FirstPersonCamera (existing)
├── Animator (with CharacterAnimationController)
├── CharacterAnimationController (NEW)
└── PlayerAnimations (NEW)
```

### Configuration Steps

1. **Add CharacterAnimationController:**
   ```csharp
   Player Inspector:
   ├── Animator: [Drag animator with CharacterAnimationController]
   ├── Animation Blend Settings:
   │  ├── Locomotion Blend Time: 0.15
   │  ├── Action Animation Blend Time: 0.2
   │  └── Transition Blend Time: 0.3
   ├── Speed Thresholds:
   │  ├── Stationary Threshold: 0.01
   │  ├── Walk Threshold: 2.0
   │  ├── Run Threshold: 5.0
   │  └── Sprint Threshold: 8.0
   └── Animation Setup:
      ├── Idle Animation Name: Locomotion.Idle
      ├── Walk Animation Name: Locomotion.Walk
      ├── ... (etc. for all animation names)
   ```

2. **Add PlayerAnimations:**
   ```csharp
   Player Inspector:
   ├── Component References:
   │  ├── Animation Controller: [Drag CharacterAnimationController]
   │  ├── Movement Controller: [Drag MovementController]
   │  └── First Person Camera: [Drag FirstPersonCamera]
   ├── Head Bob Settings:
   │  ├── Head Bob Amount: 0.02
   │  └── Head Bob Frequency: 5.0
   ├── Landing Feedback:
   │  ├── Landing Camera Shake Duration: 0.1
   │  └── Landing Camera Shake Amount: 0.15
   └── Upper Body Layer:
      ├── Upper Body Layer Index: 1
      └── Use Upper Body Layer: true
   ```

## Part 3: NPC Setup

### Add Components to NPC Prefab

```
NPC Prefab
├── Animator (with CharacterAnimationController)
├── NPCController (existing)
├── CharacterAnimationController (NEW)
└── NPCAnimationSync (NEW)
```

### Configuration Steps

1. **Add CharacterAnimationController (same as player above)**

2. **Add NPCAnimationSync:**
   ```csharp
   NPC Inspector:
   ├── References:
   │  ├── NPC Controller: [Auto or drag]
   │  ├── Character Animation Controller: [Auto or drag]
   │  └── Animator: [Auto or drag]
   ├── Animation Configuration:
   │  └── Locomotion Update Frequency: 0.05
   └── Combat Animation Settings:
      ├── Attack Animation Prefix: Attack
      ├── Damage Animation Prefix: Damage
      └── Death Animation Name: Death
   ```

## Part 4: Code Integration Examples

### Player Animation Control

```csharp
using ProtocolEMR.Core.Animation;

public class PlayerCombatSystem : MonoBehaviour
{
    private PlayerAnimations playerAnimations;
    private CharacterAnimationController animController;

    private void Start()
    {
        playerAnimations = GetComponent<PlayerAnimations>();
        animController = playerAnimations.GetAnimationController();
    }

    public void PerformAttack(AttackType attackType)
    {
        // Play attack animation
        animController.PlayAttackAnimation($"Actions.{attackType.ToString()}");
        
        // Schedule hit check at 50% through animation
        float animLength = animController.GetAnimationLength($"Actions.{attackType}");
        Invoke(nameof(CheckHit), animLength * 0.5f);
    }

    public void TakeDamage(float amount, Vector3 knockbackDir)
    {
        // Play damage animation
        playerAnimations.PlayDamageAnimation(knockbackDir);
        
        // Apply knockback
        ApplyKnockback(knockbackDir);
    }

    public void Die()
    {
        playerAnimations.PlayDeathAnimation();
        // Death sequence...
    }

    private void CheckHit()
    {
        // Hit detection logic
    }

    private void ApplyKnockback(Vector3 direction)
    {
        // Apply physics knockback
    }
}
```

### NPC Animation Control

```csharp
using ProtocolEMR.Core.Animation;
using ProtocolEMR.Core.AI;

public class NPCCombatSystem : MonoBehaviour
{
    private NPCAnimationSync npcAnimSync;
    private NPCController npcController;

    private void Start()
    {
        npcAnimSync = GetComponent<NPCAnimationSync>();
        npcController = GetComponent<NPCController>();
    }

    public void PerformAttack(int attackIndex)
    {
        // Trigger attack animation
        npcAnimSync.TriggerAttackAnimation(attackIndex);
        
        // Get animation length for hit timing
        float animLength = npcAnimSync.GetAnimationLength($"Attack{attackIndex + 1}");
        
        // Schedule hit at animation midpoint
        Invoke(nameof(ExecuteHit), animLength * 0.5f);
    }

    public void ReactToDamage()
    {
        npcAnimSync.PlayDamageAnimation();
        
        // Update AI state
        npcController.SetState(NPCState.Stun);
        Invoke(nameof(RecoverFromDamage), 0.5f);
    }

    public void Die()
    {
        npcAnimSync.PlayDeathAnimation();
        npcController.SetState(NPCState.Dead);
    }

    private void ExecuteHit()
    {
        // Execute hit logic
    }

    private void RecoverFromDamage()
    {
        npcController.SetState(NPCState.Idle);
    }
}
```

### Animation Monitoring

```csharp
using ProtocolEMR.Core.Animation;

public class AnimationDebugger : MonoBehaviour
{
    private CharacterAnimationController animController;

    private void Start()
    {
        animController = GetComponent<CharacterAnimationController>();
    }

    private void Update()
    {
        // Log current animation state
        Debug.Log($"Current Animation: {animController.GetCurrentAnimationName()}");
        Debug.Log($"Locomotion State: {animController.CurrentLocomotionState}");
        Debug.Log($"Normalized Time: {animController.GetCurrentAnimationNormalizedTime():F2}");
        Debug.Log($"Is Transitioning: {animController.StateManager.IsTransitioning}");
    }

    public void PrintAnimationInfo(string animationName)
    {
        float length = animController.GetAnimationLength(animationName);
        Debug.Log($"Animation '{animationName}' length: {length:F2}s");
    }
}
```

## Part 5: Animator Window Setup

### Transition Configuration Example

For `Idle → Walk` transition:
```
Transition Settings:
├── Conditions: Speed > 0.5
├── Duration: 0.15s
├── Offset: 0%
├── Interruption Source: None (Current State)
├── Ordered Interruption: unchecked
├── Exit Time: unchecked
├── Fixed Duration: unchecked
└── Transition Offset: 0
```

### Blend Tree Setup (Optional)

For smooth locomotion blending:
```
Locomotion (Blend Tree)
├── Idle (Speed = 0)
├── Walk (Speed = 1)
├── Run (Speed = 2)
└── Sprint (Speed = 3)

Blend: Linear Direct (Speed parameter)
```

## Part 6: Performance Optimization

### Best Practices

1. **Cache References:**
   ```csharp
   // Good
   CharacterAnimationController animController = GetComponent<CharacterAnimationController>();
   
   // Bad (every frame)
   for (int i = 0; i < frame; i++) {
       CharacterAnimationController animController = GetComponent<CharacterAnimationController>();
   }
   ```

2. **Batch Parameter Updates:**
   ```csharp
   // Good
   animController.SetMovementSpeed(5.0f);
   
   // Bad
   stateManager.SetFloat("Speed", 5.0f);
   stateManager.SetBool("IsMoving", true);
   stateManager.SetBool("InCombat", false);
   ```

3. **Use Animation Events:**
   ```csharp
   // In Animation Timeline:
   // At frame 15: Animation Event → OnAttackHit()
   // At frame 30: Animation Event → OnAttackEnd()
   
   public void OnAttackHit()
   {
       // Execute hit logic synchronized with animation
   }
   ```

4. **Layer Weight Optimization:**
   ```csharp
   // Only modify upper body layer when needed
   animController.StateManager.SetLayerWeight(1, needsUpperBody ? 1.0f : 0.0f);
   ```

## Part 7: Common Animation Sequences

### Combat Sequence Example

```
Player: Idle
  ↓ (space bar pressed)
Player: Jump animation plays
  ↓ (animation ends, player falling)
Player: Fall animation plays
  ↓ (player lands)
Player: Land animation plays (0.2s)
  ↓ (land animation ends)
Player: Walk/Idle (depending on input)

Simultaneous (if holding attack):
  ↓ (attack window active during jump)
Player: Attack animation queued
  ↓ (after land animation)
Player: Attack animation plays
```

### NPC Combat Sequence Example

```
NPC: Idle
  ↓ (player detected)
NPC: Alert animation plays
  ↓ (alert ends)
NPC: Chase (Run animation blended)
  ↓ (reached player)
NPC: Attack animation plays
  ↓ (attack ends)
NPC: Back to Run (if player fleeing)
  OR Idle (if player defeated)
```

## Part 8: Debugging

### Enable Debug Logging

```csharp
public class AnimationDebugUI : MonoBehaviour
{
    private CharacterAnimationController animController;

    private void OnGUI()
    {
        if (GUILayout.Button("Log Current State"))
        {
            Debug.Log($"Current: {animController.CurrentLocomotionState}");
            Debug.Log($"Animation: {animController.GetCurrentAnimationName()}");
            Debug.Log($"Transitioning: {animController.StateManager.IsTransitioning}");
        }
    }
}
```

### Animation Preview

In Unity Animator window:
1. Select state you want to test
2. Click "Live" preview at bottom of window
3. Move character around to see animations blend
4. Use speed slider to test different speeds

## Part 9: Troubleshooting

### Issue: Animation not playing

**Solution:**
1. Check animation name matches exactly (case-sensitive)
2. Verify animation exists in Animator Controller
3. Check Animator component is assigned
4. Verify animation clip duration > 0

### Issue: Jerky transitions

**Solution:**
1. Increase blend time in AnimationBlendSettings
2. Check framerate is consistent
3. Verify character controller is smoothly updating position
4. Check for animation exit time conflicts

### Issue: NPC animations out of sync

**Solution:**
1. Verify NPCAnimationSync component present
2. Check NPC state transitions are working
3. Verify animation names in NPCAnimationSync inspector
4. Add debug logging to AI state changes

### Issue: Performance drops

**Solution:**
1. Reduce character count or animation update frequency
2. Increase locomotionUpdateFrequency in NPCAnimationSync
3. Use animation blending instead of separate states
4. Profile with Unity Profiler to identify bottleneck

## Part 10: Extension Points

### Custom Animation States

To add new animation states:

1. **Update CharacterAnimationController:**
   ```csharp
   public enum AnimationState
   {
       // existing states...
       Emote,  // NEW
       Victory // NEW
   }
   ```

2. **Update AnimationSetup:**
   ```csharp
   [SerializeField] public string emoteAnimationName = "Actions.Emote";
   ```

3. **Add playback method:**
   ```csharp
   public void PlayEmoteAnimation(string emoteName)
   {
       stateManager.TransitionToState($"Actions.Emote.{emoteName}", 0.2f);
   }
   ```

### Custom Animation Events

Add animation events in the Animation Timeline:
1. Open animation clip in Timeline
2. Right-click at desired frame
3. Add Event
4. Select function to call (e.g., OnFootstep, OnAttackHit)
5. Add parameters if needed

## References

- **Sprint 2 Documentation:** Sprint2_Animation_System.md
- **Quick Start Guide:** Animation_System_QuickStart.md
- **Unity Animator Documentation:** https://docs.unity3d.com/Manual/class-Animator.html
- **Mecanim System:** https://docs.unity3d.com/Manual/MecanimAnimationSystem.html

---

**Integration Complete!** 🎬

Your animation system is now fully integrated and ready to bring your characters to life.
