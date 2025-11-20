# Combat System Setup Guide

## Quick Setup (5 Minutes)

This guide will get you from zero to a working combat scene in under 5 minutes.

## Prerequisites

- Unity 2021.3 LTS or later
- Mixamo account (free) for animations
- Basic Unity knowledge

## Step 1: Scene Setup (1 minute)

1. **Create New Scene**:
   - File → New Scene
   - Save as `TestCombat.unity`

2. **Add Ground Plane**:
   - GameObject → 3D Object → Plane
   - Scale: (10, 1, 10)
   - Position: (0, 0, 0)

3. **Add Lighting**:
   - Scene automatically has directional light
   - Adjust if needed

## Step 2: Player Setup (2 minutes)

### Create Player Object

1. **Create Capsule for Player**:
   ```
   GameObject → 3D Object → Capsule
   Name: Player
   Tag: Player
   Position: (0, 1, 0)
   ```

2. **Add Components**:
   - Rigidbody (set constraints: Freeze Rotation X, Y, Z)
   - CapsuleCollider (already present)

3. **Add Combat Scripts** (drag from src/Combat/):
   ```
   Player GameObject:
   - Core/CombatManager.cs
   - Weapons/WeaponManager.cs
   - Feedback/CombatFeedbackSystem.cs
   - Audio/CombatAudioManager.cs
   - Input/CombatInputHandler.cs
   ```

4. **Create Attack Origin**:
   ```
   Create Empty Child of Player
   Name: AttackOrigin
   Position: (0, 0, 0.5) relative to player
   ```

5. **Configure CombatManager**:
   - Player Camera: Drag Main Camera
   - Attack Origin: Drag AttackOrigin object
   - Enemy Layer: Select "Enemy" (create if doesn't exist)

### Setup Camera

1. **Position Main Camera**:
   ```
   Main Camera
   Parent: Player
   Position: (0, 0.6, 0)  // Eye height
   Rotation: (0, 0, 0)
   ```

2. **Configure Camera**:
   - Clear Flags: Skybox
   - Field of View: 60
   - Tag: MainCamera

## Step 3: NPC Setup (2 minutes)

### Create Test NPC

1. **Create Capsule for NPC**:
   ```
   GameObject → 3D Object → Capsule
   Name: TestNPC
   Layer: Enemy (create if doesn't exist)
   Position: (0, 1, 5)  // 5 meters in front of player
   ```

2. **Add Components**:
   ```
   - NavMeshAgent
   - Animator
   - Rigidbody (uncheck "Use Gravity" for now)
   - NPC/NPCCombatController.cs
   ```

3. **Configure NPCCombatController**:
   - Max Health: 30
   - Attack Damage: 5
   - Detection Range: 10
   - Target Player: Drag Player object

4. **Configure NavMeshAgent**:
   - Speed: 3.5
   - Angular Speed: 120
   - Acceleration: 8
   - Stopping Distance: 2

### Bake NavMesh

1. **Open Navigation Window**:
   - Window → AI → Navigation

2. **Bake Settings**:
   - Agent Radius: 0.5
   - Agent Height: 2
   - Max Slope: 45
   - Step Height: 0.4

3. **Bake**:
   - Select ground plane
   - Navigation window → Object tab → Check "Navigation Static"
   - Bake tab → Click "Bake"

## Step 4: Layer Setup (30 seconds)

1. **Create Layers**:
   - Edit → Project Settings → Tags and Layers
   - Add Layer "Enemy"
   - Add Layer "Player"

2. **Assign Layers**:
   - Player object → Layer: Player
   - TestNPC object → Layer: Enemy

3. **Physics Layer Collision** (optional):
   - Edit → Project Settings → Physics
   - Uncheck Player-Player collision
   - Uncheck Enemy-Enemy collision

## Step 5: Testing (1 minute)

### Test Controls

1. **Play the Scene**

2. **Test Basic Combat**:
   - Left Mouse Button → Punch (should hit NPC at close range)
   - Right Mouse Button → Kick
   - Q → Dodge

3. **Expected Behavior**:
   - Screen shakes on hit
   - NPC health decreases
   - NPC reacts to damage
   - Console shows combat logs

### Debug Mode

Add debug visualization by adding this to CombatManager:

```csharp
void OnDrawGizmos()
{
    if (attackOrigin != null && playerCamera != null)
    {
        Gizmos.color = Color.red;
        Vector3 direction = playerCamera.transform.forward;
        Gizmos.DrawRay(attackOrigin.position, direction * meleeRange);
        Gizmos.DrawWireSphere(attackOrigin.position + direction * meleeRange, meleeWidth);
    }
}
```

## Common Setup Issues

### Issue: Attacks Don't Hit

**Solution 1**: Check Layer Configuration
```
- NPC must be on "Enemy" layer
- CombatManager.enemyLayer must include "Enemy" layer
```

**Solution 2**: Check Range
```
- Stand closer to NPC (within 2 meters)
- Increase meleeRange in CombatManager if needed
```

**Solution 3**: Check Attack Origin
```
- Attack Origin should be in front of player
- Position: (0, 0, 0.5) relative to player
```

### Issue: NPC Doesn't Move

**Solution**: Check NavMesh
```
- NavMesh must be baked
- NPC must be on NavMesh (blue area in Scene view)
- NavMeshAgent must be enabled
```

### Issue: No Screen Shake

**Solution**: Check Camera Reference
```
- CombatManager.playerCamera must be assigned
- Camera must be child of player or follow player
```

### Issue: Console Errors

**Solution**: Check Component Order
```
- CombatManager should be added before other combat components
- All scripts must compile without errors
```

## Advanced Setup

### Adding Animations (Optional for Basic Testing)

1. **Download from Mixamo**:
   - Go to mixamo.com
   - Download animations:
     - Punching (multiple variations)
     - Kicking
     - Dodging
     - Hit Reactions
     - Death animations

2. **Import to Unity**:
   - Drag FBX files into Unity
   - Unity will auto-create Animator Controller

3. **Setup Animator**:
   - Create Animator Controller
   - Add animation states
   - Add triggers: PunchJab, KickRoundhouse, Dodge, Hit, Death

4. **Assign to Player**:
   - Player → Animator component
   - Assign Animator Controller
   - CombatManager.playerAnimator → Assign Animator

### Adding Health Bar (Optional)

1. **Create Health Bar Prefab**:
   ```
   Canvas (World Space)
   └── Image (Background)
       └── Image (Fill)
   ```

2. **Configure Canvas**:
   - Render Mode: World Space
   - Width: 100
   - Height: 20
   - Scale: 0.01 x 0.01 x 0.01

3. **Configure Fill Image**:
   - Image Type: Filled
   - Fill Method: Horizontal
   - Fill Amount: 1

4. **Assign to NPC**:
   - NPCCombatController.healthBarPrefab → Drag prefab

### Adding Weapons (Optional)

1. **Create Weapon Prefab**:
   ```
   GameObject → 3D Object → Cube
   Name: Wrench
   Scale: (0.1, 0.5, 0.1)
   Add: WeaponPickup.cs
   ```

2. **Configure WeaponPickup**:
   - Weapon Type: Wrench
   - Weapon Display Name: "Wrench"
   - Interaction Range: 3

3. **Place in Scene**:
   - Position near player
   - Add Collider (trigger)

4. **Test**:
   - Approach weapon
   - Press E to pick up
   - Press 2 to switch to wrench

### Adding Combat UI (Optional)

1. **Create Canvas**:
   ```
   GameObject → UI → Canvas
   Name: CombatUI
   ```

2. **Add Damage Flash**:
   ```
   Canvas → UI → Image
   Name: DamageFlash
   Color: Red, Alpha: 0
   Stretch to fill screen
   ```

3. **Assign to Feedback System**:
   - CombatFeedbackSystem.damageFlashImage → Drag DamageFlash

4. **Add Damage Number Prefab** (optional):
   ```
   3D Text or TextMeshPro
   Name: DamageNumber
   Save as prefab
   ```

## Performance Testing

### Test with Multiple NPCs

1. **Duplicate NPC**:
   - Select TestNPC
   - Ctrl+D (duplicate) twice
   - Position at different locations

2. **Test Performance**:
   - Play scene
   - Window → Analysis → Profiler
   - Check FPS (should maintain 60)
   - Check CPU usage (combat < 3ms)

### Performance Targets

```
Target: 60 FPS with 3 NPCs
- Combat raycast: < 1ms
- Knockback physics: < 2ms
- Animations: < 1ms
- Particles: < 3ms
```

## Next Steps

### Recommended Order

1. ✅ **Basic Setup** (above) - Working combat
2. **Add Animations** - Visual polish
3. **Add Audio** - Sound feedback
4. **Add Weapons** - Gameplay variety
5. **Add UI** - Player feedback
6. **Tune Values** - Balance gameplay

### Integration with Other Systems

Once basic combat works:

1. **Integrate with Sprint 1** (Input):
   - Replace input handling with input system
   - Add key rebinding support

2. **Integrate with Sprint 2** (Movement):
   - Connect to stamina pool
   - Add sprint blocking during combat

3. **Integrate with Sprint 3+** (UI/HUD):
   - Add health bar
   - Add stamina bar
   - Add weapon durability display

## Reference Values

### Recommended Starting Values

```csharp
// Combat
punchDamage = 10f;
kickDamage = 15f;
attackSpeed = 0.6f;
meleeRange = 2f;
meleeWidth = 0.5f;

// Stamina
maxStamina = 100f;
punchStaminaCost = 5f;
kickStaminaCost = 8f;
dodgeStaminaCost = 10f;
staminaRegenRate = 10f;

// Knockback
punchKnockback = 5f;
kickKnockback = 10f;

// Dodge
dodgeDuration = 0.4f;
dodgeCooldown = 1.5f;

// NPC
maxHealth = 30f;
attackDamage = 5f;
attackRange = 2f;
attackCooldown = 2f;
detectionRange = 10f;
```

### Tuning Tips

**Too Easy**:
- Reduce player damage
- Increase NPC health
- Decrease stamina regen
- Increase NPC aggression

**Too Hard**:
- Increase player damage
- Reduce NPC health
- Increase stamina regen
- Reduce NPC aggression

**Stamina Issues**:
- Increase stamina regen rate
- Reduce stamina costs
- Increase max stamina

**Combat Feel**:
- Screen shake intensity
- Knockback force
- Attack speed
- Animation timing

## Troubleshooting Checklist

Before asking for help, verify:

- [ ] All scripts compiled without errors
- [ ] Player has all required components
- [ ] NPC has all required components
- [ ] Layers created and assigned correctly
- [ ] NavMesh baked successfully
- [ ] Camera reference assigned
- [ ] Attack Origin created and positioned
- [ ] Console shows no errors
- [ ] Unity version compatible (2021.3+)

## Support

- **Documentation**: `/docs/protocol-emr/sprint3-combat-implementation.md`
- **README**: `/src/Combat/README.md`
- **Combat Spec**: `/docs/protocol-emr/combat-movement.md`

## Completion Checklist

Setup is complete when:

- [ ] Player can punch (LMB)
- [ ] Player can kick (RMB)
- [ ] Player can dodge (Q)
- [ ] NPC takes damage
- [ ] Screen shakes on hit
- [ ] NPC eventually dies
- [ ] Performance maintains 60 FPS
- [ ] No console errors

**Congratulations!** You now have a working combat system. Time to add polish and integrate with other systems.
