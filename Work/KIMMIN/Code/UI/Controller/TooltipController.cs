using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.UI.Core;
using Code.UI.Tooltip;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.Core.Interaction;

namespace Code.UI.Controller
{
    public class TooltipContext
    {
        public Coroutine delayRoutine;
        public List<BaseTooltip> tooltips = new();
    }
    
    [DefaultExecutionOrder(-10)]
    public class TooltipController : MonoBehaviour
    {
        [SerializeField] private List<BaseTooltip> tooltipTypes; 
        [SerializeField] private TooltipMover tooltipMover;
        [SerializeField] private Transform tooltipRoot;
        
        private Dictionary<Type, BaseTooltip> _tooltipMap = new();
        private Dictionary<Type, Queue<BaseTooltip>> _tooltipPools = new();
        private Dictionary<InteractableUI, TooltipContext> _contexts = new();
        
        private bool _rebuildFlag;
        private Coroutine _rebuildCoroutine;
        
        public RectTransform RootRect => tooltipRoot as RectTransform;

        private void Awake()
        {
            MappingTooltip();
            tooltipMover.InitMover(RootRect);
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
            foreach (BaseTooltip tooltip in tooltipTypes)
            {
                if(tooltip == null) continue;
                _tooltipMap.TryAdd(tooltip.DataType, tooltip);
            }
        }
        
        public void BindTooltip<T>(InteractableUI owner, Func<T> data, float delay)
        {
            BindEnterTooltip(owner, data, delay);
            BindExitTooltip(owner);
        }
        
        public void UnbindTooltip(InteractableUI owner)
        {
            if(owner == null || owner.EventHandler == null)
                return;
            
            var handler = owner.EventHandler;
            handler.ClearUIEvent(owner, EUIEvent.PointerEnter);
            handler.ClearUIEvent(owner, EUIEvent.PointerExit);
            
            if (_contexts.TryGetValue(owner, out var context))
            {
                HideTooltip(context);
            }
        }

        private void BindEnterTooltip<T>(InteractableUI owner, Func<T> dataCallback, float delay)
        {
            owner.EventHandler.BindUIEvent(owner, _ => 
            {
                var context = GetContext(owner);
                StopDelayRoutine(context);

                T data = dataCallback.Invoke();
                if (data == null) return;
                
                if (delay > 0)
                    context.delayRoutine = StartCoroutine(ShowTooltipRoutine(context, data, delay));
                else
                    ShowTooltip(context, data);
            }, EUIEvent.PointerEnter);
        }

        private void BindExitTooltip(InteractableUI owner)
        {
            owner.EventHandler.BindUIEvent(owner, _ => 
            {
                if (!_contexts.TryGetValue(owner, out var context))
                    return;
                
                StopDelayRoutine(context);
                HideTooltip(context);
            }, EUIEvent.PointerExit);
        }
        
        private void StopDelayRoutine(TooltipContext context)
        {
            if (context.delayRoutine != null)
            {
                StopCoroutine(context.delayRoutine);
                context.delayRoutine = null;
            }
        }

        private IEnumerator ShowTooltipRoutine<T>(TooltipContext context, T data, float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowTooltip(context, data);
        }
        
        private void ShowTooltip<T>(TooltipContext context, T data)
        {
            Type type = data.GetType();
            if (!_tooltipMap.TryGetValue(type, out BaseTooltip prefab))
                return;

            BaseTooltip tooltip;
            if (_tooltipPools.TryGetValue(type, out var queue) && queue.Count > 0)
                tooltip = queue.Dequeue();
            else
                tooltip = Instantiate(prefab, tooltipRoot);

            tooltip.ShowTooltip(data);
            context.tooltips.Add(tooltip);
            SortTooltips(context);
            
            _rebuildFlag = true;
        }

        private void HideTooltip(TooltipContext context)
        {
            StopDelayRoutine(context);

            foreach (BaseTooltip tooltip in context.tooltips)
            {
                Type type = tooltip.DataType;

                if (!_tooltipPools.ContainsKey(type))
                    _tooltipPools[type] = new Queue<BaseTooltip>();

                tooltip.HidePopup();
                _tooltipPools[type].Enqueue(tooltip);
            }

            context.tooltips.Clear();
        }
        
        public void HideAll()
        {
            foreach (var context in _contexts.Values)
            {
                HideTooltip(context);
            }
        }
        
        private TooltipContext GetContext(InteractableUI owner)
        {
            if (!_contexts.TryGetValue(owner, out var context))
            {
                context = new TooltipContext();
                _contexts[owner] = context;
            }
            return context;
        }
        
        private IEnumerator RebuildLayout()
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(RootRect);
        }
        
        private void SortTooltips(TooltipContext context)
        {
            context.tooltips.Sort((a, b) => b.SortOrder.CompareTo(a.SortOrder));

            for (int i = 0; i < context.tooltips.Count; i++)
            {
                context.tooltips[i].transform.SetSiblingIndex(i);
            }
        }
        
        public bool HasActiveTooltip() 
            => _contexts.Values.Any(c => c.tooltips.Count > 0);
    }
}