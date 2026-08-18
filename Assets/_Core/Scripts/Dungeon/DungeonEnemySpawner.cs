using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public struct RoomEnemySpawn
    {
        public int RoomIndex;
        public RoomType RoomType;
        public int Count;
    }

    public class DungeonEnemySpawner
    {
        public List<RoomEnemySpawn> GetSpawnPlan(DungeonData data)
        {
            var plan = new List<RoomEnemySpawn>(data.Rooms.Count);

            for (int i = 0; i < data.Rooms.Count; i++)
            {
                var room = data.Rooms[i];
                int count = room.Type switch
                {
                    RoomType.Start => 0,
                    RoomType.Boss => 1,
                    _ => Random.Range(1, 4)
                };

                plan.Add(new RoomEnemySpawn
                {
                    RoomIndex = i,
                    RoomType = room.Type,
                    Count = count
                });
            }

            return plan;
        }
    }
}