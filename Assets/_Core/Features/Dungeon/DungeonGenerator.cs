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
        [SerializeField] private bool _debugLog;

        // Output - the generated dungeon data
        public DungeonData Data { get; private set; }
        public int OddZoneSize => _zoneSize % 2 == 0 ? _zoneSize + 1: _zoneSize;

        public void Generate()
        {
            if (_minRoomSize > OddZoneSize)
                _minRoomSize = OddZoneSize;

            int seed = _seed == 0 ? Random.Range(0, int.MaxValue) : _seed;

            if (_debugLog)
                Debug.Log($"[Generator] Seed={seed}, ZoneSize={OddZoneSize}, Zones={_zonesCount}, CorridorWidth={_corridorWidth}, CorridorLength={_corridorLength}");

            var random = new System.Random(seed);

            Data = new DungeonData
            {
                ZoneSize = OddZoneSize,
                ZonesCount = _zonesCount,
                Seed = seed,
                Rooms = null,
                Corridors = null
            };

            var zonePositions = GenerateZonePositions(random);

            if (_debugLog)
                Debug.Log($"[Generator] Generated {zonePositions.Count} zone positions");

            Data.Rooms = PlaceRoomsInZones(random, zonePositions);

            if (_debugLog)
                Debug.Log($"[Generator] Generated {Data.Rooms.Length} rooms");

            Data.Corridors = GenerateCorridors(zonePositions);

            if (_debugLog)
                Debug.Log($"[Generator] Generated {Data.Corridors.Length} corridors");

            if (_debugLog)
            {
                for (int i = 0; i < Data.Rooms.Length; i++)
                {
                    var room = Data.Rooms[i];
                    var corridors = room.ConnectedCorridorIndices != null ? string.Join(",", room.ConnectedCorridorIndices) : "none";
                    Debug.Log($"  Room {i}: corridors=[{corridors}], type={room.Type}");
                }
            }

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
                    ConnectedCorridorIndices = new int[0],
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
            int roomCount = rooms.Length;

            if (roomCount == 0)
                return corridors.ToArray();

            var component = new int[roomCount];
            for (int i = 0; i < roomCount; i++)
                component[i] = i;

            int expectedDistance = OddZoneSize + _corridorLength;

            for (int i = 0; i < roomCount; i++)
            {
                for (int j = i + 1; j < roomCount; j++)
                {
                    int dx = zonePositions[j].x - zonePositions[i].x;
                    int dy = zonePositions[j].y - zonePositions[i].y;

                    bool isHorizontalAdjacent = dx == expectedDistance && dy == 0;
                    bool isVerticalAdjacent = dx == 0 && dy == expectedDistance;

                    if (isHorizontalAdjacent || isVerticalAdjacent)
                    {
                        var corridor = CreateCorridor(i, j, zonePositions, rooms, isHorizontalAdjacent);
                        if (corridor != null)
                        {
                            corridors.Add(corridor);
                            rooms[i].ConnectedCorridorIndices = AddToArray(rooms[i].ConnectedCorridorIndices, corridors.Count - 1);
                            rooms[j].ConnectedCorridorIndices = AddToArray(rooms[j].ConnectedCorridorIndices, corridors.Count - 1);

                            Union(component, i, j);

                            if (_debugLog)
                                Debug.Log($"  Corridor {corridors.Count - 1} ({(isHorizontalAdjacent ? "H" : "V")}): Room {i} -> Room {j}");
                        }
                    }
                }
            }

            for (int i = 0; i < roomCount; i++)
            {
                if (rooms[i].ConnectedCorridorIndices == null || rooms[i].ConnectedCorridorIndices.Length == 0)
                {
                    if (_debugLog)
                        Debug.Log($"  Room {i} has no corridors - finding fallback...");

                    int nearest = -1;
                    float minDist = float.MaxValue;
                    for (int j = 0; j < roomCount; j++)
                    {
                        if (i == j) continue;
                        float dist = Mathf.Abs(zonePositions[i].x - zonePositions[j].x) + Mathf.Abs(zonePositions[i].y - zonePositions[j].y);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearest = j;
                        }
                    }
                    if (nearest >= 0)
                    {
                        int dx = zonePositions[nearest].x - zonePositions[i].x;
                        int dy = zonePositions[nearest].y - zonePositions[i].y;
                        bool isHorizontal = Mathf.Abs(dx) > Mathf.Abs(dy);

                        if (_debugLog)
                            Debug.Log($"  Room {i} -> nearest Room {nearest}, dx={dx}, dy={dy}, horizontal={isHorizontal}");

                        var corridor = CreateCorridor(i, nearest, zonePositions, rooms, isHorizontal);
                        if (corridor != null)
                        {
                            corridors.Add(corridor);
                            rooms[i].ConnectedCorridorIndices = AddToArray(rooms[i].ConnectedCorridorIndices, corridors.Count - 1);
                            rooms[nearest].ConnectedCorridorIndices = AddToArray(rooms[nearest].ConnectedCorridorIndices, corridors.Count - 1);

                            Union(component, i, nearest);

                            if (_debugLog)
                                Debug.Log($"  Fallback corridor {corridors.Count - 1}: Room {i} -> Room {nearest}");
                        }
                        else
                        {
                            Debug.LogError($"  FAILED to create fallback corridor for Room {i} -> Room {nearest}");
                        }
                    }
                }
            }

            for (int c = 0; c < roomCount; c++)
                component[c] = Find(component, c);

            int rootComponent = Find(component, 0);
            bool madeProgress = true;
            int safetyLimit = 100;
            int iterations = 0;

            while (madeProgress && iterations < safetyLimit)
            {
                iterations++;
                madeProgress = false;
                rootComponent = Find(component, 0);

                for (int i = 1; i < roomCount; i++)
                {
                    if (Find(component, i) != rootComponent)
                    {
                        if (_debugLog)
                            Debug.Log($"  Room {i} disconnected - connecting...");

                        int nearest = -1;
                        float minDist = float.MaxValue;
                        for (int j = 0; j < roomCount; j++)
                        {
                            if (j == i) continue;
                            float dist = Mathf.Abs(zonePositions[i].x - zonePositions[j].x) + Mathf.Abs(zonePositions[i].y - zonePositions[j].y);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                nearest = j;
                            }
                        }
                        if (nearest >= 0)
                        {
                            int dx = zonePositions[nearest].x - zonePositions[i].x;
                            int dy = zonePositions[nearest].y - zonePositions[i].y;
                            bool isHorizontal = Mathf.Abs(dx) > Mathf.Abs(dy);

                            var corridor = CreateCorridor(i, nearest, zonePositions, rooms, isHorizontal);
                            if (corridor != null)
                            {
                                corridors.Add(corridor);
                                rooms[i].ConnectedCorridorIndices = AddToArray(rooms[i].ConnectedCorridorIndices, corridors.Count - 1);
                                rooms[nearest].ConnectedCorridorIndices = AddToArray(rooms[nearest].ConnectedCorridorIndices, corridors.Count - 1);

                                Union(component, i, nearest);
                                madeProgress = true;

                                if (_debugLog)
                                    Debug.Log($"  Bridge corridor: Room {i} -> Room {nearest}");
                            }
                        }
                        break;
                    }
                }
            }

            if (iterations >= safetyLimit)
                Debug.LogError("Dungeon generation: hit safety limit in connectivity check!");

            return corridors.ToArray();
        }

        private int Find(int[] parent, int x)
        {
            if (parent[x] != x)
                parent[x] = Find(parent, parent[x]);
            return parent[x];
        }

        private void Union(int[] parent, int x, int y)
        {
            int px = Find(parent, x);
            int py = Find(parent, y);
            if (px != py)
                parent[px] = py;
        }

        private Corridor CreateCorridor(int roomA, int roomB, List<Vector2Int> zonePositions, Room[] rooms, bool isHorizontal)
        {
            int dx = zonePositions[roomB].x - zonePositions[roomA].x;
            int dy = zonePositions[roomB].y - zonePositions[roomA].y;

            if (isHorizontal)
            {
                bool aIsLeft = dx < 0;
                int leftRoom = aIsLeft ? roomB : roomA;
                int rightRoom = aIsLeft ? roomA : roomB;

                int corridorStartX = rooms[leftRoom].GridX + rooms[leftRoom].Width;
                int corridorEndX = rooms[rightRoom].GridX;

                int roomALeft = rooms[leftRoom].GridY;
                int roomARight = rooms[leftRoom].GridY + rooms[leftRoom].Height;
                int roomBLeft = rooms[rightRoom].GridY;
                int roomBRight = rooms[rightRoom].GridY + rooms[rightRoom].Height;

                int corridorY = (roomALeft + roomARight + roomBLeft + roomBRight) / 4;

                Vector2 bottomLeft = new Vector2(corridorStartX, corridorY - _corridorWidth / 2);
                Vector2 topRight = new Vector2(corridorEndX, corridorY + _corridorWidth / 2);

                var corridor = new Corridor
                {
                    PointFrom = bottomLeft,
                    PointTo = topRight,
                    FromRoomIndex = leftRoom,
                    ToRoomIndex = rightRoom,
                    CorridorWidth = _corridorWidth,
                    CorridorLength = corridorEndX - corridorStartX
                };

                corridor.DoorPositions = new List<Vector3Int>();
                for (int y = 0; y < _corridorWidth; y++)
                {
                    corridor.DoorPositions.Add(new Vector3Int(corridorStartX, corridorY - _corridorWidth / 2 + y, 0));
                    corridor.DoorPositions.Add(new Vector3Int(corridorEndX - 1, corridorY - _corridorWidth / 2 + y, 0));
                }

                return corridor;
            }
            else
            {
                bool aIsBottom = dy < 0;
                int bottomRoom = aIsBottom ? roomB : roomA;
                int topRoom = aIsBottom ? roomA : roomB;

                int corridorStartY = rooms[bottomRoom].GridY + rooms[bottomRoom].Height;
                int corridorEndY = rooms[topRoom].GridY;

                int roomABottom = rooms[bottomRoom].GridX;
                int roomATop = rooms[bottomRoom].GridX + rooms[bottomRoom].Width;
                int roomBBottom = rooms[topRoom].GridX;
                int roomBTop = rooms[topRoom].GridX + rooms[topRoom].Width;

                int corridorX = (roomABottom + roomATop + roomBBottom + roomBTop) / 4;

                Vector2 bottomLeft = new Vector2(corridorX - _corridorWidth / 2, corridorStartY);
                Vector2 topRight = new Vector2(corridorX + _corridorWidth / 2, corridorEndY);

                var corridor = new Corridor
                {
                    PointFrom = bottomLeft,
                    PointTo = topRight,
                    FromRoomIndex = bottomRoom,
                    ToRoomIndex = topRoom,
                    CorridorWidth = _corridorWidth,
                    CorridorLength = corridorEndY - corridorStartY
                };

                corridor.DoorPositions = new List<Vector3Int>();
                for (int x = 0; x < _corridorWidth; x++)
                {
                    corridor.DoorPositions.Add(new Vector3Int(corridorX - _corridorWidth / 2 + x, corridorStartY, 0));
                    corridor.DoorPositions.Add(new Vector3Int(corridorX - _corridorWidth / 2 + x, corridorEndY - 1, 0));
                }

                return corridor;
            }
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
    }
}
