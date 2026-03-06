using Player;
using UnityEngine;
using R3;

namespace Enemy
{
    public class Enemy : MonoBehaviour
    {
        [field: SerializeField] public PlayerHealth Health { get; private set; }
        [field: SerializeField] public EnemyMovement Movement { get; private set; }

        void Start()
        {
            Movement.SetIsAlliwedTiMove(true);
            Health.OnDeath.Subscribe(_ => DestroyEnemy());
        }

        void DestroyEnemy()
        {
            Movement.SetIsAlliwedTiMove(false);
            Observable.Timer(System.TimeSpan.FromSeconds(1f)).Subscribe(_ => Destroy(gameObject));
        }
    }
}