using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Enemy
{
    [Serializable]
    public class EnemySimpleAttack : IEnemyState
    {
        [SerializeField] private Transform _transform;

        [Header("Charge Settings")]
        [SerializeField] private float _chargeTime = 1f;

        [Header("Attack Settings")]
        [SerializeField] private float _maxAttackLength = 1f;
        [SerializeField] private float _attackRadius = 1f;
        [SerializeField] private float baseDamage = 10f;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer _attackVisual;
        [SerializeField] private Gradient _attackChargeVisualization;

        private float _chargeProgress;
        private Vector2 _attackPoint;

        public bool IsFinished {  get; private set; }

        public void StartActionAnew()
        {
            IsFinished = false;
            _chargeProgress = 0f;
            _attackPoint = GetAttackPoint();
            EnableAttackVisual(_attackPoint);
        }

        public void StopAttack()
        {
            IsFinished = true;
            DisableAttackVisual();
        }

        public void UpdateByTime(float deltaTime)
        {
            if (IsFinished || deltaTime == 0)
                return;

            SetAttackVisualProgress(_chargeProgress);

            _chargeProgress += deltaTime / _chargeTime;
            if (_chargeProgress > 1)
            {
                MakeHit(_attackPoint);
                DisableAttackVisual();
                IsFinished = true;
            }
        }

        private void MakeHit(Vector2 attackPoint)
        {
            var colliders = Physics2D.OverlapCircleAll(attackPoint, _attackRadius);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out IDamageable damageable) && collider.transform != _transform)
                {
                    damageable.TakeDamage(baseDamage);
                    Debug.Log($"Damageable hitted: {collider.name}");
                }
            }
        }

        private Vector2 GetAttackPoint()
        {
            return (Vector2)_transform.localPosition + UnityEngine.Random.insideUnitCircle.normalized * _attackRadius;
        }

        private void SetAttackVisualProgress(float chargeProgress)
        {
            _attackVisual.color = _attackChargeVisualization.Evaluate(chargeProgress);
        }

        private void EnableAttackVisual(Vector2 attackPoint)
        {
            SetAttackVisualProgress(0);
            float trueSpriteRadius = _attackVisual.sprite.bounds.extents.x; // half-width in world units

            _attackVisual.enabled = true;
            _attackVisual.transform.localScale = Vector3.one * _attackRadius / trueSpriteRadius;
            _attackVisual.transform.position = attackPoint;
        }

        private void DisableAttackVisual()
        {
            _attackVisual.enabled = false;
        }
    }
}
