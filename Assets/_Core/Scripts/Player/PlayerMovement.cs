using UnityEngine;
using UnityEngine.InputSystem;
using TimeControll;
using Zenject;

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
        private TimeController _timeController;
        private Vector2 _currentDirection = Vector2.right;
        private float _estimatedSpeed;

        [Inject]
        private void Construct(TimeController timeController) => _timeController = timeController;

        private void OnEnable()
        {
            if(_gameInput == null)
                _gameInput = new GameInput();

            _gameInput.Enable();
            _gameInput.Player.Move.performed += OnMovePerformed;
            
            _currentDirection = Vector2.up;
        }

        private void OnDisable()
        {
            _gameInput.Player.Move.performed -= OnMovePerformed;
            _gameInput.Disable();
        }

        private void Update()
        {
            IncreaseMoveAcceleration();
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

        private void IncreaseMoveAcceleration()
        {
            _estimatedSpeed += _speedIncreaseRate;
            _estimatedSpeed = Mathf.Min(_estimatedSpeed, _maxSpeed);
        }

        private void Move()
        {
            _rb.linearVelocity = _currentDirection * _estimatedSpeed * _timeController.TimeScale;
        }

        public void ResetMoveAcceleration()
        {
            _estimatedSpeed = _initialSpeed;
        }
    }
}
