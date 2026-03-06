using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

using Random = UnityEngine.Random;

namespace FlyBoxEffect
{
    public class UniTaskBoxFlyEffect : MonoBehaviour
    {
        [SerializeField] private FlyingBox _boxPrefab;
        [SerializeField] private Transform _loadingTable;
        [SerializeField] private Transform _transform;
        [SerializeField] private float _spawnRadius;
        [SerializeField] private float _depthOffset;
        [SerializeField] private float _centerOffset;
        [SerializeField] private float _spawnDelay;

        private List<FlyingBox> _boxes;
        private CancellationTokenSource _cancelTokenSource;

        [ContextMenu("Start Spawning")]
        private async UniTaskVoid StartEffect()
        {
            _cancelTokenSource = new CancellationTokenSource();
            while (_cancelTokenSource.IsCancellationRequested == false)
            {
                LaunchBox().Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(_spawnDelay));
            }
        }

        [ContextMenu("Stop Spawning")]
        private void StopEffect() => _cancelTokenSource?.Cancel();

        [ContextMenu("Spawn new box")]
        private async UniTaskVoid LaunchBox()
        {
            FlyingBox box = await GetFreeBox();
            Vector3 spawnPosition = Random.insideUnitCircle.normalized * _spawnRadius;
            Vector3 center = _transform.position + Random.insideUnitCircle.ToVector3XY() * _centerOffset;
            Quaternion lookRotation = Quaternion.LookRotation(center - (_transform.position + spawnPosition));
            spawnPosition.z += Random.Range(-_depthOffset, _depthOffset);

            box.SetupInitialPosition(spawnPosition, lookRotation);
            //box.SetActive(true);
            box.SetActiveAndGoFly(_spawnRadius * 2).Forget();
        }

        public UniTask<FlyingBox> GetFreeBox()
        {
            if (_boxes == null)
                _boxes = new List<FlyingBox>(16);

            if (TryFindFreeBox(out var foundBox) == true)
                return UniTask.FromResult(foundBox);
            else
                return CreateNewBox();
        }

        public bool TryFindFreeBox(out FlyingBox foundFreeBox)
        {
            foundFreeBox = null;
            for (int i = 0; i < _boxes.Count; i++)
            {
                if (_boxes[i]?.IsActive == false)
                    foundFreeBox = _boxes[i];
            }

            return foundFreeBox != null;
        }

        public async UniTask<FlyingBox> CreateNewBox()
        {
            FlyingBox[] newBoxs = await InstantiateAsync(_boxPrefab, 1, _loadingTable, Vector3.zero, Quaternion.identity);

            newBoxs[0].SetActive(false);
            newBoxs[0].Transform.SetParent(_transform);
            _boxes.Add(newBoxs[0]);

            return newBoxs[0];
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.purple;
            Gizmos.DrawWireSphere(_transform.position, _spawnRadius);
        }
    }
}