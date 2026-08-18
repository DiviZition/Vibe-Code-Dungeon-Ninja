using Cysharp.Threading.Tasks;
using Dungeon;
using System;

public class DungeonGameDirector : IDisposable
{
    private DungeonPresenter _dungeonPresenter;
    private DungeonEnemySpawner _enemySpawner;

    public DungeonGameDirector()
    {
        _dungeonPresenter = new DungeonPresenter();
        _enemySpawner = new DungeonEnemySpawner();
    }

    public async UniTaskVoid StartGame()
    {
        await _dungeonPresenter.GenerateDungeon();
    }

    public void Dispose()
    {
        _dungeonPresenter.Dispose();
    }
}

public class DungeonEnemySpawner
{

}
