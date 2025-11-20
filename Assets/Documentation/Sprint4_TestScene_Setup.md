# Sprint 4 Test Scene Setup Guide

## Acceptance Test Scene Creation

This guide walks through creating the test scene for Sprint 4 acceptance criteria.

## Scene Setup

### 1. Base Scene Configuration

1. Open/Create Main.unity scene
2. Keep existing Main Camera with FirstPersonCamera script
3. Keep existing DirectionalLight for lighting
4. Create a new empty GameObject "InteractionTestZone"

### 2. Manager Setup

These should be created if not already present:

1. **InputManager**
   - Should already exist from Sprint 1
   - Ensures E key is bound to Interact action

2. **GameManager**
   - Should already exist from Sprint 1
   - Handles pause and scene loading

3. **InventoryManager**
   - Auto-created as singleton on first InventoryManager.Instance call
   - Or manually create empty GameObject and add component

4. **InteractionManager**
   - Auto-created as singleton on first access
   - Or manually create empty GameObject with InteractionManager component
   - Set Interactable Layer to appropriate layer

5. **InteractionPromptUI**
   - Create Canvas for UI
   - Add TextMeshPro text for prompt display
   - Add InteractionPromptUI component

6. **ItemPickupNotification**
   - Create separate Canvas or use same as above
   - Add TextMeshPro text for notifications
   - Add ItemPickupNotification component

7. **ObjectStateManager**
   - Create empty GameObject and add component
   - This handles save/load functionality

### 3. Layer Setup

Create a new layer called "Interactable":
1. Go to Layer dropdown in Inspector
2. Add new layer "Interactable"
3. Set InteractionManager's Interactable Layer to "Interactable"
4. Assign all interactable GameObjects to this layer

### 4. Test Object Setup

#### A. Weapon Pickup (Assault Rifle)

```
GameObject: AssaultRifle_Pickup
├─ Collider: Box Collider (Trigger: OFF)
├─ Renderer: Simple cube or model (red material)
└─ PickupItem Script:
   ├─ Item Data: Create new Item
   │  ├─ ID: "weapon_assault_rifle"
   │  ├─ Name: "Assault Rifle"
   │  ├─ Description: "An assault rifle for combat"
   │  ├─ Type: Weapon
   │  ├─ Weight: 3.5
   │  └─ Max Stack: 1
   ├─ Pickup Count: 1
   ├─ Remove After Pickup: true
   ├─ Pickup Sound: (Optional) rifle_pickup.wav
   ├─ Pickup Animation Duration: 0.5
   └─ Highlight Sound: (Optional) soft_beep.wav
```

#### B. Health Pack Pickup

```
GameObject: HealthPack_Small
├─ Collider: Sphere Collider (Trigger: OFF)
├─ Renderer: Sphere with green material
└─ PickupItem Script:
   ├─ Item Data: Create new Item
   │  ├─ ID: "consumable_health_small"
   │  ├─ Name: "Health Pack (Small)"
   │  ├─ Description: "Restores 25 health"
   │  ├─ Type: Consumable
   │  ├─ Weight: 0.2
   │  └─ Max Stack: 5
   ├─ Pickup Count: 1
   ├─ Contextual Message: "This may prove useful."
   └─ Highlight Color: Green
```

#### C. Locked Door (Physical Lock)

```
GameObject: LockedDoor_Metal
├─ Collider: Box Collider
├─ Renderer: Cube with gray material
├─ Child: DoorPivot (empty GameObject)
│  └─ Renderer: Actual door model
└─ LockedDoor Script:
   ├─ Lock Type: Physical
   ├─ Start Locked: true
   ├─ Unlock Duration: 2.0
   ├─ Open Angle: 90
   ├─ Open Speed: 2.0
   ├─ Door Pivot: DoorPivot (assign)
   ├─ Door Open Sound: door_open.wav
   ├─ Door Close Sound: door_close.wav
   ├─ Lock Sound: lock_click.wav
   └─ Unlock Sound: unlock_success.wav
```

#### D. Electronic Door (Requires Hacking)

```
GameObject: ElectronicDoor_Bulkhead
├─ Collider: Box Collider
├─ Renderer: Bulkhead door model
├─ Child: DoorPivot
│  └─ Renderer: Door model
└─ LockedDoor Script:
   ├─ Lock Type: Electronic
   ├─ Start Locked: true
   ├─ Unlock Duration: 3.0
   ├─ Door Pivot: DoorPivot
   └─ Audio: Electronic lock sounds
```

#### E. Lockpick Item

```
GameObject: Lockpick_Pickup
├─ Collider: Box Collider
├─ Renderer: Small tool model
└─ PickupItem Script:
   ├─ Item Data:
   │  ├─ ID: "tool_lockpick"
   │  ├─ Name: "Lockpick Set"
   │  ├─ Type: Tool
   │  ├─ Tool Type: Lockpick
   │  └─ Weight: 0.3
   └─ Highlight Color: Yellow
```

#### F. Hacking Device Item

```
GameObject: HackingDevice_Pickup
├─ Collider: Box Collider
├─ Renderer: Device model
└─ PickupItem Script:
   ├─ Item Data:
   │  ├─ ID: "tool_hacking_device"
   │  ├─ Name: "Hacking Device"
   │  ├─ Type: Tool
   │  ├─ Tool Type: HackingDevice
   │  └─ Weight: 0.5
   └─ Highlight Color: Cyan
```

#### G. Document Pickup

```
GameObject: Document_Pickup
├─ Collider: Box Collider
├─ Renderer: Paper/document model
└─ PickupItem Script:
   ├─ Item Data:
   │  ├─ ID: "document_mission_brief"
   │  ├─ Name: "Mission Brief"
   │  ├─ Type: Document
   │  └─ Weight: 0.1
   └─ Contextual Message: "Interesting. Add this to your knowledge."
```

#### H. Terminal (Interactive)

```
GameObject: SecurityTerminal
├─ Collider: Box Collider
├─ Renderer: Terminal model/screen
└─ Terminal Script:
   ├─ Terminal Title: "Security Terminal v2.1"
   ├─ Terminal Text: "SECURITY SYSTEM ONLINE
   │                  All systems nominal.
   │                  Press [ACCESS] for status."
   ├─ Requires Hacking: true
   ├─ Hacking Duration: 3.0
   ├─ Boot Sound: terminal_boot.wav
   ├─ Hacking Sound: hacking_loop.wav
   └─ Complete Sound: hacking_success.wav
```

### 5. UI Canvas Setup

#### Interaction Prompt Canvas

```
Canvas
├─ Name: InteractionPromptCanvas
├─ Canvas Scaler: Scale with Screen Size
├─ Render Mode: Screen Space - Overlay
└─ Child: InteractionPromptText (TextMeshPro)
   ├─ Text: "Press E to interact"
   ├─ Alignment: Center / Center
   ├─ Font Size: 36
   ├─ Anchor: Center (or crosshair position)
   └─ Component: InteractionPromptUI

Add to Canvas:
- InteractionPromptUI script
```

#### Pickup Notification Canvas

```
Canvas
├─ Name: NotificationCanvas
├─ Render Mode: Screen Space - Overlay
└─ Child: NotificationText (TextMeshPro)
   ├─ Text: "Item picked up"
   ├─ Alignment: Bottom-Left
   ├─ Font Size: 24
   ├─ Anchor: Bottom-Left
   └─ Component: CanvasGroup

Add to Canvas:
- ItemPickupNotification script
```

### 6. Layer Assignment

1. Select all interactable GameObjects
2. In Inspector, set Layer to "Interactable"
3. Verify InteractionManager has correct layer assigned

### 7. Scene Layout Suggestion

```
Player Start Position: (0, 1, 0)

Test Objects Layout:
- Weapon pickup: (3, 1, 0)       [straight ahead]
- Health pack: (5, 1, 2)          [right side]
- Locked door: (0, 1, -5)         [behind]
- Physical lock: placed at door
- Lockpick: (2, 1, -3)            [near door]
- Electronic door: (10, 1, -5)    [further back]
- Hacking device: (8, 1, -3)      [near electronic door]
- Terminal: (15, 1, 0)            [far right]
- Document: (10, 1, 5)            [opposite side]
```

## Acceptance Test Procedure

### Test 1: Weapon Pickup
```
GIVEN: Player starts game and weapon is on ground 3m away
WHEN: Player aims at weapon
THEN: 
  - Object highlights with yellow glow
  - Prompt appears: "Press E to Pick up Assault Rifle"
  - Soft beep sound plays
WHEN: Player presses E
THEN:
  - Weapon animates (shrinks + fades)
  - Pickup sound plays
  - Notification shows "Item picked up: Assault Rifle"
  - Weapon disappears from world
  - Weapon appears in inventory
```

### Test 2: Inventory Full
```
GIVEN: Inventory has 20 items
WHEN: Player tries to pick up 21st item
THEN:
  - Pickup fails silently
  - Item remains in world
  - Console logs warning
```

### Test 3: Locked Door - Physical Lock
```
GIVEN: Player approaches locked physical door
WHEN: Player aims at door
THEN:
  - Door highlights
  - Prompt shows: "Press E to Examine (Locked)"
WHEN: Player presses E without lockpick
THEN:
  - Door doesn't open
  - Prompt changes to: "Press E to Examine (Requires Lockpick)"
WHEN: Player picks up lockpick
WHEN: Player aims at door again
THEN:
  - Prompt shows: "Press E to Unlock"
WHEN: Player presses E with lockpick
THEN:
  - Hacking progress bar appears (or timer)
  - Hacking sound plays
  - After 2 seconds: Door unlocks (sound plays)
  - Prompt changes to: "Press E to Open"
WHEN: Player presses E
THEN:
  - Door rotates open
  - Door open sound plays
```

### Test 4: Electronic Door - Hacking Device
```
GIVEN: Player approaches electronic door
WHEN: Player needs hacking device
WHEN: Player picks up hacking device
WHEN: Player approaches door
WHEN: Player presses E with hacking device
THEN:
  - Electronic hacking sounds play
  - After 3 seconds: Door unlocks
  - Electronic unlock sound plays
```

### Test 5: Terminal Access
```
GIVEN: Player approaches terminal
WHEN: Player aims at terminal
THEN:
  - Prompt shows: "Press E to Access Terminal"
WHEN: Player presses E (if requires hacking and no device)
THEN:
  - "Access Denied (Requires Hacking Device)" displays
WHEN: Player has hacking device
WHEN: Player presses E
THEN:
  - Terminal boot sound plays
  - Hacking loop sound plays
  - After 3 seconds: Access granted sound
  - Terminal content displays in debug/UI
```

### Test 6: Document Pickup
```
GIVEN: Document on ground
WHEN: Player picks up document
THEN:
  - Pickup animation plays
  - Contextual message appears
  - Document in inventory
```

### Test 7: Quick Slots
```
GIVEN: Player has items in inventory
WHEN: Player presses "1" key (in Quick Slot 1)
THEN:
  - Item equips or quick slot fills
```

### Test 8: State Persistence
```
GIVEN: Player picks up items, unlocks door
WHEN: Player presses save (via menu or F5)
THEN:
  - ObjectStateManager saves state
  - File written to persistent data path
WHEN: Player loads game
THEN:
  - Picked up items don't respawn
  - Door remains unlocked
  - Terminal remains accessed
```

### Test 9: Weight System
```
GIVEN: Player has max weight
WHEN: Player tries to pick up heavy item
THEN:
  - Pickup fails
  - Warning message displays
  - Item stays in world
```

### Test 10: Inventory Events
```
GIVEN: UI listening to InventoryManager events
WHEN: Player picks up item
THEN:
  - OnItemAdded fires
  - OnInventoryChanged fires
  - UI updates
```

## Console Debug Output

Expected console output during test:

```
Interacted with interactable object
[Weapon_Assault_Rifle]: Interacted by Player
World state saved to /path/to/world_state.json

Terminal [Security Terminal v2.1]
SECURITY SYSTEM ONLINE
All systems nominal.
Press [ACCESS] for status.
```

## Performance Benchmarks to Verify

1. **Highlight Detection**: Check frame rate while moving across objects
   - Should maintain 60+ FPS
   
2. **Pickup Animation**: Multiple items picked up in sequence
   - Should be smooth without stutter
   
3. **Inventory Add**: Add 20 items rapidly
   - Should complete <5ms total
   
4. **State Serialization**: Save/load with 20 objects
   - Should complete <100ms

## Troubleshooting

### Issue: Prompt not showing
- Check InteractionPromptUI has TextMeshPro component
- Verify Canvas is active and visible
- Check InteractionManager layer matches object layer

### Issue: Pickup not working
- Verify PickupItem component attached
- Check Item Data assigned
- Verify itemData is not null
- Check InventoryManager exists

### Issue: Door won't open
- Verify LockedDoor script, not just InteractableDoor
- Check door pivot assigned correctly
- Verify lock type matches tool type
- Check audio source on door GameObject

### Issue: Terminal won't hack
- Verify Terminal script assigned
- Check hacking duration > 0
- Verify HackingDevice tool type matches
- Check audio clips assigned

## Next Steps (Sprint 5)

1. Create inventory UI panel
2. Display items in grid
3. Add item tooltips
4. Implement equipment slots
5. Add drag/drop functionality

## File Locations

- Scene: `/home/engine/project/Assets/Scenes/Main.unity`
- Scripts: `/home/engine/project/Assets/Scripts/Systems/`
- Documentation: `/home/engine/project/Assets/Documentation/`

## Additional Resources

See also:
- Sprint4_Interactions_Inventory.md (Technical docs)
- CodingStandards.md (Code conventions)
- AssetSourcing.md (Audio file sources)
