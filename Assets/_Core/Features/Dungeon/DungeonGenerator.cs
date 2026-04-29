using UnityEngine;
using System.Collections.Generic;
using Enemy;

namespace Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        public int ZoneSize = 15;
        public int MinRoomSize = 6;
        [Range(1, 20)] public int ZonesCount = 10;
        public int CorridorWidth = 2;
        public int CorridorLength = 1;
        public int Seed = 0;

        [Header("Debug")]
        [SerializeField] private bool _debugLog;

        // Output - the generated dungeon data
        public DungeonData Data { get; private set; }
        public int OddZoneSize => ZoneSize % 2 == 0 ? ZoneSize + 1: ZoneSize;

        public void Generate()
        {
            if (MinRoomSize > OddZoneSize)
                MinRoomSize = OddZoneSize;

            int seed = Seed == 0 ? Random.Range(0, int.MaxValue) : Seed;

            Log($"[Generator] Seed={seed}, ZoneSize={OddZoneSize}, Zones={ZonesCount}, CorridorWidth={CorridorWidth}, CorridorLength={CorridorLength}");

            var random = new System.Random(seed);
            var zonePositions = GenerateZonePositions(random);
            List<RoomData> rooms = PlaceRoomsInZones(random, zonePositions);
            List<CorridorData> corridors = GenerateCorridors(rooms, zonePositions);

            Data = new DungeonData(rooms, corridors, ZoneSize, ZonesCount, Seed);

            if (_debugLog)
            {
                for (int i = 0; i < Data.Rooms.Count; i++)
                {
                    var room = Data.Rooms[i];
                    var roomCorridors = room.ConnectedCorridors != null ? string.Join(",", room.ConnectedCorridors) : "none";
                }
            }

            DetermineBossRoom();
        }

        private List<Vector2Int> GenerateZonePositions(System.Random random)
        {
            var zones = new List<Vector2Int>();
            zones.Add(new Vector2Int(0, 0));

            int maxAttempts = ZonesCount * 100;
            int zoneHalf = OddZoneSize / 2;

            while (zones.Count < ZonesCount)
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
                        case 0: newY += OddZoneSize + CorridorLength; break;  // up
                        case 1: newY -= OddZoneSize + CorridorLength; break;  // down
                        case 2: newX -= OddZoneSize + CorridorLength; break;  // left
                        case 3: newX += OddZoneSize + CorridorLength; break;  // right
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

        private List<RoomData> PlaceRoomsInZones(System.Random random, List<Vector2Int> zonePositions)
        {
            var rooms = new List<RoomData>();

            for (int i = 0; i < zonePositions.Count; i++)
            {
                var zone = zonePositions[i];

                // Random room size between _minRoomSize and _zoneSize
                int width = random.Next(MinRoomSize, OddZoneSize);
                int height = random.Next(MinRoomSize, OddZoneSize);

                int roomX = zone.x - width / 2;
                int roomY = zone.y - height / 2;

                var room = new RoomData(new Vector2Int(roomX, roomY), width, height);

                rooms.Add(room);
            }

            return rooms;
        }

        private List<CorridorData> GenerateCorridors(List<RoomData> rooms, List<Vector2Int> zonePositions)
        {
            var corridors = new List<CorridorData>();
            int roomCount = rooms.Count;

            if (roomCount == 0)
                return corridors;

            var component = new int[roomCount];
            for (int i = 0; i < roomCount; i++)
                component[i] = i;

            int expectedDistance = OddZoneSize + CorridorLength;

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
                            rooms[i].ConnectedCorridors.Add(corridor);
                            rooms[j].ConnectedCorridors.Add(corridor);

                            Union(component, i, j);

                            if (_debugLog)
                                Debug.Log($"  Corridor {corridors.Count - 1} ({(isHorizontalAdjacent ? "H" : "V")}): Room {i} -> Room {j}");
                        }
                    }
                }
            }

            for (int i = 0; i < roomCount; i++)
            {
                if (rooms[i].ConnectedCorridors == null || rooms[i].ConnectedCorridors.Count == 0)
                {
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

                        var corridor = CreateCorridor(i, nearest, zonePositions, rooms, isHorizontal);
                        if (corridor != null)
                        {
                            corridors.Add(corridor);
                            rooms[i].ConnectedCorridors.Add(corridor);
                            rooms[nearest].ConnectedCorridors.Add(corridor);

                            Union(component, i, nearest);
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
                                rooms[i].ConnectedCorridors.Add(corridor);
                                rooms[nearest].ConnectedCorridors.Add(corridor);

                                Union(component, i, nearest);
                                madeProgress = true;
                            }
                        }
                        break;
                    }
                }
            }

            if (iterations >= safetyLimit)
                Debug.LogError("Dungeon generation: hit safety limit in connectivity check!");

            return corridors;
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

        private CorridorData CreateCorridor(int roomA, int roomB, List<Vector2Int> zonePositions, List<RoomData> rooms, bool isHorizontal)
        {
            int dx = zonePositions[roomB].x - zonePositions[roomA].x;
            int dy = zonePositions[roomB].y - zonePositions[roomA].y;

            if (isHorizontal)
            {
                bool aIsLeft = dx < 0;
                int leftRoom = aIsLeft ? roomB : roomA;
                int rightRoom = aIsLeft ? roomA : roomB;

                // Calculate where corridor should be (between rooms)
                int corridorStartX = rooms[leftRoom].GridX + rooms[leftRoom].Width;
                int corridorEndX = rooms[rightRoom].GridX;

                // Log for debugging
                if (_debugLog)
                    Debug.Log($"Horizontal corridor: leftRoom[{leftRoom}] right edge={corridorStartX}, rightRoom[{rightRoom}] left edge={corridorEndX}, gap={corridorEndX - corridorStartX}");

                // If rooms overlap or gap is too small, skip this corridor
                if (corridorStartX >= corridorEndX)
                {
                    Debug.LogWarning($"Corridor gap is {corridorEndX - corridorStartX} (invalid) between Room {leftRoom} (right edge={corridorStartX}) and Room {rightRoom} (left edge={corridorEndX}). Skipping.");
                    return null;
                }

                int roomALeft = rooms[leftRoom].GridY;
                int roomARight = rooms[leftRoom].GridY + rooms[leftRoom].Height;
                int roomBLeft = rooms[rightRoom].GridY;
                int roomBRight = rooms[rightRoom].GridY + rooms[rightRoom].Height;

                int corridorY = (roomALeft + roomARight + roomBLeft + roomBRight) / 4;

                int corridorStartY = corridorY - CorridorWidth / 2;
                Vector2 bottomLeft = new Vector2(corridorStartX, corridorStartY);
                Vector2 topRight = new Vector2(corridorEndX, corridorStartY + CorridorWidth);

                var corridor = new CorridorData(bottomLeft, topRight, CorridorWidth, corridorEndX - corridorStartX, true, leftRoom, rightRoom);

                for (int y = 0; y < CorridorWidth; y++)
                 {
                     corridor.DoorPositions.Add(new Vector3Int(corridorStartX, corridorStartY + y, 0));      // Left room east wall
                     corridor.DoorPositions.Add(new Vector3Int(corridorEndX - 1, corridorStartY + y, 0));  // Right room west wall
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

                // Adjust corridor to ensure it doesn't extend into rooms
                corridorStartY = Mathf.Max(corridorStartY, rooms[bottomRoom].GridY + rooms[bottomRoom].Height);
                corridorEndY = Mathf.Min(corridorEndY, rooms[topRoom].GridY);

                // Validate corridor has positive length
                if (corridorStartY >= corridorEndY)
                {
                    Debug.LogWarning($"Corridor length is invalid ({corridorEndY - corridorStartY}) between Room {bottomRoom} and Room {topRoom}. Skipping.");
                    return null;
                }

                int roomABottom = rooms[bottomRoom].GridX;
                int roomATop = rooms[bottomRoom].GridX + rooms[bottomRoom].Width;
                int roomBBottom = rooms[topRoom].GridX;
                int roomBTop = rooms[topRoom].GridX + rooms[topRoom].Width;

                int corridorX = (roomABottom + roomATop + roomBBottom + roomBTop) / 4;

                int corridorStartX = corridorX - CorridorWidth / 2;
                Vector2 bottomLeft = new Vector2(corridorStartX, corridorStartY);
                Vector2 topRight = new Vector2(corridorStartX + CorridorWidth, corridorEndY);

                var corridor = new CorridorData(bottomLeft, topRight, CorridorWidth, corridorEndY - corridorStartY, false, bottomRoom, topRoom);

                for (int x = 0; x < CorridorWidth; x++)
                 {
                     corridor.DoorPositions.Add(new Vector3Int(corridorStartX + x, corridorStartY, 0));      // Bottom room north wall
                     corridor.DoorPositions.Add(new Vector3Int(corridorStartX + x, corridorEndY - 1, 0));  // Top room south wall
                   }

                return corridor;
            }
        }

        private void DetermineBossRoom()
        {
            if (Data.Rooms == null || Data.Rooms.Count == 0)
                return;

            var startRoom = Data.Rooms[0];
            int maxDistance = -1;
            int bossIndex = 0;

            for (int i = 1; i < Data.Rooms.Count; i++)
            {
                var room = Data.Rooms[i];
                int distance = Mathf.Abs(room.GridX - startRoom.GridX) + Mathf.Abs(room.GridY - startRoom.GridY);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    bossIndex = i;
                }
            }

            Data.Rooms[bossIndex].SetRoomType(RoomType.Boss);
        }

        private void Log(string message)
        {
            if (_debugLog == true)
                Debug.Log(message);
        }   
    }
}
