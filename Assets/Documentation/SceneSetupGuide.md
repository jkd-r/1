# Protocol EMR - Scene Setup Guide

## Quick Start: Creating Your First Test Scene

This guide walks you through setting up a basic test scene with all core systems working.

---

## Step 1: Create New Scene

1. **File → New Scene**
2. Save as: `Assets/Scenes/Main.unity`

---

## Step 2: Set Up Core Systems

### GameManager
1. **Create Empty GameObject**: Right-click Hierarchy → Create Empty
2. **Rename**: "GameManager"
3. **Add Component**: GameManager script
4. Position: (0, 0, 0)

**Note**: GameManager will auto-instantiate InputManager, SettingsManager, and PerformanceMonitor if not present.

---

## Step 3: Create Player

### Player GameObject
1. **Create Capsule**: Right-click Hierarchy → 3D Object → Capsule
2. **Rename**: "Player"
3. **Transform**:
   - Position: (0, 1, 0)
   - Rotation: (0, 0, 0)
   - Scale: (1, 1, 1)

### Add Components to Player
4. **Remove Capsule Collider** (we'll use CharacterController instead)
5. **Add Component**: Character Controller
   - Height: 2
   - Radius: 0.5
   - Center: (0, 0, 0)
6. **Add Component**: PlayerController script
   - Walk Speed: 5
   - Sprint Speed: 8
   - Crouch Speed: 2.5
   - Max Stamina: 100
7. **Set Layer**: Player (Layer 8)

### Create Camera Child
8. **Right-click Player → Camera**
9. **Rename**: "PlayerCamera"
10. **Transform**:
    - Position: (0, 0.6, 0)  ← Eye height
    - Rotation: (0, 0, 0)
11. **Add Component**: FirstPersonCamera script
    - Player Body: Drag Player GameObject here
    - Mouse Sensitivity: 1.0
    - Default FOV: 90

12. **Remove** default "Main Camera" from scene (we're using PlayerCamera)
13. **Tag PlayerCamera as "MainCamera"**

---

## Step 4: Create Environment

### Floor
1. **Create Plane**: Right-click Hierarchy → 3D Object → Plane
2. **Rename**: "Floor"
3. **Transform**:
   - Position: (0, 0, 0)
   - Scale: (2, 1, 2)  ← 20x20 units
4. **Set Layer**: Ground (Layer 12)

### Walls (Optional)
1. **Create Cubes** for walls:
   - North Wall: Position (0, 2.5, 10), Scale (20, 5, 0.5)
   - South Wall: Position (0, 2.5, -10), Scale (20, 5, 0.5)
   - East Wall: Position (10, 2.5, 0), Scale (0.5, 5, 20)
   - West Wall: Position (-10, 2.5, 0), Scale (0.5, 5, 20)

### Lighting
1. **Directional Light** (should exist by default)
   - Rotation: (50, -30, 0)
   - Intensity: 1
   - Color: White

---

## Step 5: Add Interactable Test Object

### Create Test Cube
1. **Create Cube**: Right-click Hierarchy → 3D Object → Cube
2. **Rename**: "InteractableCube"
3. **Transform**:
   - Position: (2, 1, 3)
   - Scale: (0.5, 0.5, 0.5)
4. **Add Component**: InteractableObject script
5. **Set Layer**: Interactable (Layer 9)

### Configure PlayerController Interaction
1. **Select Player**
2. **PlayerController component**:
   - Interaction Range: 3
   - Interactable Layer: Set to "Interactable" only

---

## Step 6: Configure Layers and Collisions

### Create Layers
1. **Edit → Project Settings → Tags and Layers**
2. **Add Layers**:
   - Layer 8: Player
   - Layer 9: Interactable
   - Layer 10: Enemy
   - Layer 11: Projectile
   - Layer 12: Ground

### Set Up Collision Matrix
1. **Edit → Project Settings → Physics**
2. **Layer Collision Matrix** (scroll down):
   - ✅ Player ↔ Ground
   - ✅ Player ↔ Interactable
   - ✅ Player ↔ Enemy
   - ❌ Player ↔ Player
   - ✅ Ground ↔ Everything

---

## Step 7: Configure URP (If Not Already Done)

### Check URP Asset
1. **Edit → Project Settings → Graphics**
2. Ensure "Scriptable Render Pipeline Settings" shows URP asset
3. If not, create one:
   - **Right-click Project → Create → Rendering → URP Asset (with Universal Renderer)**
   - Assign in Graphics settings

### Configure Camera
1. **Select PlayerCamera**
2. **Camera component**:
   - Rendering Path: Use Pipeline Settings
   - Allow HDR: Enabled
   - Allow MSAA: Enabled

---

## Step 8: Test the Scene

### Play Mode Testing
1. **Press Play**
2. **Test Controls**:
   - WASD: Move around
   - Mouse: Look around
   - Left Shift: Sprint (watch stamina)
   - Left Ctrl: Crouch (camera lowers)
   - Space: Jump
   - E: Interact with cube (when close)
   - ESC: Pause game
   - F1: Toggle performance monitor

### Expected Behavior
- ✅ Player moves smoothly on floor
- ✅ Mouse look rotates camera
- ✅ Sprint drains stamina bar (visible in PerformanceMonitor)
- ✅ Crouch reduces player height
- ✅ Jump works when on ground
- ✅ ESC pauses (time stops, cursor appears)
- ✅ Console shows "Interacted with: InteractableCube" when pressing E near cube
- ✅ No errors in Console

---

## Step 9: Create Prefabs

### Player Prefab
1. **Drag Player** from Hierarchy to `Assets/Prefabs/Player/`
2. **Rename**: "Player.prefab"

### Interactable Prefab
1. **Drag InteractableCube** from Hierarchy to `Assets/Prefabs/`
2. **Rename**: "InteractableCube.prefab"

---

## Step 10: Save and Build

### Save Scene
1. **File → Save**
2. Confirm save location: `Assets/Scenes/Main.unity`

### Add to Build Settings
1. **File → Build Settings**
2. **Click "Add Open Scenes"**
3. Main.unity should appear in "Scenes in Build"

---

## Advanced Setup (Optional)

### Add Test Door

1. **Create Empty GameObject**: "Door"
2. **Position**: (5, 0, 0)
3. **Create Cube as child**: "DoorMesh"
   - Position: (0, 1.5, 0)
   - Scale: (1, 3, 0.1)
4. **Add BoxCollider to Door** (parent)
5. **Add Component to Door**: InteractableDoor script
   - Door Pivot: Leave empty (will use self)
   - Open Angle: 90
   - Open Speed: 2
6. **Set Layer**: Interactable

**Test**: Walk up to door, press E to open/close

### Add Stamina UI (Simple Debug)

For now, stamina is visible in PerformanceMonitor (F1). Full UI comes in Sprint 5.

### Add Multiple Interactables

Create several InteractableCubes with different colors:
1. Duplicate InteractableCube (Ctrl+D)
2. Change Material color
3. Position around room
4. Test interaction range (3 units default)

---

## Troubleshooting

### Player Falls Through Floor
- ✅ Check Player has CharacterController
- ✅ Check Floor has collider
- ✅ Verify layers in Physics collision matrix

### Mouse Look Not Working
- ✅ Check FirstPersonCamera is attached to PlayerCamera
- ✅ Verify Player Body reference is set
- ✅ Check InputManager exists in scene
- ✅ Cursor should be locked (invisible) during play

### Input Not Responding
- ✅ Check InputManager exists (auto-created by GameManager)
- ✅ Verify Input System package installed
- ✅ Check Project Settings → Player → Active Input Handling = "Input System Package (New)"

### Camera Bobbing Too Much
- ✅ Select PlayerCamera
- ✅ Reduce Bob Amplitude (default: 0.05)
- ✅ Or disable Camera Bob in accessibility settings

### Interaction Not Working
- ✅ Check InteractableObject has Collider
- ✅ Verify Layer is set to "Interactable"
- ✅ Check PlayerController Interactable Layer mask includes Layer 9
- ✅ Test distance (default range: 3 units)
- ✅ Look directly at object (raycast from camera center)

### No Performance Monitor
- ✅ Press F1 to toggle
- ✅ Check PerformanceMonitor exists in scene
- ✅ GameManager should auto-create it

---

## Scene Hierarchy Example

```
Main.unity
├── GameManager (GameManager.cs)
├── Player (PlayerController.cs)
│   └── PlayerCamera (FirstPersonCamera.cs, Camera)
├── InputManager (InputManager.cs) [Auto-created]
├── SettingsManager (SettingsManager.cs) [Auto-created]
├── PerformanceMonitor (PerformanceMonitor.cs) [Auto-created]
├── Floor (Plane)
├── Walls (Optional)
│   ├── NorthWall (Cube)
│   ├── SouthWall (Cube)
│   ├── EastWall (Cube)
│   └── WestWall (Cube)
├── Directional Light
└── Interactables
    ├── InteractableCube (InteractableObject.cs)
    └── Door (InteractableDoor.cs) [Optional]
```

---

## Next Steps

After completing scene setup:
1. ✅ Test all input controls
2. ✅ Verify settings persistence (change sensitivity, restart Unity)
3. ✅ Create additional test scenes for specific features
4. → Move to **Sprint 2**: Add Mixamo character and animations

---

## Quick Commands

### Create Player Setup (Console)
```
// Copy to Unity Console for quick setup
GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
player.name = "Player";
player.transform.position = new Vector3(0, 1, 0);
Destroy(player.GetComponent<Collider>());
player.AddComponent<CharacterController>();
player.AddComponent<PlayerController>();

GameObject cam = new GameObject("PlayerCamera");
cam.transform.SetParent(player.transform);
cam.transform.localPosition = new Vector3(0, 0.6f, 0);
cam.AddComponent<Camera>();
cam.AddComponent<FirstPersonCamera>();
```

---

## Reference Documents

- **[Project Setup Guide](./ProjectSetup.md)**: Unity configuration
- **[Asset Sourcing Guide](./AssetSourcing.md)**: Free assets
- **[Build Roadmap](../../docs/protocol-emr/build-coding-roadmap.md)**: Development plan

---

**Last Updated**: Sprint 1 Foundation Phase
**Scene Template**: Available in `Assets/Scenes/Main.unity`
