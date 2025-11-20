# Protocol EMR - Unity Project Foundation

## Project Overview

Protocol EMR is a first-person narrative-driven game with procedurally generated content, NPC AI systems, and immersive audio design. This repository contains the complete Unity project foundation with core systems ready for development.

**Current Status**: Sprint 8 - Unknown Dialogue System Complete ✅

---

## Features (Sprint 1)

### ✅ Implemented

- **Input System**: Fully remappable controls with keyboard/mouse and gamepad support
- **First-Person Camera**: Smooth mouse look with camera bobbing and FOV adjustment
- **Player Controller**: WASD movement with sprint stamina, crouch, and jump mechanics
- **Settings System**: Graphics, audio, gameplay, and accessibility settings with JSON persistence
- **Performance Monitor**: Real-time FPS counter and memory usage display (toggle with F1)
- **Project Structure**: Complete folder hierarchy ready for asset integration

### Input Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD | Left Stick |
| Look | Mouse | Right Stick |
| Sprint | Left Shift | Left Stick Press |
| Crouch | Left Ctrl | B (East) |
| Jump | Space | A (South) |
| Interact | E | X (West) |
| Inventory | I | D-Pad Up |
| Phone | C | D-Pad Down |
| Pause | ESC | Start |
| Fire | LMB | Right Trigger |
| Aim | RMB | Left Trigger |
| Copy Seed | F8 | - |

---

## Getting Started

### Prerequisites

- **Unity 2022.3 LTS** or newer
- **Git** (for version control)
- **Git LFS** (recommended for binary assets)

### Installation

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd protocol-emr
   ```

2. **Open in Unity Hub**:
   - Open Unity Hub
   - Click "Open" → Navigate to project folder
   - Select Unity 2022.3.15f1 or compatible version

3. **Install required packages** (should auto-install):
   - Universal Render Pipeline
   - Input System
   - TextMeshPro

4. **Open the test scene**:
   - Navigate to `Assets/Scenes/Main.unity`
   - Press Play to test

### First Run

When you first run the project:
1. Press **WASD** to move
2. Move **mouse** to look around
3. Press **ESC** to pause/unpause
4. Press **F1** to toggle performance monitor
5. Settings auto-save to: `Application.persistentDataPath/game_settings.json`

---

## Project Structure

```
Assets/
├── Scenes/                 # Unity scene files
├── Scripts/                # C# scripts
│   ├── Core/
│   │   ├── Input/         # InputManager, PlayerInputActions
│   │   ├── Camera/        # FirstPersonCamera
│   │   ├── Player/        # PlayerController
│   │   ├── Settings/      # SettingsManager
│   │   └── Performance/   # PerformanceMonitor
│   └── Systems/           # Future gameplay systems
├── Prefabs/               # Reusable game objects
├── Materials/             # PBR materials
├── Models/                # 3D models (FBX, OBJ)
├── Textures/              # Texture maps
├── Audio/                 # Sound effects, music, voice
├── Animations/            # Animation clips and controllers
├── VFX/                   # Visual effects and shaders
├── UI/                    # UI sprites and prefabs
└── Documentation/         # Setup guides and standards
    ├── ProjectSetup.md    # Unity project setup guide
    └── AssetSourcing.md   # Free asset sources
```

---

## Core Systems

### InputManager
Centralized input handling with event-driven architecture.

**Usage**:
```csharp
using ProtocolEMR.Core.Input;

void Start()
{
    InputManager.Instance.OnJump += HandleJump;
}

void HandleJump()
{
    Debug.Log("Player jumped!");
}
```

**Features**:
- Action maps: Movement, Look, Interact, UI, Combat
- Input rebinding system
- Gamepad support with auto-detection
- Hold vs Toggle options for sprint/crouch

### FirstPersonCamera
Smooth first-person camera with advanced features.

**Features**:
- Mouse look with adjustable sensitivity
- Camera bobbing during movement (toggle in settings)
- FOV adjustment (60-120° range)
- Camera shake system (for impact feedback)
- Accessibility options (motion blur, shake intensity)

### PlayerController
Physics-based character controller with movement mechanics.

**Features**:
- WASD movement with sprint and crouch
- Stamina system (drains during sprint, regenerates when idle)
- Jump mechanics with gravity
- Interaction raycast system (E to interact)
- Collision detection with CharacterController

### SettingsManager
Comprehensive settings system with persistence.

**Settings Categories**:
- **Graphics**: Quality presets, resolution, VSync, frame rate
- **Audio**: Master, music, SFX, voice volume
- **Gameplay**: Mouse sensitivity, difficulty, HUD options
- **Accessibility**: Colorblind mode, motion blur, FOV, camera shake

**Persistence**: Settings saved to JSON at:
```
Windows: C:/Users/[User]/AppData/LocalLow/[Company]/ProtocolEMR/game_settings.json
macOS: ~/Library/Application Support/[Company]/ProtocolEMR/game_settings.json
Linux: ~/.config/unity3d/[Company]/ProtocolEMR/game_settings.json
```

### PerformanceMonitor
Debug tool for tracking performance metrics.

**Metrics**:
- FPS (frames per second)
- Frame time (milliseconds)
- Memory usage (MB)
- Resolution and quality level

**Toggle**: Press **F1** during gameplay

---

## Configuration

### Input Bindings

Custom bindings saved to: `Application.persistentDataPath/input_bindings.json`

**Rebind Example**:
```csharp
InputManager.Instance.StartRebinding("Movement/Jump", 0, (success) =>
{
    if (success)
        Debug.Log("Rebinding successful!");
});
```

### Performance Targets

| Setting | Target | Notes |
|---------|--------|-------|
| FPS | 60+ | At 1080p Medium settings |
| Memory | <2GB | Foundation only |
| Load Time | <3s | Per scene |
| Input Latency | <16ms | 1 frame at 60fps |

---

## Development Roadmap

### Sprint 1: Foundation ✅ (Current)
- Project initialization
- Input system
- Camera controller
- Player movement
- Settings architecture

### Sprint 2: Locomotion (Next)
- Mixamo animation integration
- Animation state machine
- Blend trees for movement
- IK (Inverse Kinematics)

### Sprint 3: Combat Foundation
- Weapon system
- Projectile mechanics
- Hit detection
- Recoil patterns

### Sprint 4: NPC AI Foundation
- Behavior trees
- NavMesh pathfinding
- Perception system
- State machines

### Sprint 5-6: UI Systems
- HUD design
- Inventory system
- Phone interface
- Menu screens

### Sprint 7-8: Audio Integration
- FMOD/Wwise integration
- Dynamic music system
- 3D audio positioning
- Voice line system

### Sprint 9-10: Content & Polish
- Procedural generation
- Mission system
- VFX polish
- Performance optimization

See full roadmap: [`/docs/protocol-emr/build-coding-roadmap.md`](./docs/protocol-emr/build-coding-roadmap.md)

---

## Testing

### Manual Testing Checklist

- [ ] WASD movement in all directions
- [ ] Mouse look (horizontal and vertical)
- [ ] Sprint stamina depletes and regenerates
- [ ] Crouch reduces player height
- [ ] Jump works when grounded
- [ ] ESC pauses game (time stops)
- [ ] Settings persist after restart
- [ ] Input rebinding works
- [ ] FPS counter displays correctly (F1)
- [ ] No console errors on startup

### Automated Testing
- Unit tests: Coming in Sprint 2
- Integration tests: Coming in Sprint 3

---

## Performance Profiling

### Unity Profiler
1. **Window → Analysis → Profiler**
2. Enable "Deep Profile" for detailed analysis
3. Focus areas:
   - CPU: Look for spikes in Update/FixedUpdate
   - Memory: Check for memory leaks
   - Rendering: Monitor draw calls and batching

### Built-in Monitor
- Press **F1** in-game
- Green bar: 60+ FPS (good)
- Yellow bar: 30-60 FPS (acceptable)
- Red bar: <30 FPS (needs optimization)

---

## Contributing

### Code Standards
- Follow C# naming conventions (PascalCase for methods, camelCase for fields)
- Use XML documentation for public APIs
- Keep methods focused and under 50 lines
- Use namespaces: `ProtocolEMR.Core.[System]`

### Commit Messages
```
feat: Add player sprint stamina system
fix: Camera bobbing not resetting on idle
docs: Update ProjectSetup.md with URP config
refactor: Extract input callbacks to separate methods
```

### Branch Strategy
- `main`: Stable releases only
- `develop`: Active development
- `feature/[name]`: Feature branches
- `sprint/[number]`: Sprint work

---

## Documentation

- **[Project Setup Guide](./Assets/Documentation/ProjectSetup.md)**: Unity configuration and setup
- **[Asset Sourcing Guide](./Assets/Documentation/AssetSourcing.md)**: Free asset sources and import procedures
- **[Build & Coding Roadmap](./docs/protocol-emr/build-coding-roadmap.md)**: Complete development roadmap

---

## Known Issues

### Sprint 1
- Player capsule is placeholder (no visual model)
- No UI elements yet (coming in Sprint 5)
- Audio system uses Unity built-in (FMOD/Wwise in Sprint 7)
- No animation system (coming in Sprint 2)

---

## License

[Specify license - e.g., MIT, GPL, Proprietary]

---

## Credits

### Free Asset Sources
- **Mixamo**: Character animations
- **Poly Haven**: PBR materials and HDRIs
- **Freesound**: Audio effects
- **Unity Asset Store**: UI elements

See full credits in `CREDITS.txt` (to be added in Sprint 6)

---

## Procedural Seed System

Protocol EMR features a deterministic procedural generation system that ensures reproducible gameplay experiences through seed-based randomization.

### Key Features

- **Deterministic Generation**: Same seed = identical world layout, NPC placement, and story beats
- **Scope-based Seeding**: Separate random streams for NPCs, chunks, audio, story, loot, and environment
- **Settings Integration**: Players can set custom seeds or use auto-generated ones
- **Save/Load Support**: Seeds are preserved with game saves for exact state restoration

### Using Seeds

**In-Game**:
- Press **F8** to copy current seed to clipboard
- View current seed in Performance Monitor overlay (F1)
- Seeds are automatically saved with your game

**Settings**:
```csharp
// Enable custom seed in settings
SettingsManager.Instance.SetUseProceduralSeed(true);
SettingsManager.Instance.SetProceduralSeed(12345);
```

**Programmatic Usage**:
```csharp
// Get deterministic random values
int npcCount = SeedManager.Instance.GetRandomInt(SeedManager.SCOPE_NPCS, 1, 5);
Vector3 position = new Vector3(
    SeedManager.Instance.GetRandomFloat(SeedManager.SCOPE_NPCS, 0) * 100f,
    0f,
    SeedManager.Instance.GetRandomFloat(SeedManager.SCOPE_NPCS, 1) * 100f
);
```

### QA Testing

For reproducible testing:
1. Set a specific seed (e.g., 12345)
2. Document NPC positions and behaviors
3. Restart with same seed to verify identical generation
4. Test save/load to ensure state preservation

---

## Contact

**Project Lead**: [Your Name]
**Repository**: [Repository URL]
**Documentation**: `/docs/protocol-emr/`

---

## System Requirements

### Minimum
- **OS**: Windows 10 64-bit, macOS 10.14, Ubuntu 20.04
- **CPU**: Intel i5-7500 / AMD Ryzen 5 1600
- **GPU**: NVIDIA GTX 1060 / AMD RX 580
- **RAM**: 8GB
- **Storage**: 10GB

### Recommended
- **OS**: Windows 10/11 64-bit, macOS 11+, Ubuntu 22.04
- **CPU**: Intel i7-9700K / AMD Ryzen 7 3700X
- **GPU**: NVIDIA RTX 2060 / AMD RX 5700
- **RAM**: 16GB
- **Storage**: 10GB SSD

---

**Sprint 1 Foundation - Complete** ✅

Next: Sprint 2 - Locomotion & Animations 🎯
