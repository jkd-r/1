# Protocol EMR - Combat System

## Overview

This directory contains the complete implementation of the first-person combat system for Protocol EMR, developed as part of Sprint 3: Core Systems Phase.

## Quick Start

### Basic Setup

1. **Attach Combat Manager to Player**:
   ```
   Player GameObject
   ├── CombatManager.cs
   ├── WeaponManager.cs
   ├── CombatFeedbackSystem.cs
   ├── CombatAudioManager.cs
   ├── CombatInputHandler.cs
   └── UnknownAIIntegration.cs
   ```

2. **Configure Camera Reference**:
   - Set `playerCamera` field in CombatManager
   - Ensure camera is tagged "MainCamera"

3. **Setup Attack Origin**:
   - Create child object under player: "AttackOrigin"
   - Position in front of player (0.5m forward, chest height)
   - Assign to `attackOrigin` field

4. **Configure Layer Masks**:
   - Create "Enemy" layer
   - Assign to all NPCs
   - Set `enemyLayer` in CombatManager to "Enemy"

### NPC Setup

1. **Add NPC Components**:
   ```
   NPC GameObject
   ├── NPCCombatController.cs
   ├── NavMeshAgent
   ├── Animator
   ├── Rigidbody (for knockback)
   └── Collider
   ```

2. **Configure NavMesh**:
   - Bake NavMesh in scene
   - Ensure NPCs are on NavMesh
   - Set obstacle avoidance

3. **Setup Health Bar**:
   - Assign `healthBarPrefab` in NPCCombatController
   - Prefab should contain Canvas + Image (fill)

## Directory Structure

```
Combat/
├── Core/
│   ├── CombatManager.cs              # Main player combat controller
│   ├── DifficultyScaling.cs          # Global difficulty system
│   └── UnknownAIIntegration.cs       # AI narrator tactical hints
├── Weapons/
│   ├── WeaponManager.cs              # Weapon inventory & switching
│   └── WeaponPickup.cs               # World weapon pickups
├── Feedback/
│   └── CombatFeedbackSystem.cs       # Screen shake, particles, VFX
├── Audio/
│   └── CombatAudioManager.cs         # Combat audio management
├── NPC/
│   ├── NPCCombatController.cs        # NPC combat AI & states
│   └── NPCHealthBar.cs               # NPC health bar display
└── Input/
    └── CombatInputHandler.cs         # Input integration layer
```

## Core Systems

### Combat Manager

**Purpose**: Handles all player combat actions

**Key Methods**:
- `PerformAttack(AttackType)` - Execute attack with type
- `PerformDodge()` - Dodge with invulnerability
- `TakeDamage(float)` - Player takes damage
- `GetCurrentStamina()` - Query stamina level

**Key Fields**:
- `punchDamage` - Damage per punch (default: 10)
- `kickDamage` - Damage per kick (default: 15)
- `meleeRange` - Attack range in meters (default: 2)
- `dodgeCooldown` - Cooldown between dodges (default: 1.5s)

### Weapon Manager

**Purpose**: Manages weapon inventory and ranged weapons

**Key Methods**:
- `SwitchToWeapon(WeaponType)` - Change active weapon
- `PickupWeapon(WeaponType)` - Add weapon to inventory
- `DropWeapon()` - Drop current weapon
- `FireRangedWeapon()` - Shoot ranged weapon
- `ReloadWeapon()` - Reload current weapon

**Weapon Slots**:
- Slot 1: Fist (always available)
- Slots 2-7: Picked-up weapons

### Combat Feedback System

**Purpose**: Provides visual and tactile combat feedback

**Key Methods**:
- `OnHitSuccess(Vector3, float, AttackType)` - Trigger hit feedback
- `OnPlayerDamaged(float)` - Trigger damage feedback
- `TriggerScreenShake(float)` - Manual screen shake
- `EnableDamageNumbers(bool)` - Toggle damage numbers

**Feedback Types**:
- Screen shake (camera offset)
- Damage flash (red vignette)
- Blood particles
- Damage numbers (floating text)
- Impact effects

### NPC Combat Controller

**Purpose**: NPC combat behavior and state management

**Key Methods**:
- `TakeDamage(float, Vector3, Vector3)` - Damage with knockback
- `ApplyStun(float)` - Stun for duration
- `SetAggressionLevel(float)` - Adjust aggression (difficulty)
- `GetHealthPercentage()` - Health as percentage

**States**:
- Idle → Alert → Attack → Damaged → Death
- Stagger (light hit)
- Knockback (heavy hit)
- Stunned (taser effect)

### Difficulty Scaling

**Purpose**: Global difficulty settings and scaling

**Key Methods**:
- `SetDifficulty(DifficultyLevel)` - Change difficulty
- `ScalePlayerDamage(float)` - Apply player damage scaling
- `ScaleNPCDamage(float)` - Apply NPC damage scaling
- `ShouldDropAmmo()` - Check if ammo should drop

**Difficulty Levels**:
- Easy: 1.5x player damage, 0.5x NPC damage
- Normal: 1.0x balanced
- Hard: 0.75x player damage, 1.25x NPC damage
- Nightmare: 0.5x player damage, 1.5x NPC damage, no ammo

## Configuration Guide

### Combat Parameters

Adjust in CombatManager inspector:

```
Damage:
- Punch: 10 HP
- Kick: 15 HP

Stamina:
- Max Stamina: 100
- Punch Cost: 5
- Kick Cost: 8
- Dodge Cost: 10
- Regen Rate: 10/sec

Knockback:
- Punch: 5 units
- Kick: 10 units

Range:
- Melee Range: 2 meters
- Melee Width: 0.5 meters

Timing:
- Attack Speed: 0.6 seconds
- Dodge Duration: 0.4 seconds
- Dodge Cooldown: 1.5 seconds
```

### Weapon Configuration

Edit in WeaponManager `InitializeWeapons()`:

```csharp
new WeaponData {
    type = WeaponType.Wrench,
    weaponName = "Wrench",
    damage = 15f,
    attackSpeed = 0.5f,
    maxDurability = 100,
    isRanged = false
}
```

### Feedback Settings

Adjust in CombatFeedbackSystem:

```
Screen Shake:
- Punch Intensity: 0.1
- Weapon Intensity: 0.3
- Ranged Intensity: 0.5
- Duration: 0.2 seconds

Damage Flash:
- Color: Red (1, 0, 0, 0.3)
- Duration: 0.3 seconds

Damage Numbers:
- Enabled: true
- Color: Red
- Duration: 1 second
```

## Input Bindings

Default controls (configurable in CombatInputHandler):

```
Left Mouse Button:  Primary Attack (Punch)
Right Mouse Button: Secondary Attack (Kick)
Q:                  Dodge
E:                  Interact / Pick Up Weapon
R:                  Reload
G:                  Drop Weapon
1-7:                Weapon Slots
```

## Animation Requirements

### Player Animations (Mixamo)

Required animation triggers:
- `PunchJab` - Quick jab
- `PunchCross` - Power cross
- `KickRoundhouse` - Roundhouse kick
- `KickSide` - Side kick
- `WrenchSwing` - Wrench swing
- `CrowbarJab` - Crowbar jab
- `PipeSmash` - Pipe smash
- `Dodge` - Dodge animation

### NPC Animations

Required animation triggers:
- `Attack` - NPC attack
- `Hit` - Light damage
- `Knockback` - Heavy damage
- `Stunned` - Stun animation
- `Death` - Death animation

Animation parameters:
- `Speed` (float) - Movement speed
- `State` (int) - Current state index

## Audio Setup

### Required Audio Clips

Assign in CombatAudioManager:

**Combat Sounds**:
- Punch sounds (3-5 variations)
- Kick sounds (3-5 variations)
- Hit impacts (3-5 variations)

**Weapon Sounds**:
- Wrench swing
- Crowbar swing
- Pipe swing
- Taser buzz
- Plasma zap

**NPC Sounds**:
- Grunt sounds (5+ variations)
- Pain sounds (5+ variations)
- Death sounds (3-5 variations)

**UI Sounds**:
- Weapon switch
- Weapon pickup
- Weapon break
- Reload
- Empty click

### Audio Configuration

```csharp
// In CombatAudioClip
volumeMin = 0.9f;      // Minimum volume
volumeMax = 1.0f;      // Maximum volume
pitchMin = 0.9f;       // Minimum pitch (variation)
pitchMax = 1.1f;       // Maximum pitch (variation)
```

## Performance Guidelines

### Target Performance

- **Combat raycast**: < 1ms per attack
- **Knockback physics**: < 2ms per frame
- **Animation blending**: < 1ms per frame
- **Particle effects**: < 3ms total
- **Target FPS**: 60 FPS with 3+ NPCs

### Optimization Tips

1. **Use Layer Masks**: Limit raycast collision checks
2. **Pool Particles**: Reuse particle systems when possible
3. **Throttle Audio**: Use sound cooldowns
4. **Cull Health Bars**: Hide beyond 20 meters
5. **Limit Simultaneous NPCs**: Cap at 5-10 active NPCs

## Integration Points

### Sprint 1 (Input & Camera)

Combat system uses Sprint 1 input mapping:
- Mouse input for attacks
- Keyboard for abilities
- Camera system for screen shake

### Sprint 2 (Movement & Interaction)

Combat integrates with movement:
- Cannot attack while sprinting
- Shares stamina pool
- Uses interaction system for pickups
- Respects physics layers

### Sprint 3+ (Future Systems)

Ready for integration with:
- UI/HUD system (health, stamina, ammo display)
- Inventory system (weapon storage, durability tracking)
- Mission system (combat objectives, kill counters)
- Save/Load system (combat state persistence)

## Troubleshooting

### Common Issues

**Problem**: Attacks not hitting enemies
- **Solution**: Check Layer Mask configuration
- **Solution**: Verify enemy has collider on correct layer
- **Solution**: Increase `meleeRange` or `meleeWidth`

**Problem**: Screen shake too intense/weak
- **Solution**: Adjust shake intensity values in CombatFeedbackSystem
- **Solution**: Check camera transform reference

**Problem**: Animations not playing
- **Solution**: Verify Animator has required triggers
- **Solution**: Check Animator Controller transitions
- **Solution**: Ensure animations are imported correctly

**Problem**: Weapons not switching
- **Solution**: Check weapon slot key bindings
- **Solution**: Verify weapon has been picked up
- **Solution**: Check WeaponManager weapon list

**Problem**: NPCs not reacting to damage
- **Solution**: Check NPCCombatController is attached
- **Solution**: Verify health values
- **Solution**: Check state machine transitions

**Problem**: Performance issues
- **Solution**: Reduce active NPC count
- **Solution**: Lower particle counts
- **Solution**: Profile physics performance
- **Solution**: Check for memory leaks

## Testing Checklist

- [ ] Player can punch and kick
- [ ] Damage values are correct
- [ ] Stamina drains and regenerates
- [ ] Dodge works with cooldown
- [ ] Weapons can be picked up
- [ ] Weapon switching responsive
- [ ] Ranged weapons hit targets
- [ ] Reload functions correctly
- [ ] NPCs take damage correctly
- [ ] NPC health bars update
- [ ] Knockback physics work
- [ ] NPC death/ragdoll functions
- [ ] Screen shake feels good
- [ ] Audio plays correctly
- [ ] Damage numbers display
- [ ] Performance meets 60 FPS
- [ ] Difficulty scaling works
- [ ] Unknown AI hints display

## API Reference

### CombatManager

```csharp
public void PerformAttack(AttackType attackType)
public void PerformDodge()
public void TakeDamage(float damage)
public bool IsInvulnerable()
public CombatState GetCurrentState()
public float GetCurrentStamina()
public float GetMaxStamina()
```

### WeaponManager

```csharp
public void SwitchToWeapon(WeaponType weaponType)
public bool PickupWeapon(WeaponType weaponType)
public void DropWeapon()
public void ReduceWeaponDurability(int amount = 1)
public WeaponData GetCurrentWeapon()
public bool IsReloading()
```

### NPCCombatController

```csharp
public void TakeDamage(float damage, Vector3 knockbackForce, Vector3 hitPoint)
public void ApplyStun(float duration)
public void SetAggressionLevel(float level)
public NPCState GetCurrentState()
public float GetHealthPercentage()
public bool IsDead()
```

### CombatFeedbackSystem

```csharp
public void OnHitSuccess(Vector3 hitPosition, float damage, AttackType attackType)
public void OnPlayerDamaged(float damage)
public void TriggerScreenShake(float intensity)
public void EnableDamageNumbers(bool enabled)
public void SpawnImpactEffect(Vector3 position, Vector3 normal)
```

### DifficultyScaling

```csharp
public void SetDifficulty(DifficultyLevel level)
public float ScalePlayerDamage(float baseDamage)
public float ScaleNPCDamage(float baseDamage)
public bool ShouldDropAmmo()
public DifficultyLevel GetCurrentDifficulty()
```

## Support & Documentation

For detailed implementation information, see:
- **Full Documentation**: `/docs/protocol-emr/sprint3-combat-implementation.md`
- **Combat Specification**: `/docs/protocol-emr/combat-movement.md`
- **Project Roadmap**: `/docs/protocol-emr/build-coding-roadmap.md`

## Version

**Sprint**: 3 (Core Systems Phase)
**Version**: 1.0.0
**Last Updated**: 2024
**Status**: Production Ready

## Contributors

Protocol EMR Development Team

## License

Protocol EMR - Proprietary
