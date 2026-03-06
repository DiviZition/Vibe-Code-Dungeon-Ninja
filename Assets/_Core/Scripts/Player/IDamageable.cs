using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public interface IDamageable
{
    public float CurrentHealth { get; }
    public float MaxHealth { get; }
    public bool IsDead { get; }
    public Subject<Unit> OnDeath { get; }
    public Subject<float> OnTakeDamage { get; }
    public Subject<float> OnHealed { get; }
    void TakeDamage(float amount);
    void Heal(float amount);
}
