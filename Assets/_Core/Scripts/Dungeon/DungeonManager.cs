using UnityEngine;
using R3;
using System;

namespace Dungeon
{
    public class DungeonManager : MonoBehaviour
    {
        [SerializeField] private DungeonVisualizer _visualizer;
        [field: SerializeField] public DungeonGenerator Generator;

        IDisposable _roomEnemiesAppearanceEvents;

        [ContextMenu("Generate Dungeon")]
        public void GenerateDungeon()
        {
            Generator.Generate();
            _visualizer.Visualize(Generator.Data);
        }

        public void OpenRoom(int roomIndex) => Generator.Data.OpenRoomCorridors(roomIndex);
        public void CloseRoom(int roomIndex) => Generator.Data.CloseRoomCorridors(roomIndex);

        private void OnDestroy()
        {
            _roomEnemiesAppearanceEvents?.Dispose();
        }
    }
}