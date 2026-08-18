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
| Namespaces | PascalCase | `Player`, `Enemy`, `TimeControll`, `Core` |
| Classes/Structs | PascalCase | `PlayerModel`, `PlayerView`, `HealthModel` |
| Public Methods | PascalCase | `Init()`, `TakeDamage()` |
| Private Fields | _camelCase | `_rb`, `_model` |
| Properties | PascalCase | `IsActive`, `Health` |
| Parameters | camelCase | `newActiveState`, `damageAmount` |
| Constants | PascalCase | `MaxSpeed`, `DefaultDelay` |
| Interfaces | IPascalCase | `IPlayerModel`, `IEnemyModel`, `IDamageable` |

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
    // Model - pure C#, no Unity components, ticks via SimulationTicker
    public class PlayerModel : IPlayerModel, ITickable
    {
        public readonly struct MovementInput { }
        public void Tick() { }
    }

    // View - MonoBehaviour, initialized with the model, bridges Unity to the model
    public class PlayerView : MonoBehaviour, IView<IPlayerModel>, IHasDamageable
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D _rb;

        public void Init(IPlayerModel model) { }
    }

    // Forwarding view - translates Unity input into model calls only
    public class PlayerInputView : MonoBehaviour
    {
        public void Init(IPlayerModel model) { }
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

- **Crossing the View/Model boundary is always through an interface** (`IPlayerModel`, `IEnemyModel`, `IDungeonModel`, ...) - this is the one case where interfaces are required by the architecture
- **Don't create additional interfaces** for internal model internals unless you have 3+ implementations
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
- Bind `AsSingle().NonLazy()` for system singletons (TimeController, SimulationTicker, DungeonModel, DungeonFacade, GameBootstrapper)
- `ScriptableObject` configs (e.g. `DungeonGeneratorConfig`) cannot be `new`-ed in a container - bind via `ScriptableObject.CreateInstance<T>()`
- Views are **not** resolved through the container - they are spawned by `GameBootstrapper` (Addressables) and wired via `Init(IModel)`

---

## 5b. View/Model Architecture (MANDATORY)

This project uses a strict View/Model separation. All gameplay code MUST follow it:

- **Models** (`*Model`, `IModel`): pure C#, hold state/logic/data. NO `MonoBehaviour`, NO `GameObject`, NO components, NO `[SerializeField]`, NO Unity lifecycle callbacks. They register with `SimulationTicker` to tick (Zenject `ITickable`). May use `UnityEngine.Random`, `Vector2`, `Mathf`.
- **Views** (`*View`, `IView<TModel>`): `MonoBehaviour`s. Implement `Init(TModel model)` and forward Unity signals (colliders, input, physics) into the model. Never own gameplay decisions.
- **Forwarding views**: thin Unity-to-model translators (e.g. `PlayerInputView` reads input, calls `model.SetMovementInput(...)`).
- **Boundary crossing is always via interfaces**: models depend on `ITimeController`/`ITickable`, views depend on `IPlayerModel`/`IEnemyModel`/`IDungeonModel`.
- **Entry point**: pure C# `GameBootstrapper` (`Zenject.IInitializable`) runs the async init pipeline (generate dungeon → spawn DungeonVisualizer → spawn player → spawn enemies → bind views). `BootstrapOfDungeon` is the Zenject installer.
- **Facade**: `DungeonFacade` is the single facade over dungeon generation + enemy spawning.
- **Physics** (OverlapCircle, collisions, raycasts) belongs in views, then forwarded. **Combat math** lives in models.

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
├── _Core/                    # Gameplay code (assembly "Core")
│   ├── Scripts/
│   │   ├── Core/             # IModel, IView, IDamageable, IHealthModel, HealthModel, IHasDamageable, SimulationTicker
│   │   ├── Dungeon/          # IDungeonModel/DungeonModel, IDungeonFacade/DungeonFacade, DungeonEnemySpawner
│   │   │   └── Dungeon Generation/   # DungeonGenerator, DungeonVisualizer (view), DungeonData, config
│   │   ├── Player/           # IPlayerModel/PlayerModel, PlayerConfig, PlayerView, PlayerInputView
│   │   ├── Enemy/            # IEnemyModel/EnemyModel, EnemyBehaviorConfig, EnemyView
│   │   │   └── Behavior/     # Enemy states (EnemyMoveState, EnemyAttackState, EnemyIdleState)
│   │   ├── Time Manager/     # ITimeController, TimeController, SimulationTicker usage
│   │   ├── Shared/           # DamageableVisual (view for IDamageable)
│   │   ├── GameBootstrapper.cs    # Pure C# init pipeline (IInitializable)
│   │   ├── BootstrapOfDungeon.cs  # Zenject installer
│   │   └── Tests/
│   ├── Prefabs/
│   ├── Scenes/
│   └── Settings/             # GameInput.inputactions (+ generated GameInput.cs)
├── AGENTS.md
```

### Model / View / Bootstrapper responsibilities

| Layer | Lives in | Rules |
|-------|----------|-------|
| **Model** | `Core`/`Dungeon`/`Player`/`Enemy` namespaces | Pure C#, state+logic, `ITickable` via `SimulationTicker`, events via R3 `Observable` |
| **View** | MonoBehaviours | `Init(IModel)`, bridges Unity (physics/input/rendering) to model, no gameplay logic |
| **Facade** | `DungeonFacade` | Single entry to generation + spawning |
| **Bootstrapper** | `GameBootstrapper` | Spawns prefabs via Addressables, wires views to models |
| **Installer** | `BootstrapOfDungeon` | Zenject bindings only |

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

- **Strict View/Model** (see 5b): `PlayerModel` is pure C# (state + movement logic + ticking), `PlayerView` renders + applies physics forces, `PlayerInputView` forwards input, `DamageableVisual` renders damage feedback.
- **Health**: `IHealthModel`/`HealthModel` (pure C#, R3 events `OnDamaged`/`OnDeath`), reached via `IHasDamageable.Damageable`.
- **Damage**: any `IDamageable` (player, enemies) exposes `HealthModel`; physics checks (e.g. `OverlapCircleAll` in `EnemyView`) forward damage calls into models.

```csharp
// External → Player
playerModel.TakeDamage(10);

// Player → External
playerModel.Health.OnDeath.Subscribe(...);
```

---

## 11. Workflow: Use BMad Skills

This project uses the BMad framework for structured development. For any development task, check available skills first:

- **Story development**: `bmad-dev-story` or `gds-dev-story`
- **Code changes**: `bmad-quick-dev` or `gds-quick-dev`
- **Architecture**: `gds-game-architecture` or `bmad-create-architecture`
- **Documentation**: `gds-create-gdd` or `gds-document-project`

Skills are loaded via the skill tool and provide step-by-step guidance. Prefer them over ad-hoc work.
