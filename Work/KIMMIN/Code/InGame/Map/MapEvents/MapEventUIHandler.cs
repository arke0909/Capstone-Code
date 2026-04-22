using System;
using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.TimeSystem;
using Code.UI.Core;
using DewmoLib.Dependencies;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.GameEvents;

namespace Work.Code.MapEvents
{
    public class MapEventUIHandler : MonoBehaviour
    {
        [SerializeField] private LayoutGroup layout;
        [SerializeField] private MapEventUI eventUI;
        [SerializeField] private Transform root;

        private Stack<MapEventUI> _uiStack = new ();

        [Inject] private TimeController _timeController;
        private const int _initCount = 5;
        private readonly float _xOffset = 100f;
        //private readonly float _yHeight = 100f;
        private readonly float _tweenDuration = 0.1f;

        private void Awake()
        {
            for (int i = 0; i < _initCount; i++)
            {
                var ui = Instantiate(eventUI, root);
                _uiStack.Push(ui);
                ui.SetTimeController(_timeController);
                ui.Clear();
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
            MapEventUI ui = _uiStack.Pop();
            ui.EnableFor(evt, remainTime);
            ui.OnInActive += HandleInActiveUI;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)root);
            PushUI(ui);
        }
        
        private void HandleInActiveUI(MapEventUI ui)
        {
            ui.OnInActive -= HandleInActiveUI;
            _uiStack.Push(ui);
        }

        private void PushUI(MapEventUI ui)
        {
            ui.transform.SetAsLastSibling();
            ui.Canvas.alpha = 0f;
            float targetX = ui.Rect.anchoredPosition.x;
            ui.Rect.anchoredPosition = new Vector2(targetX + _xOffset, ui.Rect.anchoredPosition.y);
            
            Sequence seq = DOTween.Sequence();
            seq.Join(ui.Rect.DOAnchorPosX(targetX, _tweenDuration));
            seq.Join(ui.Canvas.DOFade(1, _tweenDuration));
        }
    }
}