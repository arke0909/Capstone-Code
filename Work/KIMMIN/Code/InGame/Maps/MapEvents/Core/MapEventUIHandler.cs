using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.TimeSystem;
using DewmoLib.Dependencies;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.GameEvents;

namespace Work.Code.MapEvents
{
    public class MapEventUIHandler : MonoBehaviour
    {
        [SerializeField] private MapEventUI eventUI;
        [SerializeField] private Transform root;

        [Inject] private TimeController _timeController;

        private const int InitCount = 5;
        private readonly Stack<MapEventUI> _uiStack = new();

        private void Awake()
        {
            for (int i = 0; i < InitCount; i++)
            {
                _uiStack.Push(CreateUI());
            }

            EventBus.Subscribe<MapEventStartEvent>(HandleStartMapEvent);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<MapEventStartEvent>(HandleStartMapEvent);
        }

        private void HandleStartMapEvent(MapEventStartEvent evt)
        {
            AddUI(evt.MapEvent, evt.Duration);
        }

        public void AddUI(MapEvent evt, float remainTime = 0f)
        {
            MapEventUI ui = _uiStack.Count > 0 ? _uiStack.Pop() : CreateUI();
            ui.OnInActive += HandleInActiveUI;
            ui.EnableFor(evt, remainTime);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)root);
        }

        private void HandleInActiveUI(MapEventUI ui)
        {
            ui.OnInActive -= HandleInActiveUI;
            _uiStack.Push(ui);
        }

        private MapEventUI CreateUI()
        {
            MapEventUI ui = Instantiate(eventUI, root);
            ui.SetTimeController(_timeController);
            ui.Clear();
            return ui;
        }
    }
}
