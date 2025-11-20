# Protocol EMR - Project Setup Guide

## Unity Version Requirements

**Minimum**: Unity 2022.3 LTS (Long Term Support)
**Recommended**: Unity 2022.3.15f1 or newer

Download from: https://unity.com/releases/lts

---

## Initial Setup

### 1. Create New Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select template: **3D (URP)** - Universal Render Pipeline
4. Project name: `ProtocolEMR`
5. Location: Choose appropriate directory
6. Click "Create Project"

### 2. Install Required Packages

Open **Window → Package Manager** and install:

#### Required Packages
- ✅ **Universal RP** (com.unity.render-pipelines.universal) - v14.0.8+
- ✅ **Input System** (com.unity.inputsystem) - v1.7.0+
- ✅ **TextMeshPro** (com.unity.textmeshpro) - v3.0.6+
- ✅ **Visual Scripting** (com.unity.visualscripting) - v1.9.0+

#### Optional (for later sprints)
- **Cinemachine** (com.unity.cinemachine) - Advanced camera control
- **ProBuilder** (com.unity.probuilder) - Level design
- **Timeline** (com.unity.timeline) - Cutscenes

### 3. Configure Project Settings

#### Player Settings (Edit → Project Settings → Player)
- **Company Name**: Your studio name
- **Product Name**: Protocol EMR
- **Default Icon**: [Set custom icon in Sprint 6]
- **Resolution**:
  - Default Screen Width: 1920
  - Default Screen Height: 1080
  - Fullscreen Mode: Fullscreen Window
  - Resizable Window: Enabled

#### Quality Settings (Edit → Project Settings → Quality)
Configure 4 quality presets:

**Low**:
- Pixel Light Count: 2
- Texture Quality: Half Res
- Anisotropic Textures: Disabled
- Anti Aliasing: Disabled
- Soft Particles: Disabled
- Shadows: Disable

**Medium**:
- Pixel Light Count: 4
- Texture Quality: Full Res
- Anisotropic Textures: Per Texture
- Anti Aliasing: 2x Multi Sampling
- Soft Particles: Enabled
- Shadows: Hard Shadows Only

**High**:
- Pixel Light Count: 8
- Texture Quality: Full Res
- Anisotropic Textures: Forced On
- Anti Aliasing: 4x Multi Sampling
- Soft Particles: Enabled
- Shadows: All

**Ultra**:
- Pixel Light Count: 16
- Texture Quality: Full Res
- Anisotropic Textures: Forced On
- Anti Aliasing: 8x Multi Sampling
- Soft Particles: Enabled
- Shadows: All
- Shadow Resolution: Very High

#### Physics Settings (Edit → Project Settings → Physics)
- **Default Solver Iterations**: 8
- **Default Solver Velocity Iterations**: 3
- **Bounce Threshold**: 2
- **Default Contact Offset**: 0.01
- **Sleep Threshold**: 0.005
- **Queries Hit Triggers**: Enabled
- **Enable Adaptive Force**: Disabled

**Layer Collision Matrix**:
```
Layer 8: Player
Layer 9: Interactable
Layer 10: Enemy
Layer 11: Projectile
Layer 12: Ground
```

Configure collisions:
- Player collides with: Ground, Interactable, Enemy
- Enemy collides with: Ground, Player, Projectile
- Projectile collides with: Ground, Enemy

#### Input System Settings (Edit → Project Settings → Input System Package)
- **Update Mode**: Dynamic Update
- **Compensate For Screen Orientation**: Disabled
- **Filter Noise On Current**: Enabled
- **Default Dead Zone**: 0.125
- **Default Button Press Point**: 0.5

#### Graphics Settings (Edit → Project Settings → Graphics)
- **Scriptable Render Pipeline**: UniversalRenderPipelineAsset
- **Transparency Sort Mode**: Orthographic
- **Camera-Relative Culling**: Enabled

#### URP Asset Configuration
1. Locate `UniversalRenderPipelineAsset` in Project (usually in Settings/)
2. Configure for high-quality visuals:
   - **Rendering → Renderer**: Forward+
   - **Quality → HDR**: Enabled
   - **Quality → MSAA**: 4x (for High preset)
   - **Quality → Render Scale**: 1.0
   - **Lighting → Main Light**: Per Pixel
   - **Lighting → Additional Lights**: Per Pixel
   - **Shadows → Max Distance**: 100
   - **Shadows → Cascade Count**: 4
   - **Post-processing → Enabled**: True

---

## Folder Structure Setup

### Standard Project Organization
```
Assets/
├── Scenes/
│   ├── Main.unity
│   ├── TestScenes/
│   └── Levels/
├── Scripts/
│   ├── Core/
│   │   ├── Input/
│   │   ├── Camera/
│   │   ├── Player/
│   │   ├── Settings/
│   │   └── Performance/
│   └── Systems/
├── Prefabs/
│   ├── Player/
│   ├── UI/
│   ├── NPCs/
│   └── Environment/
├── Materials/
│   ├── Characters/
│   ├── Props/
│   └── Environment/
├── Models/
│   ├── Characters/
│   ├── Props/
│   └── Weapons/
├── Textures/
├── Audio/
│   ├── SFX/
│   ├── Music/
│   └── Voice/
├── Animations/
│   ├── Controllers/
│   ├── Clips/
│   └── BlendTrees/
├── VFX/
│   ├── Particles/
│   └── Shaders/
├── UI/
│   ├── Sprites/
│   ├── Fonts/
│   └── Prefabs/
└── Documentation/
    ├── ProjectSetup.md (this file)
    ├── AssetSourcing.md
    └── CodeStandards.md
```

---

## Git Configuration

### .gitignore Setup
Already configured in project root with Unity-specific exclusions.

### Git LFS (Large File Storage)
Recommended for binary assets:

```bash
git lfs install
git lfs track "*.psd"
git lfs track "*.fbx"
git lfs track "*.png"
git lfs track "*.wav"
git lfs track "*.ogg"
```

### Branching Strategy
- **main**: Stable builds only
- **develop**: Active development
- **feature/[name]**: Feature branches
- **sprint/[number]**: Sprint-specific work

---

## Performance Baseline

### Target Specifications

**Minimum Requirements**:
- CPU: Intel i5-7500 / AMD Ryzen 5 1600
- GPU: NVIDIA GTX 1060 / AMD RX 580
- RAM: 8GB
- Storage: 10GB
- OS: Windows 10 64-bit

**Recommended Requirements**:
- CPU: Intel i7-9700K / AMD Ryzen 7 3700X
- GPU: NVIDIA RTX 2060 / AMD RX 5700
- RAM: 16GB
- Storage: 10GB SSD
- OS: Windows 10/11 64-bit

### Performance Targets (Sprint 1)
- **FPS**: 60+ at 1080p Medium settings
- **Memory**: <2GB for foundation
- **Load Time**: <3 seconds per scene
- **Input Latency**: <16ms (1 frame at 60fps)

---

## Testing Setup

### Test Scene Creation

1. Create `Assets/Scenes/TestScenes/Sprint1_Foundation.unity`
2. Add components:
   - Directional Light
   - Plane (10x10 scale) as floor
   - Player capsule with PlayerController
   - Camera with FirstPersonCamera
   - Empty GameObject with GameManager

3. Configure lighting:
   - Directional Light: Intensity 1.0, Color white
   - Environment Lighting: Skybox
   - Ambient Mode: Skybox

### Debug Tools

**Console Logging**:
- Enable "Collapse" for repeated messages
- Enable "Error Pause" for debugging
- Use Debug.Log() for info, Debug.LogWarning() for warnings

**Performance Monitoring**:
- Enable "Stats" in Game view
- Use F1 to toggle PerformanceMonitor
- Profile with Unity Profiler (Window → Analysis → Profiler)

---

## Code Standards

### Naming Conventions

**Scripts/Classes**: PascalCase
```csharp
public class PlayerController { }
```

**Methods**: PascalCase
```csharp
public void HandleMovement() { }
```

**Variables**: camelCase
```csharp
private float movementSpeed;
```

**Serialized Fields**: camelCase with [SerializeField]
```csharp
[SerializeField] private float walkSpeed = 5.0f;
```

**Constants**: UPPER_SNAKE_CASE
```csharp
private const float MAX_SPEED = 10.0f;
```

**Events**: PascalCase with "On" prefix
```csharp
public event Action OnJump;
```

### Code Organization

**Namespaces**:
```csharp
namespace ProtocolEMR.Core.Player
{
    // Player-related code
}
```

**Regions** (use sparingly):
```csharp
#region Unity Lifecycle
private void Awake() { }
private void Start() { }
#endregion
```

**Comments**:
- XML documentation for public APIs
- Inline comments for complex logic only
- No redundant comments

```csharp
/// <summary>
/// Handles player movement based on input.
/// </summary>
/// <param name="input">Normalized movement vector</param>
public void Move(Vector2 input)
{
    // Complex calculation requires explanation
    float adjustedSpeed = baseSpeed * (1 + staminaMultiplier * 0.5f);
}
```

---

## Troubleshooting

### Common Issues

**Input System not working**:
- Ensure "Active Input Handling" is set to "Both" or "Input System Package (New)"
- Location: Edit → Project Settings → Player → Other Settings

**URP not rendering correctly**:
- Check Graphics settings has URP asset assigned
- Verify camera has URP Forward Renderer assigned
- Regenerate lighting (Window → Rendering → Lighting)

**Scripts not compiling**:
- Check Console for errors
- Ensure all namespaces are correct
- Verify Unity version compatibility

**Performance issues**:
- Disable MSAA in URP settings
- Reduce shadow distance
- Lower quality preset
- Check Profiler for bottlenecks

---

## Build Settings

### Platform Configuration

**PC, Mac & Linux Standalone**:
1. File → Build Settings
2. Select "PC, Mac & Linux Standalone"
3. Target Platform: Windows
4. Architecture: x86_64
5. Compression Method: LZ4 (fast) or LZ4HC (smaller)
6. Development Build: Enabled (for testing)

### Build Optimization
- Strip Engine Code: Enabled (in Player Settings)
- Managed Stripping Level: Low (for release)
- Script Optimization: Speed (for release)

---

## Next Steps

After completing project setup:
1. ✅ Test input system (WASD, mouse look, ESC pause)
2. ✅ Verify FPS counter displays (F1)
3. ✅ Check settings persistence (change sensitivity, restart game)
4. ✅ Test input rebinding (documented in InputManager)
5. → Move to **Sprint 2**: Locomotion and animations

---

## References

- **Unity Manual**: https://docs.unity3d.com/Manual/index.html
- **URP Documentation**: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest
- **Input System Guide**: https://docs.unity3d.com/Packages/com.unity.inputsystem@latest
- **Protocol EMR Build Roadmap**: `/docs/protocol-emr/build-coding-roadmap.md`

---

**Last Updated**: Sprint 1 Foundation Phase
**Document Owner**: Protocol EMR Development Team
