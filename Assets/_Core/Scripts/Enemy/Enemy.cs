using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using TimeControll;
using Player;
using R3;
using System.Collections;
using AssetInventory;

namespace Enemy
{
    public enum EnemyState
    {
        Idle,
        Moving,
        Attacking
    }

    public class Enemy : MonoBehaviour
    {
        [field: SerializeField] public PlayerHealth Health { get; private set; }

        [SerializeField] private EnemySimpleMovement _simpleMovement;
        [SerializeField] private EnemySimpleAttack _simpleAttack;

        private IEnemyState _movement;
        private IEnemyState _attack;
        private TimeController _timeController;

        private IEnumerator _behaviourLoopRoutine;

        [Inject]
        public void Construct(TimeController timeController)
        {
            _timeController = timeController;
        }

        private void Start()
        {
            _movement = _simpleMovement;
            _attack = _simpleAttack;

            Health.OnDeath.Subscribe(_ => DestroyEnemy());
            _behaviourLoopRoutine = BehaviourLoop();
            StartCoroutine(_behaviourLoopRoutine);
        }

        private IEnumerator BehaviourLoop()
        {
            var idleState = new EnemyIdleState(idleTime: 1);
            while (true)
            {
                yield return PerformAction(idleState);
                yield return PerformAction(_movement);
                yield return PerformAction(_attack);
            }
        }

        private IEnumerator PerformAction(IEnemyState action)
        {
            action.StartActionAnew();
            while (action.IsFinished == false)
            {
                action.UpdateByTime(_timeController.DeltaTime);
                yield return null;
            }   
        }

        private void DestroyEnemy()
        {
            StopCoroutine(_behaviourLoopRoutine);
            Observable.Timer(System.TimeSpan.FromSeconds(1f)).Subscribe(_ => Destroy(gameObject));
        }
    }   
}