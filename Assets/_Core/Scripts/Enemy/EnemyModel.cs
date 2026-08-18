using Core;
using R3;
using System;
using TimeControll;
using UnityEngine;
using Zenject;

namespace Enemy
{
    public class EnemyModel : IEnemyModel, IEnemyStateContext, ITickable, IDisposable
    {
        private readonly SimulationTicker _simulationTicker;
        private readonly ITimeController _timeController;

        private IEnemyState _idleState;
        private EnemyMoveState _moveState;
        private EnemyAttackState _attackState;
        private IEnemyState _currentState;

        private bool _disposed;

        public IHealthModel Health { get; }
        public EnemyBehaviorConfig Config { get; }
        public int RoomIndex { get; }

        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public Vector2 AttackPoint => _attackState.AttackPoint;
        public float ChargeProgress => _currentState == _attackState ? _attackState.ChargeProgress : 0;
        public bool IsAttacking => _currentState == _attackState;

        public Subject<EnemyAttackData> OnAttackExecuted { get; } = new();

        public EnemyModel(EnemyBehaviorConfig config, Vector2 startPosition, int roomIndex,
            SimulationTicker simulationTicker, ITimeController timeController)
        {
            Config = config;
            Health = new HealthModel(config.MaxHealth);
            Position = startPosition;
            RoomIndex = roomIndex;
            _simulationTicker = simulationTicker;
            _timeController = timeController;

            _idleState = new EnemyIdleState(config.IdleTime);
            _moveState = new EnemyMoveState(this);
            _attackState = new EnemyAttackState(this);
            _currentState = _idleState;
            _currentState.StartActionAnew();

            _simulationTicker.Register(this);
        }

        public void Tick()
        {
            if (Health.IsDead) return;

            _currentState.UpdateByTime(_timeController.DeltaTime);

            if (_currentState == _moveState)
            {
                Position = _moveState.CurrentPosition;
                Direction = _moveState.Direction;
            }

            if (_currentState.IsFinished)
                AdvanceToNextState();
        }

        private void AdvanceToNextState()
        {
            if (_currentState == _idleState)
            {
                _currentState = _moveState;
            }
            else if (_currentState == _moveState)
            {
                _currentState = _attackState;
            }
            else if (_currentState == _attackState)
            {
                OnAttackExecuted.OnNext(new EnemyAttackData
                {
                    AttackPoint = _attackState.AttackPoint,
                    Radius = Config.AttackRadius,
                    Damage = Config.Damage
                });
                _currentState = _idleState;
            }

            _currentState.StartActionAnew();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _simulationTicker.Unregister(this);
        }
    }
}