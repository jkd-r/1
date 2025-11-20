# Sprint 2: Locomotion & Animation System Setup Guide

## Overview
Sprint 2 implements a complete locomotion system with Mixamo character animations, footstep audio, and stamina management. This guide covers setup, integration, and asset creation.

## Architecture

### Core Systems
1. **AnimationController.cs** - Manages Animator parameters and animation state
2. **AudioManager.cs** - Handles footstep, breathing, and impact sounds
3. **StaminaDisplay.cs** - UI display for stamina bar
4. **PlayerController.cs** (updated) - Integrated with animation and audio

### Data Flow
```
PlayerController.HandleMovement()
  ↓
AnimationController.SetLocomotionSpeed() → Animator parameters
  ↓
Animator blends animations based on speed parameter
  ↓
Animation events trigger PlayFootstepSound()
  ↓
AudioManager.PlayFootstep() with surface detection
```

## Step 1: Download Mixamo Animations

### Using Mixamo Free Tier (Recommended)
1. Go to [mixamo.com](https://www.mixamo.com)
2. Sign in or create account
3. Download each animation as **FBX** with:
   - Skin: OFF (skeletal animation only, smaller files)
   - Frame Range: Full
   - Framerate: 60 fps
   - FK/IK: FK (Forward Kinematics recommended)

### Required Animation Categories
Create folders in `Assets/Animations/Mixamo/`:

#### Idle Animations (5 variants)
- Idle_Breathing.fbx
- Idle_Look_Around.fbx
- Idle_Scratch_Head.fbx
- Idle_Tired.fbx
- Idle_Trophy.fbx

#### Locomotion
- Walk_Forward.fbx
- Run_Forward.fbx
- Sprint_Forward.fbx
- Walk_Backward.fbx
- Strafe_Left.fbx
- Strafe_Right.fbx
- Jump_Start.fbx
- Jump_Land.fbx

#### Crouch
- Crouch_Idle.fbx
- Crouch_Walk_Forward.fbx
- Crouch_Walk_Backward.fbx

#### Special Moves (Sprint 2 optional)
- Vault_Diagonal_Up.fbx
- Climb_Front.fbx

### Mixamo Setup Steps
1. For each FBX, in Unity's Inspector set:
   - Model → Rig Type: **Humanoid**
   - Model → Avatar Definition: **Create from This Model**
   - Armature: Keep default (will be extracted)
   - Animation → Import Animation: **Checked**
   - Animation → Bake Animation: **OFF** (unless you need bone poses)

2. Apply to all animations

## Step 2: Create Avatar

1. In `Assets/Animations/`, create folder `Avatar`
2. Drag any Mixamo animation model into scene
3. In Inspector of imported model:
   - Rig → Avatar Definition: **Create From This Model**
   - Click Configure
   - Verify bone mapping (should be automatic for humanoid)
   - Done

4. This creates a `.asset` file in Avatar folder (e.g., `character_avatar.asset`)
5. For other animations, change Avatar Definition to **Copy From Other Avatar**
6. Select the shared avatar asset

## Step 3: Create Animator Controller

### Blend Tree Setup

**File Path:** `Assets/Animations/PlayerLocomotion.controller`

#### Layers
- Layer 0: "Base Layer"
  - Additive: OFF
  - Mask: Default (Full Body)
  - Weight: 1.0

#### Parameters
- **Locomotion_Speed** (Float) - Range 0-1, smoothing 0.15s
- **Direction_X** (Float) - Range -1 to 1 (strafe), smoothing 0.1s
- **Direction_Y** (Float) - Range -1 to 1 (forward/back), smoothing 0.1s
- **IsCrouching** (Bool)
- **IsJumping** (Bool)
- **IsLanding** (Bool)

#### State Machine Structure
```
States:
├─ Idle (any Idle animation, loops)
├─ Walk (1D Blend Tree: Walk Forward + Back)
├─ Run (1D Blend Tree: Run animations)
├─ Sprint (1D Blend Tree: Sprint animations)
├─ CrouchIdle (Crouch Idle animation)
├─ CrouchWalk (1D Blend Tree: Crouch Walk animations)
├─ Jump (Jump Start animation, doesn't loop)
└─ Land (Jump Land animation, doesn't loop)

Transitions:
├─ Idle → Walk: Locomotion_Speed > 0.1
├─ Walk → Run: Locomotion_Speed > 0.4
├─ Run → Sprint: Locomotion_Speed > 0.8
├─ Sprint → Run: Locomotion_Speed < 0.75
├─ Run → Walk: Locomotion_Speed < 0.3
├─ Walk → Idle: Locomotion_Speed < 0.05
├─ * → CrouchIdle: IsCrouching = true
├─ CrouchIdle → CrouchWalk: Locomotion_Speed > 0.1
├─ CrouchWalk → CrouchIdle: Locomotion_Speed < 0.05
├─ CrouchIdle/Walk → Idle/Walk: IsCrouching = false
├─ * → Jump: IsJumping = true
├─ Jump → Land: Jump animation ends
└─ Land → *: Land animation ends
```

### 1D Blend Trees

**Walk Tree (Locomotion_Speed 0-1):**
```
Position 0.0: Walk_Backward (reversed)
Position 0.5: Walk_Forward
Position 1.0: Walk_Forward (or strafe variant)
```

**Run Tree:**
```
Position 0.0: Run_Backward (reversed)
Position 0.5: Run_Forward
Position 1.0: Run_Forward
```

**Sprint Tree:**
```
Position 0.5: Sprint_Forward
Position 1.0: Sprint_Forward
```

**Crouch Walk Tree:**
```
Position 0.0: Crouch_Walk_Backward
Position 0.5: Crouch_Walk_Forward
Position 1.0: Crouch_Walk_Forward
```

### Transition Settings
- **Idle → Walk:** Exit Time 0.5, Transition Duration 0.2s
- **Walk → Run:** Exit Time 0.5, Transition Duration 0.2s
- **Run → Sprint:** Exit Time 0.5, Transition Duration 0.15s
- **Sprint → Run:** Exit Time 0.5, Transition Duration 0.2s
- **Jump:** Exit Time disabled (single shot)
- **Land:** Exit Time disabled (single shot)
- **Crouch transitions:** Transition Duration 0.3s (slower for comfort)

## Step 4: Setup Animation Events

For each footstep animation (Walk, Run, Sprint, Crouch Walk):

1. Select animation clip in Project
2. In Animation window, expand **Events**
3. At ~40% of animation (where foot touches ground):
   - Add Event
   - Function: `PlayFootstepSound`
   - Parameter: 0 (or 1 for alternate foot)
4. Repeat at ~80% (second foot contact)

### Example Timeline
For 0.6s Walk animation:
- Event 1: Frame ~0.24s (40% of cycle) - Left foot
- Event 2: Frame ~0.48s (80% of cycle) - Right foot

## Step 5: Create Footstep Audio Clips

**Folder Structure:**
```
Assets/Audio/
├─ Footsteps/
│  ├─ Concrete/
│  │  ├─ footstep_concrete_01.wav
│  │  ├─ footstep_concrete_02.wav
│  │  └─ ...
│  ├─ Metal/
│  ├─ Carpet/
│  ├─ Wood/
│  ├─ Glass/
│  └─ Digital/
├─ Breathing/
│  ├─ breathing_loop.wav
│  └─ breathing_heavy_loop.wav
└─ Landing/
   ├─ land_concrete.wav
   └─ land_metal.wav
```

### Audio Specifications
- **Format:** WAV, 16-bit, 48kHz
- **Mono** for world-space audio (footsteps)
- **Stereo** acceptable for UI audio
- **Filesize:** Keep individual clips < 100KB for memory efficiency
- **Duration:** 0.3-0.8s per footstep sample

### Free Audio Sources
- **Freesound.org** - Creative Commons licensed
- **Zapsplat.com** - Free sound effects
- **OpenGameArt.org** - Game-specific audio

### Loading Audio Clips
Audio clips should be placed in:
```
Assets/Resources/Audio/Footsteps/[SurfaceType]/
Assets/Resources/Audio/Breathing/
Assets/Resources/Audio/Landing/
```

The AudioManager will load them automatically with `Resources.LoadAll()`.

## Step 6: Scene Setup

### Prefab: Player Character
1. Create empty GameObject `Player`
2. Add CharacterController component
3. Add PlayerController script
4. Add AnimationController script
5. Add AudioManager script (or reference existing)

### Add Animated Model
1. Drag Mixamo model with humanoid rig as child
2. Set position (0, 0, 0) relative to parent
3. In Animator component:
   - Controller: Select `PlayerLocomotion.controller`
   - Avatar: Select shared humanoid avatar

### Add StaminaDisplay UI Canvas
1. Right-click hierarchy → UI → Canvas
2. Add Panel child with Image component
3. Add StaminaDisplay script to canvas
4. Assign:
   - staminaBar: Image (fill type: Horizontal)
   - staminaText: Text element showing percentage

### Layer Setup
1. Create Layer "Footstep Surfaces"
2. Assign all floor/ground objects to this layer
3. In AudioManager, set "Surface Layer" to this layer

## Step 7: Surface Material Tags

For surface-specific audio, assign tags to ground objects:

```
Concrete → GameObject tag: "Concrete"
Metal → GameObject tag: "Metal"
Carpet → GameObject tag: "Carpet"
Wood → GameObject tag: "Wood"
Glass → GameObject tag: "Glass"
Digital → GameObject tag: "Digital"
```

AudioManager.DetectSurface() will raycast and check tags.

## Performance Optimization

### Animation Controller Memory
- Each animation: ~2-5MB compressed
- 12 animations total: ~30-60MB
- Animator cache: ~500KB

### Audio Memory (Streaming)
- Footsteps: 3-5 clips × 6 surfaces = 18-30 clips
- Total: ~30-50MB if all loaded
- Recommend using Resources folder with streaming

### Footstep Raycast Optimization
- Currently: Every foot contact (animation event)
- Alternative: Check every 0.3s instead
- This reduces raycast cost from 24×/sec to 3×/sec

## Testing Checklist

- [ ] Player walks smoothly without animation snapping
- [ ] Footsteps sync with animation (no audio lag)
- [ ] Stamina depletes during sprint, regenerates after
- [ ] Stamina bar shows/hides appropriately
- [ ] Breathing audio plays during heavy sprint
- [ ] Landing sound plays on jump
- [ ] Different surfaces have different footstep audio
- [ ] Crouch animation shortens player height
- [ ] Movement feels responsive (< 50ms input lag)
- [ ] 60 FPS maintained during locomotion

## Accessibility Settings Integration

### From SettingsManager
- `enableCameraBob` - Affects head movement in animation
- `enableMotionBlur` - Affects sprint visual feedback
- `cameraShakeIntensity` - Affects landing shake
- `fieldOfView` - Already integrated in camera

### Optional Extensions
- Locomotion speed multiplier (50-150% difficulty)
- Footstep volume multiplier
- Breathing audio toggle

## Known Issues & Solutions

### Issue: Animations jitter or snap between states
**Solution:** Increase transition duration to 0.25-0.3s

### Issue: Footsteps play twice or not at all
**Solution:** Check animation event is set to exact frame, not range

### Issue: Audio Manager not finding clips
**Solution:** Ensure clips are in `Assets/Resources/` path with correct naming

### Issue: Animation blending looks unnatural
**Solution:** Check animation has proper exit times (typically 0.5-0.8)

## Integration with Sprint 3

Sprint 3 (Combat) will add:
- Animation blending for weapons
- Combat states (Blocking, Attacking)
- Two-handed weapon animations
- Sound effects for weapon contact

Ensure animation controller supports adding combat layers with additive blending.

## Future Enhancements (Sprint 4+)

- IK foot placement on uneven terrain
- Slope-aware animation blending
- Ledge grab preparation animations
- Water wading animations
- Procedural footstep surface blending
