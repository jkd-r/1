# Advanced VFX & Atmosphere Protocol

This document outlines the comprehensive visual effects, atmospheric design, and environmental storytelling systems that create Protocol EMR's immersive high-tech escape room aesthetic.

## Table of Contents

1. [Visual Effects Library](#visual-effects-library)
2. [Atmospheric Design](#atmospheric-design)
3. [Environmental Storytelling](#environmental-storytelling)
4. ["Unknown" Entity Visual Integration](#unknown-entity-visual-integration)
5. [Dynamic Visual Feedback](#dynamic-visual-feedback)
6. [Performance & Polish](#performance--polish)
7. [PBR Workflow](#pbr-workflow)
8. [Technical Requirements](#technical-requirements)
9. [Prototype Deliverables](#prototype-deliverables)
10. [QA Checklist](#qa-checklist)

## Visual Effects Library

### Particle Systems

#### Electrical Effects
- **Sparks**: 
  - Particle count: 50-200 particles
  - Lifetime: 0.1-0.8 seconds
  - Color: Blue-white (255, 240, 200) to yellow (255, 200, 100)
  - Size: 0.01-0.05 units
  - Emission rate: 20-100 per second
  - Use cases: Electrical arcs, damaged equipment, energy discharge

- **Electrical Arcs**:
  - Lightning-type particle system with branching
  - Duration: 0.5-2.0 seconds
  - Intensity: Based on danger level
  - Sound integration: Crackling electrical sounds
  - Physics: Attract to metal surfaces

#### Atmospheric Particles
- **Dust Motes**:
  - Particle count: 100-500 particles
  - Lifetime: 10-30 seconds with fade
  - Color: Gray-brown (120, 110, 100) with transparency
  - Movement: Slow floating with air currents
  - Response: Reacts to player movement

- **Smoke/Steam**:
  - Density: Varies by source intensity
  - Color: Gray to white gradient
  - Movement: Turbulent with heat simulation
  - Lifetime: 5-20 seconds
  - Sources: Ventilation, damaged systems, machinery

#### Digital Effects
- **Digital Glitch**:
  - Screen-space particle effects
  - Color: RGB split (red, green, blue channels)
  - Pattern: Random pixelation and distortion
  - Trigger: System malfunctions, "Unknown" presence
  - Duration: 0.5-3.0 seconds

- **Data Streams**:
  - Flowing particle lines
  - Color: Cyan (0, 255, 255) to blue (0, 100, 255)
  - Direction: Along cables, through data ports
  - Speed: 5-20 units/second
  - Pulse: Synchronized with data transfer

### Lighting Effects

#### Volumetric Lighting
- **God Rays**:
  - Light shafts through grates, windows
  - Intensity: 0.3-0.8 opacity
  - Color: Warm (255, 240, 200) for safe areas, cool (100, 150, 255) for data spaces
  - Movement: Slow rotation with dust interaction
  - Performance: Toggle based on graphics settings

- **Volumetric Fog**:
  - Density: 0.01-0.05 per zone
  - Height: Ground-level to 2 meters
  - Color: Zone-dependent (blue for safe, red for danger)
  - Movement: Slow drift with air currents
  - Response: Parts around player movement

#### Dynamic Shadows
- **Real-time Shadows**:
  - Distance: 20-50 units based on quality
  - Resolution: 1024-4096 shadow maps
  - Cascades: 2-4 shadow cascades for large areas
  - Softness: Penumbra based on light distance
  - Performance: LOD system for shadow quality

- **Shadow Puppets**:
  - Animated shadow patterns
  - Storytelling: Show past events through shadows
  - Trigger: Player proximity to story areas
  - Duration: 5-15 seconds
  - Fade: Smooth in/out transitions

### Screen Distortions

#### Digital Artifacts
- **Pixelation**:
  - Block size: 2-8 pixels
  - Intensity: 0-100% based on system corruption
  - Areas: Screen edges or full screen
  - Trigger: System errors, "Unknown" communication
  - Recovery: Gradual return to normal

- **Circuit Patterns**:
  - Overlay texture with circuit board design
  - Color: Green (0, 255, 0) or amber (255, 200, 0)
  - Animation: Flowing electricity effect
  - Transparency: 10-30% opacity
  - Areas: UI elements, screen edges

#### Chromatic Aberration
- **Color Separation**:
  - RGB channel displacement
  - Intensity: 0-5 pixels offset
  - Trigger: High stress situations, system overload
  - Recovery: Gradual normalization
  - Direction: Radial from screen center

### Impact Effects

#### Combat/Damage Effects
- **Punch Hit**:
  - Impact sparks: 20-50 particles
  - Screen shake: 0.1-0.3 seconds
  - Sound integration: Impact sounds
  - Directional force: Based on attack angle
  - Damage indication: Red flash with pain sound

- **Explosion Effects**:
  - Shockwave: Expanding ring
  - Debris: 100-300 particles
  - Smoke: Dense, billowing
  - Light flash: 0.1-0.5 seconds
  - Sound: Explosion with reverb

#### Energy Discharge
- **EMP Blast**:
  - Electromagnetic pulse visual
  - Radius: 5-20 units
  - Color: Blue-white discharge
  - Effect radius: Electronic malfunction visualization
  - Duration: 2-5 seconds with fade

### Environmental Effects

#### Holograms
- **Type**: 
  - Transparent blue projections
  - Flicker rate: 2-5 Hz when damaged
  - Resolution: Varies by system importance
  - Interaction: Player can pass through
  - Content: Data displays, instructions, warnings

- **Holographic Interfaces**:
  - Floating UI elements
  - Touch interaction visualization
  - Color: Cyan to blue gradient
  - Animation: Smooth transitions
  - Feedback: Visual response to interaction

#### Force Fields
- **Energy Barriers**:
  - Visual: Shimmering energy wall
  - Color: Red (danger) or blue (safety)
  - Pattern: Wave distortion effect
  - Sound: Humming with pitch variation
  - Interaction: Block player, allow projectiles

## Atmospheric Design

### High-Tech Aesthetic

#### Material Design
- **Clean Lines**:
  - Sharp edges, 90-degree angles
  - Minimal ornamentation
  - Functional design philosophy
  - Color scheme: White, gray, black with accent colors
  - Surface treatment: Brushed metal, matte finishes

- **Technological Elements**:
  - Visible circuitry and wiring
  - LED indicators and status lights
  - Touch interfaces and holographic displays
  - Mechanical components with purpose
  - Integration of form and function

#### Architectural Style
- **Modular Construction**:
  - Prefabricated panel systems
  - Exposed structural elements
  - Utility conduits and cable trays
  - Standardized component sizing
  - Maintenance access panels

### Zone-Specific Atmospheres

#### Safe Areas
- **Lighting**: 
  - Color temperature: 4000K (neutral white)
  - Brightness: 80-100% of maximum
  - Shadows: Soft, minimal contrast
  - Emergency lighting: Green exit signs
  - Ambiance: Calm, orderly

- **Atmosphere**:
  - Air quality: Clean, filtered
  - Temperature: Comfortable (20-22°C)
  - Humidity: 40-50%
  - Background: Subtle mechanical hum
  - Visual: Clean surfaces, organized spaces

#### Hostile Areas
- **Lighting**:
  - Color temperature: 2000K (warm red)
  - Brightness: 20-40% with flickering
  - Shadows: Harsh, high contrast
  - Emergency lighting: Red warning lights
  - Ambiance: Tense, dangerous

- **Atmosphere**:
  - Air quality: Smoke, dust, chemical smell
  - Temperature: Variable (15-30°C)
  - Humidity: 60-80%
  - Background: Alarms, electrical arcing
  - Visual: Damage, debris, warning signs

#### Data Spaces
- **Lighting**:
  - Color temperature: 8000K (cool blue)
  - Brightness: 60-80% with pulsing
  - Shadows: Minimal, ethereal
  - Special effects: Digital artifacts, data streams
  - Ambiance: Surreal, otherworldly

- **Atmosphere**:
  - Air quality: Sterile, filtered
  - Temperature: Cool (18-20°C)
  - Humidity: 30-40%
  - Background: Data processing sounds
  - Visual: Holograms, circuit patterns

### Dynamic Atmosphere Shifts

#### State Transitions
- **Calm → Alert**:
  - Duration: 2-5 seconds
  - Lighting: Dimming, color shift to amber
  - Sound: Background alarm activation
  - Visual: Warning lights, UI changes
  - Particle: Increased dust, smoke

- **Alert → Danger**:
  - Duration: 1-3 seconds
  - Lighting: Red emergency lighting
  - Sound: Full alarm, system warnings
  - Visual: Rapid flashing, distortions
  - Particle: Sparks, smoke, debris

- **Danger → Calm Recovery**:
  - Duration: 5-10 seconds
  - Lighting: Gradual return to normal
  - Sound: Alarm fade, normal ambience return
  - Visual: Stabilization, damage assessment
  - Particle: Settling dust, smoke dissipation

### Lighting Design

#### Key Lighting
- **Primary Light Sources**:
  - Overhead fixtures (recessed LED panels)
  - Task lighting (work stations, equipment)
  - Emergency lighting (battery-powered fixtures)
  - Accent lighting (architectural features)
  - Natural light (windows, skylights where applicable)

- **Light Quality**:
  - Color rendering index (CRI): 85+
  - Uniformity ratio: 0.7 minimum
  - Glare control: UGR < 19
  - Flicker-free operation
  - Dimming capability: 0-100%

#### Fill Lighting
- **Ambient Fill**:
  - Indirect lighting techniques
  - Wall washing and grazing
  - Cove lighting integration
  - Reflective surfaces utilization
  - Light shelf implementation

- **Task-Specific Fill**:
  - Workstation illumination
  - Equipment operation lighting
  - Reading and display lighting
  - Maintenance access lighting
  - Safety critical illumination

#### Ambient Occlusion
- **Static AO**:
  - Baked lighting for static geometry
  - Contact shadow enhancement
  - Crevice darkening
  - Material separation definition
  - Depth perception improvement

- **Dynamic AO**:
  - Real-time AO for moving objects
  - Character contact shadows
  - Interactive object shadows
  - Temporal reprojection for stability
  - Performance-based quality scaling

### Color Grading

#### Emotional State Mapping
- **Trust/Safety**:
  - Primary: Cool blues (200, 220, 255)
  - Secondary: Soft greens (180, 255, 200)
  - Saturation: 70-80%
  - Contrast: Medium-low
  - Temperature: Cool (6500K)

- **Danger/Warning**:
  - Primary: Warm reds (255, 100, 100)
  - Secondary: Orange accents (255, 180, 100)
  - Saturation: 90-100%
  - Contrast: High
  - Temperature: Warm (3000K)

- **Confusion/Unknown**:
  - Primary: Desaturated grays (180, 180, 180)
  - Secondary: Muted purples (200, 180, 220)
  - Saturation: 30-50%
  - Contrast: Low-medium
  - Temperature: Neutral (4500K)

### Environmental Hazards

#### Visual Indicators
- **Danger Zones**:
  - Floor markings: Red/yellow striped patterns
  - Light projections: Warning symbols on floor
  - Particle effects: Hazardous particles
  - Sound cues: Warning tones
  - UI indicators: Zone status displays

- **Laser Systems**:
  - Visual: Red/green laser beams
  - Pattern: Scanning or stationary
  - Interaction: Block player movement
  - Sound: Electrical humming
  - Warning: Activation sequence visualization

- **Force Fields**:
  - Visual: Shimmering energy barrier
  - Color: Blue (safe) or red (danger)
  - Pattern: Wave distortion
  - Interaction: Solid collision
  - Sound: Energy field hum

## Environmental Storytelling

### Object Placement Narratives

#### Intentional Arrangement
- **Workstations**:
  - Personal items: Photos, mugs, notes
  - Work status: Half-finished tasks, open documents
  - Abandonment clues: Left suddenly, mid-activity
  - Time indicators: Clocks stopped, calendar dates
  - Personal touches: Decorations, customization

- **Equipment Status**:
  - Damage progression: New vs old damage
  - Repair attempts: Tools, spare parts nearby
  - Usage patterns: Wear patterns, frequent use areas
  - Malfunction evidence: Error messages, warning lights
  - Maintenance history: Service logs, repair stickers

#### Sequential Storytelling
- **Event Progression**:
  - Before: Normal operation evidence
  - During: Chaos and panic indicators
  - After: Cleanup and containment efforts
  - Investigation: Search and evidence collection
  - Cover-up: Removal of incriminating evidence

### Decay and Damage Indicators

#### Time-Based Deterioration
- **Structural Damage**:
  - Cracks: Stress patterns, age progression
  - Water damage: Stains, mold growth
  - Corrosion: Rust, chemical degradation
  - Wear patterns: High-traffic areas
  - Environmental effects: Dust accumulation, cobwebs

- **Equipment Degradation**:
  - Flickering lights: Electrical issues
  - Mechanical failure: Jammed components
  - Screen damage: Dead pixels, cracks
  - Cable damage: Frayed insulation, exposed wires
  - Surface damage: Scratches, dents, burn marks

#### Violence Indicators
- **Struggle Evidence**:
  - Blood spatter: Pattern analysis for events
  - Scratches: Defense wounds, furniture damage
  - Bullet holes: Entry/exit trajectories
  - Scuff marks: Foot placement, movement patterns
  - Broken objects: Thrown, smashed, knocked over

### Corporate and Facility Branding

#### Logo Integration
- **Corporate Identity**:
  - Company logos: Walls, equipment, documents
  - Department markings: Color coding, symbols
  - Safety signage: Standardized warning systems
  - Directional signage: Wayfinding systems
  - Brand colors: Consistent visual identity

- **Propaganda and Messaging**:
  - Posters: Corporate slogans, motivational messages
  - Digital displays: Company announcements
  - Safety reminders: Protocol posters, warning signs
  - Achievement displays: Project milestones, awards
  - Internal communications: Memo boards, announcements

#### Surveillance and Control
- **Security Systems**:
  - Cameras: Placement patterns, status indicators
  - Motion sensors: Visible detection zones
  - Access control: Keycard readers, biometric scanners
  - Monitoring stations: Security office layouts
  - Alarm systems: Visual and audible warnings

- **Oppression Indicators**:
  - Restriction zones: Authorized access only areas
  - Monitoring equipment: Recording indicators
  - Control mechanisms: Automated systems override
  - Punishment displays: Warning examples
  - Psychological control: Subtle manipulation evidence

### Personal Effects and Human Stories

#### Individual Narratives
- **Personal Items**:
  - Family photos: Connection to outside world
  - Personal belongings: Hobbies, interests
  - Clothing choices: Individual expression
  - Food containers: Eating habits, preferences
  - Entertainment: Books, games, personal devices

- **Work-Life Balance**:
  - Sleep arrangements: Cots, sleeping bags
  - Personal care items: Toiletries, comfort items
  - Break areas: Recreation, relaxation spaces
  - Personal storage: Lockers, personal drawers
  - Communication attempts: Messages to loved ones

#### Abandonment Stories
- **Emergency Evacuation**:
  - Left-behind items: Rushed evacuation evidence
  - Protection attempts: Barricades, defensive positions
  - Communication attempts: Last messages, warnings
  - Survival efforts: Food hoarding, water collection
  - Escape attempts: Tools, plans, failed efforts

## "Unknown" Entity Visual Integration

### Glitch Effects System

#### Communication Manifestations
- **Visual Glitches**:
  - Screen distortion during messages
  - RGB color splitting effects
  - Pixelation and artifact generation
  - Circuit pattern overlays
  - Temporary resolution degradation

- **Environmental Distortion**:
  - Localized reality warping
  - Physics anomalies near messages
  - Light flickering synchronized with communication
  - Sound distortion accompanying visuals
  - Temperature visualization effects

#### Digital Overlay Visuals
- **Message Presentation**:
  - Non-UI based text appearance
  - Floating text in world space
  - Holographic projection effects
  - Data stream visualization
  - Binary code rain effects

- **Information Display**:
  - Schematic overlays
  - System status visualizations
  - Pathfinding guidance
  - Security system bypasses
  - Hidden information revelation

### Holographic Representation

#### Entity Manifestation
- **Form Variations**:
  - Abstract geometric shapes
  - Humanoid silhouette with distortion
  - Pure energy form
  - Data cloud representation
  - Multiple simultaneous manifestations

- **Behavior Patterns**:
  - Flickering between forms
  - Size and intensity variations
  - Movement patterns: Smooth to jerky
  - Color shifts based on emotional state
  - Interaction with environment: Phasing through objects

#### Data Visualization
- **Information Flow**:
  - Particle streams representing data
  - Connection lines between systems
  - Network topology visualization
  - Code execution visualization
  - System access patterns

### Mysterious Presence Effects

#### Subtle Environmental Changes
- **Atmospheric Effects**:
  - Localized temperature changes (visual)
  - Air distortion effects
  - Unexplained light sources
  - Sound propagation anomalies
  - Magnetic field visualization

- **Object Manipulation**:
  - Remote object movement
  - Door control visualization
  - System interface activation
  - Security camera manipulation
  - Environmental control effects

#### Psychological Impact
- **Perception Alteration**:
  - Brief reality shifts
  - Memory flash visualization
  - Temporal distortion effects
  - Spatial manipulation hints
  - Cognitive dissonance visualization

## Dynamic Visual Feedback

### Interactable Objects

#### Visual Highlighting System
- **Approach Detection**:
  - Detection radius: 2-5 units
  - Highlight intensity: Based on distance
  - Color coding: Blue (neutral), green (positive), red (dangerous)
  - Animation: Subtle pulsing or glow
  - Transition: Smooth fade in/out

- **Interaction States**:
  - **Available**: Soft blue glow, gentle pulsing
  - **In Use**: Bright cyan glow, rapid pulsing
  - **Used/Depleted**: Grayed out, no glow
  - **Required for Puzzle**: Golden highlight
  - **Dangerous**: Red glow with warning particles

#### Shader Implementation
- **Outline Shader**:
  - Width: 2-4 pixels
  - Color: State-dependent
  - Intensity: Distance-based
  - Animation: Breathing effect
  - Performance: GPU-friendly implementation

- **Glow Effects**:
  - Emission mapping
  - Fresnel-based highlighting
  - Rim lighting enhancement
  - Post-processing glow
  - Bloom integration

### Puzzle Elements

#### Visual Hint System
- **Color Coding**:
  - Primary colors for main puzzle types
  - Secondary colors for sub-elements
  - Progress indication through color intensity
  - State changes through color transitions
  - Accessibility: Color blind friendly patterns

- **Spatial Relationships**:
  - Connection lines between related elements
  - Path visualization for solutions
  - Area of effect indicators
  - Sequence order numbering
  - Grouping visualizations

#### Progress Visualization
- **Completion States**:
  - Partial completion: 25%, 50%, 75% indicators
  - Fully completed: Golden celebration effect
  - Failed attempt: Red flash with shake
  - Hint activation: Blue pulse effect
  - Reset required: Gray fade out

### Danger Warnings

#### Alert Systems
- **Visual Alarms**:
  - Red pulsing lights: 1-2 Hz frequency
  - Screen edge effects: Red vignette
  - Particle effects: Warning symbols
  - UI overlays: Warning messages
  - Environmental: Flashing emergency lights

- **Shader Distortion**:
  - Heat distortion effects
  - Screen shake integration
  - Chromatic aberration
  - Noise overlay
  - Color desaturation

#### Progressive Warning Levels
- **Caution (Yellow)**:
  - Slow pulsing (0.5 Hz)
  - Mild screen effects
  - Gentle audio cues
  - Soft visual indicators
  - Reversible state

- **Warning (Orange)**:
  - Medium pulsing (1 Hz)
  - Moderate screen effects
  - Clearer audio warnings
  - Obvious visual indicators
  - Time pressure indication

- **Danger (Red)**:
  - Rapid pulsing (2 Hz)
  - Strong screen effects
  - Urgent audio alarms
  - Impossible to miss visuals
  - Immediate threat indication

### Collectible Items

#### Attention Drawing
- **Glow Effects**:
  - Constant soft glow
  - Pulsing animation
  - Color: Gold or cyan
  - Radius: 1-2 units
  - Intensity: Distance-based

- **Particle Effects**:
  - Floating particles around item
  - Upward particle streams
  - Sparkle effects
  - Attraction particles when nearby
  - Collection burst effect

#### Rarity Indication
- **Common Items**:
  - White glow
  - Simple particle effects
  - Standard collection sound
  - Basic icon in inventory

- **Rare Items**:
  - Blue glow
  - Complex particle effects
  - Special collection sound
  - Enhanced icon in inventory

- **Legendary Items**:
  - Gold/purple glow
  - Elaborate particle effects
  - Unique collection sound
  - Animated icon in inventory

### NPC State Indicators

#### Visual Status Systems
- **Hostile NPCs**:
  - Red aura: 1-2 units radius
  - Aggressive posture animation
  - Weapon highlighting
  - Threat level visualization
  - Attack telegraphing effects

- **Friendly NPCs**:
  - Blue aura: 1-2 units radius
  - Relaxed posture animation
  - Open hand gestures
  - Help availability indicators
  - Information icon when approachable

- **Neutral NPCs**:
  - White/gray aura
  - Neutral posture
  - No special highlighting
  - Contextual interaction only
  - State change visualization when triggered

#### Behavior Visualization
- **AI State Indicators**:
  - Patrol: Green path visualization
  - Search: Yellow cone of vision
  - Alert: Orange heightened awareness
  - Attack: Red aggressive state
  - Flee: White escape route

## Performance & Polish

### Level of Detail (LOD) Systems

#### Distance-Based Optimization
- **LOD Levels**:
  - LOD0 (0-10 units): Full quality, all effects
  - LOD1 (10-25 units): Reduced particle count
  - LOD2 (25-50 units): Basic lighting only
  - LOD3 (50+ units): Minimal rendering
  - Culling: Beyond 100 units

- **Transition Handling**:
  - Smooth LOD transitions
  - Hysteresis to prevent popping
  - Distance-based shader switching
  - Progressive quality reduction
  - Performance monitoring integration

#### Asset Optimization
- **Model LODs**:
  - Polygon reduction: 25% per LOD level
  - Texture resolution scaling
  - Material simplification
  - Animation compression
  - Collision mesh optimization

### Occlusion Culling

#### Spatial Optimization
- **Portal Culling**:
  - Doorway-based visibility
  - Room-by-room rendering
  - Frustum culling integration
  - Distance-based portal fading
  - Debug visualization tools

- **Occlusion Queries**:
  - Hardware occlusion queries
  - Software fallback systems
  - Hierarchical Z-buffer testing
  - Temporal coherence optimization
  - Performance profiling integration

### Particle System Optimization

#### Performance Scaling
- **Quality Settings**:
  - **Ultra**: 100% particles, full effects
  - **High**: 75% particles, most effects
  - **Medium**: 50% particles, essential effects
  - **Low**: 25% particles, minimal effects
  - **Potato**: 10% particles, critical effects only

- **Particle Budgeting**:
  - Global particle limit: 10,000 particles
  - Per-system limits based on importance
  - Automatic particle reduction under load
  - Priority-based culling
  - Frame rate adaptive scaling

#### Optimization Techniques
- **GPU Particles**:
  - Compute shader simulation
  - Reduced CPU overhead
  - Higher particle counts possible
  - Limited to simple behaviors
  - Fallback to CPU particles

- **Particle Pooling**:
  - Object pooling for particle systems
  - Reuse of expired particles
  - Memory allocation optimization
  - Garbage collection minimization
  - Dynamic pool resizing

### Shader Performance

#### Quality vs Performance Balance
- **Shader Complexity Levels**:
  - **Simple**: Basic lighting, no special effects
  - **Standard**: PBR lighting, basic effects
  - **Complex**: Advanced lighting, multiple effects
  - **Cinematic**: Maximum quality, all effects
  - **Mobile**: Optimized for performance

- **Feature Toggles**:
  - Normal mapping: Optional on low-end
  - Reflections: Quality-based
  - Emission: Distance-based
  - Post-processing: User preference
  - Shadow quality: Performance-based

#### Optimization Strategies
- **Shader LODs**:
  - Distance-based shader switching
  - Simplified math for distant objects
  - Reduced texture lookups
  - Fewer render passes
  - Approximated calculations

### Graphics Settings Management

#### User Customization
- **Quality Presets**:
  - **Potato**: Minimum settings, 30 FPS target
  - **Low**: Basic quality, 45 FPS target
  - **Medium**: Balanced quality, 60 FPS target
  - **High**: Good quality, 60+ FPS target
  - **Ultra**: Maximum quality, uncapped FPS

- **Individual Settings**:
  - Resolution scaling
  - Texture quality
  - Shadow quality
  - Particle quality
  - Post-processing quality
  - vsync toggle
  - Frame rate limit

## PBR Workflow

### Material Specifications

#### Material Properties
- **Metallic Values**:
  - Metals: 0.8-1.0 (steel, aluminum, copper)
  - Non-metals: 0.0-0.2 (plastic, glass, fabric)
  - Transition materials: 0.3-0.7 (coated metals)
  - Weathered materials: Variable based on age
  - Special materials: Custom values as needed

- **Roughness Values**:
  - Very smooth: 0.0-0.2 (polished metal, glass)
  - Smooth: 0.2-0.4 (finished plastic, painted surfaces)
  - Semi-rough: 0.4-0.6 (concrete, wood, fabric)
  - Rough: 0.6-0.8 (concrete, asphalt, worn surfaces)
  - Very rough: 0.8-1.0 (rust, damage, natural materials)

- **Albedo Guidelines**:
  - Metals: Dark, neutral values (0.5-0.7)
  - Non-metals: Full color range
  - Brightness: 50-230 sRGB range
  - Avoid pure white (255, 255, 255)
  - Avoid pure black (0, 0, 0)

### Texture Resolution Guidelines

#### Asset Classification
- **Hero Assets**:
  - Resolution: 4K (4096×4096)
  - Usage: Player interactable objects
  - Examples: Key items, puzzle pieces, important props
  - Memory priority: High
  - LOD support: Required

- **Environment Assets**:
  - Resolution: 2K (2048×2048)
  - Usage: Architecture, large objects
  - Examples: Walls, floors, machinery
  - Memory priority: Medium
  - LOD support: Required

- **Background Assets**:
  - Resolution: 1K (1024×1024)
  - Usage: Distant objects, filler content
  - Examples: Background geometry, small props
  - Memory priority: Low
  - LOD support: Optional

#### Texture Maps Required
- **Albedo/Diffuse**:
  - Linear color space
  - No lighting information
  - Clean, normalized values
  - Proper UV layout
  - Seaming considerations

- **Normal Maps**:
  - Tangent space normal maps
  - Proper blue channel (128, 255, 128)
  - Compression-friendly
  - Detail normal support
  - UV seam handling

- **Roughness Maps**:
  - Grayscale values
  - Linear gradient
  - Material property accuracy
  - Wear and tear representation
  - Consistent value ranges

- **Metallic Maps**:
  - Grayscale values
  - Clear metal/non-metal distinction
  - Proper edge definition
  - Weathering representation
  - Mask clarity

- **Ambient Occlusion**:
  - Cavity detection
  - Contact shadow enhancement
  - Detail preservation
  - Subtle application
  - Multi-bounce consideration

### Advanced Material Features

#### Emission Maps
- **Technical Lighting**:
  - LED indicators: Small, bright areas
  - Screen displays: Varied intensity
  - Warning lights: Pulsing capability
  - Control panels: Status indication
  - Emergency systems: High intensity

- **Implementation**:
  - HDR color values
  - Intensity controls
  - Animation support
  - Bloom integration
  - Performance considerations

#### Subsurface Scattering
- **Applicable Materials**:
  - Plastic components
  - Glass elements
  - Organic materials
  - Translucent surfaces
  - Special effects materials

- **Quality Settings**:
  - Distance-based quality
  - Performance scaling
  - Approximation methods
  - Screen-space techniques
  - Fallback options

### Material Library Organization

#### Naming Conventions
- **Asset Naming**:
  - Format: `[MaterialType]_[ObjectName]_[Variant]`
  - Example: `Metal_SteelPanel_Weathered`
  - Consistency across all assets
  - Version control integration
  - Search-friendly structure

- **Texture Naming**:
  - Format: `[ObjectName]_[MapType]_[Resolution]`
  - Example: `ControlPanel_Albedo_2K`
  - Suffixes: `_Albedo`, `_Normal`, `_Roughness`, `_Metallic`, `_AO`
  - Consistent file organization
  - Automated pipeline support

#### Material Variation Systems
- **Wear and Tear**:
  - Clean state
  - Light wear
  - Heavy damage
  - Weathered state
  - Destroyed state

- **Color Variations**:
  - Standard colors
  - Warning colors
  - Status colors
  - Corporate branding
  - Custom variations

## Technical Requirements

### Particle System Engine

#### Unity Particle System
- **System Requirements**:
  - Unity 2022.3 LTS or newer
  - Built-in Particle System
  - Visual Effect Graph (optional)
  - SRP support (URP/HDRP)
  - Mobile shader support

- **Features Required**:
  - GPU particle support
  - Collision detection
  - Force fields
  - Noise modules
  - Custom shader integration
  - Performance profiling tools

#### Alternative: PopcornFX
- **Integration Requirements**:
  - Plugin integration
  - Custom shader support
  - Performance optimization
  - Platform compatibility
  - Team training requirements

### Shader Technology

#### Shader Language Support
- **HLSL**:
  - DirectX compatibility
  - Unity shader graph support
  - Performance optimization
  - Feature level support
  - Debugging tools

- **GLSL**:
  - OpenGL compatibility
  - Cross-platform support
  - Mobile optimization
  - Feature parity with HLSL
  - Performance considerations

#### Shader Pipeline
- **Rendering Pipelines**:
  - Universal Render Pipeline (URP)
  - High Definition Render Pipeline (HDRP)
  - Built-in Render Pipeline
  - Custom pipeline support
  - Migration path planning

### Post-Processing Stack

#### Required Effects
- **Color Grading**:
  - Tonemapping
  - Color adjustments
  - Contrast/brightness
  - Saturation control
  - LUT support

- **Image Effects**:
  - Bloom
  - Ambient occlusion
  - Screen space reflections
  - Motion blur
  - Depth of field

- **Distortion Effects**:
  - Chromatic aberration
  - Vignette
  - Lens distortion
  - Grain
  - Color split

#### Performance Considerations
- **Quality Settings**:
  - Effect intensity controls
  - Resolution scaling
  - Temporal effects
  - Performance profiling
  - Platform optimization

### Integration Requirements

#### Audio Integration
- **Visual-Audio Sync**:
  - Particle sound triggers
  - Lighting audio response
  - Environmental audio zones
  - Dynamic audio mixing
  - Spatial audio support

#### Physics Integration
- **Collision Detection**:
  - Particle collision
  - Force field interaction
  - Environmental response
  - Performance optimization
  - Debug visualization

#### AI Integration
- **Visual Feedback**:
  - AI state visualization
  - Pathfinding display
  - Behavior indicators
  - Debug information
  - Performance monitoring

## Prototype Deliverables

### Core Visual Effects
- **Particle Effects Library**:
  - 10+ distinct particle systems
  - Electrical effects (sparks, arcs)
  - Atmospheric effects (dust, smoke)
  - Digital effects (glitches, data streams)
  - Impact effects (explosions, hits)
  - Environmental effects (holograms, force fields)

### Lighting Implementation
- **Dynamic Lighting System**:
  - Zone-specific lighting setups
  - Atmospheric lighting transitions
  - Emergency lighting systems
  - Dynamic shadow implementation
  - Performance-optimized lighting

### Atmospheric Systems
- **Environmental Atmosphere**:
  - Zone-specific atmospheric presets
  - Dynamic atmosphere transitions
  - Color grading implementation
  - Environmental hazard visualization
  - Weather effect systems

### Performance Optimization
- **Optimization Systems**:
  - LOD implementation for visual effects
  - Particle system optimization
  - Shader performance balancing
  - Graphics settings management
  - Performance profiling tools

### Integration Testing
- **System Integration**:
  - Audio-visual synchronization
  - AI visual feedback systems
  - Environmental storytelling elements
  - Interactive object highlighting
  - "Unknown" entity visual effects

## QA Checklist

### Visual Effects Quality
- [ ] All particle systems function correctly
- [ ] Effects trigger at appropriate times
- [ ] Performance meets target specifications
- [ ] Visual quality is consistent
- [ ] Effects scale properly with distance
- [ ] No visual artifacts or glitches
- [ ] Proper integration with lighting
- [ ] Sound synchronization works correctly
- [ ] Effects respond to game state changes
- [ ] Memory usage is within limits

### Lighting Validation
- [ ] Zone-specific lighting is implemented correctly
- [ ] Dynamic lighting transitions work smoothly
- [ ] Shadow rendering is accurate
- [ ] Performance targets are met
- [ ] No light bleeding or artifacts
- [ ] Emergency lighting systems function
- [ ] Color grading is applied correctly
- [ ] Atmospheric effects integrate properly
- [ ] Lighting responds to gameplay events
- [ ] Visual hierarchy is maintained

### Atmospheric Systems
- [ ] Zone atmospheres are distinct and appropriate
- [ ] Transitions between states work correctly
- [ ] Color grading enhances mood appropriately
- [ ] Environmental hazards are clearly visible
- [ ] Performance remains stable during transitions
- [ ] Audio integration with atmosphere works
- [ ] Weather effects function correctly
- [ ] Time-of-day systems work (if implemented)
- [ ] Player feedback systems are clear
- [ ] Accessibility requirements are met

### Performance Verification
- [ ] Frame rate targets are achieved
- [ ] Memory usage is within budget
- [ ] LOD systems function correctly
- [ ] Particle optimization is effective
- [ ] Shader performance is acceptable
- [ ] Occlusion culling works properly
- [ ] Graphics settings have appropriate impact
- [ ] Platform-specific optimizations work
- [ ] Thermal management is effective
- [ ] Battery life impact is reasonable (mobile)

### Integration Testing
- [ ] Audio-visual synchronization is accurate
- [ ] AI visual feedback systems work
- [ ] Environmental storytelling is effective
- [ ] Interactive object highlighting functions
- [ ] "Unknown" entity effects are integrated
- [ ] Save/load systems preserve visual state
- [ ] Network synchronization works (if applicable)
- [ ] Input systems respond correctly
- [ ] UI integration is seamless
- [ ] Debug tools function properly

### User Experience
- [ ] Visual feedback is intuitive
- [ ] Important information is highlighted
- [ ] Visual hierarchy guides attention
- [ ] Accessibility features work correctly
- [ ] Motion sickness is minimized
- [ ] Visual clarity is maintained
- [ ] Consistency across all systems
- [ ] Professional polish level is achieved
- [ ] Player immersion is enhanced
- [ ] Artistic vision is realized