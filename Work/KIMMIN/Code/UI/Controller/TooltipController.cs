using System;
using System.Collections;
using System.Collections.Generic;
using Code.UI.Core;
using Code.UI.Tooltip;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.Core.Interaction;

namespace Code.UI.Controller
{
    class TooltipState
    {
        public Coroutine DelayRoutine;
        public List<BaseTooltip> Tooltips = new();
    }
    
    [DefaultExecutionOrder(-10)]
    public class TooltipController : MonoBehaviour
    {
        [SerializeField] private List<BaseTooltip> tooltipTypes; 
        [SerializeField] private TooltipMover tooltipMover;
        [SerializeField] private Transform tooltipRoot;
        
        private Dictionary<Type, BaseTooltip> _tooltipMap = new();
        private Dictionary<Type, Queue<BaseTooltip>> _pool = new();
        private Dictionary<InteractableUI, TooltipState> _states = new();
        
        private bool _rebuildFlag;
        private Coroutine _rebuildCoroutine;
        
        public RectTransform RootRect => tooltipRoot as RectTransform;

        private void Awake()
        {
            MappingTooltip();
            tooltipMover.Init(RootRect);
        }

        private void LateUpdate()
        {
            if (_rebuildFlag)
            {
                if (_rebuildCoroutine != null)
                {
                    StopCoroutine(_rebuildCoroutine);
                    _rebuildCoroutine = null;
                }
                
                _rebuildCoroutine = StartCoroutine(RebuildLayout());
                _rebuildFlag = false;
            }
        }

        private void MappingTooltip()
        {
            foreach (var tooltip in tooltipTypes)
            {
                if(tooltip == null) continue;
                _tooltipMap.TryAdd(tooltip.DataType, tooltip);
            }
        }
        
        public void BindTooltip<T>(InteractableUI owner, Func<T> data, float delay)
        {
            var handler = owner.EventHandler;
            BindEnterTooltip(owner, data, delay, handler);
            BindExitTooltip(owner, handler);
        }
        
        public void UnbindTooltip(InteractableUI owner)
        {
            if(owner == null || owner.EventHandler == null) return;
            
            var handler = owner.EventHandler;
            handler.ClearUIEvent(owner, EUIEvent.PointerEnter);
            handler.ClearUIEvent(owner, EUIEvent.PointerExit);
            
            if (_states.TryGetValue(owner, out var state))
            {
                HideTooltip(state);
            }
        }

        public void BindEnterTooltip<TData>(InteractableUI owner, Func<TData> dataCallback, float delay, UIEventHandler handler)
        {
            handler.BindUIEvent(owner, _ => {
                var state = GetState(owner);
                StopDelayRoutine(state);

                var data = dataCallback.Invoke();
                if (data == null) return;
                
                if (delay > 0)
                    state.DelayRoutine = StartCoroutine(ShowTooltipRoutine(state, data, delay));
                else
                    ShowTooltip(state, data);
            }, EUIEvent.PointerEnter);
        }

        public void BindExitTooltip(InteractableUI owner, UIEventHandler handler)
        {
            handler.BindUIEvent(owner, _ => {
                if (!_states.TryGetValue(owner, out var state)) return;
                StopDelayRoutine(state);
                HideTooltip(state);
            }, EUIEvent.PointerExit);
        }
        
        private void StopDelayRoutine(TooltipState state)
        {
            if (state.DelayRoutine != null)
            {
                StopCoroutine(state.DelayRoutine);
                state.DelayRoutine = null;
            }
        }

        private IEnumerator ShowTooltipRoutine(TooltipState state, object data, float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowTooltip(state, data);
        }
        
        private void ShowTooltip(TooltipState state, object data)
        {
            var type = data.GetType();
            if (!_tooltipMap.TryGetValue(type, out var prefab)) return;

            BaseTooltip tooltip;
            if (_pool.TryGetValue(type, out var queue) && queue.Count > 0)
                tooltip = queue.Dequeue();
            else
                tooltip = Instantiate(prefab, tooltipRoot);

            _rebuildFlag = true;
            tooltip.ShowTooltip(data);
            state.Tooltips.Add(tooltip);
            SortTooltips(state);
        }

        private void HideTooltip(TooltipState state)
        {
            StopDelayRoutine(state);

            foreach (var tooltip in state.Tooltips)
            {
                var type = tooltip.DataType;

                if (!_pool.ContainsKey(type))
                    _pool[type] = new Queue<BaseTooltip>();

                tooltip.HidePopup();
                _pool[type].Enqueue(tooltip);
            }

            state.Tooltips.Clear();
        }
        
        private TooltipState GetState(InteractableUI owner)
        {
            if (!_states.TryGetValue(owner, out var state))
            {
                state = new TooltipState();
                _states[owner] = state;
            }
            return state;
        }
        
        private IEnumerator RebuildLayout()
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(RootRect);
        }
        
        private void SortTooltips(TooltipState state)
        {
            state.Tooltips.Sort((a, b) => b.SortOrder.CompareTo(a.SortOrder));

            for (int i = 0; i < state.Tooltips.Count; i++)
            {
                state.Tooltips[i].transform.SetSiblingIndex(i);
            }
        }
    }
}