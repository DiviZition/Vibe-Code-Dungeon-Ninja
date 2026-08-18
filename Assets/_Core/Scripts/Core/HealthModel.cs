using R3;
using UnityEngine;

namespace Core
{
    public class HealthModel : IHealthModel
    {
        public float MaxHealth { get; }
        public float CurrentHealth => MaxHealth - _damageTaken;
        public bool IsDead => _damageTaken >= MaxHealth;

        public Subject<Unit> OnDeath { get; } = new();
        public Subject<float> OnDamaged { get; } = new();
        public Subject<float> OnHealed { get; } = new();

        private float _damageTaken;

        public HealthModel(float maxHealth)
        {
            MaxHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount == 0) return;

            _damageTaken = Mathf.Clamp(_damageTaken + amount, 0, MaxHealth);
            OnDamaged.OnNext(CurrentHealth);

            if (IsDead)
                OnDeath.OnNext(Unit.Default);
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0) return;

            _damageTaken = Mathf.Clamp(_damageTaken - amount, 0, MaxHealth);
            OnHealed.OnNext(CurrentHealth);
        }

        public void ResetHealth()
        {
            _damageTaken = 0;
            OnHealed.OnNext(CurrentHealth);
        }
    }
}