using System;
using System.Collections.Generic;
using Code.UI.Popup;
using UnityEngine;

namespace Code.UI.Controller
{
    public class PopupController : MonoBehaviour
    {
        [SerializeField] private List<BasePopup> popups;
        [SerializeField] private Transform root;

        private Dictionary<Type, BasePopup> _popupMap = new();
        private Dictionary<Type, Stack<BasePopup>> _pool = new();
        private Stack<BasePopup> _popupStack = new();

        private void Awake()
        {
            foreach (var popup in popups)
            {
                if (popup == null) continue;
                _popupMap.TryAdd(popup.DataType, popup);
            }
        }
        
        public void BindPopup(IPopupable popup)
        {
            popup.OnClickHandler += HandleClickPopup;
        }

        public void UnbindPopup(IPopupable popup)
        {
            popup.OnClickHandler -= HandleClickPopup;
        }
        
        private void HandleClickPopup<T>(Func<T> data, ICallbackData callback)
        {
            var type = data.Invoke();
            if (type == null) return;
            ShowPopup(type, callback);
        }
        
        public void ShowPopup<T>(T data, ICallbackData callback = null)
        {
            var type = data.GetType();
            if (!_popupMap.TryGetValue(type, out var prefab))
            {
                Debug.LogWarning($"Popup not found for type: {type}");
                return;
            }

            BasePopup popup;
            if (_pool.TryGetValue(type, out var stack) && stack.Count > 0)
                popup = stack.Pop();
            else
                popup = Instantiate(prefab, root);

            popup.ShowPopup(data, callback);
            _popupStack.Push(popup);
        }
        
        public void CloseTopPopup()
        {
            if (_popupStack.Count == 0) return;

            var popup = _popupStack.Pop();
            var type = popup.DataType;
            popup.ClosePopup();

            if (!_pool.ContainsKey(type))
                _pool[type] = new Stack<BasePopup>();

            _pool[type].Push(popup);
        }
    }
}