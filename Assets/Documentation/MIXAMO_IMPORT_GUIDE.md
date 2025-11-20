# Mixamo Animation Import Guide

## Quick Start
1. Go to https://www.mixamo.com
2. Create free account (no premium needed)
3. Download animations as FBX
4. Import into Unity
5. Follow setup steps below

## Detailed Process

### Step 1: Download from Mixamo

#### Account Setup
- Visit https://www.mixamo.com
- Sign in with Adobe ID (create free if needed)
- No subscription required - free tier includes 100+ animations

#### Finding Animations
Use search for these categories:

**Idle Animations:**
- Idle
- Idle Breathing
- Idle Look Around
- Idle Scratching Head
- Idle Tired
- Idle Injured

**Locomotion:**
- Walk Forward
- Run Forward
- Sprint Forward
- Walk Backward
- Strafe Left
- Strafe Right
- Walk Cycle (generic)
- Run Cycle (generic)

**Jump & Platforming:**
- Jump
- Jump Land
- Double Jump
- Jump Up
- Vault Over

**Crouch:**
- Crouch Idle
- Crouch Walk Forward
- Crouch Walk Backward
- Crouch Run

**Special Moves (Optional):**
- Climb
- Climb Wall

### Step 2: Configure Download Settings

**For each animation:**

1. Click **"DOWNLOAD"** button
2. Set options:
   - **Format:** FBX (Binary)
   - **Skin:** Off ✓ (skeletal only, smaller file)
   - **Frames per Second:** 60 fps
   - **Keyframe Reduction:** On (default)
   - **Properies:** 
     - Rotation representation: Euler
     - Medium precision

3. Click "Download"
4. Save to `Downloads/Mixamo/`

### Step 3: Import into Unity

#### Folder Organization
Create this structure in your project:

```
Assets/
├─ Animations/
│  ├─ Mixamo/
│  │  ├─ Idle/
│  │  │  ├─ Idle_Breathing.fbx
│  │  │  ├─ Idle_Look_Around.fbx
│  │  │  └─ ...
│  │  ├─ Locomotion/
│  │  │  ├─ Walk_Forward.fbx
│  │  │  ├─ Run_Forward.fbx
│  │  │  ├─ Sprint_Forward.fbx
│  │  │  ├─ Walk_Backward.fbx
│  │  │  ├─ Strafe_Left.fbx
│  │  │  ├─ Strafe_Right.fbx
│  │  │  ├─ Jump_Start.fbx
│  │  │  └─ Jump_Land.fbx
│  │  ├─ Crouch/
│  │  │  ├─ Crouch_Idle.fbx
│  │  │  ├─ Crouch_Walk_Forward.fbx
│  │  │  └─ Crouch_Walk_Backward.fbx
│  │  └─ Special/
│  │     ├─ Vault_Diagonal.fbx
│  │     └─ Climb_Front.fbx
│  ├─ Avatar/
│  │  └─ humanoid_avatar.asset
│  └─ PlayerLocomotion.controller
└─ ...
```

#### Import Process

1. **First Time: Create Avatar**
   - Import first Mixamo FBX into `Assets/Animations/Mixamo/Idle/`
   - In Inspector, expand **Model** section:
     - Rig → Avatar Definition: **Create From This Model**
     - Rig → Humanoid (if not already selected)
   - Click **Configure** next to Avatar Definition
   - Verify bone mapping (should show green checkmarks)
   - Click **Done**
   - Apply

2. **Subsequent Imports: Use Shared Avatar**
   - Import remaining FBX files
   - In Inspector for each:
     - Rig → Avatar Definition: **Copy From Other Avatar**
     - Select the humanoid_avatar.asset from first import
   - Apply to all

#### Import Settings Per File

**Model Tab:**
```
Import Geometry: ✓
Import Blend Shapes: ✗
Optimized Mesh: ✓
Materials: ✗ (we'll use Unity materials)
Skins: ✗ (skeletal only)
```

**Rig Tab:**
```
Avatar Definition: Copy From Other Avatar
Animator: Auto Existing Avatar
```

**Animation Tab:**
```
Import Animation: ✓
Bake Animation: ✗
Resample Curves: ✓
Animation Compression: High Quality (or Best Compression for smaller files)
Anim. Root Transform Rotation: ✓ Bake Into Pose
Anim. Root Transform XZ Position: ✓ Bake Into Pose
Anim. Root Transform Y Position: ✗ (keep vertical movement for jumps)
```

**Materials Tab:**
```
Import Materials: ✗
```

**Tangents & Normals:**
```
Normals: Import
Normals & Tangents: Calculate
```

### Step 4: Create Shared Humanoid Avatar

This is critical for all animations to work together.

1. Go to `Assets/Animations/Mixamo/Avatar/`
2. Create folder if not exists
3. Import FIRST animation FBX
4. In Inspector:
   - Model → Rig → Avatar Definition: **Create From This Model**
   - Click **Configure**
   - In Configuration window:
     - Head: Select head bone (usually "Head")
     - Chest: Select chest bone
     - Left Arm: Shoulder → Upper Arm → Lower Arm → Hand
     - Right Arm: (mirror left)
     - Left Leg: Hip → Knee → Ankle → Foot → Toes
     - Right Leg: (mirror left)
   - Click **Done**
3. Apply

**Verify Avatar Created:**
- A new `.asset` file appears in `Avatar/` folder
- Should be named similar to model (e.g., `Character_avatar.asset`)

### Step 5: Configure All Animations with Avatar

For EACH remaining animation FBX:

1. Select in Project
2. In Inspector, go to **Rig** tab
3. Avatar Definition dropdown: **Copy From Other Avatar**
4. Drag the humanoid_avatar.asset into the field
5. Apply
6. Wait for import (progress bar at bottom)

**Result:** All animations now share the same humanoid skeleton.

### Step 6: Create Animation Clips

After importing, you can see animation clips in:
- Project: Navigate to Animations/Mixamo/Idle/, etc.
- Each FBX shows its animation in the timeline

To use in animator:
1. Drag FBX into scene (temporary)
2. Copy animation clip from it
3. Place clips in logical folders
4. Delete temporary instance

### Step 7: Setup in Scene

#### Add Character to Scene

1. Create empty GameObject: `Player`
2. Add components:
   - CharacterController
   - PlayerController (script)
   - AnimationController (script)
3. Add child object:
   - Drag a Mixamo FBX into hierarchy
   - Position at (0, 0, 0) relative to Player
   - In Inspector of child:
     - Animator → Avatar: humanoid_avatar.asset
     - Animator → Controller: PlayerLocomotion.controller
4. Delete/disable mesh and cloth (keep only skeleton)

#### Add Animations to Controller States

Using Animator Window:

1. In `PlayerLocomotion.controller`:
   - Double-click to enter editing mode
   - Find state "Idle"
   - In Inspector, Motion field: Select Idle_Breathing animation
   - Set Mask to "Default"
2. Repeat for other states:
   - Walk → Walk_Forward
   - Run → Run_Forward
   - Sprint → Sprint_Forward
   - Crouch Idle → Crouch_Idle
   - etc.

### Step 8: Add Animation Events (Footsteps)

In Animation window for locomotion clips:

1. Select Walk_Forward animation
2. In Timeline/Animation panel:
   - Expand Events
   - Add event at 40% of clip (red marker)
   - Function: `FootstepEventTrigger.PlayFootstepSound`
   - Parameter: 0
3. Add second event at 80% (second footstep)

Repeat for:
- Walk_Forward, Walk_Backward
- Run_Forward, Run_Backward
- Sprint_Forward
- Crouch_Walk_Forward, Crouch_Walk_Backward

### Step 9: Troubleshooting

#### Issue: "Avatar is incompatible with rig"
**Solution:** 
- Ensure humanoid checkbox is enabled
- Run Configure on first animation
- Copy avatar to all others
- Reimport

#### Issue: Animation plays but bones don't match
**Solution:**
- Check Avatar has all required bones mapped
- Open Configure window again
- Ensure Left/Right are correct
- Test with a simple animation first

#### Issue: Avatar shows but is in wrong pose
**Solution:**
- Blend Shape might be baked
- Try reimporting with "Bake Animation" OFF
- Or check if root bone is properly set

#### Issue: Animation plays twice speed
**Solution:**
- Check "Resample Curves" is checked
- Ensure framerate matches (60 fps from Mixamo)
- Verify animation length in Inspector

#### Issue: Bones twist unnaturally
**Solution:**
- This is normal for mocap - it's realism!
- Adjust blend tree thresholds
- Or select different animation variant
- Use IK pass in animation events to correct

### Step 10: Performance Tips

1. **Use Humanoid rig** (not Generic) - much faster
2. **Bake root motion** for locomotion:
   - In Animation clip Inspector
   - Check "Bake Into Pose" for X/Z position
   - Keep Y unchecked (for gravity)
3. **Optimize models:**
   - Mixamo: Skin OFF when downloading
   - Remove mesh, keep armature only
   - Merge bones: Select model → Optimize
4. **Use compression:**
   - Animation compression: High Quality or Best Compression
   - Tangents & Normals: Calculate
5. **LOD groups:**
   - Not needed for single character
   - Useful if spawning many characters

### Free Character Models

If you need character models to go with animations:

**Humanoid Characters:**
- **Mixamo:** Free humanoid models with rigs
- **Sketchfab:** Filter by Humanoid/Downloadable
- **CGTrader:** Free low-poly characters

**Quick Setup:**
1. Download character with humanoid rig
2. Import with same process as above
3. Use existing humanoid_avatar.asset
4. Should work immediately

### Helpful Links

- Mixamo: https://www.mixamo.com
- Mixamo Help: https://www.mixamo.com/help
- Unity Humanoid Rig: https://docs.unity3d.com/Manual/AvatarCreationandSetup.html
- Animation Events: https://docs.unity3d.com/Manual/AnimationEvents.html
- Animator Parameters: https://docs.unity3d.com/Manual/AnimatorParameters.html

### Next Steps

1. ✅ Import all required animations
2. ✅ Setup shared humanoid avatar
3. ✅ Assign animations to animator states
4. ✅ Add animation events for footsteps
5. → Test in game!

See **SPRINT2_LOCOMOTION_SETUP.md** for animation controller creation and testing.
