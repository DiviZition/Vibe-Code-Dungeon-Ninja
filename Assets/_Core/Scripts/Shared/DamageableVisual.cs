using System;
using Core;
using DG.Tweening;
using R3;
using UnityEngine;

public class DamageableVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Hit Settings")]
    [SerializeField] private Color _hitColor = Color.white;
    [SerializeField] private float _hitDuration = 0.15f;

    [Header("Death Settings")]
    [SerializeField] private Color _deathColor = new Color(0.3f, 0.3f, 0.3f, 0f);
    [SerializeField] private float _deathDuration = 0.5f;

    private Color _originalColor;
    private IDisposable _subscriptions;

    private void Awake()
    {
        _originalColor = _spriteRenderer.color;
    }

    public void Init(IDamageable damageable)
    {
        _subscriptions?.Dispose();

        var d = Disposable.CreateBuilder();

        damageable.OnDamaged
            .Subscribe(_ => PlayHitEffect())
            .AddTo(ref d);

        damageable.OnDeath
            .Subscribe(_ => PlayDeathEffect())
            .AddTo(ref d);

        _subscriptions = d.Build();
    }

    private void OnDestroy()
    {
        _subscriptions?.Dispose();
        _spriteRenderer.DOKill();
    }

    private void PlayHitEffect()
    {
        _spriteRenderer.DOKill();
        _spriteRenderer.color = _hitColor;
        _spriteRenderer.DOColor(_originalColor, _hitDuration).SetEase(Ease.OutQuad);
    }

    private void PlayDeathEffect()
    {
        _spriteRenderer.DOKill();
        _spriteRenderer.DOColor(_deathColor, _deathDuration).SetEase(Ease.InQuad);
    }
}