using UnityEngine;

namespace Dungeon
{
    public enum RoomType
    {
        Normal,
        Boss,
    }

    public class Room
    {
        // Grid position (top-left corner)
        public int GridX { get; set; }
        public int GridY { get; set; }

        // Room size in grid units
        public int Width { get; set; }
        public int Height { get; set; }

        public RoomType Type { get; set; }

        // Connections to other rooms (indices in DungeonData.Rooms)
        public int[] ConnectedRoomIndices { get; set; }

        // Center position for corridor connections
        public (int x, int y) Center => (GridX + Width / 2, GridY + Height / 2);

        // Zone coordinates
        public int ZoneX { get; set; }
        public int ZoneY { get; set; }

        // Game state flags
        public bool IsVisited { get; set; }
        public bool IsCleared { get; set; }
        public bool IsLocked { get; set; }
    }

    public class Corridor
    {
        // Corridor position (top-left corner)
        public Vector2 PointFrom { get; set; }
        public Vector2 PointTo { get; set; }

        public int CorridorWidth { get; set; }
        public int CorridorLength { get; set; }
        
        // Indices of rooms this corridor connects
        public int FromRoomIndex { get; set; }
        public int ToRoomIndex { get; set; }
    }

    public class DungeonData
    {
        public Room[] Rooms { get; set; }
        public Corridor[] Corridors { get; set; }

        // Zone configuration
        public int ZoneSize { get; set; }
        public int ZonesCount { get; set; }

        // Seed for reproducible generation
        public int Seed { get; set; }
    }
}
