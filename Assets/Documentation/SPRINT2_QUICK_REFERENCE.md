# Sprint 2: Quick Reference Guide

## Quick Start (5 minutes)

### 1. Import Animations
```
1. Go to mixamo.com
2. Download ~12 animations as FBX
3. Copy to Assets/Animations/Mixamo/
4. Reimport first one with "Create From This Model" avatar
5. Copy avatar to others
```

### 2. Setup Audio Folders
```
Assets/Resources/Audio/
├─ Footsteps/Concrete/ → .wav files
├─ Footsteps/Metal/ → .wav files
└─ Breathing/ → breathing_loop.wav
```

### 3. Create Animator Controller
Option A (Manual):
```
1. Right-click Assets/Animations/
2. Create → Animator Controller
3. Rename to PlayerLocomotion
4. Add parameters (6 total)
5. Create states and transitions
```

Option B (Automatic):
```
Menu: Tools → Protocol EMR → Create Locomotion Animator
```

### 4. Scene Setup
```
1. Player GameObject
   - CharacterController component
   - PlayerController script
   - AnimationController script

2. Player/Character (child)
   - Mixamo model with humanoid rig
   - Animator → humanoid_avatar.asset
   - Animator → PlayerLocomotion.controller

3. UI Canvas
   - Stamina bar (Image with horizontal fill)
   - StaminaDisplay script

4. AudioManager
   - GameObject with AudioManager script
```

### 5. Test
```
Play scene → W key → should walk with animation
Shift → sprint with stamina display
Ctrl → crouch
Space → jump with landing
```

---

## Key Scripts Reference

### AnimationController
```csharp
// Get component
var animCtrl = GetComponent<AnimationController>();

// Update locomotion speed (0-1 range)
animCtrl.SetLocomotionSpeed(moveDirection, currentSpeed, maxSpeed);

// Crouch state
animCtrl.SetCrouching(true);

// Jump/Land
animCtrl.PlayJump();
animCtrl.PlayLanding();
```

### AudioManager
```csharp
// Play footstep sound
AudioManager.Instance.PlayFootstep(position, isSprinting);

// Surface audio context
AudioManager.Instance.UpdateSurface(position);

// Breathing during sprint
AudioManager.Instance.PlayBreathing();
AudioManager.Instance.StopBreathing();

// Landing impact
AudioManager.Instance.PlayLandingSound(position);
```

### PlayerController
```csharp
// Access movement state
player.CurrentSpeed;        // m/s
player.IsSprinting;         // bool
player.IsCrouching;         // bool
player.CurrentStamina;      // 0-100
player.MaxStamina;          // 100

// Stamina management
player.ResetStamina();
```

### StaminaDisplay
```csharp
// Get component
var staminaUI = GetComponent<StaminaDisplay>();

// Control visibility
staminaUI.SetVisibility(true);
staminaUI.SetStaminaColor(Color.red);
```

---

## Animator Parameters

| Name | Type | Range | Purpose |
|------|------|-------|---------|
| Locomotion_Speed | Float | 0-1 | Drives walk/run/sprint blending |
| Direction_X | Float | -1 to 1 | Strafe left/right |
| Direction_Y | Float | -1 to 1 | Forward/backward |
| IsCrouching | Bool | — | Crouch state |
| IsJumping | Bool | — | Jump trigger |
| IsLanding | Bool | — | Landing state |

---

## Audio Folder Structure

```
Assets/Resources/Audio/
├─ Footsteps/
│  ├─ Concrete/
│  │  ├─ footstep_01.wav
│  │  ├─ footstep_02.wav
│  │  └─ footstep_03.wav
│  ├─ Metal/
│  ├─ Carpet/
│  ├─ Wood/
│  ├─ Glass/
│  └─ Digital/
├─ Breathing/
│  └─ breathing_loop.wav
└─ Landing/
   └─ land_concrete.wav
```

---

## Movement Parameters

| Action | Speed | Stamina | Notes |
|--------|-------|---------|-------|
| Idle | 0 m/s | Regenerates | — |
| Walk | 5.5 m/s | Regenerates | Normal movement |
| Run | 8.0 m/s | Regenerates | Faster than walk |
| Sprint | 12.0 m/s | Depletes | Max 5 seconds |
| Crouch | 3.0 m/s | Regenerates | Can't sprint/jump |
| Jump | — | — | 5.5 meter height |

---

## Stamina System

### Depletion (Sprinting)
```
100 → 0 in 5 seconds
Rate: 20 points/second
```

### Regeneration (Walking/Idle)
```
0 → 100 in 5 seconds
Rate: 20 points/second
Delay: 0.5 seconds before starting
```

### Force Walk
When stamina = 0, sprint is disabled until recovery complete

---

## Accessibility Settings

From `SettingsManager`:

```csharp
// Camera head bob
var cameraBobIntensity = SettingsManager.Instance.GetCameraBobIntensity(); // 0-1
SettingsManager.Instance.SetCameraBobIntensity(0.5f);

// Sprint mode (hold vs toggle)
var isToggleMode = SettingsManager.Instance.IsSprintToggleModeEnabled();

// Movement difficulty
var speedMult = SettingsManager.Instance.GetMovementSpeedMultiplier(); // 0.5-1.5
SettingsManager.Instance.SetMovementSpeedMultiplier(1.2f); // 20% faster

// Audio
var footstepVol = SettingsManager.Instance.GetFootstepVolumeMultiplier();
var breathingOn = SettingsManager.Instance.IsBreathingAudioEnabled();
```

---

## Common Issues & Fixes

### Issue: No footsteps
**Check:**
1. Audio clips in `Assets/Resources/Audio/Footsteps/`
2. Surface tags match SurfaceType enum
3. AudioManager instance exists
4. Animation events added to clips

### Issue: Jerky animations
**Check:**
1. Blend tree thresholds (0.1, 0.4, 0.8, 0.05)
2. Transition duration not too short (0.2s)
3. Animator.SetFloat using smoothing parameter
4. Animation clips have proper exit times

### Issue: No sprint decay
**Check:**
1. Stamina drain rate > 0
2. Sprint is being triggered (check IsSprintPressed)
3. Movement input > 0.1 magnitude
4. AnimationController.SetLocomotionSpeed called

### Issue: Crouch sliding
**Check:**
1. CharacterController not moving during crouch
2. Height transition smooth (crouchTransitionSpeed)
3. Center.y properly adjusted

### Issue: 60 FPS drop
**Check:**
1. Reduce animation clip count (keep best 8-10)
2. Use animation compression (High Quality)
3. Profile to find bottleneck
4. Reduce audio source count

---

## Animator Controller States

```
Idle
 ↓
Walk (Locomotion_Speed > 0.1)
 ↓
Run (Locomotion_Speed > 0.4)
 ↓
Sprint (Locomotion_Speed > 0.8)

Jump (Any state → IsJumping = true)
 ↓
Land (Jump animation ends)
 ↓
Any state (based on speed)

Crouch (IsCrouching = true)
 ├─ CrouchIdle
 └─ CrouchWalk
```

---

## Testing Checklist (Quick)

```
[ ] W key → walk with animation
[ ] Shift → sprint, stamina bar appears
[ ] Stamina depletes → forced walk
[ ] Ctrl → crouch animation
[ ] Space → jump with landing
[ ] Metal surface → different audio
[ ] Settings persist → toggle options
[ ] 60 FPS → no stuttering
```

---

## Key Files

| File | Purpose |
|------|---------|
| AnimationController.cs | Animator parameter management |
| AudioManager.cs | Footstep/breathing/landing audio |
| PlayerController.cs | Movement + integration |
| StaminaDisplay.cs | UI stamina bar |
| CameraShake.cs | Landing impact feedback |
| PlayerLocomotion.controller | Animator state machine |
| MIXAMO_IMPORT_GUIDE.md | Animation import steps |
| SPRINT2_LOCOMOTION_SETUP.md | Complete setup guide |

---

## Performance Tips

1. **Animation Controller:**
   - Use Humanoid rig (faster than Generic)
   - Bake root motion for locomotion
   - Compress animations (High Quality)

2. **Audio:**
   - Stream breathing audio (don't load fully)
   - Pool audio sources
   - Update surface detection every 0.3s (not every frame)

3. **General:**
   - Cache GetComponent calls
   - Use animator parameter hashing (StringToHash)
   - Profile regularly with Profiler window

---

## Next Steps (Sprint 3)

Combat system will add:
- Weapon animations
- Combat layer (additive blending)
- Attack/block states
- Weapon audio (hits, impacts)
- Combo system

Animation controller already supports adding layers!

---

## Useful Resources

- **Mixamo:** https://www.mixamo.com
- **Unity Humanoid:** https://docs.unity3d.com/Manual/AvatarCreationandSetup.html
- **Animation Events:** https://docs.unity3d.com/Manual/AnimationEvents.html
- **Profiler:** https://docs.unity3d.com/Manual/Profiler.html

---

## Quick Debugging

### Console Output Expected:
```
"Audio clips loaded for surface: Concrete"
"Stamina regenerating..."
"Setting locomotion speed: 0.5"
```

### Should Not See:
```
"NullReferenceException: AudioManager"
"Missing animation clip"
"Audio source limit exceeded"
```

---

## Support

For detailed setup: See `SPRINT2_IMPLEMENTATION_CHECKLIST.md`
For Mixamo help: See `MIXAMO_IMPORT_GUIDE.md`
For animator setup: See `SPRINT2_LOCOMOTION_SETUP.md`
