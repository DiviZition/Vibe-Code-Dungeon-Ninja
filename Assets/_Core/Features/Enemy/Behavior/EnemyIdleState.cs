using UnityEngine;

namespace Enemy
{
    public class EnemyIdleState : IEnemyState
    {
        public bool IsFinished {get; private set;}
        private float _timer;
        private float _idleTime;

        public EnemyIdleState(float idleTime) => _idleTime = idleTime;

        public void StartActionAnew()
        {
            IsFinished = false;
            _timer = 0;
        }

        public void UpdateByTime(float deltaTime)
        {
            _timer += deltaTime;
            IsFinished = _timer > _idleTime;
        }

    }
}
