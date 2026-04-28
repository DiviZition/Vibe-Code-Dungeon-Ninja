using UnityEngine;
using R3;
using System;

namespace Dungeon
{
    public class DungeonManager : MonoBehaviour
    {
        [SerializeField] private DungeonGenerator _generator;
        [SerializeField] private DungeonVisualizer _visualizer;

        IDisposable _roomEnemiesAppearanceEvents;

        [ContextMenu("Generate Dungeon")]
        public void GenerateDungeon()
        {
            _generator.Generate();
            _visualizer.Visualize(_generator.Data);
        }

        public void OpenRoom(int roomIndex) => _generator.Data.OpenRoomCorridors(roomIndex);
        public void CloseRoom(int roomIndex) => _generator.Data.CloseRoomCorridors(roomIndex);

        private void OnDestroy()
        {
            _roomEnemiesAppearanceEvents?.Dispose();
        }
    }
}