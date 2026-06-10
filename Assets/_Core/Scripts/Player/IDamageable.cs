using R3;

public interface IDamageable
{
    public float CurrentHealth { get; }
    public float MaxHealth { get; }
    public bool IsDead { get; }
    public Subject<Unit> OnDeath { get; }
    public Subject<float> OnHealed { get; }
    public Subject<float> OnDamaged { get; }
    void TakeDamage(float amount);
    void Heal(float amount);
}
