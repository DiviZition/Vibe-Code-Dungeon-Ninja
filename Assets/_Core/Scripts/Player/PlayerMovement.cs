using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _initialSpeed = 10f;
        [SerializeField] private float _maxSpeed = 30f;
        [SerializeField] private float _speedIncreaseRate = 20f;

        [Header("References")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private PlayerVisual _playerVisual;

        private GameInput _gameInput;
        private Vector2 _currentDirection = Vector2.right;
        private float _currentSpeed;

        private void OnEnable()
        {
            if(_gameInput == null)
                _gameInput = new GameInput();

            _gameInput.Enable();
            _gameInput.Player.Move.performed += OnMovePerformed;
            
            _currentDirection = Vector2.up;
            ResetSpeed();
        }

        private void OnDisable()
        {
            _gameInput.Player.Move.performed -= OnMovePerformed;
            _gameInput.Disable();
        }

        private void Update()
        {
            IncreaseSpeedOverTime();
            Move();
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            Vector2 inputDirection = context.ReadValue<Vector2>();

            if (inputDirection != Vector2.zero)
            {
                _currentDirection = inputDirection.normalized;
                _playerVisual?.SetDirection(_currentDirection);
            }
        }

        private void IncreaseSpeedOverTime()
        {
            if (_currentSpeed < _maxSpeed)
            {
                _currentSpeed += _speedIncreaseRate * Time.deltaTime;
                _currentSpeed = Mathf.Min(_currentSpeed, _maxSpeed);
            }
        }

        private void Move()
        {
            _rb.linearVelocity = _currentDirection * _currentSpeed;
        }

        public void ResetSpeed()
        {
            _currentSpeed = _initialSpeed;
        }
    }
}
