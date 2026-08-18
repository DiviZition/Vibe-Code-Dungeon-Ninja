using UnityEngine;

namespace Enemy
{
    public class EnemyAttackState : IEnemyState
    {
        private readonly IEnemyStateContext _context;

        private Vector2 _attackPoint;
        private float _chargeProgress;

        public bool IsFinished { get; private set; }
        public float ChargeProgress => _chargeProgress;
        public Vector2 AttackPoint => _attackPoint;

        public EnemyAttackState(IEnemyStateContext context)
        {
            _context = context;
        }

        public void StartActionAnew()
        {
            IsFinished = false;
            _chargeProgress = 0;
            _attackPoint = _context.Position + Random.insideUnitCircle.normalized * _context.Config.AttackRadius;
        }

        public void UpdateByTime(float deltaTime)
        {
            if (IsFinished || deltaTime == 0) return;

            _chargeProgress += deltaTime / _context.Config.ChargeTime;
            if (_chargeProgress >= 1)
            {
                _chargeProgress = 1;
                IsFinished = true;
            }
        }
    }
}