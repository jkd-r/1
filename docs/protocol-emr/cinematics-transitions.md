# Cinematics and Transition Systems

This document defines the cinematic language, transition rules, and implementation requirements that make Protocol EMR feel polished, cohesive, and immersive. The focus is on delivering seamless movement between zones, high-impact event cinematics, and precise audio-visual synchronization backed by robust tooling inside Unity.

## Table of Contents

1. [Zone Transition Cinematics](#zone-transition-cinematics)
2. [Event Cinematics](#event-cinematics)
3. [Camera Systems](#camera-systems)
4. [Post-Processing Effects](#post-processing-effects)
5. [Audio-Visual Sync](#audio-visual-sync)
6. [Puzzle & Story Integration](#puzzle--story-integration)
7. [Technical Implementation Guide](#technical-implementation-guide)
8. [Prototype Deliverables & Acceptance Criteria](#prototype-deliverables--acceptance-criteria)

## Zone Transition Cinematics

Zone transitions must avoid hard cuts and instead layer motion, audio, and UI feedback into a single cinematic moment. Every transition should complete within 3-5 seconds while conveying the new zone's tone.

### Door Opening Sequences

| Component | Description | Implementation Notes |
|-----------|-------------|----------------------|
| **Animated Door Rig** | Multi-bone rig with hinges, locking pistons, and emissive seams | Driven by Timeline animation tracks with additive detail layers (steam bursts, panel rotation) |
| **Sound Design** | Layered mechanical clanks, servo whines, and pressure equalization | Trigger FMOD snapshot that transitions from "door_locked" to "door_open" states with crossfade < 250 ms |
| **Camera Alignment** | Framed at 35-50mm focal length for premium feel | Cinemachine Dolly track that slides from player shoulder to door center, easing in/out with cubic curves |

### Camera Dolly/Zoom Effects

- Use Cinemachine Dolly Cart with spline nodes placed before/after the threshold.
- FOV ramps ±10° synchronized with door animation for perceived acceleration.
- Blend hint: `Ease In Out` with 0.4s blend time to maintain player control readability.

### Lighting Transitions

- Blend between **Zone Lighting Profiles** using HDR color grading LUTs.
- Implement transition volumes that lerp exposure, temperature, and fill light within 1.5 seconds.
- Trigger post-processing weight curves tied to door state (`Opening → 0.5`, `Opened → 1`).

### Particle Effects During Transitions

- Emitters: sparks (electric), dust (pressure change), digital glitch (safe → hostile shift).
- Spawn offsets anchored to door frame bones; lifetime capped at 1 second to avoid lingering.
- Particle scripts listen for Timeline signals to align bursts with audio cues.

### Audio Layering Strategy

1. **Door Mechanicals** – close-mic, center-panned.
2. **Ambient Shift** – crossfade between zone ambience buses over 2 seconds.
3. **Music Crossfade** – use FMOD AHDSR to duck outgoing track by -12 dB while fading in new stem.
4. **Subtle Hits** – low-frequency impacts at door unlock moment to reinforce weight.

### HUD Overlay Updates

- Overlay slides from top with 300 ms ease-out animation showing zone name, threat level, and environmental status.
- Display remains for 2.5 seconds and never interrupts chat channels; uses translucent glass UI style.
- HUD receives Timeline signal marker to stay synchronized with transition midpoint.

### NPC Emergence Animations

- NPCs appear through choreographed sequences (e.g., guard stepping through haze, drone unfolding).
- Blend animation layers for weapon ready stance + idle fix to avoid snapping.
- NPC AI stays in "Cinematic Hold" state until Timeline sends `NPC_RELEASE` signal.

### Atmosphere Shift Indicators

- Visual cues: shifting volumetric fog colors, holographic signage updates, or floor LED strips.
- Danger zones: introduce red rim lighting + flickering 3 Hz pulses.
- Safe zones: cooler tones, stable lighting, soft particles.

## Event Cinematics

Event cinematics translate mission-critical beats into short, interactive sequences without removing agency.

### Door/Puzzle Unlock Sequences

- Multi-step animation stacking: lock tumblers → energy surge → door slide.
- Puzzle dependency graph triggers Cinemachine blend to highlight unlocking components.
- Use audio riser culminating in harmonic chime for solved state.

### NPC Encounter Cinematics

- Dialogue-focused sequences leverage shot-reverse-shot coverage.
- Facial blendshapes keyed in Timeline to match voice acting, with eye dart procedural layer.
- Camera anchored to spline that orbits 15° to keep composition dynamic.

### Combat Start Cinematics

- Trigger when hostile awareness crosses 0.8 threshold.
- Play 1-second stuttered zoom onto enemy leader, overlaying HUD warning.
- NPCs shift from "idle" to "combat stance" animation while weapon VFX boot up.

### Puzzle Solve Moments

- Object glow intensity ramps up (bloom + emissive).
- Camera performs 1.2x zoom with slight tilt to emphasize scale.
- Satisfaction feedback: resonant chord, HUD checkmark, small confetti particles.

### Story Beats

- Environmental storytelling via holographic projections, archivist drones, or flashbacks.
- Use Cinemachine blending to cut between vignettes without jarring jump cuts.
- Dialogue text avoided; rely on visuals, VO, and ambient cues.

### Alarm/Emergency Sequences

- Post-processing shifts to red tint, lens distortion at 0.2.
- Screen shake escalates to "medium" preset.
- Audio siren loops layered with building groans and radio chatter.

## Camera Systems

### Smooth Camera Movement

- Cinemachine Brain handles blend tree (dolly, pan, zoom, rotate).
- Dolly tracks authored per zone with named waypoints for reuse.
- Constraint: maintain player proxy at rule-of-thirds for readability.

### First-Person Dynamic Camera

- Head bob amplitude: `walk 0.05 m`, `sprint 0.09 m` vertical displacement.
- Recoil curves: immediate 0.04 rad pitch kick followed by 0.3 s damped return.
- Impact responses share noise profile with screen shake to avoid dissonance.

### Cinematographic Angles

- Use 35mm for intimate dialogue, 24mm for environmental reveals, 50mm for hero shots.
- Auto-composition script aligns horizon and anchors subject to golden ratio lines.

### Screen Shake Intensity Levels

| Level | Use Case | Amplitude | Frequency |
|-------|----------|-----------|-----------|
| **Small** | UI taps, minor impacts | 0.02 | 12 Hz |
| **Medium** | Combat hits, alarm triggers | 0.05 | 9 Hz |
| **Heavy** | Explosions, structural shifts | 0.09 | 6 Hz |

### Depth of Field Effects

- Focus target automatically swapped to highlight object or NPC.
- Aperture stops between f/2.8 (story beats) and f/5.6 (gameplay readability).
- Transition smoothing: 0.4 s to avoid sudden blur jumps.

### Vignette & Chromatic Aberration

- Activate vignette when stress meter > 70% with intensity 0.35.
- Chromatic aberration pulse at 0.1 amplitude during EMP or glitch events.

## Post-Processing Effects

### Motion Blur

- Enabled on Cinemachine impulse events > medium intensity.
- Clamped at shutter angle 180° to retain clarity.

### Bloom/Glow Intensity

- Safe zones: bloom threshold 1.2, intensity 0.6.
- Danger zones: threshold 0.9, intensity 1.1 for heightened tension.

### Color Grading per Zone

| Zone Type | Palette | Narrative Signal |
|-----------|---------|------------------|
| **Safe** | Cool blues, teal highlights | Calm, recuperation |
| **Neutral** | Balanced white, subtle green | Investigation |
| **Danger** | Warm reds/orange, crushed blacks | Hostility |
| **Digital Anomaly** | Magenta shifts, glitch LUT | Reality distortion |

### Film Grain

- Static intensity 0.1 baseline, ramp to 0.25 during flashbacks for archival feel.

### Lens Distortion

- Subtle barrel distortion 0.05 for digital world hints, spikes to 0.15 during glitches.

### Screen Fade Transitions

- Use quad overlay with exponential fade curve (ease-in 0.2 s, hold 0.3 s, ease-out 0.4 s).
- Avoid hard cuts except for deliberate blackout events.

## Audio-Visual Sync

- Particle systems publish timing events; FMOD listens to spawn/impact timestamps for aligned sounds.
- Music swells keyed through Timeline markers; composer provides stems for tension, relief, and climax.
- Silence window (~500 ms) before major reveals to heighten tension.
- Environmental audio layering: maintain base ambience bus, add event-specific emitters that respect occlusion zones.
- Dialogue VO triggered via Timeline signal emitter feeding FMOD or Wwise, ensuring lip-sync alignment.

## Puzzle & Story Integration

### Puzzle Solve Cinematics

- Highlight solved components via emissive pulse + bloom.
- Camera clamps player control for <2 seconds to show mechanism reconfiguration.
- Provide tactile audio: click cascade + resonant success chord.

### Environmental Storytelling

- Use holographic playback, ghost data projections, or drone reenactments.
- Cinemachine FreeLook transitions to over-the-shoulder vantage with slow dolly.

### Narrative Reveals

- Branching Timeline segments triggered by player choice; Cinemachine cameras swapped via timeline track mix.
- Visual motifs (e.g., glowing glyphs) recur to tie story threads together.

### Consequence Cinematics

- After major decisions, show quick montage of world reactions (NPC crowd shift, security level change).
- Use split-screen overlays or cascading UI notifications to convey systemic impact.

## Technical Implementation Guide

### Cinemachine Setup (Unity)

1. **Camera Rig Library**: Maintain prefab set (ExplorerCam, CombatCam, CinematicRig) with standardized lens and damping.
2. **Blend Tree**: Configure Cinemachine Brain with default blend `Ease In Out 0.5s`; special cases override per Timeline clip.
3. **Dolly Tracks**: Author ScriptableObjects referencing spline paths; designers reuse tracks via addressables.
4. **Impulse System**: Define impulses for small/medium/heavy events tied to camera shake presets.

### Timeline Sequencing

- Each cinematic uses a master Timeline with tracks for Animation, Cinemachine, Audio, Particles, HUD, and Signals.
- Use Signal Emitters to notify gameplay systems (`LOCK_PLAYER`, `UNLOCK_PLAYER`, `NPC_RELEASE`).
- Nested timelines handle reusable beats (standard door open, NPC greet, alarm spike).
- Timeline duration targets: 2-6 seconds to keep gameplay pacing tight.

### Post-Processing Stack Requirements

- Profile per zone stored as Volume assets with overrides for exposure, color, bloom, vignette, DOF, grain, lens distortion.
- Transition volumes placed at door thresholds blend between profiles with weight curves keyed via Timeline.
- Ensure mobile/console performance by limiting expensive effects (chromatic aberration, film grain) to cinematic windows.

### Audio Mixing Strategy

- Utilize FMOD buses: `Master > Music`, `Ambience`, `SFX`, `VO`, `UI`.
- Automation curves triggered via Timeline control mix states (e.g., duck ambience by 4 dB during VO).
- Door transitions use snapshot blending; combat cinematics engage side-chain compression to keep dialogue intelligible.
- Music stems prepared for tension, neutral, and victory states; Cinematics request state changes via parameter automation.

### Implementation Checklist

- [ ] Cinemachine tracks authored with normalized naming (`Zone_A_DoorEnter`).
- [ ] Timeline bound references validated per scene load.
- [ ] Post-processing profiles linked to zone manager scriptable data.
- [ ] FMOD events referenced via GUID constants to prevent typos.
- [ ] QA pass verifying 60 FPS during cinematics with profiler capture.

## Prototype Deliverables & Acceptance Criteria

### Deliverables

1. **5+ Unique Transitional Cinematics** covering different zone types (safe, neutral, danger, anomaly, emergency) with bespoke door, camera, and lighting treatments.
2. **Event Cinematic Library** including puzzle unlock, NPC encounter, combat start, narrative reveal, and alarm escalation.
3. **Cinemachine + Timeline Toolkit** prefabs/presets for designers to author new sequences quickly.
4. **Audio Mixing Profiles** for transitions, alarms, and story beats.
5. **QA Checklist** verifying sync, performance, and polish (attached to project QA suite).

### Acceptance Criteria

| # | Scenario | Acceptance Criteria |
|---|----------|--------------------|
| 1 | Zone transition cinematic | **GIVEN** the player approaches a secure door **WHEN** the transition triggers **THEN** the door animation, camera dolly, lighting blend, HUD overlay, and audio layers all complete within 5 seconds without hitching. |
| 2 | Particle/audio sync | **GIVEN** sparks or dust particles emit during a transition **WHEN** they spawn **THEN** matching SFX play within 100 ms and align with light flashes. |
| 3 | NPC emergence | **GIVEN** an NPC reveal cinematic **WHEN** the zone loads **THEN** the NPC animation, VO, and camera coverage stay synchronized and AI regains control only after the cinematic releases it. |
| 4 | Story beat cinematic | **GIVEN** a narrative reveal Timeline **WHEN** it plays **THEN** Cinemachine depth of field, vignette, and VO cues activate according to the Timeline markers with no abrupt cuts. |
| 5 | Post-processing consistency | **GIVEN** any transition between zones **WHEN** color grading changes **THEN** blending curves prevent hard jumps and respect the zone palette definitions. |
| 6 | Performance | **GIVEN** all five transitional cinematics **WHEN** executed on target hardware **THEN** frame rate never drops below 55 FPS and no audio latency exceeds 50 ms. |
| 7 | Tooling readiness | **GIVEN** designers need new cinematics **WHEN** they duplicate existing Timeline templates **THEN** all bindings resolve automatically, requiring only content tweaks (no script editing). |

Meeting all acceptance criteria confirms the cinematics and transition system delivers polished, atmospheric sequences aligned with the Protocol EMR experience.
