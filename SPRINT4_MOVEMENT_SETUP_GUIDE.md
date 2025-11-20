# Sprint 4: Movement System Setup Guide

## Quick Start for Test Scene

### Step 1: Create Player GameObject

1. Create a new GameObject in your scene named "Player"
2. Add components in this order:
   - `CharacterController` (built-in)
   - `MovementController` (ProtocolEMR.Core.Player)
   - `FirstPersonCamera` (ProtocolEMR.Core.Camera)
   - `PlayerController` (ProtocolEMR.Core.Player)

### Step 2: Configure CharacterController

Select the "Player" GameObject and configure CharacterController:
- **Center**: X=0, Y=1, Z=0
- **Radius**: 0.4
- **Height**: 2.0
- **Slope Limit**: 45
- **Step Offset**: 0.3

### Step 3: Configure MovementController

Expand the CharacterController requirements and ensure all subsystems are present.

**Movement Settings**:
- Enable Movement: ✓
- Enable Jump: ✓
- Enable Sprint: ✓
- Enable Crouch: ✓

**Crouch Settings**:
- Standing Height: 2.0
- Crouch Height: 1.0
- Crouch Transition Speed: 10

### Step 4: Setup Ground Layer

1. In **Tags & Layers** window, create new layer named "Ground"
2. On all terrain/floor colliders, assign to "Ground" layer
3. In MovementController → GroundDetection:
   - Ground Layer: "Ground"
   - Raycast Distance: 0.2
   - Skin Width: 0.01

### Step 5: Add Surface Tags

For terrain that should have special properties, add tags:
- `Surface_Mud` - slowing terrain
- `Surface_Ice` - slippery terrain
- `Surface_Sand` - sandy terrain
- `Surface_Metal` - metallic terrain
- `Surface_Wood` - wooden terrain
- `Surface_Water` - water surface

Example (C#):
```csharp
groundCollider.tag = "Surface_Ice";
```

### Step 6: Configure Audio

In MovementController → MovementFeedback:

**Footstep Settings**:
1. Create or find 2-4 footstep audio clips
2. Create AudioClip array in "Footstep Clips"
3. Assign clips to array

**Landing Settings**:
1. Create or find 2-4 landing audio clips
2. Create AudioClip array in "Landing Clips"
3. Assign clips to array

### Step 7: Setup First Person Camera

1. Create a child GameObject of "Player" named "MainCamera"
2. Position at Y=0.6 (eye height in standing position)
3. Add "FirstPersonCamera" component
4. Configure mouse sensitivity

### Step 8: Verify Input System

Ensure InputManager is in scene with:
- Movement actions (WASD)
- Sprint (Shift)
- Crouch (Ctrl)
- Jump (Space)
- Pause (Esc)

### Step 9: Test Basic Movement

Press Play and test:
- [x] WASD moves player smoothly
- [x] Shift sprints (speed increases)
- [x] Ctrl crouches (height decreases)
- [x] Space jumps
- [x] Footstep sounds play while moving
- [x] Landing sound plays after falling

### Step 10: Test Surfaces

Create test areas with different surfaces:

**Mud Area**:
```csharp
// Setup
mudCollider.tag = "Surface_Mud";
mudMaterial.name = "Mat_Mud";
```
- Expected: 60% speed, slowed acceleration
- Audio: Muffled footsteps (0.6x volume)

**Ice Area**:
```csharp
iceCollider.tag = "Surface_Ice";
iceMaterial.name = "Mat_Ice";
```
- Expected: 130% speed, slippery (20% traction)
- Audio: Crisp footsteps (1.1x volume)

**Sand Area**:
```csharp
sandCollider.tag = "Surface_Sand";
sandMaterial.name = "Mat_Sand";
```
- Expected: 75% speed, natural feel
- Audio: Soft footsteps (0.7x volume)

### Step 11: Verify Performance

Open Profiler (Window → Analysis → Profiler):
- Ground detection: <0.2ms
- Movement update: <0.3ms
- Total per frame: <1ms
- Frame rate: 60+ FPS maintained

---

## Configuration Presets

### Realistic Movement

```csharp
// CharacterLocomotion
walkSpeed = 5.0f;
sprintSpeed = 8.0f;
crouchSpeed = 2.5f;
accelerationTime = 0.15f;
decelerationTime = 0.1f;

// JumpSystem
jumpHeight = 2.0f;
apexControlFactor = 0.5f;
coyoteTime = 0.1f;

// StaminaSystem
maxStamina = 100f;
staminaDrainRate = 25f;
staminaRegenRate = 15f;
staminaRegenDelay = 1.0f;
```

### Fast Action Game

```csharp
// CharacterLocomotion
walkSpeed = 7.0f;
sprintSpeed = 12.0f;
crouchSpeed = 3.5f;
accelerationTime = 0.1f;
decelerationTime = 0.05f;

// JumpSystem
jumpHeight = 2.5f;
apexControlFactor = 0.7f;
coyoteTime = 0.15f;

// StaminaSystem
maxStamina = 150f;
staminaDrainRate = 30f;
staminaRegenRate = 25f;
staminaRegenDelay = 0.5f;
```

### Survival Horror

```csharp
// CharacterLocomotion
walkSpeed = 4.0f;
sprintSpeed = 7.0f;
crouchSpeed = 1.5f;
accelerationTime = 0.2f;
decelerationTime = 0.15f;

// JumpSystem
jumpHeight = 1.5f;
apexControlFactor = 0.3f;
coyoteTime = 0.05f;

// StaminaSystem
maxStamina = 60f;
staminaDrainRate = 35f;
staminaRegenRate = 8f;
staminaRegenDelay = 2.0f;
```

---

## Integration with Existing Systems

### With Animator

```csharp
public class AnimatorController : MonoBehaviour
{
    private MovementController movement;
    private Animator animator;

    private void Update()
    {
        if (movement == null) return;

        animator.SetFloat("Speed", movement.GetAnimationSpeed());
        animator.SetFloat("DirectionX", movement.GetAnimationDirection().x);
        animator.SetFloat("DirectionZ", movement.GetAnimationDirection().z);
        animator.SetFloat("JumpPhase", movement.GetJumpPhase());
        animator.SetBool("IsInAir", movement.IsInAir());
        animator.SetBool("IsCrouch", movement.IsCrouching);
    }
}
```

### With UI

```csharp
public class HUDController : MonoBehaviour
{
    private MovementController movement;
    private Image staminaBar;

    private void Update()
    {
        if (movement == null) return;

        float staminaPercent = movement.GetStaminaPercentage();
        staminaBar.fillAmount = staminaPercent;

        // Color feedback
        if (staminaPercent < 0.25f)
            staminaBar.color = Color.red;
        else if (staminaPercent < 0.5f)
            staminaBar.color = Color.yellow;
        else
            staminaBar.color = Color.green;
    }
}
```

### With Camera

```csharp
public class CameraShake : MonoBehaviour
{
    private MovementController movement;

    private void Update()
    {
        if (movement == null) return;

        if (movement.IsMoving && movement.IsSprinting)
        {
            // More shake when sprinting
            ShakeIntensity = 0.3f;
        }
        else if (movement.IsMoving)
        {
            // Light shake when walking
            ShakeIntensity = 0.1f;
        }
        else
        {
            // No shake when standing
            ShakeIntensity = 0f;
        }
    }
}
```

---

## Troubleshooting

### Player Falls Through Floor

1. Check that CharacterController height matches your character model
2. Verify ground colliders are on "Ground" layer
3. Ensure CharacterController.skinWidth > 0 (default 0.08)
4. Check GroundDetection raycast distance

### Movement Feels Unresponsive

1. Decrease accelerationTime (default 0.15)
2. Check that input is being received (debug logs)
3. Verify surface isn't applying extreme speed multiplier
4. Check frame rate isn't too low

### Jump Doesn't Work

1. Verify Jump action is mapped in InputManager
2. Check that player is grounded (IsGrounded property)
3. Ensure jumpHeight > 0
4. Verify gravity value is negative

### Audio Not Playing

1. Check AudioSource component exists on player
2. Verify audio clips are assigned to arrays
3. Check volume settings in inspector
4. Ensure 3D spatial audio is enabled (spatialBlend = 1)

---

## Testing Checklist

- [ ] Player spawns at correct position
- [ ] WASD movement works smoothly
- [ ] Acceleration feels responsive
- [ ] Can sprint (speed increases, stamina drains)
- [ ] Stamina regenerates after delay
- [ ] Can jump from ground
- [ ] Apex control reduces gravity at peak
- [ ] Can jump after small fall (coyote time)
- [ ] Can jump before landing (jump buffer)
- [ ] Crouch transitions smoothly
- [ ] Cannot stand up in low spaces
- [ ] Footsteps play in rhythm
- [ ] Landing sound plays after fall
- [ ] Mud slows movement
- [ ] Ice increases speed
- [ ] Movement stops smoothly
- [ ] No sliding or jittering
- [ ] Animation parameters update
- [ ] Performance stays 60+ FPS

---

## Performance Optimization

If experiencing performance issues:

1. **Reduce raycast frequency**: Only update grounding every 2-3 frames for distant players
2. **Simplify audio**: Use fewer footstep variations
3. **Cache multipliers**: Pre-calculate surface modifiers
4. **Batch raycasts**: If multiple players, batch ground detection
5. **Object pooling**: For frequently created/destroyed projectiles

---

## Next Steps

1. Integrate with animator (see examples above)
2. Add footstep sounds from audio system
3. Test with actual character model
4. Tune movement feel for game design
5. Add special movement states (climbing, swimming)
6. Integrate with damage/knockback systems

---

**Last Updated**: Sprint 4  
**Status**: Ready for Integration  
