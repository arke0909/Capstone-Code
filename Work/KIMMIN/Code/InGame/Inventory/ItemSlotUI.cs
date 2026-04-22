using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using Code.UI.Core;
using DG.Tweening;
using System;
using Code.UI.Core.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Work.Code.UI.ContextMenu;
using Work.Code.UI.Interaction;
using Work.Code.UI.Misc;
using Work.LKW.Code.Items;

namespace InGame.InventorySystem
{
    public class ItemSlotUI : DraggableUI, IUIElement<ItemSlot>, IHoverable, IDroppable
    {
        [SerializeField] private ContextMenuSO inventoryMenu;
        [SerializeField] private CraftableItemListSO craftableItemListSO;
        [SerializeField] private DynamicText stackText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image background;
        [SerializeField] private Image colorBackground;
        [SerializeField] private Image skillBackground;
        [SerializeField] private Image outline;
        [SerializeField] private Image skillIcon;
        [SerializeField] private Button button;

        private Color _originColor;
        private Color _defaultColor = new Color32(0, 0, 0, 0);

        public ItemSlot ItemSlot { get; private set; }
        public override Sprite DragSprite => ItemSlot.Item?.ItemData.itemImage;

        public event Action<ItemSlotUI, GameObject> OnDropEvent;
        public event Action<ItemSlot> OnClickEvent;

        private void SetHoveringItem(ItemSlotUI itemSlot)
            => EventBus<HoveringSlotEvent>.Raise(new HoveringSlotEvent { ItemSlot = itemSlot });

        public override bool CanDrag()
        {
            return base.CanDrag() && ItemSlot != null && ItemSlot.Item != null;
        }

        protected override void Awake()
        {
            base.Awake();
            button.onClick.AddListener(HandleClick);
            BindContextMneu(inventoryMenu, () => ItemSlot);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            button.onClick.RemoveListener(HandleClick);
            UnBindContextMneu();
        }

        public void EnableFor(ItemSlot itemSlot)
        {
            if (itemSlot == null || itemIcon == null ||
                stackText == null || nameText == null) return;

            ItemSlot = itemSlot;
            var item = ItemSlot.Item;
            if (item == null) return;

            SetBackground(true);

            string text = itemSlot.Stack > 0 ? itemSlot.Stack.ToString() : string.Empty;
            stackText.SetText(text, itemSlot.Stack == 0);
            nameText.text = item.ItemData.itemName;
            itemIcon.sprite = item.ItemData.itemImage;
            _originColor = background.color = UIDefine.RarityColors[(int)item.ItemData.rarity];
            SetImage(itemIcon, true);

            if (itemSlot.Item is EquipableItem equipItem)
            {
                ShowSkill(equipItem);
            }
            
            var recipes = craftableItemListSO.GetRecipes(item.ItemData);
            if (recipes != null && recipes.Count != 0)
            {
                BindTooltip(() => recipes);
            }
            
            BindTooltip(() => ItemSlot.Item.ItemData);
        }

        public void Clear()
        {
            ItemSlot = null;
            SetBackground(false);
            skillBackground.gameObject.SetActive(false);
            UnbindTooltip();
        }

        public void SetImage(Image targetImage, bool isEnable)
            => targetImage.enabled = isEnable;

        public void SetBackground(bool isEnable)
        {
            if (background == null) return;
            background.gameObject.SetActive(isEnable);
        }

        public void SetBackgroundColor(Color32 color, bool isDefault = false)
        {
            if (ItemSlot == null || ItemSlot.Item == null)
                colorBackground.color = isDefault ? _defaultColor : color;
            else
                background.color = isDefault ? _originColor : color;
        }

        public void SetOutlineColor(Color32 color, bool isDefault = false)
        {
            if (outline == null) return;
            outline.color = isDefault ? Color.white : color;
        }

        public void PlayAnim()
        {
            transform.localScale = Vector3.one * 0.7f;
            transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        }

        private void ShowSkill(EquipableItem equipItem)
        {
            if (equipItem.Skill == null) return;
            skillBackground.gameObject.SetActive(true);
            skillIcon.sprite = equipItem.Skill.skillIcon;
            BindTooltip(() => equipItem.Skill);
        }
        
        private void HandleClick()
        {
            OnClickEvent?.Invoke(ItemSlot);
        }

        protected override void HandleDragEnd(PointerEventData eventData)
        {
            base.HandleDragEnd(eventData);
            if (ItemSlot == null || ItemSlot.Item == null) return;
            EventBus<EndDragEvent>.Raise(new EndDragEvent());
        }
        
        public void OnHoverEnter(PointerEventData eventData)
        {
            SetHoveringItem(this);
        }

        public void OnHoverExit(PointerEventData eventData)
        {
            SetHoveringItem(null);
        }

        protected override void HandleDragStart(PointerEventData eventData)
        {
            base.HandleDragStart(eventData);
            if (ItemSlot == null || ItemSlot.Item == null) return;
            EventBus<StartDragEvent>.Raise(new StartDragEvent(this));

        }

        public void OnDrop(PointerEventData eventData)
        {
            OnDropEvent?.Invoke(this, eventData.pointerDrag);
        }
    }
}