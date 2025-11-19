# Performance & Debug Tools Protocol

This document defines the comprehensive in-game performance monitoring, debug utilities, and development tools for Protocol EMR. These systems provide real-time visibility into game performance, visual debugging capabilities, and developer console access for rapid iteration and troubleshooting during development and testing.

## Table of Contents

1. [FPS & Performance Counter](#fps--performance-counter)
2. [Advanced Profiling](#advanced-profiling)
3. [Debug Visualization](#debug-visualization)
4. [Developer Console](#developer-console)
5. [Quality Settings Benchmark](#quality-settings-benchmark)
6. [Bug Reporting Tool](#bug-reporting-tool)
7. [Gameplay Debug Tools](#gameplay-debug-tools)
8. [Performance Targets](#performance-targets)
9. [Optimization Settings](#optimization-settings)
10. [Logging & Analytics](#logging--analytics-optional)
11. [Technical Requirements](#technical-requirements)
12. [Integration Points](#integration-points)
13. [Prototype Deliverables](#prototype-deliverables)
14. [QA and Testing Checklist](#qa-and-testing-checklist)

## FPS & Performance Counter

### Overview

The FPS and Performance Counter provides real-time statistics about game performance, rendering efficiency, and system resource utilization. This overlay is essential for developers and QA to identify performance bottlenecks and verify optimization effectiveness.

### Performance Counter Display

#### Toggle Control
- **Hotkey**: F1 (configurable in settings)
- **Visibility States**: Hidden (default), Visible, Compact
- **Corner Placement**: Top-left, top-right, bottom-left, bottom-right (user-configurable)
- **Display Opacity**: 80% (configurable, prevents blocking gameplay)
- **Background**: Semi-transparent dark panel with subtle border

#### Display Information

| Metric | Description | Unit | Sampling Method |
|--------|-------------|------|-----------------|
| **Current FPS** | Frames rendered in current frame | FPS | Instantaneous 1/deltaTime |
| **Average FPS** | Rolling average over last 60 frames | FPS | 60-frame moving average |
| **Min FPS** | Lowest frame rate in last 60 seconds | FPS | Rolling minimum over 60-second window |
| **Max FPS** | Highest frame rate in last 60 seconds | FPS | Rolling maximum over 60-second window |
| **Frame Time** | Time to render current frame | ms | deltaTime * 1000 |
| **CPU Load** | Estimated CPU usage percentage | % | Based on frame time vs target |
| **GPU Load** | Estimated GPU usage percentage | % | Via graphics API queries (DirectX/Vulkan) |
| **Memory Usage** | Total RAM used by game process | MB | Process working set size |
| **Player Position** | World coordinates of camera | X, Y, Z | Current camera transform |
| **Frame Rate Target** | Target FPS setting | FPS | Quality setting derived value |

#### Display Format

```
═══════════════════════════════
  FPS & PERFORMANCE COUNTER
═══════════════════════════════
  Current FPS:  120
  Average FPS:  118
  Min/Max FPS:  95 / 144
  
  Frame Time:   8.33 ms
  CPU Load:     65%
  GPU Load:     72%
  
  Memory:       2,340 MB
  
  Position:
    X: 125.43
    Y: 45.67
    Z: 89.21
═══════════════════════════════
```

#### Performance Indicators

Color-coded thresholds for visual quick-reference:
- **Green**: Performance within target (60+ FPS)
- **Yellow**: Performance acceptable but not optimal (45-59 FPS)
- **Orange**: Performance degraded (30-44 FPS)
- **Red**: Performance critical (<30 FPS)

### Implementation Details

#### Data Collection
- **Sampling Window**: 60 frames for rolling averages
- **History Buffer**: Circular buffer maintaining last 60 frame times
- **Sample Rate**: Every frame for deltaTime, per-frame update for CPU/GPU
- **GPU Query Timing**: Staggered over multiple frames to avoid stalls

#### Memory Tracking
- **Process Working Set**: Platform-specific memory reporting
  - Windows: WMI or performance counters
  - macOS: Mach kernel APIs
  - Linux: /proc/self/status
- **Reported Memory**: Total RAM consumed by game process

#### CPU/GPU Load Estimation
- **CPU Load**: Calculated as `(frameTime / targetFrameTime) * 100`
- **GPU Load**: Query GPU utilization via graphics API counters
  - DirectX 12: Built-in performance counters
  - Vulkan: vkGetDeviceQueue performance queries
  - Fallback: Estimate based on draw calls and geometry

### UI Integration

#### Positioning System
- **Anchor Point**: Dynamically positioned based on corner selection
- **Padding**: 16px from screen edges
- **Responsive Scaling**: Adjusts text size for different resolutions (1080p, 1440p, 4K)
- **Safe Area Consideration**: Respects safe area margins on different platforms

#### Keyboard Controls
- **Toggle Visibility**: F1 key
- **Cycle Positions**: Shift+F1 cycles through corner positions
- **Compact Mode**: Ctrl+F1 toggles between full and compact display

## Advanced Profiling

### Overview

Advanced Profiling provides detailed breakdown of per-frame performance, identifying exactly where processing time is spent and which resources are consuming the most VRAM.

### Frame Breakdown Analysis

#### Performance Metrics

| Component | Description | Display | Unit |
|-----------|-------------|---------|------|
| **Rendering Time** | Time spent in graphics API calls and draw submission | Milliseconds | ms |
| **Physics Simulation** | Time spent updating physics, collisions, and rigidbodies | Milliseconds | ms |
| **AI Update** | Time for NPC behavior trees, pathfinding, perception | Milliseconds | ms |
| **Audio Processing** | Time for audio mixing, effects, spatial processing | Milliseconds | ms |
| **Animation** | Time for skeletal animation blending and updates | Milliseconds | ms |
| **Scripting** | Time for C# MonoBehaviour updates and coroutines | Milliseconds | ms |
| **Particle Systems** | Time for particle simulation and rendering | Milliseconds | ms |
| **UI Rendering** | Time for UI layout, rendering, and event processing | Milliseconds | ms |
| **Other** | Remaining frame time (memory management, etc.) | Milliseconds | ms |

#### Detailed Profiling Display

Accessible via F2 key, shows hierarchical breakdown:

```
╔═ DETAILED FRAME PROFILING ═╗
║ Total Frame Time: 8.33 ms   ║
║                             ║
║ ┌─ Rendering        6.12 ms ║  73.4%
║ │ ├─ Cull & Setup    1.50 ms ║
║ │ ├─ Shadows         1.23 ms ║
║ │ ├─ G-Buffer Pass   2.10 ms ║
║ │ └─ Post-Process    1.29 ms ║
║ │                           ║
║ ├─ Physics         0.89 ms  ║  10.7%
║ ├─ AI & NPCs       0.52 ms  ║  6.2%
║ ├─ Animation       0.35 ms  ║  4.2%
║ ├─ Audio           0.28 ms  ║  3.4%
║ ├─ Particles       0.12 ms  ║  1.4%
║ └─ UI              0.05 ms  ║  0.6%
╚═════════════════════════════╝
```

### GPU Memory Analysis

#### Memory Breakdown

| Resource Type | Description | Displayed Value |
|---------------|-------------|-----------------|
| **Texture Memory** | All loaded textures (diffuse, normal, PBR maps) | MB used / MB pool |
| **Mesh Memory** | Vertex buffers, index buffers for all geometry | MB used / MB pool |
| **Material Memory** | Shader parameters, material instances | MB used |
| **Shadow Maps** | Depth textures for dynamic and cascaded shadows | MB used |
| **Render Targets** | Intermediate render targets for effects | MB used |
| **Shader Cache** | Compiled shader variants in memory | MB used |
| **Other VRAM** | Remaining GPU memory allocations | MB used |

#### GPU Memory Display (F3)

```
╔═══ GPU MEMORY ANALYSIS ═══╗
║ Total GPU Memory: 1,842 MB ║
║ Available: 158 MB          ║
║                            ║
║ Textures:      1,024 MB    ║
║ Meshes:          512 MB    ║
║ Render Targets:  128 MB    ║
║ Shadow Maps:      96 MB    ║
║ Materials:        48 MB    ║
║ Shaders:          24 MB    ║
║ Other:            10 MB    ║
╚════════════════════════════╝
```

### Geometry Statistics

#### On-Screen Metrics

| Metric | Description | Purpose |
|--------|-------------|---------|
| **Vertex Count** | Total vertices currently being rendered | Identify geometry heavy scenes |
| **Triangle Count** | Total triangles on screen | Detect over-geometry conditions |
| **Draw Calls** | Number of draw submissions per frame | Identify state change overhead |
| **Batches** | Number of GPU batches (after instancing) | Verify batching effectiveness |
| **Material Slots** | Unique material combinations rendered | Check redundant state changes |
| **Active Lights** | Dynamic lights affecting screen | Monitor light performance cost |

#### Geometry Display (F4)

```
╔═══ GEOMETRY STATISTICS ═══╗
║ Vertices On Screen: 2.4M   ║
║ Triangles: 1.2M            ║
║ Draw Calls: 287            ║
║ GPU Batches: 156           ║
║ Material Switches: 89      ║
║ Active Lights: 12          ║
║                            ║
║ Batch Efficiency: 54.4%    ║
║ (Batches/DrawCalls)        ║
╚════════════════════════════╝
```

### Shader Compilation Monitoring

#### Compile Warnings Display (F5)

- **Shader Warnings**: List of all active shader warnings
- **Compilation Time**: Time spent compiling shaders in current session
- **Variant Count**: Number of shader variants compiled
- **Hot-Reload Support**: Monitors shader file changes and recompilation

```
╔═════ SHADER STATUS ═════╗
║ Compilation Warnings:   ║
║                         ║
║ ⚠ PBR_Diffuse (var 24)  ║
║   Unused uniform:       ║
║   _DetailNormalScale    ║
║                         ║
║ ⚠ Particles (var 8)     ║
║   High complexity:      ║
║   Consider optimization ║
║                         ║
║ Total Variants: 487     ║
║ Compile Time: 1.23s     ║
╚═════════════════════════╝
```

## Debug Visualization

### Overview

Debug visualization tools allow developers to visualize game world structures, invisible systems, and spatial relationships. These overlays help identify design issues, optimize spatial queries, and validate AI behavior.

### Wireframe Mode

#### Toggle Control
- **Hotkey**: F6 (toggle wireframe render mode)
- **Display Mode**: Solid wireframe with slight transparency to underlying shading
- **Performance Impact**: Moderate (disables most optimizations)

#### Visualization Features
- **Mesh Structure**: Shows all mesh faces with vertex connectivity
- **Hidden Geometry**: Reveals collision meshes, trigger volumes, occluders
- **Deformation**: Visualizes skinning, morphs, and vertex animation
- **UV Boundaries**: Option to highlight UV seams and islands

### NavMesh Visualization

#### Toggle Control
- **Hotkey**: F7 (toggle NavMesh overlay)
- **Display Mode**: Transparent overlay on game world
- **Color Coding**: Regions color-coded by type and walkability

#### NavMesh Display Features

| Element | Display | Color |
|---------|---------|-------|
| **Walkable Area** | Triangle mesh coverage | Green with white edges |
| **Non-Walkable Area** | Obstacles and off-mesh areas | Red |
| **Cliff Edges** | Steep slopes exceeding walkable gradient | Orange |
| **Poly Boundaries** | NavMesh polygon edges | White lines |
| **Agent Radius** | Circle showing agent collision radius | Yellow dashed |

#### Pathfinding Debug
- **Path Visualization**: Current NPC path displayed as connected waypoints
- **Goal Point**: Target destination marked with marker
- **Search Radius**: Shows query radius for pathfinding searches
- **Obstacle Avoidance**: Visualizes local steering around dynamic obstacles

### Physics Colliders Display

#### Toggle Control
- **Hotkey**: F8 (toggle physics visualization)
- **Display Mode**: Wireframe collider geometry over game objects

#### Collider Visualization Features

| Collider Type | Display | Color |
|---------------|---------|-------|
| **Box Colliders** | Wireframe boxes | Blue |
| **Sphere Colliders** | Wireframe spheres | Green |
| **Capsule Colliders** | Wireframe capsules | Cyan |
| **Mesh Colliders** | Triangle mesh geometry | Yellow |
| **Triggers** | Translucent volume overlay | Purple |
| **Kinematic Bodies** | Dashed wireframe | Magenta |

#### Collision State Indicators
- **Active Collisions**: Red highlight for colliders currently colliding
- **Sleeping Bodies**: Dimmed appearance for inactive rigidbodies
- **Constraints**: Red lines connecting constrained bodies

### Light Volume Visualization

#### Toggle Control
- **Hotkey**: Ctrl+F7 (toggle light volume visualization)
- **Display Mode**: Semi-transparent geometric representation of light falloff

#### Light Visualization Features

| Light Type | Visualization | Color |
|------------|---------------|-------|
| **Point Lights** | Sphere with radius | Yellow |
| **Directional Lights** | Pyramid frustum | White |
| **Spot Lights** | Cone geometry | Orange |
| **Area Lights** | Rectangular plane | Green |
| **Volumetric Lights** | Ray-marched volume | Cyan |

#### Light Debug Information
- **Active Lights**: Count of lights currently affecting scene
- **Light Cascades**: Shadow cascade boundaries for directional lights
- **Light Intensity**: Display as brightness of visualization
- **Shadow Coverage**: Show shadow frustum boundaries

### Particle System Bounds

#### Toggle Control
- **Hotkey**: Ctrl+F8 (toggle particle visualization)
- **Display Mode**: Bounding boxes for all particle systems

#### Particle Visualization Features
- **Bounds Display**: Wireframe AABB for particle simulation
- **Emission Visualization**: Gizmo showing particle emission direction
- **Lifetime Indicators**: Color gradient showing particle age
- **Velocity Vectors**: Arrow indicators for particle velocity

### Audio Source Visualization

#### Toggle Control
- **Hotkey**: Ctrl+F6 (toggle audio source visualization)
- **Display Mode**: 3D markers at audio source locations

#### Audio Visualization Features

| Element | Display | Details |
|---------|---------|---------|
| **Sound Sources** | Sphere at source location | Color indicates attenuation |
| **Falloff Radius** | Wireframe sphere showing max distance | Orange dashed line |
| **Attenuation Curve** | Visual representation of volume falloff | Adjustable shape |
| **Priority Indicators** | Label showing source priority | Importance level |
| **3D Pan** | Direction indicators in 3D space | Stereo positioning |

#### 3D Sound Positioning
- **Listener Position**: Marked with special symbol
- **Sound Direction**: Arrow from listener toward source
- **Doppler Visualization**: Frequency shift visualization for moving sources
- **Distance Attenuation**: Sphere size indicates attenuation distance

## Developer Console

### Overview

The Developer Console provides command-line interface for rapid iteration, debugging, and developer access to game systems without UI interaction. The console is designed for keyboard efficiency and supports both direct commands and variable monitoring.

### Console Access

#### Activation
- **Hotkey**: Tilde (~) key (toggle open/close)
- **Display Style**: Full-height overlay on left 40% of screen or bottom 25% based on preference
- **Input Focus**: Captures all keyboard input when open
- **Persistence**: Remains open until manually closed or on game pause

#### Console UI

```
┌─────────────────────────────────────────────────────────────┐
│ DEVELOPER CONSOLE                                    [×] [−] │
├─────────────────────────────────────────────────────────────┤
│ [SYSTEM] Loaded scene: Zone_01_Facility                     │
│ [SCRIPT] Player spawned at (45.23, 1.5, 123.67)             │
│ [PHYSICS] Collision between Player and Door_Main            │
│ [AUDIO] Initialized FMOD Studio (v2.02)                     │
│ [AI] NavMesh built: 45 polygons, 12 off-mesh links          │
│ [RENDER] Compiled 156 shader variants                       │
│                                                              │
│ > _                                                         │
│                                                              │
├─────────────────────────────────────────────────────────────┤
│ [Commands] [Variables] [All Output]                         │
└─────────────────────────────────────────────────────────────┘
```

### Command System

#### Command Structure
- **Format**: `commandName [argument1] [argument2] ...`
- **Case Insensitive**: Commands are case-insensitive
- **Argument Parsing**: Quoted strings, numeric values, boolean flags
- **History**: Up/Down arrow keys navigate command history
- **Autocomplete**: Tab key provides command and variable name completion

#### Core Commands

| Command | Arguments | Description |
|---------|-----------|-------------|
| **help** | [command] | Display all commands or help for specific command |
| **clear** | - | Clear console output |
| **save** | [filename] | Export console output to file |
| **echo** | [text] | Print text to console |
| **sv_gravity** | [value] | Set gravity value (m/s²), default 9.81 |
| **timescale** | [value] | Set time scale (0.5 = slow-mo, 2.0 = fast) |
| **noclip** | - | Toggle no-clip flight mode (free camera movement) |
| **god** | - | Toggle invulnerability mode |
| **kill** | - | Instantly kill player |
| **respawn** | - | Respawn player at last checkpoint |
| **teleport** | [x] [y] [z] | Teleport player to coordinates |
| **tp_npc** | [npc_name] [x] [y] [z] | Teleport NPC to location |
| **spawn_item** | [item_id] [quantity] | Spawn item in inventory |
| **spawn_npc** | [npc_id] [x] [y] [z] | Spawn NPC at location |
| **skip_cutscene** | - | Skip current cinematic/animation |
| **skip_tutorial** | - | Skip tutorial sequence |
| **unlock_mission** | [mission_id] | Unlock mission for completion |
| **complete_mission** | [mission_id] | Mark mission as complete |
| **set_weather** | [weather_type] | Set current weather state |
| **set_time** | [hour] [minute] | Set game world time |
| **load_scene** | [scene_name] | Load specific scene by name |
| **screenshot** | [filename] | Capture screenshot to file |
| **profile_frame** | [frames] | Profile and record frame performance data |
| **dump_state** | [filename] | Export current game state to JSON |

#### Cheat Code Registry

Optional cheat commands for testing:

| Cheat Code | Effect | Availability |
|-----------|--------|--------------|
| **GODMODE** | Toggle god mode | Development & QA builds |
| **NOCLIP** | Toggle noclip mode | Development & QA builds |
| **MOONWALK** | Reverse gravity effect | Optional fun mode |
| **SPEEDRUN** | 3x player speed temporarily | Optional challenge mode |
| **SLOWMO** | Set timescale to 0.3 | Optional testing mode |

### Output Logging

#### Log Channels

| Channel | Color | Description |
|---------|-------|-------------|
| **[SYSTEM]** | White | Engine and framework messages |
| **[SCRIPT]** | Green | C# script execution and events |
| **[PHYSICS]** | Blue | Physics simulation and collisions |
| **[AUDIO]** | Yellow | Audio system and events |
| **[AI]** | Cyan | NPC behavior and pathfinding |
| **[RENDER]** | Magenta | Graphics and rendering |
| **[UI]** | Orange | UI system events |
| **[ERROR]** | Red | Error messages |
| **[WARNING]** | Orange | Warning messages |

#### Log Filtering
- **Channel Filter**: Click tabs to show/hide specific channels
- **Keyword Search**: Filter logs by text pattern
- **Error Highlight**: Automatic expansion and highlighting of error messages
- **Verbosity Levels**: Debug, Info, Warning, Error

### Variable Monitoring

#### Variable Display Panel

```
┌─── VARIABLES ─────────────────────┐
│ sv_gravity:              9.81      │
│ timescale:               1.0       │
│ player_health:           100       │
│ player_position:         (45.2...)  │
│ active_npcs:             7         │
│ draw_calls:              287       │
│ fps:                     120       │
└───────────────────────────────────┘
```

#### Variable Commands

| Command | Arguments | Description |
|---------|-----------|-------------|
| **set** | [var_name] [value] | Set variable value |
| **get** | [var_name] | Get variable value |
| **list_vars** | - | List all available variables |
| **watch** | [var_name] | Add variable to watch panel |
| **unwatch** | [var_name] | Remove variable from watch panel |

#### Monitored Variables

| Variable | Type | Range | Default |
|----------|------|-------|---------|
| **sv_gravity** | Float | 0.0 - 50.0 | 9.81 |
| **timescale** | Float | 0.1 - 2.0 | 1.0 |
| **player_health** | Int | 0 - 100 | 100 |
| **god_mode** | Bool | - | false |
| **noclip_mode** | Bool | - | false |
| **draw_calls** | Int | Read-only | - |
| **vertex_count** | Int | Read-only | - |
| **active_npcs** | Int | Read-only | - |
| **active_lights** | Int | Read-only | - |

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **~** | Toggle console open/close |
| **Enter** | Execute command |
| **Shift+Enter** | New line in multi-line input |
| **Up/Down Arrow** | Navigate command history |
| **Tab** | Autocomplete |
| **Ctrl+A** | Select all |
| **Ctrl+C** | Copy console output |
| **Ctrl+L** | Clear console |
| **Esc** | Close console and return focus to game |

## Quality Settings Benchmark

### Overview

The Quality Settings Benchmark provides automatic performance testing across different quality levels, resolutions, and hardware configurations. The benchmark helps identify optimal settings and validates performance targets.

### Benchmark Modes

#### Quick Benchmark (60 seconds)
- **Purpose**: Fast performance check
- **Duration**: 1 minute of profiling
- **Test Sequence**: Static scene with varying camera positions
- **Output**: Average, min, max FPS and frame times

#### Full Benchmark (300 seconds)
- **Purpose**: Comprehensive performance analysis
- **Duration**: 5 minutes covering all major systems
- **Test Sequence**: 
  1. Static geometry test (60s)
  2. NPC interaction test (60s)
  3. Particle-heavy effects test (60s)
  4. Physics-heavy test (60s)
  5. Complex audio scene test (60s)

#### Custom Benchmark
- **User-Defined**: Select specific test elements and duration
- **Scene Selection**: Choose from predefined benchmark scenes
- **Quality Override**: Force specific quality settings
- **Iteration Count**: Run benchmark multiple times for averaging

### Benchmark Execution

#### Activation
- **Hotkey**: Ctrl+F9 (launch benchmark dialog)
- **Console Command**: `benchmark [quick|full|custom]`
- **Menu Access**: Settings > Performance > Run Benchmark

#### Benchmark UI

```
╔═══ QUALITY BENCHMARK ═══╗
║                         ║
║ Running Benchmark...    ║
║ [████████░░░░] 65%     ║
║                         ║
║ Elapsed: 3m 54s         ║
║ Remaining: 2m 06s       ║
║                         ║
║ Current Test:           ║
║ Physics-Heavy Scene     ║
║                         ║
║ [CANCEL] [PAUSE]        ║
╚═════════════════════════╝
```

### Performance Target Validation

#### Test Scenarios

| Scenario | Duration | Goal | Critical Metric |
|----------|----------|------|-----------------|
| **Idle Scene** | 60s | Measure baseline | Minimum FPS |
| **Exploration** | 60s | Typical gameplay | Average FPS |
| **Combat** | 60s | Action sequences | Min FPS (no stutters) |
| **Heavy VFX** | 60s | Special effects | Frame stability |
| **Multiple NPCs** | 60s | Crowded scenes | AI overhead |

### Results Report

#### Benchmark Report Display

```
╔════ BENCHMARK RESULTS ════╗
║                           ║
║ QUALITY SETTINGS:         ║
║ ├─ Resolution: 1920x1080  ║
║ ├─ Quality Preset: High   ║
║ ├─ Shadows: Ultra         ║
║ ├─ Reflections: On        ║
║ └─ Particles: High        ║
║                           ║
║ PERFORMANCE METRICS:      ║
║ ├─ Average FPS: 118       ║
║ ├─ Min FPS: 95            ║
║ ├─ Max FPS: 144           ║
║ ├─ Avg Frame Time: 8.47ms ║
║ └─ Stutter Events: 2      ║
║                           ║
║ SYSTEM INFO:              ║
║ ├─ GPU: RTX 3080          ║
║ ├─ CPU: Ryzen 5900X       ║
║ ├─ RAM: 32GB              ║
║ └─ VRAM: 10GB             ║
║                           ║
║ RECOMMENDATION:           ║
║ ✓ Can run Ultra settings  ║
║ ✓ 144 FPS achievable      ║
║ → Recommended: Very High  ║
║                           ║
║ [SAVE REPORT] [OK]        ║
╚═══════════════════════════╝
```

#### Multi-Resolution Testing

| Resolution | Quality | Target FPS | Result | Recommendation |
|------------|---------|------------|--------|-----------------|
| **2560×1440** | Ultra | 60 | 62 FPS | ✓ Excellent |
| **2560×1440** | High | 144 | 140 FPS | ✓ Excellent |
| **1920×1080** | Ultra | 60 | 118 FPS | ✓ Excellent |
| **1920×1080** | High | 144 | 144 FPS | ✓ Excellent |
| **1920×1080** | Medium | 60 | 180 FPS | ✓ Exceeds target |

### Performance Graphs

#### 60-Second Graph Display

```
FPS Over Time (60 sec test)
144├─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─┤
    │  ╱────╲ ╱────╲ ╱────╲   │
120 ├─╱      ╲╱      ╲╱      ╲─┤ AVG: 118
    │                         │
100 ├─────────────────────────┤ MIN: 95
    │╱────╲ ╱────╲ ╱────╲    │
    └─────────────────────────┘
    0s         30s         60s
```

#### Frame Time Distribution

```
Frame Time Distribution
  Count
    500├────┐
       │    │┌──┐
    400├────┤│  │┌──┐
       │    ││  ││  │
    300├────┤│  ││  │┌──┐
       │    ││  ││  ││  │
    200├────┤│  ││  ││  │
       │    ││  ││  ││  │
    100├────┤│  ││  ││  │┌──┐
       │    ││  ││  ││  ││  │
      0└────┴┴──┴┴──┴┴──┴┴──┴─
        8ms  9ms 10ms11ms12ms
```

### Report Export

#### Export Format
- **JSON Export**: Complete benchmark data with all frames
- **CSV Export**: Simplified metrics for spreadsheet analysis
- **HTML Report**: Interactive visualization with graphs
- **Screenshot**: Capture benchmark results for documentation

## Bug Reporting Tool

### Overview

The Bug Reporting Tool provides developers and QA with streamlined crash reporting, system information capture, and feedback submission capabilities. This tool automates data collection and ensures critical debugging information is preserved.

### Bug Report Access

#### Activation
- **Hotkey**: F12 (launch bug report dialog)
- **Menu Access**: Help > Report Bug
- **Automatic Trigger**: On crash detection (modal dialog)
- **Console Command**: `report_bug`

### Bug Report Dialog

#### UI Layout

```
╔════════ BUG REPORT DIALOG ════════╗
║                                   ║
║ Title: [____________________]     ║
║                                   ║
║ Category: [▼ Graphics Issue]      ║
║ Severity: [▼ High]                ║
║                                   ║
║ Description:                      ║
║ ┌──────────────────────────────┐  ║
║ │ [Describe the issue...]      │  ║
║ │                              │  ║
║ │                              │  ║
║ └──────────────────────────────┘  ║
║                                   ║
║ [📷 Screenshot] [🎥 Video Clip]   ║
║ [📊 Logs] [📋 Copy System Info]   ║
║                                   ║
║ □ Include System Info             ║
║ □ Include Last Save File          ║
║ □ Include Screenshots/Video       ║
║                                   ║
║ [SUBMIT] [CANCEL]                 ║
╚═══════════════════════════════════╝
```

#### Report Fields

| Field | Type | Purpose |
|-------|------|---------|
| **Title** | Text | Brief summary of issue |
| **Category** | Dropdown | Bug classification |
| **Severity** | Dropdown | Priority level |
| **Description** | Text Area | Detailed explanation |
| **Attachments** | File/Media | Screenshots, video, logs |
| **System Info** | Auto-filled | Hardware and software details |

### Category Classification

| Category | Examples |
|----------|----------|
| **Gameplay** | Mission progression, mechanics, balance |
| **Graphics** | Rendering glitches, texture issues, lighting bugs |
| **Audio** | Sound mixing, voice-over sync, spatial audio |
| **Physics** | Collision issues, clipping, ragdoll problems |
| **UI/UX** | Menu issues, display bugs, control problems |
| **Performance** | Framerate, stuttering, memory leaks |
| **Crash** | Application crashes, fatal errors |
| **Save/Load** | Data persistence, profile issues |

### Severity Levels

| Level | Description | Response Time |
|-------|-------------|----------------|
| **Critical** | Crash or gamebreaker | Immediate |
| **High** | Significant gameplay impact | Within 24 hours |
| **Medium** | Notable issue but workable | Within 48 hours |
| **Low** | Minor issue, low priority | Next sprint |

### Attachment System

#### Screenshot Capture
- **Hotkey During Report**: Click [📷 Screenshot] button
- **Capture Mechanism**: Full frame screenshot with UI overlay
- **Format**: PNG with metadata embedded
- **Multiple Snapshots**: Can attach up to 3 screenshots

#### Video Clip Recording
- **Duration**: Last 30 seconds of gameplay
- **Trigger**: Click [🎥 Video Clip] button at time of issue
- **Format**: MP4 H.264 codec
- **Size Limit**: Maximum 100MB per clip

#### Log Attachment
- **Auto-Include**: Console output and system logs
- **Filter Options**: Select which log channels to include
- **Manual Selection**: Browse and attach custom log files
- **Compressed**: ZIP archive if multiple files

### System Information Auto-Fill

#### Automatically Captured Data

| Information | Details |
|-------------|---------|
| **GPU Model** | GPU name and VRAM amount |
| **CPU Model** | CPU brand, cores, clock speed |
| **RAM** | Total system RAM, available RAM |
| **Resolution** | Current display resolution |
| **Current FPS** | Performance metric from counter |
| **Scene Name** | Current loaded scene/level |
| **OS** | Windows version, macOS version, Linux distro |
| **Driver Version** | GPU driver and chipset versions |
| **Engine Version** | Unity version and build number |
| **Build Type** | Development, QA, or Release |

#### Sample System Info Block

```
SYSTEM INFORMATION
──────────────────
GPU: NVIDIA RTX 3080 (10GB VRAM)
CPU: Intel Core i9-10900K @ 4.7GHz (8 cores)
RAM: 32GB (28GB available)
Resolution: 1920x1080
Display: 144Hz
OS: Windows 11 22H2
Driver: NVIDIA 535.104

GAME INFORMATION
────────────────
Scene: Zone_03_Containment
Current FPS: 120
Build: QA Build v0.8.2
Engine: Unity 2022.3.14f1
```

### Crash Log Submission

#### Automatic Crash Detection
- **On Crash**: Automatically launches bug report dialog
- **Pre-filled Fields**: Scene name, error message, system info
- **Stack Trace**: Full call stack included
- **Memory State**: Last known memory allocation state

#### Crash Report Content

```
CRASH REPORT
────────────
Exception Type: NullReferenceException
Scene: Zone_02_Laboratory
Player Position: (142.3, 5.2, 89.7)

Stack Trace:
  at NPCBehaviorTree.Update() line 245
  at AIManager.UpdateAI() line 523
  at GameManager.Update() line 1847

Recent Actions:
  - Entered zone 3 seconds ago
  - Interacted with NPC at 1 second ago
  - No input in last 0.5 seconds
```

### Report Submission

#### Submission Targets
- **Development Build**: Saves locally to `/Logs/BugReports/`
- **QA Build**: Sends to QA database via HTTPS POST
- **Release Build**: Sends to analytics backend with user consent
- **Offline Mode**: Queues reports for upload when connection restored

#### Submission Status

```
╔════ SUBMITTING BUG REPORT ════╗
║                              ║
║ Preparing data...      [✓]   ║
║ Compressing files...   [✓]   ║
║ Uploading (42%)        [▓▓▓] ║
║                              ║
║ Estimated: 3 seconds         ║
║                              ║
║ [BACKGROUND]                 ║
╚══════════════════════════════╝
```

## Gameplay Debug Tools

### Overview

Gameplay Debug Tools provide rapid testing capabilities for content, mechanics, and balancing. These tools accelerate development iteration by allowing quick access to game states and content testing without lengthy normal progression.

### Teleportation System

#### Player Teleportation
- **Command**: `teleport [x] [y] [z]`
- **Quick Teleport Hotkey**: Ctrl+T (opens dialog)
- **Predefined Locations**: List of named checkpoint locations
- **Cursor Teleport**: Shift+Click to teleport to clicked location

#### Teleport Dialog

```
╔═══ TELEPORT TO LOCATION ═══╗
║                             ║
║ Location: [___________  ▼]  ║
║                             ║
║ X: [________________]        ║
║ Y: [________________]        ║
║ Z: [________________]        ║
║                             ║
║ Recent Locations:            ║
║ • Zone_01_Entrance           ║
║ • Zone_02_Laboratory         ║
║ • Zone_03_Containment        ║
║ • Checkpoint_Alpha           ║
║                             ║
║ [TELEPORT] [CANCEL]         ║
╚═════════════════════════════╝
```

### NPC Spawning

#### NPC Spawn Command
- **Command**: `spawn_npc [npc_id] [x] [y] [z]`
- **Hotkey Dialog**: Ctrl+N (opens NPC spawn dialog)
- **Predefined Templates**: Select from available NPC types

#### NPC Spawn Dialog

```
╔═══════ SPAWN NPC ══════════╗
║                            ║
║ NPC Type: [▼ Guard]         ║
║                            ║
║ Behavior: [▼ Patrol]        ║
║                            ║
║ Location:                   ║
║ X: [_______]               ║
║ Y: [_______]               ║
║ Z: [_______]               ║
║                            ║
║ Count: [_____] (max 10)    ║
║                            ║
║ Options:                    ║
║ □ Aggressive               ║
║ □ Alerted                  ║
║ □ In Combat                ║
║                            ║
║ [SPAWN] [CANCEL]           ║
╚════════════════════════════╝
```

#### Available NPC Templates

| NPC Type | AI Behavior | Difficulty | Special Traits |
|----------|-------------|-----------|-----------------|
| **Guard** | Patrol + Alert | Standard | Weapons, armor |
| **Worker** | Routine tasks | None | Passive, communicative |
| **Security** | Combat trained | Advanced | Heavy weapons |
| **Scientist** | Investigate | None | Neutral, analytical |
| **Corrupted** | Hostile | Advanced | Unpredictable, fast |

### Item Spawning

#### Item Spawn Command
- **Command**: `spawn_item [item_id] [quantity]`
- **Hotkey Dialog**: Ctrl+I (opens item spawn dialog)
- **Categories**: Filter items by type

#### Item Spawn Dialog

```
╔═══════ SPAWN ITEMS ═════════╗
║                             ║
║ Item Category: [▼ All]      ║
║                             ║
║ Item: [▼ Health Pack]       ║
║                             ║
║ Quantity: [_____]           ║
║                             ║
║ Common Items:               ║
║ • Health Pack (Standard)    ║
║ • Energy Cells (50)         ║
║ • Key Card (Level 2)        ║
║ • Audio Log (Audio-01)      ║
║ • Research Notes (Page 7)   ║
║                             ║
║ [SPAWN] [CLEAR INVENTORY]   ║
║ [CANCEL]                    ║
╚─────────────────────────────╝
```

### Cinematic/Tutorial Skip

#### Skip Cinematics
- **Command**: `skip_cutscene`
- **Hotkey**: Shift+K (during cinematic playback)
- **Effect**: Immediately advances to next scene or objective

#### Skip Tutorial
- **Command**: `skip_tutorial`
- **Hotkey**: Ctrl+Shift+T
- **Effect**: Marks all tutorial objectives complete and unlocks core systems

#### Tutorial Skip Confirmation

```
╔═════ SKIP TUTORIAL? ═════╗
║                          ║
║ This will skip the       ║
║ complete tutorial        ║
║ sequence.                ║
║                          ║
║ All basic systems will   ║
║ become available.        ║
║                          ║
║ [SKIP] [CANCEL]         ║
╚══════════════════════════╝
```

### Mission Unlock/Complete

#### Mission Commands
- **Unlock Mission**: `unlock_mission [mission_id]`
- **Complete Mission**: `complete_mission [mission_id]`
- **Reset Mission**: `reset_mission [mission_id]`
- **List Missions**: `list_missions` (shows all with status)

#### Mission Manager Dialog

```
╔═══ MISSION MANAGER ═══╗
║                       ║
║ Filtering: [▼ Active] ║
║                       ║
║ ✓ Main: Escape Wing-A ║
║ ✗ Side: Collect Data  ║
║ ✓ Challenge: Speed    ║
║ ◆ Hidden: Truth       ║
║ ✕ Failed: Protocol    ║
║                       ║
║ [SELECT MISSION]      ║
║ [UNLOCK] [COMPLETE]   ║
║ [RESET] [CANCEL]      ║
╚═══════════════════════╝
```

### Time Scaling

#### Time Scale Control
- **Command**: `timescale [value]` (0.5 = slow-mo, 2.0 = fast)
- **Hotkey Presets**:
  - **Ctrl+[**: Set 0.5x (slow-motion)
  - **Ctrl+]**: Set 1.0x (normal)
  - **Ctrl+\\**: Set 2.0x (fast-forward)
- **Slider Control**: Shift+T opens time scale slider

#### Time Scale UI

```
╔═════ TIME SCALE ═════╗
║                      ║
║ 0.5x    1.0x    2.0x ║
║  ◊      ●      □     ║
║  |---|---|---|---|   ║
║ [────────●────────]  ║
║ Current: 1.0x        ║
║                      ║
║ Effects:             ║
║ ✓ Audio scaled       ║
║ ✓ Physics scaled     ║
║ ✓ Animations scaled  ║
║ ✓ Particle scaled    ║
╚══════════════════════╝
```

#### Time Scale Effects
- **Audio**: Pitch shifts with time scale
- **Physics**: Gravity and forces adjusted
- **Animations**: All animations play at scaled speed
- **Particles**: Emission and lifetime adjusted
- **UI**: Remains at normal speed for usability

## Performance Targets

### Target Specifications

The following performance targets guide optimization efforts and define success criteria for different hardware configurations:

#### Primary Target: Recommended Hardware

| Target | Specification | Rationale |
|--------|---------------|-----------|
| **Resolution** | 1920×1080 (1080p) | Industry standard for monitor refresh |
| **Quality Preset** | High | Balance between visual fidelity and performance |
| **Target FPS** | 60 | Smooth gameplay minimum, common monitor refresh |
| **Average Frame Time** | 16.67 ms | 1000ms / 60 FPS |
| **Max Frame Time** | 20.00 ms | Allow occasional frame budget overruns |
| **GPU Requirement** | RTX 2070 / RX Vega 56 equivalent | Mid-range gaming GPU (2020 era) |
| **CPU Requirement** | i5-8400 / Ryzen 5 2600 equivalent | Mid-range CPU (2018 era) |
| **RAM Requirement** | 8GB | Standard for gaming |
| **VRAM Budget** | <2GB at full settings | Fit within common VRAM allocations |

#### Secondary Target: High-End Hardware

| Target | Specification | Rationale |
|--------|---------------|-----------|
| **Resolution** | 2560×1440 (1440p) | High-resolution monitor support |
| **Quality Preset** | Ultra | Maximum visual settings |
| **Target FPS** | 144 | High refresh rate support |
| **GPU Requirement** | RTX 3070 / RX 6700XT equivalent | High-end gaming GPU (2021 era) |
| **VRAM Budget** | <3GB at full settings | Modern high-VRAM cards |

#### Minimum Target: Entry-Level Hardware

| Target | Specification | Rationale |
|--------|---------------|-----------|
| **Resolution** | 1920×1080 (1080p) | Maintain base resolution |
| **Quality Preset** | Low | Reduced visual features |
| **Target FPS** | 30 | Acceptable for narrative-focused game |
| **GPU Requirement** | GTX 960 / RX 470 equivalent | Entry-level GPU (2015 era) |
| **CPU Requirement** | i3-6100 / Ryzen 3 1200 equivalent | Entry-level CPU (2015 era) |
| **RAM Requirement** | 4GB | Minimum recommended |
| **VRAM Budget** | <1GB at minimal settings | Integrated graphics support |

### Memory Budgets

#### RAM Allocation Target

| Component | Budget | Notes |
|-----------|--------|-------|
| **Engine & Framework** | 200 MB | Unity core, rendering, physics |
| **Scene Assets** | 1500 MB | 3D models, textures, materials |
| **Audio** | 300 MB | Music streams, sound effects, voice |
| **UI** | 50 MB | Interface, fonts, panel assets |
| **Scripting & State** | 200 MB | Game logic, player data, NPC state |
| **Streaming/Buffers** | 400 MB | Asset streaming, network buffers |
| **Reserve** | 350 MB | Headroom for dynamic allocations |
| **Total Budget** | <4000 MB | 4GB maximum target |

#### VRAM Allocation Target (Max Settings)

| Component | Budget | Notes |
|-----------|--------|-------|
| **Textures** | 1024 MB | Diffuse, normal, PBR maps |
| **Meshes** | 512 MB | Vertex/index buffers |
| **Render Targets** | 128 MB | Post-process, UI rendering |
| **Shadow Maps** | 96 MB | Cascaded + dynamic shadows |
| **Materials** | 48 MB | Material instances, parameters |
| **Shader Cache** | 24 MB | Compiled shader variants |
| **Other** | 68 MB | Miscellaneous GPU resources |
| **Total Budget** | <2000 MB | 2GB maximum target |

### Load Time Targets

| Scenario | Target | Acceptance Criteria |
|----------|--------|-------------------| 
| **Initial Boot** | <30 seconds | From launcher to main menu |
| **New Game Start** | <5 seconds | From new game selection to gameplay |
| **Zone Transition** | <3 seconds | Between major areas |
| **Quick Load** | <2 seconds | Restore from save file |
| **Cinematic Load** | <2 seconds | Load cinematic sequence |

### Frame Stability Targets

| Metric | Target | Tolerance |
|--------|--------|-----------|
| **Frame Time Variance** | <1ms average deviation | Smooth, consistent motion |
| **Stutter Events** | <2 per minute at target FPS | Imperceptible hitches acceptable |
| **Dropped Frames** | <1% over benchmark duration | 99%+ of frames delivered on time |

## Optimization Settings

### Overview

Optimization settings provide granular control over rendering quality, detail levels, and performance-intensive features. These settings allow players and developers to fine-tune performance for their hardware.

### Lodging (Level of Detail)

#### LOD Distance Configuration

| LOD Level | Vertex Count | Display Distance | Use Case |
|-----------|-------------|------------------|----------|
| **LOD0** | 100% | <30m | Player proximity |
| **LOD1** | 50-75% | 30-60m | Medium distance |
| **LOD2** | 25-50% | 60-120m | Far distance |
| **LOD3** | 10-25% | 120-200m | Very far |
| **Billboard** | Quad | >200m | Skyline/distant |

#### LOD Debug Settings

```
╔═════ LOD CONFIGURATION ═════╗
║                             ║
║ LOD Distances:              ║
║ LOD0→1: [_____] m (30)      ║
║ LOD1→2: [_____] m (60)      ║
║ LOD2→3: [_____] m (120)     ║
║ LOD3→Billboard: [____] m    ║
║                             ║
║ Visualization:              ║
║ □ Color code by LOD         ║
║ □ Show LOD transition zones ║
║ □ Lock camera distance      ║
║                             ║
║ [APPLY] [RESET]             ║
╚─────────────────────────────╝
```

#### LOD Performance Impact
- **Adjusting LOD distances**: Immediate frame rate improvement
- **Measurement**: Run benchmark between settings to compare
- **Recommended Tuning**: Balance visual quality with target FPS

### Occlusion Culling

#### Occlusion Debug View
- **Hotkey**: Ctrl+O (toggle occlusion culling visualization)
- **Display Mode**: Color-coded visible/hidden geometry
  - **Green**: Visible geometry
  - **Red**: Occluded geometry (not rendered)
  - **Yellow**: Partial occlusion

#### Occlusion Configuration

| Parameter | Range | Impact | Default |
|-----------|-------|--------|---------|
| **Cell Size** | 1.0 - 10.0 | Finer = more accurate, slower | 5.0 |
| **Smallest Occluder** | 0.5 - 5.0 | Smaller = better culling, more memory | 1.5 |
| **Backface Threshold** | 0.0 - 180.0 | Lower = more aggressive | 100.0 |

### Texture Streaming

#### Texture Streaming Monitor

```
╔═════ TEXTURE STREAMING ═════╗
║ VRAM Usage: 1,824 MB / 2GB  ║
║ Streaming: 125 MB           ║
║ Streamed In: 847 textures   ║
║ Pending: 12 textures        ║
║                             ║
║ Streaming Bandwidth:        ║
║ ▓▓▓▓▓▓▓░░░░░░░░░░░░░░░    ║
║ 38% of available           ║
║                             ║
║ [📊 View Textures]         ║
║ [⚙️  Streaming Settings]   ║
╚─────────────────────────────╝
```

#### Streaming Configuration

| Parameter | Impact | Recommendation |
|-----------|--------|-----------------|
| **Pool Size** | Amount of VRAM reserved for streaming | Set to 80% of VRAM budget |
| **Fade-In Duration** | Time to blend streamed texture in | 0.5s default |
| **Priority Bias** | Prioritize nearby vs. high-resolution | 0.5 (balanced) |
| **Max Concurrent Loads** | Simultaneous streaming operations | 4 default |

### Particle Performance

#### Particle Count vs Performance

```
╔═══ PARTICLE OPTIMIZATION ═══╗
║                             ║
║ Max Active Particles:       ║
║ [────────●─────────] 10,000 ║
║                             ║
║ Particle Cost: ~4.2ms/frame ║
║                             ║
║ Quality Presets:            ║
║ • Low:    2,000 particles   ║
║ • Medium: 5,000 particles   ║
║ • High:   10,000 particles  ║
║ • Ultra:  20,000 particles  ║
║                             ║
║ Impact Analysis:            ║
║ FPS Impact: -8 per 5000     ║
║ Memory Impact: -50MB per    ║
║                             ║
║ [APPLY] [BENCHMARK]         ║
╚─────────────────────────────╝
```

#### Particle Optimization Strategies
- **Reduce Max Particles**: Lower overall budget per effect
- **Reduce Emission Rate**: Fewer particles per second
- **Shorter Lifetime**: Particles despawn faster
- **Lower Simulation Quality**: Less physics simulation per particle
- **Batching**: Combine similar particles to reduce draw calls

## Logging & Analytics (Optional)

### Overview

Logging and Analytics tracks player behavior, performance, and gameplay events for debugging and understanding player engagement. This optional system helps identify issues and balance problems.

### Player Action Tracking

#### Tracked Events

| Event Type | Data Captured | Use Case |
|-----------|--------------|----------|
| **Deaths** | Location, cause, damage source, health | Identify dangerous areas |
| **Puzzle Solutions** | Puzzle ID, time to solve, hints used | Balance difficulty |
| **NPC Encounters** | NPC type, distance, outcome | Validate AI behavior |
| **Item Pickups** | Item ID, location, frequency | Verify loot distribution |
| **Mission Progress** | Mission ID, status, time invested | Track progression difficulty |
| **Combat Engagement** | Enemy type, duration, damage dealt/taken | Validate combat balance |
| **UI Interactions** | Menu opened, buttons clicked, time spent | Improve interface usability |

#### Event Data Structure

```javascript
{
  eventType: "player_death",
  timestamp: "2024-01-15T14:23:45Z",
  playerPosition: { x: 142.3, y: 5.2, z: 89.7 },
  cause: "fall_damage",
  sourceIdentifier: "fall_from_platform_b2",
  sessionId: "session_12345",
  playerLevel: 8,
  currentMission: "mission_escape_wing_a"
}
```

### Heatmap Generation

#### Death Heatmaps
- **Visualization**: 3D heatmap overlay showing player deaths by location
- **Color Intensity**: Red = high death count, cool colors = low
- **Filtering**: Filter by death cause, time period, difficulty
- **Analysis**: Identify problematic areas requiring difficulty tuning

#### Struggle Heatmaps
- **Metrics**: Areas where players spend excessive time
- **Calculation**: Time spent per zone divided by zone size
- **Purpose**: Find confusing or frustrating regions
- **Output**: 3D visualization with statistical overlay

### Session Analytics

#### Session Metrics

| Metric | Description | Collection Method |
|--------|-------------|-------------------|
| **Session Duration** | Total time from login to logout | Timer start/stop |
| **Play Sessions** | Total count of play sessions | Session counter |
| **Average Session Length** | Mean duration across all sessions | Calculated from session data |
| **Session Intervals** | Time between sessions | Timestamp comparison |
| **Churn Rate** | Players not returning (no session in 7 days) | User status tracking |

#### Session Report

```
ANALYTICS REPORT - Week of Jan 15
════════════════════════════════
Total Players: 1,247
Active Sessions: 823
Avg Session: 47 minutes

New Players: 156
Retention (7d): 68%
Churn: 12%

Most Active Zones:
1. Zone_01_Facility: 12,340 minutes
2. Zone_02_Laboratory: 9,870 minutes
3. Zone_03_Containment: 7,650 minutes

Avg Time to Complete:
- Story: 142 minutes
- All Missions: 267 minutes
```

### Mission Completion Rates

#### Completion Tracking

| Mission | Attempts | Completions | Success Rate | Avg Time |
|---------|----------|-------------|--------------|----------|
| **Tutorial** | 1,247 | 1,189 | 95.3% | 8 min |
| **Main_01** | 1,189 | 1,156 | 97.2% | 12 min |
| **Main_02** | 1,156 | 1,045 | 90.4% | 18 min |
| **Side_01** | 1,045 | 823 | 78.7% | 22 min |
| **Challenge** | 456 | 201 | 44.1% | 35 min |

#### Difficulty Calibration
- **Success Rate Target**: 70-85% for normal missions
- **Challenge Missions**: 30-50% success rate expected
- **Completion Time**: Compare against target completion windows

### Data Export

#### Export Formats
- **CSV**: Spreadsheet-compatible data
- **JSON**: Full hierarchical data export
- **SQL**: Direct database import
- **Report**: Human-readable PDF summary

#### Export Command
- **Command**: `export_analytics [format] [date_range]`
- **Example**: `export_analytics json 2024-01-01:2024-01-31`
- **Output Location**: `/Analytics/Exports/`

## Technical Requirements

### Profiling API Integration

#### Performance Monitoring Framework
- **Use Unity Profiler API**: Access `Profiler.GetTotalMemory()`, frame time data
- **Graphics Debugging**: DirectX PIX, Vulkan layers for GPU profiling
- **Custom Instrumentation**: Add `[Profiler.BeginSample]` markers to code
- **Third-party Tools**: Support for external profilers (JetBrains Rider, Nvidia NSight)

#### Implementation Strategy
```csharp
// Example profiling integration
using Unity.Profiling;

CustomSampler s_PhysicsSampler = CustomSampler.Create("Physics.Update");

void UpdatePhysics() {
    using (s_PhysicsSampler.Auto())
    {
        // Physics code here
    }
}
```

### In-Game UI Framework

#### Console UI Implementation
- **Canvas System**: Render on World Space canvas or Screen Space overlay
- **Input Handling**: Intercept keyboard input without blocking gameplay
- **Text Rendering**: Support for monospace font for code appearance
- **Scrolling**: Efficient buffering for large console output

#### Performance Counter UI
- **Update Frequency**: Sample every frame but display every 0.1 seconds
- **Memory Efficiency**: Reuse UI text objects instead of recreating
- **Rendering**: Render on separate canvas layer above gameplay

### Console Scripting Engine

#### Command Processing
- **Parser**: Tokenize input, handle quoted strings and numbers
- **Dispatcher**: Route commands to handler functions
- **Reflection**: Use C# reflection to discover and call command methods
- **Variable Binding**: Bind console variables to game state properties

#### Implementation Strategy
```csharp
// Command registration pattern
public static void RegisterCommand(string name, Action<string[]> handler)
{
    commands[name.ToLower()] = handler;
}

// Variable binding pattern
private float sv_gravity = 9.81f;
public ConsoleVariable gravity = 
    new ConsoleVariable("sv_gravity", ref sv_gravity, "World gravity");
```

### Analytics Backend (Optional)

#### Data Storage
- **Local Mode**: JSON files in `%UserDocuments%/ProtocolEMR/Analytics/`
- **Server Mode**: HTTP POST to analytics service
- **Offline Queue**: Queue events when offline, sync when connected

#### Privacy Considerations
- **User Consent**: Request permission before enabling analytics
- **Data Anonymization**: Strip personally identifiable information
- **Opt-Out**: Provide option to disable analytics in settings
- **Data Retention**: Implement data deletion policies

## Integration Points

### Connection to Existing Systems

#### HUD Integration
- **Performance Counter**: Integrated into pause menu settings
- **Shortcut Display**: Show keybinds in options menu
- **Persistent Settings**: Save debug tool preferences to user settings

#### Save System Integration
- **Include Debug State**: Optionally save debug tool states with game saves
- **Restore on Load**: Reapply debug settings from save file

#### Input System Integration
- **Key Binding**: Support remapping debug hotkeys in controls settings
- **Conflict Resolution**: Prevent debug keys from conflicting with gameplay

#### Console Integration
- **Command History**: Persist command history across sessions
- **Autocompletion**: Reference command registry for autocomplete
- **Help System**: Link console commands to documentation

## Prototype Deliverables

### MVP Feature Set

**Phase 1 (Weeks 1-2):**
- [ ] FPS counter and basic performance metrics
- [ ] Frame time display
- [ ] Memory usage monitoring
- [ ] Camera position display
- [ ] Toggle UI with F1 key
- [ ] Position persistence

**Phase 2 (Weeks 3-4):**
- [ ] Advanced frame breakdown profiling
- [ ] GPU memory analysis
- [ ] Geometry statistics display
- [ ] Shader compilation warnings
- [ ] Data export to CSV

**Phase 3 (Weeks 5-6):**
- [ ] Developer console with basic commands
- [ ] Command history and autocomplete
- [ ] Variable watching system
- [ ] 10+ core commands implemented

**Phase 4 (Weeks 7-8):**
- [ ] Debug visualization (wireframe, NavMesh, colliders)
- [ ] Light volume visualization
- [ ] Audio source visualization
- [ ] Quality settings benchmark
- [ ] Performance graph display

**Phase 5 (Weeks 9-10):**
- [ ] Bug reporting tool
- [ ] Screenshot/video attachment
- [ ] Automatic system info capture
- [ ] Crash report submission
- [ ] Database storage

**Phase 6 (Weeks 11-12):**
- [ ] Gameplay debug tools (teleport, spawn, skip)
- [ ] Mission management console
- [ ] Time scaling system
- [ ] Item spawning
- [ ] Polish and optimization

### Acceptance Criteria

#### Performance Counter
- [ ] Display current, average, min, max FPS
- [ ] Show frame time in milliseconds
- [ ] Display CPU/GPU load percentages
- [ ] Show memory usage in MB
- [ ] Display player position coordinates
- [ ] Corner placement working correctly
- [ ] Toggle with F1 key
- [ ] Update every frame with smooth display
- [ ] No frame rate impact (< 0.5ms overhead)

#### Debug Visualization
- [ ] Wireframe mode toggles with F6
- [ ] NavMesh displays with proper coloring
- [ ] Physics colliders render correctly
- [ ] Light volumes visible with F7
- [ ] Audio sources display with F8
- [ ] Can toggle each visualization independently
- [ ] No performance degradation when disabled

#### Developer Console
- [ ] Console opens with ~ key
- [ ] Command execution working
- [ ] Command history navigation
- [ ] Tab autocomplete functional
- [ ] Variable monitoring panel
- [ ] Log filtering by channel
- [ ] 20+ commands implemented
- [ ] Help system functional

#### Quality Benchmark
- [ ] Benchmark runs for specified duration
- [ ] Performance metrics collected
- [ ] Results exported to file
- [ ] Recommendation system functional
- [ ] Multiple resolution testing
- [ ] Results graph displayed

## QA and Testing Checklist

### Functionality Testing

#### Performance Counter Tests
- [ ] FPS counter matches expected values
- [ ] Frame time accurate (verified against task manager)
- [ ] Memory tracking shows correct usage
- [ ] Position updates in real-time
- [ ] Toggle visibility with F1
- [ ] Cycle positions with Shift+F1
- [ ] Compact mode works (Ctrl+F1)
- [ ] Display persists across scenes
- [ ] Overlay doesn't interfere with gameplay

#### Console Tests
- [ ] Console opens/closes with ~ key
- [ ] Commands execute correctly
- [ ] Command history navigable
- [ ] Autocomplete suggestions appear
- [ ] Variable watching updates in real-time
- [ ] Log channels filter correctly
- [ ] Multi-line input with Shift+Enter
- [ ] Copy output to clipboard
- [ ] Console state resets on scene load

#### Debug Visualization Tests
- [ ] Wireframe mode toggles smoothly
- [ ] NavMesh displays with correct colors
- [ ] Colliders render for all types
- [ ] Light volumes update dynamically
- [ ] Audio sources show 3D positioning
- [ ] Visualizations toggle independently
- [ ] Performance impact acceptable when disabled
- [ ] Overlays update without lag

### Performance Testing

#### Overhead Measurement
- [ ] Performance counter adds < 0.5ms per frame
- [ ] Console open adds < 1.0ms per frame
- [ ] Each visualization adds < 2.0ms per frame
- [ ] Combined overhead < 10% of frame budget
- [ ] No memory leaks in debug tools
- [ ] No frame pacing issues

#### Load Testing
- [ ] Console handles 10,000+ log messages
- [ ] Performance counter stable over 30+ minutes
- [ ] Multiple visualizations enabled simultaneously
- [ ] Benchmark runs without crashes
- [ ] Export operations complete in reasonable time

### Compatibility Testing

#### Platform Testing
- [ ] All tools function on Windows 10/11
- [ ] Tested on macOS 12+
- [ ] Verified on Linux (Ubuntu 20.04+)
- [ ] Keyboard input handling consistent across platforms
- [ ] Display scaling appropriate for different resolutions

#### Input Testing
- [ ] All hotkeys work as documented
- [ ] Key remapping preserved
- [ ] Console input handling robust (special characters, unicode)
- [ ] Mouse input works in debug dialogs
- [ ] Gamepad support for menu navigation

### Edge Case Testing

- [ ] Console behavior when game paused
- [ ] Performance counter in cutscenes
- [ ] Debug tools in menu scenes
- [ ] Profiling during load operations
- [ ] Benchmark interrupt and resume
- [ ] Bug report submission with no internet connection
- [ ] Debug visualization with thousands of objects
- [ ] Console with invalid commands
- [ ] Extreme quality settings combinations

### Regression Testing

- [ ] Existing gameplay unaffected by debug tools
- [ ] Existing UI responsive with overlays active
- [ ] No impact on save/load systems
- [ ] Audio not disrupted by profiling
- [ ] Animation systems not affected
- [ ] Physics simulation unchanged

---

**Document Version**: 1.0  
**Last Updated**: January 2024  
**Status**: Draft  
**Maintainer**: Development Team
