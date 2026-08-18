using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Dungeon
{
    public class DungeonFacade : IDungeonFacade
    {
        private readonly DungeonEnemySpawner _enemySpawner;

        public IDungeonModel Model { get; }
        public DungeonData Data => Model.Data;

        public DungeonFacade(IDungeonModel model, DungeonEnemySpawner enemySpawner)
        {
            Model = model;
            _enemySpawner = enemySpawner;
        }

        public UniTask GenerateDungeon()
        {
            Model.Generate();
            return UniTask.CompletedTask;
        }

        public IReadOnlyList<RoomEnemySpawn> GetEnemySpawnPlan()
        {
            if (Data == null)
                throw new System.InvalidOperationException("Dungeon must be generated before getting an enemy spawn plan.");

            return _enemySpawner.GetSpawnPlan(Data);
        }

        public void AddEnemyToRoom(int roomIndex, Enemy.IEnemy enemy) => Model.AddEnemyToRoom(roomIndex, enemy);
        public void RemoveEnemyFromRoom(int roomIndex, Enemy.IEnemy enemy) => Model.RemoveEnemyFromRoom(roomIndex, enemy);

        public void Dispose()
        {
            Model?.Dispose();
        }
    }
}