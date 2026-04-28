using TimeControll;

namespace Enemy
{
    public interface IEnemyState : ITimeControllable
    {
        bool IsFinished {get;}
        void StartActionAnew();
    }
}
