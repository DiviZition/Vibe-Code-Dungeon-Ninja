using R3;
using System;
using System.Collections.Generic;
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
        private IDisposable _corridorEvents;

        public void Visualize(DungeonData data)
        {
            _data = data;
            ClearAll();

            if (data == null || data.Rooms == null || data.Corridors == null)
                throw new NullReferenceException($"DungeonData = [{data}], Rooms = [{data.Rooms}], Corridors = [{data.Corridors}]");

            Debug.Log($"[DungeonVisualizer] Generating {data.Rooms.Count} rooms, {data.Corridors?.Count ?? 0} corridors");

            foreach (var room in data.Rooms)
            {
                RenderRoom(room);
                Debug.Log($"[Room {room}] GridX={room.GridX}, GridY={room.GridY}, Size={room.Width}x{room.Height}, " +
                         $"Corridors Count=[{room.ConnectedCorridors.Count}]");
            }

            foreach (var corridor in data.Corridors)
            {
                RenderCorridor(corridor);
                SubscribeForCorridorsEvents(corridor);
                Debug.Log($"[Corridor {corridor}] From=({corridor.PointFrom.x:F1},{corridor.PointFrom.y:F1}) " +
                    $"To=({corridor.PointTo.x:F1},{corridor.PointTo.y:F1}) Width={corridor.Width} Length={corridor.Length} " +
                    $"Doors={corridor.DoorPositions?.Count ?? 0}");
            }

            Debug.Log("[DungeonVisualizer] Generation complete");
        }

        public void ClearAll()
        {
            _floorTilemap?.ClearAllTiles();
            _wallTilemap?.ClearAllTiles();
        }

        private void RenderRoom(RoomData room)
        {
            RenderRoomFloor(room);
            RenderRoomWalls(room);
        }

        private void RenderRoomFloor(RoomData room)
        {
            for (int x = room.GridX; x < room.GridX + room.Width; x++)
            {
                for (int y = room.GridY; y < room.GridY + room.Height; y++)
                {
                    _floorTilemap.SetTile(new Vector3Int(x, y, 0), _floorTile);
                }
            }
        }

        private void RenderRoomWalls(RoomData room)
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

        private void RenderCorridor(CorridorData corridor)
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
            CloseCorridor(corridor);
        }

        private void SubscribeForCorridorsEvents(CorridorData corridor)
        {
            var openEvent = corridor.OnOpened.Subscribe(OpenCorridor);
            var closeEvent = corridor.OnClosed.Subscribe(CloseCorridor);
            _corridorEvents = Disposable.Combine(openEvent, closeEvent);
        }

        private void RenderCorridorWalls(CorridorData corridor, int minX, int maxX, int minY, int maxY)
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

        public void OpenCorridor(CorridorData corridor) => SetCorridorDoors(corridor, null);
        public void CloseCorridor(CorridorData corridor) => SetCorridorDoors(corridor, _doorTile);

        private void SetCorridorDoors(CorridorData corridor, Tile tile)
        {
            if (corridor?.DoorPositions == null)
                return;

            foreach (var doorPos in corridor.DoorPositions)
            {
                _wallTilemap.SetTile(doorPos, tile);
            }
        }

        private void OnDestroy()
        {
            _corridorEvents?.Dispose();
        }
    }
}
