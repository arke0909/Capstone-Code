using System;
using Chipmunk.GameEvents;
using DewmoLib.Dependencies;
using UnityEngine;
using Work.Code.GameEvents;

namespace Code.TimeSystem
{
    public static class TimeUtil
    {
        //이건 현실 시간 기준임
        // ex 현실 시간 기준을 게임 기준 시간으로 변환 해줌
        public static float Min(float v) => v ;
        public static float Hour(float v) => v * 60f;
        public static float Day(float v) => v * 3600f;
    }
    
    [Provide]
    public class TimeController : MonoBehaviour, IDependencyProvider
    {
        public static TimeController Instance { get; private set; }

        public event Action<float> OnTimeScaleChanged;

        [Header("Time Settings")]
        [SerializeField] private float startTime = 0f;
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private TimeUI timeUI;
        [SerializeField] private float secondsPerDay = 1440f;

        public float TotalTime { get; private set; }
        public float DayTime { get; private set; }
        public float SecondsPerDay => secondsPerDay;
        public int CurrentDay { get; private set; } = 1;
        public bool IsPaused { get; private set; }

        
        // 현재는 24분이 하루  1분이 1시간으로 대응 돼있음 TimeScale로 조정 가능함
        public float TimeScale
        {
            get => timeScale;
            set
            {
                timeScale = value;
                OnTimeScaleChanged?.Invoke(value);
            }
        }

        private EventScheduler scheduler = new EventScheduler();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            TotalTime += TimeUtil.Hour(startTime);
            DayTime += TimeUtil.Hour(startTime);
        }

        private void Update()
        {
            if (IsPaused) return;
            float cheatTimeScale = timeScale; //빌드본 시연용
            if (Input.GetKey(KeyCode.F6))
                cheatTimeScale = 240;
            float dt = Time.deltaTime * cheatTimeScale;
            TotalTime += dt;
            DayTime += dt;

            if (DayTime >= secondsPerDay)
            {
                CurrentDay++;
                DayTime = 0;
                Debug.Log($"Day {DayTime} 시작");
                timeUI.SetDay($"{CurrentDay}일차");
                var evt = new DayChangeEvent();
                EventBus.Raise(evt);
            }

            scheduler.Update(TotalTime);
            UpdateUI();
        }

        private void UpdateUI()
        {
            int hour = (int)((DayTime % 3600) / 60);
            int minute = (int)(DayTime % 60);

            timeUI.SetTime($"{hour:D2}:{minute:D2}");
            
        }

        #region Control

        public void SetPause(bool pause)
        {
            IsPaused = pause;
        }

        public void SetTimeScale(float scale)
        {
            timeScale = scale;
        }

        public void ResetTime()
        {
            TotalTime = 0f;
            DayTime = 0;
            CurrentDay = 0;
        }

        #endregion

        #region Event

        public int AddEvent(float delay, Action action)
        {
            return scheduler.AddEvent(TotalTime, delay, action);
        }

        public void CancelEvent(int id)
        {
            scheduler.Cancel(id);
        }

        public int AddRepeatEvent(float interval, Action action)
        {
            return scheduler.AddRepeat(TotalTime, interval, action);
        }

        #endregion
    }
}
