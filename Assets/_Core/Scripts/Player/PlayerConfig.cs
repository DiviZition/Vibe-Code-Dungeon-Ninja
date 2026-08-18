using System;

namespace Player
{
    [Serializable]
    public class PlayerConfig
    {
        public float InitialSpeed = 10f;
        public float MaxSpeed = 20f;
        public float SpeedIncreaseRate = 3f;
        public float MaxHealth = 20f;
        public float Damage = 5f;
        public float AttackInterval = 0.2f;
    }
}