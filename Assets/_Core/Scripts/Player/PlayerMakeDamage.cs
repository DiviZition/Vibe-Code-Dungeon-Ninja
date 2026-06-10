using TimeControll;
using UnityEngine;
using Zenject;

namespace Player
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerMakeDamage : MonoBehaviour
    {
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private float _baseAttackInterval = 0.5f;
        private float _nextAttackTime = 0f;
        private TimeController _timeController;

        [Inject]
        private void Contruct(TimeController timeController)
        {
            _timeController = timeController;
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (_nextAttackTime < _timeController.CurrentTime && collider.TryGetComponent(out IDamageable damageable))
            {    
                _nextAttackTime = _timeController.CurrentTime + _baseAttackInterval;
                damageable.TakeDamage(_baseDamage);
            }
        }
    }
}
