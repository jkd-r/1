# Sprint 5: UI System Setup Guide

## Quick Start: Setting Up the UI System

This guide walks through creating the necessary UI infrastructure for the inventory, HUD, and menu systems.

## Scene Setup

### 1. Create UI Canvas for HUD

1. In your game scene, right-click in Hierarchy → UI → Canvas
2. Name it `HUDCanvas`
3. Set Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
   - Match: Width or Height (0.5)

4. Create the following children:
   ```
   HUDCanvas
   ├── HealthBar (Panel)
   │   ├── Background (Image)
   │   ├── Fill (Image, use green color)
   │   └── Text (Text)
   ├── StaminaBar (Panel)
   │   ├── Background (Image)
   │   ├── Fill (Image, use blue color)
   │   └── Text (Text)
   ├── AmmoCounter (Text)
   ├── Crosshair (Image)
   ├── ObjectiveText (Text)
   ├── InteractionPrompt (Text)
   ├── WeaponIcons (HorizontalLayoutGroup)
   │   ├── MeleeIcon (Image)
   │   └── RangedIcon (Image)
   ├── DamageIndicators
   │   ├── LeftEdge (Image, red)
   │   ├── RightEdge (Image, red)
   │   ├── TopEdge (Image, red)
   │   └── BottomEdge (Image, red)
   ├── VignettePulse (Image, red overlay, alpha 0)
   └── NotificationContainer (VerticalLayoutGroup)
   ```

### 2. Create UI Canvas for Inventory

1. Right-click in Hierarchy → UI → Canvas
2. Name it `InventoryCanvas`
3. Add CanvasGroup component
4. Set layer to UI, interactable to true

5. Create the following structure:
   ```
   InventoryCanvas
   ├── Background (Panel, dark with some transparency)
   ├── GridContainer (Panel with GridLayoutGroup)
   │   └── InventorySlot (Prefab, see below)
   ├── ItemDetailPanel (Panel)
   │   ├── Title (Text)
   │   ├── Icon (Image)
   │   ├── Description (Text)
   │   ├── Stats (Text)
   │   ├── Weight (Text)
   │   └── Type (Text)
   ├── QuickSlotsBar (HorizontalLayoutGroup)
   │   └── QuickSlot (Prefab, x4)
   ├── ControlsPanel
   │   ├── SortDropdown (Dropdown)
   │   ├── FilterDropdown (Dropdown)
   │   └── SearchInput (InputField)
   └── WeightDisplay (Text)
   ```

### 3. Create Inventory Slot Prefab

1. Create an empty GameObject, name it `InventorySlot`
2. Add Image component (set as button)
3. Add Button component
4. Add InventorySlotUI script
5. Create children:
   ```
   InventorySlot
   ├── Icon (Image)
   ├── Quantity (Text)
   └── Highlight (Image, use yellow, alpha 0.5)
   ```

6. Assign in InventorySlotUI:
   - itemIcon: Icon Image
   - quantityText: Quantity Text
   - highlightBorder: Highlight Image

7. Save as `Assets/Resources/Prefabs/UI/InventorySlot.prefab`

### 4. Create Quick Slot Prefab

1. Similar to InventorySlot
2. Add QuickSlotUI script
3. Create children:
   ```
   QuickSlot
   ├── Icon (Image)
   ├── SlotNumber (Text)
   └── Background (Image)
   ```

4. Save as `Assets/Resources/Prefabs/UI/QuickSlot.prefab`

### 5. Create Notification Prefab

1. Create Panel prefab named `Notification`
2. Add CanvasGroup component
3. Add child Text for message
4. Add LayoutElement for sizing
5. Save as `Assets/Resources/Prefabs/UI/Notification.prefab`

### 6. Create Menu Canvases

1. Create three Canvas objects:
   - `PauseMenuCanvas`
   - `MainMenuCanvas`
   - `SettingsMenuCanvas`
   - `ConfirmationDialog`

2. For each, add:
   - CanvasGroup component
   - Set layer to UI
   - Create button hierarchy as needed

### 7. Add Managers to Scene

1. Create an empty GameObject named `UIManagers`
2. Add these components:
   - HUDManager script
   - InventoryUIManager script
   - MenuUIManager script
   - NotificationManager script
   - UIThemeManager script

3. Assign all prefabs and canvases in inspector

### 8. Add Player Systems

1. Create/update Player GameObject with:
   - HealthSystem script
   - Assign health/stamina values

2. Add to GameManager startup sequence

## Prefab Setup Details

### Inventory Slot Setup

```csharp
// In Inspector:
- itemIcon: Reference Icon child Image
- quantityText: Reference Quantity child Text
- highlightBorder: Reference Highlight child Image
- hoverColor: Color.yellow
- normalColor: Color.white
```

### Quick Slot Setup

```csharp
// In Inspector:
- itemIcon: Reference Icon child Image
- slotNumberText: Reference SlotNumber child Text
- slotBackground: Reference Background child Image
- emptyColor: Color.gray
- filledColor: Color.white
```

## Settings UI Setup

### Graphics Tab
```
GraphicsTab (Panel)
├── FOVSlider (Slider + Text)
├── TextureQualitySlider (Slider + Text)
├── ShadowQualitySlider (Slider + Text)
├── EffectsQualitySlider (Slider + Text)
├── MotionBlurToggle (Toggle)
├── DepthOfFieldToggle (Toggle)
└── RayTracingToggle (Toggle)
```

### Audio Tab
```
AudioTab (Panel)
├── MasterVolumeSlider (Slider + Text)
├── MusicVolumeSlider (Slider + Text)
├── SFXVolumeSlider (Slider + Text)
├── VoiceVolumeSlider (Slider + Text)
├── SpatialAudioToggle (Toggle)
└── SubtitlesToggle (Toggle)
```

### Gameplay Tab
```
GameplayTab (Panel)
├── DifficultyDropdown (Dropdown)
├── HUDOpacitySlider (Slider + Text)
├── ObjectiveMarkersToggle (Toggle)
├── CrosshairStyleDropdown (Dropdown)
├── CameraSensitivitySlider (Slider + Text)
├── SprintToggleToggle (Toggle)
└── InvertYAxisToggle (Toggle)
```

### Controls Tab
```
ControlsTab (Panel)
├── SearchInput (InputField)
├── ResetAllButton (Button)
└── KeybindingList (ScrollView)
    └── KeybindingItem (Prefab, dynamic)
```

### Accessibility Tab
```
AccessibilityTab (Panel)
├── ColorblindModeDropdown (Dropdown)
├── HighContrastToggle (Toggle)
├── UIScaleSlider (Slider + Text)
└── FontSizeDropdown (Dropdown)
```

## Keybinding Item Setup

Create `KeybindingItem` prefab:
```
KeybindingItem (Panel)
├── ActionName (Text)
├── CurrentBinding (Text)
├── RebindButton (Button)
└── ResetButton (Button)
```

Add KeybindingItemUI script.

## Theme Setup

1. Create new ScriptableObject (Assets → Create → ProtocolEMR → UI Theme)
2. Name it `DefaultUITheme`
3. Set colors:
   - Primary Accent: #00F0FF (Cyan)
   - Secondary Accent: #FF3CF3 (Magenta)
   - Warning Color: #FF9A3C (Amber)
   - Health Color: #00FF00 (Green)
   - Stamina Color: #00AAFF (Light Blue)
   - Damage Color: #FF0000 (Red)
   - Text Primary: #FFFFFF (White)
   - Text Secondary: #CCCCCC (Light Gray)
   - Background Primary: #05070B (Very Dark Blue)
   - Background Secondary: #101621 (Dark Blue-Gray)

4. Duplicate for colorblind modes:
   - ProtanopiaUITheme
   - DeuteranopiaUITheme
   - TritanopiaUITheme

5. Adjust colors in themes according to protocol

6. Assign in UIThemeManager

## Input Setup

Verify in `PlayerInputActions.inputactions`:
```
UI Action Map:
├── Inventory (Key: I)
├── Pause (Key: Escape)
├── Phone (Key: C)
└── General actions

Movement Action Map:
├── Sprint (Key: LeftShift)
├── Crouch (Key: LeftCtrl)
└── Jump (Key: Space)
```

## Inventory Manager Setup

1. Create empty GameObject named `InventorySystem`
2. Add InventoryManager component
3. Set:
   - inventorySlotCount: 20
   - maxCarryWeight: 50

## Health System Setup

1. Add HealthSystem component to Player
2. Set:
   - maxHealth: 100
   - maxStamina: 100
   - staminaRegenRate: 15
   - staminaRegenDelay: 1
   - damageFlashDuration: 0.5

## Testing the Setup

### Test 1: HUD Display
```csharp
var health = GetComponent<HealthSystem>();
health.TakeDamage(25);
// Verify health bar updates, damage flash shows
```

### Test 2: Inventory
```csharp
// Add test items
var item = new ItemData
{
    itemName = "Test Item",
    itemType = ItemType.Consumable,
    weight = 0.5f,
    quantity = 1
};
InventoryManager.Instance.AddItem(item);
// Press I key, verify inventory opens
```

### Test 3: Menus
```csharp
// Press ESC to open pause menu
// Verify menu appears and game pauses
// Check all buttons are clickable
```

### Test 4: Settings
```csharp
// Open Settings from pause menu
// Adjust FOV slider, verify change in real-time
// Check HUD opacity affects display
// Verify changes persist after close
```

## Common Issues & Fixes

### Canvas Not Visible
- Check Canvas component is enabled
- Verify Graphic Raycaster is attached
- Check RectTransform is sized correctly
- Verify layer is set to UI

### Buttons Not Clickable
- Add EventSystem to scene if missing
- Check GraphicRaycaster on Canvas
- Verify button transition is set correctly
- Check nothing is blocking raycasts

### Text Appears Blurry
- Disable "Best Fit" on Text component
- Set Font Size to 16 or higher
- Use Arial or Roboto font
- Check Canvas scale mode

### Prefabs Not Loading
- Verify they're in Assets/Resources/Prefabs/UI/
- Check serialization in InventoryUIManager
- Log errors if null references occur

## Performance Tips

1. Use UI Toolkit for complex layouts
2. Enable "Raycast Target" only on interactive elements
3. Batch UI updates, don't change every frame
4. Use ScrollRect with object pooling for large lists
5. Disable Canvas rendering when not visible
6. Profile with Profiler to find bottlenecks

## Next Steps

1. Create icon assets for items (16x16 or 32x32 PNG)
2. Design color scheme matching sci-fi aesthetic
3. Create sound effects for UI interactions
4. Implement item database with item definitions
5. Add craftable item tracking system
6. Implement equipment comparison overlay
7. Create achievement notification effects

## File Checklist

Required scripts:
- [ ] ItemData.cs
- [ ] InventoryManager.cs
- [ ] HUDManager.cs
- [ ] InventoryUIManager.cs
- [ ] InventorySlotUI.cs
- [ ] QuickSlotUI.cs
- [ ] MenuUIManager.cs
- [ ] SettingsPanelUI.cs
- [ ] KeybindingItemUI.cs
- [ ] UIThemeManager.cs
- [ ] NotificationManager.cs
- [ ] HealthSystem.cs

Required Prefabs:
- [ ] InventorySlot.prefab
- [ ] QuickSlot.prefab
- [ ] Notification.prefab
- [ ] KeybindingItem.prefab

Required ScriptableObjects:
- [ ] DefaultUITheme.asset
- [ ] ProtanopiaUITheme.asset
- [ ] DeuteranopiaUITheme.asset
- [ ] TritanopiaUITheme.asset

Required Scene Setup:
- [ ] HUD Canvas
- [ ] Inventory Canvas
- [ ] Menu Canvases (Pause, Main, Settings)
- [ ] UI Managers GameObject
- [ ] Player with HealthSystem
