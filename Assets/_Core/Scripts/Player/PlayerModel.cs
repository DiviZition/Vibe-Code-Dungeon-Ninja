using Core;
using System;
using TimeControll;
using UnityEngine;
using Zenject;

namespace Player
{
    public class PlayerModel : IPlayerModel, ITickable, IDisposable
    {
        private readonly SimulationTicker _simulationTicker;
        private readonly ITimeController _timeController;

        private Vector2 _direction = Vector2.up;
        private float _speed;
        private float _nextAttackTime;

        public PlayerConfig Config { get; }
        public IHealthModel Health { get; }

        public Vector2 Position { get; private set; }
        public Vector2 Direction => _direction;
        public Vector2 Velocity => _direction * _speed * _timeController.TimeScale;

        public PlayerModel(PlayerConfig config, SimulationTicker simulationTicker, ITimeController timeController)
        {
            Config = config;
            Health = new HealthModel(config.MaxHealth);
            _simulationTicker = simulationTicker;
            _timeController = timeController;
            _speed = config.InitialSpeed;

            _simulationTicker.Register(this);
        }

        public void Tick()
        {
            _speed = Mathf.Min(_speed + Config.SpeedIncreaseRate, Config.MaxSpeed);
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction == Vector2.zero) return;
            _direction = direction.normalized;
        }

        public void SetPosition(Vector2 position) => Position = position;

        public void DealContactDamage(IDamageable target)
        {
            if (target == null || target.IsDead) return;
            if (_timeController.CurrentTime < _nextAttackTime) return;

            _nextAttackTime = _timeController.CurrentTime + Config.AttackInterval;
            target.TakeDamage(Config.Damage);
        }

        public void ResetSpeed() => _speed = Config.InitialSpeed;

        public void Dispose() => _simulationTicker.Unregister(this);
    }
}