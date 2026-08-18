using System;
using R3;

namespace Dungeon
{
    public interface IDungeonModel : Core.IModel, IDisposable
    {
        DungeonData Data { get; }
        Subject<DungeonData> OnGenerated { get; }

        void Generate();
        void CloseRoom(int roomIndex);
        void OpenRoom(int roomIndex);
        void AddEnemyToRoom(int roomIndex, Enemy.IEnemy enemy);
        void RemoveEnemyFromRoom(int roomIndex, Enemy.IEnemy enemy);
    }
}