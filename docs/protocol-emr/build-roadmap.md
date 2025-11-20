# Protocol EMR - Build Roadmap

## Overview

This document provides a compressed build and coding plan for Protocol EMR, translating the comprehensive specification set into actionable development sprints. The roadmap prioritizes core gameplay functionality first, followed by visual polish, and optional features last, ensuring a playable prototype is achieved early while maintaining the "professional indie" quality target.

### Guiding Principles

- **Core First**: Implement essential gameplay mechanics before visual polish
- **Incremental Value**: Each sprint delivers a playable, testable experience
- **Asset Efficiency**: Leverage free and low-cost assets to maintain budget constraints
- **Performance Targeted**: Optimize for 60 FPS on mid-range hardware throughout development
- **Quality Bar**: Professional indie standards with clean code and thorough testing

### Quality Standards

- **Code Quality**: Clean, documented, and maintainable code following Unity best practices
- **Performance**: Consistent 60 FPS on GTX 1660/RX 580 equivalent hardware
- **Stability**: Zero crashes during 30-minute play sessions
- **User Experience**: Intuitive interfaces with accessibility support
- **Asset Quality**: Professional-grade audio/visual assets from curated sources

### Asset Sourcing Strategy

| Asset Type | Primary Sources | Secondary Sources | Budget Target |
|------------|----------------|-------------------|---------------|
| **3D Models** | Unity Asset Store (Free/Paid), Sketchfab Free | Itch.io, TurboSquid (Free) | $0-500 |
| **Audio** | Freesound.org, FMOD Studio (Free tier) | AudioJungle (Budget) | $0-200 |
| **Textures** | Poly Haven, Textures.com (Free tier) | Unity Asset Store | $0-100 |
| **UI Assets** | Unity UI Toolkit, Font Awesome | Custom creation | $0-50 |
| **Animation** | Mixamo, Unity Asset Store | Custom keyframes | $0-300 |

## Table of Contents

1. [Sprint 0: Foundation & Setup](#sprint-0-foundation--setup)
2. [Sprint 1: Core Gameplay Loop](#sprint-1-core-gameplay-loop)
3. [Sprint 2: Combat & Interaction Systems](#sprint-2-combat--interaction-systems)
4. [Sprint 3: Narrative & Progression](#sprint-3-narrative--progression)
5. [Sprint 4: Audio/Visual Polish](#sprint-4-audiovisual-polish)
6. [Sprint 5: Quality & Accessibility](#sprint-5-quality--accessibility)
7. [Dependency Sequencing](#dependency-sequencing)
8. [Risk Mitigation](#risk-mitigation)
9. [Performance Gates](#performance-gates)

---

## Sprint 0: Foundation & Setup

**Focus**: Establish development infrastructure and core project architecture
**Duration**: 1-2 weeks
**Priority**: Critical foundation for all subsequent work

### Task List

1. **Unity Project Setup & Architecture** - [core-systems.md]
   - Create Unity 2022.3 LTS project with proper folder structure
   - Configure version control (Git) with appropriate .gitignore
   - Set up build pipeline for Windows/macOS/Linux targets
   - Implement core project architecture (managers, events, data structures)

2. **First-Person Controller Framework** - [core-systems.md]
   - Implement basic player movement (WASD, mouse look)
   - Add character controller with collision detection
   - Create input system using Unity's new Input System
   - Implement basic interaction raycast system

3. **Core Systems Framework** - [core-systems.md], [performance-debug-tools.md]
   - Set up event system for component communication
   - Implement state management system
   - Create debug console and basic performance monitoring
   - Establish save/load data structure foundation

4. **Basic UI Framework** - [ui-menus-settings.md]
   - Implement Unity UI Toolkit integration
   - Create basic HUD layout (health, interaction prompts)
   - Set up main menu scene with navigation
   - Implement basic settings menu structure

5. **Development Tooling** - [performance-debug-tools.md]
   - Configure FPS counter and basic profiling
   - Set up debug visualization tools
   - Implement scene loading/unloading system
   - Create basic asset import pipeline

### Dependencies & Blockers

- **Dependencies**: Unity 2022.3 LTS installation, source control setup
- **Blockers**: Unity version compatibility issues, project structure decisions

### Deliverables

- Functional Unity project with proper architecture
- Playable character with basic movement
- Main menu and basic HUD
- Debug tools and performance monitoring
- Core systems framework ready for feature implementation

### Asset Integration

- **Required**: Unity UI Toolkit, Input System packages
- **Suggested**: Cinemachine for camera management
- **Free Assets**: Basic placeholder models from Unity primitives

### Exit Criteria

**GIVEN** the project is set up
**WHEN** the game launches
**THEN** the main menu appears and can navigate to a test scene

**GIVEN** a test scene is loaded
**WHEN** the player moves with WASD
**THEN** the character moves smoothly with proper collision detection

**GIVEN** debug tools are enabled
**WHEN** the game is running
**THEN** FPS counter and basic performance metrics are visible

---

## Sprint 1: Core Gameplay Loop

**Focus**: Implement fundamental gameplay mechanics for playable prototype
**Duration**: 2-3 weeks
**Priority**: Essential for playable experience

### Task List

1. **Interaction System** - [core-systems.md]
   - Implement interactable object framework with raycast detection
   - Create pickup, examine, and use interaction types
   - Add visual feedback (highlighting, interaction prompts)
   - Implement audio feedback for interactions

2. **Inventory System** - [core-systems.md]
   - Create inventory data structure and UI panel
   - Implement item pickup and storage mechanics
   - Add item categorization and basic organization
   - Create pickup notification system

3. **Basic Save/Load** - [save-load-account.md]
   - Implement JSON serialization for game state
   - Create manual save slot system (5 slots)
   - Add player position and inventory persistence
   - Implement save/load UI in pause menu

4. **Environmental Puzzles** - [npc-procedural.md]
   - Create basic terminal hacking mini-game
   - Implement keycard door system
   - Add simple environmental interaction puzzles
   - Create puzzle progression tracking

5. **Basic NPC Framework** - [npc-procedural.md]
   - Implement basic NPC AI with state machines
   - Add simple patrol and alert behaviors
   - Create basic dialogue system
   - Implement NPC interaction framework

### Dependencies & Blockers

- **Dependencies**: Sprint 0 completion, interaction framework
- **Blockers**: Complex puzzle design decisions, AI behavior complexity

### Deliverables

- Complete interaction system with multiple object types
- Functional inventory with pickup and management
- Working save/load system
- Basic environmental puzzles
- Simple NPC encounters

### Asset Integration

- **Free Assets**: 
  - 3D models: Sketchfab Free (terminals, doors, environment props)
  - Audio: Freesound.org (interaction sounds, ambient)
  - Textures: Poly Haven (wall, floor, prop textures)

### Exit Criteria

**GIVEN** an interactable object exists
**WHEN** the player interacts with it
**THEN** the appropriate action occurs with visual/audio feedback

**GIVEN** items are placed in the world
**WHEN** the player picks them up
**THEN** items are added to inventory with notification

**GIVEN** the player saves the game
**WHEN** they load the save
**THEN** all game state is restored correctly

---

## Sprint 2: Combat & Interaction Systems

**Focus**: Add combat mechanics and advanced interaction systems
**Duration**: 2-3 weeks
**Priority**: Core gameplay enhancement

### Task List

1. **Melee Combat System** - [combat-movement.md]
   - Implement unarmed combat (punches, kicks)
   - Add hit detection with physics-based collision
   - Create impact feedback (screen shake, audio, visual)
   - Implement combo system with timing windows

2. **Weapons & Tools** - [weapons-tools-system.md]
   - Add improvised melee weapons (wrench, pipe, crowbar)
   - Implement weapon pickup and equipment system
   - Create weapon durability and degradation
   - Add weapon-specific animations and effects

3. **Advanced NPC Behaviors** - [npc-procedural.md], [combat-movement.md]
   - Implement NPC combat AI with reaction states
   - Add NPC health and damage systems
   - Create NPC patrol and investigation behaviors
   - Implement NPC alert and chase mechanics

4. **Advanced Movement** - [combat-movement.md]
   - Add sprint and crouch mechanics
   - Implement dodge and evasive maneuvers
   - Create climbing and vaulting systems
   - Add stamina system for advanced movements

5. **Environmental Hazards** - [combat-movement.md]
   - Implement electrical hazards and damage zones
   - Add falling damage and safe landing mechanics
   - Create environmental interaction combat opportunities
   - Implement hazard visual and audio warnings

### Dependencies & Blockers

- **Dependencies**: Sprint 1 completion, combat animation assets
- **Blockers**: Animation complexity, combat balance tuning

### Deliverables

- Complete melee combat system with combos
- Weapon system with durability
- Advanced NPC AI with combat responses
- Enhanced movement mechanics
- Environmental hazard systems

### Asset Integration

- **Free Assets**:
  - Animations: Mixamo (combat, movement)
  - Weapons: Sketchfab Free (improvised weapons)
  - Audio: Freesound.org (combat sounds, impacts)
  - VFX: Unity Particle Systems (sparks, impacts)

### Exit Criteria

**GIVEN** an enemy is present
**WHEN** the player attacks
**THEN** the enemy takes damage with appropriate feedback

**GIVEN** a weapon is available
**WHEN** the player picks it up
**THEN** the weapon can be equipped and used in combat

**GIVEN** the player is in combat
**WHEN** they use advanced movement
**THEN** stamina depletes appropriately and abilities function

---

## Sprint 3: Narrative & Progression

**Focus**: Implement story delivery systems and mission progression
**Duration**: 2-3 weeks
**Priority**: Narrative integration and player guidance

### Task List

1. **AI Narrator System** - [ai-narrator-and-missions.md]
   - Implement context-aware narrator messaging
   - Create HUD, phone, and voice-over delivery channels
   - Add event-driven commentary triggers
   - Implement narrator personality and tone management

2. **Virtual Phone Interface** - [core-systems.md], [ai-narrator-and-missions.md]
   - Create smartphone-style UI for messaging
   - Implement chat interface with message history
   - Add notification system for new messages
   - Create mission and information panels

3. **Mission System** - [ai-narrator-and-missions.md]
   - Implement objective tracking and progress validation
   - Create branching narrative logic
   - Add reward and consequence systems
   - Implement mission state persistence

4. **Profile Management** - [save-load-account.md]
   - Create profile selection and management UI
   - Implement profile-specific save organization
   - Add profile statistics and progress tracking
   - Create profile switching functionality

5. **Tutorial & Onboarding** - [tutorial-onboarding.md]
   - Implement interactive tutorial flow
   - Create adaptive guidance system
   - Add controls help and accessibility introduction
   - Implement tutorial state tracking

### Dependencies & Blockers

- **Dependencies**: Sprint 2 completion, voice-over assets
- **Blockers**: Narrative content creation, tutorial flow complexity

### Deliverables

- Complete AI narrator system
- Functional virtual phone interface
- Mission progression system
- Profile management interface
- Interactive tutorial system

### Asset Integration

- **Free Assets**:
  - Voice-over: Text-to-speech or volunteer recording
  - UI elements: Unity UI Toolkit components
  - Audio: Freesound.org (notification sounds, interface sounds)
  - Graphics: Custom UI icons and elements

### Exit Criteria

**GIVEN** a story event occurs
**WHEN** the narrator responds
**THEN** appropriate message appears through correct channel

**GIVEN** the player opens the phone
**WHEN** they navigate the interface
**THEN** all functions work smoothly with proper navigation

**GIVEN** a mission is active
**WHEN** objectives are completed
**THEN** progress updates correctly and new objectives appear

---

## Sprint 4: Audio/Visual Polish

**Focus**: Enhance atmosphere with professional audio and visual effects
**Duration**: 2-3 weeks
**Priority**: Immersion and quality enhancement

### Task List

1. **Audio Integration** - [audio-assets.md]
   - Implement FMOD Studio or Unity audio middleware
   - Create ambient tension system with dynamic mixing
   - Add interaction SFX with proper audio feedback
   - Implement voice-over integration and timing

2. **Visual Effects System** - [advanced-vfx-atmosphere.md]
   - Create particle effects for electrical and atmospheric elements
   - Implement dynamic lighting and shadow systems
   - Add post-processing effects for atmosphere
   - Create environmental storytelling VFX

3. **Atmospheric Design** - [advanced-vfx-atmosphere.md]
   - Implement dynamic weather and environmental effects
   - Add procedural atmosphere variation
   - Create environmental storytelling elements
   - Implement "unknown" entity visual integration

4. **UI Polish & Animation** - [ui-menus-settings.md]
   - Add smooth transitions and animations to all UI
   - Implement visual feedback for all interactions
   - Create loading screens and transitions
   - Add UI sound effects and haptic feedback

5. **Performance Optimization** - [performance-debug-tools.md]
   - Implement LOD systems for 3D models
   - Add occlusion culling and spatial partitioning
   - Optimize particle effects and lighting
   - Create quality settings for different hardware

### Dependencies & Blockers

- **Dependencies**: Sprint 3 completion, audio middleware setup
- **Blockers**: Asset quality standards, performance optimization complexity

### Deliverables

- Complete audio system with atmospheric design
- Professional visual effects and atmosphere
- Polished UI with smooth animations
- Optimized performance across target hardware
- Loading screens and transitions

### Asset Integration

- **Free Assets**:
  - Audio: Freesound.org, FMOD Studio (free tier)
  - VFX: Unity Particle Systems, custom shaders
  - Textures: Poly Haven (high-quality PBR textures)
  - Models: Sketchfab Free (detailed environment pieces)

### Exit Criteria

**GIVEN** the player enters a new area
**WHEN** the audio system responds
**THEN** appropriate ambient audio plays with smooth transitions

**GIVEN** visual effects are triggered
**WHEN** they play
**THEN** effects run smoothly without performance impact

**GIVEN** UI elements are interacted with
**WHEN** animations play
**THEN** transitions are smooth and responsive

---

## Sprint 5: Quality & Accessibility

**Focus**: Final polish, accessibility features, and quality assurance
**Duration**: 2-3 weeks
**Priority**: Professional quality and inclusive design

### Task List

1. **Accessibility Features** - [advanced-accessibility.md]
   - Implement colorblind support with multiple modes
   - Add high contrast mode options
   - Create subtitle system with customization
   - Implement input remapping and alternative controls

2. **Advanced Save/Load** - [save-load-account.md]
   - Add auto-save system with rotating slots
   - Implement quick-save functionality
   - Create cloud sync compatibility framework
   - Add save corruption protection and recovery

3. **Challenge Modes** - [challenge-modes.md]
   - Implement time attack challenges
   - Create completion percentage tracking
   - Add difficulty-based modifiers
   - Implement leaderboards and statistics

4. **Comprehensive QA Tools** - [performance-debug-tools.md]
   - Add automated testing framework
   - Create performance benchmarking tools
   - Implement bug reporting system
   - Add gameplay analytics and metrics

5. **Final Polish & Optimization** - [performance-debug-tools.md]
   - Conduct performance profiling and optimization
   - Fix critical bugs and polish rough edges
   - Optimize memory usage and loading times
   - Finalize asset compression and packaging

### Dependencies & Blockers

- **Dependencies**: Sprint 4 completion, comprehensive testing
- **Blockers**: Accessibility implementation complexity, final bug discovery

### Deliverables

- Complete accessibility support
- Advanced save/load features
- Challenge modes and replayability
- Comprehensive QA and debug tools
- Optimized, polished final build

### Asset Integration

- **Free Assets**:
  - Fonts: Google Fonts (readable, accessible options)
  - Audio: Additional SFX for accessibility feedback
  - UI: Additional accessibility interface elements
  - Tools: Unity Asset Store (testing and analytics tools)

### Exit Criteria

**GIVEN** accessibility options are enabled
**WHEN** players with different needs play
**THEN** all game content is accessible and usable

**GIVEN** the game is running on target hardware
**WHEN** performance is monitored
**THEN** 60 FPS is maintained consistently

**GIVEN** comprehensive testing is conducted
**WHEN** critical bugs are identified
**THEN** all issues are resolved or documented

---

## Dependency Sequencing

### Critical Path Dependencies

```
Sprint 0 → Sprint 1 → Sprint 2 → Sprint 3 → Sprint 4 → Sprint 5
```

### Parallel Development Opportunities

- **Asset Pipeline**: Can run parallel to all sprints
- **Audio Production**: Can begin during Sprint 2
- **Narrative Writing**: Can begin during Sprint 1
- **QA Testing**: Can begin during Sprint 3

### Integration Points

| Sprint | Key Integrations | Risk Level |
|--------|------------------|------------|
| **Sprint 0** | Unity setup, core architecture | Low |
| **Sprint 1** | Save/Load, inventory, basic AI | Medium |
| **Sprint 2** | Combat system, advanced AI | High |
| **Sprint 3** | AI narrator, mission system | Medium |
| **Sprint 4** | Audio middleware, VFX systems | High |
| **Sprint 5** | Accessibility, optimization | Medium |

---

## Risk Mitigation

### High-Risk Areas

1. **Combat System Complexity**
   - **Risk**: Combat mechanics too complex for timeline
   - **Mitigation**: Start with simple combat, expand based on testing
   - **Fallback**: Simplify to unarmed combat only

2. **AI Narrator Implementation**
   - **Risk**: Context-aware AI too ambitious
   - **Mitigation**: Implement scripted narrator first, add context later
   - **Fallback**: Pre-scripted narrator with basic triggers

3. **Performance Optimization**
   - **Risk**: Cannot maintain 60 FPS on target hardware
   - **Mitigation**: Profile early, optimize throughout development
   - **Fallback**: Reduce visual quality or target 30 FPS

4. **Asset Acquisition**
   - **Risk**: Free assets insufficient for quality target
   - **Mitigation**: Budget allocated for essential paid assets
   - **Fallback**: Use placeholder assets, upgrade later

### Medium-Risk Areas

1. **Save/Load System**
   - **Risk**: Data corruption or complexity issues
   - **Mitigation**: Use proven JSON serialization, test extensively

2. **UI/UX Complexity**
   - **Risk**: Interface too complex for players
   - **Mitigation**: User testing throughout development

3. **Narrative Integration**
   - **Risk**: Story doesn't integrate well with gameplay
   - **Mitigation**: Iterative testing and adjustment

### Contingency Planning

- **Time Buffers**: 20% additional time per sprint for unexpected issues
- **Feature Prioritization**: Core features must be completed before optional features
- **Asset Backup**: Multiple sources for critical assets
- **Technical Debt**: Accept some technical debt to meet timeline, document for future cleanup

---

## Performance Gates

### Gate 1: Sprint 0 Completion
- **Requirements**: Project runs, basic character moves
- **Performance**: 60+ FPS in empty scene
- **Memory**: < 1GB RAM usage
- **Stability**: No crashes in 10-minute test

### Gate 2: Sprint 1 Completion
- **Requirements**: Complete gameplay loop playable
- **Performance**: 60+ FPS with basic interactions
- **Memory**: < 2GB RAM usage
- **Stability**: No crashes in 30-minute play session

### Gate 3: Sprint 2 Completion
- **Requirements**: Combat and advanced systems functional
- **Performance**: 60+ FPS with combat active
- **Memory**: < 3GB RAM usage
- **Stability**: No crashes in 1-hour play session

### Gate 4: Sprint 3 Completion
- **Requirements**: Narrative and progression complete
- **Performance**: 60+ FPS with all systems active
- **Memory**: < 4GB RAM usage
- **Stability**: No crashes in 2-hour play session

### Gate 5: Sprint 4 Completion
- **Requirements**: Audio/visual polish complete
- **Performance**: 60+ FPS on target hardware
- **Memory**: < 4GB RAM usage
- **Stability**: No crashes in extended testing

### Gate 6: Sprint 5 Completion
- **Requirements**: All features complete and optimized
- **Performance**: 60+ FPS on minimum spec hardware
- **Memory**: < 4GB RAM usage
- **Stability**: Zero crashes in QA testing

---

## Related Documents

- [overview.md] - Project overview and workstreams
- [core-systems.md] - First-person controller and core architecture
- [combat-movement.md] - Combat mechanics and movement systems
- [npc-procedural.md] - NPC AI and procedural generation
- [save-load-account.md] - Save/load and account management
- [ai-narrator-and-missions.md] - AI narrator and mission systems
- [ui-menus-settings.md] - User interface and settings
- [weapons-tools-system.md] - Weapons and equipment systems
- [audio-assets.md] - Audio design and integration
- [advanced-vfx-atmosphere.md] - Visual effects and atmosphere
- [tutorial-onboarding.md] - Tutorial and player onboarding
- [advanced-accessibility.md] - Accessibility features
- [performance-debug-tools.md] - Performance monitoring and debug tools
- [challenge-modes.md] - Additional gameplay modes

---

## Conclusion

This build roadmap provides a structured approach to developing Protocol EMR from foundation to polished prototype. By following the sprint sequence and maintaining focus on core gameplay first, the project can achieve its quality targets while managing scope and risk effectively.

The emphasis on free and low-cost assets, combined with a clear dependency structure and performance gates, ensures the project remains achievable within typical indie development constraints while delivering a professional-quality experience.

Regular progress reviews against the sprint deliverables and performance gates will help identify issues early and keep the project on track for successful completion.