using UnityEngine;

namespace Enemy
{
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private Vector2 _moveDistance;
        [SerializeField] private Vector2 _idleTime;

        private Vector3 _targetPosition;
        private bool _isMoving;
        private float _idleTimer;

        public bool IsAllowedToMove { get; private set;}

        public void SetIsAlliwedTiMove(bool value) => IsAllowedToMove = value;

        private void Update()
        {
            if (IsAllowedToMove == false) return;

            if (_isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
                {
                    transform.position = _targetPosition;
                    ScheduleNextMove();
                }
            }
            else
            {
                _idleTimer -= Time.deltaTime;
                if (_idleTimer <= 0f)
                {
                    PickRandomTarget();
                    _isMoving = true;
                }
            }
        }

        private void ScheduleNextMove()
        {
            _isMoving = false;
            _idleTimer = Random.Range(_idleTime.x, _idleTime.y);
        }

        private void PickRandomTarget()
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            _targetPosition = transform.position + new Vector3(direction.x, direction.y, 0f) * Random.Range(_moveDistance.x, _moveDistance.y);
        }
    }
}
