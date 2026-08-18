using Core;
using R3;
using System;
using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyView : MonoBehaviour, IView<IEnemyModel>, IHasDamageable
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private SpriteRenderer _attackVisual;
        [SerializeField] private Gradient _attackChargeVisualization;
        [SerializeField] private DamageableVisual _damageableVisual;

        private IEnemyModel _model;
        private IDisposable _subscriptions;

        public IDamageable Damageable => _model?.Health;

        public void Init(IEnemyModel model)
        {
            _model = model;
            _damageableVisual?.Init(model.Health);

            var d = Disposable.CreateBuilder();
            model.Health.OnDeath
                .Subscribe(_ => OnEnemyDeath())
                .AddTo(ref d);
            model.OnAttackExecuted
                .Subscribe(MakeHit)
                .AddTo(ref d);
            _subscriptions = d.Build();
        }

        private void FixedUpdate()
        {
            if (_model == null) return;

            _rb.MovePosition(_model.Position);
            UpdateAttackVisual();
        }

        private void UpdateAttackVisual()
        {
            if (_attackVisual == null) return;

            if (_model.IsAttacking == false)
            {
                _attackVisual.enabled = false;
                return;
            }

            _attackVisual.enabled = true;
            _attackVisual.color = _attackChargeVisualization.Evaluate(_model.ChargeProgress);

            float trueSpriteRadius = _attackVisual.sprite.bounds.extents.x;
            _attackVisual.transform.localScale = Vector3.one * _model.Config.AttackRadius / trueSpriteRadius;
            _attackVisual.transform.position = _model.AttackPoint;
        }

        private void MakeHit(EnemyAttackData attackData)
        {
            var colliders = Physics2D.OverlapCircleAll(attackData.AttackPoint, attackData.Radius);
            foreach (var collider in colliders)
            {
                if (collider.transform == transform) continue;
                if (collider.TryGetComponent(out IHasDamageable damageable))
                    damageable.Damageable?.TakeDamage(attackData.Damage);
            }
        }

        private void OnEnemyDeath()
        {
            _model.Dispose();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _subscriptions?.Dispose();
        }
    }
}