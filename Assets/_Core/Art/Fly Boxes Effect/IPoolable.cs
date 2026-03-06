namespace FlyBoxEffect
{
    public interface IPoolable
    {
        bool IsActive { get; }
        public void SetActive(bool newActiveState);
    }
}