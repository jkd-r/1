# Build & Coding Roadmap

## Overview

This document outlines the development roadmap for Protocol EMR, organized into development phases that prioritize gameplay first and ensure consistent, measurable progress. Each phase contains focused tasks that build upon previous work, with a target of 5 or fewer tasks per phase to maintain quality and momentum.

## Development Philosophy

**Gameplay First**: Every feature and system prioritizes player experience and narrative immersion over technical complexity. Core gameplay mechanics are established early and refined iteratively.

**Incremental Delivery**: Each phase delivers functional, testable features that can be integrated into the prototype and evaluated with playtesters.

**Quality Over Scope**: Limiting tasks per phase ensures thorough implementation, testing, and polish rather than attempting too many features simultaneously.

## Sprint Structure

Protocol EMR development is organized into 10 core workstreams grouped into logical phases:

### Phase 1: Foundation & Setup
- **Sprint 1: Setup** (Workstream 1)
  - Establish development environment and core project structure
  - Unity project setup with version control
  - Build pipeline and CI/CD configuration
  - Team collaboration tools and documentation
  - Core project architecture and folder structure

### Phase 2: Core Systems (Gameplay Foundation)
- **Sprint 2: Core Systems** (Workstream 2)
  - Implement fundamental gameplay mechanics
  - Player movement and interaction systems
  - Physics and collision detection
  - Save/load functionality
  - State management and event systems
  - Debug and development tools

### Phase 3: Player Interface & Interaction
- **Sprint 3: UI/HUD** (Workstream 3)
  - Create intuitive neural interface visualization
  - Health and status indicators
  - Inventory and item management displays
  - System access terminals and interfaces
  - Accessibility options and settings

- **Sprint 4: Inventory System** (Workstream 4)
  - Item pickup and storage mechanics
  - Item categories and properties
  - Crafting and combination systems
  - Key item and progression item tracking
  - Visual feedback for item interactions

### Phase 4: Communication & NPC Systems
- **Sprint 5: Phone System** (Workstream 5)
  - AI narrator messaging interface
  - Character dialogue system
  - Notification and alert management
  - Message history and search functionality
  - Visual and audio integration

- **Sprint 6: NPC Framework** (Workstream 6)
  - Character data and behavior systems
  - Dialogue tree implementation
  - Character relationship tracking
  - Animation and visual representation systems
  - AI behavior and pathfinding

### Phase 5: Narrative & Mission Systems
- **Sprint 7: AI Narrator** (Workstream 7)
  - Context-aware response generation
  - Event-driven commentary triggers
  - Voice-over integration and timing
  - Personality and tone management
  - Player choice consequence tracking

- **Sprint 8: Missions** (Workstream 8)
  - Mission structure and objective tracking
  - Branching narrative logic
  - Progress validation and completion detection
  - Reward and consequence systems
  - Mission editor tools for content creation

### Phase 6: Content Generation & Polish
- **Sprint 9: Procedural Generation** (Workstream 9)
  - Randomized puzzle elements
  - Environmental variation systems
  - Dynamic event triggers
  - Procedural audio generation
  - Content balancing and difficulty scaling
  - End-to-end procedural story generation pipeline
  - Seed management and deterministic reproducibility
  - AI narrator coupling with dynamic narrative adaptation
  - See **[Procedural Story Generation](./procedural-story-generation.md)** for complete system specifications

- **Sprint 10: Audio & Polish** (Workstream 10)
  - Audio integration and mixing
  - Visual effects and shader implementation
  - Performance optimization
  - Quality assurance and bug fixing
  - Final presentation and user experience refinement

## Key Milestones

| Milestone | Phase | Description |
|-----------|-------|-------------|
| **Alpha 1** | After Sprint 2 | Playable core loop with movement, interaction, and basic puzzles |
| **Alpha 2** | After Sprint 4 | Full inventory and UI systems with item interactions |
| **Beta 1** | After Sprint 6 | NPC interactions and dialogue systems fully functional |
| **Beta 2** | After Sprint 8 | Complete mission structure and narrative branching |
| **Release Candidate** | After Sprint 9 | Procedural content and full replay value |
| **Gold Master** | After Sprint 10 | Polished, optimized, ready for release |

## Task Distribution

- **Average tasks per sprint**: 4-5 focused development tasks
- **Quality focus**: Each sprint includes testing, integration, and documentation time
- **Iteration**: Each phase builds on previous systems with integration points clearly defined
- **Flexibility**: Phases can be adjusted based on playtester feedback and technical discoveries

## Success Criteria

Development success is measured by:

1. **Gameplay delivery**: Each phase delivers functional, testable gameplay
2. **Technical stability**: No regressions in previously completed systems
3. **Performance maintenance**: Frame rate targets maintained throughout development
4. **Playtester feedback**: Consistent improvement in user satisfaction scores
5. **Schedule adherence**: Each phase completes within estimated timeframe

## Related Documentation

- [Protocol EMR Overview](./overview.md) - Project concept and high-level design
- [Core Systems](./core-systems.md) - Foundation gameplay mechanics
- [NPC Framework](./npc-procedural.md) - Character and AI systems
- [Save/Load System](./save-load-account.md) - State persistence and progression
- [AI Narrator & Missions](./ai-narrator-and-missions.md) - Narrative systems
- [Procedural Story Generation](./procedural-story-generation.md) - End-to-end procedural narrative system
- [Combat & Movement](./combat-movement.md) - Advanced gameplay mechanics
