using UnityEngine;

namespace Dungeon
{
    [CreateAssetMenu(fileName = "Dungeon Generation Config", menuName = "Configs/DungeonGeneration")]
    public class DungeonGeneratorConfig : ScriptableObject
    {
        [Header("Generation Settings")]
        public int ZoneSize = 15;
        public int MinRoomSize = 6;
        [Range(1, 20)] public int ZonesCount = 10;
        public int CorridorWidth = 2;
        public int CorridorLength = 1;
        public int Seed = 0;

        [Header("Debug")]
        public bool IsDebugging;
    }
}
