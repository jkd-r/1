# Sprint 5: Inventory UI & HUD Systems - Implementation Guide

## Overview

Sprint 5 implements a complete inventory management system with a polished HUD, comprehensive settings menus, and visual feedback systems for Protocol EMR.

## System Architecture

### Core Components

#### 1. **Inventory System** (`InventoryManager.cs`)
- Manages player inventory with weight-based capacity
- Supports stackable and non-stackable items
- Equipment slots for weapons and armor
- 4 quick-access slots for rapid item selection
- Events for inventory changes

**Key Methods:**
```csharp
AddItem(ItemData item)              // Add item to inventory
RemoveItem(ItemData item, int qty)  // Remove item from inventory
EquipItem(ItemData item, slot)      // Equip item to slot
UnequipItem(EquipmentSlot slot)     // Unequip item from slot
AssignToQuickSlot(ItemData, index)  // Assign item to quick slot
GetItemsByType(ItemType type)       // Filter items by type
```

#### 2. **Item Data System** (`ItemData.cs`)
- Base `ItemData` class with common properties (name, weight, rarity, icon)
- Specialized classes:
  - `EquipmentData`: Armor, weapons with damage/defense stats
  - `ConsumableData`: Health packs, stamina restores
  - `WeaponData`: Melee/ranged weapons with ammo tracking
  - `AmmoData`: Ammunition for ranged weapons

**Item Types:**
- Weapon
- Armor
- Consumable
- Ammo
- Tool
- Document
- Miscellaneous

**Rarity Levels:**
- Common, Uncommon, Rare, Epic, Legendary

#### 3. **HUD Manager** (`HUDManager.cs`)
- Real-time health and stamina bar display
- Ammo counter with magazine management
- Crosshair display and customization
- Objective markers with fading
- Interaction prompts
- Damage feedback system with directional indicators
- Low health warning with pulsing vignette
- Weapon icon display for equipped weapons

**HUD Elements:**
- Health Bar (bottom-left)
- Stamina Bar (bottom-left, shows during sprint)
- Ammo Counter (bottom-right)
- Crosshair (center screen)
- Objective Text (top-left, auto-fade)
- Interaction Prompt (center, near crosshair)
- Weapon Icons (top-center)

#### 4. **Inventory UI Manager** (`InventoryUIManager.cs`)
- 5x4 grid-based inventory display (20 slots, expandable to 25)
- Item filtering by type (Weapons, Tools, Consumables, Documents)
- Search functionality for finding items by name
- Sorting options (by name, weight, rarity)
- Item detail panel showing stats and information
- Quick slot management
- Weight tracking display
- Equipped weapon display

**Features:**
- Toggle with I key (from InputManager)
- Smooth slide-in/out animation (0.4 sec)
- Real-time filtering and sorting
- Drag-and-drop support for quick slots
- Item hover highlighting

#### 5. **Menu UI Manager** (`MenuUIManager.cs`)
- Pause menu with Resume/Settings/Inventory/Main Menu/Quit
- Main menu with New Game/Load/Settings/Credits/Exit
- Settings menu with tabbed interface
- Confirmation dialogs for destructive actions
- Animated transitions between menus

**Pause Menu Features:**
- Accessible with ESC key
- Game paused (Time.timeScale = 0)
- Can access inventory without unpausing
- Settings changes persist after resume
- Context-aware restrictions (no pause during cinematics)

#### 6. **Settings Panels** (`SettingsPanelUI.cs`)
Organized settings with real-time preview:

**Graphics Tab:**
- FOV slider (60-120°, default 90°)
- Motion blur toggle
- Depth of field toggle
- Post-processing intensity slider
- Resolution and FPS cap dropdowns
- Ray tracing toggle (if supported)

**Audio Tab:**
- Master volume slider (0-100%)
- Music volume slider (0-100%)
- SFX volume slider (0-100%)
- Voice/Dialogue volume slider (0-100%)
- Spatial audio toggle
- Subtitles toggle with size selector

**Gameplay Tab:**
- Difficulty dropdown (Easy/Normal/Hard/Nightmare)
- HUD opacity slider (30-100%)
- Objective markers toggle
- Crosshair style dropdown
- Camera sensitivity slider (0.5-5.0)
- Sprint toggle vs hold
- Invert Y-axis toggle

**Accessibility Tab:**
- Colorblind mode selector (None/Protanopia/Deuteranopia/Tritanopia/Achromatopsia)
- High contrast mode toggle
- UI scale slider (0.8x-1.3x)
- Font size selector
- Subtitle customization

**Controls Tab:**
- Action-based keybinding rebinding interface
- "Press any key..." prompt for rebinding
- Conflict detection with resolution options
- Preset configurations (WASD/IJKL/ESDF/Gamepad)
- Export/Import keybindings

#### 7. **Keybinding System** (`KeybindingItemUI.cs`)
- Visual list of all input actions
- Click to rebind action
- Shows current binding next to action
- Conflict detection prevents silent overrides
- Reset single action or all bindings
- Integrates with InputManager for persistence

#### 8. **Health System** (`HealthSystem.cs`)
- Manages player health and stamina
- Stamina regeneration with configurable delays
- Damage system with direction support
- Death handling with optional respawn
- Event notifications for HUD updates
- Integrates with notification system

**Methods:**
```csharp
TakeDamage(float damage, Vector3 direction)  // Apply damage
Heal(float amount)                             // Restore health
ConsumesStamina(float amount)                 // Spend stamina
RestoreStamina(float amount)                  // Recover stamina
GetHealthPercentage()                         // 0-1 value
GetStaminaPercentage()                        // 0-1 value
```

#### 9. **Notification Manager** (`NotificationManager.cs`)
- Centralized notification system
- Different notification types with custom durations
- Integration with HUD display

**Notification Types:**
- ItemPickup (2 sec)
- MissionUpdate (3 sec)
- AchievementUnlock (5 sec)
- QuestComplete (5 sec)
- SystemMessage (3 sec)
- Warning (4 sec)
- Error (4 sec)

**Methods:**
```csharp
ShowNotification(string message, NotificationType type)
ShowItemPickup(string itemName, int quantity)
ShowMissionUpdate(string missionText)
ShowAchievementUnlock(string name, string description)
ShowQuestComplete(string questName)
ShowWarning(string warningText)
ShowError(string errorText)
```

#### 10. **UI Theme Manager** (`UIThemeManager.cs`)
- Centralized color theme system
- Colorblind-friendly color schemes
- High-contrast mode support
- ScriptableObject-based configuration

**Color Sets:**
- Default (cyan/amber sci-fi aesthetic)
- Protanopia (Red → Blue conversion)
- Deuteranopia (Green → Yellow conversion)
- Tritanopia (Yellow → Pink conversion)
- Achromatopsia (Complete grayscale)

## UI Component Structure

### Inventory UI Hierarchy
```
InventoryCanvas
├── GridContainer
│   ├── InventorySlot (0-19)
│   │   ├── Icon
│   │   ├── Quantity Text
│   │   └── Highlight Border
├── DetailPanel
│   ├── ItemName
│   ├── ItemDescription
│   ├── ItemStats
│   ├── ItemIcon
│   ├── Weight Text
│   └── Type Text
├── QuickSlotsBar (Top)
│   ├── QuickSlot (0-3)
│   │   ├── Icon
│   │   ├── Slot Number
│   │   └── Background
├── ControlsPanel
│   ├── Sort Dropdown (Type/Name/Weight)
│   ├── Filter Dropdown
│   └── Search Input Field
└── EquippedItemsDisplay
    ├── Melee Weapon Icon
    └── Ranged Weapon Icon
```

### HUD Canvas Hierarchy
```
HUDCanvas
├── HealthBar
│   ├── Bar Fill
│   └── Text Display
├── StaminaBar
│   ├── Bar Fill
│   └── Text Display
├── AmmoCounter
├── Crosshair
├── ObjectiveText (with fade)
├── InteractionPrompt (with fade)
├── WeaponIndicators
│   ├── Melee Icon
│   └── Ranged Icon
├── DamageIndicators
│   ├── Left Edge Flash
│   ├── Right Edge Flash
│   ├── Top Edge Flash
│   └── Bottom Edge Flash
├── VignettePulse (Low health warning)
└── NotificationContainer
    └── Notification Items (dynamic)
```

### Menu Canvas Hierarchy
```
MenuCanvas
├── PauseMenuPanel
│   ├── Title
│   ├── ResumeButton
│   ├── SettingsButton
│   ├── InventoryButton
│   ├── MainMenuButton
│   └── QuitButton
├── MainMenuPanel
│   ├── Logo
│   ├── NewGameButton
│   ├── LoadGameButton
│   ├── SettingsButton
│   ├── CreditsButton
│   └── ExitButton
├── SettingsPanel
│   ├── TabButtons (Graphics/Audio/Gameplay/Controls/Accessibility)
│   ├── GraphicsTab
│   ├── AudioTab
│   ├── GameplayTab
│   ├── ControlsTab
│   ├── AccessibilityTab
│   └── ApplyButton
└── ConfirmationDialog
    ├── MessageText
    ├── ConfirmButton
    └── CancelButton
```

## Integration with Previous Sprints

### Depends On:
- **Sprint 1**: InputManager, SettingsManager, GameManager
- **Sprint 2**: Character animations and models
- **Sprint 3**: Combat system for ammo/weapon data
- **Sprint 4**: Inventory data structures

### Provides To:
- **Sprint 6**: Save/load UI for inventory state
- **Sprint 7-8**: Audio system integration for UI SFX
- **Sprint 9-10**: NPC interaction UI overlays

## Performance Targets

| Metric | Target |
|--------|--------|
| Inventory UI render time | < 5ms per frame |
| HUD render time | < 3ms per frame |
| Settings UI render time | < 3ms per frame |
| Menu transitions | Smooth 60 FPS |
| Total UI memory | < 50MB |
| Inventory grid updates | < 1ms per frame |
| Canvas redrawn | Minimal (only on change) |

## Accessibility Features

### Colorblind Support
- Protanopia: Red → Blue, maintains contrast
- Deuteranopia: Green → Yellow, red → transparent
- Tritanopia: Yellow → Pink, blue → red
- Achromatopsia: Full grayscale with high contrast

### Contrast Compliance
- WCAG AA standard (4.5:1 for text)
- High contrast mode (7:1 ratio)
- No reliance on color alone for information

### Input Accessibility
- Keyboard navigation (Tab to move, Enter to select)
- Gamepad support (D-Pad/Stick navigation, A/X to confirm, B/Circle to cancel)
- Rebindable controls in settings
- Adjustable mouse sensitivity

### Visual Accessibility
- Adjustable UI scale (0.8x - 1.3x)
- Font size options (small, medium, large)
- Dyslexia-friendly font option
- Adjustable HUD opacity
- High contrast backgrounds

### Auditory Accessibility
- Closed caption support
- Subtitle size customization
- Visual indicators for sound direction
- Volume controls per category

## Configuration Files

### UITheme ScriptableObject
Create a new UITheme asset at `Assets/Resources/UITheme.asset`:
```json
{
  "defaultColors": {
    "primaryAccent": "#00F0FF",  // Cyan
    "warningColor": "#FF9A3C",   // Amber
    "healthColor": "#00FF00",    // Green
    "damageColor": "#FF0000"     // Red
  }
}
```

### Settings Persistence
Settings saved to `Application.persistentDataPath/game_settings.json`:
```json
{
  "graphics": {
    "fov": 90,
    "motionBlur": true,
    "rayTracing": false
  },
  "gameplay": {
    "hudOpacity": 0.8,
    "difficulty": "Normal"
  }
}
```

## How to Use

### Adding Items to Inventory
```csharp
ItemData healthPack = new ConsumableData
{
    itemId = "health_pack_01",
    itemName = "Medical Kit",
    quantity = 1,
    weight = 0.5f,
    healthRestore = 50f
};

InventoryManager.Instance.AddItem(healthPack);
NotificationManager.Instance.ShowItemPickup("Medical Kit");
```

### Managing Health & Stamina
```csharp
HealthSystem healthSystem = GetComponent<HealthSystem>();

// Take damage from enemy
healthSystem.TakeDamage(25f, enemyPosition);

// Use stamina for sprinting
if (healthSystem.ConsumesStamina(10f))
{
    StartSprint();
}

// Heal from consumable
healthSystem.Heal(50f);
```

### Showing Notifications
```csharp
// Item pickup
NotificationManager.Instance.ShowItemPickup("Plasma Rifle", 1);

// Mission update
NotificationManager.Instance.ShowMissionUpdate("Find the research data");

// Achievement
NotificationManager.Instance.ShowAchievementUnlock("First Blood", "Kill first enemy");

// Warning
NotificationManager.Instance.ShowWarning("Low ammo!");
```

### Customizing HUD
```csharp
HUDManager hud = HUDManager.Instance;

// Update weapon display
hud.SetWeaponIcons(meleeSprite, rangedSprite);

// Show objective
hud.SetObjective("Proceed to the main lab", fadeDuration: 5f);

// Adjust HUD opacity
hud.SetHUDOpacity(0.7f);

// Show damage flash
hud.ShowDamageIndicator(damageDirection, intensity: 1f);
```

### Opening/Closing Inventory
```csharp
// From input or button
if (InputManager.Instance != null)
{
    InputManager.Instance.OnInventory += () =>
    {
        InventoryUIManager.Instance.ToggleInventory();
    };
}

// Or manually
InventoryUIManager.Instance.OpenInventory();
InventoryUIManager.Instance.CloseInventory();
```

## Testing Checklist

### Inventory System
- [ ] Can add/remove items
- [ ] Weight capacity prevents overfilling
- [ ] Stackable items combine quantities
- [ ] Filtering by type works
- [ ] Searching by name works
- [ ] Sorting by weight/name/rarity works
- [ ] Quick slots assign and retrieve items
- [ ] Equipment can be equipped/unequipped

### HUD Display
- [ ] Health bar updates in real-time
- [ ] Stamina bar shows only during sprint
- [ ] Ammo counter updates when firing
- [ ] Crosshair visible and customizable
- [ ] Objective text appears and fades
- [ ] Interaction prompts show correctly
- [ ] Damage flash effects on impact
- [ ] Low health warning pulses
- [ ] Weapon icons display correctly

### UI Menus
- [ ] Pause menu opens with ESC
- [ ] Game pauses when menu open
- [ ] Settings changes apply immediately
- [ ] All tabs accessible and functional
- [ ] Keybinding rebinding works
- [ ] Conflict detection appears for duplicate bindings
- [ ] Menu animations smooth
- [ ] Confirmation dialogs appear for destructive actions

### Accessibility
- [ ] Colorblind mode changes colors
- [ ] High contrast mode readable
- [ ] UI scale slider works
- [ ] Keyboard navigation works
- [ ] Gamepad navigation works
- [ ] Subtitles display correctly
- [ ] Font sizes adjustable

### Performance
- [ ] Inventory grid renders <5ms
- [ ] HUD updates <3ms
- [ ] Menu transitions stay 60 FPS
- [ ] No memory leaks on repeated opens/closes
- [ ] Canvas redrawn only on state change

## Future Enhancements

### Sprint 6 (Save/Load Integration)
- Persist inventory state to save files
- Load inventory on game load
- Quick load/save from inventory UI

### Sprint 7-8 (Audio Integration)
- UI SFX for button clicks, item pickups
- Contextual audio for notifications
- Volume control per audio type
- FMOD integration

### Sprint 9-10 (Advanced Features)
- Item comparison overlay (hold shift to compare)
- Crafting UI for tool creation
- Equipment loadout presets
- Minimap in inventory overlay
- Item trading/selling UI
- Achievement notifications with particle effects
- Voice command support (accessibility)

## Known Limitations

1. No item dragging between slots (UI limitation - can be enhanced)
2. No item comparison modal (planned for future)
3. No item descriptions in detail panel (will add with quest/item database)
4. Minimap not implemented in this sprint
5. Colorblind filters are static (can be enhanced with post-processing)
6. No controller-specific button glyphs (uses generic icons)

## Troubleshooting

### Inventory Not Showing
- Ensure `InventoryUIManager` component exists on canvas
- Check that `inventoryCanvasGroup` is assigned
- Verify input action "Inventory" is bound to I key

### HUD Elements Overlapping
- Check Canvas sorting order
- Verify RectTransform anchors are set correctly
- Ensure Canvas scaler settings match reference resolution

### Settings Not Persisting
- Verify `SettingsManager.Instance` is not null
- Check write permissions to `Application.persistentDataPath`
- Ensure JSON serialization doesn't have errors

### Performance Issues
- Check if Canvas.enabled can be toggled instead of destroying
- Verify image atlasing is used for inventory icons
- Profile with Profiler window to find bottlenecks

## References

- Unity UI Toolkit: https://docs.unity3d.com/Manual/UIToolkits.html
- Input System: https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/
- Canvas Scaler: https://docs.unity3d.com/Manual/script-CanvasScaler.html
- Colorblind Accessibility: https://www.color-blindness.com/
