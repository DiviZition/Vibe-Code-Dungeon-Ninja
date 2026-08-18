using UnityEngine;

namespace Enemy
{
    public class EnemyMoveState : IEnemyState
    {
        private readonly IEnemyStateContext _context;

        private Vector2 _startPosition;
        private Vector2 _targetPosition;
        private float _moveProgress;
        private float _estimatedDuration;

        public bool IsFinished { get; private set; }
        public Vector2 CurrentPosition => Vector2.Lerp(_startPosition, _targetPosition, Mathf.SmoothStep(0, 1, _moveProgress));
        public Vector2 Direction
        {
            get
            {
                Vector2 direction = CurrentPosition - _startPosition;
                return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
            }
        }

        public EnemyMoveState(IEnemyStateContext context)
        {
            _context = context;
        }

        public void StartActionAnew()
        {
            IsFinished = false;
            _moveProgress = 0;
            _startPosition = _context.Position;

            float distance = Random.Range(_context.Config.MoveDistanceMin, _context.Config.MoveDistanceMax);
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            _targetPosition = _startPosition + randomDirection * distance;
            _estimatedDuration = _context.Config.MoveDurationPerUnit * distance;
        }

        public void UpdateByTime(float deltaTime)
        {
            IsFinished = _moveProgress >= 1;
            if (IsFinished || deltaTime == 0) return;

            _moveProgress += deltaTime / _estimatedDuration;
        }
    }
}