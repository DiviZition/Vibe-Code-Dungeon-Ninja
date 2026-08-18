using R3;

namespace Core
{
    public interface IDamageable : IModel
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }

        Subject<Unit> OnDeath { get; }
        Subject<float> OnHealed { get; }
        Subject<float> OnDamaged { get; }

        void TakeDamage(float amount);
        void Heal(float amount);
    }
}