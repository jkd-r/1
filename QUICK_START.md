# Protocol EMR - Quick Start Guide

Get up and running with Protocol EMR v1.0.0 in under 5 minutes.

---

## Prerequisites

- **Unity 2022.3.15f1 LTS** or newer
- **Git** (optional, for version control)
- **Build Tools** (for creating release builds)

---

## Setup (3 Steps)

### 1. Open Project
```bash
# Clone repository (if using Git)
git clone <repository-url>
cd protocol-emr

# Open in Unity Hub
# Click "Open" → Navigate to project folder → Select
```

### 2. Wait for Import
Unity will automatically:
- ✅ Import packages (URP, Input System)
- ✅ Compile scripts
- ✅ Generate meta files

**First import takes 2-5 minutes.**

### 3. Open Test Scene
1. Navigate to `Assets/Scenes/Main.unity`
2. Double-click to open
3. Press **Play** ▶️

---

## Controls

| Action | Key/Button |
|--------|------------|
| Move | WASD |
| Look | Mouse |
| Sprint | Shift |
| Crouch | Ctrl |
| Jump | Space |
| Interact | E |
| Copy Seed | F8 |
| Pause | ESC |
| Performance Monitor | F1 |

---

## Test Checklist

Press **Play** and verify:

- [ ] WASD moves player
- [ ] Mouse look rotates camera
- [ ] Shift sprints (drains stamina)
- [ ] Ctrl crouches (lowers camera)
- [ ] Space jumps
- [ ] ESC pauses (time stops, cursor appears)
- [ ] F1 shows FPS counter
- [ ] No console errors

---

## Troubleshooting

### Issue: Input not working
**Fix**: Edit → Project Settings → Player → Other Settings → Active Input Handling → Set to "Input System Package (New)"

### Issue: Scripts not compiling
**Fix**: Check Console (Ctrl+Shift+C) for errors, resolve any namespace issues

### Issue: Black screen
**Fix**: Ensure Main.unity scene is open, check if camera exists

### Issue: Player falls through floor
**Fix**: Verify Player has CharacterController component, Floor has collider

---

## Next Steps

1. ✅ Read [README.md](./README.md) for full feature overview
2. ✅ Follow [Scene Setup Guide](./Assets/Documentation/SceneSetupGuide.md) to create custom scenes
3. ✅ Review [Project Setup](./Assets/Documentation/ProjectSetup.md) for Unity configuration
4. ✅ Check [Build Roadmap](./docs/protocol-emr/build-coding-roadmap.md) for development plan

---

## Common Commands

### Enable Performance Monitor
```
Press F1 in Play mode
```

### Rebind Input
```csharp
InputManager.Instance.StartRebinding("Movement/Jump", 0, (success) => {
    Debug.Log($"Rebind {(success ? "successful" : "failed")}");
});
```

### Change Settings
```csharp
SettingsManager.Instance.SetMouseSensitivity(1.5f);
SettingsManager.Instance.SetFieldOfView(100f);
```

### Pause Game
```csharp
GameManager.Instance.SetPaused(true);
```

---

## File Locations

### Settings
```
Windows: C:/Users/[User]/AppData/LocalLow/[Company]/ProtocolEMR/
macOS: ~/Library/Application Support/[Company]/ProtocolEMR/
Linux: ~/.config/unity3d/[Company]/ProtocolEMR/
```

**Files**:
- `game_settings.json` - Graphics, audio, gameplay settings
- `input_bindings.json` - Custom key bindings

---

## Key Scripts

| Script | Location | Purpose |
|--------|----------|---------|
| InputManager | Core/Input/ | Handles all input |
| FirstPersonCamera | Core/Camera/ | Camera control |
| PlayerController | Core/Player/ | Player movement |
| SettingsManager | Core/Settings/ | Settings persistence |
| PerformanceMonitor | Core/Performance/ | FPS/memory tracking |
| GameManager | Core/ | System initialization |

---

## Creating Your First Scene

**Quick Setup** (5 minutes):

1. **Create Scene**: File → New Scene → Save as `TestScene.unity`

2. **Add GameManager**:
   - Create Empty GameObject → Rename "GameManager"
   - Add Component → GameManager

3. **Add Player**:
   - Create Capsule → Rename "Player"
   - Remove Capsule Collider
   - Add Component → Character Controller
   - Add Component → PlayerController
   - Set Layer: Player

4. **Add Camera**:
   - Right-click Player → Camera → Rename "PlayerCamera"
   - Position: (0, 0.6, 0)
   - Add Component → FirstPersonCamera
   - Drag Player to "Player Body" field
   - Tag as "MainCamera"

5. **Add Floor**:
   - Create Plane → Scale (2, 1, 2)
   - Set Layer: Ground

6. **Add Light**:
   - Create Directional Light
   - Rotation: (50, -30, 0)

7. **Press Play** ▶️

---

## Support

- **Documentation**: `Assets/Documentation/`
- **Console Logs**: Check Unity Console (Ctrl+Shift+C)
- **Performance**: Press F1 for real-time stats
- **Procedural Seeds**: Press F8 to copy current seed for reproducible testing

## Procedural Generation Quick Tips

- **Reproducible Testing**: Use F8 to copy seed, restart with same seed for identical world
- **Custom Seeds**: Set in Settings Manager for reproducible runs
- **Debug Info**: Current seed shown in Performance Monitor overlay
- **Save/Load**: Seeds automatically preserved with game saves

---

## Build & Run

### Quick Build
1. File → Build Settings
2. Add Open Scenes
3. Click "Build and Run"

### Automated Build Pipeline (Recommended)
1. **Protocol EMR** → **Build** → **Release Candidate** (for testing)
2. **Protocol EMR** → **Build** → **Gold Master** (for release)
3. Wait for automated build process
4. Find builds in `Builds/[version]/` folder

### Development Build
1. File → Build Settings
2. ✅ Enable "Development Build"
3. ✅ Enable "Script Debugging"
4. Click "Build and Run"

### Performance Testing
1. **Protocol EMR** → **Performance** → **Configure Quality Settings**
2. **Protocol EMR** → **Performance** → **Analyze Performance Hotspots**
3. Press **F9** in-game for automated performance validation
4. Check **Performance Monitor** (F1) for real-time metrics

---

## Release Checklist ✅

### Pre-Build Validation
- [ ] Performance targets met (60 FPS @ 1080p Medium)
- [ ] Memory usage <3.5 GB during testing
- [ ] No console errors/warnings
- [ ] All platforms build successfully
- [ ] Telemetry and crash logging functional

### Post-Build Testing
- [ ] Windows build launches and runs
- [ ] macOS build launches and runs
- [ ] Performance validation passes (60+ FPS)
- [ ] Input systems work correctly
- [ ] Save/load functionality works
- [ ] Procedural generation is deterministic

---

**Ready to release!** 🚀

Protocol EMR v1.0.0 - Build Pipeline Complete
