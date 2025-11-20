# Build & Coding Roadmap

## Overview

This roadmap defines the sprint-by-sprint coding plan for Protocol EMR. Every sprint contains four to five concrete gameplay-first tasks that ship testable slices of player value while respecting integration, dependency, QA, and performance requirements. Milestones are earned by progressively combining sprint deliverables into wider gameplay loops, ensuring that playtests, narrative reviews, and platform checks remain in lockstep.

## Development Philosophy

- **Gameplay First**: Input responsiveness, combat feel, UI clarity, and AI believability always outrank feature breadth. Every sprint produces something that can be playtested immediately.
- **Incremental Delivery**: Systems are layered with clear integration points so that each sprint’s code merges smoothly into the previous sprint’s foundation.
- **Quality Gates**: Testing/QA expectations and performance targets accompany every task to keep the prototype shippable and regression-free.
- **Cross-Team Visibility**: Design, audio, narrative, and platform contributors receive explicit handoffs so they can author content the moment systems go live.

## Sprint Overview Matrix

| Sprint | Goal Focus | Key Metrics | Cross-Team Handoffs | Milestone Contribution |
|--------|------------|-------------|----------------------|-------------------------|
| 1. Setup & Foundation | Stand up repo, CI, input, camera, telemetry | Clean CI pipeline, controller latency < 40 ms | Tools + Ops for pipelines, Anim/Design for movement spec review | Establishes groundwork toward **Alpha 1** |
| 2. Core Systems | Ship playable locomotion, interaction, saves | 60 FPS in greybox arena, 100% save/load integrity | Gameplay design for traversal tuning, QA for regression suite | Unlocks **Alpha 1** core loop |
| 3. UI/HUD | Deliver neural HUD, prompts, accessibility shell | HUD render cost < 1.5 ms, UX bug count < 5 | UX/Accessibility for heuristics, Audio for UI cues | Sets stage for **Alpha 2** |
| 4. Inventory System | Implement inventory data + UI + crafting hooks | Item swap < 200 ms, inventory desync rate 0 | Narrative for item lore, Economy design for crafting values | Completes **Alpha 2** |
| 5. Phone System | Build AI phone UI and notification stack | Message latency < 500 ms, drop rate 0 | Narrative for scripts, Audio for VO stingers | Prepares **Beta 1** |
| 6. NPC Framework | Authorable NPC AI, nav, relationship logic | 50 NPCs @ 60 FPS, behavior tree success > 95% | Animation for rigs, Narrative for dialogue trees | Achieves **Beta 1** |
| 7. AI Narrator | Context engine, VO playback, choice tracking | VO sync drift < 100 ms, decision latency < 150 ms | Narrative director for persona tuning, Audio for mix | Enables **Beta 2** |
| 8. Mission Systems | Objective graphs, branching outcomes, rewards | Mission pass rate analytics live, branching load < 5 ms | Mission design for graph authoring, Analytics for hooks | Achieves **Beta 2** |
| 9. Procedural Generation | Seeded levels, dynamic events, procedural audio | Gen time < 8 s, streaming hitches < 2 ms | Level design for tile sets, Audio for generative cues | Builds **Release Candidate** feature set |
| 10. Audio & Polish | Final mix, rendering polish, perf + bug closure | 60 FPS on target HW, crash rate < 0.5% | QA for certification, Production for release checklist | Ships **Gold Master** |

## Phase 1: Foundation & Setup

### Sprint 1: Setup & Foundation (Workstream 1)
Goal: Stand up stable tooling and the minimal playable shell so later gameplay work plugs in without rework.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Repository & CI Hardening | Configure Git branching, Git LFS for assets, and gated CI (build + lint) | Hooks into build agents, documentation repo links | None | CI must run green on two successive commits; lint warnings = 0 | CI throughput < 10 min per run |
| Player Controller Skeleton | Implement input mapping, movement state machine scaffold, and physics collision shells | Hooks to combat-movement module, telemetry bus | Unity input stack, physics layers | Controller smoke tests on KB/M + gamepad | Input latency < 40 ms end-to-end |
| Camera Rig Baseline | Third-person camera with obstruction handling and FOV presets | Integrates with player controller and upcoming cinematic cues | Player controller skeleton | Automated camera jitter tests; manual comfort review | Camera update costs < 0.8 ms per frame |
| Telemetry & Diagnostics Hooks | Instrument frame time, memory, and input events to central dashboard | Links to analytics service and in-editor overlay | Repo + CI configuration | QA verifies telemetry packets in staging | Telemetry overhead < 0.5 ms |

**Acceptance Criteria**
- GIVEN the project template, WHEN the repo is cloned THEN CI builds succeed without manual setup.
- GIVEN a gamepad, WHEN the player avatar is moved in the greybox THEN camera follow and collision remain stable.
- GIVEN telemetry is enabled, WHEN a 5-minute play session occurs THEN input, FPS, and memory metrics stream to the dashboard with no drops.

**Milestone Roll-up:** Provides the tooling and control scaffolding required for **Alpha 1** readiness in Sprint 2.

## Phase 2: Core Systems (Gameplay Foundation)

### Sprint 2: Core Systems (Workstream 2)
Goal: Deliver the first fully playable loop featuring locomotion, interaction, and persistence.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Locomotion Refinement & Parkour | Implement sprinting, vaulting, wall interactions, and stamina hooks | Uses controller skeleton, animation placeholders, combat-movement spec | Sprint 1 controller | Movement regression suite + animation review | 60 FPS during traversal stress path |
| Physics & Interaction Layer | Author collision matrix, raycast interactables, and IK alignment for pickups | Integrates with inventory system placeholder, mission triggers | Locomotion refinement | QA verifies 20 interaction cases; zero tunneling defects | Physics step < 2 ms |
| Save/Load Slice | Serialize player stats, inventory stub, and world toggles; add auto-save rotation | Hooks to save-load-account spec, telemetry | Repo encryption libs | Automated integrity tests; corrupt save recovery manual test | Save/Load under 500 ms |
| Debug & Developer Tools | Build free-fly cam, console commands, and feature flags for rapid validation | Linked to QA harness and designers | Controller + telemetry | QA validates 15 debug commands; designers sign off | Tool overlays < 1 ms |

**Acceptance Criteria**
- GIVEN traversal spaces, WHEN sprinting, vaulting, or sliding THEN animations and physics remain synchronized.
- GIVEN interactable props, WHEN inspecting or picking up items THEN state persists after save/load.
- GIVEN a build triggers auto-save, WHEN a crash is simulated THEN latest save restores without corruption.

**Milestone Roll-up:** Completes the **Alpha 1** milestone by shipping a replayable locomotion + interaction loop.

## Phase 3: Player Interface & Interaction

### Sprint 3: UI/HUD (Workstream 3)
Goal: Visualize player state through the neural HUD and provide contextual prompts with accessibility options.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Neural HUD Framework | Implement health, focus, stamina bars with diegetic styling | Hooks to player stats service and VFX cues | Sprint 2 stats data | UI automation verifying states; art review | HUD draw < 1.5 ms |
| Contextual Prompt System | Build radial prompts and interaction callouts with localization keys | Integrates with interaction layer + mission hooks | Interaction layer | QA verifies prompts in 10 locales; accessibility review | Prompt spawn < 50 ms |
| Accessibility Toggles | Add colorblind palettes, text-to-speech hooks, input remap UI | Ties to settings backend, phone system preview | UI framework | Accessibility QA pass; compliance checklist | Settings load < 200 ms |
| UI Event Bus | Centralize HUD messaging, debounced updates, and notification priorities | Connects UI, mission status, narrator | Telemetry + interaction | Unit tests for event ordering; stress UI spam scenario | Event dispatch < 1 ms |

**Acceptance Criteria**
- GIVEN varying player health, WHEN thresholds are crossed THEN HUD animates and remains legible in all palettes.
- GIVEN interactables, WHEN the player approaches THEN localized prompts appear and honor accessibility toggles.
- GIVEN 30 minutes of play, WHEN UI events burst THEN no frame hitch above 2 ms occurs.

**Milestone Roll-up:** Establishes UI foundations needed for **Alpha 2** and unlocks UX/narrative teams to author content.

### Sprint 4: Inventory System (Workstream 4)
Goal: Deliver the inventory gameplay loop with data, UI, crafting, and equipment integration.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Item Database & Tagging | Define item schema, rarity, stack rules, and metadata service | Connects to mission rewards, loot drops, and UI | Save/load slice | Data validation tests; design sign-off | DB queries < 0.3 ms |
| Inventory UI + Drag/Drop | Grid + radial inventory with drag/drop, compare stats, quick-equip | Hooks to HUD framework, controller input | UI bus | UX heuristic eval; 25 manual equip tests | UI input latency < 120 ms |
| Crafting & Combination System | Implement recipes, success checks, and fail feedback | Links to item DB, mission scripting | Item DB | Crafting unit tests; telemetry of usage | Craft calc < 50 ms |
| Equipment & Loadout Sync | Sync equipped items to player stats, visuals, and save data | Integrates with combat module, animation overlays | Item DB + UI | QA verifies persistence across reloads; no stat drift | Equip change < 200 ms |
| Visual Feedback Hooks | Add audio/VFX cues for pickup, craft, equip states | Connects Audio sprint 10 backlog, VFX pipeline | Inventory UI | Sensory QA for brightness/audio mix | Effect budget < 0.5 ms |

**Acceptance Criteria**
- GIVEN crafting materials, WHEN recipes are fulfilled THEN crafted items appear with correct metadata and persist after reload.
- GIVEN loadout changes, WHEN switching weapons or gear THEN stats update across HUD and combat calculations.
- GIVEN inventory is full, WHEN additional items are collected THEN overflow messaging occurs without duplication bugs.

**Milestone Roll-up:** Completes **Alpha 2** by enabling end-to-end inventory and UI gameplay.

## Phase 4: Communication & NPC Systems

### Sprint 5: Phone System (Workstream 5)
Goal: Implement the in-world phone interface that carries narrator messages, alerts, and mission updates.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Messaging UI Shell | Build phone UI, inbox views, threaded conversations | Integrates with UI bus, localization, audio cues | UI framework | UX review, multi-input tests | UI render cost < 1.2 ms |
| Notification Scheduler | Manage push notifications, urgency buckets, do-not-disturb windows | Hooks to mission + narrator events | Messaging shell | QA tests 30 notification permutations | Delivery latency < 500 ms |
| Voice/Text Bridge | Route narrator VO + text transcripts with sync markers | Connects to audio middleware, subtitles | Notification scheduler | Audio QA verifying sync, caption accuracy | VO drift < 80 ms |
| Message History & Search | Indexed storage, filters, search suggestions | Links to save system, analytics | Save/Load | Functional tests on 5k message dataset | Query time < 100 ms |

**Acceptance Criteria**
- GIVEN mission updates, WHEN a trigger fires THEN phone notifications appear with correct urgency and VO alignment.
- GIVEN archives, WHEN searching keywords THEN relevant threads return within 100 ms.
- GIVEN accessibility toggles, WHEN narrator messages arrive THEN both audio and text channels honor user preferences.

**Milestone Roll-up:** Provides narrative communication rails required ahead of **Beta 1** NPC integration.

### Sprint 6: NPC Framework (Workstream 6)
Goal: Enable fully interactive NPCs with navigation, dialogue, and relationship tracking.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Character Data Graph | Define NPC archetypes, stats, schedules, and dialogue keys | Links to narrative DB, mission triggers | Item DB, phone logs | Data validation + tooling export | Data fetch < 0.5 ms |
| Behavior Tree Runtime | Implement modular behavior trees with decorators for perception and combat | Integrates with AI narrator hooks, mission objectives | Character data, locomotion | 100% unit coverage on decorators; QA sandbox tests | BT tick budget < 2 ms per NPC |
| NavMesh + Crowd Control | Author nav data, dynamic obstacle avoidance, group behaviors | Hooks to level streaming, locomotion | Behavior runtime | Stress test with 50 NPCs; no stuck agents | Path recompute < 80 ms |
| Relationship & Reputation Service | Track player choices, NPC attitudes, unlocks | Ties to mission graphs, phone notifications | Character data | QA runs branching attitude tests; persistence verified | Update < 20 ms |
| NPC Animation Bridge | Blend trees, facial cues, syncing to BT states | Connects to animation rigging + VO cues | Behavior runtime | Animation QA pass; lip-sync review | Blend evaluation < 1 ms |

**Acceptance Criteria**
- GIVEN patrol NPCs, WHEN obstacles appear THEN they re-route without jitter or stalls.
- GIVEN player choices, WHEN dialogue concludes THEN relationship scores adjust and persist.
- GIVEN simultaneous VO and animation triggers, WHEN NPCs emote THEN lip-sync and gestures stay synchronized.

**Milestone Roll-up:** Achieves **Beta 1** by delivering interactive NPCs tied to communication systems.

## Phase 5: Narrative & Mission Systems

### Sprint 7: AI Narrator (Workstream 7)
Goal: Deliver the adaptive narrator brain that contextualizes events and drives immersion.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Context Engine & Blackboards | Aggregate world + player state, mission flags, and NPC cues | Hooks to mission tracker, telemetry | Relationship service, mission data | Unit tests per context rule; scenario validation | Rule eval < 1 ms |
| Dialogue Selection & Personality Tuning | Weighted selection, cooldowns, emotional arcs | Connects to phone system, VO bank | Context engine | Narrative review board; AB tests for tone | Selection latency < 150 ms |
| VO Playback & Lipsync Sync | Manage VO queues, subtitle sync, fallback TTS | Integrates with audio middleware, NPC animation | Phone system, animation bridge | Audio QA, multi-language checks | Sync drift < 100 ms |
| Choice Tracking & Consequences | Log player responses, branch narrator lines, expose data to missions | Links to mission graphs, analytics | Relationship service | QA verifies 20 branching cases; data exported | Logging overhead < 0.3 ms |

**Acceptance Criteria**
- GIVEN concurrent mission events, WHEN narrator commentary is requested THEN rules prioritize the most urgent beat without repeats.
- GIVEN localized VO, WHEN lines play THEN subtitles and lip-sync remain aligned within tolerance.
- GIVEN player choices, WHEN consequences trigger THEN mission and reputation systems receive the updates.

**Milestone Roll-up:** Powers the adaptive storytelling layer needed for **Beta 2**.

### Sprint 8: Mission Systems (Workstream 8)
Goal: Provide designers mission authoring, branching progression, and reward pipelines.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Mission Graph Authoring Tool | Node-based editor with validation and export | Integrates with narrative DB, analytics | Context engine | Tooling QA, designer training | Graph compile < 2 s |
| Objective Tracker Runtime | Runtime state machine with fail/ retry logic | Hooks to UI bus, phone notifications | Mission graph tool | Automated mission sim; manual fail-state tests | State update < 0.5 ms |
| Branching Outcome Resolver | Evaluate conditions, trigger rewards, unlock paths | Linked to inventory, reputation, narrator | Objective tracker | QA tests 15 branching arcs; telemetry logging | Branch eval < 1 ms |
| Reward & Economy Hooks | Grant XP, currency, items, narrative tokens | Connects to inventory, analytics | Inventory + item DB | Balance review; dupe prevention tests | Reward grant < 50 ms |
| Mission Editor Collaboration Flow | Versioning, conflict resolution, and content linting | Links to repo + CI, mission authors | Graph tool | Content lint ensures 0 errors before merge | Lint runtime < 30 s per pack |

**Acceptance Criteria**
- GIVEN authored mission graphs, WHEN exported THEN runtime loads without validation errors.
- GIVEN branching objectives, WHEN players succeed or fail THEN rewards, narrator beats, and reputation adjustments fire correctly.
- GIVEN collaborative edits, WHEN mission files merge THEN lint catches conflicts before integration.

**Milestone Roll-up:** Completes **Beta 2** by enabling full mission + narrative delivery.

## Phase 6: Content Generation & Polish

### Sprint 9: Procedural Generation (Workstream 9)
Goal: Introduce replayable procedural spaces, events, and audio layers that respect performance caps.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Seed Manager & Persistence | Deterministic seed assignment, sharing, recovery | Hooks to save system, analytics | Mission + save data | Unit tests for seed determinism; QA cross-play | Seed calc < 10 ms |
| Level Chunk Generator | Modular tile assembly, adjacency rules, hazard placement | Integrates with navmesh baking, mission hooks | Seed manager, navmesh | 100 automated generation runs; manual fun-factor review | Generation time < 8 s |
| Dynamic Event Orchestrator | Spawn ambient encounters, combat waves, puzzles | Links to AI narrator, mission tracker | Seed + AI systems | QA scenario coverage; telemetry instrumentation | Runtime scheduling < 1 ms |
| Procedural Audio Layer | Blend generative ambiences tied to seeds + events | Hooks to audio middleware, telemetry | Audio assets | Audio QA for phasing; platform mix tests | Audio CPU < 5% |
| Streaming & Performance Guardrails | Async loading, LOD gates, hitch detection | Integrates with telemetry, render pipeline | Generator + engine | Perf QA on target HW; hitch budget tests | Frame hitches < 2 ms | 

**Acceptance Criteria**
- GIVEN a seed, WHEN levels regenerate THEN geometry, NPC placement, and events match deterministic expectations.
- GIVEN procedural encounters, WHEN players revisit THEN narrator reacts uniquely yet within performance budgets.
- GIVEN streaming thresholds, WHEN running on target hardware THEN no hitch exceeds 2 ms during traversal.

**Milestone Roll-up:** Delivers the feature completeness required for the **Release Candidate** build.

### Sprint 10: Audio & Polish (Workstream 10)
Goal: Finalize audio mix, rendering polish, optimization, and bug burn-down for release readiness.

| Task | Description | Integration Points | Dependencies | Testing & QA Expectations | Performance Targets |
|------|-------------|--------------------|--------------|---------------------------|----------------------|
| Final Audio Mix & Mastering | Balance VO, SFX, music, dynamic range for platforms | Hooks to procedural audio, narrator, combat | Audio assets from previous sprints | Audio regression suite; platform loudness cert | Mix headroom within ±1 dB target |
| Rendering & VFX Polish | Shader refinements, lighting passes, cinematic post | Integrates with HUD, procedural content | VFX + rendering pipeline | Visual QA, HDR calibration | Maintain 60 FPS @ target HW |
| Performance Optimization Pass | Profile CPU/GPU, fix hotspots, memory budget | Links to telemetry, streaming guards | All gameplay systems | Perf QA with soak tests; leak detection | FPS >= 60, memory < 3.5 GB |
| Bug Burn-down & Certification Prep | Triage remaining bugs, finalize release checklist, package builds | Handoffs to QA, production, platform partners | All prior sprints complete | Zero high-sev defects open; cert checklist signed | Crash rate < 0.5% |
| Player Experience Validation | End-to-end playtests, accessibility audits, tutorial polish | Connects UI, missions, narrator | Finished content | UX/Narrative sign-off; accessibility audit pass | Tutorial completion > 90% |

**Acceptance Criteria**
- GIVEN the release candidate build, WHEN QA runs certification suites THEN no blocking issues remain.
- GIVEN accessibility guidelines, WHEN audits run THEN all mandatory checks pass without waivers.
- GIVEN representative hardware, WHEN playtests run for 60 minutes THEN FPS never dips below 60 and no crashes occur.

**Milestone Roll-up:** Ships the **Gold Master** build and hands off to publishing.

## Key Milestones

| Milestone | Phase | Description |
|-----------|-------|-------------|
| **Alpha 1** | After Sprint 2 | Playable core loop with locomotion, interaction, and persistence |
| **Alpha 2** | After Sprint 4 | UI + Inventory systems powering sandbox missions |
| **Beta 1** | After Sprint 6 | Full NPC interaction layer with communication stack |
| **Beta 2** | After Sprint 8 | Narrative-complete build with missions and narrator integration |
| **Release Candidate** | After Sprint 9 | Procedural replayability with full content breadth |
| **Gold Master** | After Sprint 10 | Polished, optimized, and certified build |

## Task Distribution & Success Criteria

- **Average tasks per sprint**: 4–5 highly focused coding tasks with explicit integration notes.
- **Integration cadence**: Each sprint concludes with a shippable branch, telemetry review, and milestone alignment statement.
- **Testing discipline**: Automated suites, targeted manual QA, and accessibility reviews are defined alongside tasks.
- **Performance vigilance**: Every task lists budget numbers so regression spikes are caught immediately.
- **Cross-team clarity**: Overview matrix specifies who receives each sprint’s output to accelerate content production.

## Related Documentation

- [Protocol EMR Overview](./overview.md)
- [Combat & Movement](./combat-movement.md)
- [NPC Framework](./npc-procedural.md)
- [Save/Load System](./save-load-account.md)
- [AI Narrator & Missions](./ai-narrator-and-missions.md)
- [Audio & Assets Protocol](./audio-assets.md)
