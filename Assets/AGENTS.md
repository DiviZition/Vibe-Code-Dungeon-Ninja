# AGENTS.md - Agent Coding Guidelines

This document provides guidelines for agentic coding agents operating in this Unity project.

---

## 1. Build, Test, and Run Commands

### Unity Editor
- Open project in Unity Hub or via `Unity -projectPath <path>`
- Build: `File > Build Settings > Build` (Ctrl+Shift+B)
- Play: Press Play button in Editor

### Running Tests (Unity Test Runner - NUnit)

**Via Editor:**
1. Open `Window > General > Test Runner`
2. Click "Run All" or double-click specific test

**Via Command Line:**
```bash
Unity -projectPath "<path>" -runTests -testResults "results.xml" -testPlatform playmode
```

**Single Test via Command Line:**
```bash
Unity -projectPath "<path>" -runTests -testFilter "TestMethodName" -testResults "results.xml"
```

**Single Test (specific class):**
```bash
Unity -projectPath "<path>" -runTests -testFilter "Namespace.ClassName.TestMethodName" -testResults "results.xml"
```

### Building
- **Windows**: `File > Build Settings > Build` (outputs .exe)
- **WebGL**: Switch platform to WebGL, then build

---

## 2. Code Style Guidelines

### Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Namespaces | PascalCase | `Player`, `Enemy`, `TimeControll` |
| Classes/Structs | PascalCase | `PlayerMovement`, `Enemy` |
| Public Methods | PascalCase | `SetActive()`, `TakeDamage()` |
| Private Fields | _camelCase | `_rb`, `_playerVisual` |
| Properties | PascalCase | `IsActive`, `Health` |
| Parameters | camelCase | `newActiveState`, `damageAmount` |
| Constants | PascalCase | `MaxSpeed`, `DefaultDelay` |
| Interfaces | IPascalCase | `ITimeControllable`, `IDamageable` |

### File Organization
```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using Zenject;
using Player;
using R3;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _initialSpeed = 10f;

        [Header("References")]
        [SerializeField] private Rigidbody2D _rb;

        private GameInput _gameInput;
        private Vector2 _currentDirection;

        private void OnEnable() { }
        private void Update() { }
        
        public void ResetSpeed() { }
    }
}
```

### Using Statements
- Group: System, Unity, Third-party (Zenject, R3, UniTask)
- Order alphabetically within groups

### Serialization
- `[SerializeField]` for private fields needing Unity serialization
- `[field: SerializeField]` for properties with private backing fields
- `[SerializeField, Range(0, 10)]` for slider fields
- Use `[Header("Section")]` to group fields in Inspector

### Component Dependencies
```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
}
```

### Properties
```csharp
// Serialized backing field pattern
[field: SerializeField] public PlayerHealth Health { get; private set; }

// Computed properties
public Vector3 Position => Transform.localPosition;
```

---

## 3. Anti-Overengineering Principles

- **Don't create interfaces** unless you have 3+ implementations
- **Don't create abstractions** "just in case" — only when needed
- **Prefer simple, readable code** over clever architecture
- Only add code truly needed for the current task
- When in doubt, ask the user

---

## 4. Async Code (UniTask)

```csharp
private CancellationTokenSource _cancellationTokenSource;

private async void StartBehavior()
{
    _cancellationTokenSource = new CancellationTokenSource();
    while (!_cancellationTokenSource.IsCancellationRequested)
    {
        await UniTask.Yield(PlayerLoopTiming.Update);
    }
}

private void OnDestroy()
{
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
}
```

- Use `UniTask` for async/await in Unity
- Use `UniTaskVoid` for fire-and-forget
- Always dispose `CancellationTokenSource` in `OnDestroy`

---

## 5. Dependency Injection (Zenject)

```csharp
[Inject] private TimeController _timeController;

[Inject]
public void Construct(TimeController timeController)
{
    _timeController = timeController;
}
```

- Use `[Inject]` for field injection
- Use `[Inject]` on constructor-like methods for method injection

---

## 6. Error Handling

- Use try-catch for potentially failing operations
- Log errors: `Debug.LogError()`, `Debug.LogWarning()`
- Null checks: Use null-conditional operators `?.`
- In `Start()`, log error if required component is missing

---

## 7. Testing Conventions

### NUnit Framework
```csharp
[Test]
public static void TestMethodName()
{
    Assert.AreEqual(expected, actual, "Message on failure");
}
```

- Place tests in `Editor/` folder or test assemblies
- Use `[UnityTest]` for PlayMode tests (yield return null)

---

## 8. Project Structure

```
Assets/
├── _Core/                    # Your gameplay code
│   ├── Scripts/
│   │   ├── Player/
│   │   ├── Enemy/
│   │   └── Time Manager/
│   ├── Scenes/
│   └── Settings/
├── Plugins/                  # Zenject, R3, etc.
└── Assets/                   # Third-party assets
```

---

## 9. Key Dependencies

| Package | Purpose |
|---------|---------|
| Unity 6000+ | Game engine |
| URP | Rendering |
| New Input System | Player input |
| UniTask | Async/await |
| R3 | Events |
| Zenject | DI |
| DOTween | Animation |
| Cinemachine 3.x | Camera (use `Unity.Cinemachine` namespace) |

---

## 10. Player System Architecture

- **Service Locator**: `PlayerCore` holds service references
- **Services**: Implement interfaces (`IPlayerMovementService`, etc.)
- **Events**: Use R3 `EventBus` for output-only communication

```csharp
// External → Player
playerCore.GetService<IPlayerStatsService>().TakeDamage(10);

// Player → External
EventBus.Publish(new PlayerDiedEvent());
```

---

## 11. Workflow: Use BMad Skills

This project uses the BMad framework for structured development. For any development task, check available skills first:

- **Story development**: `bmad-dev-story` or `gds-dev-story`
- **Code changes**: `bmad-quick-dev` or `gds-quick-dev`
- **Architecture**: `gds-game-architecture` or `bmad-create-architecture`
- **Documentation**: `gds-create-gdd` or `gds-document-project`

Skills are loaded via the skill tool and provide step-by-step guidance. Prefer them over ad-hoc work.
