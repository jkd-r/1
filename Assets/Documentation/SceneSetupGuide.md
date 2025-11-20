# Protocol EMR - Scene Setup Guide

## Quick Start: Creating Your First Test Scene

This guide walks you through setting up a basic test scene with all core systems working.

---

## ⚡ **RECOMMENDED: Automated Setup**

The fastest way to set up the Main scene is using the automated setup tool:

1. **Open Unity Editor** with Protocol EMR project
2. **Menu**: `Protocol EMR → Setup → Setup Main Scene`
3. **Wait**: All required systems will be created automatically
4. **Validate**: `Protocol EMR → Setup → Validate Scene`
5. **Done**: Press Play to test!

This creates:
- ✅ GameManager
- ✅ MissionSystem
- ✅ PlaytestFlowController
- ✅ RegressionHarness
- ✅ ProceduralLevelBuilder
- ✅ NPCSpawner
- ✅ DynamicEventOrchestrator
- ✅ WorldStateBlackboard
- ✅ PlayerSpawnPoint
- ✅ Basic level geometry (floor, lighting)

**Continue reading for manual setup instructions if needed.**

---

## Step 1: Create New Scene

1. **File → New Scene**
2. Save as: `Assets/Scenes/Main.unity`

---

## Step 2: Set Up Core Systems

### Automated Setup (Recommended)
1. **Menu**: `Protocol EMR → Setup → Setup Main Scene`
2. **Validate**: `Protocol EMR → Setup → Validate Scene`

### Manual Setup (Alternative)

#### GameManager
1. **Create Empty GameObject**: Right-click Hierarchy → Create Empty
2. **Rename**: "GameManager"
3. **Add Component**: GameManager script
4. Position: (0, 0, 0)

**Note**: GameManager will auto-instantiate InputManager, SettingsManager, PerformanceMonitor, SeedManager, and UnknownDialogueManager if not present.

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

## Step 11: Procedural Level Builder Setup (Sprint 10)

### Overview
The Procedural Level Builder automatically generates playable levels by stitching modular chunks together with deterministic NPC placement based on the SeedManager.

### Setup for ProceduralTest Scene

#### 1. Create ProceduralTest Scene
1. **File → New Scene**
2. **Save as**: `Assets/Scenes/ProceduralTest.unity`

#### 2. Add Core Infrastructure
1. **Create Empty GameObject**: "GameManager"
   - Add GameManager component (auto-instantiates core systems)
2. **Create Empty GameObject**: "LevelBuilder"
   - Add ProceduralLevelBuilder component
   - Add ProceduralLevelTester component (for testing UI)

#### 3. Configure ProceduralLevelBuilder
1. **Select LevelBuilder GameObject**
2. **ProceduralLevelBuilder component**:
   - Target Chunk Count: 8
   - Max Generation Time: 8 seconds
   - Bake NavMesh Async: ✅ Enabled
   - Log Generation Metrics: ✅ Enabled
   - Draw Debug Gizmos: ✅ Enabled (for development)
3. **Available Chunks**: Assign chunks from Resources/Procedural/Chunks/
4. **Starting Chunk**: Assign starting chunk definition
5. **NPC Spawner**: Drag NPCSpawner object from hierarchy

#### 4. Create NPC Spawner
1. **Create Empty GameObject**: "NPCSpawner"
   - Position: (0, 0, 0)
2. **Add NPCSpawner component**:
   - Spawn On Start: ✅ Enabled
   - Use Global Seed: ✅ Enabled
   - Max NPCs Per Level: 30
3. **Configure NPC Prefabs**: Assign NPC prefab references
4. **Assign to LevelBuilder**: Drag into NPC Spawner field

#### 5. Generate Test Chunks
1. **In Editor, Open Console Window**
2. **Run**: `Protocol EMR > Procedural > Generate Test Chunks`
   - Creates 3 example chunks (Corridor, Room, Vault)
3. **Validate**: `Protocol EMR > Procedural > Validate All Chunks`
   - Checks all chunk definitions for consistency

#### 6. Test the Procedural Scene
1. **Press Play**
2. **Expected Behavior**:
   - Level generates automatically on scene load
   - Console shows generation metrics:
     ```
     [LevelBuilder] Built 8 chunks in ~1.2s
     [LevelBuilder] NavMesh baking completed in ~0.4s
     [LevelBuilder] NPC zones provisioned in ~0.02s
     === LEVEL GENERATION METRICS ===
     Total Time: 1.703s (Budget: 8.000s)
     ```
   - NPCs spawn in procedurally determined zones
   - Same seed produces identical level layout

#### 7. Test Regeneration
- **Press R**: Regenerate level with same seed
- **Press C**: Clear level
- **F8**: Copy current seed to clipboard

### Creating Your Own Chunk

#### 1. Create Chunk Definition
1. **Right-click in Assets folder**
2. **Create > Protocol EMR > Procedural > Chunk Definition**
3. **Name**: "ChunkDef_MyChunk"

#### 2. Configure Chunk Properties
1. **Identity**:
   - Chunk ID: `chunk_my_corridor`
   - Display Name: `My Custom Corridor`
   - Biome Type: `interior`
2. **Dimensions**:
   - Chunk Size: `(10, 3, 20)` ← Width, Height, Depth
3. **Level Constraints**:
   - Min Level: 1
   - Max Level: 100
   - Selection Weight: 0.8 ← 80% chance to include
   - Allow Duplicates: ✅

#### 3. Add Connectors
1. **Expand Connectors array** → Set Size to 2
2. **Connector 0 (Entrance)**:
   - Tag: `corridor_start`
   - Local Position: `(0, 0, -10)` ← At chunk entrance
   - Connection Size: `(10, 3, 2)`
   - Allow Inbound: ✅
   - Allow Outbound: ❌
3. **Connector 1 (Exit)**:
   - Tag: `corridor_end`
   - Local Position: `(0, 0, 10)` ← At chunk exit
   - Connection Size: `(10, 3, 2)`
   - Allow Inbound: ❌
   - Allow Outbound: ✅

#### 4. Add NPC Zones
1. **Expand NPC Zone Templates** → Set Size to 1
2. **Zone 0**:
   - Zone Name: `patrol_zone`
   - Center Offset: `(0, 0, 0)`
   - Zone Size: `(8, 2, 18)`
   - Min NPCs: 1
   - Max NPCs: 2
   - Allowed Types: `Patrol`
   - Difficulty Multiplier: 1.0

#### 5. Create Chunk Prefab
1. **Build your chunk geometry** in a test scene
2. **Add colliders** for all solid surfaces
3. **Drag into Prefabs folder**: `Assets/Resources/Procedural/Prefabs/`
4. **Name**: "ChunkPrefab_MyChunk"
5. **Assign to ChunkDefinition**: Chunk Prefab field

#### 6. Validate and Test
1. **Run**: `Protocol EMR > Procedural > Validate All Chunks`
   - Fix any errors reported
2. **Add to LevelBuilder**: Add to Available Chunks array
3. **Regenerate level** (R key in test scene)
   - Your chunk should appear based on probability weight

#### 7. Stress Test
```
Protocol EMR > Procedural > Stress Test Chunk Generation
```
- Generates 100 different seeds
- Verifies deterministic ordering
- Reports usage distribution

### Chunk Hierarchy Example

```
ChunkPrefab_MyChunk
├── Floor (Plane or custom mesh)
├── Walls
│   ├── NorthWall
│   ├── SouthWall
│   ├── EastWall
│   └── WestWall
├── Obstacles
│   └── Crate (BoxCollider)
└── Lighting
    └── Point Light
```

### Performance Monitoring

**Expected Generation Times** (8s budget):
- Chunk Building: 500ms - 1500ms (70% budget = 5600ms available)
- NavMesh Baking: 500ms - 1000ms (20% budget = 1600ms available)
- NPC Provisioning: <100ms (10% budget = 800ms available)

**Total typical**: 1-3 seconds with ample margin

### Integration with Main Scene

To enable procedural generation in Main.unity:
1. **Add ProceduralLevelBuilder to GameManager's scene**
2. **Configure same as ProceduralTest setup**
3. **Game boots directly into generated level**
4. **Seed can be saved with game state** for replayability

---

## Next Steps

After completing scene setup:
1. ✅ Test all input controls
2. ✅ Verify settings persistence (change sensitivity, restart Unity)
3. ✅ Create additional test scenes for specific features
4. → **Sprint 10**: Set up ProceduralTest scene (see Step 11)
5. → Move to **Sprint 2**: Add Mixamo character and animations

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
