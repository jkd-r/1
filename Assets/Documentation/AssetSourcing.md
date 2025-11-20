# Protocol EMR - Asset Sourcing Guide

## Overview
This document outlines free asset sources and import procedures for Protocol EMR development. The project uses placeholder Unity primitives in Sprint 1-2, with plans to integrate high-quality free assets in later sprints.

---

## Free Asset Sources

### 3D Models

#### **Mixamo**
- **URL**: https://www.mixamo.com
- **Content**: Character models and animations
- **Format**: FBX
- **License**: Free for commercial use
- **Integration**: Auto-rigged characters with built-in animation library
- **Use Case**: Player character animations (walking, running, crouching, combat)

#### **Poly Haven**
- **URL**: https://polyhaven.com
- **Content**: PBR materials, HDRIs, 3D models
- **Format**: FBX, OBJ, Blend
- **License**: CC0 (Public Domain)
- **Use Case**: Environment props, materials, lighting

#### **Sketchfab**
- **URL**: https://sketchfab.com/features/free-3d-models
- **Content**: Free 3D models (filter by "Downloadable")
- **Format**: Various (FBX, OBJ, GLTF)
- **License**: Check individual model licenses (CC0, CC-BY)
- **Use Case**: Sci-fi props, environment assets, weapons

#### **CGTrader Free**
- **URL**: https://www.cgtrader.com/free-3d-models
- **Content**: Free 3D models
- **Format**: Various
- **License**: Check individual model licenses
- **Use Case**: High-tech environment props, furniture, electronics

#### **Itch.io**
- **URL**: https://itch.io/game-assets/free
- **Content**: Game-ready asset packs
- **Format**: Unity packages, FBX
- **License**: Varies (check individual assets)
- **Use Case**: Complete asset packs, UI elements, particle effects

---

## Audio Sources

### **Freesound**
- **URL**: https://freesound.org
- **Content**: Sound effects
- **License**: CC0, CC-BY
- **Use Case**: Footsteps, ambiance, UI sounds

### **Incompetech (Kevin MacLeod)**
- **URL**: https://incompetech.com
- **Content**: Royalty-free music
- **License**: CC-BY (requires attribution)
- **Use Case**: Background music, menu music

### **Zapsplat**
- **URL**: https://www.zapsplat.com
- **Content**: Sound effects and music
- **License**: Free with attribution
- **Use Case**: Combat sounds, environmental audio

---

## Import Procedures

### Mixamo Character Import

1. **Download from Mixamo**:
   - Select character model
   - Choose "FBX for Unity" format
   - Download with T-pose or A-pose

2. **Import to Unity**:
   - Drag FBX file into `Assets/Models/Characters/`
   - Select imported model in Project window
   - Inspector → Rig tab → Animation Type: Humanoid
   - Click "Apply"

3. **Animation Import**:
   - Download animations from Mixamo (same character)
   - Drag animation FBX into `Assets/Animations/`
   - Select animation → Rig: Humanoid → Apply
   - Extract animation clips from FBX

4. **Setup Animator Controller**:
   - Create Animator Controller in `Assets/Animations/Controllers/`
   - Add animation states for: Idle, Walk, Run, Crouch, Jump
   - Set up blend trees for movement
   - Assign controller to character prefab

### PBR Material Setup

1. **Download PBR Textures** (Albedo, Normal, Metallic, Roughness, AO):
   - From Poly Haven or similar source
   - Recommended resolution: 2048x2048 or 1024x1024

2. **Import to Unity**:
   - Place textures in `Assets/Materials/[MaterialName]/`
   - Select Normal map → Inspector → Texture Type: Normal Map

3. **Create Material**:
   - Right-click in Materials folder → Create → Material
   - Shader: Universal Render Pipeline → Lit
   - Assign textures:
     - Albedo → Base Map
     - Normal → Normal Map
     - Metallic/Smoothness → Metallic/Smoothness Map
     - AO → Occlusion Map

4. **Configure Settings**:
   - Surface Type: Opaque or Transparent
   - Workflow Mode: Metallic
   - Enable/disable emission as needed

### Audio Import

1. **Download Audio Files**:
   - Format: WAV or OGG (OGG recommended for compression)
   - Quality: 44.1kHz sample rate minimum

2. **Import to Unity**:
   - Place in `Assets/Audio/[Category]/` (SFX, Music, Voice)
   - Select audio clip → Inspector settings:
     - **3D sounds**: Spatial Blend = 1.0, Compression: Vorbis
     - **2D sounds**: Spatial Blend = 0.0, Compression: Vorbis
     - **Music**: Load Type: Streaming, Compression: Vorbis
     - **UI/SFX**: Load Type: Decompress On Load, Compression: ADPCM

3. **FMOD/Wwise Integration** (Sprints 4+):
   - Will be integrated via middleware in later sprints
   - Current Sprint 1-2: Use Unity's built-in audio

---

## Sprint 1-2 Placeholder Strategy

### Temporary Assets

**Geometry**:
- Use Unity primitives: Cube, Sphere, Capsule, Cylinder
- ProBuilder for basic environment blocking (optional)
- Color-coded materials (Red=Enemy, Green=Interactive, Blue=Objective)

**Player**:
- Capsule primitive with Camera child
- No visual body in Sprint 1 (first-person only)

**Environment**:
- Cubes for walls, floors, ceilings
- Basic lighting with directional light + ambient
- Simple URP skybox

**Audio**:
- Unity default audio clips for testing
- Placeholder beep sounds for interactions

### Transition to Final Assets

**Sprint 3-4**:
- Replace player capsule with Mixamo character (first-person arms only)
- Add weapon models from Sketchfab/CGTrader
- Integrate PBR materials from Poly Haven

**Sprint 5-6**:
- Replace environment primitives with modular asset packs
- Add particle effects (muzzle flash, blood, smoke)
- Integrate FMOD/Wwise for dynamic audio

**Sprint 7-8**:
- Polish all visuals with post-processing
- Add ambient occlusion, bloom, color grading
- Finalize animation blending and transitions

---

## PBR Workflow Guidelines

### Texture Naming Convention
```
MaterialName_Albedo.png
MaterialName_Normal.png
MaterialName_Metallic.png
MaterialName_Roughness.png
MaterialName_AO.png
MaterialName_Height.png (optional)
MaterialName_Emission.png (optional)
```

### Recommended Texture Resolutions
- **Hero assets**: 2048x2048
- **Standard props**: 1024x1024
- **Small details**: 512x512
- **Tiling materials**: 1024x1024 or 2048x2048

### Material Performance
- Use texture atlases for multiple small objects
- Combine materials where possible to reduce draw calls
- Use LOD (Level of Detail) for distant objects
- Enable GPU instancing on materials for repeated objects

---

## Asset Organization Structure

```
Assets/
├── Models/
│   ├── Characters/
│   ├── Props/
│   ├── Weapons/
│   └── Environment/
├── Materials/
│   ├── Characters/
│   ├── Props/
│   └── Environment/
├── Textures/
│   ├── [MaterialName]/
│   │   ├── Albedo.png
│   │   ├── Normal.png
│   │   ├── Metallic.png
│   │   └── ...
├── Audio/
│   ├── SFX/
│   ├── Music/
│   └── Voice/
├── Animations/
│   ├── Controllers/
│   ├── Clips/
│   └── BlendTrees/
└── VFX/
    ├── Particles/
    └── Shaders/
```

---

## Attribution Requirements

When using CC-BY licensed assets, include attribution in:
1. Game credits screen
2. `CREDITS.txt` file in project root
3. Documentation comments in scene files

**Format**:
```
Asset Name by Author Name (Source URL)
Licensed under Creative Commons Attribution
```

---

## Quality Standards

### Models
- Tri count: <10K for props, <20K for characters
- Clean topology, no overlapping faces
- Proper UV unwrapping (no stretching)
- Pivot points at base or center

### Textures
- Power-of-two dimensions (512, 1024, 2048)
- No visible seams on tiling textures
- Proper sRGB vs Linear color space
- Optimized file size (compressed where possible)

### Audio
- Normalized volume levels
- No clipping or distortion
- Proper fade-in/fade-out
- Loopable where appropriate

---

## Resources for Learning

- **Unity Learn**: https://learn.unity.com
- **Poly Haven Blog**: PBR texture tutorials
- **Mixamo Tutorial**: https://www.youtube.com/mixamo
- **URP Documentation**: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest

---

**Last Updated**: Sprint 1 Foundation Phase
**Next Review**: Sprint 3 (Asset Integration Phase)
