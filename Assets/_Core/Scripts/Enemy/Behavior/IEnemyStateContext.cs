using UnityEngine;

namespace Enemy
{
    public interface IEnemyStateContext
    {
        EnemyBehaviorConfig Config { get; }
        Vector2 Position { get; }
    }
}