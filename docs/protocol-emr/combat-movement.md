# Combat & Movement Systems Specification

## Overview

This document defines the advanced player and NPC movement and combat mechanics for Protocol EMR. The systems are designed to create immersive, responsive gameplay that supports the game's atmosphere of tension and discovery while maintaining technical performance targets.

## Table of Contents

1. [First-Person Combat System](#first-person-combat-system)
2. [Chase & Flee Sequences](#chase--flee-sequences)
3. [Advanced Movement Mechanics](#advanced-movement-mechanics)
4. [NPC Movement & Animation States](#npc-movement--animation-states)
5. [Integration with Gameplay](#integration-with-gameplay)
6. [Technical Requirements](#technical-requirements)
7. [Prototype Deliverables](#prototype-deliverables)
8. [QA and Testing Checklist](#qa-and-testing-checklist)

## First-Person Combat System

### Core Combat Mechanics

The combat system emphasizes visceral, responsive first-person melee combat with realistic physics and impactful feedback.

#### Attack Types

| Attack Type | Damage | Stamina Cost | Animation Duration | Knockback Force |
|-------------|--------|--------------|-------------------|-----------------|
| **Light Punch** | 15 | 5 | 0.4s | 50N |
| **Heavy Punch** | 25 | 12 | 0.6s | 100N |
| **Light Kick** | 20 | 8 | 0.5s | 75N |
| **Heavy Kick** | 35 | 15 | 0.7s | 150N |
| **Dodge Attack** | 30 | 10 | 0.3s | 125N |

#### Combo System

```javascript
// Combo state machine
ComboSystem {
  currentCombo: int,
  comboTimer: float,
  maxComboLength: 3,
  comboWindow: 1.2, // seconds
  
  combos: [
    { sequence: ["light_punch", "light_punch", "heavy_kick"], damage: 1.5 },
    { sequence: ["heavy_punch", "light_kick"], damage: 1.3 },
    { sequence: ["light_kick", "light_punch", "heavy_punch"], damage: 1.8 }
  ]
}
```

#### Hit Detection System

**Hit Box Configuration**:
```csharp
// Player attack hit boxes
public class AttackHitBox : MonoBehaviour {
    public Vector3 center = Vector3.zero;
    public Vector3 size = new Vector3(0.5f, 0.5f, 1.0f);
    public float damage = 25f;
    public float knockbackForce = 100f;
    public LayerMask targetLayers;
    public bool isActive = false;
    
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + center, size);
    }
}
```

**Hit Registration Flow**:
1. Attack animation begins → Hit box activates
2. Physics overlap detection checks for valid targets
3. Hit confirmed → Apply damage and knockback
4. Hit box deactivates → Cooldown period

#### Impact Feedback System

| Feedback Type | Implementation | Intensity Scaling |
|---------------|----------------|-------------------|
| **Screen Shake** | Cinemachine Impulse Source | Damage-based (0.1-0.5s) |
| **Sound Impact** | FMOD Event (one-shot) | Distance and material based |
| **Visual Flash** | Post-processing vignette | Critical hit highlight |
| **Controller Rumble** | Haptic feedback | Impact strength mapping |
| **Blood Spatter** | Particle system | Hit location and force |

#### NPC Reaction System

**Reaction Types**:
- **Light Hit**: Stagger animation (0.3s), brief pause
- **Medium Hit**: Knockback (1-2m), recovery animation (0.8s)
- **Heavy Hit**: Fall animation (1.2s), ground recovery (1.5s)
- **Critical Hit**: Ragdoll trigger, extended recovery

**Reaction State Machine**:
```
NPC Reaction States:
├── Normal
├── HitStagger (light damage)
│   ├── Animation: Stagger_Left/Right
│   ├── Duration: 0.3s
│   └── Recovery: Return to combat stance
├── Knockback (medium damage)
│   ├── Physics: Apply force vector
│   ├── Animation: Fall_Backward
│   ├── Duration: 0.8s
│   └── Recovery: Get_Up animation
└── Ragdoll (heavy/critical damage)
    ├── Physics: Full ragdoll activation
    ├── Duration: 2-3s
    └── Recovery: Blend to animated stance
```

#### Blood and Injury Effects

**Visual Injury System**:
```javascript
// Injury visualization configuration
InjurySystem {
  woundTypes: [
    {
      name: "punch_wound",
      particleEffect: "blood_spatter_small",
      decalTexture: "bruise_light",
      duration: 30, // seconds
      fadeOut: true
    },
    {
      name: "kick_wound", 
      particleEffect: "blood_spatter_medium",
      decalTexture: "bruise_heavy",
      duration: 45,
      fadeOut: true
    }
  ],
  
  applyWound(target, hitLocation, damageType) {
    // Spawn particle effect at hit location
    // Apply decal to target mesh
    // Track wound for healing/fade logic
  }
}
```

**Blood Spatter Physics**:
- Particle count: 20-50 particles per hit
- Velocity: 2-8 m/s based on impact force
- Surface adhesion: Dynamic collision with environment
- Lifetime: 5-30 seconds with fade-out
- Color variation: Bright red (fresh) to dark brown (old)

#### Combat Audio Design

**Audio Event Mapping**:
| Event | FMOD Event | Volume Range | Pitch Variation |
|-------|------------|-------------|-----------------|
| Player Light Punch | combat/player/punch_light | -12dB to -6dB | ±0.1 semitones |
| Player Heavy Punch | combat/player/punch_heavy | -9dB to -3dB | ±0.15 semitones |
| Player Light Kick | combat/player/kick_light | -10dB to -4dB | ±0.1 semitones |
| Player Heavy Kick | combat/player/kick_heavy | -6dB to 0dB | ±0.2 semitones |
| NPC Hit Light | combat/npc/hit_light | -15dB to -9dB | ±0.2 semitones |
| NPC Hit Heavy | combat/npc/hit_heavy | -12dB to -6dB | ±0.3 semitones |
| NPC Groan | combat/npc/groan | -18dB to -12dB | ±0.25 semitones |

**Audio Implementation**:
```csharp
public class CombatAudioManager : MonoBehaviour {
    [FMODUnity.EventRef]
    public string punchLightEvent;
    
    [FMODUnity.EventRef]
    public string punchHeavyEvent;
    
    public void PlayAttackSound(AttackType type, Vector3 position) {
        FMOD.Studio.EventInstance instance;
        
        switch(type) {
            case AttackType.LightPunch:
                instance = FMODUnity.RuntimeManager.CreateInstance(punchLightEvent);
                break;
            case AttackType.HeavyPunch:
                instance = FMODUnity.RuntimeManager.CreateInstance(punchHeavyEvent);
                break;
        }
        
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
        instance.start();
        instance.release();
    }
}
```

## Chase & Flee Sequences

### Player Sprint Mechanics

**Sprint Parameters**:
```csharp
public class PlayerSprint : MonoBehaviour {
    [Header("Sprint Settings")]
    public float sprintSpeed = 8.0f;
    public float normalSpeed = 4.0f;
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f; // per second
    public float staminaRecoveryRate = 15f; // per second
    public float exhaustionThreshold = 20f;
    
    [Header("Exhaustion Effects")]
    public float exhaustedSpeedMultiplier = 0.5f;
    public float exhaustionDuration = 3.0f;
    public AnimationCurve breathingIntensity;
}
```

**Stamina Management**:
- **Full Stamina**: 100% sprint speed, normal breathing
- **Medium Stamina** (50-100%): Reduced speed to 85%, moderate breathing
- **Low Stamina** (20-50%): Reduced speed to 70%, heavy breathing
- **Exhausted** (0-20%): Speed reduced to 50%, forced walking, gasping

**Breathing Audio System**:
```javascript
// Breathing state configuration
BreathingStates = {
  normal: {
    audioEvent: "player/breathing_normal",
    volume: -24dB,
    interval: 2.5s,
    variation: ±0.3s
  },
  exerted: {
    audioEvent: "player/breathing_exerted", 
    volume: -18dB,
    interval: 1.8s,
    variation: ±0.2s
  },
  heavy: {
    audioEvent: "player/breathing_heavy",
    volume: -12dB, 
    interval: 1.2s,
    variation: ±0.15s
  },
  exhausted: {
    audioEvent: "player/breathing_exhausted",
    volume: -6dB,
    interval: 0.8s,
    variation: ±0.1s
  }
}
```

### NPC Pursuit AI

**Pursuit State Machine**:
```
NPC Pursuit States:
├── Idle
├── Alert (player detected)
├── Investigate (move to last known position)
├── Chase (direct pursuit)
│   ├── Walk (far distance > 15m)
│   ├── Run (medium distance 8-15m)
│   └── Sprint (close distance < 8m)
├── Attack (within melee range)
└── Flee (health low or overwhelmed)
```

**AI Movement Speeds**:
| State | Speed Multiplier | Animation | Audio |
|-------|------------------|-----------|-------|
| Walk | 0.4x base speed | Walk_Cycle | footsteps_normal |
| Run | 0.7x base speed | Run_Cycle | footsteps_heavy |
| Sprint | 1.0x base speed | Sprint_Cycle | footsteps_panic |
| Attack | 0.2x base speed | Attack_Anims | attack_grunts |

**Dynamic Difficulty Scaling**:
```javascript
// Chase difficulty adjustment
ChaseDifficulty = {
  playerPerformance: {
    successfulEscapes: 0,
    averageEscapeTime: 0,
    combatSuccessRate: 0
  },
  
  adjustDifficulty() {
    if (this.playerPerformance.combatSuccessRate > 0.8) {
      // Player is very successful - increase challenge
      this.npcSpeedMultiplier *= 1.1;
      this.npcDetectionRange *= 1.05;
    } else if (this.playerPerformance.successfulEscapes < 0.3) {
      // Player struggling - reduce difficulty
      this.npcSpeedMultiplier *= 0.9;
      this.npcStaminaDrainRate *= 1.2;
    }
  }
}
```

### Camera and Visual Effects

**Chase Camera System**:
```csharp
public class ChaseCamera : MonoBehaviour {
    [Header("Camera Shake")]
    public AnimationCurve shakeIntensity;
    public float maxShakeIntensity = 0.3f;
    public float shakeDuration = 0.5f;
    
    [Header("Field of View")]
    public float normalFOV = 75f;
    public float chaseFOV = 85f;
    public float fovTransitionSpeed = 2f;
    
    [Header("Motion Blur")]
    public float normalMotionBlur = 0f;
    public float chaseMotionBlur = 0.5f;
    
    void UpdateChaseEffects(float playerSpeed, float distanceToThreat) {
        // Dynamic FOV based on speed
        float targetFOV = Mathf.Lerp(normalFOV, chaseFOV, playerSpeed / 10f);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
        
        // Camera shake during close encounters
        if (distanceToThreat < 5f) {
            TriggerCameraShake();
        }
    }
}
```

**Visual Intensity Indicators**:
- **Heartbeat Effect**: Screen pulse synchronization with breathing
- **Vision Tunnel**: Vignette intensification during exhaustion
- **Motion Trails**: Speed-based motion blur
- **Color Grading**: Desaturation during high-stress moments

### Exit Strategies and Environmental Interaction

**Escape Mechanics**:
| Escape Type | Input Requirement | Success Chance | Stamina Cost |
|-------------|-------------------|----------------|--------------|
| **Hide** | Hold near cover | 70% (varies by cover) | 5 |
| **Jump Barrier** | Space + Direction | 60% (height dependent) | 15 |
| **Climb Ledge** | Hold at ledge | 85% (stamina dependent) | 20 |
| **Break Door** | Rapid clicks | 40% (door strength) | 25 |
| **Slide Under** | Crouch + Forward | 90% (clearance check) | 10 |

**Environmental Interaction Points**:
```javascript
// Escape point configuration
EscapePoint = {
  type: "ledge_climb",
  position: Vector3,
  height: 2.5, // meters
  requiredStamina: 20,
  animation: "climb_ledge_medium",
  successCondition: function(player) {
    return player.stamina >= this.requiredStamina && 
           player.distanceTo(this.position) < 1.0;
  },
  onInteraction: function(player) {
    player.consumeStamina(this.requiredStamina);
    player.playAnimation(this.animation);
    player.teleportTo(this.position + Vector3.up * this.height);
  }
}
```

## Advanced Movement Mechanics

### Parkour System

**Parkour Move Set**:
| Move | Input | Height/Distance | Stamina | Animation Duration |
|------|-------|-----------------|---------|-------------------|
| **Vault** | Space + Forward | 1.2m height | 12 | 0.8s |
| **Climb Low** | Space + Forward | 0.8m height | 8 | 0.6s |
| **Climb High** | Hold Space | 2.5m height | 25 | 1.5s |
| **Ledge Grab** | Space at edge | 0.5m reach | 15 | 0.4s |
| **Wall Jump** | Space at wall | 3m horizontal | 18 | 0.7s |
| **Slide** | Crouch + Forward | N/A | 5 | Variable |

**Parkour Detection System**:
```csharp
public class ParkourDetector : MonoBehaviour {
    [Header("Detection Settings")]
    public float vaultDetectionRange = 2.0f;
    public float climbDetectionHeight = 3.0f;
    public LayerMask obstacleLayer;
    
    [Header("Cast Points")]
    public Transform vaultCheckPoint;
    public Transform climbCheckPoint;
    public Transform ledgeCheckPoint;
    
    void Update() {
        CheckForVaultOpportunity();
        CheckForClimbOpportunity();
        CheckForLedgeGrabOpportunity();
    }
    
    bool CheckForVaultOpportunity() {
        RaycastHit hit;
        Vector3 castDirection = transform.forward;
        
        if (Physics.Raycast(vaultCheckPoint.position, castDirection, out hit, vaultDetectionRange, obstacleLayer)) {
            if (hit.collider.height <= 1.5f) {
                return true; // Vaultable obstacle detected
            }
        }
        return false;
    }
}
```

### Animation Blending System

**Blend Tree Configuration**:
```csharp
public class MovementAnimator : MonoBehaviour {
    private Animator animator;
    private float currentSpeed;
    private float verticalVelocity;
    private bool isGrounded;
    private bool isSprinting;
    
    void Update() {
        // Update animation parameters
        animator.SetFloat("Speed", currentSpeed);
        animator.SetFloat("VerticalSpeed", verticalVelocity);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetFloat("Direction", GetMovementDirection());
    }
    
    float GetMovementDirection() {
        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        float angle = Vector3.Angle(transform.forward, inputDirection);
        
        // Convert to -1 to 1 range for blend tree
        if (Vector3.Cross(transform.forward, inputDirection).y < 0) {
            angle = -angle;
        }
        
        return angle / 180f; // Normalize to -1 to 1
    }
}
```

**Blend Tree Weights**:
- **Idle → Walk**: 0.2s blend time
- **Walk → Run**: 0.15s blend time  
- **Run → Sprint**: 0.1s blend time
- **Jump → Fall**: 0.05s blend time
- **Land → Idle**: 0.3s blend time

### Slope Handling and Foot Placement

**Slope Detection**:
```csharp
public class SlopeHandler : MonoBehaviour {
    [Header("Slope Settings")]
    public float maxSlopeAngle = 45f;
    public float slopeCheckDistance = 1.0f;
    public float stepHeight = 0.3f;
    
    void HandleSlopeMovement() {
        RaycastHit slopeHit;
        
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, slopeCheckDistance)) {
            float slopeAngle = Vector3.Angle(slopeHit.normal, Vector3.up);
            
            if (slopeAngle < maxSlopeAngle) {
                // Adjust movement to follow slope
                Vector3 slopeDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
                characterController.Move(slopeDirection * moveSpeed * Time.deltaTime);
            } else {
                // Too steep - slide down
                Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal);
                characterController.Move(slideDirection * slideSpeed * Time.deltaTime);
            }
        }
    }
}
```

**IK Foot Placement**:
```csharp
public class FootIK : MonoBehaviour {
    [Header("IK Settings")]
    public float footHeight = 0.1f;
    public float footRayLength = 0.5f;
    public LayerMask groundLayer;
    
    void OnAnimatorIK(int layerIndex) {
        // Left foot
        Vector3 leftFootPosition = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        RaycastHit leftHit;
        
        if (Physics.Raycast(leftFootPosition + Vector3.up * footRayLength, Vector3.down, out leftHit, footRayLength * 2, groundLayer)) {
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftHit.point + Vector3.up * footHeight);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, leftHit.normal), leftHit.normal));
        }
        
        // Right foot (similar implementation)
    }
}
```

### Landing Impact System

**Landing Detection**:
```javascript
// Landing impact calculation
LandingSystem = {
  calculateImpact(fallVelocity) {
    const impactForce = Math.abs(fallVelocity) * this.playerMass;
    const impactCategory = this.categorizeImpact(impactForce);
    
    return {
      force: impactForce,
      category: impactCategory,
      damage: this.calculateDamage(impactForce),
      screenShake: this.calculateScreenShake(impactForce),
      audioEvent: this.getLandingSound(impactCategory),
      animation: this.getLandingAnimation(impactCategory)
    };
  },
  
  categorizeImpact(force) {
    if (force < 500) return "soft";
    if (force < 1500) return "medium"; 
    if (force < 3000) return "hard";
    return "crushing";
  },
  
  getLandingAnimation(category) {
    const animations = {
      soft: "land_soft",
      medium: "land_medium", 
      hard: "land_hard",
      crushing: "land_crushing"
    };
    return animations[category];
  }
}
```

**Landing Audio Events**:
| Impact Category | FMOD Event | Volume | Low Pass Filter |
|-----------------|------------|--------|-----------------|
| Soft (0-1m) | player/land_soft | -24dB | No filter |
| Medium (1-2m) | player/land_medium | -18dB | Light filter |
| Hard (2-3m) | player/land_hard | -12dB | Medium filter |
| Heavy (3-4m) | player/land_heavy | -6dB | Heavy filter |
| Crushing (4m+) | player/land_crushing | 0dB | Maximum filter |

### Sprint Exhaustion System

**Exhaustion States**:
```csharp
public class ExhaustionSystem : MonoBehaviour {
    public enum ExhaustionLevel {
        None,
        Light,
        Moderate,
        Heavy,
        Critical
    }
    
    [System.Serializable]
    public class ExhaustionSettings {
        public ExhaustionLevel level;
        public float staminaThreshold;
        public float speedMultiplier;
        public float recoveryRate;
        public AnimationCurve breathingIntensity;
        public float cameraShakeAmount;
        public Color visionTint;
    }
    
    public ExhaustionSettings[] exhaustionLevels;
    private ExhaustionLevel currentLevel;
    
    void UpdateExhaustion() {
        float staminaPercent = playerStamina / maxStamina;
        currentLevel = CalculateExhaustionLevel(staminaPercent);
        
        ApplyExhaustionEffects(currentLevel);
    }
    
    void ApplyExhaustionEffects(ExhaustionLevel level) {
        var settings = GetExhaustionSettings(level);
        
        // Apply movement penalty
        moveSpeed = baseMoveSpeed * settings.speedMultiplier;
        
        // Update breathing audio
        UpdateBreathingAudio(settings.breathingIntensity);
        
        // Apply camera effects
        ApplyCameraEffects(settings.cameraShakeAmount, settings.visionTint);
    }
}
```

**Recovery Mechanics**:
- **Passive Recovery**: 15 stamina/second when not sprinting
- **Active Recovery**: 25 stamina/second when crouching/resting
- **Item Recovery**: 50 instant stamina from energy consumables
- **Environmental Recovery**: 35 stamina/second near rest points

### Crouch and Prone System

**Stance Configuration**:
| Stance | Height | Speed Multiplier | Detection Chance | Audio Level |
|--------|--------|------------------|------------------|-------------|
| Standing | 1.8m | 1.0x | 100% | Normal |
| Crouch | 0.9m | 0.6x | 60% | -12dB |
| Prone | 0.4m | 0.3x | 25% | -24dB |

**Stance Transition System**:
```csharp
public class StanceController : MonoBehaviour {
    public enum Stance { Standing, Crouching, Prone }
    
    [Header("Stance Settings")]
    public float standingHeight = 1.8f;
    public float crouchHeight = 0.9f;
    public float proneHeight = 0.4f;
    public float stanceTransitionSpeed = 5f;
    
    private Stance currentStance;
    private float targetHeight;
    private float currentHeight;
    
    void Update() {
        HandleStanceInput();
        UpdateStanceTransition();
        UpdateCharacterController();
    }
    
    void HandleStanceInput() {
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            CycleStance();
        }
        
        if (Input.GetKeyDown(KeyCode.C)) {
            SetStance(Stance.Crouching);
        }
        
        if (Input.GetKeyDown(KeyCode.Z)) {
            SetStance(Stance.Prone);
        }
    }
    
    void SetStance(Stance newStance) {
        currentStance = newStance;
        
        switch (newStance) {
            case Stance.Standing:
                targetHeight = standingHeight;
                moveSpeed = baseMoveSpeed;
                break;
            case Stance.Crouching:
                targetHeight = crouchHeight;
                moveSpeed = baseMoveSpeed * 0.6f;
                break;
            case Stance.Prone:
                targetHeight = proneHeight;
                moveSpeed = baseMoveSpeed * 0.3f;
                break;
        }
    }
}
```

## NPC Movement & Animation States

### Animation State Architecture

**Master Animation Controller**:
```
NPC Animation Layers:
├── Base Layer (Movement)
│   ├── Idle
│   ├── Walk
│   ├── Run
│   ├── Sprint
│   └── Turn
├── Combat Layer
│   ├── Idle_Combat
│   ├── Attack_Light
│   ├── Attack_Heavy
│   ├── Block
│   ├── Dodge_Left
│   ├── Dodge_Right
│   └── Hit_Reaction
├── Injury Layer
│   ├── Stagger_Light
│   ├── Stagger_Heavy
│   ├── Fall_Forward
│   ├── Fall_Backward
│   └── Get_Up
└── Death Layer
    ├── Death_Standing
    ├── Death_Kneeling
    └── Ragdoll_Activation
```

**State Machine Configuration**:
```csharp
public class NPCAnimationController : MonoBehaviour {
    private Animator animator;
    private NPCController npcController;
    
    [Header("Animation Parameters")]
    private const string SPEED = "Speed";
    private const string VERTICAL_SPEED = "VerticalSpeed";
    private const string IS_GROUNDED = "IsGrounded";
    private const string IS_IN_COMBAT = "IsInCombat";
    private const string ATTACK_TYPE = "AttackType";
    private const string HIT_DIRECTION = "HitDirection";
    private const string IS_DEAD = "IsDead";
    
    void Update() {
        UpdateMovementParameters();
        UpdateCombatParameters();
        UpdateInjuryParameters();
    }
    
    void UpdateMovementParameters() {
        Vector3 velocity = npcController.velocity;
        float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        
        animator.SetFloat(SPEED, speed);
        animator.SetFloat(VERTICAL_SPEED, velocity.y);
        animator.SetBool(IS_GROUNDED, npcController.isGrounded);
    }
    
    void UpdateCombatParameters() {
        animator.SetBool(IS_IN_COMBAT, npcController.isInCombat);
        
        if (npcController.isAttacking) {
            animator.SetInteger(ATTACK_TYPE, (int)npcController.currentAttackType);
        }
    }
    
    void UpdateInjuryParameters() {
        if (npcController.wasHit) {
            Vector3 hitDirection = npcController.lastHitDirection;
            float angle = Vector3.Angle(transform.forward, -hitDirection);
            
            // Convert angle to parameter for directional hit reaction
            animator.SetFloat(HIT_DIRECTION, angle);
        }
    }
}
```

### Footstep Audio System

**Surface-Based Audio**:
```csharp
public class FootstepAudio : MonoBehaviour {
    [System.Serializable]
    public class SurfaceSound {
        public SurfaceType surfaceType;
        public FMODUnity.EventRef[] footstepSounds;
        public float volumeRange = 0.2f;
        public float pitchRange = 0.1f;
    }
    
    public enum SurfaceType {
        Concrete, Metal, Carpet, Grass, Dirt, Water, Wood, Glass
    }
    
    [Header("Footstep Settings")]
    public SurfaceSound[] surfaceSounds;
    public float footstepInterval = 0.5f;
    public LayerMask surfaceLayer;
    
    private float nextFootstepTime;
    private bool isLeftFoot = true;
    
    void Update() {
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.5f) {
            if (Time.time >= nextFootstepTime) {
                PlayFootstep();
                nextFootstepTime = Time.time + footstepInterval;
                isLeftFoot = !isLeftFoot;
            }
        }
    }
    
    void PlayFootstep() {
        SurfaceType surface = DetectSurfaceType();
        SurfaceSound soundConfig = GetSurfaceSound(surface);
        
        if (soundConfig != null && soundConfig.footstepSounds.Length > 0) {
            FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(
                soundConfig.footstepSounds[Random.Range(0, soundConfig.footstepSounds.Length)]
            );
            
            // Add random variation
            instance.setVolume(Random.Range(1f - soundConfig.volumeRange, 1f));
            instance.setPitch(Random.Range(1f - soundConfig.pitchRange, 1f + soundConfig.pitchRange));
            
            instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            instance.start();
            instance.release();
        }
    }
    
    SurfaceType DetectSurfaceType() {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 1.0f, surfaceLayer)) {
            SurfaceDetector surfaceDetector = hit.collider.GetComponent<SurfaceDetector>();
            if (surfaceDetector != null) {
                return surfaceDetector.surfaceType;
            }
        }
        
        return SurfaceType.Concrete; // Default surface
    }
}
```

**Footstep Audio Library**:
| Surface | Event Path | Variations | Room Tone |
|---------|------------|------------|-----------|
| Concrete | npc/footsteps/concrete | 8 | Medium reverb |
| Metal | npc/footsteps/metal | 6 | High metallic reverb |
| Carpet | npc/footsteps/carpet | 5 | Low muffling |
| Grass | npc/footsteps/grass | 7 | Outdoor ambiance |
| Dirt | npc/footsteps/dirt | 6 | Dry texture |
| Water | npc/footsteps/water | 4 | Splash effects |

### Collision Avoidance and Pathfinding

**Local Avoidance System**:
```csharp
public class LocalAvoidance : MonoBehaviour {
    [Header("Avoidance Settings")]
    public float avoidanceRadius = 1.0f;
    public float predictionTime = 1.0f;
    public LayerMask obstacleLayer;
    public LayerMask npcLayer;
    
    void Update() {
        Vector3 avoidanceForce = CalculateAvoidanceForce();
        Vector3 desiredDirection = (targetPosition - transform.position).normalized;
        
        // Combine desired direction with avoidance
        Vector3 finalDirection = (desiredDirection + avoidanceForce).normalized;
        characterController.Move(finalDirection * moveSpeed * Time.deltaTime);
    }
    
    Vector3 CalculateAvoidanceForce() {
        Vector3 avoidanceForce = Vector3.zero;
        
        // Check for static obstacles
        Collider[] obstacles = Physics.OverlapSphere(transform.position, avoidanceRadius, obstacleLayer);
        foreach (Collider obstacle in obstacles) {
            Vector3 awayFromObstacle = transform.position - obstacle.transform.position;
            float distance = awayFromObstacle.magnitude;
            
            if (distance < avoidanceRadius && distance > 0) {
                float strength = 1.0f - (distance / avoidanceRadius);
                avoidanceForce += (awayFromObstacle / distance) * strength;
            }
        }
        
        // Check for other NPCs
        Collider[] npcs = Physics.OverlapSphere(transform.position, avoidanceRadius * 1.5f, npcLayer);
        foreach (Collider npc in npcs) {
            if (npc.gameObject != gameObject) {
                NPCMovement otherNPC = npc.GetComponent<NPCMovement>();
                if (otherNPC != null) {
                    Vector3 separation = CalculateSeparationForce(otherNPC);
                    avoidanceForce += separation;
                }
            }
        }
        
        return avoidanceForce.normalized;
    }
    
    Vector3 CalculateSeparationForce(NPCMovement otherNPC) {
        Vector3 currentPos = transform.position;
        Vector3 otherPos = otherNPC.transform.position;
        Vector3 otherVelocity = otherNPC.velocity;
        
        // Predict future positions
        Vector3 predictedCurrent = currentPos + velocity * predictionTime;
        Vector3 predictedOther = otherPos + otherVelocity * predictionTime;
        
        Vector3 separation = predictedCurrent - predictedOther;
        float distance = separation.magnitude;
        
        if (distance < avoidanceRadius * 2 && distance > 0) {
            float strength = 1.0f - (distance / (avoidanceRadius * 2));
            return (separation / distance) * strength * 2; // Stronger separation for NPCs
        }
        
        return Vector3.zero;
    }
}
```

**NavMesh Integration**:
```csharp
public class NPCNavigation : MonoBehaviour {
    private NavMeshAgent navMeshAgent;
    private LocalAvoidance localAvoidance;
    
    [Header("Navigation Settings")]
    public float pathUpdateInterval = 0.5f;
    public float stuckThreshold = 0.1f;
    public float maxStuckTime = 2.0f;
    
    private Vector3 lastPosition;
    private float stuckTimer;
    
    void Start() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        localAvoidance = GetComponent<LocalAvoidance>();
        
        InvokeRepeating(nameof(UpdatePath), 0f, pathUpdateInterval);
    }
    
    void UpdatePath() {
        if (navMeshAgent.isOnNavMesh && navMeshAgent.destination != null) {
            navMeshAgent.SetDestination(targetPosition);
        }
    }
    
    void Update() {
        CheckIfStuck();
        HandleNavigation();
    }
    
    void CheckIfStuck() {
        float movementThisFrame = (transform.position - lastPosition).magnitude;
        
        if (movementThisFrame < stuckThreshold) {
            stuckTimer += Time.deltaTime;
            
            if (stuckTimer > maxStuckTime) {
                HandleStuckNPC();
                stuckTimer = 0f;
            }
        } else {
            stuckTimer = 0f;
        }
        
        lastPosition = transform.position;
    }
    
    void HandleStuckNPC() {
        // Try to find alternative path
        Vector3 randomOffset = Random.insideUnitSphere * 5f;
        Vector3 alternativeTarget = targetPosition + randomOffset;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(alternativeTarget, out hit, 10f, NavMesh.AllAreas)) {
            navMeshAgent.SetDestination(hit.position);
        }
    }
    
    void HandleNavigation() {
        if (localAvoidance != null) {
            // Use local avoidance for fine movement
            Vector3 avoidanceForce = localAvoidance.CalculateAvoidanceForce();
            
            if (avoidanceForce.magnitude > 0.1f) {
                // Override NavMesh with local avoidance
                navMeshAgent.velocity = avoidanceForce * navMeshAgent.speed;
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
            } else {
                // Return to NavMesh control
                navMeshAgent.updatePosition = true;
                navMeshAgent.updateRotation = true;
            }
        }
    }
}
```

### Ragdoll Physics Integration

**Ragdoll Activation System**:
```csharp
public class NPCRagdoll : MonoBehaviour {
    [Header("Ragdoll Settings")]
    public float ragdollActivationForce = 500f;
    public float blendToRagdollDuration = 0.2f;
    public float ragdollDuration = 3.0f;
    
    private Rigidbody[] ragdollBodies;
    private CharacterController characterController;
    private Animator animator;
    private bool isRagdollActive;
    
    void Start() {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        SetRagdollState(false);
    }
    
    public void ActivateRagdoll(Vector3 impactForce, Vector3 impactPoint) {
        if (isRagdollActive) return;
        
        isRagdollActive = true;
        characterController.enabled = false;
        animator.enabled = false;
        
        // Enable ragdoll physics
        SetRagdollState(true);
        
        // Apply impact force
        ApplyImpactForce(impactForce, impactPoint);
        
        // Schedule ragdoll deactivation
        Invoke(nameof(DeactivateRagdoll), ragdollDuration);
    }
    
    void SetRagdollState(bool enabled) {
        foreach (Rigidbody body in ragdollBodies) {
            body.isKinematic = !enabled;
            body.detectCollisions = enabled;
        }
    }
    
    void ApplyImpactForce(Vector3 force, Vector3 point) {
        foreach (Rigidbody body in ragdollBodies) {
            float distance = Vector3.Distance(body.position, point);
            float forceMultiplier = Mathf.Clamp01(1.0f - (distance / 2.0f));
            
            body.AddForce(force * forceMultiplier, ForceMode.Impulse);
        }
    }
    
    void DeactivateRagdoll() {
        if (!isRagdollActive) return;
        
        // Find the main ragdoll body (usually hips/chest)
        Rigidbody mainBody = GetMainRagdollBody();
        
        // Position character at ragdoll position
        transform.position = mainBody.position;
        transform.rotation = mainBody.rotation;
        
        // Disable ragdoll physics
        SetRagdollState(false);
        
        // Re-enable character systems
        characterController.enabled = true;
        animator.enabled = true;
        
        // Play get up animation
        animator.SetTrigger("GetUp");
        
        isRagdollActive = false;
    }
    
    Rigidbody GetMainRagdollBody() {
        // Priority: Hips > Chest > any body
        foreach (Rigidbody body in ragdollBodies) {
            if (body.name.Contains("Hips") || body.name.Contains("Chest")) {
                return body;
            }
        }
        
        return ragdollBodies[0]; // Fallback
    }
}
```

**Death State Management**:
```csharp
public class NPCDeath : MonoBehaviour {
    public enum DeathType {
        Standing,
        Kneeling,
        Falling,
        Ragdoll
    }
    
    [Header("Death Settings")]
    public DeathType defaultDeathType = DeathType.Standing;
    public float deathAnimationDuration = 2.0f;
    
    private NPCRagdoll ragdollSystem;
    private NPCHealth healthSystem;
    private Animator animator;
    
    void Start() {
        ragdollSystem = GetComponent<NPCRagdoll>();
        healthSystem = GetComponent<NPCHealth>();
        animator = GetComponent<Animator>();
        
        healthSystem.OnDeath.AddListener(HandleDeath);
    }
    
    void HandleDeath(Vector3 killingBlowDirection, float killingBlowForce) {
        DeathType deathType = DetermineDeathType(killingBlowDirection, killingBlowForce);
        
        switch (deathType) {
            case DeathType.Ragdoll:
                ActivateRagdollDeath(killingBlowDirection, killingBlowForce);
                break;
            case DeathType.Falling:
                PlayFallingDeath(killingBlowDirection);
                break;
            case DeathType.Kneeling:
                PlayKneelingDeath();
                break;
            default:
                PlayStandingDeath();
                break;
        }
    }
    
    DeathType DetermineDeathType(Vector3 direction, float force) {
        // High force impacts trigger ragdoll
        if (force > 1000f) {
            return DeathType.Ragdoll;
        }
        
        // Directional impacts trigger falling death
        if (direction.magnitude > 0.5f) {
            return DeathType.Falling;
        }
        
        // Low health triggers kneeling death
        if (healthSystem.currentHealth < healthSystem.maxHealth * 0.2f) {
            return DeathType.Kneeling;
        }
        
        return defaultDeathType;
    }
    
    void ActivateRagdollDeath(Vector3 direction, float force) {
        Vector3 impactForce = direction.normalized * force;
        Vector3 impactPoint = transform.position + Vector3.up * 1.0f;
        
        ragdollSystem.ActivateRagdoll(impactForce, impactPoint);
    }
    
    void PlayFallingDeath(Vector3 direction) {
        animator.SetFloat("DeathDirection", GetDeathDirectionParameter(direction));
        animator.SetTrigger("DeathFalling");
        
        // Optionally transition to ragdoll after animation
        Invoke(nameof(TransitionToRagdoll), deathAnimationDuration);
    }
    
    void PlayKneelingDeath() {
        animator.SetTrigger("DeathKneeling");
    }
    
    void PlayStandingDeath() {
        animator.SetTrigger("DeathStanding");
    }
    
    float GetDeathDirectionParameter(Vector3 direction) {
        float angle = Vector3.Angle(transform.forward, -direction);
        
        // Convert to -1 to 1 range for animator
        if (Vector3.Cross(transform.forward, -direction).y < 0) {
            angle = -angle;
        }
        
        return angle / 180f;
    }
}
```

## Integration with Gameplay

### Combat-Mission Integration

**Mission Progression through Combat**:
```javascript
// Combat mission objectives
CombatMissionObjectives = {
  primary: {
    type: "eliminate_threats",
    description: "Neutralize hostiles in the area",
    requiredKills: 5,
    currentKills: 0,
    isComplete() {
      return this.currentKills >= this.requiredKills;
    }
  },
  
  secondary: {
    type: "survive_ambush",
    description: "Survive the enemy ambush",
    timeLimit: 120, // seconds
    startTime: null,
    isComplete() {
      return this.getElapsedTime() < this.timeLimit;
    }
  },
  
  optional: {
    type: "stealth_elimination",
    description: "Eliminate targets without alerting others",
    stealthKills: 0,
    requiredStealthKills: 3,
    isComplete() {
      return this.stealthKills >= this.requiredStealthKills;
    }
  }
}
```

**Combat Event Triggers**:
```csharp
public class CombatMissionManager : MonoBehaviour {
    [Header("Mission Settings")]
    public CombatMission currentMission;
    public float combatEndDelay = 3.0f;
    
    [Header("Area Settings")]
    public Collider combatArea;
    public LayerMask enemyLayer;
    
    private int enemiesInArea;
    private bool combatActive;
    
    void Start() {
        combatArea.isTrigger = true;
    }
    
    void OnTriggerEnter(Collider other) {
        if (IsEnemy(other)) {
            enemiesInArea++;
            if (!combatActive && enemiesInArea > 0) {
                StartCombat();
            }
        }
    }
    
    void OnTriggerExit(Collider other) {
        if (IsEnemy(other)) {
            enemiesInArea--;
            if (combatActive && enemiesInArea == 0) {
                EndCombat();
            }
        }
    }
    
    void StartCombat() {
        combatActive = true;
        
        // Notify AI narrator
        AINarrator.Instance.TriggerEvent("combat_started", currentMission.description);
        
        // Update music
        AudioManager.Instance.SetMusicState("combat");
        
        // Spawn additional enemies if needed
        if (ShouldSpawnReinforcements()) {
            SpawnReinforcements();
        }
    }
    
    void EndCombat() {
        combatActive = false;
        
        // Award mission completion
        AwardMissionRewards();
        
        // Notify systems
        AINarrator.Instance.TriggerEvent("combat_ended", "Area secured");
        AudioManager.Instance.SetMusicState("exploration");
        
        // Trigger next mission phase
        MissionManager.Instance.AdvanceMission();
    }
    
    bool ShouldSpawnReinforcements() {
        return currentMission.difficulty > Difficulty.Normal && 
               Random.value < 0.3f; // 30% chance
    }
}
```

### AI Narrator Tactical Guidance

**Combat Context Hooks**:
```javascript
// AI narrator combat guidance system
CombatNarrator = {
  contextHooks: {
    combat_start: {
      priority: "high",
      messages: [
        "Hostiles detected. Engage with caution.",
        "Combat initiated. Use the environment to your advantage.",
        "Multiple targets. Consider tactical positioning."
      ],
      conditions: {
        enemyCount: "> 1",
        playerHealth: "> 50%"
      }
    },
    
    low_health: {
      priority: "critical",
      messages: [
        "Your vitals are critical. Find cover immediately.",
        "Warning: Severe injuries sustained. Retreat recommended.",
        "Medical supplies may be nearby. Search the area."
      ],
      conditions: {
        playerHealth: "< 25%",
        inCombat: true
      }
    },
    
    successful_combo: {
      priority: "medium",
      messages: [
        "Excellent combat technique. Maintain pressure.",
        "Flawless execution. The enemy is weakening.",
        "Your skill is impressive. Press the advantage."
      ],
      conditions: {
        comboLength: ">= 3",
        lastHitWasCritical: true
      }
    },
    
    tactical_opportunity: {
      priority: "high",
      messages: [
        "Enemy vulnerable. Execute finishing move.",
        "Opening detected. Strike now for maximum effect.",
        "Opponent off-balance. This is your chance."
      ],
      conditions: {
        enemyStunned: true,
        playerStamina: "> 30%"
      }
    },
    
    escape_advice: {
      priority: "high",
      messages: [
        "Overwhelmed. Tactical retreat advised.",
        "Multiple hostiles converging. Find escape route.",
        "Disengage and regroup. Live to fight another day."
      ],
      conditions: {
        enemyCount: "> 3",
        playerHealth: "< 40%",
        nearbyEscapePoints: "> 0"
      }
    }
  },
  
  evaluateContext(gameState) {
    for (const [hookName, hookData] of Object.entries(this.contextHooks)) {
      if (this.evaluateConditions(hookData.conditions, gameState)) {
        this.deliverMessage(hookData);
        break; // Only deliver one message per evaluation
      }
    }
  },
  
  evaluateConditions(conditions, gameState) {
    for (const [condition, requirement] of Object.entries(conditions)) {
      if (!this.meetsCondition(condition, requirement, gameState)) {
        return false;
      }
    }
    return true;
  },
  
  meetsCondition(condition, requirement, gameState) {
    const value = this.getGameStateValue(condition, gameState);
    
    if (requirement.startsWith(">")) {
      return value > parseFloat(requirement.substring(1));
    } else if (requirement.startsWith("<")) {
      return value < parseFloat(requirement.substring(1));
    } else if (requirement.startsWith(">=")) {
      return value >= parseFloat(requirement.substring(2));
    } else if (requirement.startsWith("<=")) {
      return value <= parseFloat(requirement.substring(2));
    } else {
      return value === requirement;
    }
  }
}
```

### Dynamic Difficulty Scaling

**Performance-Based Adjustment**:
```csharp
public class DynamicDifficulty : MonoBehaviour {
    [Header("Difficulty Settings")]
    public float adjustmentInterval = 30.0f;
    public float maxDifficultyMultiplier = 2.0f;
    public float minDifficultyMultiplier = 0.5f;
    
    [Header("Performance Tracking")]
    public float combatSuccessWeight = 0.4f;
    public float damageTakenWeight = 0.3f;
    public float completionTimeWeight = 0.3f;
    
    private PlayerPerformanceTracker performanceTracker;
    private float currentDifficultyMultiplier = 1.0f;
    
    void Start() {
        performanceTracker = GetComponent<PlayerPerformanceTracker>();
        InvokeRepeating(nameof(AdjustDifficulty), adjustmentInterval, adjustmentInterval);
    }
    
    void AdjustDifficulty() {
        float performanceScore = CalculatePerformanceScore();
        float targetMultiplier = CalculateDifficultyMultiplier(performanceScore);
        
        // Smooth transition to new difficulty
        currentDifficultyMultiplier = Mathf.Lerp(
            currentDifficultyMultiplier, 
            targetMultiplier, 
            0.2f
        );
        
        ApplyDifficultyAdjustments();
    }
    
    float CalculatePerformanceScore() {
        float combatScore = performanceTracker.combatSuccessRate * combatSuccessWeight;
        float damageScore = (1.0f - performanceTracker.damageTakenRatio) * damageTakenWeight;
        float timeScore = performanceTracker.completionTimeScore * completionTimeWeight;
        
        return combatScore + damageScore + timeScore;
    }
    
    float CalculateDifficultyMultiplier(float performanceScore) {
        // Map performance score (0-1) to difficulty multiplier
        if (performanceScore > 0.8f) {
            // Player doing very well - increase difficulty
            return Mathf.Min(maxDifficultyMultiplier, 1.0f + (performanceScore - 0.8f) * 5f);
        } else if (performanceScore < 0.4f) {
            // Player struggling - decrease difficulty
            return Mathf.Max(minDifficultyMultiplier, 0.5f + performanceScore * 0.5f);
        }
        
        return 1.0f; // Normal difficulty
    }
    
    void ApplyDifficultyAdjustments() {
        // Apply to enemy health
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in enemies) {
            enemy.maxHealth = enemy.baseMaxHealth * currentDifficultyMultiplier;
            enemy.currentHealth = Mathf.Min(enemy.currentHealth, enemy.maxHealth);
        }
        
        // Apply to enemy damage
        EnemyCombat[] enemyCombat = FindObjectsOfType<EnemyCombat>();
        foreach (EnemyCombat combat in enemyCombat) {
            combat.damageMultiplier = currentDifficultyMultiplier;
        }
        
        // Apply to enemy speed
        EnemyMovement[] enemyMovement = FindObjectsOfType<EnemyMovement>();
        foreach (EnemyMovement movement in enemyMovement) {
            movement.speedMultiplier = 1.0f + (currentDifficultyMultiplier - 1.0f) * 0.3f;
        }
    }
}
```

**Skill-Based Challenges**:
| Skill Level | Enemy Health | Enemy Damage | Enemy Speed | Special Abilities |
|-------------|--------------|--------------|-------------|-------------------|
| Novice | 0.7x | 0.8x | 0.9x | None |
| Normal | 1.0x | 1.0x | 1.0x | Basic attacks |
| Expert | 1.3x | 1.2x | 1.1x | Dodging, blocking |
| Master | 1.6x | 1.4x | 1.2x | Combo attacks, tactical movement |

### Consequences and Choice System

**Stealth vs Aggression Mechanics**:
```csharp
public class CombatChoiceSystem : MonoBehaviour {
    [Header("Choice Tracking")]
    public float stealthBonusMultiplier = 1.5f;
    public float aggressionPenaltyMultiplier = 0.8f;
    
    [Header("Consequence Settings")]
    public int maxAlertLevel = 5;
    public float alertDecayRate = 0.1f; // per second
    
    private int currentAlertLevel;
    private int stealthEliminations;
    private int aggressiveEngagements;
    private bool lastActionWasStealthy;
    
    public enum Playstyle {
        Stealth,
        Balanced,
        Aggressive
    }
    
    void Update() {
        // Decay alert level over time
        if (currentAlertLevel > 0) {
            currentAlertLevel = Mathf.Max(0, currentAlertLevel - Mathf.RoundToInt(alertDecayRate * Time.deltaTime));
        }
    }
    
    public void RegisterStealthElimination() {
        stealthEliminations++;
        lastActionWasStealthy = true;
        
        // Apply stealth bonuses
        GrantStealthBonus();
        
        // Notify narrator
        AINarrator.Instance.TriggerEvent("stealth_elimination", "Silent takedown successful");
    }
    
    public void RegisterAggressiveEngagement() {
        aggressiveEngagements++;
        lastActionWasStealthy = false;
        
        // Increase alert level
        currentAlertLevel = Mathf.Min(maxAlertLevel, currentAlertLevel + 1);
        
        // Apply aggression consequences
        ApplyAggressionConsequences();
        
        // Notify narrator
        AINarrator.Instance.TriggerEvent("aggressive_engagement", "Combat detected");
    }
    
    void GrantStealthBonus() {
        // Bonus experience points
        ExperienceManager.Instance.AwardExperience(50 * stealthBonusMultiplier);
        
        // Reduced enemy detection for duration
        PlayerStealth playerStealth = FindObjectOfType<PlayerStealth>();
        if (playerStealth != null) {
            playerStealth.ApplyDetectionReduction(0.5f, 30.0f);
        }
        
        // Unlock stealth paths
        MissionManager.Instance.UnlockStealthPaths();
    }
    
    void ApplyAggressionConsequences() {
        // Spawn additional enemies
        if (currentAlertLevel >= 3) {
            EnemySpawner.Instance.SpawnReinforcements(2);
        }
        
        // Lock stealth options temporarily
        MissionManager.Instance.LockStealthPaths(60.0f);
        
        // Increase enemy alertness
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies) {
            enemy.IncreaseAlertness(0.2f);
        }
    }
    
    public Playstyle GetCurrentPlaystyle() {
        float totalActions = stealthEliminations + aggressiveEngagements;
        
        if (totalActions == 0) return Playstyle.Balanced;
        
        float stealthRatio = stealthEliminations / totalActions;
        
        if (stealthRatio > 0.7f) return Playstyle.Stealth;
        if (stealthRatio < 0.3f) return Playstyle.Aggressive;
        return Playstyle.Balanced;
    }
    
    public void ApplyPlaystyleConsequences() {
        Playstyle playstyle = GetCurrentPlaystyle();
        
        switch (playstyle) {
            case Playstyle.Stealth:
                ApplyStealthConsequences();
                break;
            case Playstyle.Aggressive:
                ApplyAggressiveConsequences();
                break;
            case Playstyle.Balanced:
                ApplyBalancedConsequences();
                break;
        }
    }
    
    void ApplyStealthConsequences() {
        // Unlock stealth-focused rewards
        RewardManager.Instance.UnlockStealthRewards();
        
        // Favor dialogue options that value stealth
        DialogueManager.Instance.SetStealthPreference(true);
    }
    
    void ApplyAggressiveConsequences() {
        // Unlock combat-focused rewards
        RewardManager.Instance.UnlockCombatRewards();
        
        // Favor dialogue options that value directness
        DialogueManager.Instance.SetAggressionPreference(true);
    }
    
    void ApplyBalancedConsequences() {
        // Provide balanced rewards
        RewardManager.Instance.UnlockBalancedRewards();
        
        // Maintain neutral dialogue options
        DialogueManager.Instance.SetNeutralPreference(true);
    }
}
```

**Mission Impact System**:
```javascript
// Combat choice impact on mission progression
MissionImpact = {
  trackCombatChoice(choiceType, missionPhase) {
    const impact = this.calculateImpact(choiceType, missionPhase);
    this.applyMissionChanges(impact);
  },
  
  calculateImpact(choiceType, missionPhase) {
    const impacts = {
      stealth: {
        early_phase: {
          enemyReduction: 0.3,
          timeBonus: 1.2,
          resourceBonus: 1.5,
          narrativeBranch: "stealth_path"
        },
        mid_phase: {
          enemyReduction: 0.2,
          timeBonus: 1.1,
          resourceBonus: 1.3,
          narrativeBranch: "tactical_path"
        },
        late_phase: {
          enemyReduction: 0.1,
          timeBonus: 1.0,
          resourceBonus: 1.1,
          narrativeBranch: "balanced_path"
        }
      },
      aggressive: {
        early_phase: {
          enemyIncrease: 0.5,
          timePenalty: 0.8,
          resourcePenalty: 0.7,
          narrativeBranch: "combat_path"
        },
        mid_phase: {
          enemyIncrease: 0.3,
          timePenalty: 0.9,
          resourcePenalty: 0.85,
          narrativeBranch: "action_path"
        },
        late_phase: {
          enemyIncrease: 0.2,
          timePenalty: 0.95,
          resourcePenalty: 0.9,
          narrativeBranch: "confrontation_path"
        }
      }
    };
    
    return impacts[choiceType][missionPhase] || {};
  },
  
  applyMissionChanges(impact) {
    // Modify enemy placement
    if (impact.enemyReduction) {
      this.reduceEnemyCount(impact.enemyReduction);
    }
    if (impact.enemyIncrease) {
      this.increaseEnemyCount(impact.enemyIncrease);
    }
    
    // Adjust mission timers
    if (impact.timeBonus) {
      this.extendTimeLimit(impact.timeBonus);
    }
    if (impact.timePenalty) {
      this.reduceTimeLimit(impact.timePenalty);
    }
    
    // Modify resource availability
    if (impact.resourceBonus) {
      this.increaseResources(impact.resourceBonus);
    }
    if (impact.resourcePenalty) {
      this.decreaseResources(impact.resourcePenalty);
    }
    
    // Set narrative branch
    if (impact.narrativeBranch) {
      NarrativeManager.Instance.SetBranch(impact.narrativeBranch);
    }
  }
}
```

## Technical Requirements

### Animation System Specifications

**Character Rig Requirements**:
```csharp
// Animation skeleton specification
CharacterSkeleton = {
  bones: {
    root: "Root",
    hips: "Hips",
    spine: ["Spine", "Spine1", "Spine2"],
    head: "Head",
    
    // Arms
    leftArm: ["LeftShoulder", "LeftArm", "LeftForeArm", "LeftHand"],
    rightArm: ["RightShoulder", "RightArm", "RightForeArm", "RightHand"],
    
    // Legs
    leftLeg: ["LeftUpLeg", "LeftLeg", "LeftFoot", "LeftToeBase"],
    rightLeg: ["RightUpLeg", "RightLeg", "RightFoot", "RightToeBase"],
    
    // Fingers (for detailed interactions)
    leftFingers: ["LeftHandThumb1", "LeftHandThumb2", "LeftHandThumb3",
                  "LeftHandIndex1", "LeftHandIndex2", "LeftHandIndex3",
                  "LeftHandMiddle1", "LeftHandMiddle2", "LeftHandMiddle3",
                  "LeftHandRing1", "LeftHandRing2", "LeftHandRing3",
                  "LeftHandPinky1", "LeftHandPinky2", "LeftHandPinky3"],
    rightFingers: ["RightHandThumb1", "RightHandThumb2", "RightHandThumb3",
                   "RightHandIndex1", "RightHandIndex2", "RightHandIndex3",
                   "RightHandMiddle1", "RightHandMiddle2", "RightHandMiddle3",
                   "RightHandRing1", "RightHandRing2", "RightHandRing3",
                   "RightHandPinky1", "RightHandPinky2", "RightHandPinky3"]
  },
  
  animationRequirements: {
    humanAvatar: true,
    ikSupport: true,
    rootMotion: false,
    optimizeGameObjects: true
  },
  
  mandatoryAnimations: [
    // Movement
    "idle", "walk_forward", "walk_backward", "walk_left", "walk_right",
    "run_forward", "run_backward", "sprint_forward",
    "jump_start", "jump_airborne", "jump_land",
    "turn_left", "turn_right",
    
    // Combat
    "punch_left", "punch_right", "kick_left", "kick_right",
    "dodge_left", "dodge_right", "dodge_back",
    "block_start", "block_hold", "block_end",
    "hit_light", "hit_medium", "hit_heavy",
    "stagger_left", "stagger_right", "stagger_backward",
    "fall_forward", "fall_backward", "get_up_front", "get_up_back",
    
    // Parkour
    "vault_low", "vault_medium", "vault_high",
    "climb_up", "climb_over", "ledge_grab", "ledge_pull_up",
    "wall_jump", "slide_start", "slide_loop", "slide_end",
    
    // Stances
    "crouch_idle", "crouch_walk_forward", "crouch_walk_backward",
    "prone_idle", "prone_crawl_forward", "prone_crawl_backward",
    
    // Death
    "death_standing", "death_kneeling", "death_falling_forward", "death_falling_backward"
  ]
}
```

**Animation Blending Parameters**:
```csharp
public class AnimationBlendSettings : MonoBehaviour {
    [Header("Blend Times (seconds)")]
    [SerializeField] private float idleToWalk = 0.2f;
    [SerializeField] private float walkToRun = 0.15f;
    [SerializeField] private float runToSprint = 0.1f;
    [SerializeField] private float jumpToAirborne = 0.05f;
    [SerializeField] private float airborneToLand = 0.1f;
    [SerializeField] private float combatTransition = 0.3f;
    [SerializeField] private float hitReaction = 0.05f;
    [SerializeField] private float deathTransition = 0.2f;
    
    [Header("Blend Tree Weights")]
    [SerializeField] private AnimationCurve movementBlendCurve;
    [SerializeField] private AnimationCurve directionBlendCurve;
    [SerializeField] private AnimationCurve speedBlendCurve;
    
    [Header("IK Settings")]
    [SerializeField] private float ikWeight = 1.0f;
    [SerializeField] private float footIkWeight = 0.8f;
    [SerializeField] private float handIkWeight = 0.6f;
    [SerializeField] private float lookAtWeight = 0.7f;
    
    [Header("Root Motion Settings")]
    [SerializeField] private bool applyRootMotion = false;
    [SerializeField] private float rootMotionWeight = 0.0f;
    
    public float GetBlendTime(AnimationTransition transition) {
        switch (transition) {
            case AnimationTransition.IdleToWalk: return idleToWalk;
            case AnimationTransition.WalkToRun: return walkToRun;
            case AnimationTransition.RunToSprint: return runToSprint;
            case AnimationTransition.JumpToAirborne: return jumpToAirborne;
            case AnimationTransition.AirborneToLand: return airborneToLand;
            case AnimationTransition.CombatTransition: return combatTransition;
            case AnimationTransition.HitReaction: return hitReaction;
            case AnimationTransition.DeathTransition: return deathTransition;
            default: return 0.1f;
        }
    }
}

public enum AnimationTransition {
    IdleToWalk,
    WalkToRun,
    RunToSprint,
    JumpToAirborne,
    AirborneToLand,
    CombatTransition,
    HitReaction,
    DeathTransition
}
```

### Physics Engine Requirements

**Physics Configuration**:
```csharp
public class PhysicsSettings : MonoBehaviour {
    [Header("Global Physics")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float defaultMaterialFriction = 0.6f;
    [SerializeField] private float defaultMaterialBounciness = 0.0f;
    [SerializeField] private int solverIterations = 6;
    [SerializeField] private int solverVelocityIterations = 1;
    
    [Header("Character Physics")]
    [SerializeField] private float characterRadius = 0.4f;
    [SerializeField] private float characterHeight = 1.8f;
    [SerializeField] private float characterStepOffset = 0.3f;
    [SerializeField] private float characterSlopeLimit = 45f;
    [SerializeField] private LayerMask characterLayerMask;
    
    [Header("Combat Physics")]
    [SerializeField] private float hitDetectionRadius = 0.5f;
    [SerializeField] private float knockbackForceMultiplier = 100f;
    [SerializeField] private float ragdollMass = 80f;
    [SerializeField] private float ragdollDrag = 0.5f;
    [SerializeField] private float ragdollAngularDrag = 0.05f;
    
    [Header("Parkour Physics")]
    [SerializeField] private float vaultDetectionDistance = 2.0f;
    [SerializeField] private float climbDetectionHeight = 3.0f;
    [SerializeField] private float ledgeGrabDistance = 0.8f;
    [SerializeField] private float wallJumpForce = 10f;
    [SerializeField] private float wallJumpHorizontalMultiplier = 1.5f;
    
    void Start() {
        ApplyPhysicsSettings();
    }
    
    void ApplyPhysicsSettings() {
        Physics.gravity = Vector3.up * gravity;
        Physics.defaultSolverIterations = solverIterations;
        Physics.defaultSolverVelocityIterations = solverVelocityIterations;
        
        // Create physics materials
        CreatePhysicsMaterials();
    }
    
    void CreatePhysicsMaterials() {
        // Character material
        PhysicMaterial characterMaterial = new PhysicMaterial("CharacterMaterial");
        characterMaterial.frictionCombine = PhysicMaterialCombine.Multiply;
        characterMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
        characterMaterial.staticFriction = defaultMaterialFriction;
        characterMaterial.dynamicFriction = defaultMaterialFriction;
        characterMaterial.bounciness = defaultMaterialBounciness;
        
        // Apply to character controller
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) {
            // Note: CharacterController doesn't use PhysicMaterial directly
            // These settings are for when the character becomes a ragdoll
        }
    }
}
```

**Collision Matrix**:
| Layer | Player | NPC | Environment | Props | Attack | Trigger |
|-------|--------|-----|-------------|-------|--------|---------|
| Player | ✖ | ✔ | ✔ | ✔ | ✖ | ✔ |
| NPC | ✔ | ✖ | ✔ | ✔ | ✔ | ✔ |
| Environment | ✔ | ✔ | ✖ | ✖ | ✖ | ✖ |
| Props | ✔ | ✔ | ✖ | ✔ | ✖ | ✔ |
| Attack | ✖ | ✔ | ✖ | ✖ | ✖ | ✖ |
| Trigger | ✔ | ✔ | ✖ | ✔ | ✖ | ✖ |

**Performance Optimization**:
```csharp
public class PhysicsOptimization : MonoBehaviour {
    [Header("LOD Settings")]
    public float highDetailDistance = 10f;
    public float mediumDetailDistance = 25f;
    public float lowDetailDistance = 50f;
    
    [Header("Update Rates")]
    public int highPhysicsFPS = 60;
    public int mediumPhysicsFPS = 30;
    public int lowPhysicsFPS = 15;
    
    private Dictionary<GameObject, int> originalUpdateRates;
    private Camera playerCamera;
    
    void Start() {
        playerCamera = Camera.main;
        originalUpdateRates = new Dictionary<GameObject, int>();
        
        InvokeRepeating(nameof(UpdatePhysicsLOD), 0.5f, 0.5f);
    }
    
    void UpdatePhysicsLOD() {
        NPCController[] npcs = FindObjectsOfType<NPCController>();
        
        foreach (NPCController npc in npcs) {
            float distance = Vector3.Distance(playerCamera.transform.position, npc.transform.position);
            int targetUpdateRate = CalculateUpdateRate(distance);
            
            SetPhysicsUpdateRate(npc.gameObject, targetUpdateRate);
        }
    }
    
    int CalculateUpdateRate(float distance) {
        if (distance < highDetailDistance) return highPhysicsFPS;
        if (distance < mediumDetailDistance) return mediumPhysicsFPS;
        if (distance < lowDetailDistance) return lowPhysicsFPS;
        return 10; // Very low update rate for distant objects
    }
    
    void SetPhysicsUpdateRate(GameObject obj, int updateRate) {
        Rigidbody[] rigidbodies = obj.GetComponentsInChildren<Rigidbody>();
        
        foreach (Rigidbody rb in rigidbodies) {
            if (!originalUpdateRates.ContainsKey(rb.gameObject)) {
                originalUpdateRates[rb.gameObject] = rb.solverIterations;
            }
            
            // Adjust solver iterations based on update rate
            float iterationRatio = (float)updateRate / 60f;
            rb.solverIterations = Mathf.RoundToInt(originalUpdateRates[rb.gameObject] * iterationRatio);
        }
    }
}
```

### Engine Integration Requirements

**Unity Component Architecture**:
```csharp
// Required component structure for combat characters
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class CombatCharacter : MonoBehaviour {
    [Header("Core Components")]
    protected CharacterController characterController;
    protected Animator animator;
    protected HealthSystem healthSystem;
    protected StaminaSystem staminaSystem;
    protected CombatSystem combatSystem;
    protected MovementController movementController;
    
    [Header("Audio")]
    protected AudioSource audioSource;
    protected FMODStudioEventEmitter footstepEmitter;
    protected FMODStudioEventEmitter combatEmitter;
    
    [Header("Physics")]
    protected Rigidbody[] ragdollBodies;
    protected Collider[] ragdollColliders;
    
    [Header("Detection")]
    protected LayerMask groundLayer;
    protected LayerMask attackLayer;
    protected LayerMask interactionLayer;
    
    protected virtual void Awake() {
        InitializeComponents();
    }
    
    protected virtual void InitializeComponents() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();
        staminaSystem = GetComponent<StaminaSystem>();
        combatSystem = GetComponent<CombatSystem>();
        movementController = GetComponent<MovementController>();
        
        audioSource = GetComponent<AudioSource>();
        footstepEmitter = GetComponent<FMODStudioEventEmitter>();
        combatEmitter = GetComponent<FMODStudioEventEmitter>();
        
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        
        // Initialize layers
        groundLayer = LayerMask.GetMask("Ground", "Environment");
        attackLayer = LayerMask.GetMask("Attack", "NPC");
        interactionLayer = LayerMask.GetMask("Interaction", "Props");
    }
}
```

**Performance Targets**:
| Metric | Target | Minimum | Acceptable |
|--------|--------|---------|------------|
| Frame Rate | 60 FPS | 45 FPS | 30 FPS |
| Animation Update Rate | 60 Hz | 45 Hz | 30 Hz |
| Physics Update Rate | 60 Hz | 50 Hz | 30 Hz |
| Audio Latency | < 20ms | < 40ms | < 100ms |
| Input Response | < 16ms | < 33ms | < 50ms |
| Memory Usage | 500MB | 750MB | 1GB |

**Platform-Specific Optimizations**:
```csharp
public class PlatformOptimization : MonoBehaviour {
    #if UNITY_STANDALONE
        // PC optimizations
        public bool enableHighQualityPhysics = true;
        public bool enableAdvancedRagdolls = true;
        public int maxConcurrentAnimations = 20;
    #elif UNITY_PS5 || UNITY_XBOX_SERIES_X
        // Console optimizations
        public bool enableHighQualityPhysics = true;
        public bool enableAdvancedRagdolls = true;
        public int maxConcurrentAnimations = 15;
    #elif UNITY_ANDROID || UNITY_IOS
        // Mobile optimizations
        public bool enableHighQualityPhysics = false;
        public bool enableAdvancedRagdolls = false;
        public int maxConcurrentAnimations = 8;
    #endif
    
    void Start() {
        ApplyPlatformSettings();
    }
    
    void ApplyPlatformSettings() {
        // Adjust physics settings
        Physics.defaultSolverIterations = enableHighQualityPhysics ? 6 : 4;
        Physics.defaultSolverVelocityIterations = enableHighQualityPhysics ? 1 : 1;
        
        // Configure ragdoll systems
        NPCRagdoll[] ragdolls = FindObjectsOfType<NPCRagdoll>();
        foreach (NPCRagdoll ragdoll in ragdolls) {
            ragdoll.enabled = enableAdvancedRagdolls;
        }
        
        // Limit concurrent animations
        AnimationOptimizer animationOptimizer = FindObjectOfType<AnimationOptimizer>();
        if (animationOptimizer != null) {
            animationOptimizer.maxConcurrentAnimations = maxConcurrentAnimations;
        }
    }
}
```

## Prototype Deliverables

### Core Systems Implementation

**Minimum Viable Combat System**:
```csharp
// Prototype combat system requirements
public class PrototypeCombatDeliverable : MonoBehaviour {
    [Header("Required Features")]
    public bool[] featureStatus = new bool[10];
    
    // Feature checklist
    enum Feature {
        BasicAttacks,           // Light/heavy punch and kick
        HitDetection,          // Working hit boxes
        NPCReactions,          // Stagger and knockback
        MovementIntegration,   // Combat during movement
        AudioFeedback,         // Impact sounds
        VisualFeedback,        // Screen shake and effects
        StaminaSystem,         // Attack stamina cost
        ComboSystem,           // Basic 2-3 hit combos
        DodgeMechanics,        // Basic dodge roll
        DeathAnimations        // NPC death states
    }
    
    [Header("Acceptance Criteria")]
    public int requiredWorkingFeatures = 8;
    public float targetFrameRate = 45f;
    public float maxInputLatency = 50f; // milliseconds
    
    bool ValidatePrototype() {
        int workingFeatures = 0;
        
        foreach (bool status in featureStatus) {
            if (status) workingFeatures++;
        }
        
        return workingFeatures >= requiredWorkingFeatures &&
               Application.targetFrameRate >= targetFrameRate &&
               InputLatencyTester.averageLatency <= maxInputLatency;
    }
}
```

**Movement System Prototype**:
```csharp
public class PrototypeMovementDeliverable : MonoBehaviour {
    [Header("Movement Features")]
    public bool basicMovementImplemented = false;
    public bool sprintSystemWorking = false;
    public bool staminaDepletionFunctional = false;
    public bool basicParkourWorking = false;
    public bool slopeHandlingImplemented = false;
    
    [Header("Performance Requirements")]
    public float smoothTransitionTime = 0.2f;
    public float animationBlendError = 0.1f;
    public int requiredAnimationStates = 12;
    
    [Header("Test Scenarios")]
    public bool basicMovementTest = false;
    public bool chaseSequenceTest = false;
    public bool escapeRouteTest = false;
    public bool combatMovementTest = false;
    
    bool ValidateMovementPrototype() {
        int passedTests = 0;
        
        if (basicMovementTest) passedTests++;
        if (chaseSequenceTest) passedTests++;
        if (escapeRouteTest) passedTests++;
        if (combatMovementTest) passedTests++;
        
        return passedTests >= 3 && // At least 3/4 test scenarios
               basicMovementImplemented &&
               sprintSystemWorking &&
               staminaDepletionFunctional;
    }
}
```

### Test Scenarios and Acceptance Criteria

**Combat Test Scenarios**:
```csharp
public class CombatTestScenarios : MonoBehaviour {
    [Header("Test Scenario 1: Basic Combat")]
    public bool basicCombatComplete = false;
    public string basicCombatRequirements = "Player can successfully land light and heavy attacks on stationary NPC with proper hit detection and feedback";
    
    [Header("Test Scenario 2: Dynamic Combat")]
    public bool dynamicCombatComplete = false;
    public string dynamicCombatRequirements = "Player can engage moving NPC with attacks, dodges, and combos while maintaining responsive controls";
    
    [Header("Test Scenario 3: Chase Sequence")]
    public bool chaseSequenceComplete = false;
    public string chaseSequenceRequirements = "Player can initiate and survive chase sequence with working sprint, stamina, and escape mechanics";
    
    [Header("Test Scenario 4: NPC Reactions")]
    public bool npcReactionsComplete = false;
    public string npcReactionsRequirements = "NPCs properly react to hits with stagger, knockback, and death animations with ragdoll physics";
    
    [Header("Test Scenario 5: Integration Test")]
    public bool integrationTestComplete = false;
    public string integrationTestRequirements = "Combat system integrates with mission progression, AI narrator, and difficulty scaling";
    
    // Automated test validation
    public bool RunAllTests() {
        bool allTestsPass = true;
        
        allTestsPass &= TestBasicCombat();
        allTestsPass &= TestDynamicCombat();
        allTestsPass &= TestChaseSequence();
        allTestsPass &= TestNPCReactions();
        allTestsPass &= TestIntegration();
        
        return allTestsPass;
    }
    
    bool TestBasicCombat() {
        // Automated test for basic combat functionality
        TestPlayer player = FindObjectOfType<TestPlayer>();
        TestNPC npc = FindObjectOfType<TestNPC>();
        
        // Test light attack
        player.PerformLightAttack();
        if (!npc.wasHit) return false;
        
        // Test heavy attack
        player.PerformHeavyAttack();
        if (!npc.wasKnockedBack) return false;
        
        // Test hit feedback
        if (!ScreenShake.isActive) return false;
        if (!AudioManager.playedImpactSound) return false;
        
        basicCombatComplete = true;
        return true;
    }
    
    bool TestDynamicCombat() {
        // Test combat during movement
        TestPlayer player = FindObjectOfType<TestPlayer>();
        TestNPC npc = FindObjectOfType<TestNPC>();
        
        // Start movement
        player.StartMoving();
        npc.StartMoving();
        
        // Test attack during movement
        player.PerformLightAttack();
        if (!npc.wasHit) return false;
        
        // Test dodge
        player.PerformDodge();
        if (!player.isDodging) return false;
        
        // Test combo
        player.PerformCombo();
        if (npc.hitsTaken < 2) return false;
        
        dynamicCombatComplete = true;
        return true;
    }
    
    bool TestChaseSequence() {
        // Test chase mechanics
        TestPlayer player = FindObjectOfType<TestPlayer>();
        TestNPC pursuer = FindObjectOfType<TestNPC>();
        
        // Initiate chase
        pursuer.StartChase(player);
        
        // Test sprint
        player.StartSprint();
        if (player.currentSpeed <= player.normalSpeed) return false;
        
        // Test stamina depletion
        if (player.stamina >= player.maxStamina) return false;
        
        // Test breathing audio
        if (!AudioManager.isPlayingBreathing) return false;
        
        // Test escape
        player.EscapeToSafePoint();
        if (!pursuer.isChasing) return false;
        
        chaseSequenceComplete = true;
        return true;
    }
    
    bool TestNPCReactions() {
        // Test NPC reaction system
        TestNPC npc = FindObjectOfType<TestNPC>();
        
        // Test light hit reaction
        npc.ApplyLightHit();
        if (!npc.isPlayingStagger) return false;
        
        // Test heavy hit reaction
        npc.ApplyHeavyHit();
        if (!npc.isKnockedBack) return false;
        
        // Test death
        npc.ApplyFatalHit();
        if (!npc.isDead) return false;
        
        // Test ragdoll
        if (!npc.ragdollActive) return false;
        
        npcReactionsComplete = true;
        return true;
    }
    
    bool TestIntegration() {
        // Test system integration
        MissionManager missionManager = FindObjectOfType<MissionManager>();
        AINarrator narrator = FindObjectOfType<AINarrator>();
        DynamicDifficulty difficulty = FindObjectOfType<DynamicDifficulty>();
        
        // Test mission progression
        if (!missionManager.combatObjectiveComplete) return false;
        
        // Test narrator integration
        if (!narrator.combatEventTriggered) return false;
        
        // Test difficulty scaling
        if (!difficulty.adjustmentApplied) return false;
        
        integrationTestComplete = true;
        return true;
    }
}
```

**Performance Benchmarks**:
| System | Target Performance | Minimum Acceptable | Test Method |
|--------|-------------------|-------------------|-------------|
| Combat Input Response | < 16ms | < 33ms | Input latency measurement |
| Animation Frame Rate | 60 FPS | 45 FPS | Frame time analysis |
| Physics Collision Detection | < 5ms | < 10ms | Physics profiler |
| Audio Impact Latency | < 20ms | < 40ms | Audio delay measurement |
| NPC AI Response Time | < 100ms | < 200ms | AI behavior timing |
| Memory Usage (Combat) | 100MB | 150MB | Memory profiler |
| CPU Usage (Combat) | 15% | 25% | CPU profiler |

**Quality Assurance Checklist**:
```csharp
public class QualityAssurance : MonoBehaviour {
    [Header("Visual Quality")]
    public bool[] visualChecks = new bool[8];
    
    [Header("Audio Quality")]
    public bool[] audioChecks = new bool[6];
    
    [Header("Gameplay Quality")]
    public bool[] gameplayChecks = new bool[10];
    
    [Header("Performance Quality")]
    public bool[] performanceChecks = new bool[7];
    
    // Visual quality checks
    enum VisualCheck {
        SmoothAnimationBlending,
        ProperHitReactions,
        ScreenShakeEffects,
        BloodParticleEffects,
        CharacterModelQuality,
        LightingConsistency,
        ShadowQuality,
        EffectTiming
    }
    
    // Audio quality checks
    enum AudioCheck {
        ImpactSoundVariety,
        FootstepSurfaceVariation,
        BreathingAudioIntensity,
        VoiceLineClarity,
        SpatialAudioPositioning,
        AudioLevelBalance
    }
    
    // Gameplay quality checks
    enum GameplayCheck {
        ResponsiveControls,
        FairHitDetection,
        BalancedDifficulty,
        ClearVisualFeedback,
        IntuitiveMovement,
        ProperStaminaBalance,
        MeaningfulChoices,
        SmoothTransitions,
        ConsistentRules,
        PlayerAgency
    }
    
    // Performance quality checks
    enum PerformanceCheck {
        StableFrameRate,
        LowInputLatency,
        EfficientMemoryUsage,
        QuickLoadTimes,
        SmoothPhysics,
        OptimizedAnimations,
        ScalableQuality
    }
    
    public bool RunQualityAssurance() {
        bool visualQuality = CheckVisualQuality();
        bool audioQuality = CheckAudioQuality();
        bool gameplayQuality = CheckGameplayQuality();
        bool performanceQuality = CheckPerformanceQuality();
        
        return visualQuality && audioQuality && gameplayQuality && performanceQuality;
    }
    
    bool CheckVisualQuality() {
        int passedChecks = 0;
        
        foreach (bool check in visualChecks) {
            if (check) passedChecks++;
        }
        
        return passedChecks >= visualChecks.Length * 0.8; // 80% pass rate
    }
    
    bool CheckAudioQuality() {
        int passedChecks = 0;
        
        foreach (bool check in audioChecks) {
            if (check) passedChecks++;
        }
        
        return passedChecks >= audioChecks.Length * 0.8; // 80% pass rate
    }
    
    bool CheckGameplayQuality() {
        int passedChecks = 0;
        
        foreach (bool check in gameplayChecks) {
            if (check) passedChecks++;
        }
        
        return passedChecks >= gameplayChecks.Length * 0.9; // 90% pass rate
    }
    
    bool CheckPerformanceQuality() {
        int passedChecks = 0;
        
        foreach (bool check in performanceChecks) {
            if (check) passedChecks++;
        }
        
        return passedChecks >= performanceChecks.Length * 0.8; // 80% pass rate
    }
}
```

## QA and Testing Checklist

### Manual Testing Procedures

**Combat System Testing**:
1. **Basic Attack Validation**
   - [ ] Light punch registers hit on target
   - [ ] Heavy punch deals increased damage
   - [ ] Kick attacks have proper reach
   - [ ] Attack animations play correctly
   - [ ] Hit boxes align with visual attacks

2. **Combo System Testing**
   - [ ] 2-hit combos execute properly
   - [ ] 3-hit combos complete successfully
   - [ ] Combo timing windows feel responsive
   - [ ] Failed combos reset correctly
   - [ ] Combo damage multipliers apply

3. **Defense Mechanics Testing**
   - [ ] Dodge roll avoids incoming attacks
   - [ ] Block reduces incoming damage
   - [ ] Parry timing windows work correctly
   - [ ] Perfect parry triggers counter-attack
   - [ ] Defense stamina costs feel balanced

4. **NPC Reaction Testing**
   - [ ] Light hits trigger stagger animations
   - [ ] Heavy hits cause knockback
   - [ ] Fatal hits trigger death animations
   - [ ] Ragdoll physics activate appropriately
   - [ ] Recovery animations play correctly

**Movement System Testing**:
1. **Basic Movement Validation**
   - [ ] Walking speed feels appropriate
   - [ ] Running transitions smoothly
   - [ ] Sprint provides noticeable speed boost
   - [ ] Direction changes respond instantly
   - [ ] Turning animations blend naturally

2. **Parkour Mechanics Testing**
   - [ ] Vault animations trigger at appropriate heights
   - [ ] Ledge grabbing works reliably
   - [ ] Wall jumps provide proper momentum
   - [ ] Sliding mechanics function correctly
   - [ ] Parkour stamina costs feel balanced

3. **Environmental Interaction**
   - [ ] Slope movement works naturally
   - [ ] Stair climbing animates correctly
   - [ ] Obstacle avoidance functions properly
   - [ ] Surface detection works for audio
   - [ ] Collision handling prevents glitches

4. **Stamina System Testing**
   - [ ] Sprint depletes stamina at correct rate
   - [ ] Recovery happens when not sprinting
   - [ ] Exhaustion penalties apply correctly
   - [ ] Audio feedback matches stamina levels
   - [ ] Visual indicators update accurately

### Automated Testing Framework

**Unit Tests for Combat Logic**:
```csharp
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class CombatSystemTests {
    private CombatSystem combatSystem;
    private TestPlayer testPlayer;
    private TestNPC testNPC;
    
    [SetUp]
    public void Setup() {
        // Initialize test objects
        testPlayer = new TestPlayer();
        testNPC = new TestNPC();
        combatSystem = new CombatSystem();
        
        combatSystem.Initialize(testPlayer);
    }
    
    [Test]
    public void LightAttack_DealsCorrectDamage() {
        // Arrange
        float expectedDamage = 15f;
        float initialHealth = testNPC.health;
        
        // Act
        combatSystem.PerformLightAttack(testNPC);
        
        // Assert
        Assert.AreEqual(initialHealth - expectedDamage, testNPC.health);
        Assert.IsTrue(testNPC.wasHit);
    }
    
    [Test]
    public void HeavyAttack_DealsIncreasedDamage() {
        // Arrange
        float expectedDamage = 25f;
        float initialHealth = testNPC.health;
        
        // Act
        combatSystem.PerformHeavyAttack(testNPC);
        
        // Assert
        Assert.AreEqual(initialHealth - expectedDamage, testNPC.health);
        Assert.IsTrue(testNPC.wasKnockedBack);
    }
    
    [Test]
    public void ComboSystem_ExecutesCorrectSequence() {
        // Arrange
        combatSystem.StartCombo();
        
        // Act
        combatSystem.AddAttack(AttackType.LightPunch);
        combatSystem.AddAttack(AttackType.LightPunch);
        combatSystem.AddAttack(AttackType.HeavyKick);
        
        // Assert
        Assert.IsTrue(combatSystem.IsComboComplete());
        Assert.AreEqual(1.5f, combatSystem.GetComboDamageMultiplier());
    }
    
    [Test]
    public void StaminaSystem_DepletesCorrectly() {
        // Arrange
        float initialStamina = testPlayer.stamina;
        float expectedStaminaCost = 20f;
        
        // Act
        testPlayer.StartSprinting();
        testPlayer.UpdateStamina(1.0f); // 1 second of sprinting
        
        // Assert
        Assert.AreEqual(initialStamina - expectedStaminaCost, testPlayer.stamina);
    }
    
    [Test]
    public void DodgeMechanics_AvoidsIncomingAttack() {
        // Arrange
        testPlayer.StartDodge();
        Attack incomingAttack = new Attack { damage = 25f };
        
        // Act
        bool hitRegistered = combatSystem.ProcessIncomingAttack(incomingAttack, testPlayer);
        
        // Assert
        Assert.IsFalse(hitRegistered);
        Assert.AreEqual(testPlayer.maxHealth, testPlayer.health);
    }
}

[TestFixture]
public class MovementSystemTests {
    private MovementController movementController;
    private TestPlayer testPlayer;
    
    [SetUp]
    public void Setup() {
        testPlayer = new TestPlayer();
        movementController = new MovementController();
        movementController.Initialize(testPlayer);
    }
    
    [Test]
    public void SprintSpeed_IncreasesCorrectly() {
        // Arrange
        float expectedSprintSpeed = testPlayer.baseSpeed * 2.0f;
        
        // Act
        testPlayer.StartSprinting();
        float currentSpeed = movementController.GetCurrentSpeed();
        
        // Assert
        Assert.AreEqual(expectedSprintSpeed, currentSpeed);
    }
    
    [Test]
    public void ParkourDetection_FindsVaultableObstacle() {
        // Arrange
        TestObstacle vaultableObstacle = new TestObstacle { height = 1.2f, isVaultable = true };
        
        // Act
        bool canVault = movementController.CheckVaultOpportunity(vaultableObstacle);
        
        // Assert
        Assert.IsTrue(canVault);
    }
    
    [Test]
    public void LedgeGrab_TriggersAtCorrectHeight() {
        // Arrange
        Vector3 ledgePosition = new Vector3(0, 2.0f, 3.0f);
        testPlayer.transform.position = new Vector3(0, 1.8f, 0);
        
        // Act
        bool canGrabLedge = movementController.CheckLedgeGrab(ledgePosition);
        
        // Assert
        Assert.IsTrue(canGrabLedge);
    }
    
    [Test]
    public void StaminaExhaustion_AppliesMovementPenalty() {
        // Arrange
        testPlayer.stamina = 10f; // Low stamina
        float expectedPenaltySpeed = testPlayer.baseSpeed * 0.5f;
        
        // Act
        testPlayer.StartSprinting(); // Should trigger exhaustion
        float currentSpeed = movementController.GetCurrentSpeed();
        
        // Assert
        Assert.AreEqual(expectedPenaltySpeed, currentSpeed);
        Assert.IsTrue(testPlayer.isExhausted);
    }
}
```

**Integration Tests**:
```csharp
[TestFixture]
public class IntegrationTests {
    private GameManager gameManager;
    private MissionManager missionManager;
    private AINarrator aiNarrator;
    private DynamicDifficulty dynamicDifficulty;
    
    [SetUp]
    public void Setup() {
        gameManager = new GameManager();
        missionManager = new MissionManager();
        aiNarrator = new AINarrator();
        dynamicDifficulty = new DynamicDifficulty();
        
        gameManager.Initialize();
    }
    
    [Test]
    public void CombatMission_UpdatesProgressCorrectly() {
        // Arrange
        Mission combatMission = new Mission { type = MissionType.Combat, requiredKills = 3 };
        missionManager.SetCurrentMission(combatMission);
        
        // Act
        gameManager.RegisterEnemyElimination();
        gameManager.RegisterEnemyElimination();
        gameManager.RegisterEnemyElimination();
        
        // Assert
        Assert.IsTrue(combatMission.IsComplete());
        Assert.IsTrue(missionManager.IsReadyForNextMission());
    }
    
    [Test]
    public void AINarrator_RespondsToCombatEvents() {
        // Arrange
        bool narratorTriggered = false;
        aiNarrator.OnEventTriggered += (eventType, message) => {
            if (eventType == "combat_started") narratorTriggered = true;
        };
        
        // Act
        gameManager.StartCombatEncounter();
        
        // Assert
        Assert.IsTrue(narratorTriggered);
    }
    
    [Test]
    public void DifficultyScaling_AdjustsToPlayerPerformance() {
        // Arrange
        gameManager.RegisterCombatSuccess(); // Player doing well
        gameManager.RegisterCombatSuccess();
        gameManager.RegisterCombatSuccess();
        
        float initialDifficulty = dynamicDifficulty.currentMultiplier;
        
        // Act
        dynamicDifficulty.AdjustDifficulty();
        
        // Assert
        Assert.IsTrue(dynamicDifficulty.currentMultiplier > initialDifficulty);
    }
    
    [Test]
    public void StealthChoice_UnlocksAlternativePaths() {
        // Arrange
        gameManager.RegisterStealthElimination();
        Mission currentMission = missionManager.GetCurrentMission();
        
        // Act
        gameManager.ProcessConsequences();
        
        // Assert
        Assert.IsTrue(currentMission.stealthPathsUnlocked);
        Assert.IsFalse(currentMission.combatPathsLocked);
    }
}
```

### Performance Testing

**Stress Testing Framework**:
```csharp
public class PerformanceStressTests : MonoBehaviour {
    [Header("Test Parameters")]
    public int maxConcurrentNPCs = 20;
    public int maxConcurrentAttacks = 10;
    public float testDuration = 60f;
    
    private List<TestNPC> testNPCs;
    private PerformanceMonitor performanceMonitor;
    
    [UnityTest]
    public IEnumerator StressTest_CombatWithManyNPCs() {
        // Arrange
        performanceMonitor = new PerformanceMonitor();
        testNPCs = new List<TestNPC>();
        
        // Spawn maximum NPCs
        for (int i = 0; i < maxConcurrentNPCs; i++) {
            TestNPC npc = SpawnTestNPC();
            testNPCs.Add(npc);
        }
        
        float startTime = Time.time;
        
        // Act - Run combat scenario for test duration
        while (Time.time - startTime < testDuration) {
            // Simulate combat
            foreach (TestNPC npc in testNPCs) {
                if (Random.value < 0.1f) {
                    npc.PerformRandomAttack();
                }
            }
            
            // Monitor performance
            performanceMonitor.Update();
            
            // Assert performance doesn't drop below minimum
            Assert.IsTrue(performanceMonitor.frameRate >= 30f, $"Frame rate dropped to {performanceMonitor.frameRate}");
            Assert.IsTrue(performanceMonitor.memoryUsage <= 1024f, $"Memory usage exceeded: {performanceMonitor.memoryUsage}MB");
            
            yield return null;
        }
        
        // Final assertions
        Assert.IsTrue(performanceMonitor.averageFrameRate >= 45f, "Average frame rate below target");
        Assert.IsTrue(performanceMonitor.peakMemoryUsage <= 1500f, "Peak memory usage exceeded");
    }
    
    [UnityTest]
    public IEnumerator StressTest_ParkourWithComplexEnvironment() {
        // Arrange
        TestEnvironment complexEnvironment = CreateComplexEnvironment();
        TestPlayer player = FindObjectOfType<TestPlayer>();
        
        float startTime = Time.time;
        
        // Act - Continuous parkour movements
        while (Time.time - startTime < testDuration) {
            // Simulate various parkour actions
            if (Random.value < 0.3f) player.PerformVault();
            if (Random.value < 0.2f) player.PerformLedgeGrab();
            if (Random.value < 0.15f) player.PerformWallJump();
            
            performanceMonitor.Update();
            
            // Assert animation system keeps up
            Assert.IsTrue(performanceMonitor.animationUpdateTime <= 16.67f, "Animation update too slow");
            
            yield return null;
        }
    }
    
    [UnityTest]
    public IEnumerator StressTest_ChaseSequenceWithMultiplePursuers() {
        // Arrange
        TestPlayer player = FindObjectOfType<TestPlayer>();
        List<TestNPC> pursuers = new List<TestNPC>();
        
        // Create multiple pursuers
        for (int i = 0; i < 5; i++) {
            TestNPC pursuer = CreateHostileNPC();
            pursuer.StartChasing(player);
            pursuers.Add(pursuer);
        }
        
        float startTime = Time.time;
        
        // Act - Extended chase sequence
        while (Time.time - startTime < testDuration) {
            player.UpdateMovement();
            
            foreach (TestNPC pursuer in pursuers) {
                pursuer.UpdatePursuit();
            }
            
            performanceMonitor.Update();
            
            // Assert pathfinding performance
            Assert.IsTrue(performanceMonitor.pathfindingTime <= 5f, "Pathfinding too slow");
            
            yield return null;
        }
    }
}

public class PerformanceMonitor {
    public float frameRate { get; private set; }
    public float averageFrameRate { get; private set; }
    public float memoryUsage { get; private set; }
    public float peakMemoryUsage { get; private set; }
    public float animationUpdateTime { get; private set; }
    public float pathfindingTime { get; private set; }
    
    private Queue<float> frameRateHistory = new Queue<float>();
    private float frameRateSum;
    
    public void Update() {
        // Measure frame rate
        float currentFrameRate = 1.0f / Time.deltaTime;
        frameRate = currentFrameRate;
        
        frameRateHistory.Enqueue(currentFrameRate);
        frameRateSum += currentFrameRate;
        
        if (frameRateHistory.Count > 60) {
            frameRateSum -= frameRateHistory.Dequeue();
        }
        
        averageFrameRate = frameRateSum / frameRateHistory.Count;
        
        // Measure memory usage
        memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false) / (1024f * 1024f);
        peakMemoryUsage = Mathf.Max(peakMemoryUsage, memoryUsage);
        
        // Measure system performance
        animationUpdateTime = MeasureAnimationUpdateTime();
        pathfindingTime = MeasurePathfindingTime();
    }
    
    private float MeasureAnimationUpdateTime() {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Force animation update
        Animator[] animators = Object.FindObjectsOfType<Animator>();
        foreach (Animator animator in animators) {
            animator.Update(Time.deltaTime);
        }
        
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
    
    private float MeasurePathfindingTime() {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Force pathfinding update
        NavMeshAgent[] agents = Object.FindObjectsOfType<NavMeshAgent>();
        foreach (NavMeshAgent agent in agents) {
            if (agent.hasPath) {
                agent.CalculatePath(agent.destination, agent.path);
            }
        }
        
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}
```

### Regression Testing

**Automated Regression Suite**:
```csharp
[TestFixture]
public class RegressionTests {
    [Test]
    public void CombatAnimation_BlendingNotBroken() {
        // Test that recent changes didn't break animation blending
        TestPlayer player = CreateTestPlayer();
        
        // Test state transitions
        player.SetState(PlayerState.Idle);
        player.SetState(PlayerState.Walking);
        player.SetState(PlayerState.Running);
        player.SetState(PlayerState.Combat);
        
        // Verify smooth transitions
        Assert.IsTrue(player.animationBlendTime < 0.3f);
        Assert.IsTrue(player.currentAnimationState == PlayerState.Combat);
    }
    
    [Test]
    public void HitDetection_RemainsAccurate() {
        // Test that hit detection still works correctly
        CombatSystem combatSystem = new CombatSystem();
        TestPlayer player = CreateTestPlayer();
        TestNPC npc = CreateTestNPC();
        
        // Test hit registration
        bool hitRegistered = combatSystem.CheckHit(player.attackHitBox, npc.hitBox);
        
        Assert.IsTrue(hitRegistered);
        Assert.AreEqual(npc.initialHealth - 15f, npc.health);
    }
    
    [Test]
    public void StaminaSystem_BalancingMaintained() {
        // Test that stamina system balance wasn't broken
        TestPlayer player = CreateTestPlayer();
        
        float initialStamina = player.stamina;
        player.StartSprinting();
        player.UpdateStamina(5.0f); // 5 seconds
        
        float expectedStamina = initialStamina - (5.0f * player.staminaDrainRate);
        Assert.AreEqual(expectedStamina, player.stamina, 0.1f);
    }
    
    [Test]
    public void NPCAI_BehaviorConsistent() {
        // Test that NPC behavior remains consistent
        TestNPC npc = CreateTestNPC();
        
        // Test behavior tree execution
        npc.UpdateBehavior();
        
        Assert.IsTrue(npc.currentState != NPCState.Invalid);
        Assert.IsTrue(npc.hasValidTarget || npc.isPatrolling);
    }
    
    [Test]
    public void AudioIntegration_FunctioningCorrectly() {
        // Test that audio integration still works
        AudioManager audioManager = new AudioManager();
        
        // Test combat audio
        audioManager.PlayCombatSound("punch_light");
        
        Assert.IsTrue(audioManager.IsSoundPlaying("punch_light"));
        Assert.IsTrue(audioManager.lastPlayedVolume >= -24f);
    }
}
```

---

This comprehensive combat and movement systems specification provides the technical foundation for implementing responsive, engaging gameplay mechanics in Protocol EMR. The systems are designed to work together seamlessly while maintaining performance targets and supporting the game's atmospheric narrative experience.