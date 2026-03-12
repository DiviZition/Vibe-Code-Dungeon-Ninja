using TimeControll;

namespace StateMachine
{
    public interface IStateBase
    {
        void Enter();
        void Exit();
    }

        public interface ISimpleState : IStateBase
    {
        void Update();
    }

    public interface ISimpleStateTimed : IStateBase, ITimeControllable
    {
    }

    public interface ITaskStateTimed : IStateBase, ITimeControllable
    {
    }
}
