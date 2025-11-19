# UI, Menus & Settings System Protocol

This document defines the complete user interface ecosystem for Protocol EMR, covering the main menu, pause menu, settings suite, key binding customization, profile management, and supporting informational screens. The goal is to deliver a polished, modern experience that mirrors the game's diegetic aesthetic while ensuring every gameplay-relevant toggle, slider, and binding is persistent, discoverable, and accessible.

## Table of Contents

1. [Main Menu System](#main-menu-system)
2. [Pause Menu](#pause-menu)
3. [Settings Menu](#settings-menu)
4. [Keyboard Bindings & Input Customization](#keyboard-bindings--input-customization)
5. [Player Profile & Account](#player-profile--account)
6. [UI/UX Design Standards](#uiux-design-standards)
7. [Pause Menu Inventory Access](#pause-menu-inventory-access)
8. [Save Slot Indicators & Feedback](#save-slot-indicators--feedback)
9. [Credits & Info Screens](#credits--info-screens)
10. [Technical Requirements](#technical-requirements)
11. [Implementation Blueprint](#implementation-blueprint)
12. [Integration Points](#integration-points)
13. [Prototype Deliverables](#prototype-deliverables)
14. [QA and Testing Checklist](#qa-and-testing-checklist)
15. [Performance & Accessibility Targets](#performance--accessibility-targets)
16. [Acceptance Criteria](#acceptance-criteria)

## Main Menu System

### Overview
The main menu is the player's first touchpoint and must communicate the high-tech, clinical aesthetic of Protocol EMR. All entry flows (single player, multiplayer placeholder, settings, credits, exit) originate here, with profile context and latest save metadata surfaced without clutter.

### Layout Specification

| Zone | Content | Notes |
|------|---------|-------|
| **Upper Left** | Protocol EMR logotype + build version | Version string pulls from `BuildInfo.json` for traceability. |
| **Center** | Hero background (animated facility atrium), parallax holographic elements, dynamic camera sweep | Background loops every 45 seconds with subtle exposure shifts. |
| **Primary Button Stack** | Continue (if available), Single Player, Multiplayer (future), Settings, Credits, Exit | Buttons are vertically aligned with 32px spacing and iconography for fast recognition. |
| **Secondary Row** | Profile selector, Accessibility shortcut, Patch notes link | Styled as pill buttons; accessibility shortcut opens colorblind/high-contrast quick menu. |
| **Footer** | Legal text, social links, telemetry opt-in toggle | Telemetry toggle persists per profile. |

### Navigation Flow

```
Main Menu
├── Continue (recent profile) → Loading Screen → Last save
├── Single Player → Profile Selection → Save Slot Picker → Game
├── Multiplayer (locked) → Coming Soon modal
├── Settings → Settings Tabs (Graphics/Audio/Gameplay/Controls)
├── Credits → Credits Roll Screen → Back
└── Exit → Confirmation → Desktop
```

Input support: keyboard (arrows/WSAD/Enter/Esc), mouse, and gamepad (left stick/D-pad, A for confirm, B for back). Focus order wraps to prevent dead ends.

### Visual Theme & Background
- **Palette**: Deep charcoal (#0D1117), cyan accent (#00F0FF), emergency amber (#FF9A3C) for warnings.
- **Materials**: Glass panels with additive bloom, volumetric light shafts, subtle screen-space reflections to mirror in-game labs.
- **Dynamic Elements**: Holographic schematics animate in sync with ambient music BPM (96 BPM). Procedural particle drift adds depth without distracting from text.

### Animations & Transitions

| Event | Animation | Duration | Notes |
|-------|-----------|----------|-------|
| Screen arrival | Camera dolly-in + logo fade | 900 ms | Uses Timeline asset with cubic ease-out. |
| Button focus change | Scale 1.0 → 1.06 + glow pulse | 120 ms | Works for mouse hover or controller focus. |
| Screen change | Crossfade with directional wipe | 350 ms | Pause/resume states disabled during transition. |
| Exit confirmation | Modal slides from bottom, background blur increases | 250 ms | Click-outside closes modal. |

### Loading Screen & Progress Indicators
- **Design**: Full-bleed render of destination environment with blueprint overlay.
- **Progress UI**: Circular progress ring + percentage + textual stage (e.g., "Streaming Assets", "Syncing Profile").
- **Tips Carousel**: Rotates contextual tips every 6 seconds; taps into localization database.
- **Fallback**: If loading exceeds 10 seconds, show animated waveform and reassure player via narrator VO snippet.

### Menu Audio & Music Integration
- **Music**: Ambient track in FMOD with stems (pads, pulses, percussion). Layers modulate based on highlight (e.g., entering Settings adds percussive ticks).
- **SFX**: UI interactions map to `ui/menu/hover`, `ui/menu/select`, `ui/menu/back`. Each event has 5% random pitch variation to avoid fatigue.
- **Duck Logic**: Menu music volume reduces to 70% when narrator VO or accessibility screen reader speaks.
- **Loading VO**: Optional AI narrator lines triggered if estimated load > 5 seconds; suppressed if player disabled narration.

## Pause Menu

### Access & Activation
- Triggered by **ESC** (keyboard), **Start** (gamepad), or hold left menu button on Steam Deck.
- Pausing is disabled during cinematics, timed escape sequences, or while critical network events occur; attempting to pause shows "Pause Unavailable" toast.
- Menu opens within 200 ms, freezing player input but allowing camera orbit in Photo Mode (if unlocked).

### Core Options

| Option | Description | Notes |
|--------|-------------|-------|
| **Resume** | Closes pause menu, returns to gameplay | Default focus when menu opens. |
| **Settings** | Opens same tabs as main menu | Changes persist immediately but can be reverted via countdown dialog. |
| **Inventory & Mission Log** | Tabbed sub-panel accessible without unpausing | Mirrors HUD data; read-only for mission objectives when narrative lock is active. |
| **Main Menu** | Returns player to main menu after confirmation | Warns about unsaved progress unless auto-save triggered within last 60 seconds. |
| **Quit to Desktop** | Exits application after double confirmation | Forces save checkpoint if safe. |

### HUD State When Paused
- Main HUD fades to 30% opacity but remains visible beneath overlay for context.
- Inventory grid, mission log, communications, and codex remain accessible in read-only mode unless explicitly disabled by designers.
- Multiplayer placeholder: when implemented, pause becomes "Tactical Overlay" where gameplay continues; UI clearly labels "Game Continues".

### Visual Pause Overlay
- Foreground panel slides in from right with 12 px radius corners.
- Background uses 40% dim, 6 px Gaussian blur, and chromatic aberration reduction for legibility.
- Depth-of-field effect subtly emphasizes menu panel while preserving silhouette of frozen action.

### Context Awareness
- **No-Pause Zones**: Cinematics, quick-time events, forced walking sequences. ESC input displays unobtrusive toast explaining restriction.
- **Auto-Resume**: When an unskippable cutscene begins, any open pause menu auto closes and logs the action for telemetry.
- **Audio Handling**: Gameplay audio ducks to 25% volume; menu theme crossfades in.

## Settings Menu

### Structural Overview
- Organized into tabs: **Graphics**, **Audio**, **Gameplay**, **Controls**, **Accessibility** (shortcut from footer).
- Left column hosts category list; right pane displays controls with inline descriptions and preview thumbnails.
- Apply/Cancel bar anchored at bottom with "Apply Changes" (Ctrl+Enter), "Revert" (R), and timer-based rollback (10-second countdown) for graphics changes.

### Graphics Quality Presets

| Preset | Target Hardware | Changes Applied |
|--------|-----------------|-----------------|
| **Low** | Integrated GPUs / Steam Deck | Reduces texture/shadow resolution, disables ray tracing, lowers particle count, caps FPS at 60. |
| **Medium** | Mid-range cards (GTX 1660 / RX 580) | Balanced visuals; medium textures/shadows, SSR low, volumetrics medium. |
| **High** | Upper-mid (RTX 3060 / RX 6700 XT) | High textures, long shadow distance, SSAO high, ray tracing optional. |
| **Ultra** | Enthusiast (RTX 4080 / RX 7900 XT) | Max textures, ultra shadows/effects, ray tracing max, unlocked FPS. |
| **Custom** | User-defined | Preserves individual overrides; displays "Custom" badge when any slider differs from preset. |

### Individual Graphics Settings

| Setting | Options / Range | Default | Notes |
|---------|-----------------|---------|-------|
| Resolution | 1920×1080, 2560×1440, 3840×2160, Custom | 1920×1080 | Custom opens dropdown + manual entry validated at runtime. |
| Window Mode | Fullscreen, Borderless, Windowed | Borderless | Borderless recommended for fast alt-tab. |
| FPS Cap | 30, 60, 144, Unlimited | 60 | Unlimited still respects V-Sync toggle. |
| V-Sync | On / Off | On | Auto-disabled if FPS cap is 30 to reduce latency. |
| Texture Quality | Low / Medium / High / Ultra | High | Affects streaming budget (MB). |
| Shadow Quality | Off / Low / Medium / High | Medium | Shadow distance slider (10–200 m) unlocked when ON. |
| Effects Quality | Low / Medium / High / Ultra | High | Controls particles, bloom, heat distortion. |
| Ray Tracing | Disabled / Enabled | Disabled | "Intensity" slider (0–100%) appears when enabled to scale bounce strength. |
| Field of View | 75°–110° | 90° | Slider increments 1°. Preview updates instantly. |
| Motion Blur | Off / Low / Medium / High | Low | Toggles per-camera to respect accessibility preferences. |
| Depth of Field | Off / Cinematic / Balanced | Balanced | Balanced maintains clarity for interactables. |
| Post-Processing Intensity | 0–100% slider | 70% | Scales color grading LUT strength and film grain. |

### Audio Settings

| Setting | Range / Options | Default | Notes |
|---------|-----------------|---------|-------|
| Master Volume | 0–100% slider | 80% | Linear slider with numeric input. |
| Music Volume | 0–100% | 70% | Affects ambient and menu music. |
| SFX Volume | 0–100% | 85% | Includes footsteps, UI, interactions. |
| Voice / Dialogue | 0–100% | 90% | Prioritized over other channels via ducking. |
| 3D / Spatial Audio | On / Off | On (if hardware supports) | Switches between stereo, Windows Sonic, Dolby Atmos. |
| Subtitles | Off / On | On | Supports font size (Small, Medium, Large) + background box toggle. |
| Subtitle Size | Small / Medium / Large | Medium | Independent of global UI scale. |

### Gameplay Settings

| Setting | Options / Range | Default | Notes |
|---------|-----------------|---------|-------|
| Difficulty | Easy, Normal, Hard, Nightmare | Normal | Updates enemy AI heuristics and checkpoint frequency. |
| HUD Opacity | 20%–100% slider | 80% | Applies to health, stamina, objective UI. |
| Objective Markers | Show / Hide | Show | Hiding still allows compass pulses when near objectives. |
| Crosshair | Off / Minimal / Standard / Dynamic | Standard | Style preview displayed next to selector. |
| Camera Sensitivity | 0.1–2.0 multiplier | 1.0 | Separate sliders for X/Y; link toggle available. |
| Sprint | Hold / Toggle | Hold | Toggle retains 1-second debounce to avoid accidental toggling. |
| Invert Y-Axis | Off / On | Off | Applies to both mouse and controller unless split option enabled. |
| Interact Prompt Style | Text / Icon / Both | Both | Aids accessibility preferences. |

### Settings Application Flow
1. Player adjusts settings within tab.
2. "Apply" lights up when unsaved changes exist; pressing confirms and writes to temp buffer.
3. Graphics changes prompt 10-second confirmation overlay; failure to confirm reverts to prior stable state.
4. "Restore Defaults" available per tab and globally; per-tab reset uses last preset per profile.
5. All successful changes emit `SettingsChanged` event for subsystems.

### Settings Persistence & Serialization
- Settings stored per profile in `ProfileSettings.json` located under `%USERPROFILE%/ProtocolEMR/Profiles/<profileId>/`.
- Schema versioned to support migration; includes CRC for corruption detection.
- Snippet:
  ```json
  {
    "profileId": "1f9c2fd5-7fcd-4c6d-9b1e-112233445566",
    "graphics": { "preset": "High", "fov": 90, "rayTracing": {"enabled": false, "intensity": 0} },
    "audio": { "master": 0.8, "music": 0.7, "3dAudio": true },
    "gameplay": { "difficulty": "Normal", "hudOpacity": 0.8 },
    "controls": { "scheme": "WASD", "bindingsHash": "9ac4..." }
  }
  ```

## Keyboard Bindings & Input Customization

### System Overview
A unified input abstraction (Unity Input System) feeds both gameplay and UI. Each action maps to a `BindingContext` containing keyboard, mouse, and gamepad bindings. Changes are transactional: players edit in a staging layer, preview effects, then apply.

### Rebinding Interaction Flow
1. Player selects action → highlight animates + "Press any key..." prompt.
2. Input listener waits 5 seconds for new key; Escape cancels.
3. If modifier is held (Ctrl/Shift/Alt), binding records chord.
4. Conflict detection runs before commit (see below).
5. New binding previewed; player can Apply, Undo, or Reset Single Action.

### Default Key Bindings

| Action | Default Primary | Default Alternate | Category | Notes |
|--------|-----------------|--------------------|----------|-------|
| Move Forward | W | Up Arrow | Movement | Supports analog blending with controller stick. |
| Move Left | A | Left Arrow | Movement | |
| Move Back | S | Down Arrow | Movement | |
| Move Right | D | Right Arrow | Movement | |
| Sprint | Left Shift (Hold) | Press Mouse Button 4 (Toggle) | Movement | Toggle option surfaced in Gameplay settings. |
| Crouch | Left Ctrl | C (Hold) | Movement | "C" acts as long-press crouch; conflict rules allow shared use with Virtual Phone due to gesture differentiation. |
| Jump | Space | Mouse Button 5 | Movement | |
| Interact / Pick Up | E | F | Interaction | Context prompts show both keys when assigned. |
| Inventory | I | Tab | UI | Tab opens compact view overlay. |
| Pause | ESC | P | UI | ESC reserved globally. |
| Virtual Phone | C (Double Tap) | V | UI | Double-tap detection window 250 ms; conflict detection warns if user assigns C elsewhere. |
| Quick Save | F5 | None | System | Disabled in combat arenas per design. |
| Quick Load | F9 | None | System | Requires confirmation when in combat. |
| Melee Attack | Left Mouse Button | Left Ctrl + Left Mouse | Combat | Supports charge by holding. |
| Dodge / Evade | Q | Middle Mouse Button | Combat | Has cooldown indicator in HUD. |
| Map / Compass | M | ` | UI | Auto-centers on player. |
| Objectives Log | J | L | UI | Mirrors mission log from pause menu. |

### Conflict Detection & Resolution
- When a new binding duplicates an existing one (same device + chord), the UI presents modal: `"Key already bound to [Action]. Overwrite?"`.
- Options:
  - **Overwrite**: Existing action becomes unassigned.
  - **Swap**: Exchange bindings between actions.
  - **Cancel**: Keep original binding.
- Conflict highlights appear in list (amber outline) until resolved.
- System logs conflicts to analytics to inform future default tuning.

### Preset Configurations

| Preset | Description | Use Case |
|--------|-------------|----------|
| **WASD Standard** | Default PC layout with mouse aim | Most players; QA baseline. |
| **IJKL Alternative** | Movement on IJKL, actions shifted right-hand | Left-handed mouse players. |
| **ESDF Alternative** | Movement on ESDF, frees extra key reach | Competitive players wanting home-row access. |
| **Custom/Hardcore** | Minimal HUD prompts, sparse bindings | Immersive runs; prompts disabled by default. |
| **Gamepad (Xbox)** | ABXY, triggers, sticks mapped to Xbox labels | Series controllers, XInput API. |
| **Gamepad (PlayStation)** | Cross/Circle/Square/Triangle mapping | DualSense/DualShock, uses glyph swap. |

One-click preset load updates all bindings and settings; confirmation dialog warns about unsaved custom mappings.

### Advanced Rebinding Features
- **Reset to Default**: Global button plus per-action reset icon.
- **Search/Filter**: Instant search reduces list to matching actions; filters by category (Movement, Combat, UI, General).
- **Bind/Unbind Keys**: Users can clear a binding entirely to prevent accidental triggers.
- **Preview Layer**: Pending changes highlighted in cyan until applied; pressing Apply commits and writes to disk.
- **Profile Awareness**: Bindings stored per profile; switching profiles updates UI immediately.
- **Export / Import**: Players can export bindings to JSON (`bindings_<profile>_<timestamp>.json`) and import from disk; invalid files validated with schema.

### Gamepad Support Enhancements

| Feature | Specification |
|---------|---------------|
| Button Rebinding | Supports ABXY, LB/RB, LT/RT (analog), Start/Back, L3/R3. |
| Stick Sensitivity | Slider per stick (0.5–2.0) plus dead zone adjustment (0–20%). |
| Trigger Curves | Linear, Exponential, or Custom curve editor for LT/RT. |
| Vibration Intensity | 0–100% slider plus test button. |
| Toggle Gamepad Support | Master switch disables detection to prevent controller wake-ups. |
| Glyph System | Dynamically swaps between Xbox/PlayStation glyph fonts; falls back to generic icons for others. |

## Player Profile & Account

### Profile Selection at Startup
- On boot, players land on profile carousel if multiple profiles exist; otherwise they are prompted to create one.
- Carousel displays avatar, profile name, last played timestamp, total playtime, and current difficulty.
- Quick actions: **Continue**, **Manage**, **New Profile**.

### New & Edit Profile Flow
- **Create Screen**: Name input (1–20 characters, validation, inline error messages), avatar grid, preferred difficulty, optional color accent.
- **Edit Screen**: Rename, cycle avatars, switch accent color, toggle privacy (share analytics).
- All changes previewed on right side in real time.

### Statistics & Metadata Panel

| Metric | Display | Source |
|--------|---------|--------|
| Total Playtime | hh:mm:ss | Aggregated from save metadata. |
| Missions Completed | Numeric + bar | Derived from mission tracker. |
| Achievements | Badge grid | Unlocked achievements highlight; locked greyed out. |
| Last Location | Text + thumbnail | Uses last save screenshot. |

### Profile Icon / Avatar Selection
- 12 default avatars + unlockable frames tied to achievements.
- Avatar selection UI supports mouse hover preview, controller bumpers to page through categories.

### Profile Deletion
- Requires two-step confirmation: modal + typing profile name.
- Displays cascading consequences (saves, settings, bindings) before final confirmation.

## UI/UX Design Standards

### Modern Aesthetic Guidelines
- Clean, minimalist layout with generous negative space and glassmorphism panels.
- Use of subtle gradients (vertical 4% intensity) to differentiate sections without heavy borders.
- Micro-interactions (hover glows, haptic ticks) provide feedback without overwhelming.

### Color & Contrast
- Base background #05070B, panels #101621 @ 88% opacity.
- Accent colors limited to cyan (interactive), amber (warnings), magenta (#FF3CF3) for narrative-critical alerts.
- Accessibility mode increases contrast to WCAG AAA (7:1 ratio) and swaps gradient backgrounds for solid colors.

### Button States

| State | Treatment |
|-------|-----------|
| Normal | 1px outline, 80% opacity fill. |
| Hover / Focus | Scale +4%, accent glow, text color brightens. |
| Active / Pressed | Depressed effect, drop shadow retracts, haptic pulse (gamepad). |
| Disabled | 40% opacity, desaturated icon, tooltips explain requirements. |

### Typography
- Primary font: **Space Grotesk** (Headings), Secondary: **Inter** (Body/UI), both optimized for 4K readability.
- Font hierarchy: H1 36px, H2 28px, body 18px, captions 14px.

### Accessibility
- Colorblind filters (Protanopia, Deuteranopia, Tritanopia) previewed live.
- High-contrast mode toggle accessible from footer without entering Settings tab.
- Screen reader support enumerates focus order; all buttons have accessible names.
- Subtitle size and background box available globally; closed captions include speaker labels and SFX tags.

### Controller & Keyboard Navigation
- Analog stick or D-pad moves focus; shoulder buttons switch tabs.
- Focus trapping prevents user from navigating behind modal dialogs.

### Responsive Scaling
- UI scales between 0.8x–1.3x; safe zones adapt to 16:9, 16:10, 21:9.
- Min target size 48 px for touch-support roadmap.

### Animation Principles
- Use easing consistent with Material Motion (cubic-bezier 0.4, 0, 0.2, 1).
- Entrance/exit animations under 400 ms to keep UI responsive.
- Blur and depth effects kept under 8 ms GPU budget.

## Pause Menu Inventory Access

| Feature | Details |
|---------|---------|
| Inventory Grid | Displays 6×4 items with category filters (Weapons, Tools, Consumables). |
| Item Details | Hover/select reveals 3D turntable preview, stats, lore snippet. |
| Equipment Management | Drag-and-drop (mouse) or radial selection (controller) allows equipping/unequipping gear while paused; changes propagate immediately upon resume. |
| Quick Slot Binding | Dedicated panel shows four quick slots; players can assign items via drag or button prompts (e.g., `Assign to Slot 1`). |
| Mission Log | Adjacent tab lists active + completed objectives, timestamped updates. |
| Restrictions | Certain boss arenas lock equipment swaps; UI labels slots as "Locked" with tooltip explanation. |

## Save Slot Indicators & Feedback

| Indicator | Description |
|-----------|-------------|
| Current Save Slot Badge | Displays slot type (Auto, Manual, Quick, Checkpoint) + timestamp in pause and main menus. |
| Progress Bar | Within save/load menu and pause overlay; shows completion percentage derived from mission manager. |
| Auto-Save Icon | Animated spiral appears top-right of HUD during auto-save; also mirrors in pause menu with "Auto-Save Active" message. |
| Slot Preview | Each slot shows location thumbnail, playtime, difficulty, and save version; invalid slots flagged in red with repair option. |

## Credits & Info Screens

### Credits Roll
- Scrollable list with job titles, names, and departments.
- Supports skip (hold Space/Controller A) while still logging view time.
- Background features slow parallax of facility schematics.

### Version & Legal Information
- Panel accessible from main menu footer.
- Displays build number, commit hash, third-party middleware versions, EULA, privacy policy links.

### Controls Help Screen
- Presents current keybinds and gamepad layout diagrams, generated dynamically from binding data.
- Search bar filters actions; print/export to PDF planned for future.

### Legal & Licensing
- Includes logos and attribution text for Unity, FMOD Studio, Dolby Atmos, etc.
- Displays ESRB/PEGI placeholder icons, localization acknowledgements.

## Technical Requirements

| Component | Requirement |
|-----------|-------------|
| UI Framework | Unity UI Toolkit + UGUI Canvas hybrid. Canvas scaler set to Scale With Screen Size (1920×1080 reference). |
| Input Manager Architecture | Unity Input System (1.5+) with action maps (`UI`, `Gameplay`, `Menus`). Supports rebinding API, composite bindings, and device hot-swap. |
| Settings Serialization | JSON via `System.Text.Json` with versioning, CRC checks, and async disk IO to avoid frame drops. |
| Gamepad API Integration | XInput for Xbox controllers, HID/Sony API for DualSense, SDL fallback on Linux. Glyph provider maps runtime IDs to correct icons. |
| Audio Middleware | FMOD Studio routing for UI SFX/music, with exposed parameters for ducking and accessibility voices. |
| Localization | XLIFF-based string tables; UI loads text via key to guarantee translation coverage. |
| Telemetry | Menu interactions emit events (`MenuOpen`, `SettingChanged`, `BindingConflict`) to analytics queue for UX analysis. |

## Implementation Blueprint

1. **Wireframe & Content Audit (Week 1)**
   - Finalize layouts, gather required strings/assets, confirm focus order.
2. **Core Systems (Week 2)**
   - Implement Canvas hierarchy, Input action maps, Settings data model, serialization layer.
3. **Feature Integration (Week 3)**
   - Hook save-slot indicators, profile metadata, pause inventory, audio routing.
4. **Polish & Accessibility (Week 4)**
   - Add animations, haptics, localization passes, screen reader labels, QA tuning.
5. **Stability & Performance (Week 5)**
   - Instrument telemetry, fix edge cases, ensure persistence across restarts, finalize art passes.

## Integration Points

| System | Integration Detail |
|--------|--------------------|
| Save/Load | Settings, bindings, and profile metadata stored alongside saves; pause menu warns before losing progress. |
| Mission System | Objectives and mission logs pulled via API for HUD and pause menu. |
| Inventory & Equipment | Shared data models allow pause menu edits to propagate immediately. |
| Audio Subsystem | Menu SFX routed through dedicated bus; master slider updates RTPC values. |
| Localization | All strings pulled from localization database with fallback to English. |
| Analytics | Key interactions tracked for UX improvements and bug triage. |
| Accessibility Manager | Colorblind, contrast, subtitles toggles centrally managed for consistency. |

## Prototype Deliverables
- Fully functional main menu with transitions, audio, loading screen, and profile integration.
- Pause menu with inventory, mission log, settings access, and context-aware restrictions.
- Settings suite implementing all graphics/audio/gameplay options with persistence and reversion logic.
- Complete keyboard/mouse + gamepad rebinding UI with conflict detection, presets, and export/import.
- Profile management screens (select, create, edit, delete) tied into save-slot system.
- Credits, version info, and controls help screens populated with placeholder content and dynamic bindings.
- Telemetry hooks, accessibility features, and localization-ready strings.

## QA and Testing Checklist

| ID | Scenario | Expected Result |
|----|----------|-----------------|
| UI-001 | Launch game → main menu | All buttons enabled, focus on Continue or Single Player, background animates smoothly. |
| UI-005 | Change graphics preset to Ultra → Apply → Confirm | Visual settings update, revert countdown appears, confirm saves to disk. |
| UI-010 | Rebind Interact to same key as Jump | Conflict dialog appears, user chooses overwrite or cancel, state reflects choice. |
| UI-015 | Export custom bindings → Import after restart | Bindings file saved to disk, import restores mapped keys accurately. |
| UI-020 | Open pause menu during cinematic | Toast "Pause unavailable during cinematics" appears, gameplay continues. |
| UI-030 | Access inventory while paused | Inventory grid interactable, equipment changes persist after resume. |
| UI-040 | Trigger auto-save | Save indicator animates, pause menu shows updated slot timestamp. |
| UI-050 | Open controls help screen | Displays current bindings with correct glyphs, updates if bindings change mid-session. |
| UI-060 | Enable colorblind mode | Palette adjusts immediately, contrast meets WCAG guidelines. |
| UI-070 | Quit to desktop from pause menu | Confirmation dialog displayed, unsaved progress warning shown when necessary. |

## Performance & Accessibility Targets

| Metric | Target |
|--------|--------|
| Menu Load Time | < 2.0 seconds from splash to main menu on SSD, < 4.0 seconds on HDD. |
| Pause Menu Open Time | < 0.2 seconds. |
| Input Latency (UI) | < 50 ms between input and visual response. |
| Memory Overhead | UI subsystems consume < 150 MB combined. |
| Frame Budget | Menu scenes maintain 120 FPS on target hardware; in-game menus add < 2 ms GPU time. |
| Localization Coverage | 100% of strings in translation tables with fallback. |
| Accessibility Compliance | Meets WCAG 2.1 AA for contrast; subtitles available in all supported languages. |

## Acceptance Criteria

1. **GIVEN** the player is on the main menu **WHEN** they navigate between Single Player, Settings, Credits, and Exit **THEN** transitions are animated, audio cues play, and visual themes remain consistent with Protocol EMR branding.
2. **GIVEN** the player is in-game **WHEN** they press ESC outside restricted sequences **THEN** the pause menu opens with Resume, Settings, Main Menu, Quit options and HUD/inventory panels remain accessible without gameplay progressing.
3. **GIVEN** the player opens the Settings menu **WHEN** they modify graphics, audio, or gameplay options **THEN** changes can be previewed, applied, reverted, and persist via JSON serialization per profile.
4. **GIVEN** the player customizes key bindings **WHEN** they assign keys that already map to another action **THEN** the system detects the conflict, prompts for resolution, and prevents silent overrides; presets and reset options function as described.
5. **GIVEN** the player uses keyboard, mouse, or gamepad **WHEN** they navigate menus or rebind actions **THEN** input is recognized, modifiers are supported, presets can be loaded in one click, and profile-specific bindings load automatically on startup.
6. **GIVEN** the player accesses inventory from the pause menu **WHEN** they inspect items, manage equipment, or bind quick slots **THEN** changes reflect immediately after resuming gameplay while respecting area restrictions.
7. **GIVEN** a save event occurs **WHEN** the player views menus **THEN** save slot indicators display current slot, progress, and auto-save status so the player always knows the safety of their progress.
8. **GIVEN** the player opens Credits, Version Info, or Controls Help **WHEN** they review the content **THEN** credits roll smoothly, build metadata appears, and controls reflect real-time bindings for transparency.

A build satisfying all criteria delivers a fully functional, polished menu system with smooth navigation, complete keybinding customization, conflict detection, presets, and persistent settings across sessions.
