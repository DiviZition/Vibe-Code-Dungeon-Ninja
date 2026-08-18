using Cysharp.Threading.Tasks;
using Core;
using Dungeon;
using Enemy;
using Player;
using R3;
using System;
using System.Collections.Generic;
using TimeControll;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class GameBootstrapper : IInitializable, IDisposable
{
    private readonly IDungeonFacade _dungeonFacade;
    private readonly ITimeController _timeController;
    private readonly SimulationTicker _simulationTicker;

    private readonly List<IDisposable> _disposables = new();
    private readonly List<Enemy.IEnemyModel> _spawnedEnemies = new();

    private AsyncOperationHandle<GameObject> _dungeonVisualHandle;
    private IPlayerModel _playerModel;

    public GameBootstrapper(IDungeonFacade dungeonFacade, ITimeController timeController, SimulationTicker simulationTicker)
    {
        _dungeonFacade = dungeonFacade;
        _timeController = timeController;
        _simulationTicker = simulationTicker;
    }

    public void Initialize() => InitializeGameAsync().Forget();

    private async UniTaskVoid InitializeGameAsync()
    {
        try
        {
            await _dungeonFacade.GenerateDungeon();
            await SpawnDungeonVisual();
            await SpawnPlayer();
            await SpawnEnemies();
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameBootstrapper] Initialization failed: {e}");
        }
    }

    private async UniTask SpawnDungeonVisual()
    {
        _dungeonVisualHandle = Addressables.InstantiateAsync(AssetKeys.DungeonVew);
        await _dungeonVisualHandle;
        var visualizer = _dungeonVisualHandle.Result.GetComponent<DungeonVisualizer>();
        visualizer.Init(_dungeonFacade.Model);
    }

    private async UniTask SpawnPlayer()
    {
        var startRoom = _dungeonFacade.Data.Rooms[0];
        Vector3 position = GetRoomCenter(startRoom);

        _playerModel = new PlayerModel(new PlayerConfig(), _simulationTicker, _timeController);
        _playerModel.SetPosition(position);

        var handle = Addressables.InstantiateAsync(AssetKeys.Player, position, Quaternion.identity);
        await handle;

        var playerView = handle.Result.GetComponent<PlayerView>();
        playerView.Init(_playerModel);

        if (handle.Result.TryGetComponent(out PlayerInputView inputView))
            inputView.Init(_playerModel);
    }

    private async UniTask SpawnEnemies()
    {
        var plan = _dungeonFacade.GetEnemySpawnPlan();
        foreach (var spawn in plan)
        {
            if (spawn.Count <= 0) continue;

            var room = _dungeonFacade.Data.Rooms[spawn.RoomIndex];
            for (int i = 0; i < spawn.Count; i++)
            {
                Vector3 position = GetRandomPositionInRoom(room);
                await SpawnEnemy(spawn.RoomIndex, position);
            }
        }
    }

    private async UniTask SpawnEnemy(int roomIndex, Vector3 position)
    {
        var config = new EnemyBehaviorConfig();
        var enemyModel = new EnemyModel(config, position, roomIndex, _simulationTicker, _timeController);
        _dungeonFacade.AddEnemyToRoom(roomIndex, enemyModel);
        _spawnedEnemies.Add(enemyModel);

        _disposables.Add(enemyModel.Health.OnDeath
            .Subscribe(_ => _dungeonFacade.RemoveEnemyFromRoom(roomIndex, enemyModel)));

        var handle = Addressables.InstantiateAsync(AssetKeys.Enemy, position, Quaternion.identity);
        await handle;

        var enemyView = handle.Result.GetComponent<EnemyView>();
        enemyView.Init(enemyModel);
    }

    private static Vector3 GetRoomCenter(RoomData room)
    {
        return new Vector3(room.GridX + room.Width / 2f, room.GridY + room.Height / 2f, 0);
    }

    private static Vector3 GetRandomPositionInRoom(RoomData room)
    {
        return new Vector3(
            room.GridX + UnityEngine.Random.Range(1, room.Width - 1),
            room.GridY + UnityEngine.Random.Range(1, room.Height - 1),
            0);
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
            disposable?.Dispose();
        _playerModel?.Dispose();
        foreach (var enemy in _spawnedEnemies)
            enemy?.Dispose();
        _dungeonFacade?.Dispose();

        if (_dungeonVisualHandle.IsValid())
            Addressables.ReleaseInstance(_dungeonVisualHandle);
    }
}