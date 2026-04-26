using System;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.Craft
{
    public class MapViewButton : MonoBehaviour
    {
        public event Action OnShowItems;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnShowItems?.Invoke());
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}