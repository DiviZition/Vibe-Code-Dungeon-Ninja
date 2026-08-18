using Core;
using DG.Tweening;
using R3;
using System;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerView : MonoBehaviour, IView<IPlayerModel>, IHasDamageable
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private DamageableVisual _damageableVisual;

        [Header("Visual Settings")]
        [SerializeField] private float _rotationDuration = 0.1f;

        private IPlayerModel _model;
        private IDisposable _subscriptions;

        public IDamageable Damageable => _model?.Health;

        public void Init(IPlayerModel model)
        {
            _model = model;
            _damageableVisual?.Init(model.Health);

            var d = Disposable.CreateBuilder();
            model.Health.OnDeath
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(ref d);
            _subscriptions = d.Build();
        }

        private void FixedUpdate()
        {
            if (_model == null) return;

            _rb.linearVelocity = _model.Velocity;
            _model.SetPosition(_rb.position);
            RotateVisual(_model.Direction);
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (_model == null) return;

            if (collider.TryGetComponent(out IHasDamageable damageable))
                _model.DealContactDamage(damageable.Damageable);
        }

        private void RotateVisual(Vector2 direction)
        {
            if (_visualTransform == null || direction == Vector2.zero) return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            _visualTransform.DORotate(new Vector3(0, 0, angle), _rotationDuration);
        }

        private void OnDestroy()
        {
            _subscriptions?.Dispose();
        }
    }
}