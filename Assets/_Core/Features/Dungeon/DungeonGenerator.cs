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
        [SerializeField] private bool _regenerateOnValidate;
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
                    Debug.Log($"  Room {i}: corridors=[{roomCorridors}], type={room.Type}");
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
                            rooms[i].ConnectedCorridors.Add(corridor);
                            rooms[nearest].ConnectedCorridors.Add(corridor);

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
                                rooms[i].ConnectedCorridors.Add(corridor);
                                rooms[nearest].ConnectedCorridors.Add(corridor);

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

                int corridorStartX = rooms[leftRoom].GridX + rooms[leftRoom].Width;
                int corridorEndX = rooms[rightRoom].GridX;

                int roomALeft = rooms[leftRoom].GridY;
                int roomARight = rooms[leftRoom].GridY + rooms[leftRoom].Height;
                int roomBLeft = rooms[rightRoom].GridY;
                int roomBRight = rooms[rightRoom].GridY + rooms[rightRoom].Height;

                int corridorY = (roomALeft + roomARight + roomBLeft + roomBRight) / 4;

                Vector2 bottomLeft = new Vector2(corridorStartX, corridorY - CorridorWidth / 2);
                Vector2 topRight = new Vector2(corridorEndX, corridorY + CorridorWidth / 2);

                var corridor = new CorridorData(bottomLeft, topRight, CorridorWidth, corridorEndX - corridorStartX, leftRoom, rightRoom);

                for (int y = 0; y < CorridorWidth; y++)
                {
                    corridor.DoorPositions.Add(new Vector3Int(corridorStartX, corridorY - CorridorWidth / 2 + y, 0));
                    corridor.DoorPositions.Add(new Vector3Int(corridorEndX - 1, corridorY - CorridorWidth / 2 + y, 0));
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

                Vector2 bottomLeft = new Vector2(corridorX - CorridorWidth / 2, corridorStartY);
                Vector2 topRight = new Vector2(corridorX + CorridorWidth / 2, corridorEndY);

                var corridor = new CorridorData(bottomLeft, topRight, CorridorWidth, corridorEndY - corridorStartY, bottomRoom, topRoom);

                for (int x = 0; x < CorridorWidth; x++)
                {
                    corridor.DoorPositions.Add(new Vector3Int(corridorX - CorridorWidth / 2 + x, corridorStartY, 0));
                    corridor.DoorPositions.Add(new Vector3Int(corridorX - CorridorWidth / 2 + x, corridorEndY - 1, 0));
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
