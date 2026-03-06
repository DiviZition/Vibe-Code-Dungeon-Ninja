using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

namespace FlyBoxEffect
{
    public class FlyingBox : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public Transform Transform { get; private set; }
        [SerializeField] private GameObject _gameObject;
        [SerializeField] private float _flySpeed;

        public bool IsActive { get; private set; }

        public void SetActive(bool newActiveState)
        {
            IsActive = newActiveState;
            _gameObject.SetActive(newActiveState);
        }

        public void SetupInitialPosition(Vector3 position, Quaternion direction)
        {
            Transform.localPosition = position;
            Transform.rotation = direction;
        }

        public async UniTaskVoid SetActiveAndGoFly(float flyDistance)
        {
            SetActive(true);
            await UniTask.NextFrame();

            Vector3 startLocalPosition = Transform.localPosition;
            while (Vector3.Distance(Transform.localPosition, startLocalPosition) < flyDistance)
            {
                Transform.localPosition += Transform.forward * _flySpeed * Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            SetActive(false);
        }
    }
}