# Dungeon Setup Instructions

## Architecture Overview

The dungeon uses the strict **View/Model** architecture. Scene setup is minimal: gameplay objects are **not** placed in the scene manually - they are generated and wired at runtime by the pure C# `GameBootstrapper` (`Zenject.IInitializable`).

Flow:

1. `BootstrapOfDungeon` (Zenject installer on the `=== BOOTSTRAP ===` object in `Main.unity`) binds all models, facades, tickers and the `GameBootstrapper`.
2. `GameBootstrapper.Initialize()` runs the async pipeline:
   - `GenerateDungeon()` - generates `DungeonData` via `DungeonModel`
   - `SpawnDungeonVisual()` - Addressables loads `DungeonVisual` prefab (`DungeonVisualizer` view) → `Init(model)`
   - `SpawnPlayer()` - Addressables loads `Player` prefab → `Init(PlayerModel)`
   - `SpawnEnemies()` - `DungeonFacade` spawns each enemy into its room, each view `Init(EnemyModel)`
3. Views are wired to models via `Init(IModel)`; models tick via `SimulationTicker` (Zenject `ITickable`).

## Scene Hierarchy Setup

The only required object in `Main.unity` is the Zenject scene context with `BootstrapOfDungeon` installed on the `=== BOOTSTRAP ===` GameObject. Tilemaps are under `=== WORLD === / Grid`:

- `Floor Tilemap`
- `Bounds Tilemap`

## Configure DungeonGeneratorConfig

The `Dungeon Generation Config` asset (`Assets/_Core/Scripts/Dungeon/Dungeon Generation Config.asset`, ScriptableObject `DungeonGeneratorConfig`) controls generation:

| Field | Value |
|-------|-------|
| Zone Size | 10 |
| Min Room Size | 3 |
| Zones Count | 5 |
| Corridor Width | 1 |
| Corridor Length | 1 |
| Seed | 0 (random) |

The installer binds it via `DungeonGeneratorConfig.CreateInstance<DungeonGeneratorConfig>()` - never `new`.

## Configure DungeonVisualizer Prefab

`Assets/_Core/Prefabs/Dungeon Visualizer.prefab` holds the `DungeonVisualizer` view:

| Field | Reference |
|-------|-----------|
| Floor Tilemap | `Floor Tilemap` |
| Wall Tilemap | `Bounds Tilemap` |
| Floor Tile | `White` (from TestTileMap folder) |
| Wall Tile | `Black` (from TestTileMap folder) |
| Door Tile | `Brown` (from TestTileMap folder) |

## Expected Result

When you press Play:

1. `GameBootstrapper` generates the dungeon
2. `DungeonVisualizer` view is spawned and `Init`'d with the dungeon model
3. Rooms rendered with white floor + black walls, corridors + doors (brown tiles)
4. Player spawned at the start room and wired to `PlayerModel`
5. Enemies spawned into their rooms and wired to `EnemyModel`s
6. Walls on `Bounds Tilemap` have collision (TilemapCollider2D)
7. Door locking/unlocking driven by room enemy count (doors unlock when all enemies die)

## Testing Door Logic

Door state is derived from room enemy counts on the model (`RoomData.EnemiesInside`):

- Adding/removing enemies happens through `DungeonModel.AddEnemyToRoom/RemoveEnemyFromRoom` (called by `GameBootstrapper`/enemy death)

## Layer Configuration

Ensure `Bounds Tilemap` GameObject has:

- Layer: `8` (or your collision layer)
- `TilemapCollider2D` component (existing)

The `TilemapCollider2D` provides collision so the player cannot pass through walls.