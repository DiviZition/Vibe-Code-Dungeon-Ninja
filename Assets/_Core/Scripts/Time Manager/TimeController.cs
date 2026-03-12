using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace TimeControll
{
    public class TimeController : MonoInstaller
    {
        [SerializeField] private float _defaultTimeChangeDuration;

        private HashSet<ITimeControllable> _subscribers;
        private Coroutine _switchTimeScaleRoutine;

        [ReadOnly][field: SerializeField] public float TimeScale { get; private set; } = 1;
        [ReadOnly][field: SerializeField] public float CurrentTime {get; private set;}
        public float DeltaTime {get; private set;}

        public override void InstallBindings()
        {
            Container.Bind<TimeController>().FromInstance(this).AsSingle();
            ChangeTheTime(targetTimeScale: 1, timeChangeDuration: 0);
        }

        private void Update()
        {
            UpdateSubscribers();
        }

        private void UpdateSubscribers()
        {
            if (TimeScale == 0) return;

            DeltaTime = Time.deltaTime * TimeScale;
            CurrentTime += DeltaTime;

            if (_subscribers == null || _subscribers.Count <= 0) return;
            foreach (ITimeControllable item in _subscribers)
            {
                item?.UpdateByTime(DeltaTime);
            }
        }

        public void StopTheTime(float timeChangeDuration = -1) => ChangeTheTime(targetTimeScale: 0, timeChangeDuration);
        public void ContinueTheTime(float timeChangeDuration = -1) => ChangeTheTime(targetTimeScale: 1, timeChangeDuration);

        public void ChangeTheTime(float targetTimeScale, float timeChangeDuration = -1)
        {
            timeChangeDuration = timeChangeDuration == -1 ? _defaultTimeChangeDuration : timeChangeDuration;
            if (timeChangeDuration == 0 || targetTimeScale == TimeScale)
            {
                TimeScale = targetTimeScale;
                return;
            }

            StopAllCoroutines();
            _switchTimeScaleRoutine = StartCoroutine(UpdateTimeSpeed(targetTimeScale, timeChangeDuration));
        }

        private IEnumerator UpdateTimeSpeed(float targetTimeScale, float timeChangeDuration)
        {
            float progress = 0;
            float startTimeScale = TimeScale;
            while (progress < 1)
            {
                progress += Time.deltaTime / timeChangeDuration;
                TimeScale = Mathf.Lerp(startTimeScale, targetTimeScale, progress);
                yield return null;
            }
        }

        public void AddSubscriber(ITimeControllable subscriber)
        {
            if (_subscribers == null)
                _subscribers = new HashSet<ITimeControllable>(8);

            if (_subscribers.Add(subscriber) == false)
                Debug.LogError($"Can't remove the subscriber from TimeController: {subscriber}");
        }

        public void RemoveSubscriber(ITimeControllable subscriber)
        {
            if(_subscribers?.Remove(subscriber) == false)
                Debug.LogError($"Can't remove the subscriber from TimeController: {subscriber}");
        }
    }
}