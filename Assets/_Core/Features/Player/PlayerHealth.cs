using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [field: SerializeField] public float MaxHealth { get; private set; } = 100f;

        private float _damageTaken = 0;
        public float CurrentHealth => MaxHealth - _damageTaken;
        public bool IsDead => _damageTaken >= MaxHealth;

        public Subject<Unit> OnDeath { get; private set; } = new Subject<Unit>();
        public Subject<float> OnDamaged { get; private set; } = new Subject<float>();
        public Subject<float> OnHealed { get; private set; } = new Subject<float>();

        [Button]
        public void TakeDamage(float amount)
        {
            if (IsDead || amount == 0) return;

            _damageTaken = Mathf.Clamp(Mathf.Abs(_damageTaken) + amount, 0, MaxHealth);
            OnDamaged.OnNext(CurrentHealth);

            if (IsDead)
                OnDeath.OnNext(Unit.Default);
        }

        [Button]
        public void Heal(float amount)
        {
            if (IsDead || amount <= 0) return;

            _damageTaken = Mathf.Clamp(Mathf.Abs(_damageTaken) - amount, 0, MaxHealth);
            OnHealed.OnNext(CurrentHealth);
        }

        public void ResetHealth()
        {
            _damageTaken = 0;
            OnHealed?.OnNext(CurrentHealth);
        }
    }
}
