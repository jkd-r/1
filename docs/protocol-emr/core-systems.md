# Protocol EMR: Core Systems Documentation

This document defines the technical architecture for the core gameplay systems in Protocol EMR, including movement and camera control, HUD rendering, inventory management, interactable object systems, and the virtual phone interface.

---

## 1. First-Person Controller System

The first-person controller provides the foundation for player movement, camera control, and input processing.

### 1.1 Responsibilities

- **Movement Processing**: Handle player locomotion in 3D space (forward, backward, strafing, jumping, sprinting)
- **Camera Control**: Manage first-person viewpoint with head bob, tilt, and smooth tracking
- **Input Stack**: Unify keyboard, mouse, and gamepad input with rebindable controls
- **Collision Detection**: Integrate with physics system for ground detection and obstacle avoidance
- **State Management**: Track movement states (idle, walking, sprinting, jumping, falling) and transitions
- **Audio Cues**: Trigger footstep and movement sounds based on surface materials

### 1.2 Data Structures

```
PlayerController {
  Transform position
  Vector3 velocity
  Vector3 inputDirection
  float moveSpeed
  float sprintSpeed
  float jumpForce
  float groundDrag
  float airDrag
  bool isGrounded
  MovementState currentState
  
  Camera firstPersonCamera
  Transform cameraAnchor
  Vector2 lookInput
  float mouseSensitivity
  float lookSmoothing
  Vector2 viewBob
  float bobAmplitude
  float bobFrequency
}

MovementState = { IDLE, WALKING, SPRINTING, JUMPING, FALLING, LANDING }

InputBindings {
  Key moveForward
  Key moveBackward
  Key moveLeft
  Key moveRight
  Key sprint
  Key jump
  float mouseX
  float mouseY
  bool allowRebind
}
```

### 1.3 Engine Features & Plugins

- **Physics Engine**: Rigidbody with capsule collider for player collision
- **Input System**: Modern Input Manager (or equivalent) for unified input handling
- **Animation System**: Animator controller for movement state transitions and blending
- **Raycast System**: Ground detection via raycasts for jumping/landing
- **Audio System**: Audio source and mixer for footsteps and movement feedback

### 1.4 Integration Points

- **Inventory System**: Restrict movement/sprinting based on weight or encumbrance
- **Interactable Objects**: Trigger nearby object detection and highlight systems
- **HUD System**: Feed movement and health state to HUD indicators
- **Animation System**: Drive character model animations based on controller state
- **Physics System**: Respond to force applications (explosions, knockback)

### 1.5 Acceptance Criteria - Functional Prototype

✓ Player can move in all cardinal directions with smooth acceleration/deceleration
✓ Camera follows mouse/gamepad input with configurable sensitivity and smoothing
✓ Sprinting increases movement speed by 1.5x when stamina available
✓ Jump mechanic works with proper gravity and apex hang time
✓ Head bob is subtle and matches movement rhythm (off when idle)
✓ Footstep sounds trigger on surface contact with material-specific audio
✓ Input rebinding interface allows key reassignment persistence
✓ Smooth transitions between movement states with no clipping
✓ Works with keyboard/mouse and full gamepad support

---

## 2. HUD Styling System

The HUD provides real-time player status information and interaction feedback with a modern, minimalist aesthetic emphasizing clarity and visual polish.

### 2.1 Responsibilities

- **Status Display**: Render health, stamina, ammunition, and other vital stats
- **Visual Feedback**: Highlight interactive elements with outline shaders and glow effects
- **Reticle & Aiming**: Display aiming reticle with spread/bloom indicators
- **Damage Indicators**: Show directional damage with screen-edge visual cues
- **Contextual Hints**: Display interaction prompts near interactable objects
- **Minimap/Radar**: Render tactical information about environment and enemies
- **Post-Processing**: Apply glow, bloom, and outline shader effects

### 2.2 Data Structures

```
HUDManager {
  Canvas mainHUDCanvas
  RectTransform statusPanel
  RectTransform reticlePanel
  RectTransform damageIndicators[]
  
  StatusDisplay {
    Image healthBar
    Text healthValue
    Image staminaBar
    Text ammoCounter
    Image[] statusIcons
  }
  
  ReticleDisplay {
    RectTransform reticle
    float spreadRadius
    float maxSpread
    Vector2[] damageIndicatorPositions
  }
  
  OutlineShaderSettings {
    Color outlineColor
    float outlineWidth
    float glowIntensity
    AnimationCurve pulseCurve
    float fadeInDuration
    float fadeOutDuration
  }
  
  HUDTheme {
    Color accentColor
    Color warningColor
    Color criticalColor
    Material glowMaterial
    Material outlineMaterial
  }
}

HUDElement {
  RectTransform rectTransform
  CanvasGroup canvasGroup
  OutlineRenderer outlineRenderer
  bool isHighlighted
  float highlightIntensity
}
```

### 2.3 Engine Features & Plugins

- **Canvas/UI System**: Canvas, RectTransform, and CanvasGroup for UI layout
- **Shader System**: Custom outline and glow shaders applied via material overrides
- **Post-Processing**: Post-process volumes for screen-space effects (bloom, glow)
- **Animation System**: Animator for HUD element transitions and pulsing effects
- **TextMesh Pro**: Advanced text rendering for status displays
- **Sprite Atlas**: Optimized sprite rendering for icons and reticles
- **Camera Stacking**: Separate camera for HUD rendering at UI layer

### 2.4 Integration Points

- **Player Controller**: Subscribe to health, stamina, and input state
- **Weapon System**: Display ammunition and firing spread data
- **Inventory System**: Show selected item status and quick-slot contents
- **Damage System**: Receive damage direction and magnitude for indicators
- **Interactable Objects**: Display interaction hints and prompts
- **Mission System**: Show objectives and mission progress
- **Audio System**: Play UI confirmation and warning sounds

### 2.5 Acceptance Criteria - Functional Prototype

✓ Health bar smoothly animates with critical state color coding
✓ Stamina display decreases during sprinting and recovers when idle
✓ Reticle spreads realistically based on movement and weapon state
✓ Outline shader highlights nearby interactable objects with distinct color
✓ Glow/bloom effect creates polished visual hierarchy without performance cost
✓ Damage indicators appear at screen edges with directional arrow animation
✓ Status icons (poison, stun, buffs) appear/disappear with fade effects
✓ All HUD elements scale correctly for 16:9, 21:9, and 4:3 aspect ratios
✓ HUD remains visible and legible during all gameplay scenarios
✓ Theme colors can be swapped via configuration without shader recompilation

---

## 3. Inventory System

The inventory system manages player item collection, storage, and usage with intuitive flows and minimal UI friction.

### 3.1 Responsibilities

- **Item Collection**: Handle pickup mechanics and add/remove items from inventory
- **Storage Management**: Organize items by type with weight/capacity constraints
- **Quick Access**: Provide quick-slot system for frequently used items
- **Item Usage**: Manage consumption and equipment state changes
- **Notifications**: Display mini-notifications on item pickup and status changes
- **Detailed UI**: Render comprehensive inventory screen with filtering and sorting
- **Persistence**: Save/load inventory state with data serialization
- **Drop Mechanics**: Allow item dropping with physics-based world placement

### 3.2 Data Structures

```
InventoryManager {
  List<InventorySlot> inventorySlots
  int maxSlots
  float maxWeight
  float currentWeight
  EquipmentSlot[] equipmentSlots
  QuickSlot[] quickSlots
  
  Dictionary<ItemType, int> itemCounts
  Stack<PickupNotification> recentPickups
}

InventorySlot {
  ItemData itemData
  int quantity
  Vector2 gridPosition
  bool isEquipped
  float durability
  Dictionary<string, object> properties
}

ItemData {
  int itemID
  string itemName
  ItemType type
  float weight
  Sprite icon
  string description
  bool isStackable
  int maxStack
  float sellValue
  ItemRarity rarity
  AudioClip pickupSound
}

QuickSlot {
  InventorySlot linkedSlot
  KeyCode bindKey
  RectTransform uiElement
  float cooldown
}

PickupNotification {
  ItemData item
  int quantity
  Vector3 worldPosition
  float duration
  float fadeInDuration
  float fadeOutDuration
}

CollectionFlow {
  ItemData targetItem
  bool canStackWithExisting
  QuickSlot assignedSlot
  NotificationTrigger notificationType
}
```

### 3.3 Engine Features & Plugins

- **Prefab System**: Item prefabs with serialized ItemData and pickup triggers
- **Physics Engine**: Rigidbody for dropped items with kinematic control
- **UI System**: Canvas and layout groups for inventory grid display
- **TextMesh Pro**: Item names, quantities, and descriptions in inventory UI
- **Animation System**: Slide and pop animations for pickup notifications
- **Scriptable Objects**: ItemData stored as scriptable objects for easy editing
- **Save System**: JSON serialization or native save system for inventory persistence
- **Particle Effects**: Visual feedback on item pickup (sparkles, light bursts)

### 3.4 Integration Points

- **Player Controller**: Restrict movement if encumbered by weight
- **HUD System**: Display quick-slot contents and selected item status
- **Interactable Objects**: Detect and collect items from pickup zones
- **Equipment System**: Equip/unequip items and apply stat bonuses
- **Mission System**: Track item collection for quest completion
- **Audio System**: Play pickup sounds and inventory UI feedback sounds
- **Pause Menu**: Integrate inventory screen into pause menu
- **Network System**: Synchronize inventory state for multiplayer scenarios

### 3.5 Acceptance Criteria - Functional Prototype

✓ Items can be picked up and added to inventory with automatic stacking
✓ Inventory displays grid-based layout with item icons and quantities
✓ Weight system prevents overencumbrance, restricting sprint and movement
✓ Quick-slots (1-5 keys) provide rapid access to selected items
✓ Mini-notification pops up on pickup with item icon, name, and quantity
✓ Notifications fade in/out smoothly without blocking gameplay
✓ Inventory UI allows item sorting by type, rarity, and weight
✓ Items can be dropped with physics and collected by other players (if multiplayer)
✓ Right-click/alternate action allows item usage (consume, equip, sell)
✓ Inventory state persists across level loads and game sessions

---

## 4. Interactable Object System

The interactable object system defines rules for how players interact with environmental objects and NPCs.

### 4.1 Responsibilities

- **Object Registration**: Track all interactable objects in the scene
- **Detection & Highlighting**: Detect proximity and highlight when in range
- **Interaction Triggers**: Execute appropriate actions on player input
- **State Management**: Track object state (locked, used, destroyed, recycled)
- **Audio & Visual Feedback**: Provide feedback on interaction attempt
- **Usage Rules**: Enforce interaction constraints (one-time use, cooldowns, permissions)
- **Data Synchronization**: Update object state across networked clients
- **Dynamic Objects**: Support doors, switches, terminals, and environmental puzzles

### 4.2 Data Structures

```
InteractableObject : MonoBehaviour {
  InteractableType type
  string objectID
  string displayName
  Collider interactionRadius
  bool isLocked
  bool isUsed
  bool canRepeat
  float interactionCooldown
  float timeUntilAvailable
  
  InteractionPrompt prompt
  List<InteractionAction> actions
  InteractableState currentState
  
  AudioClip onInteractSound
  Particle[] interactionEffects
  Animation interactionAnimation
}

InteractableType = { DOOR, CHEST, SWITCH, TERMINAL, NPC, HAZARD, COLLECTIBLE, PUZZLE }

InteractableState {
  enum Status { AVAILABLE, LOCKED, USED, DISABLED, COOLING_DOWN }
  Status currentStatus
  float usageCount
  DateTime lastUsedTime
  string ownerID
}

InteractionAction {
  ActionType actionType
  string actionName
  Delegate actionCallback
  bool requiresLineOfSight
  bool showProgressBar
  float duration
  Condition[] prerequisites
}

ActionType = { OPEN, TAKE, USE, UNLOCK, TALK, ACTIVATE, COMBINE }

InteractionPrompt {
  string promptText
  KeyCode interactKey
  RectTransform uiAnchor
  Sprite keyIcon
  bool showDistance
}

Condition {
  enum ConditionType { HAS_ITEM, HAS_SKILL, QUEST_ACTIVE, TIME_PASSED, STAT_REQUIREMENT }
  ConditionType type
  object targetValue
  bool isMet()
}
```

### 4.3 Engine Features & Plugins

- **Collider System**: Capsule/sphere colliders for interaction ranges
- **Raycasting**: Line-of-sight checks for visibility requirements
- **UI System**: Canvas for interaction prompts above objects
- **Animation System**: Animator for interaction sequences and state transitions
- **Audio System**: Audio sources for interaction feedback
- **Physics System**: Trigger zones for automatic detection
- **Coroutines**: Timing and progression for multi-step interactions
- **Event System**: Custom event broadcasting for state changes

### 4.4 Integration Points

- **Player Controller**: Proximity detection and interaction key input
- **HUD System**: Display interaction prompts and progress bars
- **Inventory System**: Item collection and requirement verification
- **Quest/Mission System**: Track interaction completions for objectives
- **NPC System**: Interact with NPCs for dialogue and transactions
- **Audio System**: Provide audio feedback on successful/failed interactions
- **Damage System**: Some objects may be destructible via damage
- **Network System**: Synchronize state for multiplayer scenarios

### 4.5 Acceptance Criteria - Functional Prototype

✓ Player can approach interactable object and see interaction prompt appear
✓ Prompt displays appropriate action name (Open, Take, Use, Unlock, etc.)
✓ Pressing interaction key executes the action with appropriate feedback
✓ Locked objects reject interaction attempts with visual/audio cue
✓ One-time-use objects cannot be interacted with after first use
✓ Objects with cooldowns show time remaining until available
✓ Line-of-sight checks prevent interaction through walls
✓ Progress bar displays for duration-based interactions (locks, puzzles)
✓ Multiple interaction options available on complex objects
✓ State changes persist and are synchronized across networked sessions

---

## 5. Virtual Phone System

The virtual phone provides an immersive in-game device for UI interactions, communications, and information management.

### 5.1 Responsibilities

- **UI Layout**: Display phone screen with app icons, notifications, and information
- **App Management**: Provide framework for multiple phone applications
- **Toggle Behavior**: Show/hide phone with animation and gameplay pause/unpause
- **Data Binding**: Synchronize phone data with game state (messages, quests, inventory)
- **Notifications**: Display incoming calls, messages, and system alerts
- **Navigation**: Implement hierarchical menu navigation and app transitions
- **Accessibility**: Provide voice-command and contextual shortcuts for accessibility
- **Realism**: Model battery, signal strength, and realistic phone UX paradigms

### 5.2 Data Structures

```
VirtualPhone : MonoBehaviour {
  bool isActive
  float toggleAnimationDuration
  CanvasGroup phoneCanvasGroup
  Image phoneScreenDisplay
  Rect phoneScreenArea
  
  PhoneOS phoneOS
  List<PhoneApp> installedApps
  PhoneApp currentApp
  
  PhoneSettings {
    float brightness
    bool vibrationEnabled
    float volume
    string language
    float batteryLevel
    float signalStrength
  }
  
  NotificationCenter notificationCenter
  MessageHistory messageHistory
}

PhoneOS {
  Canvas mainCanvas
  RectTransform homeScreen
  List<AppIcon> appIcons
  NotificationBanner notificationBanner
  StatusBar statusBar
}

PhoneApp {
  string appName
  Sprite appIcon
  int appID
  Canvas appCanvas
  List<Screen> screens
  Screen currentScreen
  bool isOpen
  AppState appState
}

Screen {
  string screenName
  RectTransform screenLayout
  List<UIElement> elements
  Button[] navigationButtons
  Screen previousScreen
  
  void OnOpen()
  void OnClose()
  void Refresh()
}

PhoneAppTypes {
  // Messages App
  MessagesApp : PhoneApp {
    List<Conversation> conversations
    Conversation selectedConversation
  }
  
  // Map App
  MapApp : PhoneApp {
    MapData currentMap
    Vector2 playerMarker
    List<POI> pointsOfInterest
  }
  
  // Quest Log App
  QuestApp : PhoneApp {
    List<Quest> activeQuests
    Quest selectedQuest
    List<QuestObjective> objectives
  }
  
  // Inventory App
  InventoryApp : PhoneApp {
    InventoryManager linkedInventory
    List<ItemCategory> categories
  }
  
  // Settings App
  SettingsApp : PhoneApp {
    AudioSettings audioSettings
    DisplaySettings displaySettings
    ControlSettings controlSettings
  }
}

Conversation {
  string conversationID
  string contactName
  Sprite contactAvatar
  List<Message> messages
  DateTime lastMessageTime
  bool isUnread
  
  Message {
    string senderName
    string messageText
    DateTime timestamp
    bool isFromPlayer
    Sprite? attachedImage
  }
}

DataBinding {
  object dataSource
  PropertyInfo sourceProperty
  UIElement targetElement
  UnityAction<object> updateCallback
  
  void BindData(object source, string propertyPath, UIElement target)
  void UnbindData()
}

ToggleState {
  enum State { CLOSED, OPENING, OPEN, CLOSING }
  State currentState
  float animationProgress
}
```

### 5.3 Engine Features & Plugins

- **Canvas System**: Hierarchical canvas for phone UI rendering
- **UI Layout Groups**: Layout elements for responsive app screens
- **Animation System**: Slide, fade, and scale animations for phone toggle
- **Text Rendering**: TextMesh Pro for high-quality text in phone apps
- **Sprite Atlasing**: Optimized app icons and UI graphics
- **Data Binding**: Custom binding system linking game state to phone UI
- **Input System**: Swipe/touch input simulation for phone navigation
- **Prefab System**: App templates for rapid phone app development
- **Screen Capture**: Optional screenshot system for photo app functionality
- **Serialization**: JSON or native serialization for message and data persistence

### 5.4 Integration Points

- **Player Controller**: Toggle phone with pause/unpause game state
- **HUD System**: Show/hide main HUD when phone is active
- **Mission/Quest System**: Display active quests and objectives in Quest App
- **Inventory System**: Show inventory contents and item details in Inventory App
- **NPC/Dialogue System**: Display conversations in Messages App
- **World State**: Integrate Map App with fog of war and discoverable locations
- **Pause Menu**: Access phone from pause menu
- **Audio System**: Play notification sounds and app UI audio
- **Network System**: Synchronize messages and state for multiplayer

### 5.5 Acceptance Criteria - Functional Prototype

✓ Phone toggle (key/button) smoothly animates in/out with zoom and fade
✓ Phone screen displays home screen with visible app icons
✓ App icons are interactive (tap to open app)
✓ Messages App shows conversation list with last message preview
✓ Opening conversation shows full message history with timestamps
✓ Sending message updates conversation and appears with sender indicator
✓ Quest App displays active quests with objectives and progress tracking
✓ Map App shows current player location and discovered POIs
✓ Inventory App mirrors current inventory with item details on tap
✓ Settings App allows brightness, volume, and control customization with persistence
✓ Notifications appear as banner alerts without blocking gameplay
✓ Phone battery depletes over time with low battery warning
✓ All data bindings automatically update when underlying game state changes
✓ Phone respects game pause state and game resumes when phone closes
✓ Navigation back button properly dismisses apps and returns to home screen

---

## Integration Architecture

### System Interconnections

```
Player Controller
  ├─→ Movement feeds to HUD stamina display
  ├─→ Proximity detection triggers Interactable System
  ├─→ Input routed to Phone toggle and Inventory access
  └─→ Collision detection affects Inventory weight constraints

HUD System
  ├─← Receives status updates from Player Controller
  ├─← Receives item pickup notifications from Inventory
  ├─← Receives interaction prompts from Interactable Objects
  ├─← Displays damage indicators from Damage System
  └─← Synchronized with Phone when active/inactive

Inventory System
  ├─← Item pickups triggered by Interactable Objects
  ├─← Equipment affects Player Controller speed and animation
  ├─→ Quick-slots displayed in HUD
  ├─→ Mirrored in Phone Inventory App
  └─← Weight constraints affect Player Controller sprint/movement

Interactable Objects
  ├─← Proximity detection via Player Controller
  ├─→ Interaction prompts displayed by HUD System
  ├─← Interaction actions may trigger Inventory collection
  ├─← State synchronized in networked environments
  └─→ Completion tracked by Mission System

Virtual Phone
  ├─← Toggles game pause state via Player Controller
  ├─← Displays HUD System visibility
  ├─← Shows Inventory System state
  ├─← Displays Mission/Quest state
  ├─← Receives NPC messages from Dialogue System
  └─← Maps world state and player position
```

### Data Flow Example: Item Pickup

1. Player approaches Interactable Object
2. Interactable System detects proximity, displays interaction prompt via HUD
3. Player presses interaction key, Interactable Object triggers pickup action
4. Item added to Inventory System with notification triggered
5. Notification displayed as mini-popup via HUD System
6. Quick-slot automatically assigned if available
7. Weight updated, affecting Player Controller movement speed if encumbered
8. Inventory App in Phone automatically reflects new item
9. If quest-related, Mission System receives completion update

---

## Implementation Priorities

### Phase 1 (Core Functionality)
1. Player Controller with basic movement and camera
2. Interactable Object System with simple pickup mechanics
3. Inventory System with grid-based storage
4. Basic HUD with health/stamina bars

### Phase 2 (Polish & Features)
1. Virtual Phone with multiple apps
2. Advanced HUD styling with outline shaders and glow effects
3. Inventory quick-slots and equipment system
4. Comprehensive interactable object state management

### Phase 3 (Integration & Content)
1. Full data binding between systems
2. Networked multiplayer synchronization
3. Mission/Quest integration
4. Content-specific interactables (doors, terminals, NPCs)

---

## Performance Considerations

- **Pooling**: Reuse objects for notifications, damage indicators, and UI elements
- **Occlusion**: Disable rendering for off-screen interactable highlights
- **LOD System**: Use Level of Detail for phone screen rendering at distance
- **Shader Optimization**: Cache outline/glow shader material instances
- **Coroutine Efficiency**: Batch UI updates rather than per-frame changes
- **Memory**: Pre-allocate inventory slots and notification queues

---

## Conclusion

These core systems form the foundation for Protocol EMR gameplay. Each system is designed with clear responsibilities, maintainable data structures, and integration points that allow for seamless interaction. The modular architecture enables parallel development and iterative refinement of each subsystem while maintaining cohesive player experience.
