# Sprint 2: Implementation Checklist

## Pre-Implementation
- [ ] Branch: `feat/sprint2-player-locomotion-mixamo` (should be current)
- [ ] Review existing Sprint 1 systems (InputManager, PlayerController, SettingsManager)
- [ ] Understand Humanoid Rigging (Unity documentation)

## Phase 1: Animation Setup

### 1a. Download Mixamo Animations
- [ ] Create Mixamo account (free tier)
- [ ] Download idle animations (5 variants)
- [ ] Download walk/run/sprint animations
- [ ] Download crouch animations
- [ ] Download jump/land animations
- [ ] Save all as FBX files to `Downloads/Mixamo/`
- [ ] Verify all use humanoid rig setting
- [ ] Total animations: 12+ clips

**Reference:** See MIXAMO_IMPORT_GUIDE.md

### 1b. Import into Unity
- [ ] Create folder: `Assets/Animations/Mixamo/`
- [ ] Organize by category (Idle/, Locomotion/, Crouch/, etc.)
- [ ] Import first FBX and create shared humanoid avatar
- [ ] Create `Assets/Animations/Avatar/humanoid_avatar.asset`
- [ ] Copy avatar to all other animations
- [ ] Verify all animations import without errors
- [ ] Each animation should appear in animation clip list

### 1c. Create Animator Controller
**Option 1: Manual (Recommended for first time)**
- [ ] Right-click in `Assets/Animations/` → Create → Animator Controller
- [ ] Rename to `PlayerLocomotion.controller`
- [ ] Double-click to edit
- [ ] Create parameters:
  - [ ] Locomotion_Speed (Float, 0-1)
  - [ ] Direction_X (Float, -1 to 1)
  - [ ] Direction_Y (Float, -1 to 1)
  - [ ] IsCrouching (Bool)
  - [ ] IsJumping (Bool)
  - [ ] IsLanding (Bool)
- [ ] Create states: Idle, Walk, Run, Sprint, CrouchIdle, CrouchWalk, Jump, Land
- [ ] Setup transitions per SPRINT2_LOCOMOTION_SETUP.md
- [ ] Assign animation clips to states
- [ ] Test transitions in preview

**Option 2: Automatic**
- [ ] In editor menu: Tools → Protocol EMR → Create Locomotion Animator
- [ ] Manually assign animations to states (still required)

### 1d. Add Animation Events
For each locomotion animation (Walk, Run, Sprint, Crouch Walk):
- [ ] Select animation in Project
- [ ] Open Animation window (Window → Animation → Animator)
- [ ] Add Events at ~40% and ~80% of animation
- [ ] Function: `FootstepEventTrigger.PlayFootstepSound()`
- [ ] Parameter: 0
- [ ] Repeat for forward, backward, and strafe variants

## Phase 2: Audio System Setup

### 2a. Create Audio Manager
- [ ] Verify AudioManager.cs exists at `Assets/Scripts/Core/Audio/`
- [ ] Audio Manager should:
  - [ ] Load footstep clips by surface type
  - [ ] Have AudioSource components for footsteps, breathing, landing
  - [ ] Support surface detection via raycast
  - [ ] Play contextual audio based on surface

### 2b. Prepare Audio Clips
Create folder structure:
```
Assets/Resources/Audio/
├─ Footsteps/
│  ├─ Concrete/ (3+ clips)
│  ├─ Metal/ (3+ clips)
│  ├─ Carpet/ (3+ clips)
│  ├─ Wood/ (2+ clips)
│  ├─ Glass/ (2+ clips)
│  └─ Digital/ (2+ clips)
├─ Breathing/
│  └─ breathing_loop.wav
└─ Landing/
   ├─ land_concrete.wav
   └─ land_metal.wav
```

Options:
- [ ] Download from Freesound.org (Creative Commons)
- [ ] Download from Zapsplat.com (free)
- [ ] Use existing game audio library
- [ ] Create placeholder audio (wav files)

Specifications:
- [ ] Format: WAV, 16-bit, 48kHz
- [ ] Mono for world-space audio
- [ ] ~100KB max per clip for memory efficiency
- [ ] 3-5 variants per surface for variation

### 2c. Setup Surface Tags
For all ground/floor objects in scene:
- [ ] Add tags: "Concrete", "Metal", "Carpet", "Wood", "Glass", "Digital"
- [ ] Assign appropriate tag to each surface collider
- [ ] Create Layer: "Footstep Surfaces" (optional but recommended)

## Phase 3: UI System Setup

### 3a. Create Stamina Display
- [ ] Create UI Canvas (right-click hierarchy → UI → Canvas)
- [ ] Add Panel child for background
- [ ] Add Image child for stamina bar (fill type: Horizontal)
- [ ] Add Text child for percentage display
- [ ] Add StaminaDisplay.cs script to canvas
- [ ] Assign UI components in inspector:
  - [ ] staminaBar: Image component
  - [ ] staminaText: Text component
  - [ ] canvasGroup: CanvasGroup component

### 3b. Configure Stamina UI
- [ ] normalColor: White or green
- [ ] depletedColor: Red
- [ ] fadeOutDuration: 0.5s
- [ ] fadeOutDelay: 1.0s after recovery

## Phase 4: Script Integration

### 4a. Update PlayerController
Verify these changes:
- [ ] New parameters added:
  - [ ] runSpeed = 8.0f
  - [ ] sprintSpeed = 12.0f
  - [ ] crouchSpeed = 3.0f
  - [ ] acceleration = 20f
  - [ ] deceleration = 15f
- [ ] AnimationController reference
- [ ] Audio settings (footstepInterval, etc.)
- [ ] Methods added:
  - [ ] UpdateAnimations()
  - [ ] HandleFootsteps()
  - [ ] HandleBreathing()
  - [ ] HandleJumpLanding()

### 4b. Add AnimationController
- [ ] Script exists at `Assets/Scripts/Core/Animation/AnimationController.cs`
- [ ] Methods working:
  - [ ] SetLocomotionSpeed()
  - [ ] SetCrouching()
  - [ ] PlayJump()
  - [ ] PlayLanding()

### 4c. Add FootstepEventTrigger
- [ ] Script exists at `Assets/Scripts/Core/Animation/FootstepEventTrigger.cs`
- [ ] Add as component to character model (under Player)
- [ ] Verify PlayFootstepSound() is callable from animation events

### 4d. Update SettingsManager
Verify new Sprint 2 settings:
- [ ] cameraBobIntensity (0-1)
- [ ] sprintToggleMode (bool)
- [ ] movementSpeedMultiplier (0.5-1.5)
- [ ] footstepVolumeMultiplier (0-1)
- [ ] enableBreathingAudio (bool)

## Phase 5: Scene Setup

### 5a. Create Test Scene
- [ ] Open `Assets/Scenes/Main.unity`
- [ ] Or create new scene: `Assets/Scenes/Locomotion_Test.unity`

### 5b. Setup Player GameObject
- [ ] Create empty GameObject: "Player"
- [ ] Add CharacterController component:
  - [ ] Height: 1.8
  - [ ] Radius: 0.4
  - [ ] Center: (0, 0.9, 0)
- [ ] Add PlayerController script:
  - [ ] Assign animation controller field
- [ ] Add AnimationController script

### 5c. Add Character Model
- [ ] Drag imported Mixamo FBX as child of Player
- [ ] Set position to (0, 0, 0)
- [ ] In Animator component:
  - [ ] Avatar: humanoid_avatar.asset
  - [ ] Controller: PlayerLocomotion.controller
- [ ] Delete or disable mesh (keep skeleton)
- [ ] Add FootstepEventTrigger script to model

### 5d. Add Camera & Input
- [ ] Add Camera as child of Player (for first-person)
- [ ] Position at (0, 1.6, 0) relative to Player
- [ ] Add FirstPersonCamera script (from Sprint 1)
- [ ] Verify InputManager exists in scene or is singleton

### 5e. Add UI Canvas
- [ ] Create UI Canvas
- [ ] Add Stamina Display panel (see Phase 3)
- [ ] Position in top-left or center

### 5f. Setup Audio Manager
- [ ] Create empty GameObject: "AudioManager"
- [ ] Add AudioManager script
- [ ] Verify it finds audio clips in Resources/Audio/

### 5g. Setup Ground
- [ ] Create simple ground plane or import test level
- [ ] Add collider with "Concrete" tag
- [ ] Add to "Footstep Surfaces" layer (if using)
- [ ] Create different surface areas:
  - [ ] Concrete patch
  - [ ] Metal grating
  - [ ] Carpet area

## Phase 6: Testing

### 6a. Basic Movement
- [ ] Run game in editor
- [ ] Press W to move forward
  - [ ] Player walks smoothly ✓
  - [ ] Walk animation plays ✓
  - [ ] Footstep sounds play ✓
  - [ ] Speed ~5.5 m/s ✓

- [ ] Hold Shift to sprint
  - [ ] Sprint animation plays ✓
  - [ ] Speed increases to ~12 m/s ✓
  - [ ] Stamina bar appears ✓
  - [ ] Stamina depletes ✓
  - [ ] Breathing sound plays ✓

- [ ] Release Shift
  - [ ] Slows to walk ✓
  - [ ] Stamina regenerates ✓

- [ ] Stamina depletes completely
  - [ ] Forced to walk ✓
  - [ ] 3-second recovery starts ✓

### 6b. Crouch Movement
- [ ] Press Ctrl to crouch
  - [ ] Player height decreases to 1.2m ✓
  - [ ] Crouch animation plays ✓
  - [ ] Speed reduces ✓

- [ ] Walk while crouching
  - [ ] Crouch walk animation plays ✓
  - [ ] Quieter footsteps? (Check audio levels)

- [ ] Release Ctrl
  - [ ] Returns to standing ✓

### 6c. Jumping
- [ ] Press Space
  - [ ] Jump animation plays ✓
  - [ ] Character leaves ground ✓
  - [ ] Gravity applied ✓

- [ ] Land
  - [ ] Landing animation plays ✓
  - [ ] Impact sound plays ✓
  - [ ] Camera shake (if enabled) ✓

### 6d. Footstep Audio
- [ ] Walk on different surfaces
  - [ ] Concrete: Expected audio ✓
  - [ ] Metal: Different pitch/tone ✓
  - [ ] Carpet: Softer sound ✓

- [ ] Sprint
  - [ ] Footsteps faster (~0.35s interval) ✓
  - [ ] Louder volume ✓

- [ ] Idle
  - [ ] No footsteps ✓

### 6e. Stamina UI
- [ ] Stamina bar visible while sprinting ✓
- [ ] Bar depletes in real-time ✓
- [ ] Turns red as depleted ✓
- [ ] Fades out after recovery ✓
- [ ] Text percentage updates ✓

### 6f. Performance
- [ ] Frame rate: Maintain 60 FPS ✓
- [ ] No animation stuttering ✓
- [ ] Smooth movement (no input lag) ✓
- [ ] Audio doesn't lag/stutter ✓

### 6g. Accessibility
- [ ] Camera bob works (enabled/disabled) ✓
- [ ] Motion blur toggle works ✓
- [ ] Camera shake toggle works ✓
- [ ] Settings persist between sessions ✓

## Phase 7: Polish

### 7a. Animation Tweaking
- [ ] Blend tree thresholds feel right:
  - [ ] Walk → Run transition smooth ✓
  - [ ] Run → Sprint transition sharp ✓
- [ ] Animation transitions (0.2s) feel natural ✓
- [ ] No sliding or feet skipping ✓

### 7b. Audio Polish
- [ ] Footstep timing syncs with animation ✓
- [ ] No double-audio or missed steps ✓
- [ ] Breathing audio volume appropriate ✓
- [ ] Landing sound satisfying ✓

### 7c. Movement Feel
- [ ] Acceleration feels snappy (20 m/s²) ✓
- [ ] Deceleration feels responsive ✓
- [ ] No momentum buildup issues ✓
- [ ] Turn radius feels natural ✓

## Phase 8: Documentation

### 8a. Code Documentation
- [ ] All public methods have XML documentation ✓
- [ ] AnimationController methods explained ✓
- [ ] AudioManager surface detection documented ✓
- [ ] StaminaDisplay UI logic documented ✓

### 8b. Setup Guides
- [ ] MIXAMO_IMPORT_GUIDE.md complete ✓
- [ ] SPRINT2_LOCOMOTION_SETUP.md complete ✓
- [ ] Animation controller structure documented ✓
- [ ] Audio folder structure documented ✓

### 8c. Known Issues
- [ ] Document any animation clipping issues ✓
- [ ] Note performance optimization tips ✓
- [ ] List compatibility notes ✓

## Phase 9: Integration Testing

### 9a. Sprint 1 Compatibility
- [ ] InputManager works with new movement ✓
- [ ] SettingsManager persists Sprint 2 settings ✓
- [ ] FirstPersonCamera compatible with animations ✓
- [ ] Performance monitor shows expected stats ✓

### 9b. Edge Cases
- [ ] Jump while sprinting works ✓
- [ ] Jump while crouching blocked ✓
- [ ] Landing during crouch handled ✓
- [ ] Sprint on ramp works (no sliding) ✓
- [ ] Rapid direction changes handled ✓

## Phase 10: Final Checks

### 10a. Build & Performance
- [ ] No console errors ✓
- [ ] No memory leaks (check profiler) ✓
- [ ] 60 FPS sustained ✓
- [ ] Build size reasonable ✓

### 10b. Acceptance Criteria (from ticket)
**Movement:**
- [ ] Press W: Walk smoothly at 5.5 m/s ✓
- [ ] Hold Shift: Sprint at 12 m/s, stamina depletes ✓
- [ ] Stamina depleted: Return to walk ✓
- [ ] Press Ctrl: Crouch at 3 m/s ✓
- [ ] Press Space: Jump with animation ✓

**Audio:**
- [ ] Footsteps sync with animation ✓
- [ ] Different surfaces have different audio ✓
- [ ] Breathing plays during sprint ✓
- [ ] Landing sound on jump ✓

**Quality:**
- [ ] Responsive (<50ms input lag) ✓
- [ ] Stable 60 FPS ✓
- [ ] No animation clipping ✓

### 10c. Git Commit
- [ ] Add all files: `git add .`
- [ ] Commit: `git commit -m "feat: Sprint 2 - Player locomotion & animations with Mixamo integration"`
- [ ] Push: `git push origin feat/sprint2-player-locomotion-mixamo`

## Troubleshooting

### Animation Issues
- **Q:** Animations don't play
- **A:** Check animator controller is assigned to Animator component

- **Q:** Bone structure wrong
- **A:** Regenerate humanoid avatar with Configure button

- **Q:** Animation jerky or stuttering
- **A:** Check Resample Curves is enabled in animation import

### Audio Issues
- **Q:** Footsteps don't play
- **A:** Check clips are in Assets/Resources/Audio/ folder

- **Q:** Wrong surface audio plays
- **A:** Verify surface tag matches SurfaceType enum

- **Q:** Audio cuts out
- **A:** Check Audio Source isn't being destroyed

### Performance Issues
- **Q:** FPS drops below 60
- **A:** Check animation compression settings
- **A:** Reduce number of audio sources
- **A:** Profile to find bottleneck

### Movement Issues
- **Q:** Player doesn't move
- **A:** Check InputManager is enabled
- **A:** Verify CharacterController is enabled

- **Q:** Player slides on slopes
- **A:** Check character's forward movement isn't clipping terrain

## Success Criteria

Sprint 2 is complete when:
1. ✓ All Mixamo animations imported and playing
2. ✓ Animator controller with smooth state transitions
3. ✓ Footstep audio synced to animation events
4. ✓ Surface detection working (different sounds per surface)
5. ✓ Stamina system visible and functional
6. ✓ Jump/crouch mechanics working
7. ✓ Breathing audio during sprint
8. ✓ All accessibility settings functional
9. ✓ 60 FPS maintained during all movements
10. ✓ Input responsiveness <50ms
11. ✓ All tests passing
12. ✓ Documentation complete

See next: **SPRINT 3 - Combat & Melee/Ranged Animations**
