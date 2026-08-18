namespace TimeControll
{
    public interface ITimeController
    {
        float TimeScale { get; }
        float CurrentTime { get; }
        float DeltaTime { get; }

        void StopTheTime(float timeChangeDuration = -1);
        void ContinueTheTime(float timeChangeDuration = -1);
        void ChangeTimeScale(float targetTimeScale, float timeChangeDuration = -1);
    }
}