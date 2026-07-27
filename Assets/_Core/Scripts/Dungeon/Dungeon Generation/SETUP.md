# Dungeon Setup Instructions

## Scene Hierarchy Setup

### 1. Create DungeonManager Object

In `Main.unity`, create a new empty GameObject:
- **Parent:** `=== WORLD ===` (existing root object)
- **Name:** `DungeonManager`

### 2. Add Components to DungeonManager

Add these scripts to the `DungeonManager` GameObject:
- `DungeonGenerator`
- `DungeonVisualizer`

### 3. Link Tilemaps (Already Exist)

The scene already has tilemaps under `=== WORLD === / Grid`:
- `Floor Tilemap`
- `Bounds Tilemap`

### 4. Configure DungeonGenerator

| Field | Value |
|-------|-------|
| Zone Size | 10 |
| Min Room Size | 3 |
| Zones Count | 5 |
| Corridor Width | 1 |
| Corridor Length | 1 |
| Seed | 0 (random) |

### 5. Configure DungeonVisualizer

| Field | Reference |
|-------|-----------|
| Floor Tilemap | `Floor Tilemap` |
| Wall Tilemap | `Bounds Tilemap` |
| Floor Tile | `White` (from TestTileMap folder) |
| Wall Tile | `Black` (from TestTileMap folder) |
| Door Tile | `Brown` (from TestTileMap folder) |

### 6. Link DungeonVisualizer in DungeonGenerator (optional)

For automatic visualization on regenerate:
- Add `DungeonVisualizer` reference if needed

---

## Expected Result

When you press Play or call `DungeonManager.GenerateDungeon()`:
1. Dungeon generates with rooms and corridors
2. Rooms rendered with white floor + black walls
3. Corridors rendered with white floor + black side walls
4. Doors (brown tiles) at corridor-room connections
5. Walls on `Bounds Tilemap` have collision (TilemapCollider2D)

---

## Testing Door Logic

### Add enemy to room:
```csharp
dungeonManager.Data.Rooms[0].AddEnemy();
```

### Remove enemy from room:
```csharp
dungeonManager.Data.Rooms[0].RemoveEnemy();
```

### Manually open room:
```csharp
dungeonManager.OpenRoom(0);
```

---

## Layer Configuration

Ensure `Bounds Tilemap` GameObject has:
- Layer: `8` (or your collision layer)
- `TilemapCollider2D` component (existing)

The `TilemapCollider2D` provides collision so the player cannot pass through walls.