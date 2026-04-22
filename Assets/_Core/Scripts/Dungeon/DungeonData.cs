using UnityEngine;
using System.Collections.Generic;

namespace Dungeon
{
    public enum RoomType
    {
        Normal,
        Boss,
    }

    public class Room
    {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public RoomType Type { get; set; }

        public int[] ConnectedCorridorIndices { get; set; }

        public (int x, int y) Center => (GridX + Width / 2, GridY + Height / 2);

        public int ZoneX { get; set; }
        public int ZoneY { get; set; }

        public bool IsVisited { get; set; }
        public bool IsCleared { get; set; }
        public bool IsLocked { get; set; }

        private int _enemyCount;
        public int EnemyCount
        {
            get => _enemyCount;
            set
            {
                int previous = _enemyCount;
                _enemyCount = value;
                OnEnemyCountChanged(previous, _enemyCount);
            }
        }

        public System.Action OnAllEnemiesCleared;
        private bool _hasHadEnemies;

        public void AddEnemy()
        {
            if (_enemyCount == 0 && _hasHadEnemies)
            {
                _enemyCount = 1;
                return;
            }

            bool wasEmpty = _enemyCount == 0;
            _enemyCount++;
            _hasHadEnemies = true;

            if (wasEmpty)
            {
                OnFirstEnemyAdded();
            }
        }

        public void RemoveEnemy()
        {
            if (_enemyCount <= 0)
                return;

            int previous = _enemyCount;
            _enemyCount--;

            if (previous > 0 && _enemyCount == 0)
            {
                OnAllEnemiesCleared?.Invoke();
            }
        }

        private void OnEnemyCountChanged(int previous, int current) { }

        private void OnFirstEnemyAdded()
        {
            Debug.Log($"Room ({GridX},{GridY}): First enemy added - closing doors");
        }
    }

    public class Corridor
    {
        public Vector2 PointFrom { get; set; }
        public Vector2 PointTo { get; set; }

        public int CorridorWidth { get; set; }
        public int CorridorLength { get; set; }

        public int FromRoomIndex { get; set; }
        public int ToRoomIndex { get; set; }

        public List<Vector3Int> DoorPositions { get; set; } = new List<Vector3Int>();

        public System.Action OnOpenAllDoors;

        public void OpenAllDoors()
        {
            OnOpenAllDoors?.Invoke();
        }
    }

    public class DungeonData
    {
        public Room[] Rooms { get; set; }
        public Corridor[] Corridors { get; set; }

        public int ZoneSize { get; set; }
        public int ZonesCount { get; set; }

        public int Seed { get; set; }
    }
}
