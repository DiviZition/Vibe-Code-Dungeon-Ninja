using System.Collections.Generic;
using TimeControll;

namespace StateMachine
{
    public class StateMachineBase
    {
        protected IStateBase _currentState;

        public virtual void SetState(IStateBase newState)
        {
            if (_currentState == newState) return;
            
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }
    }

    public class SimpleStateMachine : StateMachineBase
    {
        private ISimpleState _currentSimpleState => _currentState as ISimpleState;

        public void Update() => _currentSimpleState?.Update();
    }
}
