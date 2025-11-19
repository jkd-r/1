# Weapons & Tools System Protocol

This document defines the comprehensive weapons, tools, and equipment systems for Protocol EMR. These systems provide players with tactical options for combat, puzzle-solving, and environmental interaction while maintaining game balance and atmospheric tension.

## Table of Contents

1. [Melee Combat Weapons](#melee-combat-weapons)
2. [Ranged Weapons](#ranged-weapons)
3. [Tools & Utilities](#tools--utilities)
4. [Weapon Pickup & Inventory](#weapon-pickup--inventory)
5. [Combat Integration](#combat-integration)
6. [Balance & Difficulty](#balance--difficulty)
7. [Weapon Durability System](#weapon-durability-system)
8. [Technical Requirements](#technical-requirements)
9. [Integration Points](#integration-points)
10. [Prototype Deliverables](#prototype-deliverables)
11. [QA and Testing Checklist](#qa-and-testing-checklist)

## Melee Combat Weapons

### Overview

The melee weapon system expands upon the existing unarmed combat mechanics, providing players with tactical options that affect damage output, attack speed, and enemy reactions. Weapons are found, not crafted, emphasizing exploration and resource scarcity.

### Unarmed Combat (Base System)

Building on the existing fist/unarmed combat from the combat system:

| Attack Type | Damage | Stamina Cost | Animation Duration | Knockback Force |
|-------------|--------|--------------|-------------------|-----------------|
| **Light Punch** | 15 | 5 | 0.4s | 50N |
| **Heavy Punch** | 25 | 12 | 0.6s | 100N |
| **Light Kick** | 20 | 8 | 0.5s | 75N |
| **Heavy Kick** | 35 | 15 | 0.7s | 150N |
| **Dodge Attack** | 30 | 10 | 0.3s | 125N |

### Improvised Melee Weapons

#### Weapon Categories

**Light Weapons** (Fast, low damage):
- **Wrench**: Maintenance tool repurposed for combat
- **Pipe**: Industrial piping, balanced damage/speed
- **Small Club**: Broken handle or similar improvised weapon

**Heavy Weapons** (Slow, high damage):
- **Crowbar**: Pry bar with significant blunt force
- **Fire Extinguisher**: Heavy, can disorient enemies
- **Large Pipe**: Industrial conduit, substantial reach

**Special Weapons** (Unique properties):
- **Machete**: Cutting damage, higher critical chance
- **Hammer**: High impact, armor piercing capability

#### Weapon Statistics

| Weapon | Damage | Attack Speed | Stamina Cost | Durability | Knockback | Special |
|--------|--------|--------------|--------------|------------|-----------|---------|
| **Wrench** | 20 | 1.1x | 8 | 150 | 75N | Tool interaction |
| **Pipe** | 25 | 1.0x | 10 | 200 | 100N | Balanced |
| **Crowbar** | 35 | 0.8x | 15 | 250 | 150N | Pry doors open |
| **Machete** | 30 | 1.2x | 12 | 100 | 80N | Bleed chance |
| **Hammer** | 40 | 0.7x | 18 | 180 | 200N | Armor pierce |
| **Fire Extinguisher** | 15 | 0.6x | 20 | 50 | 60N | Disorient |

#### Attack Patterns per Weapon

**Wrench**:
- Light: Quick jab (0.3s)
- Heavy: Overhead swing (0.6s)
- Special: Tool strike (can interact with mechanisms)

**Pipe**:
- Light: Horizontal swing (0.4s)
- Heavy: Thrust (0.7s)
- Special: Two-handed strike (increased damage)

**Crowbar**:
- Light: Hook strike (0.5s)
- Heavy: Pry attack (0.8s)
- Special: Door/panel breach

### Weapon Switching Mechanics

#### Switch System

```csharp
// Weapon switching controller
public class WeaponSwitchController : MonoBehaviour {
    [SerializeField] private WeaponSlot[] weaponSlots;
    [SerializeField] private float switchTime = 0.3f;
    [SerializeField] private AnimationCurve switchCurve;
    
    private Weapon currentWeapon;
    private bool isSwitching = false;
    
    public void SwitchToSlot(int slotIndex) {
        if (isSwitching || slotIndex >= weaponSlots.Length) return;
        
        StartCoroutine(SwitchWeaponRoutine(weaponSlots[slotIndex]));
    }
    
    private IEnumerator SwitchWeaponRoutine(WeaponSlot targetSlot) {
        isSwitching = true;
        
        // Sheathe current weapon
        if (currentWeapon != null) {
            yield return StartCoroutine(currentWeapon.Sheath());
        }
        
        // Draw new weapon
        yield return StartCoroutine(targetSlot.Draw());
        currentWeapon = targetSlot.Weapon;
        
        isSwitching = false;
    }
}
```

#### Input Mapping

| Input | Function | Response Time |
|-------|----------|---------------|
| **1** | Switch to melee slot | 0.3s animation |
| **2** | Switch to ranged slot | 0.4s animation |
| **3-5** | Tool quick-slots | 0.2s animation |
| **Mouse Wheel** | Cycle weapons | 0.3s animation |
| **Q/E** | Quick melee switch | 0.2s fast switch |

## Ranged Weapons

### Overview

Ranged weapons provide tactical alternatives to melee combat, emphasizing positioning, ammo management, and stealth. Limited ammo availability creates tension and forces strategic decision-making.

### Weapon Types

#### Taser/Stunner (Non-lethal)

**Purpose**: Crowd control, non-lethal takedowns
**Ammo Type**: Energy cells (rechargeable)
**Effective Range**: 15m
**Fire Rate**: Semi-automatic (0.8s between shots)

| Stat | Value |
|------|-------|
| **Damage** | 5 (stun) |
| **Stun Duration** | 3-5 seconds |
| **Ammo Capacity** | 8 shots |
| **Reload Time** | 2.5s (battery swap) |
| **Accuracy** | High (laser sight) |
| **Recoil** | Minimal |

#### Plasma Rifle (Energy Weapon)

**Purpose**: High-tech combat, armor penetration
**Ammo Type**: Plasma canisters
**Effective Range**: 50m
**Fire Rate**: Burst fire (3-round bursts)

| Stat | Value |
|------|-------|
| **Damage** | 45 per shot |
| **Ammo Capacity** | 30 (10 bursts) |
| **Reload Time** | 3.0s (canister swap) |
| **Accuracy** | Medium (slight spread) |
| **Recoil** | Moderate |
| **Charge Time** | 1.2s (for charged shot) |

#### Grenade Launcher (Area Effect)

**Purpose**: Area denial, crowd control
**Ammo Type**: Grenades (various types)
**Effective Range**: 30m (arc trajectory)
**Fire Rate**: Single shot

| Grenade Type | Effect | Damage | Radius |
|--------------|--------|--------|--------|
| **Fragmentation** | Shrapnel damage | 80 | 5m |
| **EMP** | Electronics disable | 20 | 8m |
| **Smoke** | Vision obstruction | 0 | 10m |
| **Flashbang** | Sensory overload | 10 (stun) | 12m |

### Weapon Accuracy & Recoil

#### Accuracy System

```csharp
// Weapon accuracy controller
public class WeaponAccuracy : MonoBehaviour {
    [SerializeField] private float baseAccuracy = 0.95f;
    [SerializeField] private float movementPenalty = 0.3f;
    [SerializeField] private float recoilRecovery = 0.1f;
    [SerializeField] private float maxSpread = 0.15f;
    
    private float currentAccuracy;
    private Vector3 recoilOffset;
    
    public float CalculateAccuracy(PlayerMovement movement) {
        float accuracy = baseAccuracy;
        
        // Movement penalty
        if (movement.IsMoving) {
            accuracy -= movementPenalty * movement.SpeedRatio;
        }
        
        // Crouch bonus
        if (movement.IsCrouching) {
            accuracy += 0.1f;
        }
        
        // Aim bonus
        if (movement.IsAiming) {
            accuracy += 0.15f;
        }
        
        return Mathf.Clamp(accuracy, 0.3f, 1.0f);
    }
}
```

#### Recoil Patterns

| Weapon | Recoil Type | Recovery Time | Pattern |
|--------|-------------|---------------|---------|
| **Taser** | Minimal | 0.2s | Slight upward |
| **Plasma Rifle** | Moderate | 0.8s | Upward-right arc |
| **Grenade Launcher** | Heavy | 1.5s | Strong upward kick |

### Ammo Management & Scarcity

#### Ammo Distribution

**Common Areas** (Maintenance, storage):
- Taser energy cells: 2-3 per area
- Standard ammo: 10-15 rounds per cache

**Restricted Areas** (Security, labs):
- Plasma canisters: 1-2 per area
- Special ammo: 3-5 rounds per cache

**Hidden Supplies**:
- Emergency stashes: Random placement
- Enemy drops: 10% chance for ammo

#### Ammo Conservation Mechanics

```javascript
// Ammo tracking system
AmmoManager {
  ammoTypes: {
    taser_energy: { current: 8, max: 8, reserves: 24 },
    plasma_canister: { current: 30, max: 30, reserves: 60 },
    grenade_frag: { current: 2, max: 4, reserves: 8 },
    grenade_emp: { current: 1, max: 4, reserves: 4 }
  },
  
  // Scarcity multipliers by difficulty
  difficultyMultipliers: {
    Easy: 1.5,
    Normal: 1.0,
    Hard: 0.7,
    Nightmare: 0.5
  },
  
  consumeAmmo(type, amount) {
    if (this.ammoTypes[type].current >= amount) {
      this.ammoTypes[type].current -= amount;
      return true;
    }
    return false;
  }
}
```

## Tools & Utilities

### Overview

Tools provide non-combat solutions to obstacles, enabling alternative approaches to puzzles and environmental challenges. Each tool serves specific functions and can be used creatively in combat situations.

### Lockpick

#### Functionality
- **Purpose**: Bypass mechanical locks on doors and containers
- **Usage**: Mini-game based on tension and timing
- **Skill Progression**: Improves success chance and speed

#### Lockpick Mini-Game

```csharp
// Lockpicking mini-game controller
public class LockpickMiniGame : MonoBehaviour {
    [SerializeField] private float[] tensionLevels = { 0.2f, 0.4f, 0.6f, 0.8f };
    [SerializeField] private float sweetSpotSize = 0.1f;
    [SerializeField] private int maxAttempts = 3;
    
    private int currentPin = 0;
    private float currentRotation = 0f;
    private bool isLocked = true;
    
    public bool AttemptPick(float rotation) {
        float targetRotation = tensionLevels[currentPin];
        float tolerance = sweetSpotSize * (1f - playerSkillLevel);
        
        if (Mathf.Abs(rotation - targetRotation) < tolerance) {
            SetPin(currentPin, true);
            currentPin++;
            
            if (currentPin >= tensionLevels.Length) {
                Unlock();
                return true;
            }
        } else {
            BreakPick();
        }
        
        return false;
    }
}
```

#### Lock Types

| Lock Type | Difficulty | Time Required | Tool Durability Cost |
|-----------|------------|---------------|----------------------|
| **Basic** | Easy | 10-20s | 5 |
| **Standard** | Medium | 20-40s | 10 |
| **High-Security** | Hard | 40-60s | 20 |
| **Electronic** | Very Hard | 60-90s | 30 |

### Hacking Device

#### Functionality
- **Purpose**: Access electronic systems, terminals, and digital locks
- **Usage**: Command-line interface puzzle
- **Levels**: Basic access, administrative, root access

#### Hacking Interface

```javascript
// Terminal hacking system
HackingTerminal {
  difficulty: enum [Basic, Intermediate, Advanced, Expert],
  timeLimit: 120, // seconds
  maxAttempts: 3,
  
  commands: {
    help: "Display available commands",
    scan: "Scan network for vulnerable nodes",
    bypass: "Attempt to bypass security",
    extract: "Extract data from system",
    access: "Gain access to locked areas"
  },
  
  firewalls: [
    { type: "ICE", strength: 3, weakness: "overflow" },
    { type: "Proxy", strength: 2, weakness: "disconnect" },
    { type: "Sentry", strength: 4, weakness: "stealth" }
  ],
  
  hack(difficulty) {
    // Generate puzzle based on difficulty
    const puzzle = this.generatePuzzle(difficulty);
    return puzzle;
  }
}
```

#### Hackable Systems

| System Type | Access Level | Rewards | Risk |
|-------------|--------------|---------|------|
| **Door Console** | Basic | Unlock door | Trigger alarm |
| **Security Terminal** | Intermediate | Disable cameras | Alert guards |
| **Data Terminal** | Advanced | Obtain intel | Data wipe |
| **Main System** | Expert | Area control | System lockdown |

### Scanner

#### Functionality
- **Purpose**: Detect enemies, analyze environments, identify weaknesses
- **Usage**: Active scanning with cooldown periods
- **Modes**: Threat detection, structural analysis, resource location

#### Scanner Modes

| Mode | Function | Range | Cooldown | Battery Cost |
|------|----------|-------|----------|--------------|
| **Threat Scan** | Detect enemies/hostiles | 30m | 5s | 10% |
| **Structural** | Find weak points/paths | 20m | 8s | 15% |
| **Resource** | Locate items/ammo | 25m | 10s | 12% |
| **Deep Scan** | Full area analysis | 40m | 30s | 25% |

#### Scanner Implementation

```csharp
// Multi-mode scanner system
public class ScannerDevice : MonoBehaviour {
    [SerializeField] private float[] scanRanges = { 30f, 20f, 25f, 40f };
    [SerializeField] private float[] cooldowns = { 5f, 8f, 10f, 30f };
    [SerializeField] private LayerMask[] scanLayers;
    
    public enum ScanMode { Threat, Structural, Resource, Deep }
    
    public void PerformScan(ScanMode mode) {
        if (isCoolingDown) return;
        
        StartCoroutine(ScanRoutine(mode));
    }
    
    private IEnumerator ScanRoutine(ScanMode mode) {
        // Activate scanner effects
        ShowScanVisualization(mode);
        
        // Perform sphere cast for detected objects
        Collider[] hits = Physics.OverlapSphere(
            transform.position, 
            scanRanges[(int)mode], 
            scanLayers[(int)mode]
        );
        
        // Process and display results
        ProcessScanResults(hits, mode);
        
        // Start cooldown
        StartCoroutine(CooldownRoutine(cooldowns[(int)mode]));
    }
}
```

### EMP Device

#### Functionality
- **Purpose**: Disable electronics, stun mechanical enemies, disrupt security
- **Usage**: Area effect with limited range
- **Duration**: Temporary effect based on target type

#### EMP Effects

| Target Type | Effect Duration | Side Effects |
|-------------|-----------------|--------------|
| **Security Cameras** | 30s | Blind spot |
| **Turrets** | 15s | Offline |
| **Mechanical Enemies** | 8s | Stunned, vulnerable |
| **Electronic Locks** | 20s | Temporarily unlocked |
| **Player Electronics** | 5s | HUD disruption |

### Flashlight

#### Functionality
- **Purpose**: Illuminate dark areas, disorient enemies
- **Usage**: Toggle on/off, adjustable beam intensity
- **Battery**: Limited power requiring management

#### Flashlight System

```csharp
// Advanced flashlight controller
public class FlashlightController : MonoBehaviour {
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainRate = 2f; // per second
    [SerializeField] private float rechargeRate = 5f; // per second
    
    [SerializeField] private Light spotLight;
    [SerializeField] private ParticleSystem beamEffect;
    
    public enum Intensity { Low, Medium, High, Ultra }
    
    private float currentBattery;
    private Intensity currentIntensity = Intensity.Medium;
    
    public void ToggleFlashlight() {
        if (currentBattery <= 0) return;
        
        isOn = !isOn;
        spotLight.enabled = isOn;
        
        if (isOn) {
            beamEffect.Play();
        } else {
            beamEffect.Stop();
        }
    }
    
    public void SetIntensity(Intensity intensity) {
        currentIntensity = intensity;
        spotLight.intensity = GetIntensityValue(intensity);
        drainRate = GetDrainRate(intensity);
    }
}
```

#### Combat Applications

| Use Case | Effect | Duration | Risk |
|----------|--------|----------|------|
| **Blind Enemy** | Disorient melee attackers | 2-3s | Reveals position |
| **Signal Flare** | Attract attention | 5s | High visibility |
| **Area Light** | Illuminate combat zone | 10s | Battery drain |

### Grappling Hook

#### Functionality
- **Purpose**: Vertical traversal, reach elevated areas
- **Usage**: Aim and shoot at anchor points
- **Limitations**: Weight capacity, range restrictions

#### Grappling Mechanics

```csharp
// Grappling hook physics system
public class GrapplingHook : MonoBehaviour {
    [SerializeField] private float maxDistance = 25f;
    [SerializeField] private float reelSpeed = 10f;
    [SerializeField] private float launchForce = 20f;
    
    [SerializeField] private LineRenderer ropeLine;
    [SerializeField] private Transform hookTip;
    
    private bool isGrappling = false;
    private Rigidbody playerRb;
    private Vector3 anchorPoint;
    
    public void LaunchHook(Vector3 targetDirection) {
        if (isGrappling) return;
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, targetDirection, out hit, maxDistance)) {
            if (hit.collider.CompareTag("GrapplePoint")) {
                anchorPoint = hit.point;
                StartCoroutine(GrapplingRoutine());
            }
        }
    }
    
    private IEnumerator GrapplingRoutine() {
        isGrappling = true;
        ropeLine.enabled = true;
        
        // Extend rope
        while (Vector3.Distance(hookTip.position, anchorPoint) > 0.5f) {
            hookTip.position = Vector3.MoveTowards(
                hookTip.position, 
                anchorPoint, 
                launchForce * Time.deltaTime
            );
            ropeLine.SetPosition(0, transform.position);
            ropeLine.SetPosition(1, hookTip.position);
            yield return null;
        }
        
        // Pull player
        while (Vector3.Distance(transform.position, anchorPoint) > 2f) {
            Vector3 pullDirection = (anchorPoint - transform.position).normalized;
            playerRb.AddForce(pullDirection * reelSpeed, ForceMode.Acceleration);
            
            ropeLine.SetPosition(0, transform.position);
            yield return null;
        }
        
        ReleaseHook();
    }
}
```

## Weapon Pickup & Inventory

### World Placement

#### Weapon Locations

**Realistic Placement**:
- **Maintenance Areas**: Wrenches, pipes, fire extinguishers
- **Security Offices**: Crowd control weapons, tasers
- **Laboratories**: Energy weapons, specialized tools
- **Storage Rooms**: Various improvised weapons
- **Emergency Lockers**: Basic self-defense items

**Environmental Storytelling**:
- Weapons placed where previous defenders fell
- Tools left by fleeing workers
- Emergency supplies in panic rooms

#### Pickup System

```csharp
// Weapon pickup controller
public class WeaponPickup : MonoBehaviour {
    [SerializeField] private Weapon weaponPrefab;
    [SerializeField] private int durability = 100;
    [SerializeField] private PickupType type;
    [SerializeField] private bool isReserve = false;
    
    public enum PickupType { Melee, Ranged, Tool, Ammo }
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            
            if (inventory.CanAddItem(type)) {
                inventory.AddItem(weaponPrefab, durability, isReserve);
                PlayPickupEffects();
                Destroy(gameObject);
            }
        }
    }
    
    private void PlayPickupEffects() {
        // Audio feedback
        AudioManager.PlayOneShot("weapon_pickup");
        
        // Visual feedback
        Instantiate(pickupEffect, transform.position, Quaternion.identity);
        
        // Haptic feedback
        Gamepad.SetVibration(0.2f, 0.3f, 0.1f);
    }
}
```

### Inventory System

#### Slot Configuration

| Slot Type | Capacity | Quick Access | Description |
|-----------|----------|--------------|-------------|
| **Melee** | 1 weapon | Key 1 | Primary melee weapon |
| **Ranged** | 1 weapon | Key 2 | Primary ranged weapon |
| **Tool 1-3** | 3 tools | Keys 3-5 | Quick tool access |
| **Reserve** | 2 items | Menu access | Backup equipment |
| **Ammo** | Varies | Auto-reload | Ammunition storage |

#### Inventory Management

```csharp
// Player inventory system
public class PlayerInventory : MonoBehaviour {
    [SerializeField] private WeaponSlot[] weaponSlots;
    [SerializeField] private AmmoInventory ammoInventory;
    
    [System.Serializable]
    public class WeaponSlot {
        public WeaponType type;
        public Weapon currentWeapon;
        public bool isOccupied;
        public GameObject weaponModel;
    }
    
    public bool AddItem(Weapon weapon, int durability, bool isReserve = false) {
        WeaponSlot targetSlot = FindAvailableSlot(weapon.type, isReserve);
        
        if (targetSlot != null) {
            targetSlot.currentWeapon = weapon;
            targetSlot.isOccupied = true;
            targetSlot.currentWeapon.currentDurability = durability;
            
            if (!isReserve) {
                EquipWeapon(targetSlot);
            }
            
            return true;
        }
        
        return false;
    }
    
    public void EquipWeapon(WeaponSlot slot) {
        if (currentEquippedWeapon != null) {
            currentEquippedWeapon.Sheath();
        }
        
        currentEquippedWeapon = slot.currentWeapon;
        currentEquippedWeapon.Draw();
        
        UpdateUI();
    }
}
```

### Quick-Switch System

#### Switching Mechanics

**Standard Switch** (0.3s):
- Smooth transition animation
- Weapon sheathing/drawing
- Audio feedback

**Quick Switch** (0.2s):
- Faster animation for tools
- Minimal sheathing
- Reduced audio feedback

**Emergency Switch** (0.1s):
- Instant weapon drop
- Immediate response
- Panic animation

#### Switch State Machine

```javascript
// Weapon switching state machine
WeaponSwitchStates {
  states: {
    Ready: {
      enter: () => { enableInput(); },
      transitions: {
        Switching: () => inputReceived && !isSwitching
      }
    },
    
    Switching: {
      enter: () => { 
        disableInput();
        playSwitchAnimation();
      },
      update: () => {
        if (animationComplete) {
          transitionTo('Ready');
        }
      },
      transitions: {
        Ready: () => animationComplete
      }
    }
  }
}
```

## Combat Integration

### Tactical Decision Making

#### Weapon Choice Impact

| Situation | Recommended Weapon | Rationale |
|-----------|-------------------|-----------|
| **Close Quarters** | Melee weapon | Faster response, no ammo waste |
| **Multiple Enemies** | Grenade/AoE | Area damage, crowd control |
| **Stealth Approach** | Taser/Tools | Silent takedown, minimal noise |
| **Armored Target** | Plasma rifle | Armor penetration capability |
| **Environmental** | Crowbar/Tools | Interactive solutions |

#### AI Tactical Recommendations

```csharp
// AI weapon recommendation system
public class TacticianAI : MonoBehaviour {
    [SerializeField] private float[] weaponScores;
    
    public WeaponType RecommendWeapon(CombatSituation situation) {
        float bestScore = -1f;
        WeaponType recommendation = WeaponType.Unarmed;
        
        foreach (WeaponType weapon in Enum.GetValues(typeof(WeaponType))) {
            float score = EvaluateWeaponForSituation(weapon, situation);
            
            if (score > bestScore) {
                bestScore = score;
                recommendation = weapon;
            }
        }
        
        return recommendation;
    }
    
    private float EvaluateWeaponForSituation(WeaponType weapon, CombatSituation situation) {
        float score = 0f;
        
        // Enemy count consideration
        if (situation.enemyCount > 3 && weapon == WeaponType.GrenadeLauncher) {
            score += 3f;
        }
        
        // Distance consideration
        if (situation.averageDistance > 20f && weapon == WeaponType.PlasmaRifle) {
            score += 2f;
        }
        
        // Stealth consideration
        if (situation.requiresStealth && weapon == WeaponType.Taser) {
            score += 2.5f;
        }
        
        return score;
    }
}
```

### Environmental Combat

#### Interactive Solutions

**Crowbar Applications**:
- Pry open locked doors (damage durability)
- Break weakened walls
- Leverage objects as barriers
- Disarm traps

**Grappling Hook Tactics**:
- Gain vertical advantage
- Create escape routes
- Retrieve distant items
- Set environmental traps

**Tool Combat Uses**:
- Flashlight blind enemies
- EMP disable mechanical threats
- Scanner reveal hidden enemies
- Lockpick bypass combat entirely

#### Environmental Kill Opportunities

```csharp
// Environmental interaction system
public class EnvironmentalInteraction : MonoBehaviour {
    [SerializeField] private InteractionType[] possibleInteractions;
    
    public enum InteractionType {
        PushOffLedge,
        ElectrocuteInWater,
        CrushWithObject,
        ExplodeBarrel,
        TriggerTrap
    }
    
    public bool CanInteract(Weapon tool, InteractionType type) {
        switch (type) {
            case InteractionType.PushOffLedge:
                return tool == WeaponType.Crowbar && IsNearLedge();
            case InteractionType.ElectrocuteInWater:
                return tool == WeaponType.EMPDevice && IsInWater();
            case InteractionType.CrushWithObject:
                return tool == WeaponType.GrapplingHook && HasHeavyObject();
            default:
                return false;
        }
    }
}
```

### NPC Reactions

#### Weapon-Specific Responses

| Weapon Type | NPC Reaction | Behavior Change |
|-------------|--------------|-----------------|
| **Unarmed** | Confident | Aggressive approach |
| **Melee** | Cautious | Maintain distance |
| **Taser** | Fearful | Retreat, cover |
| **Plasma Rifle** | Panic | Flee, surrender |
| **Explosives** | Terror | Immediate retreat |

#### Response System

```csharp
// NPC weapon reaction system
public class NPCWeaponReaction : MonoBehaviour {
    [SerializeField] private float[] threatLevels;
    [SerializeField] private BehaviorTree reactionTree;
    
    public void ReactToWeapon(WeaponType playerWeapon) {
        float threatLevel = CalculateThreatLevel(playerWeapon);
        UpdateBehavior(threatLevel);
    }
    
    private float CalculateThreatLevel(WeaponType weapon) {
        switch (weapon) {
            case WeaponType.Unarmed: return 0.2f;
            case WeaponType.Wrench: return 0.4f;
            case WeaponType.Crowbar: return 0.6f;
            case WeaponType.Taser: return 0.7f;
            case WeaponType.PlasmaRifle: return 0.9f;
            case WeaponType.GrenadeLauncher: return 1.0f;
            default: return 0.3f;
        }
    }
    
    private void UpdateBehavior(float threatLevel) {
        if (threatLevel > 0.8f) {
            // Flee behavior
            reactionTree.SetBehavior("Flee");
        } else if (threatLevel > 0.5f) {
            // Defensive behavior
            reactionTree.SetBehavior("Defensive");
        } else {
            // Normal behavior
            reactionTree.SetBehavior("Aggressive");
        }
    }
}
```

## Balance & Difficulty

### Difficulty Scaling

#### Weapon Availability

| Difficulty | Weapon Spawn Rate | Ammo Quantity | Tool Durability |
|------------|-------------------|---------------|-----------------|
| **Easy** | 1.5x | 2.0x | 1.3x |
| **Normal** | 1.0x | 1.0x | 1.0x |
| **Hard** | 0.7x | 0.6x | 0.8x |
| **Nightmare** | 0.5x | 0.3x | 0.6x |

#### Enemy Resistance Scaling

```csharp
// Difficulty-based enemy resistance
public class DifficultyScaling : MonoBehaviour {
    [SerializeField] private Difficulty currentDifficulty;
    
    [System.Serializable]
    public class ResistanceMultipliers {
        public float meleeDamage = 1.0f;
        public float rangedDamage = 1.0f;
        public float stunDuration = 1.0f;
        public float knockbackForce = 1.0f;
    }
    
    public ResistanceMultipliers GetResistances() {
        switch (currentDifficulty) {
            case Difficulty.Easy:
                return new ResistanceMultipliers {
                    meleeDamage = 0.7f,
                    rangedDamage = 0.6f,
                    stunDuration = 1.5f,
                    knockbackForce = 1.3f
                };
                
            case Difficulty.Hard:
                return new ResistanceMultipliers {
                    meleeDamage = 1.3f,
                    rangedDamage = 1.4f,
                    stunDuration = 0.7f,
                    knockbackForce = 0.8f
                };
                
            default:
                return new ResistanceMultipliers();
        }
    }
}
```

### Weapon Balance Matrix

#### Damage vs. Speed Trade-offs

| Weapon | DPS | Stamina/s | Risk Factor | Skill Required |
|--------|-----|-----------|-------------|----------------|
| **Unarmed** | Medium | Low | Low | Low |
| **Wrench** | Medium-High | Medium | Medium | Low |
| **Pipe** | High | Medium | Medium | Medium |
| **Crowbar** | Very High | High | High | Medium |
| **Taser** | Low | Very Low | Very Low | Low |
| **Plasma Rifle** | High | Low | Medium | High |

#### situational Effectiveness

| Situation | Best Weapon | Worst Weapon | Alternative |
|-----------|-------------|--------------|-------------|
| **1v1 Melee** | Crowbar | Taser | Pipe |
| **Multiple Enemies** | Grenade Launcher | Wrench | Plasma Rifle |
| **Stealth** | Taser | Crowbar | Flashlight |
| **Long Range** | Plasma Rifle | All Melee | None |
| **Puzzle Solving** | Tools | Weapons | N/A |

### Player Skill Progression

#### Weapon Mastery System

```csharp
// Weapon skill progression
public class WeaponMastery : MonoBehaviour {
    [SerializeField] private WeaponSkill[] weaponSkills;
    
    [System.Serializable]
    public class WeaponSkill {
        public WeaponType weapon;
        public int experience;
        public int level;
        public float[] damageBonus = {0f, 0.05f, 0.1f, 0.15f, 0.2f};
        public float[] staminaReduction = {0f, 0.05f, 0.1f, 0.15f, 0.2f};
    }
    
    public void AddExperience(WeaponType weapon, int amount) {
        WeaponSkill skill = GetWeaponSkill(weapon);
        skill.experience += amount;
        
        // Check for level up
        int requiredXP = (skill.level + 1) * 100;
        if (skill.experience >= requiredXP) {
            LevelUpWeapon(skill);
        }
    }
    
    private void LevelUpWeapon(WeaponSkill skill) {
        skill.level++;
        skill.experience = 0;
        
        // Apply bonuses
        UpdateWeaponStats(skill.weapon);
        ShowLevelUpNotification(skill.weapon, skill.level);
    }
}
```

## Weapon Durability System

### Degradation Mechanics

#### Durability Loss

| Action | Durability Cost | Critical Chance |
|--------|-----------------|-----------------|
| **Light Attack** | 1 | 5% |
| **Heavy Attack** | 2 | 10% |
| **Special Attack** | 3 | 15% |
| **Blocked Hit** | 0.5 | 2% |
| **Environmental Use** | 5 | 20% |

#### Weapon States

| Condition | Damage Modifier | Break Chance | Visual Effects |
|-----------|----------------|--------------|----------------|
| **Perfect (100-80%)** | 1.0x | 0% | None |
| **Good (79-60%)** | 0.95x | 5% | Minor wear |
| **Worn (59-40%)** | 0.85x | 15% | Visible damage |
| **Damaged (39-20%)** | 0.7x | 30% | Heavy wear |
| **Broken (19-1%)** | 0.5x | 50% | Severely damaged |
| **Destroyed (0%)** | 0x | 100% | unusable |

### Repair System

#### Repair Methods

**Weapon Maintenance**:
- **Whetstone**: Restore 20% durability (common)
- **Repair Kit**: Restore 50% durability (rare)
- **Workbench**: Full repair (limited locations)

**Tool Maintenance**:
- **Battery Replacement**: Restore full charge
- **Calibration**: Restore precision
- **Cleaning**: Reduce jam chance

#### Repair Implementation

```csharp
// Weapon repair system
public class WeaponRepair : MonoBehaviour {
    [SerializeField] private RepairType[] repairOptions;
    
    [System.Serializable]
    public class RepairType {
        public string name;
        public int durabilityRestored;
        public float successChance;
        public GameObject repairItem;
    }
    
    public bool AttemptRepair(Weapon weapon, RepairType repairType) {
        if (Random.value <= repairType.successChance) {
            int newDurability = Mathf.Min(
                weapon.currentDurability + repairType.durabilityRestored,
                weapon.maxDurability
            );
            
            weapon.currentDurability = newDurability;
            PlayRepairEffects(weapon);
            return true;
        }
        
        // Repair failed
        PlayFailureEffects();
        return false;
    }
}
```

## Technical Requirements

### Core Systems Architecture

#### Weapon System Components

```csharp
// Main weapon system controller
public class WeaponSystem : MonoBehaviour {
    [Header("Core Components")]
    [SerializeField] private WeaponInventory inventory;
    [SerializeField] private WeaponSwitching switchController;
    [SerializeField] private WeaponDurability durabilitySystem;
    [SerializeField] private WeaponAnimation animationController;
    
    [Header("Combat Integration")]
    [SerializeField] private HitDetection hitDetection;
    [SerializeField] private ImpactFeedback impactSystem;
    [SerializeField] private WeaponAudio audioController;
    
    [Header("UI Systems")]
    [SerializeField] private WeaponHUD weaponHUD;
    [SerializeField] private CrosshairController crosshair;
    [SerializeField] private AmmoCounter ammoDisplay;
    
    private void Awake() {
        InitializeWeaponDatabase();
        SetupInputBindings();
        LoadWeaponConfigurations();
    }
    
    private void Update() {
        HandleWeaponInput();
        UpdateWeaponStates();
        ProcessDurabilityDecay();
    }
}
```

#### Data Structures

```csharp
// Weapon data structure
[System.Serializable]
public class WeaponData {
    [Header("Basic Properties")]
    public string weaponName;
    public string description;
    public WeaponType type;
    public WeaponCategory category;
    public GameObject weaponPrefab;
    
    [Header("Combat Stats")]
    public int baseDamage;
    public float attackSpeed;
    public int staminaCost;
    public float knockbackForce;
    public float range;
    
    [Header("Durability")]
    public int maxDurability;
    public int durabilityPerUse;
    public bool canBreak;
    
    [Header("Ammunition")]
    public AmmoType ammoType;
    public int magazineSize;
    public int reserveAmmo;
    public float reloadTime;
    
    [Header("Visuals")]
    public AnimationClip[] attackAnimations;
    public ParticleSystem[] muzzleEffects;
    public AudioClip[] attackSounds;
    
    [Header("Special Properties")]
    public bool hasSpecialAttack;
    public string specialAbility;
    public float specialCooldown;
}
```

### Performance Requirements

#### Optimization Targets

| System | Target FPS | Memory Usage | Loading Time |
|--------|------------|--------------|--------------|
| **Weapon Switching** | 60+ | <10MB | <100ms |
| **Combat Effects** | 60+ | <50MB | <200ms |
| **Inventory Management** | 60+ | <5MB | <50ms |
| **Tool Usage** | 60+ | <15MB | <150ms |

#### Memory Management

```csharp
// Weapon asset management
public class WeaponAssetManager : MonoBehaviour {
    [SerializeField] private Dictionary<WeaponType, WeaponData> weaponDatabase;
    [SerializeField] private Queue<GameObject> weaponPool;
    
    private void Start() {
        InitializeWeaponPool();
        PreloadCriticalAssets();
    }
    
    public GameObject GetWeaponInstance(WeaponType type) {
        WeaponData data = weaponDatabase[type];
        GameObject weapon = Instantiate(data.weaponPrefab);
        
        // Apply shared materials
        ApplyOptimizedMaterials(weapon);
        
        // Configure LOD
        SetupLOD(weapon);
        
        return weapon;
    }
    
    private void InitializeWeaponPool() {
        foreach (WeaponType type in Enum.GetValues(typeof(WeaponType))) {
            for (int i = 0; i < 3; i++) {
                GameObject pooledWeapon = GetWeaponInstance(type);
                pooledWeapon.SetActive(false);
                weaponPool.Enqueue(pooledWeapon);
            }
        }
    }
}
```

### Save/Load Integration

#### Weapon State Persistence

```javascript
// Weapon save data structure
WeaponSaveData {
  // Current equipped weapons
  equippedMelee: {
    weaponId: string,
    currentDurability: number,
    ammunition: number,
    modifications: string[]
  },
  
  equippedRanged: {
    weaponId: string,
    currentDurability: number,
    ammunition: number,
    modifications: string[]
  },
  
  // Tool states
  equippedTools: [
    {
      toolId: string,
      batteryLevel: number,
      charges: number,
      cooldownRemaining: number
    }
  ],
  
  // Inventory contents
  inventory: {
    weapons: [
      {
        weaponId: string,
        durability: number,
        ammoReserve: number,
        slotIndex: number
      }
    ],
    
    ammunition: {
      [ammoType]: number
    },
    
    tools: [
      {
        toolId: string,
        condition: number,
        uses: number
      }
    ]
  },
  
  // Weapon progression
  weaponSkills: {
    [weaponType]: {
      experience: number,
      level: number,
      unlocks: string[]
    }
  }
}
```

## Integration Points

### Combat System Integration

#### Hit Detection Integration

```csharp
// Weapon-specific hit detection
public class WeaponHitDetection : MonoBehaviour {
    [SerializeField] private Weapon currentWeapon;
    [SerializeField] private LayerMask targetLayers;
    
    private void OnAttackStart() {
        // Enable weapon-specific hit boxes
        EnableHitBoxes(currentWeapon.type);
    }
    
    private void OnAttackActive() {
        // Perform hit detection
        Collider[] hits = Physics.OverlapBox(
            currentWeapon.hitBoxCenter,
            currentWeapon.hitBoxSize,
            currentWeapon.transform.rotation,
            targetLayers
        );
        
        ProcessHits(hits);
    }
    
    private void ProcessHits(Collider[] hits) {
        foreach (Collider hit in hits) {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null) {
                int damage = CalculateDamage(currentWeapon, hit);
                damageable.TakeDamage(damage, currentWeapon.type);
                
                // Apply weapon-specific effects
                ApplySpecialEffects(currentWeapon, hit);
            }
        }
    }
}
```

#### Animation System Integration

```csharp
// Weapon animation controller
public class WeaponAnimationController : MonoBehaviour {
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator weaponAnimator;
    
    [Header("Animation States")]
    [SerializeField] private string[] attackStates;
    [SerializeField] private string[] reloadStates;
    [SerializeField] private string[] switchStates;
    
    public void PlayAttackAnimation(AttackType type) {
        string stateName = GetAttackStateName(type);
        
        // Play player animation
        playerAnimator.Play(stateName);
        
        // Play weapon animation
        weaponAnimator.Play(stateName);
        
        // Trigger animation events
        InvokeAnimationEvents(type);
    }
    
    private void InvokeAnimationEvents(AttackType type) {
        switch (type) {
            case AttackType.Light:
                OnLightAttackStart();
                break;
            case AttackType.Heavy:
                OnHeavyAttackStart();
                break;
            case AttackType.Special:
                OnSpecialAttackStart();
                break;
        }
    }
}
```

### Audio System Integration

#### Weapon Audio Controller

```csharp
// Weapon-specific audio management
public class WeaponAudioController : MonoBehaviour {
    [SerializeField] private FMODStudioEventEmitter weaponEmitter;
    
    [Header("Audio Events")]
    [SerializeField] private string[] attackSounds;
    [SerializeField] private string[] hitSounds;
    [SerializeField] private string[] missSounds;
    [SerializeField] private string[] reloadSounds;
    
    public void PlayAttackSound(WeaponType weapon, AttackType type) {
        string eventName = GetAttackSoundName(weapon, type);
        
        FMOD.Studio.EventInstance sound = FMODUnity.RuntimeManager.CreateInstance(eventName);
        sound.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
        sound.start();
        sound.release();
    }
    
    public void PlayHitSound(Material material, float force) {
        string materialTag = GetMaterialTag(material);
        string forceTag = GetForceTag(force);
        string eventName = $"weapon_hit_{materialTag}_{forceTag}";
        
        // Play impact sound with proper parameters
        PlayImpactSound(eventName);
    }
}
```

### UI System Integration

#### Weapon HUD Controller

```csharp
// Weapon heads-up display
public class WeaponHUD : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private Slider durabilityBar;
    [SerializeField] private TextMeshProUGUI ammoCounter;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private GameObject[] toolIndicators;
    
    public void UpdateWeaponDisplay(Weapon weapon) {
        weaponName.text = weapon.weaponName;
        weaponIcon.sprite = weapon.icon;
        
        UpdateDurability(weapon);
        UpdateAmmo(weapon);
        UpdateToolStatus(weapon);
    }
    
    private void UpdateDurability(Weapon weapon) {
        if (weapon.hasDurability) {
            durabilityBar.gameObject.SetActive(true);
            durabilityBar.value = (float)weapon.currentDurability / weapon.maxDurability;
            
            // Color code by condition
            Color barColor = GetDurabilityColor(weapon.GetCondition());
            durabilityBar.fillRect.GetComponent<Image>().color = barColor;
        } else {
            durabilityBar.gameObject.SetActive(false);
        }
    }
    
    private Color GetDurabilityColor(WeaponCondition condition) {
        switch (condition) {
            case WeaponCondition.Perfect: return Color.green;
            case WeaponCondition.Good: return Color.yellow;
            case WeaponCondition.Worn: return Color.orange;
            case WeaponCondition.Damaged: return Color.red;
            case WeaponCondition.Broken: return Color.darkRed;
            default: return Color.gray;
        }
    }
}
```

## Prototype Deliverables

### Core Systems

#### Minimum Viable Weapons System
1. **Basic Melee Weapons**: Wrench, pipe, crowbar with durability
2. **Simple Ranged Weapon**: Taser with limited ammo
3. **Tool System**: Flashlight and lockpick
4. **Inventory Management**: 3-slot weapon system
5. **Switching Mechanics**: Basic weapon switching

#### Enhanced Features
1. **Advanced Combat**: Plasma rifle and grenade launcher
2. **Tool Variety**: Full tool suite (scanner, EMP, grappling hook)
3. **Durability System**: Complete degradation and repair
4. **Weapon Mastery**: Skill progression system
5. **Environmental Integration**: Interactive weapon uses

### Asset Requirements

#### 3D Models
- Weapon models (wrench, pipe, crowbar, taser, plasma rifle)
- Tool models (flashlight, lockpick, scanner, EMP device)
- First-person view models with proper rigging
- World pickup models

#### Animation Set
- Attack animations (light, heavy, special) per weapon
- Weapon switching animations
- Reload animations for ranged weapons
- Tool usage animations
- Impact and hit reaction animations

#### Audio Assets
- Weapon attack sounds (swing, shoot, impact)
- Tool activation sounds
- Durability damage sounds
- Weapon switching audio
- Environmental interaction sounds

### Performance Targets

#### Frame Rate Requirements
- Combat with effects: 60+ FPS
- Weapon switching: 60+ FPS
- Tool usage: 60+ FPS
- Inventory management: 60+ FPS

#### Memory Budget
- Weapon system: <100MB total
- Individual weapons: <5MB each
- Tool systems: <2MB each
- Audio assets: <20MB total

## QA and Testing Checklist

### Functional Testing

#### Weapon Mechanics
- [ ] All weapons deal correct damage values
- [ ] Attack speeds match specifications
- [ ] Stamina costs are accurate
- [ ] Knockback forces work as intended
- [ ] Durability decreases correctly
- [ ] Weapon breaking functions properly

#### Tool Functionality
- [ ] Lockpick mini-game works for all difficulties
- [ ] Hacking interface is functional
- [ ] Scanner detects appropriate targets
- [ ] EMP device affects correct systems
- [ ] Flashlight illuminates properly
- [ ] Grappling hook physics work correctly

#### Inventory System
- [ ] Weapon pickup functions
- [ ] Quick-switching works smoothly
- [ ] Ammo tracking is accurate
- [ ] Tool quick-slots respond correctly
- [ ] Reserve system works
- [ ] UI updates reflect changes

### Integration Testing

#### Combat Integration
- [ ] Weapons integrate with hit detection
- [ ] Damage applies to enemies correctly
- [ ] NPC reactions trigger appropriately
- [ ] Environmental interactions work
- [ ] Combo system includes weapons
- [ ] Stealth mechanics function

#### System Integration
- [ ] Save/load preserves weapon states
- [ ] Audio triggers correctly
- [ ] Visual effects display properly
- [ ] UI updates in real-time
- [ ] Input handling is responsive
- [ ] Performance maintains targets

### Balance Testing

#### Difficulty Scaling
- [ ] Easy mode provides adequate resources
- [ ] Normal mode offers balanced challenge
- [ ] Hard mode creates appropriate tension
- [ ] Nightmare mode is extremely challenging
- [ ] Weapon availability scales correctly
- [ ] Enemy resistance adjustments work

#### Weapon Balance
- [ ] No single weapon dominates all situations
- [ ] Each weapon has tactical advantages
- [ ] Upgrade progression feels rewarding
- [ ] Durability creates meaningful choices
- [ ] Ammo scarcity generates tension
- [ ] Tools offer alternative solutions

### Performance Testing

#### Frame Rate Validation
- [ ] Combat maintains 60+ FPS
- [ ] Weapon switching causes no stutter
- [ ] Tool usage doesn't impact performance
- [ ] Multiple weapons in scene perform well
- [ ] Effects and particles are optimized
- [ ] Memory usage stays within budget

#### Stress Testing
- [ ] Rapid weapon switching stability
- [ ] Simultaneous tool usage
- [ ] Maximum inventory capacity
- [ ] Extended combat sessions
- [ ] Memory leak detection
- [ ] Save/load under stress

### Usability Testing

#### Controls and Interface
- [ ] Weapon switching is intuitive
- [ ] Tool access is convenient
- [ ] Inventory management is clear
- [ ] HUD displays necessary information
- [ ] Audio feedback is appropriate
- [ ] Visual indicators are clear

#### Player Experience
- [ ] Weapons feel impactful
- [ ] Tools are useful and intuitive
- [ ] Combat flow is smooth
- [ ] Difficulty progression is natural
- [ ] Resource management creates tension
- [ ] Alternative approaches are viable

### Compatibility Testing

#### Platform Validation
- [ ] Keyboard and mouse controls work
- [ ] Gamepad support is functional
- [ ] Custom key remapping works
- [ ] Different screen resolutions supported
- [ ] Audio systems work on all hardware
- [ ] Performance targets met on minimum specs

#### Save System Compatibility
- [ ] Weapon states save correctly
- [ ] Inventory persists between sessions
- [ ] Tool conditions are maintained
- [ ] Ammo quantities are accurate
- [ ] Weapon progression saves
- [ ] Cross-session compatibility verified

### Bug Reporting

#### Known Issues Tracking
- [ ] Document any weapon switching delays
- [ ] Note durability calculation discrepancies
- [ ] Track tool interaction edge cases
- [ ] Monitor performance under specific conditions
- [ ] Record any animation glitches
- [ ] Log audio synchronization issues

#### Regression Testing
- [ ] Previous weapon functionality preserved
- [ ] Existing combat mechanics unaffected
- [ ] Tool integration doesn't break other systems
- [ ] Save/load compatibility maintained
- [ ] Performance improvements preserved
- [ ] UI changes don't introduce new issues