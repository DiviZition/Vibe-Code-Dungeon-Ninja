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
        }

        private void OnRoomCleared(int roomIndex)
        {
            Debug.Log($"Room {roomIndex} cleared!");
            var room = _generator.Data.Rooms[roomIndex];

            OpenRoom(roomIndex);
        }

        public void OpenRoom(int roomIndex) => ApplyToRoomCorridors(roomIndex, _visualizer.OpenCorridor);
        public void CloseRoom(int roomIndex) => ApplyToRoomCorridors(roomIndex, _visualizer.CloseCorridor);

        private void ApplyToRoomCorridors(int roomIndex, System.Action<CorridorData> action)
        {
            var data = _generator.Data;
            if (data.Rooms == null || roomIndex >= data.Rooms.Length)
                return;

            var room = data.Rooms[roomIndex];
            if (room.ConnectedCorridors == null)
                throw new NullReferenceException($"No corridors set for room {roomIndex}");

            foreach (var corridor in room.ConnectedCorridors)
                action(corridor);
        }
    }
}