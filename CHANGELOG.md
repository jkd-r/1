# Protocol EMR - Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned
- Sprint 2: Character animations (Mixamo integration)
- Sprint 3: Combat system foundation
- Sprint 4: NPC AI and behavior trees
- Sprint 5-6: UI system (HUD, inventory, phone interface)
- Sprint 7-8: Audio middleware (FMOD/Wwise)
- Sprint 9-10: Procedural generation and mission system

---

## [0.1.0] - Sprint 1 Foundation - 2024-01-XX

### Added - Core Systems
- **Input System**
  - PlayerInputActions asset with 5 action maps (Movement, Look, Interact, UI, Combat)
  - InputManager singleton with event-driven architecture
  - Keyboard + Mouse support (WASD, mouse look, standard FPS controls)
  - Gamepad support (Xbox/PlayStation controller mapping)
  - Input rebinding system with JSON persistence
  - Hold vs Toggle options for sprint and crouch
  
- **Camera System**
  - FirstPersonCamera controller with smooth mouse look
  - Adjustable mouse sensitivity (default: 1.0)
  - Camera bobbing during movement (toggleable in accessibility)
  - FOV adjustment support (60-120° range, default: 90°)
  - Camera shake foundation for impact feedback
  - Head position offset from body center
  
- **Player Controller**
  - Character capsule with CharacterController physics
  - WASD movement with sprint and crouch mechanics
  - Sprint stamina system (drains during use, regenerates when idle)
  - Jump mechanics with gravity
  - Interaction raycast system (E key, 3-unit range)
  - Crouch state with smooth height transition
  - Configurable movement speeds (walk: 5, sprint: 8, crouch: 2.5)
  
- **Settings System**
  - SettingsManager with JSON persistence
  - Graphics settings: Quality presets (Low/Medium/High/Ultra/Custom)
  - Audio settings: Master, Music, SFX, Voice volume sliders
  - Gameplay settings: Mouse sensitivity, difficulty, HUD opacity
  - Accessibility settings: Colorblind mode, motion blur toggle, FOV, camera shake intensity
  - Settings auto-save to persistent data path
  
- **Performance Monitoring**
  - PerformanceMonitor debug tool (toggle with F1)
  - Real-time FPS counter
  - Frame time display (milliseconds)
  - Memory usage tracking (MB)
  - Resolution and quality level display
  - Color-coded performance bar (green: 60+fps, yellow: 30-60fps, red: <30fps)

- **Game Management**
  - GameManager singleton for system initialization
  - Pause system (ESC key, stops time)
  - Scene loading utilities (sync and async)
  - Auto-instantiation of core systems

### Added - Example Systems
- **InteractableObject** base class
  - IInteractable interface for player interactions
  - Configurable interaction cooldown
  - Multi-interaction support
  - Visual highlight on mouse hover
  
- **InteractableDoor** example
  - Extends InteractableObject
  - Smooth door rotation animation
  - Open/close toggle on interaction
  - Configurable open angle and speed

### Added - Documentation
- **README.md**: Complete project overview with feature list
- **ProjectSetup.md**: Unity configuration and setup guide
- **AssetSourcing.md**: Free asset sources and import procedures
- **SceneSetupGuide.md**: Step-by-step scene creation tutorial
- **CHANGELOG.md**: This file

### Added - Project Structure
- Complete folder hierarchy:
  - Assets/Scripts/Core/ (Input, Camera, Player, Settings, Performance)
  - Assets/Scripts/Systems/ (Interactables)
  - Assets/Scenes/ (Main.unity)
  - Assets/Prefabs/ (Player, UI, etc.)
  - Assets/Materials/, Models/, Textures/, Audio/, Animations/, VFX/, UI/
  - Assets/Documentation/

### Added - Configuration
- .gitignore with Unity + FMOD/Wwise exclusions
- Packages/manifest.json with URP and Input System
- ProjectSettings/ProjectVersion.txt (Unity 2022.3.15f1)
- ProjectSettings/EditorBuildSettings.asset

### Technical Details
- **Unity Version**: 2022.3.15f1 LTS
- **Render Pipeline**: Universal Render Pipeline (URP) 14.0.8
- **Input System**: Unity Input System 1.7.0
- **Physics**: PhysX with CharacterController
- **Target Performance**: 60 FPS @ 1080p Medium settings
- **Memory Budget**: <2GB for foundation

### Performance Targets
- ✅ FPS: 60+ at 1080p Medium
- ✅ Memory: <2GB
- ✅ Load Time: <3s per scene
- ✅ Input Latency: <16ms (1 frame @ 60fps)

### Integration Points
- Input system ready for Sprint 2 animation integration
- Camera system ready for Sprint 3 ADS (aim down sights)
- Settings system prepared for Sprint 5-6 UI integration
- Player controller prepared for Sprint 2 locomotion animations

---

## Version History

### Sprint Milestones
- **Sprint 1** (v0.1.0): Foundation ✅
- **Sprint 2** (v0.2.0): Locomotion & Animations - Planned
- **Sprint 3** (v0.3.0): Combat Foundation - Planned
- **Sprint 4** (v0.4.0): NPC AI Foundation - Planned
- **Sprint 5** (v0.5.0): UI Systems Part 1 - Planned
- **Sprint 6** (v0.6.0): UI Systems Part 2 - Planned
- **Sprint 7** (v0.7.0): Audio Integration Part 1 - Planned
- **Sprint 8** (v0.8.0): Audio Integration Part 2 - Planned
- **Sprint 9** (v0.9.0): Content & Polish Part 1 - Planned
- **Sprint 10** (v1.0.0): Content & Polish Part 2 - Planned

### Release Candidates
- **Alpha 1** (v0.3.0): After Sprint 3 - Core gameplay loop functional
- **Alpha 2** (v0.6.0): After Sprint 6 - Full UI and player systems
- **Beta 1** (v0.8.0): After Sprint 8 - Content complete, polish needed
- **Beta 2** (v0.9.0): After Sprint 9 - Feature complete, final polish
- **Release Candidate** (v0.10.0): After Sprint 10 - Gold master preparation
- **Gold Master** (v1.0.0): Final release build

---

## Known Issues

### Sprint 1
- Player model is primitive capsule (visual character coming in Sprint 2)
- No UI elements (HUD, inventory, menus coming in Sprint 5-6)
- Audio system uses Unity built-in (FMOD/Wwise in Sprint 7-8)
- No animation system (Mixamo integration in Sprint 2)
- No combat mechanics (coming in Sprint 3)
- No NPC AI (coming in Sprint 4)

---

## Credits

### Development Team
- Project Lead: [Your Name]
- Programmer: [Your Name]
- Designer: [Your Name]

### Third-Party Assets (Planned)
- Mixamo: Character animations
- Poly Haven: PBR materials and HDRIs
- Freesound: Audio effects
- Unity Asset Store: UI elements

### Tools and Packages
- Unity 2022.3 LTS
- Universal Render Pipeline
- Unity Input System
- TextMeshPro

---

## Links

- **Repository**: [Add URL]
- **Documentation**: `/docs/protocol-emr/`
- **Build Roadmap**: `/docs/protocol-emr/build-coding-roadmap.md`
- **Issue Tracker**: [Add URL]

---

**Last Updated**: Sprint 1 Foundation Phase
