using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerMakeDamage : MonoBehaviour
    {
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private float _baseAttackInterval = 0.5f;
        private float _nextAttackTime = 0f;

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (_nextAttackTime < Time.time && collider.TryGetComponent(out IDamageable damageable))
            {    
                _nextAttackTime = Time.time + _baseAttackInterval;
                damageable.TakeDamage(_baseDamage);
            }
        }
    }
}
