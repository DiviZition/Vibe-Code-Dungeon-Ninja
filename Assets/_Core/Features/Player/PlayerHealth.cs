using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

        public bool IsDead => _currentHealth <= 0f;

        public Subject<Unit> OnDeath { get; private set; } = new Subject<Unit>();
        public Subject<float> OnTakeDamage { get; private set; } = new Subject<float>();
        public Subject<float> OnHealed { get; private set; } = new Subject<float>();

        [Button]
        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            _currentHealth -= amount;
            _currentHealth = Mathf.Max(_currentHealth, 0f);
            OnTakeDamage.OnNext(amount);

            if (IsDead)
                OnDeath.OnNext(Unit.Default);
        }

        [Button]
        public void Heal(float amount)
        {
            if (IsDead) return;

            _currentHealth += amount;
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            OnHealed.OnNext(amount);
        }

        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
        }
    }
}
