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
}
