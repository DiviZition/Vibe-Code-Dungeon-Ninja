using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Dungeon
{
    public interface IDungeonFacade : IDisposable
    {
        IDungeonModel Model { get; }
        DungeonData Data { get; }

        UniTask GenerateDungeon();
        IReadOnlyList<RoomEnemySpawn> GetEnemySpawnPlan();
        void AddEnemyToRoom(int roomIndex, Enemy.IEnemy enemy);
        void RemoveEnemyFromRoom(int roomIndex, Enemy.IEnemy enemy);
    }
}