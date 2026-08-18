using Core;
using System;
using UnityEngine;

namespace Player
{
    public interface IPlayerModel : IModel, IDisposable
    {
        PlayerConfig Config { get; }
        IHealthModel Health { get; }

        Vector2 Position { get; }
        Vector2 Direction { get; }
        Vector2 Velocity { get; }

        void SetDirection(Vector2 direction);
        void SetPosition(Vector2 position);
        void DealContactDamage(IDamageable target);
        void ResetSpeed();
    }
}