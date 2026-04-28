using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dungeon
{
    public class DungeonVisualizer : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;

        [Header("Tiles")]
        [SerializeField] private Tile _floorTile;
        [SerializeField] private Tile _wallTile;
        [SerializeField] private Tile _doorTile;

        private DungeonData _data;

        public void Visualize(DungeonData data)
        {
            _data = data;
            ClearAll();

            if (data?.Rooms == null)
                return;

            Debug.Log($"[DungeonVisualizer] Generating {data.Rooms.Length} rooms, {data.Corridors?.Length ?? 0} corridors");

            for (int i = 0; i < data.Rooms.Length; i++)
            {
                var room = data.Rooms[i];
                Debug.Log($"[Room {i}] GridX={room.GridX}, GridY={room.GridY}, Size={room.Width}x{room.Height}, " +
                         $"Corridors=[{string.Join(",", room.ConnectedCorridorIndices ?? new int[0])}]");
                RenderRoom(room);
            }

            if (data.Corridors != null)
            {
                for (int i = 0; i < data.Corridors.Length; i++)
                {
                    var c = data.Corridors[i];
                    Debug.Log($"[Corridor {i}] From=({c.PointFrom.x:F1},{c.PointFrom.y:F1}) To=({c.PointTo.x:F1},{c.PointTo.y:F1}) " +
                             $"Width={c.CorridorWidth} Length={c.CorridorLength} Doors={c.DoorPositions?.Count ?? 0}");
                    RenderCorridor(c);
                }
            }

            Debug.Log("[DungeonVisualizer] Generation complete");
        }

        public void ClearAll()
        {
            _floorTilemap?.ClearAllTiles();
            _wallTilemap?.ClearAllTiles();
        }

        private void RenderRoom(Room room)
        {
            RenderRoomFloor(room);
            RenderRoomWalls(room);
        }

        private void RenderRoomFloor(Room room)
        {
            for (int x = room.GridX; x < room.GridX + room.Width; x++)
            {
                for (int y = room.GridY; y < room.GridY + room.Height; y++)
                {
                    _floorTilemap.SetTile(new Vector3Int(x, y, 0), _floorTile);
                }
            }
        }

        private void RenderRoomWalls(Room room)
        {
            int left = room.GridX - 1;
            int right = room.GridX + room.Width;
            int bottom = room.GridY - 1;
            int top = room.GridY + room.Height;

            for (int x = room.GridX - 1; x < room.GridX + room.Width + 1; x++)
            {
                _wallTilemap.SetTile(new Vector3Int(x, bottom, 0), _wallTile);
                _wallTilemap.SetTile(new Vector3Int(x, top, 0), _wallTile);
            }

            for (int y = room.GridY; y < room.GridY + room.Height; y++)
            {
                _wallTilemap.SetTile(new Vector3Int(left, y, 0), _wallTile);
                _wallTilemap.SetTile(new Vector3Int(right, y, 0), _wallTile);
            }
        }

        private void RenderCorridor(Corridor corridor)
        {
            int fromX = Mathf.RoundToInt(corridor.PointFrom.x);
            int fromY = Mathf.RoundToInt(corridor.PointFrom.y);
            int toX = Mathf.RoundToInt(corridor.PointTo.x);
            int toY = Mathf.RoundToInt(corridor.PointTo.y);

            int minX = Mathf.Min(fromX, toX);
            int maxX = Mathf.Max(fromX, toX);
            int minY = Mathf.Min(fromY, toY);
            int maxY = Mathf.Max(fromY, toY);

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    _floorTilemap.SetTile(new Vector3Int(x, y, 0), _floorTile);
                }
            }

            RenderCorridorWalls(corridor, minX, maxX, minY, maxY);
            RenderDoors(corridor);
        }

        private void RenderCorridorWalls(Corridor corridor, int minX, int maxX, int minY, int maxY)
        {
            if (maxX - minX > maxY - minY)
            {
                for (int x = minX; x < maxX; x++)
                {
                    _wallTilemap.SetTile(new Vector3Int(x, minY - 1, 0), _wallTile);
                    _wallTilemap.SetTile(new Vector3Int(x, maxY, 0), _wallTile);
                }
            }
            else
            {
                for (int y = minY; y < maxY; y++)
                {
                    _wallTilemap.SetTile(new Vector3Int(minX - 1, y, 0), _wallTile);
                    _wallTilemap.SetTile(new Vector3Int(maxX, y, 0), _wallTile);
                }
            }
        }

        private void RenderDoors(Corridor corridor)
        {
            if (corridor.DoorPositions == null)
                return;

            foreach (var doorPos in corridor.DoorPositions)
            {
                _wallTilemap.SetTile(doorPos, _doorTile);
            }
        }

        public void OpenDoors(int corridorIndex) => SetDoors(corridorIndex, null);
        public void CloseDoors(int corridorIndex) => SetDoors(corridorIndex, _doorTile);

        private void SetDoors(int corridorIndex, Tile tile)
        {
            if (_data?.Corridors == null || corridorIndex >= _data.Corridors.Length)
                return;

            var corridor = _data.Corridors[corridorIndex];
            if (corridor.DoorPositions == null)
                return;

            foreach (var doorPos in corridor.DoorPositions)
            {
                _wallTilemap.SetTile(doorPos, tile);
            }
        }
    }
}
