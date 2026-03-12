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

    public class TimedStateMachine : StateMachineBase, ITimeControllable
    {
        protected ISimpleStateTimed _currentTimedState => _currentState as ISimpleStateTimed;

        public TimedStateMachine(TimeController timeController)
        {
            timeController.AddSubscriber(this);
        }

        public void UpdateByTime(float deltaTime) => _currentTimedState.UpdateByTime(deltaTime);
    }

    public class StateMachineTimed : ITimeControllable
    {
        private ITaskStateTimed _currentState;
        private Queue<ITaskStateTimed> _statesQueue;

        public StateMachineTimed(TimeController timeController, Queue<ITaskStateTimed> statesQueue)
        {
            _statesQueue = statesQueue;
            timeController.AddSubscriber(this);
        }

        public void UpdateByTime(float deltaTime)
        {
            _currentState.UpdateByTime(deltaTime);
        }

        private void LaunchNextState()
        {
            var nextState = _statesQueue.Dequeue();
            if (nextState == null) return;

            //No reason to finish now, so it doesn't work yet...
        }
    }
}
