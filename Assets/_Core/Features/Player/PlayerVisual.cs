using DG.Tweening;
using UnityEngine;

namespace Player
{
    public class PlayerVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Settings")]
        [SerializeField] private float _rotationDuration = 0.1f;



        public void SetDirection(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.DORotate(new Vector3(0, 0, angle), _rotationDuration);
            }
        }
    }
}
