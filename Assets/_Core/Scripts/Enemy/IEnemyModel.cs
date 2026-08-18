using Core;
using R3;
using System;
using UnityEngine;

namespace Enemy
{
    public interface IEnemy
    {
    }

    public interface IEnemyModel : IEnemy, IModel, IDisposable
    {
        IHealthModel Health { get; }
        EnemyBehaviorConfig Config { get; }
        int RoomIndex { get; }

        Vector2 Position { get; }
        Vector2 Direction { get; }
        Vector2 AttackPoint { get; }
        float ChargeProgress { get; }
        bool IsAttacking { get; }

        Subject<EnemyAttackData> OnAttackExecuted { get; }
    }
}