using Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputView : MonoBehaviour, IView<IPlayerModel>
    {
        private IPlayerModel _model;
        private GameInput _gameInput;

        public void Init(IPlayerModel model)
        {
            _model = model;

            if (_gameInput == null)
                _gameInput = new GameInput();

            _gameInput.Enable();
            _gameInput.Player.Move.performed += OnMovePerformed;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _model?.SetDirection(context.ReadValue<Vector2>());
        }

        private void OnDisable()
        {
            if (_gameInput == null) return;

            _gameInput.Player.Move.performed -= OnMovePerformed;
            _gameInput.Disable();
        }
    }
}