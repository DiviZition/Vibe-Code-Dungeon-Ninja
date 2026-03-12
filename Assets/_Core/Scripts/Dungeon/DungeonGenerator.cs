using UnityEngine;
using System.Collections.Generic;

namespace Dungeon
{
    // Helper struct for zone positions
    public struct ZonePosition
    {
        public int X;
        public int Y;

        public ZonePosition(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int _zoneSize = 10;
        [SerializeField, Range(1, 10)] private int _zonesCount = 5;
        [SerializeField, Range(1, 10)] private int _minRoomSize = 3;
        [SerializeField] private int _corridorWidth = 1;
        [SerializeField] private int _seed;

        [Header("Debug")]
        [SerializeField] private bool _regenerateOnValidate;

        // Output - the generated dungeon data
        public DungeonData Data { get; private set; }

        public void Generate()
        {
            // Validate _minRoomSize
            if (_minRoomSize > _zoneSize)
                _minRoomSize = _zoneSize;

            if (_seed == 0)
                _seed = Random.Range(0, int.MaxValue);

            var random = new System.Random(_seed);

            Data = new DungeonData
            {
                ZoneSize = _zoneSize,
                ZonesCount = _zonesCount,
                Seed = _seed,
                Rooms = null,
                Corridors = null
            };

            // Phase 1: Generate zone skeleton via random walk
            var zonePositions = GenerateZonePositions(random);

            // Phase 2: Place rooms in each zone
            Data.Rooms = PlaceRoomsInZones(random, zonePositions);

            // Phase 3: Generate corridors between adjacent zones
            Data.Corridors = GenerateCorridors(zonePositions);

            // Phase 4: Determine boss room (farthest from start)
            DetermineBossRoom();
        }

        private List<ZonePosition> GenerateZonePositions(System.Random random)
        {
            var zones = new List<ZonePosition>();
            zones.Add(new ZonePosition(0, 0));

            int maxAttempts = _zonesCount * 100;

            while (zones.Count < _zonesCount)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < maxAttempts)
                {
                    attempts++;

                    // Pick random existing zone to grow from
                    int parentIndex = random.Next(zones.Count);
                    var parent = zones[parentIndex];

                    // Pick random direction: 0=up, 1=down, 2=left, 3=right
                    int direction = random.Next(4);

                    int newX = parent.X;
                    int newY = parent.Y;

                    switch (direction)
                    {
                        case 0: newY += 1; break;  // up
                        case 1: newY -= 1; break;  // down
                        case 2: newX -= 1; break;  // left
                        case 3: newX += 1; break;  // right
                    }

                    // Check if position already occupied
                    bool occupied = false;
                    foreach (var existing in zones)
                    {
                        if (existing.X == newX && existing.Y == newY)
                        {
                            occupied = true;
                            break;
                        }
                    }

                    if (!occupied)
                    {
                        zones.Add(new ZonePosition(newX, newY));
                        placed = true;
                    }
                }

                if (!placed)
                {
                    Debug.LogWarning($"Could not place zone {zones.Count} after {maxAttempts} attempts");
                    break;
                }
            }

            return zones;
        }

        private Room[] PlaceRoomsInZones(System.Random random, List<ZonePosition> zonePositions)
        {
            var rooms = new List<Room>();

            for (int i = 0; i < zonePositions.Count; i++)
            {
                var zone = zonePositions[i];

                // Random room size between _minRoomSize and _zoneSize
                int width = random.Next(_minRoomSize, _zoneSize + 1);
                int height = random.Next(_minRoomSize, _zoneSize + 1);

                // Room centered in zone
                int zoneCenterX = zone.X * _zoneSize + _zoneSize / 2;
                int zoneCenterY = zone.Y * _zoneSize + _zoneSize / 2;

                int roomX = zoneCenterX - width / 2;
                int roomY = zoneCenterY - height / 2;

                var room = new Room
                {
                    GridX = roomX,
                    GridY = roomY,
                    Width = width,
                    Height = height,
                    ZoneX = zone.X,
                    ZoneY = zone.Y,
                    Type = RoomType.Normal,
                    ConnectedRoomIndices = new int[0],
                    IsVisited = false,
                    IsCleared = false,
                    IsLocked = false
                };

                rooms.Add(room);
            }

            return rooms.ToArray();
        }

        private Corridor[] GenerateCorridors(List<ZonePosition> zonePositions)
        {
            var corridors = new List<Corridor>();
            var rooms = Data.Rooms;

            if (rooms == null || rooms.Length == 0)
                return corridors.ToArray();

            // Connect all adjacent zones
            for (int i = 0; i < zonePositions.Count; i++)
            {
                var zoneA = zonePositions[i];

                for (int j = i + 1; j < zonePositions.Count; j++)
                {
                    var zoneB = zonePositions[j];

                    // Check if adjacent in zone coordinates
                    int dx = Mathf.Abs(zoneA.X - zoneB.X);
                    int dy = Mathf.Abs(zoneA.Y - zoneB.Y);

                    bool isAdjacent = (dx == 1 && dy == 0) || (dx == 0 && dy == 1);

                    if (isAdjacent)
                    {
                        // Calculate zone centers
                        int centerAX = zoneA.X * _zoneSize + _zoneSize / 2;
                        int centerAY = zoneA.Y * _zoneSize + _zoneSize / 2;
                        int centerBX = zoneB.X * _zoneSize + _zoneSize / 2;
                        int centerBY = zoneB.Y * _zoneSize + _zoneSize / 2;

                        bool isHorizontal = (centerAY == centerBY);

                        int corridorX, corridorY, corridorWidth, corridorHeight;

                        if (isHorizontal)
                        {
                            corridorX = Mathf.Min(centerAX, centerBX) - _corridorWidth / 2;
                            corridorY = centerAY - _corridorWidth / 2;
                            corridorWidth = Mathf.Abs(centerBX - centerAX) + _corridorWidth;
                            corridorHeight = _corridorWidth;
                        }
                        else
                        {
                            corridorX = centerAX - _corridorWidth / 2;
                            corridorY = Mathf.Min(centerAY, centerBY) - _corridorWidth / 2;
                            corridorWidth = _corridorWidth;
                            corridorHeight = Mathf.Abs(centerBY - centerAY) + _corridorWidth;
                        }

                        var corridor = new Corridor
                        {
                            GridX = corridorX,
                            GridY = corridorY,
                            Width = corridorWidth,
                            Height = corridorHeight,
                            FromRoomIndex = i,
                            ToRoomIndex = j
                        };

                        corridors.Add(corridor);

                        // Update room connections
                        rooms[i].ConnectedRoomIndices = AddToArray(rooms[i].ConnectedRoomIndices, j);
                        rooms[j].ConnectedRoomIndices = AddToArray(rooms[j].ConnectedRoomIndices, i);
                    }
                }
            }

            return corridors.ToArray();
        }

        private int[] AddToArray(int[] array, int value)
        {
            var list = new List<int>(array) { value };
            return list.ToArray();
        }

        private void DetermineBossRoom()
        {
            if (Data.Rooms == null || Data.Rooms.Length == 0)
                return;

            var startRoom = Data.Rooms[0];
            int maxDistance = -1;
            int bossIndex = 0;

            for (int i = 1; i < Data.Rooms.Length; i++)
            {
                var room = Data.Rooms[i];
                int distance = Mathf.Abs(room.ZoneX - startRoom.ZoneX) + Mathf.Abs(room.ZoneY - startRoom.ZoneY);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    bossIndex = i;
                }
            }

            Data.Rooms[bossIndex].Type = RoomType.Boss;
        }

        // Expose for Unity Inspector regeneration
        private void OnValidate()
        {
            if (_minRoomSize > _zoneSize)
                _minRoomSize = _zoneSize;

            if (_regenerateOnValidate)
                Generate();
        }
    }
}
