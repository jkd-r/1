# Audio and Assets Protocol

This document outlines the comprehensive approach to audio design, asset acquisition, integration workflows, and quality standards for the Protocol EMR project.

## Table of Contents

1. [Audio Direction](#audio-direction)
2. [Audio Sourcing Strategy](#audio-sourcing-strategy)
3. [Audio Integration Workflow](#audio-integration-workflow)
4. [Asset Acquisition](#asset-acquisition)
5. [Licensing and Rights Management](#licensing-and-rights-management)
6. [Import Guidelines](#import-guidelines)
7. [Visual Style and Effects](#visual-style-and-effects)
8. [Prototype Deliverables](#prototype-deliverables)
9. [QA Checklist](#qa-checklist)

## Audio Direction

### Audio Aesthetic

The audio design for Protocol EMR emphasizes a **polished, professional atmosphere** with deliberate tension and immersion elements that support narrative and gameplay mechanics.

### Audio Categories

#### Ambient Tension
- **Purpose**: Establish psychological depth and emotional stakes
- **Characteristics**:
  - Low-frequency drones (60-120 Hz) for underlying unease
  - Subtle layered textures (wind, electrical hum, digital artifacts)
  - Dynamic range from -24dB to -6dB (relative to mix)
  - Responds to player state and narrative progression
- **Implementation**: Looping ambient beds with procedural variation
- **Target Duration**: 2-5 minute seamless loops

#### Interaction Sound Effects (SFX)
- **Purpose**: Provide tactile feedback and UI confirmation
- **Characteristics**:
  - Punchy, clear transients (0-5ms attack time)
  - Frequency range: 500 Hz - 12 kHz
  - Duration: 100ms - 1.5 seconds per effect
  - Distinct from ambient layer (no frequency overlap)
- **Categories**:
  - UI confirmation sounds (selections, dialogs, notifications)
  - Environmental interaction feedback (doors, objects, environmental responses)
  - Alert and warning sounds (danger, system notifications)
- **Target Count**: 30-50 unique SFX for prototype

#### Narrator Voiceover (VO)
- **Purpose**: Deliver narrative content and guidance
- **Characteristics**:
  - Professional voice acting with clear articulation
  - Delivery tone: Authoritative yet approachable
  - Recording specifications:
    - Sample rate: 48 kHz, 24-bit depth
    - Microphone: Condenser (studio-grade, cardioid pattern)
    - Environment: Treated recording space (minimal reverb)
  - Processing pipeline: Light EQ, compression, normalization
- **Localization**: Prepare English master with structure for future localization
- **Target Duration**: 5-10 minutes cumulative for prototype

## Audio Sourcing Strategy

### Primary Sources

#### Online Sound Libraries
- **Recommended Platforms**:
  - **Freesound.org**: Free CC-licensed assets (register account, filter by license)
  - **Zapsplat**: Free SFX and royalty-free audio
  - **BBC Sound Library**: High-quality, royalty-free sound effects
  - **Epidemic Sound**: Subscription-based (negotiable licensing for projects)
  - **Artlist.io**: Curated music and SFX with commercial licensing

- **Search Keywords**:
  - Ambient: "drone", "atmospheric", "tension", "sci-fi", "digital ambient"
  - SFX: "UI beep", "whoosh", "notification", "confirmation", "sci-fi effect"
  - VO: Hire professional voice actors from platforms (Fiverr, Voices.com, SAG-AFTRA if budget allows)

- **Quality Standards**:
  - Minimum 16-bit, 44.1 kHz (preferably 24-bit, 48 kHz)
  - Lossless format preferred (WAV, FLAC)
  - Download all stems and multitracks when available

#### Procedural Generation
- **Tools**:
  - **FMOD Studio**: Procedural synthesis for ambient textures
  - **SFXia**: AI-powered procedural SFX generation
  - **Pure Data (Pd)**: Custom generative ambient systems
  - **SuperCollider**: Advanced audio synthesis and processing

- **Use Cases**:
  - Generative ambient loops with real-time variation
  - Unique interaction SFX based on parameter mapping
  - Responsive audio that adapts to gameplay state

#### Custom Voice Recording
- **Process**:
  1. Script dialogue with phonetic considerations for voice actors
  2. Conduct multiple takes (minimum 2-3 per line)
  3. Record in treated studio environment
  4. Provide detailed voice direction document to talent
- **Alternatives**: 
  - Text-to-speech tools (ElevenLabs, Google Cloud TTS) for prototyping
  - AI voice generation for testing and early phases

### Source Prioritization
1. **First Priority**: Free or open-source community assets (cost-effective, legally clear)
2. **Second Priority**: Licensed libraries with commercial terms
3. **Third Priority**: Procedural generation (unique, customizable)
4. **Fourth Priority**: Custom recording (high control, higher cost/time)

## Audio Integration Workflow

### Middleware and Engine Integration

#### FMOD Studio (Recommended)
- **Setup**:
  - Create FMOD project with organizational structure:
    - Master Bus (main output)
    - Ambience Bus (ambient layers)
    - SFX Bus (interaction sounds, 3D positional audio)
    - VO Bus (dialogue, narrator)
    - UI Bus (menu interactions)
  - Implement compressor on Master Bus (-6dB threshold, 4:1 ratio, 50ms attack)

- **Implementation**:
  - Design "Mixer" parameter to control ambient intensity (0-100)
  - Create "TensionLevel" parameter for dynamic ambient modulation
  - Link VO to mix automation (ducks SFX/ambience by 6dB during dialogue)

#### Unreal Engine Integration (if applicable)
- **Plugin**: Use FMOD Studio integration plugin
- **Blueprint Setup**:
  - `AmbientAudioManager` component for ambient layer control
  - `InteractionAudioManager` for event-based SFX triggering
  - Voice system with dialogue queue management

#### Unity Integration (if applicable)
- **Tools**: FMOD Studio plugin or Wwise integration
- **Scripts**: Audio managers for ambient/SFX/VO layer control

### Mixing Passes

#### Pass 1: Level Setting
- Establish baseline levels for each bus:
  - Ambience: -12dB
  - SFX: -6dB (with peak limiting)
  - VO: -3dB (dialogue clarity priority)
  - UI: -18dB (subtle, non-intrusive)

#### Pass 2: Equalization (EQ)
- **Ambience Bus**: High-pass filter (100 Hz cutoff, gentle slope)
- **SFX Bus**: Gentle presence peak (2-4 kHz, +3dB, Q=1.0)
- **VO Bus**: Presence shelf (3-5 kHz, +2dB); de-esser for sibilance control
- **Master Bus**: Linear phase EQ for reference monitoring

#### Pass 3: Compression and Dynamics
- **Ambience**: Gentle compression (6:1 ratio, soft knee, 200ms attack)
- **SFX**: Peak limiting to prevent clipping (-1dB threshold)
- **VO**: Medium compression (4:1 ratio, transparent settings for dynamic range)
- **Master**: Transparent limiter for final safety (-0.1dB threshold)

#### Pass 4: Spatial Processing
- **3D Audio**: SFX and ambient elements positioned in 3D space
- **Reverb**: Convolver with impulse responses appropriate to environment
  - Ambience: Light room response (0.3-0.5s RT60)
  - SFX: Tight, dry response (minimal reverb for clarity)
  - VO: Dry (no reverb; recorded in treated space)
- **Stereo Width**: Ambience processed for immersive stereo field

#### Pass 5: Master Bus Processing
- Sequence: EQ → Compressor → Limiter → Metering
- Target loudness: -14 LUFS (loudness units relative to full scale)
- Peak limiting at -0.1dB for headroom

### Testing and Iteration
- Test audio in context with game loop running
- Validate mix at multiple playback levels (studio monitors, headphones, laptop speakers)
- Adjust automation as needed for dialogue and interactive sequences

## Asset Acquisition

### 3D Models and Textures

#### Primary Sources

**Free Assets**:
- **Sketchfab** (https://sketchfab.com/)
  - Filter by: CC licenses, downloadable
  - Search: "sci-fi", "modern", "minimalist" for thematic consistency
  - Recommended creators: Known studios with consistent quality

- **Freepik 3D** (https://www.freepik.com/3d-models)
  - Free tier available with attribution
  - Search collections for thematic asset packs

- **TurboSquid Free** (https://www.turbosquid.com/Search/3D-Models/free)
  - Curated free models with commercial licensing

- **Poly Haven** (https://polyhaven.com/models)
  - High-quality, CC0 licensed 3D models
  - Excellent texture resources as well

**Paid/Premium Assets**:
- **TurboSquid**: Professional-grade models with license flexibility
- **Artstation Marketplace**: Curated artist content
- **CGTrader**: Large marketplace with filtering by license
- **Adobe Stock**: 3D assets with subscription access

#### Selection Criteria
- **Topology**: Clean, optimized geometry (appropriate for target platform)
- **Textures**: PBR-compliant (see Import Guidelines)
- **Licensing**: Commercial use permitted, clear attribution requirements
- **Format**: FBX, GLTF/GLB, or OBJ with accompanying materials
- **Scale Consistency**: Verify proportions match project scale expectations

#### Texture Acquisition
- **Poly Haven Textures**: Free, PBR-ready CC0 assets
- **Texturing.xyz**: Professional PBR textures (free and paid tiers)
- **Ambient CG**: High-resolution CC0 PBR materials
- **CC0 Textures**: Curated source with commercial licensing

### Procedural Content
- **Houdini**: Procedural modeling for environmental variation
- **World Creator**: Procedural terrain and landscape generation
- **Substance Designer**: Procedural texture generation and modification

### Photography-Based Assets
- **Textures from Photos**: Use free image sources (Unsplash, Pexels) for reference and source material
- **Photogrammetry**: Potential future avenue for unique asset creation

## Licensing and Rights Management

### License Types and Compliance

#### Creative Commons Licenses (for free assets)
- **CC0 (Public Domain)**: No restrictions, use freely
- **CC-BY (Attribution)**: Requires credit in documentation
- **CC-BY-NC (Non-Commercial)**: Free for non-commercial; obtain separate license for commercial use
- **CC-BY-SA (Share-Alike)**: Requires derivative works to use same license

#### Commercial Licenses
- **Royalty-Free**: One-time payment, unlimited use
- **Site License**: Use restricted to specific website/platform
- **Exclusive**: Only one party holds rights to asset

#### Voice Acting and Audio
- **SAG-AFTRA Rates**: If hiring professional union talent, budget accordingly
- **Independent Voice Artists**: Negotiate usage rights explicitly
- **Synthesis/TTS**: Review terms of service for commercial use permissions

### Asset Tracking and Documentation

#### Manifest Format
Create `ASSET_MANIFEST.md` in project root:
```
## Audio Assets

| Asset Name | Source | License | URL | Expires | Notes |
|------------|--------|---------|-----|---------|-------|
| ambient_drone_01 | Freesound | CC-BY | https://... | N/A | Attribution: [Creator Name] |
| ui_confirm | Custom | Internal | N/A | N/A | Synthesized in FMOD |

## 3D Models

| Asset Name | Source | License | URL | Attribution | Expires |
|------------|--------|---------|-----|-------------|---------|
| cabinet_01 | Sketchfab | CC-BY | https://... | [Artist] | N/A |
```

### Rights Clearance Checklist
- [ ] License permits commercial use (if applicable)
- [ ] Attribution requirements documented
- [ ] Redistribution rights understood
- [ ] Modification rights confirmed
- [ ] Derivative use permitted
- [ ] Geographic restrictions identified (if any)
- [ ] Expiration or subscription renewal dates tracked

## Import Guidelines

### Naming Conventions

#### Naming Structure: `[Category]_[Type]_[Descriptor]_[Variant]`

**Audio Files**:
- `AMB_TENSION_DRONE_01.wav` - Ambient tension drone, first variant
- `SFX_UI_CONFIRM_01.wav` - UI confirmation sound effect
- `SFX_INTERACT_DOOR_OPEN.wav` - Door open interaction sound
- `VO_NARRATOR_INTRO_01_TAKE2.wav` - Narrator intro, take 2

**3D Models**:
- `MODEL_CABINET_OFFICE_01.fbx` - Office cabinet model, variant 1
- `MODEL_DESK_WORKSPACE_LOD0.fbx` - Workspace desk, LOD 0 (high detail)
- `MODEL_DESK_WORKSPACE_LOD1.fbx` - Workspace desk, LOD 1 (medium detail)

**Textures**:
- `TEX_METAL_BRUSHED_ALBEDO.png` - Brushed metal albedo map
- `TEX_METAL_BRUSHED_NORMAL.png` - Brushed metal normal map
- `TEX_METAL_BRUSHED_ROUGHNESS.png` - Brushed metal roughness map
- `TEX_METAL_BRUSHED_AO.png` - Brushed metal ambient occlusion map

### Audio Optimization

#### Format and Encoding
- **Master Archive**: 24-bit, 48 kHz WAV (lossless for archival)
- **Ambient Loops**: 
  - Engine/Engine Project: Lossless (WAV) during development
  - Build/Distribution: OGG Vorbis (128 kbps quality) or MP3 (192 kbps minimum)
- **SFX**: 
  - Development: 24-bit, 48 kHz WAV
  - Build: OGG Vorbis (96-128 kbps) or MP3 (192 kbps)
- **VO**: 
  - Development: 24-bit, 48 kHz WAV
  - Build: OGG Vorbis (192 kbps) or MP3 (256 kbps)

#### Processing Before Implementation
1. Normalize to -3dB peak to avoid clipping during runtime mixing
2. Remove leading/trailing silence (trim to 50ms buffer)
3. Verify no DC offset exists (apply DC removal filter if needed)
4. Check for clicks, pops, or artifacts (remove or edit)
5. Generate cue points for loop points in FMOD/Wwise

#### Compression Standards
- Ambience loops: Max 10 MB per loop (for polished prototype)
- SFX: Max 200 KB per effect
- VO: Max 2 MB per minute of dialogue

### 3D Model Optimization

#### File Format Selection
- **Primary**: FBX 2020 format (broad compatibility)
- **Alternative**: GLTF 2.0 (for web/lightweight implementations)
- **Game Engine Native**: Use engine's preferred format (UAsset for Unreal, etc.)

#### Geometry Optimization
- **Polygon Count**:
  - Hero assets (main focus): 50,000 - 150,000 polygons
  - Environmental (background): 5,000 - 20,000 polygons
  - Simple objects (UI elements): < 5,000 polygons
  
- **LOD Strategy** (Level of Detail):
  - LOD0: Full detail (poly counts above)
  - LOD1: 30-50% reduction
  - LOD2: 60-70% reduction
  - LOD3: 90% reduction (silhouette only, if needed)

- **Best Practices**:
  - Merge vertices within 0.1mm tolerance (remove duplicate geometry)
  - Remove interior faces (non-visible geometry)
  - Optimize UV seams (minimize seams, maximize texture space)
  - Clean naming (no special characters, version numbers)

#### Texture Specifications

##### PBR Workflow Requirements
- **Required Maps**:
  - **Albedo (Diffuse Color)**: sRGB color space, 1-4K resolution
  - **Normal Map**: Linear color space, tangent-space (DirectX or OpenGL format specified)
  - **Roughness**: Linear, grayscale, 1-4K resolution
  - **Metallic**: Linear, grayscale, 1-4K resolution
  - **Ambient Occlusion (AO)**: Linear, grayscale, 1-4K resolution (optional but recommended)

- **Recommended Resolution**:
  - Hero assets: 2K (2048x2048) minimum
  - Secondary assets: 1K (1024x1024)
  - Repeating/tiling textures: 512x512 acceptable

- **Texture Format**:
  - Development: PNG (lossless)
  - Build: TGA or DXT compression (engine-dependent)
  - Web: WebP or optimized PNG

- **Naming with Map Types**:
  - `MATERIAL_NAME_ALBEDO.png`
  - `MATERIAL_NAME_NORMAL.png` (specify _DX or _GL if needed)
  - `MATERIAL_NAME_ROUGHNESS.png`
  - `MATERIAL_NAME_METALLIC.png`
  - `MATERIAL_NAME_AO.png`

#### Rigging and Animation (if applicable)
- **Skeleton**: Standard humanoid or custom (document structure)
- **Skinning**: Verify weights are clean (weight paint normalized)
- **Animation Format**: FBX with embedded animations or separate animation files

## Visual Style and Effects

### VFX Integration

#### Glow Effects
- **Purpose**: Highlight interactive elements, reinforce visual hierarchy
- **Implementation**:
  - Post-process bloom effect (threshold: 0.8, intensity: 0.5)
  - Emissive material maps on select objects (glowing UI panels, tech elements)
  - Glow layer separation (render at 0.5x or 0.25x resolution for performance)

- **Color Palette**:
  - Primary glow: Cool blue (RGB: 100, 180, 255)
  - Secondary glow: Cyan accent (RGB: 100, 220, 255)
  - Warning/Alert: Warm amber (RGB: 255, 200, 100)

#### Bloom Effect
- **Settings**:
  - Threshold: 1.0 (only brightest elements bloom)
  - Intensity: 0.6 (subtle, non-overwhelming)
  - Blur radius: 8-16 pixels
  - Blend mode: Additive (additive blending for accumulation)

- **Performance Consideration**: Bloom computed at 1/4 or 1/2 resolution for optimization

### Shader Architecture

#### Master Shader Structure
- **Base Material Type**: PBR (Physically-Based Rendering) with common parameters:
  - Albedo color and texture
  - Normal map
  - Roughness and Metallic values
  - Ambient Occlusion

#### Custom Shader Variants
- **UI Shader**: 2D positioned UI elements with glow and edge fade
- **Interactive Highlight Shader**: Adds animated rim light on interactive objects
- **Sci-Fi Tech Shader**: Animated emissive pulsing, color cycling for futuristic elements
- **Transparent/Glass Shader**: Refraction and reflection with fade falloff

#### Shader Optimization
- Minimize fragment shader complexity
- Use vertex attributes efficiently (avoid redundant calculations)
- Batch similar materials to reduce state changes
- Pre-baked lighting where possible (lightmaps, ambient occlusion baked into albedo)

### Visual Style Guide

#### Color Theory
- **Primary Palette**: Cool neutrals (grays, whites) with cyan/blue accents
- **Secondary Palette**: Warm highlights (amber, gold) for warnings/important elements
- **Saturation**: Moderate (not oversaturated; professional, restrained aesthetic)
- **Contrast**: High contrast between interactive and background elements

#### Lighting Philosophy
- **Key Light**: Directional light simulating natural or artificial single source
- **Fill Light**: Soft, low-intensity secondary light to prevent harsh shadows
- **Accent Light**: Colored light on interactive/important elements (cyan/blue theme)
- **Baked Lighting**: Pre-computed for static geometry (performance optimization)

## Prototype Deliverables

### Polished-Feel Prototype Requirements

#### Audio Implementation
- [ ] Ambient tension layer: Continuous looping, responds to narrative state
- [ ] Core interaction SFX: 15-20 unique, distinct sounds for key interactions
- [ ] Narrator VO: 2-3 minute introductory sequence with professional voice acting
- [ ] Mix pass: Balanced levels, compression applied, master loudness at -14 LUFS
- [ ] Dynamic audio: Volume ducking during VO, state-based ambient modulation

#### Visual Assets
- [ ] Primary environment model: Hero asset at 2K textures, optimized LOD0
- [ ] Interactive object models: 5-10 contextually relevant props with PBR textures
- [ ] UI framework: Visual mockups or in-engine UI with glow effects
- [ ] VFX showcase: Glow, bloom, and shader effects demonstrated on key elements

#### Integration
- [ ] Middleware setup: FMOD/Wwise project configured with proper bus structure
- [ ] Engine integration: Audio systems implemented and responding to game events
- [ ] Quality baseline: Demonstrated mix quality, no audio clipping or artifacts
- [ ] Documentation: Asset manifest complete with licensing, naming conventions applied consistently

### Deliverable Checklist
- [ ] `docs/protocol-emr/audio-assets.md` (this document) in project repository
- [ ] `ASSET_MANIFEST.md` tracking all audio, models, and textures
- [ ] FMOD/Wwise project file with complete mixer setup
- [ ] Sample integrated scene/level demonstrating audio and visual integration
- [ ] Build output meeting performance targets (platform-specific frame rate/audio latency)

## QA Checklist

### Audio Quality Assurance

#### Technical Validation
- [ ] All audio files are at correct sample rate (48 kHz minimum)
- [ ] No audio clipping or distortion present (peak levels checked)
- [ ] Normalize levels verified (-3dB peak, no clipping in mix)
- [ ] Cross-platform audio tested (PC, intended target platforms)
- [ ] Latency acceptable for interaction feedback (< 100ms total latency)
- [ ] Surround/spatial audio positioning verified (if applicable)

#### Mix Quality
- [ ] Master loudness meets specification (-14 LUFS ± 1 LUFS)
- [ ] Frequency balance correct across spectrum (no excessive booming or harsh highs)
- [ ] Dialogue intelligible over ambient/SFX (clarity maintained)
- [ ] Ambience loop crossfading smooth (no clicks at loop points)
- [ ] Dynamic range preservation (not over-compressed, but controlled)

#### Playback and Performance
- [ ] Audio playback glitch-free during extended sessions (> 1 hour)
- [ ] No memory leaks or audio resource accumulation
- [ ] Voice-over lines sync correctly with on-screen events
- [ ] SFX timing aligned with visual feedback (< 50ms sync tolerance)
- [ ] Ambient layers transition smoothly (no jarring crossfades)

### Visual Quality Assurance

#### Rendering Quality
- [ ] All 3D models render without visual artifacts (no z-fighting, texture seams, etc.)
- [ ] Textures display correctly with no color banding or compression artifacts
- [ ] Normals properly oriented (no inverted surface lighting)
- [ ] LOD transitions smooth (no pop-in or visible transitions)
- [ ] Transparency rendering correct (no alpha blending artifacts)

#### Effects and Shaders
- [ ] Glow effects render correctly without bloom excessive
- [ ] Bloom effect threshold and intensity appropriate (not overwhelming)
- [ ] Custom shaders execute without performance stutters
- [ ] Material properties reflect correctly under different lighting
- [ ] PBR materials appear physically plausible (reflectance appropriate)

#### Performance Metrics
- [ ] Frame rate maintained (target platform specification met)
- [ ] Memory footprint within budget (audio/visual assets combined)
- [ ] GPU utilization reasonable (not saturated)
- [ ] CPU utilization reasonable (audio processing, asset loading)
- [ ] No memory leaks during extended play

#### Presentation and Polish
- [ ] UI elements readable and visually hierarchical
- [ ] Glow effects on interactive elements clearly visible
- [ ] Color palette consistent throughout
- [ ] Animation smoothness (if applicable)
- [ ] Overall aesthetic cohesion (thematic consistency)

### Platform-Specific Testing
- [ ] Windows build tested and verified
- [ ] [Additional platforms as applicable: Mac, Linux, Console, Mobile]
- [ ] Audio output formats verified for platform
- [ ] Texture compression appropriate for platform GPU

### Accessibility
- [ ] UI elements have sufficient contrast (WCAG AA standard minimum)
- [ ] Important audio cues have visual alternatives (if applicable)
- [ ] Subtitles/captions available for VO (optional for prototype, recommended for full release)
- [ ] No flashing or rapid visual changes (seizure precautions)

### Final Sign-Off
- [ ] Audio mix approved by audio engineer/lead
- [ ] Visual assets approved by art lead
- [ ] Technical integration verified by engineering team
- [ ] Performance targets met on target platforms
- [ ] Documentation complete and accurate
- [ ] No critical bugs identified in QA pass
