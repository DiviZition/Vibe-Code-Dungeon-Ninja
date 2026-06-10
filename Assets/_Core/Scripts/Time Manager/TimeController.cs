using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace TimeControll
{
    public class TimeController : ITickable, IDisposable
    {
        [SerializeField] private float _defaultTimeChangeDuration;
        private CancellationTokenSource _changeTimeSpeedCancelToken;

        [ReadOnly][field: SerializeField] public float TimeScale { get; private set; } = 1;
        [ReadOnly][field: SerializeField] public float CurrentTime {get; private set;}
        public float DeltaTime {get; private set;}

        public TimeController()
        {
            ChangeTimeScale(targetTimeScale: 1, timeChangeDuration: 0);
        }

        public void Tick()
        {
            if (TimeScale == 0) return;
            DeltaTime = Time.deltaTime * TimeScale;
            CurrentTime += DeltaTime;
        }

        public void StopTheTime(float timeChangeDuration = -1) => ChangeTimeScale(targetTimeScale: 0, timeChangeDuration);
        public void ContinueTheTime(float timeChangeDuration = -1) => ChangeTimeScale(targetTimeScale: 1, timeChangeDuration);

        public void ChangeTimeScale(float targetTimeScale, float timeChangeDuration = -1)
        {
            timeChangeDuration = timeChangeDuration == -1 ? _defaultTimeChangeDuration : timeChangeDuration;
            if (timeChangeDuration == 0 || targetTimeScale == TimeScale)
            {
                TimeScale = targetTimeScale;
                if (TimeScale == 0)
                    DeltaTime = 0;
                return;
            }

            _changeTimeSpeedCancelToken?.Cancel();
            _changeTimeSpeedCancelToken = new CancellationTokenSource();
            UpdateTimeSpeed(targetTimeScale, timeChangeDuration, _changeTimeSpeedCancelToken.Token).Forget();
        }

        private async UniTaskVoid UpdateTimeSpeed(float targetTimeScale, float timeChangeDuration, CancellationToken token)
        {
            float progress = 0;
            float startTimeScale = TimeScale;
            while (progress < 1)
            {
                progress += Time.deltaTime / timeChangeDuration;
                TimeScale = Mathf.Lerp(startTimeScale, targetTimeScale, progress);
                await UniTask.Yield(PlayerLoopTiming.Update);
                if (token.IsCancellationRequested == true)
                    return;
            }
        }

        public void Dispose()
        {
            _changeTimeSpeedCancelToken?.Cancel();
            _changeTimeSpeedCancelToken?.Dispose();
        }
    }
}