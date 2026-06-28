using System;
using Code.Items.ItemInfo;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI.NPC
{
    public class SubmitItemSelectButton : ItemSelectButton
    {
        [SerializeField] private UIEventHandler eventHandler;
        [SerializeField] private GameObject ownedCountRoot;
        [SerializeField] private TextMeshProUGUI ownedCountText;
        [SerializeField] private GameObject submitCountRoot;
        [SerializeField] private TextMeshProUGUI submitCountText;

        private Action<PointerEventData> _deselectAction;

        public void Init(ItemDataSO itemData, int ownedCount, int submitCount,
            Action<ItemDataSO> onSelect, Action<ItemDataSO> onDeselect)
        {
            InitItem(itemData);
            BindSelect(onSelect);
            BindDeselect(onDeselect);
            SetOwnedCount(ownedCount);
            SetSubmitCount(submitCount);
            SetSelectedState(submitCount > 0);
        }

        public void SetOwnedCount(int ownedCount)
        {
            ownedCountRoot.SetActive(true);
            ownedCountText.text = ownedCount.ToString();
        }

        public void SetSubmitCount(int submitCount)
        {
            bool hasSubmitCount = submitCount > 0;
            submitCountRoot.SetActive(hasSubmitCount);

            if (hasSubmitCount)
                submitCountText.text = submitCount.ToString();
        }

        public override void Dispose()
        {
            UnbindDeselect();
            base.Dispose();
        }

        private void BindDeselect(Action<ItemDataSO> onDeselect)
        {
            UnbindDeselect();

            _deselectAction = _ => onDeselect(ItemData);

            if (eventHandler.EventHandler.TryGetValue(EUIEvent.RightClick, out var boundAction))
                eventHandler.EventHandler[EUIEvent.RightClick] = boundAction + _deselectAction;
            else
                eventHandler.EventHandler[EUIEvent.RightClick] = _deselectAction;
        }

        private void UnbindDeselect()
        {
            if (_deselectAction == null ||
                !eventHandler.EventHandler.TryGetValue(EUIEvent.RightClick, out var rightClickAction))
                return;

            rightClickAction -= _deselectAction;

            if (rightClickAction == null)
                eventHandler.EventHandler.Remove(EUIEvent.RightClick);
            else
                eventHandler.EventHandler[EUIEvent.RightClick] = rightClickAction;

            _deselectAction = null;
        }
    }
}
