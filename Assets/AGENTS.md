# AGENTS.md - Agent Coding Guidelines

This document provides guidelines for agentic coding agents operating in this Unity project.

---

## 1. Build, Test, and Run Commands

### Unity Editor
- Open the project in Unity Hub or via `Unity -projectPath <path>`
- Build: `File > Build Settings > Build` (Ctrl+Shift+B)
- Play: `File > Build And Run` (Ctrl+B)

### Running Tests
Tests use Unity's Test Runner (NUnit framework).

**Via Editor:**
1. Open `Window > General > Test Runner`
2. Click "Run All" or select specific tests
3. For a single test, double-click the test name

**Via Command Line:**
```bash
Unity -projectPath "<path>" -runTests -testResults "<results.xml>" -testPlatform playmode
```

**Single Test via Command Line:**
```bash
Unity -projectPath "<path>" -runTests -testFilter "TestMethodName" -testResults "results.xml"
```

### Building
- **Windows**: `File > Build Settings > Build` (outputs .exe)
- **WebGL**: Switch platform to WebGL, then build
- **Android/iOS**: Switch platform, then build (requires SDK setup)

---

## 2. Code Style Guidelines

### General Principles
- Write clean, readable C# code following Unity best practices
- Avoid premature optimization
- Use meaningful names for all identifiers
- Avoid unnecessary code and methods—keep implementations simple and straightforward. Only add code when truly needed for the current task. Don't add "nice to have" methods or abstractions that aren't required yet.

### Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Namespaces | PascalCase | `FlyBoxEffect`, `VHierarchy` |
| Classes/Structs | PascalCase | `FlyingBox`, `SimpleRotator` |
| Public Methods | PascalCase | `SetActive()`, `LaunchBox()` |
| Private Fields | _camelCase | `_boxPrefab`, `_transform` |
| Properties | PascalCase | `IsActive`, `Transform` |
| Parameters | camelCase | `newActiveState`, `flyDistance` |
| Constants | PascalCase | `MaxBoxes`, `DefaultSpeed` |
| Interfaces | IPascalCase | `IPoolable` |

### File Organization
```csharp
using System;
using UnityEngine;

// Namespace
namespace DungeonExplorer.Player
{
    public class PlayerCore : MonoBehaviour
    {
        // Properties (serialized backing field pattern)
        [field: SerializeField] public Transform Transform { get; private set; }
        
        // Private fields
        [SerializeField] private CharacterController _characterController;
        
        // Public properties
        public bool IsActive { get; private set; }
        
        // Public methods
        public void Initialize() { }
        
        // Private methods
        private void OnDestroy() { }
    }
}
```

### Using Statements
- Place `using` statements at the top, grouped: System, Unity, Third-party
- Use aliases to avoid conflicts: `using Random = UnityEngine.Random;`
- Use static imports for frequently used utilities:
  ```csharp
  using static VHierarchy.Libs.VUtils;
  using static VHierarchy.VHierarchyData;
  ```

### Serialization
- Use `[SerializeField]` for private fields that need Unity serialization
- Use `[field: SerializeField]` for properties with private backing fields
- Use `[SerializeField, Range(0, 10)]` for slider fields

### Properties
```csharp
// Preferred for serialized properties
[field: SerializeField] public Transform Transform { get; private set; }

// Preferred for computed properties
public Vector3 Position => Transform.localPosition;
```

### Component Dependencies
When a service requires a Unity component, reference it through Inspector and use OnValidate for auto-assignment:

```csharp
public class PlayerCore : MonoBehaviour, IPlayerCore
{
    [Header("Required Components")]
    [SerializeField] private CharacterController _characterController;

    private void OnValidate()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }
}
```

**Pattern:**
- Serialize required components with `[SerializeField]`
- Use `OnValidate()` to auto-assign from same GameObject
- Log error in Start() if required component is missing
- This makes dependencies visible in Inspector

### Async Code
- Use `UniTask` (Unity-specific async/await library)
- Use `UniTaskVoid` for fire-and-forget operations
- Use `CancellationTokenSource` for cancellation support

```csharp
public async UniTaskVoid StartEffect()
{
    _cancelTokenSource = new CancellationTokenSource();
    while (_cancelTokenSource.IsCancellationRequested == false)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_spawnDelay));
    }
}
```

### Error Handling
- Use try-catch for potentially failing operations
- Log errors appropriately: `Debug.LogError()`, `Debug.LogWarning()`
- Handle null checks with null-conditional operators: `?.`

### Unity-Specific Patterns
- Use `[ContextMenu("Name")]` for debug/testing methods
- Use `[ExecuteInEditMode]` for editor-only behavior
- Use `[RequireComponent(typeof(Collider))]` for component dependencies
- Implement `IPoolable` for object pooling patterns

---

## 3. Testing Conventions

### NUnit Framework
Tests use NUnit (Unity's built-in testing framework).

### Test Structure
```csharp
class TreeModelTests
{
    [Test]
    public static void TestTreeModelCanAddElements()
    {
        // Arrange
        var root = new TreeElement { TreeName = "Root", Depth = -1 };
        
        // Act
        model.AddElement(newElement, root, 0);
        
        // Assert
        Assert.AreEqual(expected, actual, "Message on failure");
    }
}
```

### Running Tests
- Place tests in `Editor/` folder for editor tests
- Place tests in test assemblies (.asmdef with test framework reference)
- Use `[UnityTest]` for tests that need PlayMode (yield return null)

---

## 4. Project Structure

```
Assets/
├── _Core/                    # Your gameplay code goes here
│   ├── Scripts/
│   ├── Scenes/
│   └── Settings/
├── Plugins/                  # Third-party plugins (Sirenix, etc.)
├── ThirdParty/               # Third-party assets
├── AssetInventory/           # Asset management tool
├── vHierarchy/               # Hierarchy enhancement
├── vFavorites/                # Favorites system
└── VolumetricLightBeam/      # VFX library
```

---

## 5. Key Dependencies

| Package | Purpose |
|---------|---------|
| Unity 6000.3.10f1+ | Game engine (LTS version) |
| Universal Render Pipeline (URP) | Rendering pipeline |
| New Input System | Player input handling |
| UniTask | Async/await for asynchronous operations |
| R3 | Reactive programming (events, streams) |
| Addressables | Async asset loading and management |
| DOTween | Animation and tweening |
| Cinemachine 3.x | Camera system |
| Sirenix.OdinInspector | Enhanced inspector |

### Cinemachine 3.x Usage
For Unity 6000+, use **Cinemachine 3.x** (not the deprecated 2.x):
- **Namespace**: `Unity.Cinemachine` (not `Cinemachine`)
- **FreeLook**: Use `CinemachineCamera` component with appropriate behaviors
- **Input**: Use `CinemachineInputProvider` component to connect New Input System

Example setup:
```csharp
using Unity.Cinemachine;

CinemachineCamera cineCamera = cameraObj.AddComponent<CinemachineCamera>();
cineCamera.FollowTarget = playerObj.transform;
cineCamera.LookAtTarget = playerObj.transform;

CinemachineInputProvider inputProvider = cameraObj.AddComponent<CinemachineInputProvider>();
inputProvider.XYAxis = "Look"; // Input action name
```

### Player System Architecture
The player system uses a **Service-Oriented Architecture** with a **Service Locator** pattern.

#### Core Concepts
- **Services**: Each player system (movement, combat, stats, inventory) is a separate service
- **Interfaces**: All services expose via interfaces (`IPlayerMovementService`, `IPlayerStatsService`, etc.)
- **Service Locator**: `PlayerCore` holds references to all services via interfaces
- **Event Bus**: R3-based events for output-only communication (Player → External)

#### Communication Patterns
| Direction | Mechanism | Example |
|----------|-----------|---------|
| External → Player | Direct service calls | `playerCore.GetService<IPlayerStatsService>().TakeDamage(10)` |
| Player → External | Event Bus (R3) | `EventBus.Publish(new PlayerDiedEvent())` |

#### Directory Structure
```
Assets/_Core/Scripts/Player/
├── Core/
│   ├── IPlayerCore.cs           # Interface for PlayerCore
│   ├── PlayerCore.cs            # Service locator, holds all service refs
├── Services/
│   ├── IPlayerService.cs       # Base interface
│   ├── IPlayerInputService.cs  # Interface
│   ├── PlayerInputService.cs   # Wraps GameInput
│   ├── IPlayerMovementService.cs
│   ├── PlayerMovementService.cs
│   ├── IPlayerStatsService.cs
│   ├── PlayerStatsService.cs
│   └── IPlayerCameraService.cs
│   ├── PlayerCameraService.cs
├── Configs/
│   ├── PlayerProfile.cs        # Main profile aggregating all configs
│   ├── PlayerMovementConfig.cs
│   └── PlayerStatsConfig.cs
├── Status/
│   └── StatusData.cs           # StatusType, StatusDurationType, PlayerStatus
├── Events/
│   ├── PlayerEvents.cs         # All player-related events
│   └── EventBus.cs            # R3-based event bus
└── UI/
    └── PlayerHUD.cs            # HUD display
```

#### Creating New Services
1. Create interface `IPlayerXxxService.cs`
2. Create implementation `PlayerXxxService.cs`
3. Register in `PlayerCore.InitializeServices()`
4. Use `GetService<IPlayerXxxService>()` to access

#### Event Usage
- Only use EventBus for **output events** (Player → External)
- Internal calls use direct service access
- All events defined in `PlayerEvents.cs`

---

## 6. Common Tasks

### Creating a New Script
1. Create in appropriate `_Core/` subfolder
2. Use PascalCase for filename (e.g., `BoxController.cs`)
3. Follow naming conventions above

### Adding a Component
```csharp
[RequireComponent(typeof(Rigidbody))]
public class BoxController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
}
```

### Creating a Prefab
1. Create GameObject in scene
2. Add all necessary components
3. Drag to Project window to create prefab
4. Use prefab for instantiating via code

---

## 7. Important Notes

- This is a Unity project - not Node.js/npm
- No `npm run` or typical web build commands
- All code is C# (Unity scripts)
- Use Unity Inspector for configuration
- Third-party assets are pre-installed in Assets/
