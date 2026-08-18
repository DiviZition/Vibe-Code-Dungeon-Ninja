using System;

namespace Enemy
{
    [Serializable]
    public class EnemyBehaviorConfig
    {
        public float IdleTime = 1f;
        public float MoveDurationPerUnit = 0.25f;
        public float MoveDistanceMin = 1.68f;
        public float MoveDistanceMax = 3.18f;
        public float ChargeTime = 1f;
        public float AttackRadius = 1f;
        public float Damage = 10f;
        public float MaxHealth = 10f;
    }
}