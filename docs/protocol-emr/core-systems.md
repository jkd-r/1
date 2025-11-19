# Core Systems Architecture (v2)

This document outlines the technical architecture and implementation specifications for Protocol EMR's fundamental gameplay systems. Each system is designed with clear responsibilities, defined data structures, and established integration patterns.

## Table of Contents

1. [First-Person Controller](#first-person-controller)
2. [HUD & Visual Style](#hud--visual-style)
3. [Inventory System](#inventory-system)
4. [Interactable Objects](#interactable-objects)
5. [Virtual Phone System](#virtual-phone-system)
6. [Integration Architecture](#integration-architecture)
7. [Performance Requirements](#performance-requirements)
8. [QA Checklist](#qa-checklist)

## First-Person Controller

### Core Responsibilities

The First-Person Controller (FPC) provides responsive player movement and interaction capabilities within the research facility environment.

- **Movement Mechanics**: Smooth, physics-based movement with appropriate acceleration/deceleration curves
- **Camera Control**: Precise mouse look with configurable sensitivity and smooth interpolation
- **Input Handling**: Comprehensive input mapping for WASD movement, mouse look, and interaction keys
- **State Management**: Sprint/crouch mechanics with appropriate speed multipliers and collision adjustments
- **Interaction Detection**: Raycast-based object interaction with visual feedback and range validation

### Data Structures

```csharp
public class PlayerController
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float sprintSpeed = 6.0f;
    public float crouchSpeed = 1.5f;
    public float acceleration = 10.0f;
    public float friction = 8.0f;
    
    [Header("Camera Settings")]
    public float mouseSensitivity = 2.0f;
    public float maxLookAngle = 85.0f;
    public float smoothTime = 0.1f;
    
    [Header("Interaction Settings")]
    public float interactionRange = 2.5f;
    public LayerMask interactionLayers;
    public KeyCode interactionKey = KeyCode.E;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    
    [Header("Audio")]
    public AudioClip footstepSound;
    public AudioClip interactionSound;
    
    public PlayerState state { get; private set; }
}

public enum PlayerState
{
    Standing,
    Walking,
    Sprinting,
    Crouching,
    Interacting
}

public struct MovementInput
{
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool sprintPressed;
    public bool crouchPressed;
    public bool interactPressed;
}
```

### Engine Features Required

- **Unity CharacterController**: Built-in character controller component for movement and collision
- **Unity Input System**: New Input System for flexible input mapping and platform support
- **PhysX Physics**: Physics engine for movement, collision detection, and interaction validation
- **Unity Cinemachine**: Camera management for smooth look behavior and potential cutscenes

### Integration Points

- **Interactable Objects**: Raycast detection and interaction triggering
- **HUD System**: State-based UI updates (sprint bar, crouch indicator, interaction prompts)
- **Audio System**: Footstep sounds based on movement speed and surface type
- **Save System**: Player position, rotation, and state persistence

### Acceptance Criteria

**GIVEN** the player has spawned in the facility
**WHEN** they press WASD keys
**THEN** the character moves smoothly in the corresponding direction with appropriate acceleration

**GIVEN** the player is moving
**WHEN** they hold the sprint key
**THEN** movement speed increases by 100% with appropriate audio feedback

**GIVEN** the player is standing
**WHEN** they press the crouch key
**THEN** the character height reduces by 50% and movement speed decreases by 75%

**GIVEN** an interactable object is within range
**WHEN** the player looks at it
**THEN** an interaction prompt appears and the object highlights

## HUD & Visual Style

### Core Responsibilities

The HUD system provides essential player information while maintaining a clean, minimalist aesthetic that supports the near-future sci-fi setting.

- **Health & Status Display**: Visual indicators for player health and critical status conditions
- **Interaction Feedback**: Real-time prompts and visual feedback for object interactions
- **Navigation Aid**: Minimap or compass system for environmental orientation
- **Minimalist Design**: Clean, unobtrusive interface that doesn't break immersion
- **Visual Effects**: Glow/bloom effects and outline shaders for interactive elements

### Data Structures

```csharp
public class HUDController
{
    [Header("Display Elements")]
    public GameObject healthBar;
    public GameObject interactionPrompt;
    public GameObject minimapDisplay;
    public GameObject statusPanel;
    
    [Header("Visual Settings")]
    public float glowIntensity = 1.5f;
    public Color highlightColor = Color.cyan;
    public float outlineWidth = 2.0f;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;
    public float pulseFrequency = 2.0f;
}

public struct InteractionData
{
    public string objectName;
    public string actionText;
    public Vector3 screenPosition;
    public bool isInRange;
    public float displayDuration;
}

public enum HUDState
{
    Default,
    Interacting,
    Warning,
    Dialog
}
```

### Engine Features Required

- **Unity UI Toolkit**: Modern UI system for clean, performant interface elements
- **Unity Post-Processing Stack**: Visual effects for bloom, glow, and screen-space effects
- **Custom Shaders**: Outline shaders for interactive object highlighting
- **Canvas Components**: World-space and screen-space canvases for different UI elements
- **Animation System**: Smooth transitions and state-based animations

### Integration Points

- **Player Controller**: State-based HUD updates and interaction prompts
- **Interactable Objects**: Highlight system and interaction feedback
- **Health System**: Real-time health bar updates and status indicators
- **Audio System**: Visual feedback synchronized with audio cues
- **Phone System**: HUD state management during phone interactions

### Acceptance Criteria

**GIVEN** the player health is at 100%
**WHEN** the game starts
**THEN** the health bar displays full health with appropriate visual styling

**GIVEN** an interactable object is in view
**WHEN** the player looks at it
**THEN** the object displays a subtle glow effect and outline shader

**GIVEN** the player takes damage
**WHEN** health decreases
**THEN** the health bar animates smoothly and may flash red for critical damage

**GIVEN** the player is in a large area
**WHEN** they open the minimap
**THEN** a top-down view shows their position and nearby interactive objects

## Inventory System

### Core Responsibilities

The inventory system manages object collection, storage, and usage with intuitive visual feedback and organization.

- **Object Collection**: Seamless pickup flow with automatic inventory management
- **Visual Feedback**: Mini-notifications for item pickups (2-3 seconds duration)
- **Detailed UI Panel**: Comprehensive inventory interface with item descriptions and categorization
- **Item Management**: Sorting, filtering, and organization capabilities
- **Equipment System**: Support for equipped items with active effects

### Data Structures

```json
{
  "InventorySchema": {
    "maxSlots": 24,
    "stackSizes": {
      "key_items": 1,
      "consumables": 10,
      "materials": 50,
      "equipment": 1
    },
    "categories": [
      "key_items",
      "consumables",
      "materials",
      "equipment",
      "documents"
    ]
  },
  "ItemTemplate": {
    "id": "string",
    "name": "string",
    "description": "string",
    "category": "string",
    "stackSize": "number",
    "icon": "texture_path",
    "model": "prefab_path",
    "effects": [],
    "value": "number",
    "rarity": "common|uncommon|rare|legendary"
  }
}
```

```csharp
public class InventoryManager
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform inventoryGrid;
    public GameObject itemSlotPrefab;
    public GameObject pickupNotificationPrefab;
    
    [Header("Settings")]
    public float notificationDuration = 2.5f;
    public float animationSpeed = 0.3f;
    public KeyCode inventoryKey = KeyCode.Tab;
    
    public List<InventorySlot> slots { get; private set; }
    public bool isInventoryOpen { get; private set; }
}

public class InventorySlot
{
    public ItemData item { get; private set; }
    public int quantity { get; private set; }
    public bool isEmpty { get => item == null; }
    
    public bool AddItem(ItemData newItem, int amount);
    public bool RemoveItem(int amount);
    public void ClearSlot();
}
```

### Engine Features Required

- **Unity UI Toolkit**: Grid layout system for inventory slots
- **Scriptable Objects**: Item data templates and inventory configurations
- **Event System**: Input handling for inventory interactions
- **Animation System**: Smooth transitions and pickup animations
- **Prefab System**: Reusable item slot and notification components

### Integration Points

- **Player Controller**: Pickup detection and inventory access
- **Interactable Objects**: Item creation and transfer to inventory
- **HUD System**: Pickup notifications and inventory status indicators
- **Save System**: Inventory state persistence and loading
- **Audio System**: Pickup sounds and inventory interaction feedback

### Acceptance Criteria

**GIVEN** an item is on the ground
**WHEN** the player interacts with it
**THEN** the item disappears and a notification appears for 2-3 seconds

**GIVEN** the player presses the inventory key
**WHEN** the inventory panel opens
**THEN** all collected items are displayed in an organized grid with descriptions

**GIVEN** the inventory is full
**WHEN** attempting to pick up another item
**THEN** a "inventory full" message appears and the item remains on the ground

**GIVEN** an item is selected in inventory
**WHEN** the player clicks "use" or "equip"
**THEN** the appropriate action is performed and the item quantity updates

## Interactable Objects

### Core Responsibilities

The interactable objects system provides a framework for environmental interaction without requiring complex AI analysis, using predefined properties and clear feedback mechanisms.

- **Interaction Rules**: Simple, consistent interaction patterns based on object type
- **Predefined Properties**: Configurable object behaviors without runtime AI analysis
- **Visual Feedback**: Clear indication of interaction availability and results
- **Mechanic Support**: Pick-up, examine, and use mechanics with appropriate responses
- **State Management**: Object state tracking for puzzle progression and narrative events

### Data Structures

```json
{
  "InteractableObjectSchema": {
    "id": "string",
    "name": "string",
    "type": "pickup|examine|use|static",
    "description": "string",
    "interactionRange": 2.5,
    "highlightColor": "#00FFFF",
    "audioFeedback": "sound_path",
    "visualEffects": [],
    "requiredItems": [],
    "rewardItems": [],
    "stateChanges": {},
    "puzzleComponent": false
  }
}
```

```csharp
public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public InteractionType type;
    public float interactionRange = 2.5f;
    public bool requiresItem = false;
    public ItemData requiredItem;
    
    [Header("Visual Feedback")]
    public Material highlightMaterial;
    public GameObject interactionIcon;
    public Vector3 iconOffset = Vector3.up;
    
    [Header("Audio")]
    public AudioClip interactionSound;
    public AudioClip failureSound;
    
    [Header("Rewards")]
    public ItemData[] rewardItems;
    public int rewardQuantity = 1;
    
    public bool CanInteract(PlayerController player);
    public void Interact(PlayerController player);
    public void Highlight(bool active);
}

public enum InteractionType
{
    Pickup,
    Examine,
    Use,
    Static,
    Puzzle
}
```

### Engine Features Required

- **Unity Collider System**: Trigger and collision detection for interaction range
- **Unity Renderer System**: Material swapping for highlight effects
- **Unity Audio System**: Interaction feedback and ambient sounds
- **Scriptable Objects**: Object configuration templates and data storage
- **Event System**: Interaction events and state change notifications

### Integration Points

- **Player Controller**: Raycast detection and interaction triggering
- **HUD System**: Interaction prompts and visual highlighting
- **Inventory System**: Item rewards and requirement validation
- **Audio System**: Interaction-specific sound effects
- **Puzzle System**: State changes for puzzle progression

### Acceptance Criteria

**GIVEN** an interactable object is in the scene
**WHEN** the player enters interaction range
**THEN** the object highlights and displays an interaction prompt

**GIVEN** a pickup item is interacted with
**WHEN** the player presses the interaction key
**THEN** the item is added to inventory and removed from the scene

**GIVEN** an examine object is interacted with
**WHEN** the player interacts with it
**THEN** a description appears and the object plays an examination animation

**GIVEN** a use object requires a specific item
**WHEN** the player interacts without the required item
**THEN** a "required item" message appears and the interaction fails

## Virtual Phone System

### Core Responsibilities

The virtual phone system serves as the primary interface for AI narrator communication and information delivery, providing a seamless narrative integration mechanism.

- **UI Layout Design**: Clean, modern interface inspired by contemporary smartphone design
- **Toggle Functionality**: Quick access via 'C' key for immediate communication
- **Chat Interface**: Real-time messaging system for AI narrator interactions
- **Information Display**: Mission data, facility information, and narrative context
- **Notification Panel**: Alert system for important events and updates

### Data Structures

```csharp
public class VirtualPhone : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject phonePanel;
    public GameObject chatPanel;
    public GameObject notificationPanel;
    public GameObject missionPanel;
    
    [Header("Display Settings")]
    public KeyCode toggleKey = KeyCode.C;
    public float animationDuration = 0.3f;
    public bool startOpen = false;
    
    [Header("Chat Settings")]
    public int maxMessages = 50;
    public float messageDelay = 0.1f;
    public Color narratorColor = Color.cyan;
    public Color playerColor = Color.white;
    
    public bool isOpen { get; private set; }
    public List<ChatMessage> messageHistory { get; private set; }
}

public struct ChatMessage
{
    public string sender;
    public string content;
    public DateTime timestamp;
    public MessageType type;
}

public enum MessageType
{
    Narrator,
    System,
    Alert,
    Mission,
    Player
}
```

### Engine Features Required

- **Unity UI Toolkit**: Modern UI components for phone interface
- **Unity Input System**: Key binding and input handling
- **Animation System**: Smooth open/close transitions
- **Text Mesh Pro**: Rich text support for formatted chat messages
- **Scroll Rect Components**: Chat history scrolling and navigation

### Integration Points

- **AI Narrator System**: Message delivery and response handling
- **Mission System**: Mission updates and objective tracking
- **HUD System**: Notification integration and state management
- **Audio System**: Message notification sounds and voice-over integration
- **Save System**: Message history and phone state persistence

### Acceptance Criteria

**GIVEN** the player is in the game
**WHEN** they press the 'C' key
**THEN** the virtual phone interface opens with smooth animation

**GIVEN** the AI narrator has a message
**WHEN** the message is triggered
**THEN** it appears in the chat panel with appropriate formatting and timestamp

**GIVEN** a mission update occurs
**WHEN** the phone is open
**THEN** the mission panel displays updated objectives and progress

**GIVEN** the phone is closed
**WHEN** a new message arrives
**THEN** a notification appears on the HUD indicating new messages

## Integration Architecture

### System Dependencies

```
Player Controller
├── HUD System
├── Inventory System
├── Interactable Objects
└── Virtual Phone System

HUD System
├── Player Controller (state updates)
├── Inventory System (pickup notifications)
├── Interactable Objects (interaction prompts)
└── Virtual Phone System (notifications)

Inventory System
├── Player Controller (pickup detection)
├── Interactable Objects (item transfer)
└── Save System (persistence)

Interactable Objects
├── Player Controller (interaction)
├── HUD System (highlighting)
├── Inventory System (rewards)
└── Puzzle System (state changes)

Virtual Phone System
├── AI Narrator System (messages)
├── Mission System (updates)
├── HUD System (notifications)
└── Save System (history)
```

### Event System Architecture

```csharp
public class GameEventManager
{
    public static event System.Action<PlayerState> OnPlayerStateChanged;
    public static event System.Action<ItemData> OnItemPickedUp;
    public static event System.Action<InteractionData> OnInteractionStarted;
    public static event System.Action<ChatMessage> OnMessageReceived;
    public static event System.Action<MissionData> OnMissionUpdated;
    
    public static void TriggerPlayerStateChanged(PlayerState newState);
    public static void TriggerItemPickedUp(ItemData item);
    public static void TriggerInteractionStarted(InteractionData data);
    public static void TriggerMessageReceived(ChatMessage message);
    public static void TriggerMissionUpdated(MissionData mission);
}
```

## Performance Requirements

### Target Specifications

- **Frame Rate**: Maintain 60 FPS minimum on target hardware
- **Memory Usage**: Core systems under 512MB combined allocation
- **Input Latency**: <16ms (1 frame) response time for all interactions
- **UI Responsiveness**: <100ms for UI transitions and updates
- **Loading Time**: <2 seconds for initial system initialization

### Optimization Strategies

- **Object Pooling**: Reuse UI elements and interaction objects
- **LOD Systems**: Reduce detail for distant interactable objects
- **Async Operations**: Non-blocking inventory operations and message updates
- **Batch Rendering**: Combine similar visual effects for UI elements
- **Memory Management**: Proper cleanup of unused resources and events

## QA Checklist

### First-Person Controller
- [ ] Movement responds smoothly to WASD input
- [ ] Mouse look functions correctly with configurable sensitivity
- [ ] Sprint increases speed by 100% with appropriate audio feedback
- [ ] Crouch reduces height by 50% and speed by 75%
- [ ] Interaction prompts appear for valid objects within range
- [ ] Collision detection works properly with environment geometry
- [ ] Camera clamping prevents unrealistic rotation angles

### HUD & Visual Style
- [ ] Health bar displays current health accurately
- [ ] Interactive objects highlight when looked at
- [ ] Interaction prompts show appropriate action text
- [ ] Minimap/compass updates with player position
- [ ] Visual effects (glow/bloom) enhance without obscuring gameplay
- [ ] UI elements scale properly at different resolutions
- [ ] Color contrast meets accessibility standards

### Inventory System
- [ ] Items can be picked up and added to inventory
- [ ] Pickup notifications appear for 2-3 seconds
- [ ] Inventory panel opens/closes smoothly with Tab key
- [ ] Items display with correct icons and descriptions
- [ ] Stackable items combine properly in inventory slots
- [ ] Inventory capacity limits prevent overfilling
- [ ] Item use/equip functions work correctly

### Interactable Objects
- [ ] Objects highlight when player is in range
- [ ] Interaction prompts display correct action text
- [ ] Pickup items transfer to inventory correctly
- [ ] Examine objects show appropriate descriptions
- [ ] Use objects respond correctly with/without required items
- [ ] Audio feedback plays for all interactions
- [ ] State changes trigger appropriate events

### Virtual Phone System
- [ ] Phone opens/closes with 'C' key smoothly
- [ ] Chat messages display with correct formatting and timestamps
- [ ] AI narrator messages appear in real-time
- [ ] Mission updates reflect current objectives
- [ ] Notifications appear for important events
- [ ] Message history scrolls properly
- [ ] Phone state persists between sessions

### Integration Testing
- [ ] All systems communicate through event system correctly
- [ ] Save/load functionality preserves system states
- [ ] Performance targets are met during stress testing
- [ ] Memory usage remains within specified limits
- [ ] Cross-system interactions function without conflicts
- [ ] Error handling prevents system crashes
- [ ] User input is processed correctly across all systems