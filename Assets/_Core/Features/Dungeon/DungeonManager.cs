using UnityEngine;
using R3;
using System;

namespace Dungeon
{
    public class DungeonManager : MonoBehaviour
    {
        [SerializeField] private DungeonGenerator _generator;
        [SerializeField] private DungeonVisualizer _visualizer;

        [ContextMenu("Generate Dungeon")]
        public void GenerateDungeon()
        {
            _generator.Generate();
            var data = _generator.Data;

            WireRoomEvents(data);
            _visualizer.Visualize(data);
        }

        private void WireRoomEvents(DungeonData data)
        {
            if (data.Rooms == null)
                return;

            for (int i = 0; i < data.Rooms.Length; i++)
            {
                var room = data.Rooms[i];
                int roomIndex = i;

                room.OnAllEnemiesCleared = () => OnRoomCleared(roomIndex);
            }

            if (data.Corridors == null)
                return;

            for (int i = 0; i < data.Corridors.Length; i++)
            {
                var corridor = data.Corridors[i];
                int corridorIndex = i;

                corridor.OnOpenAllDoors = () => _visualizer.OpenDoors(corridorIndex);
            }
        }

        private void OnRoomCleared(int roomIndex)
        {
            Debug.Log($"Room {roomIndex} cleared!");
            var data = _generator.Data;
            var room = data.Rooms[roomIndex];

            foreach (var corridorIndex in room.ConnectedCorridorIndices)
            {
                data.Corridors[corridorIndex].OpenAllDoors();
            }
        }

        public void OpenRoom(int roomIndex) => ApplyToRoomCorridors(roomIndex, _visualizer.OpenDoors);
        public void CloseRoom(int roomIndex) => ApplyToRoomCorridors(roomIndex, _visualizer.CloseDoors);

        private void ApplyToRoomCorridors(int roomIndex, System.Action<int> action)
        {
            var data = _generator.Data;
            if (data.Rooms == null || roomIndex >= data.Rooms.Length)
                return;

            var room = data.Rooms[roomIndex];
            if (room.ConnectedCorridorIndices == null)
                return;

            foreach (var corridorIndex in room.ConnectedCorridorIndices)
            {
                action(corridorIndex);
            }
        }
    }
}