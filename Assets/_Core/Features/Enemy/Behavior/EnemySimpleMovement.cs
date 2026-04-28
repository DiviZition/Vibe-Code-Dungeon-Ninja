using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TimeControll;

namespace Enemy
{
    [Serializable]
    public class EnemySimpleMovement : IEnemyState
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private AnimationCurve _moveCurve;
        [SerializeField] private float _moveDurationPerUnit = 1f;
        [SerializeField] private Vector2 _moveDistance;

        private Vector2 _targetPosition;
        private Vector2 _startPosition;

        public bool IsFinished {get; private set;}
        private float _moveProgress = 0;
        private float _estimatedDuration;
        public void StartActionAnew()
        {
            IsFinished = false;
            _moveProgress = 0;
            _startPosition = _rigidbody.position;
            _targetPosition = GetRandomTargetPosition(out float distance);
            _estimatedDuration = _moveDurationPerUnit * distance;
        }

        public void UpdateByTime(float deltaTime)
        {
            IsFinished = _moveProgress > 1;
            if (IsFinished || deltaTime == 0)
                return;

            float curvedProgress = _moveCurve.Evaluate(_moveProgress);
            Vector2 currentPosition = Vector2.Lerp(_startPosition, _targetPosition, curvedProgress);

            _rigidbody.MovePosition(currentPosition);

            _moveProgress += deltaTime / _estimatedDuration;
        }

        private Vector2 GetRandomTargetPosition(out float distance)
        {
            distance = UnityEngine.Random.Range(_moveDistance.x, _moveDistance.y);
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            return (Vector2)_rigidbody.position + randomDir * distance;
        }
   }
}
