using System;
using System.Collections.Generic;
using System.Linq;
using Code.UI.Core;
using Scripts.Players;
using UnityEngine;

namespace Work.Code.UI.ContextMenu
{
    public abstract class BaseContextMenu : UIBase
    {
        protected Player _owner;

        [field: SerializeField] public ContextActionSO[] ContextActions { get; private set; }
        public override EUILayer Layer => EUILayer.ContextMenu;
        public Action OnAction;

        public abstract void ShowMenu(object data);
        public virtual void CloseMenu() => DisableUI(true);
        public void SetOwner(Player player) => _owner = player;
    }
    
    public class BaseContextMenu<T> : BaseContextMenu
    {
        [SerializeField] private Transform root;
        private readonly Dictionary<ContextActionSO, BaseContextAction<T>> _cache = new();
        
        public sealed override void ShowMenu(object data)
        {
            EnableUI(true);
            ShowMenu((T)data);
        }

        protected virtual void ShowMenu(T data)
        {
            Clear();

            foreach (var actionSO in ContextActions)
            {
                var action = GetOrCreateAction(actionSO);
                if (!action.CanShow(data))
                {
                    action.DisableUI();
                    continue;
                }
                
                InitAction(action, data);
            }
        }

        private void InitAction(BaseContextAction<T> action, T dataType)
        {
            action.Init(dataType);
            action.OnCallbackInvoked += HandleActionCalled;
        }

        private void HandleActionCalled()
        {
            OnAction?.Invoke();
            CloseMenu();
        }

        private BaseContextAction<T> GetOrCreateAction(ContextActionSO action)
        {
            if (_cache.TryGetValue(action, out var result)) return result;
            
            var prefab = action.contextAction as BaseContextAction<T>;
            var instance = Instantiate(prefab, root);
            instance.InitOwner(_owner);
            _cache[action] = instance;
            SortActions();

            return instance;
        }

        private void Clear()
        {
            foreach (var action in _cache.Values)
            {
                action.OnCallbackInvoked -= HandleActionCalled;
                action.DisableUI();
            }
        }

        private void SortActions()
        {

            var sorted = _cache.Values.ToList();
            sorted.Sort((a, b) => b.ContextActionSO.sortOrder.CompareTo(a.ContextActionSO.sortOrder));
            
            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].transform.SetSiblingIndex(i);
            }
        }
    }
}