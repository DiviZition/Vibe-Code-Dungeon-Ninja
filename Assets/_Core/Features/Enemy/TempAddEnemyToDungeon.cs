using Dungeon;
using Enemy;
using UnityEngine;
using R3;
using System;
using NUnit.Framework;

public class TempAddEnemyToDungeon : MonoBehaviour
{
    public DungeonManager DungeonManager;
    public EnemyBase[] EnemyBase;
    private CompositeDisposable _disposable;

    void Start()
    {
        _disposable = new CompositeDisposable(EnemyBase.Length);
        DungeonManager.GenerateDungeon();
        foreach (var enemy in EnemyBase)
        {
            DungeonManager.Generator.Data.AddEnemyToRoom(roomIndex: 0, enemy);
            enemy.Health.OnDeath
                .Subscribe(_ => DungeonManager.Generator.Data.RemoveEnemyFromRoom(roomIndex: 0, enemy))
                .AddTo(_disposable);
        }
    }

    private void OnDestroy()
    {
        _disposable?.Dispose();
    }
}
