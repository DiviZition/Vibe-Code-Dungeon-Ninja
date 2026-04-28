using UnityEngine;
using Zenject;

namespace TimeControll
{
    public class TimeControllerViewer : MonoBehaviour
    {
        public float CurrentTimeScale;
        public float CurrentTime;

        private TimeController _timeController;

        [Inject]
        private void Contruct(TimeController timeController) => _timeController = timeController;

        private void Update()
        {
            CurrentTime = _timeController.CurrentTime;
            CurrentTimeScale = _timeController.TimeScale;
        }

        public void StopTime() => _timeController.StopTheTime(1f);
        public void ContinueTime() => _timeController.ContinueTheTime(1f);
        public void SetTimeScale(float timeScale, float changeDuration = -1) => 
            _timeController.ChangeTimeScale(timeScale, changeDuration);
    }
}