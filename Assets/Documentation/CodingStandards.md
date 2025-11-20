# Protocol EMR - Coding Standards

## Overview

This document defines coding standards and best practices for Protocol EMR development. Following these guidelines ensures code consistency, readability, and maintainability across the project.

---

## C# Naming Conventions

### Classes and Structs
**PascalCase** - Each word capitalized
```csharp
public class PlayerController { }
public class FirstPersonCamera { }
public struct WeaponStats { }
```

### Methods
**PascalCase**
```csharp
public void HandleMovement() { }
private void ProcessInput() { }
```

### Properties
**PascalCase**
```csharp
public float MaxStamina { get; set; }
public bool IsGrounded { get; private set; }
```

### Fields (Private)
**camelCase** with optional underscore prefix
```csharp
private float movementSpeed;
private bool _isInitialized; // underscore optional but consistent within file
```

### Serialized Fields
**camelCase** with `[SerializeField]` attribute
```csharp
[SerializeField] private float walkSpeed = 5.0f;
[SerializeField] private Transform playerBody;
```

### Constants
**UPPER_SNAKE_CASE**
```csharp
private const float MAX_SPEED = 10.0f;
private const int DEFAULT_HEALTH = 100;
```

### Events
**PascalCase** with "On" prefix
```csharp
public event Action OnJump;
public event Action<float> OnHealthChanged;
```

### Interfaces
**PascalCase** with "I" prefix
```csharp
public interface IInteractable { }
public interface IDamageable { }
```

### Enums
**PascalCase** for enum and values
```csharp
public enum WeaponType
{
    Pistol,
    Rifle,
    Shotgun
}
```

---

## Namespace Organization

### Structure
```csharp
namespace ProtocolEMR.Core.Player
{
    // Player-related classes
}

namespace ProtocolEMR.Systems.Combat
{
    // Combat-related classes
}

namespace ProtocolEMR.UI.HUD
{
    // HUD UI classes
}
```

### Hierarchy
```
ProtocolEMR
├── Core
│   ├── Input
│   ├── Camera
│   ├── Player
│   ├── Settings
│   └── Performance
├── Systems
│   ├── Combat
│   ├── Interaction
│   ├── Inventory
│   └── AI
├── UI
│   ├── HUD
│   ├── Menus
│   └── Phone
└── Utilities
    ├── Extensions
    └── Helpers
```

---

## Code Organization

### File Structure
```csharp
// File header (optional for licensing)
// Copyright (c) 2024 Protocol EMR Team

using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Player
{
    /// <summary>
    /// XML documentation for public API
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        // Constants
        private const float MAX_STAMINA = 100f;

        // Serialized fields (grouped by category with headers)
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 8.0f;

        [Header("Stamina System")]
        [SerializeField] private float staminaDrainRate = 10f;

        // Private fields
        private CharacterController controller;
        private float currentStamina;
        private bool isSprinting;

        // Public properties
        public float CurrentStamina => currentStamina;
        public bool IsSprinting => isSprinting;

        // Events
        public event Action OnStaminaDepleted;

        // Unity lifecycle methods (in order)
        private void Awake() { }
        private void Start() { }
        private void Update() { }
        private void FixedUpdate() { }
        private void OnDestroy() { }

        // Public methods
        public void ResetStamina() { }

        // Private methods
        private void HandleMovement() { }
        private void UpdateStamina() { }
    }
}
```

---

## Documentation

### XML Documentation
Use for **all public APIs**:
```csharp
/// <summary>
/// Handles player movement based on input vector.
/// </summary>
/// <param name="input">Normalized movement direction (WASD input)</param>
/// <returns>True if movement was successful</returns>
public bool Move(Vector2 input)
{
    // Implementation
}
```

### Inline Comments
Use **sparingly** for complex logic only:
```csharp
// Good: Explains WHY, not WHAT
// Using square root of jump height for more natural arc
velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

// Bad: Redundant, obvious from code
// Set velocity to 0
velocity = Vector3.zero;
```

### Region Usage
Use **sparingly**, prefer small focused classes:
```csharp
#region Unity Lifecycle
private void Awake() { }
private void Start() { }
#endregion

#region Public API
public void Initialize() { }
#endregion
```

---

## Best Practices

### Singleton Pattern
Use for **managers only**, not gameplay objects:
```csharp
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

### SerializeField vs Public
**Prefer `[SerializeField]` over public fields**:
```csharp
// Good: Private with SerializeField
[SerializeField] private float speed = 5.0f;

// Bad: Public field (breaks encapsulation)
public float speed = 5.0f;

// Good: Public property with private backing field
public float Speed => speed;
```

### Null Checks
Check for null on external references:
```csharp
private void Start()
{
    if (playerTransform == null)
    {
        Debug.LogError("PlayerTransform is not assigned!");
        return;
    }
}
```

### Performance

#### Caching
Cache component references in Awake/Start:
```csharp
// Good: Cache once
private Rigidbody rb;
private void Awake() => rb = GetComponent<Rigidbody>();

// Bad: GetComponent every frame
private void Update() => GetComponent<Rigidbody>().AddForce(...);
```

#### String Comparison
Use CompareTag instead of ==:
```csharp
// Good
if (other.CompareTag("Player")) { }

// Bad
if (other.tag == "Player") { }
```

#### Avoid Allocations in Update
```csharp
// Good: Reuse vector
private Vector3 movement;
private void Update()
{
    movement.x = input.x;
    movement.z = input.y;
}

// Bad: New allocation every frame
private void Update()
{
    Vector3 movement = new Vector3(input.x, 0, input.y);
}
```

---

## Unity-Specific

### Component Requirements
Document required components:
```csharp
/// <summary>
/// Player controller requiring CharacterController for physics.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour { }
```

### Serialization
Use `[SerializeField]` for inspector visibility:
```csharp
[Header("Movement Settings")]
[SerializeField] private float speed = 5.0f;
[SerializeField] [Range(0, 10)] private float sensitivity = 1.0f;
[SerializeField] [Tooltip("Max speed when sprinting")] private float maxSpeed = 10.0f;
```

### Coroutines
Use for time-based operations:
```csharp
private IEnumerator FadeOut(float duration)
{
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float alpha = 1f - (elapsed / duration);
        // Apply alpha
        yield return null;
    }
}
```

### Events
Use UnityEvents for inspector assignment, C# events for code:
```csharp
// C# event (code-only)
public event Action<float> OnDamaged;

// UnityEvent (inspector + code)
[SerializeField] private UnityEvent onInteract;
```

---

## Error Handling

### Debug Logging
Use appropriate log levels:
```csharp
Debug.Log("Info: Player spawned");
Debug.LogWarning("Warning: Ammo low");
Debug.LogError("Error: Failed to load save file");
Debug.LogException(exception);
```

### Defensive Programming
```csharp
public void TakeDamage(float amount)
{
    if (amount < 0)
    {
        Debug.LogWarning("Damage amount cannot be negative");
        return;
    }

    currentHealth -= amount;
    currentHealth = Mathf.Max(currentHealth, 0);
}
```

---

## Code Style

### Braces
Always use braces, even for single-line statements:
```csharp
// Good
if (isGrounded)
{
    velocity.y = 0;
}

// Bad
if (isGrounded)
    velocity.y = 0;
```

### Spacing
```csharp
// Good spacing
private void Update()
{
    float speed = CalculateSpeed();
    Move(speed);
}

// Bad spacing (too compressed)
private void Update(){float speed=CalculateSpeed();Move(speed);}
```

### Line Length
Keep lines under **120 characters**:
```csharp
// Good
var result = SomeMethod(
    parameter1,
    parameter2,
    parameter3
);

// Bad
var result = SomeMethod(parameter1, parameter2, parameter3, parameter4, parameter5, parameter6);
```

### Ternary Operators
Use for simple assignments only:
```csharp
// Good
float speed = isSprinting ? sprintSpeed : walkSpeed;

// Bad (too complex)
float speed = isSprinting ? (isExhausted ? crouchSpeed : sprintSpeed) : (isCrouching ? crouchSpeed : walkSpeed);
```

---

## Version Control

### Commit Messages
Follow conventional commits:
```
feat: Add sprint stamina system
fix: Camera bobbing not resetting on idle
docs: Update coding standards
refactor: Extract input callbacks to separate methods
perf: Cache transform references in player controller
test: Add unit tests for stamina system
chore: Update Unity version to 2022.3.15f1
```

### File Naming
- **Scripts**: PascalCase.cs (e.g., `PlayerController.cs`)
- **Scenes**: PascalCase.unity (e.g., `MainMenu.unity`)
- **Prefabs**: PascalCase.prefab (e.g., `Player.prefab`)
- **Materials**: PascalCase_Material.mat (e.g., `Metal_Material.mat`)

---

## Testing

### Debug Tools
Create debug methods for testing:
```csharp
#if UNITY_EDITOR
[ContextMenu("Reset Player")]
private void DebugResetPlayer()
{
    transform.position = Vector3.zero;
    currentHealth = maxHealth;
}
#endif
```

### Gizmos
Use for visual debugging:
```csharp
private void OnDrawGizmosSelected()
{
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, interactionRange);
}
```

---

## Performance Profiling

### Profile Marks
Use for profiling specific code sections:
```csharp
void Update()
{
    Profiler.BeginSample("PlayerMovement");
    HandleMovement();
    Profiler.EndSample();
}
```

---

## Anti-Patterns to Avoid

### ❌ Don't Use Update for Everything
```csharp
// Bad: Expensive check every frame
void Update()
{
    if (Vector3.Distance(transform.position, target.position) < 5f)
        Attack();
}

// Good: Use events or coroutines with intervals
```

### ❌ Don't Use Find/FindObjectOfType in Update
```csharp
// Bad: Very expensive
void Update()
{
    GameObject player = GameObject.Find("Player");
}

// Good: Cache in Start
private GameObject player;
void Start() => player = GameObject.Find("Player");
```

### ❌ Don't Ignore Memory Allocations
```csharp
// Bad: New list every frame
void Update()
{
    List<Enemy> enemies = new List<Enemy>();
}

// Good: Reuse or use object pooling
private List<Enemy> enemies = new List<Enemy>();
```

---

## Resources

- **Unity C# Style Guide**: https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity
- **Microsoft C# Conventions**: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- **Unity Best Practices**: https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity.html

---

## Enforcement

### Code Reviews
All code must pass review before merging:
- [ ] Follows naming conventions
- [ ] Has XML documentation for public APIs
- [ ] No warnings or errors in console
- [ ] Performance considerations addressed
- [ ] Tested in editor and builds

### Tools
- **IDE**: Visual Studio 2022 or JetBrains Rider
- **Extensions**: Unity Code Snippets, C# XML Documentation
- **Linting**: Roslyn analyzers for code quality

---

**Last Updated**: Sprint 1 Foundation Phase
**Review Frequency**: Every sprint
