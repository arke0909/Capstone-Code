using Chipmunk.GameEvents;
using Code.TimeSystem;
using DewmoLib.Dependencies;
using UnityEngine;
using Work.Code.GameEvents;

namespace Work.Code.MapEvents
{
    public abstract class MapEvent : MonoBehaviour
    {
        [Inject] protected TimeController _timeController;
        
        [field: SerializeField] public MapEventSO MapEventSO { get; private set; }
        public string EventName { get; protected set; }
        
        protected virtual void Awake()
        {
            Debug.Assert(_timeController != null, "TimeController is null");
            Debug.Assert(MapEventSO != null, "MapEventSO is null");

            if (MapEventSO.isRepeat)
                AddRepeatEvent();
            else
                AddEvent();
        }

        private void AddEvent()
        {
            _timeController.AddEvent(MapEventSO.interval, OnEventCalled);
        }

        private void AddRepeatEvent()
        {
            _timeController.AddRepeatEvent(MapEventSO.interval, OnEventCalled);
        }

        private void OnEventCalled()
        {
            StartEvent();
            EventBus.Raise(new MapEventStartEvent(this, MapEventSO.duration));
        }

        protected abstract void StartEvent();
    }
}