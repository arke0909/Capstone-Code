using System.Collections.Generic;
using System.Linq;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using Code.UI.Core;
using Code.UI.Core.Interaction;
using DewmoLib.Utiles;
using InGame.InventorySystem;
using UnityEngine;
using UnityEngine.Serialization;
using Work.Code.UI.Interaction;

namespace Code.UI.Inventory
{
    public abstract class AbstractSlotUIsPanel : UIBase
    {
        [SerializeField] protected Transform root;

        protected List<ItemSlotUI> _slotUIs;
        
        protected static NotifyValue<ItemSlotUI> _hoveringSlot = new NotifyValue<ItemSlotUI>();
        protected static bool _isDraging;
        
        protected static readonly Color _defaultColor = new Color32(100, 125, 200, 125);
        protected static readonly Color _notAvailableColor = new Color32(255, 100, 100, 125);
        
        protected override void Awake()
        {
            EventBus.Subscribe<HoveringSlotEvent>(HandleHoveringItem);

            _hoveringSlot.OnValueChanged += HandleHoveringSlotChange;
            
            _slotUIs = root.GetComponentsInChildren<ItemSlotUI>().ToList();

            for (int i = 0; i < _slotUIs.Count; i++)
            {
                _slotUIs[i].OnDragStartEvent += HandleOnDragStart;
                _slotUIs[i].OnDragEndEvent += HandleOnDragEnd;
                _slotUIs[i].OnDropEvent += HandleOnDrop;
            }

            foreach (var slot in _slotUIs)
            {
                slot.OnClickEvent += HandleClick;
            }
        }

        protected virtual void HandleClick(ItemSlot item) { }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            EventBus.Unsubscribe<HoveringSlotEvent>(HandleHoveringItem);
            _hoveringSlot.OnValueChanged -= HandleHoveringSlotChange;
            
            foreach (var slot in _slotUIs)
            {
                slot.OnClickEvent -= HandleClick;
            }
        }
        
        private void HandleHoveringSlotChange(ItemSlotUI previousvalue, ItemSlotUI nextvalue)
        {
            if (_isDraging)
            {
                previousvalue?.SetBackgroundColor(Color.white, true);
                nextvalue?.SetBackgroundColor(_defaultColor);
            }
        }

        private void HandleHoveringItem(HoveringSlotEvent evt)
        {
            _hoveringSlot.Value = evt.ItemSlot;
        }
        
        protected virtual void HandleOnDragStart(DraggableUI ui)
        {
            if(ui is not ItemSlotUI slotUI) return;
            _isDraging = true;
        }
        
        protected virtual void HandleOnDragEnd()
        {
            _isDraging = false;
        }
        
        protected virtual void HandleOnDrop(ItemSlotUI aSlotUI, GameObject pointerObject)
        {
            if (aSlotUI == null || !pointerObject.TryGetComponent(out ItemSlotUI bSlotUI)
                                || bSlotUI.ItemSlot.Item == null) return;
            
            bSlotUI.SetBackgroundColor(Color.white, true);
            bSlotUI.PlayAnim();
            if (bSlotUI.ItemSlot != null)
            {
                aSlotUI.SetBackgroundColor(Color.white, true);
                aSlotUI?.PlayAnim();
            }
            
            EventBus.Raise(new SwapItemSlotEvent(bSlotUI.ItemSlot, aSlotUI.ItemSlot));
        }

        protected abstract void UpdateSlotUI();
    }
}