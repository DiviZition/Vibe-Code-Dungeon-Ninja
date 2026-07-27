using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Dungeon
{
    public class DungeonPresenter : IDisposable
    {
        private DungeonGeneratorConfig _generatorConfig;
        
        private DungeonGenerator _generator = new DungeonGenerator();
        private DungeonVisualizer _visualizer;
        public DungeonData DungeonData { get; private set; }

        IDisposable _roomEnemiesAppearanceEvents;
        AsyncOperationHandle<GameObject> _visualHandler;

        public async UniTask GenerateDungeon()
        {
            if (_visualizer == null)
                await LoadDungeonVisual();

            DungeonData = _generator.Generate(_generatorConfig);
            _visualizer.Visualize(DungeonData);
        }

        private async UniTask LoadDungeonVisual()
        {
            _visualHandler = Addressables.InstantiateAsync(AssetKeys.DungeonVew);
            await _visualHandler;
            _visualizer = _visualHandler.Result.GetComponent<DungeonVisualizer>();
        }

        public void OpenRoom(int roomIndex) => DungeonData.OpenRoomCorridors(roomIndex);
        public void CloseRoom(int roomIndex) => DungeonData.CloseRoomCorridors(roomIndex);

        public void Dispose()
        {
            _roomEnemiesAppearanceEvents?.Dispose();
            Addressables.ReleaseInstance(_visualHandler);
        }
    }
}