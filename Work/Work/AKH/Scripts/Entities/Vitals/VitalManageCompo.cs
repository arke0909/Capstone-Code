using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Chipmunk.Modules.StatSystem;
using UnityEngine;

namespace Scripts.Entities.Vitals
{
    public delegate void OnVitalChanged(StatSO vitalStat, float before, float after);
    public abstract class VitalManageCompo<TEvent> : MonoBehaviour, IContainerComponent, IAfterInitialze
        where TEvent : IVitalEvent, new()
    {
        public event OnVitalChanged OnValueChanged;
        public float CurrentValue
        {
            get => _currentValue; set
            {
                float before = _currentValue;
                _currentValue = Mathf.Clamp(value, 0, _maxValue);
                if (before != _currentValue)
                    OnValueChanged?.Invoke(ManageStat, before, _currentValue);
            }
        }
        public float MaxValue => _maxValue;
        [field: SerializeField] public StatSO ManageStat { get; private set; }
        [field: SerializeField] public StatSO StatPerSecStat { get; private set; }
        protected Entity _entity;
        protected LocalEventBus _localEventBus;
        protected StatBehavior _statCompo;
        protected float _stopTimer;
        private float _maxValue;
        private float _currentValue;
        public ComponentContainer ComponentContainer { get; set; }

        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
            _statCompo = componentContainer.Get<StatBehavior>(true);
            _entity = componentContainer.Get<Entity>(true);
            _localEventBus = componentContainer.Get<LocalEventBus>();
            OnValueChanged += HandleVitalChanged;
            Debug.Assert(_localEventBus != null, $"{gameObject.name}에게 LocalEventBus가 없음!, 컴포넌트 넣어줘요~");
        }
        protected virtual void Update()
        {
            _stopTimer = Mathf.Max(_stopTimer - Time.deltaTime, 0);
            if (_stopTimer > 0 || StatPerSecStat == null)
                return;
            CurrentValue += StatPerSecStat.Value * Time.deltaTime;
        }
        private void HandleVitalChanged(StatSO vitalStat, float before, float after)
        {
            TEvent vitalEvent = new();
            vitalEvent.Init(after, _maxValue);
            _localEventBus.Raise(vitalEvent);
        }

        public virtual void AfterInitialize()
        {
            _maxValue = _statCompo.SubscribeStat(ManageStat, HandleMaxStatChanged, 0);
            StatPerSecStat = _statCompo.GetStat(StatPerSecStat);
            CurrentValue = _maxValue;
        }
        public virtual void OnDestroy()
        {
            _statCompo.UnSubscribeStat(ManageStat, HandleMaxStatChanged);
            OnValueChanged -= HandleVitalChanged;
        }
        private void HandleMaxStatChanged(StatSO stat, float currentValue, float prevValue)
        {
            float changed = currentValue - prevValue;
            _maxValue = currentValue;
            CurrentValue += changed;
        }
        public void ChangeValueWithTimer(float value, float timer)
        {
            CurrentValue += value;
            _stopTimer = Mathf.Max(timer,_stopTimer);
        }
    }
}
