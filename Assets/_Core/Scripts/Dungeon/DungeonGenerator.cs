using UnityEngine;
using System.Collections.Generic;

namespace Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int _zoneSize = 10;
        [SerializeField] private int _minRoomSize = 3;
        [SerializeField, Range(1, 10)] private int _zonesCount = 5;
        [SerializeField] private int _corridorWidth = 1;
        [SerializeField] private int _corridorLength = 1;
        [SerializeField] private int _seed;

        [Header("Debug")]
        [SerializeField] private bool _regenerateOnValidate;

        // Output - the generated dungeon data
        public DungeonData Data { get; private set; }
        public int OddZoneSize => _zoneSize % 2 == 0 ? _zoneSize + 1: _zoneSize;

        public void Generate()
        {
            // Validate _minRoomSize
            if (_minRoomSize > OddZoneSize)
                _minRoomSize = OddZoneSize;

            int seed = _seed == 0 ? Random.Range(0, int.MaxValue) : _seed;

            var random = new System.Random(seed);

            Data = new DungeonData
            {
                ZoneSize = OddZoneSize,
                ZonesCount = _zonesCount,
                Seed = seed,
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

        private List<Vector2Int> GenerateZonePositions(System.Random random)
        {
            var zones = new List<Vector2Int>();
            zones.Add(new Vector2Int(0, 0));

            int maxAttempts = _zonesCount * 100;
            int zoneHalf = OddZoneSize / 2;

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

                    int newX = parent.x;
                    int newY = parent.y;

                    switch (direction)
                    {
                        case 0: newY += OddZoneSize + _corridorLength; break;  // up
                        case 1: newY -= OddZoneSize + _corridorLength; break;  // down
                        case 2: newX -= OddZoneSize + _corridorLength; break;  // left
                        case 3: newX += OddZoneSize + _corridorLength; break;  // right
                    }

                    // Check if position already occupied
                    bool occupied = false;
                    foreach (var existing in zones)
                    {
                        if (existing.x == newX && existing.y == newY)
                        {
                            occupied = true;
                            break;
                        }
                    }

                    if (!occupied)
                    {
                        zones.Add(new Vector2Int(newX, newY));
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

        private Room[] PlaceRoomsInZones(System.Random random, List<Vector2Int> zonePositions)
        {
            var rooms = new List<Room>();

            for (int i = 0; i < zonePositions.Count; i++)
            {
                var zone = zonePositions[i];

                // Random room size between _minRoomSize and _zoneSize
                int width = random.Next(_minRoomSize, OddZoneSize);
                int height = random.Next(_minRoomSize, OddZoneSize);

                int roomX = zone.x - width / 2;
                int roomY = zone.y - height / 2;

                var room = new Room
                {
                    GridX = roomX,
                    GridY = roomY,
                    Width = width,
                    Height = height,
                    ZoneX = zone.x,
                    ZoneY = zone.y,
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

        private Corridor[] GenerateCorridors(List<Vector2Int> zonePositions)
        {
            var corridors = new List<Corridor>();
            var rooms = Data.Rooms;

            if (rooms == null || rooms.Length == 0)
                return corridors.ToArray();

            // Connect all adjacent zones
            Debug.Log($"Distance between rooms: {OddZoneSize + _corridorLength}");
            for (int i = 0; i < zonePositions.Count; i++)
            {
                var zoneA = zonePositions[i];

                for (int j = i + 1; j < zonePositions.Count; j++)
                {
                    var zoneB = zonePositions[j];

                    // Check if adjacent in zone coordinates
                    int dx = Mathf.Abs(zoneA.x - zoneB.x);
                    int dy = Mathf.Abs(zoneA.y - zoneB.y);

                    bool isAdjacent = (dx == OddZoneSize + _corridorLength && dy == 0) || (dx == 0 && dy == OddZoneSize + _corridorLength);
                    bool isHorizontal = zoneA.x == zoneB.x;
                    bool isVertical = zoneA.y == zoneB.y;

                    int corridorHalfWidth = _corridorWidth / 2;

                    if (isAdjacent && isHorizontal)
                    {
                        //Imagine zones from lower to higher coordinates
                        if (zoneA.x > zoneB.x)
                        {
                            Vector2Int tempZonePos = zoneA;
                            zoneA = zoneB;
                            zoneB = tempZonePos;
                        }

                        Vector2 bottomLeft = zoneA;
                        bottomLeft.x += _zoneSize / 2;
                        bottomLeft.y -= corridorHalfWidth;
                        Vector2 topRight;
                        topRight.x = bottomLeft.x + _corridorLength + 1;
                        topRight.y = bottomLeft.y + _corridorWidth;

                        var corridor = new Corridor
                        {
                            PointFrom = bottomLeft,
                            PointTo = topRight,
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
            if (_minRoomSize > OddZoneSize)
                _minRoomSize = OddZoneSize;

            if (_regenerateOnValidate)
                Generate();
        }
    }
}
